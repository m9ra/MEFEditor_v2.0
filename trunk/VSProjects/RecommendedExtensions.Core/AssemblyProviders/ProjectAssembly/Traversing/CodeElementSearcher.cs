using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

using MEFEditor.TypeSystem;
using MEFEditor.Interoperability;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Searcher for getting <see cref="CodeElement"/> objects according to their path
    /// </summary>
    public class CodeElementSearcher
    {
        /// <summary>
        /// Assembly where <see cref="CodeElement"/> objects are searched
        /// </summary>
        private readonly VsProjectAssembly _searchedAssembly;

        internal CodeElementSearcher(VsProjectAssembly searchedAssembly)
        {
            _searchedAssembly = searchedAssembly;
        }

        /// <summary>
        /// Search first <see cref="CodeElement"/> with given path.
        /// </summary>
        /// <param name="path">Search path</param>
        /// <returns>First <see cref="CodeElement"/> found with sufficient path</returns>
        internal IEnumerable<CodeElement> Search(string path)
        {
            var pathSignature = PathInfo.GetSignature(path);
            var pathParts = pathSignature.Split(Naming.PathDelimiter);
            var pathLength = pathParts.Length;
            if (pathLength == 0)
                return null;

            var result = new List<CodeElement>();
            //travers elements
            var currentElements = _searchedAssembly.RootElements;
            for (var i = 0; i < pathLength; ++i)
            {
                var isLastPart = i + 1 == pathLength;
                var currentPart = pathParts[i];

                var genericIndex = currentPart.IndexOf('<');
                if (genericIndex > 0)
                    currentPart = currentPart.Substring(0, genericIndex);

                IEnumerable<CodeElement> nextElements = new CodeElement[0];
                foreach (CodeElement currentChild in currentElements)
                {
                    var name = currentChild.Name();
                    if (name == currentPart)
                    {
                        //current element satysfiing the path - step to its children
                        if (isLastPart)
                        {
                            result.Add(currentChild);
                        }

                        var children = from CodeElement child in currentChild.Children() select child;
                        nextElements = nextElements.Concat(children);
                    }
                }

                //shift to next children
                currentElements = nextElements;

                if (currentElements == null)
                    //no element matches current part
                    break;
            }

            return result;
        }
    }
}
