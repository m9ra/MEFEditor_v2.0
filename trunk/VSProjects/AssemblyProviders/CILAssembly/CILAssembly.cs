using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel.Composition;

using Mono.Cecil;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CIL;
using AssemblyProviders.CIL.Providing;

namespace AssemblyProviders.CILAssembly
{
    public class CILAssembly : AssemblyProvider
    {
        public readonly string _fullPath;

        private readonly AssemblyDefinition _assembly;

        public CILAssembly(string assemblyPath)
        {
            _fullPath = Path.GetFullPath(assemblyPath);


            //TODO use correct resolvers
            var pars = new ReaderParameters();
            var resolver = new DefaultAssemblyResolver();
            pars.AssemblyResolver = resolver;

            _assembly = AssemblyDefinition.ReadAssembly(_fullPath, pars);

            OnTypeSystemInitialized += initializeAssembly;
        }

        #region Assembly initialization routines

        /// <summary>
        /// Initialize assembly
        /// </summary>
        private void initializeAssembly()
        {
            hookChangesHandler();
            initializeReferences();
            lookupComponents();
        }

        /// <summary>
        /// Hook handler that will recieve change events in project
        /// </summary>
        private void hookChangesHandler()
        {
            //throw new NotImplementedException();
        }


        /// <summary>
        /// Set references according to project referencies
        /// </summary>
        private void initializeReferences()
        {
            StartTransaction("Collecting references");

            try
            {
                addReferences();
            }
            finally
            {
                CommitTransaction();
            }
        }

        /// <summary>
        /// Add references to current assembly
        /// </summary>
        private void addReferences()
        {
            foreach (var reference in _assembly.MainModule.AssemblyReferences)
            {
                //TODO find path of assembly

                /*
                var refAssembly = _assembly.MainModule.AssemblyResolver.Resolve(reference);
                var fullPath = refAssembly.MainModule.FullyQualifiedName;

                AddReference(fullPath);*/
            }
        }


        #endregion

        #region Components handling

        /// <summary>
        /// Search components defined in _assembly and report them.
        /// </summary>
        private void lookupComponents()
        {
            StartTransaction("Searching components");

            foreach (var type in _assembly.MainModule.GetTypes())
            {
                if (isComponent(type))
                {
                    var info = createComponentInfo(type);
                    AddComponent(info);
                }
            }

            CommitTransaction();
        }


