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
        private readonly IEnumerable<ElementPosition> _currentElements;

        /// <summary>
        /// Here is stored current path of iterator - it is used for generic building
        /// </summary>
        private readonly PathInfo _currentPath;

        private CodeElementIterator(IEnumerable<ElementPosition> currentNodes, VsProjectAssembly assembly, PathInfo currentPath)
        {
            _currentElements = currentNodes;
            _assembly = assembly;
            _currentPath = currentPath;
        }

        internal CodeElementIterator(VsProjectAssembly assembly)
        {
            _assembly = assembly;
            _currentElements = null;
            _currentPath = null;
        }

        #region Iterator implementation


        /// <inheritdoc />
        public override SearchIterator ExtendName(string suffix)
        {
            if (suffix == "")
                return this;

            IEnumerable<ElementPosition> extendedPositions;
            if (_currentElements == null)
            {
                //position has not been currently set - use root elements
                //Root element iteration IS PERFORMANCE KILLER - IS IT POSSIBLE TO WORKAROUND VISUAL STUDIO THREADING MODEL?

                extendedPositions = ElementPosition.ExtendElements(_assembly.RootElements, suffix);
            }
            else
            {
                //we already have initial position
                extendedPositions = ElementPosition.ExtendElements(_currentElements, suffix);
            }
            if (!extendedPositions.Any())
                return null;

            return new CodeElementIterator(extendedPositions, _assembly, PathInfo.Append(_currentPath, suffix));
        }

        /// <inheritdoc />
        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var methods = new List<TypeMethodInfo>();

            //firstly we test current nodes (because of implicit ctors,..)
            IEnumerable<ElementPosition> nextElements;
            if (_currentElements == null)
            {
                //searched name will be tested when filling matching info
                nextElements = ElementPosition.ExtendElements(_assembly.RootElements, null);
            }
            else
            {
                fillWithMatchingInfo(searchedName, _currentElements, methods);
                //searched name will be tested when filling matching info
                nextElements = ElementPosition.ExtendElements(_currentElements, null);
            }

            fillWithMatchingInfo(searchedName, nextElements, methods);

            var path = PathInfo.Append(_currentPath, searchedName);
            //resolve generic specializations
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
            if (_currentElements == null)
                yield break;

            foreach (var nodePosition in _currentElements)
            {
                //expand to base classes/interfaces
                var node = nodePosition.Element;
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
        /// Fill given methods list with matching <see cref="TypeMethodInfo"/> objects from given nodes
        /// </summary>
        /// <param name="searchedName">Name which matching info is searched</param>
        /// <param name="elements">Elements where methods are searched</param>
        /// <param name="methods">List which will be filled <see cref="TypeMethodInfo"/></param>
        private void fillWithMatchingInfo(string searchedName, IEnumerable<ElementPosition> elements, List<TypeMethodInfo> methods)
        {
            foreach (var element in elements)
            {
                if (!element.IsEnd)
                    continue;

                var names = _assembly.GetMatchingNames(element.Element, searchedName);
                foreach (var name in names)
                {
                    var info = _assembly.InfoBuilder.Build(element.Element, name);
                    if (info != null)
                        methods.Add(info);
                }
            }
        }

        #endregion


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var result = "[" + _assembly.Name + "]";
            if (_currentPath != null)
                result += _currentPath.Name;

            return result;
        }
    }
}
