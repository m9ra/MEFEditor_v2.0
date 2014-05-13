using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using EnvDTE;
using EnvDTE100;
using VSLangProj;
using VSLangProj80;

using Analyzing;
using TypeSystem;
using TypeSystem.Transactions;
using Interoperability;

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
        /// <see cref="CodeClass"/> set that will be discovered when change transaction is closed
        /// </summary>
        private readonly HashSet<CodeClass> _toDiscover = new HashSet<CodeClass>();

        /// <summary>
        /// <see cref="Project"/> represented by current assembly
        /// </summary>
        internal Project Project { get { return _assemblyProject.Project; } }

        /// <summary>
        /// Services that are available for assembly
        /// </summary>
        internal readonly VisualStudioServices VS;

        /// <summary>
        /// Initialize new instance of <see cref="VsProjectAssembly"/> from given <see cref="Project"/>
        /// </summary>
        /// <param name="assemblyProject">Project that will be represented by initialized assembly</param>
        /// <param name="vs"></param>
        public VsProjectAssembly(Project assemblyProject, VisualStudioServices vs)
        {
            VS = vs;
            _assemblyProject = assemblyProject.Object as VSProject;
            _searcher = new CodeElementSearcher(this);

            OnTypeSystemInitialized += initializeAssembly;
        }

        /// <summary>
        /// Root <see cref="CodeElement"/> objects of represented <see cref="Project"/>
        /// </summary>
        public IEnumerable<CodeElement> RootElements
        {
            get
            {
                foreach (var node in VS.GetRootElements(_assemblyProject))
                {
                    yield return node.Element;
                }
                /*foreach (ProjectItem projectItem in Project.ProjectItems)
                {
                    var fileCodeModel = projectItem.FileCodeModel;
                    if (fileCodeModel == null)
                        continue;

                    foreach (CodeElement element in fileCodeModel.CodeElements)
                    {
                        yield return element;
                    }
                }*/
            }
        }

        /// <summary>
        /// Provider of method parsing for assembly
        /// </summary>
        /// <param name="activation">Activation for assembly parser</param>
        /// <param name="emitter">Emitter where parsed instructions are emitted</param>
        internal void ParsingProvider(ParsingActivation activation, EmitterBase emitter)
        {
            //TODO make it language independant
            var w = Stopwatch.StartNew();

            var source = CSharp.Compiler.GenerateInstructions(activation, emitter, TypeServices);

            var methodID = activation.Method == null ? new MethodID("$inline", false) : activation.Method.MethodID;
            VS.Log.Message("Parsing time for {0} {1}ms", methodID, w.ElapsedMilliseconds);
        }

        /// <summary>
        /// Get namespaces that are valid for given <see cref="CodeFunction"/>
        /// <remarks>Note that class where method is defined also belongs to namespace</remarks>
        /// </summary>
        /// <param name="method">Method which namespaces will be returned</param>
        /// <returns>Validat namespaces for given method</returns>
        internal IEnumerable<string> GetNamespaces(CodeElement method)
        {
            var namespaces = VS.GetNamespaces(method.ProjectItem);
            return namespaces;
        }

        /// <summary>
        /// Initialize assembly
        /// </summary>
        private void initializeAssembly()
        {
            hookChangesHandler();
            initializeReferences();
        }

        /// <summary>
        /// Hook handler that will recieve change events in project
        /// </summary>
        private void hookChangesHandler()
        {
            VS.RegisterElementAdd(_assemblyProject, onAdd);
            VS.RegisterElementRemove(_assemblyProject, onRemove);
            VS.RegisterElementChange(_assemblyProject, onChange);
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
        /// Create <see cref="ComponentSearcher"/> with initialized event handlers
        /// </summary>
        /// <returns>Created <see cref="ComponentSearcher"/></returns>
        private ComponentSearcher createComponentSearcher()
        {
            var searcher = new ComponentSearcher(this, TypeServices);
            searcher.OnNamespaceEntered += reportSearchProgress;
            searcher.OnComponentFound += ComponentDiscovered;
            return searcher;
        }

        /// <summary>
        /// Reports search progress to TypeSystem
        /// </summary>
        /// <param name="e">Name of currently processed namespace</param>
        private void reportSearchProgress(CodeNamespace e)
        {
            ReportProgress(e.FullName);
        }

        #region Changes handlers

        /// <summary>
        /// Handler called for element that has been added
        /// </summary>
        /// <param name="node">Affected element node</param>
        private void onAdd(ElementNode node)
        {
            var fullname = MethodBuilder.GetFullName(node.Element);
            node.SetTag("Name", fullname);

            //every component will be reported through add
            //adding member within component is reported as component change
            if (node.Element is CodeClass)
            {
                _toDiscover.Add(node.Element as CodeClass);
                requireDiscovering();
            }
        }

        /// <summary>
        /// Handler called for element that has been changed
        /// </summary>
        /// <param name="node">Affected element node</param>
        private void onChange(ElementNode node)
        {
            var fullname = node.GetTag("Name") as string;
            var fn = node.Element as CodeFunction;
            if (fn == null)
            {
                onRemove(node);
                onAdd(node);
            }
            else
            {
                //code change within method
                ReportInvalidation(fullname);
            }
        }

        /// <summary>
        /// Handler called for element that has been removed
        /// </summary>
        /// <param name="node">Affected element node</param>
        private void onRemove(ElementNode node)
        {
            var tag = node.GetTag("Name") as string;
            if (tag == null)
                return;

            ReportInvalidation(tag);

            ComponentRemoveDiscovered(tag);
        }

        /// <summary>
        /// Require discovering action after current transaction is completed
        /// </summary>
        private void requireDiscovering()
        {
            Transactions.AttachAfterAction(Transactions.CurrentTransaction, new TransactionAction(flushDiscovering, "FlushDiscovering", (t) => t.Name == "FlushDiscovering", this));
        }

        /// <summary>
        /// Flush discovering of elements collected through changes handlers
        /// </summary>
        private void flushDiscovering()
        {
            foreach (var element in _toDiscover)
            {
                VS.Log.Message("Discovering components in {0}", element.FullName);
                var searcher = createComponentSearcher();
                //TODO single level discovering
                searcher.VisitElement(element as CodeElement);
            }
            _toDiscover.Clear();
        }

        #endregion

        #region Assembly provider implementation

        /// <inheritdoc />
        protected override void loadComponents()
        {
            var searcher = createComponentSearcher();

            //search components in whole project
            searcher.VisitProject(Project);
        }

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
            return getMethodItemFromGeneric(methodID, Naming.GetMethodPath(methodID));
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
            //from given nodes find the one with matching id
            foreach (var node in findMethodNodes(methodGenericPath.Signature))
            {
                //create generic specialization 
                var methodItem = buildGenericMethod(node, methodGenericPath);

                if (methodItem.Info.MethodID.MethodString == methodID.MethodString)
                    //we have found matching generic specialization 
                    //omit dynamic resolution flag, because it is transitive-and it dont need to be handled
                    return methodItem;
            }

            //check if param less ctor or cctor is needed
            var needParamLessCtor = Naming.IsParamLessCtor(methodID);
            var needClassCtor = Naming.IsClassCtor(methodID);

            if (needParamLessCtor || needClassCtor)
            {
                //there is no ctor defined and we need paramLessCtor - implicit one should be created
                var declaringClass = _searcher.Search(Naming.GetDeclaringType(methodID)) as CodeClass;
                if (declaringClass != null)
                {
                    if (needParamLessCtor)
                    {
                        return MethodBuilder.BuildImplicitCtor(declaringClass);
                    }
                    else
                    {
                        return MethodBuilder.BuildImplicitClassCtor(declaringClass);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Create <see cref="InheritanceChain"/> enumeration from given typeNodes
        /// </summary>
        private IEnumerable<InheritanceChain> createInheritanceChains(CodeElements typeNodes)
        {
            var chains = new List<InheritanceChain>();
            foreach (CodeElement typeNode in typeNodes)
            {
                var descriptor = MethodBuilder.CreateDescriptor(typeNode);
                var chain = TypeServices.GetChain(descriptor);
                chains.Add(chain);
            }

            return chains;
            //throw new NotImplementedException("TODO is needed to test form of references to other assemblies - because of naming");
        }

        /// <summary>
        /// Find nodes that can be base of methods with given signature
        /// <remarks>Methods can be generated from <see cref="CodeVariable"/>, <see cref="CodeFunction"/>, <see cref="CodeProperty"/></remarks>
        /// </summary>
        /// <param name="methodPathSignature">Path to method in signature form</param>
        /// <returns>Found methods</returns>
        private IEnumerable<CodeElement> findMethodNodes(string methodPathSignature)
        {
            VS.Log.Message("Searching method nodes for {0}", methodPathSignature);

            foreach (var element in _searcher.SearchAll(methodPathSignature))
            {
                switch (element.Kind)
                {
                    case vsCMElement.vsCMElementVariable:
                    case vsCMElement.vsCMElementProperty:
                    case vsCMElement.vsCMElementFunction:
                    case vsCMElement.vsCMElementClass:
                        yield return element;
                        break;
                }
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
        /// Build <see cref="MethodItem"/> from given generic methodNode
        /// </summary>
        /// <param name="methodNode">Node from which <see cref="MethodItem"/> is builded</param>
        /// <param name="methodGenericPath">Arguments for generic method</param>
        /// <returns>Builded <see cref="MethodItem"/></returns>
        private MethodItem buildGenericMethod(CodeElement methodNode, PathInfo methodGenericPath)
        {
            var methodItem = MethodBuilder.Build(methodNode, methodGenericPath.LastPart, this);

            if (methodGenericPath != null && methodGenericPath.HasGenericArguments)
                //make generic specialization
                methodItem = methodItem.Make(methodGenericPath);

            VS.Log.Message("Building method {0}", methodItem.Info.Path.Name);
            return methodItem;
        }

        #endregion

    }
}
