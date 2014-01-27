using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE100;
using VSLangProj80;

using Analyzing;
using TypeSystem;

using AssemblyProviders.ProjectAssembly.Traversing;


namespace AssemblyProviders.ProjectAssembly
{
    class VsProjectAssembly : AssemblyProvider
    {
        private readonly VSProject2 _assemblyProject;

        internal CodeModel CodeModel { get { return _assemblyProject.Project.CodeModel; } }

        internal Project Project { get { return _assemblyProject.Project; } }

        public VsProjectAssembly(VSProject2 assemblyProject)
        {
            _assemblyProject = assemblyProject;

            OnTypeSystemInitialized += initializeAssembly;
        }

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
            throw new NotImplementedException();
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
        private void lookupComponents()
        {
            StartTransaction("Searching components");

            var searcher = new ComponentSearcher();
            searcher.OnNamespaceEntered += reportSearchProgress;

            //search components in whole project
            searcher.VisitProject(Project);

            //report added components
            foreach (var component in searcher.Result)
            {
                AddComponent(component);
            }

            CommitTransaction();
        }

        /// <summary>
        /// Reports search progress to TypeSystem
        /// </summary>
        /// <param name="e">Name of currently processed namespace</param>
        private void reportSearchProgress(CodeNamespace e)
        {
            //TODO don't be CPU exhaustive
            ReportProgress(e.FullName);
        }

        #region Assembly provider implementation

        protected override string getAssemblyName()
        {
            return _assemblyProject.Project.Name;
        }

        protected override string getAssemblyFullPath()
        {
            //TODO correct fullpath
            return _assemblyProject.Project.FullName;
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            throw new NotImplementedException();
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            throw new NotImplementedException();
        }

        public override SearchIterator CreateRootIterator()
        {
            return new CodeElementIterator(this);
        }

        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo)
        {
            throw new NotImplementedException();
        }

        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            throw new NotImplementedException();
        }

        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
