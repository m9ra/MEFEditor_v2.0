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
                //the path is empty
                return null;

            var result = new List<CodeElement>();
            //traverse elements
            IEnumerable<ElementPosition> currentElements = null;
            for (var i = 0; i < pathLength; ++i)
            {
                var isLastPart = i + 1 == pathLength;
                var currentPart = pathParts[i];

                var genericIndex = currentPart.IndexOf('<');
                if (genericIndex > 0)
                    currentPart = currentPart.Substring(0, genericIndex);

                //extend next element
                if (currentElements == null)
                    currentElements = ElementPosition.ExtendElements(_searchedAssembly.RootElements, currentPart);
                else
                    currentElements = ElementPosition.ExtendElements(currentElements, currentPart);

                if (!currentElements.Any())
                    //there are no available elements
                    break;
            }

            foreach (var element in currentElements)
            {
                if (element.IsEnd)
                    result.Add(element.Element);
            }

            return result;
        }
    }
}
