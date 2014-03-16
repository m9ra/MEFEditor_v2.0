using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE100;
using VSLangProj;
using VSLangProj80;

using Analyzing;
using TypeSystem;

using AssemblyProviders.ProjectAssembly.Traversing;
using AssemblyProviders.ProjectAssembly.MethodBuilding;


namespace AssemblyProviders.ProjectAssembly
{
    /// <summary>
    /// Assembly provider implementation for <see cref="Project"/> assemblies loaded from Visual Studio solutions
    /// </summary>
    public class VsProjectAssembly : AssemblyProvider
    {
        /// <summary>
        /// Represented VsProject assembly
        /// </summary>
        private readonly VSProject _assemblyProject;

        /// <summary>
        /// Searcher of <see cref="CodeElement"/> objects
        /// </summary>
        private readonly CodeElementSearcher _searcher;

        /// <summary>
        /// Code model represented by current assembly
        /// </summary>
        internal CodeModel CodeModel { get { return _assemblyProject.Project.CodeModel; } }

        /// <summary>
        /// <see cref="Project"/> represented by current assembly
        /// </summary>
        internal Project Project { get { return _assemblyProject.Project; } }

        /// <summary>
        /// Initialize new instance of <see cref="VsProjectAssembly"/> from given <see cref="Project"/>
        /// </summary>
        /// <param name="assemblyProject">Project that will be represented by initialized assembly</param>
        public VsProjectAssembly(Project assemblyProject)
        {
            _assemblyProject = assemblyProject.Object as VSProject;
            _searcher = new CodeElementSearcher(this);

            OnTypeSystemInitialized += initializeAssembly;
        }

        /// <summary>
        /// Provider of method parsing for assembly
        /// </summary>
        /// <param name="activation">Activation for assembly parser</param>
        /// <param name="emitter">Emitter where parsed instructions are emitted</param>
        internal void ParsingProvider(ParsingActivation activation, EmitterBase emitter)
        {
            //TODO make language independant

            CSharp.Compiler.GenerateInstructions(activation, emitter, TypeServices);
        }

        /// <summary>
        /// Initialize assembly
        /// </summary>
        private void initializeAssembly()
        {
            hookChangesHandler();
            initializeReferences();
            discoverComponents();
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
            addReferences();
            CommitTransaction();
        }

        /// <summary>
        /// Add references to current assembly
        /// </summary>
        private void addReferences()
        {
            foreach (Reference3 reference in _assemblyProject.References)
            {
                var sourceProject = reference.SourceProject;

                if (sourceProject == null)
                {
                    //there is not source project for the reference
                    //we has to add reference according to path
                    AddReference(reference.Path);
                }
                else
                {
                    //we can add reference through referenced source project
                    AddReference(sourceProject);
                }
            }
        }

        /// <summary>
        /// Find components in VsProject
        /// </summary>
        private void discoverComponents()
        {
            StartTransaction("Searching components");

            var searcher = new ComponentSearcher(TypeServices);
            searcher.OnNamespaceEntered += reportSearchProgress;
            searcher.OnComponentFound += AddComponent;

            //search components in whole project
            searcher.VisitProject(Project);

            CommitTransaction();
        }

        /// <summary>
        /// Reports search progress to TypeSystem
        /// </summary>
        /// <param name="e">Name of currently processed namespace</param>
        private void reportSearchProgress(CodeNamespace e)
        {
            ReportProgress(e.FullName);
        }

        #region Assembly provider implementation

        /// <inheritdoc />
        protected override string getAssemblyName()
        {
            return _assemblyProject.Project.Name;
        }

        /// <inheritdoc />
        protected override string getAssemblyFullPath()
        {
            //TODO correct fullpath
            return _assemblyProject.Project.FullName;
        }

        /// <inheritdoc />
        public override GeneratorBase GetMethodGenerator(MethodID methodID)
        {
            var item = getMethodItem(methodID);

            if (item == null)
                return null;

            return item.Generator;
        }

        /// <inheritdoc />
        public override GeneratorBase GetGenericMethodGenerator(MethodID methodID, PathInfo searchPath)
        {
            var item = getMethodItemFromGeneric(methodID, searchPath);
            if (item == null)
                return null;

            return item.Generator;
        }

        /// <inheritdoc />
        public override SearchIterator CreateRootIterator()
        {
            return new CodeElementIterator(this);
        }

        /// <inheritdoc />
        public override MethodID GetImplementation(MethodID methodID, TypeDescriptor dynamicInfo)
        {
            //get method implemented on type described by dynamicInfo
            var implementingMethod = Naming.ChangeDeclaringType(dynamicInfo.TypeName, methodID, false);

            var item = getMethodItem(implementingMethod);
            if (item == null)
                return null;

            return item.Info.MethodID;
        }

        /// <inheritdoc />
        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            //get method implemented on type described by dynamicInfo
            var implementingMethod = Naming.ChangeDeclaringType(implementingTypePath.Name, methodID, false);

            var item = getMethodItemFromGeneric(implementingMethod, methodSearchPath);
            if (item == null)
                return null;

            return item.Info.MethodID;
        }