        /// <summary>
        /// Determine that given type defines component
        /// </summary>
        /// <param name="type">Tested type</param>
        /// <returns>True if type defines component, false otherwise</returns>
        private bool isComponent(TypeDefinition type)
        {
            foreach (var attribute in type.CustomAttributes)
            {
                if (isComponentAttribute(attribute))
                {
                    //self export defined
                    return true;
                }
            }

            foreach (var method in type.Methods)
            {
                foreach (var attribute in method.CustomAttributes)
                {
                    //importing constructor or property import/export
                    if (isComponentAttribute(attribute))
                    {
                        return true;
                    }
                }
            }

            foreach (var field in type.Fields)
            {
                //importing/exporting field
                foreach (var attribute in field.CustomAttributes)
                {
                    if (isComponentAttribute(attribute))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Create component info for given type
        /// </summary>
        /// <param name="componentType">type that will create component</param>
        /// <returns>Created component info</returns>
        private ComponentInfo createComponentInfo(TypeDefinition componentType)
        {
            var componentDescriptor = getDescriptor(componentType);

            var infoBuilder = new ComponentInfoBuilder(componentDescriptor);

            foreach (var attribute in componentType.CustomAttributes)
            {
                if (isComponentAttribute(attribute))
                {
                    //self export defined
                    throw new NotImplementedException();
                }
            }

            reportComponentMethods(componentType, infoBuilder);

            foreach (var field in componentType.Fields)
            {
                //importing/exporting field
                foreach (var attribute in field.CustomAttributes)
                {
                    if (isComponentAttribute(attribute))
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            return infoBuilder.BuildInfo();
        }

        /// <summary>
        /// Report methods defined by component type into infoBuilder
        /// </summary>
        /// <param name="componentType">Type definint component</param>
        /// <param name="infoBuilder">Builder where methods are reported</param>
        private void reportComponentMethods(TypeDefinition componentType, ComponentInfoBuilder infoBuilder)
        {
            //TODO add implicit importing constructor if needed

            foreach (var method in componentType.Methods)
            {
                foreach (var attribute in method.CustomAttributes)
                {
                    var fullname = attribute.AttributeType.FullName;

                    //importing constructor  
                    if (fullname == typeof(MEFEditor.CompositionPointAttribute).FullName)
                    {
                        //TODO add composition point arguments
                        var methodId = getMethodId(infoBuilder.ComponentType, method);
                        infoBuilder.AddExplicitCompositionPoint(methodId);
                    }
                    //property import/export
                    else if (isComponentAttribute(attribute))
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        /// <summary>
        /// Determine that given attribute defines component
        /// </summary>
        /// <param name="attribute">Tested attribute</param>
        /// <returns>True if attribute define component, false otherwise</returns>
        private bool isComponentAttribute(CustomAttribute attribute)
        {
            var fullname = attribute.AttributeType.FullName;

            return
                fullname == typeof(ExportAttribute).FullName ||
                fullname == typeof(ImportAttribute).FullName ||
                fullname == typeof(MEFEditor.CompositionPointAttribute).FullName;
        }

        #endregion

        #region Method building

        /// <summary>
        /// Create complete method info for given method definition
        /// </summary>
        /// <param name="declaringType">Type where method is declared</param>
        /// <param name="method">Method which info is retrieved</param>
        /// <returns>Created method info</returns>
        internal TypeMethodInfo CreateMethodInfo(TypeDescriptor declaringType, MethodDefinition method)
        {
            return CILInstruction.CreateMethodInfo(method, method.IsAbstract);
        }

        /// <summary>
        /// Creates method item from given method definition.
        /// Static constructors checking is proceeded
        /// </summary>
        /// <param name="method">Method definition</param>
        /// <returns>Created method item</returns>
        private MethodItem createItem(TypeDescriptor declaringType, MethodDefinition method)
        {
            //TODO cache results

            var methodInfo = CreateMethodInfo(declaringType, method);

            if (methodInfo.HasGenericParameters)
                //TODO resolve generics
                return null;

            var item = new MethodItem(new CILGenerator(method, methodInfo, TypeServices), methodInfo);

            return item;
        }

        /// <summary>
        /// Build getter method providing field value
        /// </summary>
        /// <param name="declaringType">Type where getter is defined</param>
        /// <param name="field">Field which value is provided</param>
        /// <returns>Builded method</returns>
        private MethodItem buildAutoGetter(TypeDescriptor declaringType, FieldDefinition field)
        {
            var fieldName = field.Name;
            var isStatic = field.IsStatic;
            var fieldType = getDescriptor(field.FieldType);

            var getter = new TypeMethodInfo(declaringType,
                "get_" + fieldName, fieldType,
                new ParameterTypeInfo[0], isStatic, TypeDescriptor.NoDescriptors
                );

            var getItem = new MethodItem(new GetterGenerator(fieldName), getter);

            return getItem;
        }

        /// <summary>
        /// Build setter method setting field value
        /// </summary>
        /// <param name="declaringType">Type where setter is defined</param>
        /// <param name="field">Field which value is set</param>
        /// <returns>Builded method</returns>
        private MethodItem buildAutoSetter(TypeDescriptor declaringType, FieldDefinition field)
        {
            var fieldName = field.Name;
            var isStatic = field.IsStatic;
            var fieldType = getDescriptor(field.FieldType);

            var setter = new TypeMethodInfo(declaringType,
                "set_" + fieldName, TypeDescriptor.Void,
                new ParameterTypeInfo[]{
                    ParameterTypeInfo.Create("value",fieldType)
                    }, isStatic, TypeDescriptor.NoDescriptors
                );

            var setItem = new MethodItem(new SetterGenerator(fieldName), setter);

            return setItem;
        }

        /// <summary>
        /// Build constructor methods for given type
        /// </summary>
        /// <param name="type">Type which constructors are builded</param>
        /// <returns>Builded methods</returns>
        private IEnumerable<MethodItem> buildConstructors(TypeDefinition type)
        {
            var info = TypeDescriptor.Create(type.FullName);

            foreach (var method in type.Methods)
            {
                if (method.Name == ".ctor")
                {
                    yield return createItem(info, method);
                }
            }
        }

        /// <summary>
        /// Build method for static initializer for given type
        /// </summary>
        /// <param name="type">Type which initializer is builded</param>
        /// <returns>Builded method</returns>
        private MethodItem buildStaticInitilizer(TypeDefinition type)
        {
            var info = TypeDescriptor.Create(type.FullName);

            foreach (var method in type.Methods)
            {
                if (method.Name == ".cctor")
                    return createItem(info, method);
            }

            //if no explicit cctor is found default one is created

            var initializerId = TypeServices.GetStaticInitializerID(info);

            //add default implementation
            var methodInfo = new TypeMethodInfo(
                info, Naming.GetMethodName(initializerId), TypeDescriptor.Void,
                new ParameterTypeInfo[0], false, TypeDescriptor.NoDescriptors, false
                );

            var item = new MethodItem(new CILGenerator(null, methodInfo, TypeServices), methodInfo);

            return item;
        }

        /// <summary>
        /// Get method ID for given method definition
        /// </summary>
        /// <param name="method">Method which id is retrieved</param>
        /// <param name="declaringType">Type where method is declared</param>
        /// <returns>Method ID of given method definition</returns>
        private MethodID getMethodId(TypeDescriptor declaringType, MethodDefinition method)
        {
            var methodItem = createItem(declaringType, method);

            return methodItem.Info.MethodID;
        }

        #endregion

        #region Method searching

        internal IEnumerable<MethodItem> GetMethods(string typeFullName, string searchedName)
        {
            var foundType = getType(typeFullName);
            if (foundType != null)
            {
                switch (searchedName)
                {
                    case Naming.CtorName:
                        foreach (var ctor in buildConstructors(foundType))
                        {
                            yield return ctor;
                        }
                        break;

                    case Naming.ClassCtorName:
                        yield return buildStaticInitilizer(foundType);
                        break;

                    default:
                        var typeDescriptor = getDescriptor(foundType);

                        foreach (var method in foundType.Methods)
                        {
                            if (method.Name == searchedName)
                                yield return createItem(typeDescriptor, method);
                        }

                        foreach (var field in foundType.Fields)
                        {
                            if ("get_" + field.Name == searchedName)
                                yield return buildAutoGetter(typeDescriptor, field);

                            if ("set_" + field.Name == searchedName)
                                yield return buildAutoSetter(typeDescriptor, field);
                        }
                        break;
                }
            }
        }

        internal MethodItem GetMethod(MethodID methodID)
        {
            //TODO caching

            var type = Naming.GetDeclaringType(methodID);
            var name = Naming.GetMethodName(methodID);

            var methods = GetMethods(type, name);
            if (methods == null)
                return null;

            foreach (var method in methods)
            {
                if (method.Info.MethodID.Equals(methodID))
                    return method;
            }

            return null;
        }

        internal MethodItem GetGenericMethod(MethodID methodID, PathInfo info)
        {
            //TODO caching

            throw new NotImplementedException();
        }

        #endregion

        #region Private utilities

        private TypeDescriptor getDescriptor(TypeReference type)
        {
            return TypeDescriptor.Create(type.FullName);
        }

        private TypeDefinition getType(PathInfo typePath)
        {
            return getType(typePath.Name);
        }

        private TypeDefinition getType(string fullname)
        {
            return _assembly.MainModule.GetType(fullname);
        }

        private TypeDefinition getType(TypeDescriptor descriptor)
        {
            return getType(descriptor.TypeName);
        }

        private InheritanceChain getChain(TypeDefinition type)
        {
            //caching is provided outside assembly

            var subChains = new List<InheritanceChain>();
            foreach (var iface in type.Interfaces)
            {
                var descriptor = getDescriptor(iface);
                var subChain = TypeServices.GetChain(descriptor);

                subChains.Add(subChain);
            }

            var subTypeDescriptor = getDescriptor(type.BaseType);
            var subTypeChain = TypeServices.GetChain(subTypeDescriptor);

            var typeDescriptor = getDescriptor(type);
            return TypeServices.CreateChain(typeDescriptor, subChains);
        }


        #endregion

        #region Assembly provider implementation

        protected override string getAssemblyFullPath()
        {
            return _fullPath;
        }

        protected override string getAssemblyName()
        {
            return _assembly.Name.Name;
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            var methodItem = GetMethod(method);

            if (methodItem == null)
                return null;

            return methodItem.Generator;
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            var methodItem = GetMethod(method);

            if (methodItem == null)
                return null;

            return methodItem.Generator;
        }

        public override SearchIterator CreateRootIterator()
        {
            return new TypeModuleIterator(this);
        }

        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo)
        {
            var searchedName = Naming.GetMethodName(method);
            var possibleId = Naming.ChangeDeclaringType(dynamicInfo.TypeName, method, false);
            foreach (var methodItem in GetMethods(dynamicInfo.TypeName, searchedName))
            {
                if (methodItem.Info.MethodID.Equals(possibleId))
                    return possibleId;
            }

            return null;
        }

        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            throw new NotImplementedException();
        }

        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            var type = getType(typePath);

            if (type == null)
                return null;

            var chain = getChain(type);
            return chain;
        }
        #endregion

    }
}
