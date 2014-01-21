using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using Analyzing;
using MEFEditor;
using TypeSystem;

namespace AssemblyProviders.CIL.Providing
{
    public class CILProvider : AssemblyProvider
    {
        public readonly string Path;

        private readonly HashedMethodContainer _methods = new HashedMethodContainer();

        public CILProvider(string path)
        {
            Path = path;
            OnInitialized += load;
        }

        private void load()
        {
            var pars = new ReaderParameters();
            var resolver = new DefaultAssemblyResolver();

            pars.AssemblyResolver = resolver;

            var cecilAssembly = AssemblyDefinition.ReadAssembly(Path, pars);

            /*foreach (var rf in cecilAssembly.MainModule.AssemblyReferences)
            {
                string path;
                var refAssembly = cecilAssembly.MainModule.AssemblyResolver.Resolve(rf);
                path = refAssembly.MainModule.FullyQualifiedName;
            }*/

            StartTransaction("Loading types from assembly");

            foreach (var type in cecilAssembly.MainModule.Types)
            {
                if (type.IsSpecialName)
                    //TODO: can really skip types with special names ?
                    continue;

                var declaringType = getInfo(type);
                var builder = new ComponentInfoBuilder(declaringType);

                foreach (var method in type.Methods)
                {
                    var item = createItem(declaringType, method);
                    tryReportCompositionPoint(builder, method, item);
                    addItem(item);
                }

                foreach (var field in type.Fields)
                {
                    addAutoProperty(builder, declaringType, field);
                }

                ensureStaticInitializer(type);

                if (!builder.IsEmpty)
                {
                    //report that we have found component
                    AddComponent(builder.BuildInfo());
                }
            }

            CommitTransaction();
        }

        private void addAutoProperty(ComponentInfoBuilder builder, TypeDescriptor declaringType, FieldDefinition field)
        {
            var fieldName = field.Name;
            var isStatic = field.IsStatic;
            var fieldType = getInfo(field.FieldType);

            var getter = new TypeMethodInfo(declaringType,
                "get_" + fieldName, fieldType,
                new ParameterTypeInfo[0], isStatic, TypeDescriptor.NoDescriptors
                );

            //TODO generate field load method
            var getItem = new MethodItem(new GetterGenerator(fieldName), getter);
            addItem(getItem);

            var setter = new TypeMethodInfo(declaringType,
                "set_" + fieldName, TypeDescriptor.Void,
                new ParameterTypeInfo[]{
                    ParameterTypeInfo.Create("value",fieldType)
                    }, isStatic, TypeDescriptor.NoDescriptors
                );

            //TODO generate field set method
            var setItem = new MethodItem(new SetterGenerator(fieldName), setter);
            addItem(setItem);

            //TODO add component info
        }

        private void tryReportCompositionPoint(ComponentInfoBuilder builder, MethodDefinition method, MethodItem item)
        {
            foreach (var attrib in method.CustomAttributes)
            {
                if (attrib.AttributeType.FullName == typeof(CompositionPointAttribute).FullName)
                {
                    builder.AddExplicitCompositionPoint(item.Info.MethodID);
                }
            }
        }

        private TypeDescriptor getInfo(TypeReference type)
        {
            return TypeDescriptor.Create(type.FullName);
        }

        /// <summary>
        /// Ensure that assembly contains static analyzer. Otherwise 
        /// it will create default one
        /// </summary>
        /// <param name="type"></param>
        private void ensureStaticInitializer(TypeDefinition type)
        {
            var info = TypeDescriptor.Create(type.FullName);

            var initializerId = TypeServices.GetStaticInitializer(info);
            var implementation = _methods.GetImplementation(initializerId, info);


            if (implementation == null)
            {
                //add default implementation
                var methodInfo = new TypeMethodInfo(
                    info, Naming.GetMethodName(initializerId), TypeDescriptor.Void,
                    new ParameterTypeInfo[0], false, TypeDescriptor.NoDescriptors, false
                    );
                var item = new MethodItem(new CILGenerator(null, methodInfo, TypeServices), methodInfo);
                addItem(item);
            }
        }

        /// <summary>
        /// Creates method item from given method definition.
        /// Static constructors checking is proceeded
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private MethodItem createItem(InstanceInfo declaringType, MethodDefinition method)
        {
            var methodInfo = CILMethod.CreateInfo(TypeServices, declaringType, method);

            if (methodInfo.HasGenericParameters)
                //TODO resolve generics
                return null;

            var item = new MethodItem(new CILGenerator(method, methodInfo, TypeServices), methodInfo);

            return item;
        }

        private void addItem(MethodItem item)
        {
            //TODO resolve implemented types
            if (item == null)
                return;

            _methods.AddItem(item, new InstanceInfo[0]);
        }


        #region Assembly provider API implementation

        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_methods);
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            return _methods.AccordingId(method);
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            return _methods.AccordingGenericId(method, searchPath);
        }

        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo)
        {
            return _methods.GetImplementation(method, dynamicInfo);
        }

        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            return _methods.GetGenericImplementation(methodID, methodSearchPath, implementingTypePath);
        }

        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