        /// <inheritdoc />
        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            var typeNode = getTypeNode(typePath.Signature);

            if (typeNode == null)
                //type hasn't been found
                return null;

            var chain = createInheritanceChain(typeNode);

            if (chain.Type.HasParameters)
            {
                //generic specialization is needed
                chain = chain.MakeGeneric(typePath.GenericArgs);
            }

            return chain;
        }

        #endregion


        #region Method building helpers

        /// <summary>
        /// Get <see cref="MethodItem"/> for method described by given methodID
        /// </summary>
        /// <param name="methodID">Description of desired method</param>
        /// <returns><see cref="MethodItem"/> for given methodID specialized by genericPath if found, <c>false</c> otherwise</returns>
        private MethodItem getMethodItem(MethodID methodID)
        {
            //note that for non generic methods is path same as signature
            string methodPathSignature, paramDescription;
            Naming.GetParts(methodID, out methodPathSignature, out paramDescription);

            //from given nodes find the one with matching id
            foreach (var node in findMethodNodes(methodPathSignature))
            {
                var methodItem = buildMethod(node);

                if (methodItem.Info.MethodID.Equals(methodID))
                    //we have found matching method
                    return methodItem;
            }

            return null;
        }

        /// <summary>
        /// Get <see cref="MethodItem"/> for generic method described by given methodID with specialization according to
        /// generic Path
        /// </summary>
        /// <param name="methodID">Description of desired method</param>
        /// <param name="methodGenericPath">Method path specifiing generic substitutions</param>
        /// <returns><see cref="MethodItem"/> for given methodID specialized by genericPath if found, <c>null</c> otherwise</returns>
        private MethodItem getMethodItemFromGeneric(MethodID methodID, PathInfo methodGenericPath)
        {
            //search generic method definitions
            foreach (var node in findMethodNodes(methodGenericPath.Signature))
            {
                //create generic specialization 
                var methodItem = buildGenericMethod(node, methodGenericPath);

                if (methodItem.Info.MethodID == methodID)
                    //we have found matching generic specialization
                    return methodItem;
            }

            return null;
        }

        /// <summary>
        /// Create <see cref="InheritanceChain"/> enumeration from given typeNodes
        /// </summary>
        private IEnumerable<InheritanceChain> createInheritanceChains(CodeElements typeNodes)
        {
            throw new NotImplementedException("TODO is needed to test form of references to other assemblies - because of naming");
        }

        /// <summary>
        /// Find nodes of methods with given signature
        /// </summary>
        /// <param name="methodPathSignature">Path to method in signature form</param>
        /// <returns>Found methods</returns>
        private IEnumerable<CodeFunction> findMethodNodes(string methodPathSignature)
        {
            foreach (var element in _searcher.SearchAll(methodPathSignature))
            {
                var function = element as CodeFunction;
                if (function == null)
                    continue;

                yield return function;
            }
        }

        /// <summary>
        /// Find node of type specified by given typePathSignature
        /// </summary>
        /// <param name="typePathSignature">Path to type in signature form</param>
        /// <returns>Found node if any, <c>null</c> otherwise</returns>
        private CodeClass getTypeNode(string typePathSignature)
        {
            return _searcher.Search(typePathSignature) as CodeClass;
        }

        /// <summary>
        /// Create <see cref="InheritanceChain"/> from given typeNode
        /// </summary>
        /// <param name="typeNode">Type node which inheritance chain will be created</param>
        /// <returns>Created <see cref="InheritanceChain"/></returns>
        private InheritanceChain createInheritanceChain(CodeClass typeNode)
        {
            var subChains = new List<InheritanceChain>();

            var baseChains = createInheritanceChains(typeNode.Bases);
            subChains.AddRange(baseChains);

            var interfaceChains = createInheritanceChains(typeNode.ImplementedInterfaces);
            subChains.AddRange(interfaceChains);

            var typeDescriptor = MethodBuilder.CreateDescriptor(typeNode);
            return TypeServices.CreateChain(typeDescriptor, subChains);
        }

        /// <summary>
        /// Build <see cref="MethodItem"/> from given methodNode
        /// </summary>
        /// <param name="methodNode">Node from which <see cref="MethodItem"/> is builded</param>
        /// <returns>Builded <see cref="MethodItem"/></returns>
        private MethodItem buildMethod(CodeFunction methodNode)
        {
            //building is same as with generic method without parameters
            return buildGenericMethod(methodNode, null);
        }

        /// <summary>
        /// Build <see cref="MethodItem"/> from given generic methodNode
        /// </summary>
        /// <param name="methodNode">Node from which <see cref="MethodItem"/> is builded</param>
        /// <param name="methodGenericPath">Arguments for generic method</param>
        /// <returns>Builded <see cref="MethodItem"/></returns>
        private MethodItem buildGenericMethod(CodeFunction methodNode, PathInfo methodGenericPath)
        {
            var methodItem = MethodBuilder.BuildFrom(methodNode, this);

            if (methodGenericPath != null)
                //make generic specialization
                methodItem = methodItem.Make(methodGenericPath);

            return methodItem;
        }

        #endregion

        public Interoperability.VisualStudioServices VS { get; set; }
    }
}
