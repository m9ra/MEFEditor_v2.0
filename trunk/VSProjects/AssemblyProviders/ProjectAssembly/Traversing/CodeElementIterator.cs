using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VSLangProj;
using VSLangProj2;
using VSLangProj80;

using EnvDTE;
using EnvDTE80;

using Analyzing;
using TypeSystem;

using AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace AssemblyProviders.ProjectAssembly.Traversing
{
    class CodeElementIterator : SearchIterator
    {
        private readonly VsProjectAssembly _assembly;

        private readonly IEnumerable<CodeElement> _currentNodes;

        private CodeElementIterator(IEnumerable<CodeElement> currentNodes)
        {
            _currentNodes = currentNodes;
        }

        internal CodeElementIterator(VsProjectAssembly assembly)
        {
            _assembly = assembly;

            var currentNodes = new List<CodeElement>();

            foreach (CodeElement element in assembly.CodeModel.CodeElements)
            {
                currentNodes.Add(element);
            }

            _currentNodes = currentNodes;
        }

        public override SearchIterator ExtendName(string suffix)
        {
            var actualNodes = new List<CodeElement>();
            foreach (var currentNode in _currentNodes)
            {
                foreach (CodeElement child in currentNode.Children)
                {
                    //TODO is name in correct form ?
                    if (child.Name == suffix)
                    {
                        actualNodes.Add(child);
                    }
                }
            }

            if (actualNodes.Count == 0)
                return null;

            return new CodeElementIterator(actualNodes);
        }

        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var methodItems = getMethodItems(searchedName);

            foreach (var methodItem in methodItems)
            {
                yield return methodItem.Info;
            }
        }

        private IEnumerable<MethodItem> getMethodItems(string searchedName)
        {
            var methods = new List<MethodItem>();
            var builder = new MethodBuilder();

            foreach (var currentNode in _currentNodes)
            {
                foreach (CodeElement child in currentNode.Children)
                {
                    var method=builder.Build(child);
                    methods.Add(method);
                }
            }

            return methods;
        }
    }
}
