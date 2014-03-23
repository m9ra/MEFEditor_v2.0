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
using Interoperability;

using AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Search iterator implementation for iterating through CodeElements
    /// </summary>
    class CodeElementIterator : SearchIterator
    {
        /// <summary>
        /// Owning assembly
        /// </summary>
        private readonly VsProjectAssembly _assembly;

        /// <summary>
        /// Nodes, which are initial positions of current iterator
        /// <remarks>If no position is specified - contains null</remarks>
        /// </summary>
        private readonly IEnumerable<CodeElement> _currentNodes;

        private CodeElementIterator(IEnumerable<CodeElement> currentNodes, VsProjectAssembly assembly)
        {
            _currentNodes = currentNodes;
            _assembly = assembly;
        }

        internal CodeElementIterator(VsProjectAssembly assembly)
        {
            _assembly = assembly;
            _currentNodes = null;
        }

        /// <inheritdoc />
        public override SearchIterator ExtendName(string suffix)
        {
            var selectedNodes = new List<CodeElement>();
            foreach (var actualNode in getActualNodes())
            {
                //TODO is name in correct form for generics?
                if (actualNode.Name == suffix)
                {
                    selectedNodes.Add(actualNode);
                }
            }

            if (selectedNodes.Count == 0)
                return null;

            return new CodeElementIterator(selectedNodes, _assembly);
        }

        /// <inheritdoc />
        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var methodItems = getMethodItems(searchedName);

            foreach (var methodItem in methodItems)
            {
                yield return methodItem.Info;
            }
        }

        private IEnumerable<CodeElement> getActualNodes()
        {
            if (_currentNodes == null)
            {
                //position has not been currently set - use root nodes
                foreach (CodeElement node in _assembly.CodeModel.CodeElements)
                {
                    yield return node;
                }
            }
            else
            {
                //we already have initial position
                foreach (var node in _currentNodes)
                {
                    foreach (CodeElement child in node.Children())
                    {
                        yield return child;
                    }
                }
            }
        }

        private IEnumerable<MethodItem> getMethodItems(string searchedName)
        {
            var methods = new List<MethodItem>();

            foreach (CodeElement child in getActualNodes())
            {
                var methodNode = child as CodeFunction;
                if (methodNode == null)
                    continue;

                if (searchedName == Naming.ClassCtorName || searchedName == Naming.CtorName)
                {
                    var kind = methodNode.FunctionKind;
                    if (kind != vsCMFunction.vsCMFunctionConstructor)
                    {
                        //has to be constructor
                        continue;
                    }
                }
                else
                {
                    //TODO form of generics
                    //name has to match
                    if (methodNode.Name != searchedName)
                        //name doesnt match
                        continue;
                }

                var method = MethodBuilder.Build(child, _assembly);
                if (method.Info.MethodName != searchedName)
                    //not everything could be filtered by CodeFunction testing
                    continue;

                methods.Add(method);
            }

            return methods;
        }
    }
}
