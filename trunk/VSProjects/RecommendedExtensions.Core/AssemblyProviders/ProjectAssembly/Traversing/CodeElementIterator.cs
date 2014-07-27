using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VSLangProj;

using EnvDTE;
using EnvDTE80;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.Interoperability;

using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing
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

        /// <summary>
        /// Here is stored current path of iterator - it is used for generic building
        /// </summary>
        private readonly PathInfo _currentPath;

        private CodeElementIterator(IEnumerable<CodeElement> currentNodes, VsProjectAssembly assembly, PathInfo currentPath)
        {
            _currentNodes = currentNodes;
            _assembly = assembly;
            _currentPath = currentPath;
        }

        internal CodeElementIterator(VsProjectAssembly assembly)
        {
            _assembly = assembly;
            _currentNodes = null;
            _currentPath = null;
        }

        #region Iterator implementation

        /// <inheritdoc />
        public override SearchIterator ExtendName(string suffix)
        {
            if (suffix == "")
                return this;

            var shortSuffix = suffix;
            var genericStart = shortSuffix.IndexOf('<');
            if (genericStart > 0)
                shortSuffix = shortSuffix.Substring(0, genericStart);

            var selectedNodes = new List<CodeElement>();
            foreach (var actualNode in getActualNodes())
            {
                var name = actualNode.Name();
                //TODO is name in correct form for generics?
                if (name == shortSuffix)
                {
                    selectedNodes.Add(actualNode);
                }
            }
            if (selectedNodes.Count == 0)
                return null;

            return new CodeElementIterator(selectedNodes, _assembly, PathInfo.Append(_currentPath, suffix));
        }

        /// <inheritdoc />
        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var methods = new List<TypeMethodInfo>();

            //firstly we test current nodes (because of implicit ctors,..)
            if (_currentNodes != null)
                fillWithMatchingInfo(searchedName, _currentNodes, methods);

            fillWithMatchingInfo(searchedName, getActualNodes(), methods);

            var path = PathInfo.Append(_currentPath, searchedName);

            //resolve genericity
            foreach (var method in methods)
            {
                var resolvedMethod = method;
                if (_currentPath != null && _currentPath.HasGenericArguments)
                    resolvedMethod = method.MakeGenericMethod(path);

                yield return resolvedMethod;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetExpansions()
        {
            if (_currentNodes == null)
                yield break;

            foreach (var node in _currentNodes)
            {
                //expand to base classes/interfaces

                switch (node.Kind)
                {
                    case vsCMElement.vsCMElementInterface:
                    case vsCMElement.vsCMElementClass:
                        var cls = node as CodeType;

                        foreach (CodeElement subType in cls.Bases)
                        {
                            yield return subType.FullName;
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Get nodes that are actualy iterated
        /// </summary>
        /// <returns>Actual nodes</returns>
        private IEnumerable<CodeElement> getActualNodes()
        {
            //THIS IS PERFORMANCE KILLER - IS IT POSSIBLE TO WORKAROUND VISUAL STUDIO THREADING MODEL?
            if (_currentNodes == null)
            {
                //position has not been currently set - use root elements

                foreach (var element in _assembly.RootElements)
                    yield return element;

            }
            else
            {
                //we already have initial position
                foreach (var node in _currentNodes)
                {
                    foreach (CodeElement child in node.Children())
                    {
                        var lang = node.Language;
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        /// Fill given methods list with matchin <see cref="TypeMethodInfo"/> objects from given nodes
        /// </summary>
        /// <param name="searchedName">Name which matching info is searched</param>
        /// <param name="nodes">Nodes where methods are searched</param>
        /// <param name="methods">List which will be filled <see cref="TypeMethodInfo"/></param>
        private void fillWithMatchingInfo(string searchedName, IEnumerable<CodeElement> nodes, List<TypeMethodInfo> methods)
        {
            foreach (var node in nodes)
            {
                var names = _assembly.GetMatchingNames(node, searchedName);
                foreach (var name in names)
                {
                    var info = _assembly.InfoBuilder.Build(node, name);
                    if (info != null)
                        methods.Add(info);
                }
            }
        }

        #endregion
    }
}
