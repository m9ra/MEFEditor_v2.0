using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

using TypeSystem;
using Interoperability;

namespace AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Searcher for getting <see cref="CodeElement"/> objects according to their path
    /// </summary>
    class CodeElementSearcher
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
        internal CodeElement Search(string path)
        {
            var lazyEnumeration = SearchAll(path);

            if (!lazyEnumeration.Any())
                //no element satisfying given path has been found
                return null;

            return lazyEnumeration.First();
        }

        /// <summary>
        /// Search all <see cref="CodeElement"/> with given path.
        /// </summary>
        /// <param name="path">Search path</param>
        /// <returns>First <see cref="CodeElement"/> found with sufficient path</returns>
        internal IEnumerable<CodeElement> SearchAll(string path)
        {
            var pathInfo = new PathInfo(path);
            var pathParts = pathInfo.Signature.Split(Naming.PathDelimiter);
            if (pathParts.Length == 0)
                throw new NotSupportedException("Cannot find CodeElement with given path '" + path + "'");

            //search for all children with given path on parentElement
            var parentElements = findElements(pathParts, pathParts.Length - 1);
            
            var lastPart = pathParts[pathParts.Length - 1];

            //TODO: refactor out of VsProjectAssembly
            if (
                lastPart == CSharp.LanguageDefinitions.CSharpSyntax.MemberInitializer ||
                lastPart == CSharp.LanguageDefinitions.CSharpSyntax.MemberStaticInitializer
                )
            {
                foreach (var element in parentElements)
                    yield return element;

                yield break;
            }

            foreach (CodeElement element in parentElements)
            {
                var name = element.Name();

                //match element with last part
                if (IsCorrespondingElement(element, pathParts, pathParts.Length - 1))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Determine that given element corresponds with specified part of given path considering 
        /// getter, setter, ctor and generic rules
        /// </summary>
        /// <param name="element"></param>
        /// <param name="part"></param>
        /// <param name="isLastPart"></param>
        /// <returns></returns>
        internal static bool IsCorrespondingElement(CodeElement element, string[] pathParts, int partIndex)
        {
            //TODO maybe this method should be optimized - create matching object that can be reused

            var expectedName = pathParts[partIndex];
            var isLastPart = pathParts.Length == partIndex + 1;

            //Last part has different matching rules
            if (isLastPart)
            {
                if (pathParts.Length > 1 && (expectedName == Naming.CtorName || expectedName == Naming.ClassCtorName))
                {
                    //naming convention for ctors force to use class name
                    expectedName = pathParts[pathParts.Length - 2];

                    //ctors cannot be generic by itself - just their defining classes are generic
                    //genericty of defining classes is tested at previous element in path
                    var ctorGenericIndex = expectedName.IndexOf('<');
                    if (ctorGenericIndex > 0)
                        expectedName = expectedName.Substring(0, ctorGenericIndex);
                }
                else if (expectedName == Naming.IndexerGetter || expectedName == Naming.IndexerSetter)
                {
                    expectedName = "this";
                }
                else if (expectedName.StartsWith(Naming.GetterPrefix))
                {
                    expectedName = expectedName.Substring(Naming.GetterPrefix.Length);
                }
                else if (expectedName.StartsWith(Naming.SetterPrefix))
                {
                    expectedName = expectedName.Substring(Naming.SetterPrefix.Length);
                }

            }
            else
            {
                throw new NotImplementedException();
            }

            //resolve generic parts matching
            var genericIndex = expectedName.IndexOf('<');
            var hasToBeGeneric = genericIndex > 0;

            //every generic element ends with closing brace
            var isGeneric = element.FullName.EndsWith(">");

            if (isGeneric != hasToBeGeneric)
                //genericity doesnt match
                return false;

            if (hasToBeGeneric)
            {
                expectedName = expectedName.Substring(0, genericIndex);
            }

            return expectedName == element.Name();
        }

        /// <summary>
        /// Find <see cref="CodeElement"/> specified by first pathLength pathParts
        /// </summary>
        /// <param name="pathParts">Parth parts specifiing name of <see cref="CodeElement"/></param>
        /// <param name="pathLength">Length of prefix in pathParts used for <see cref="CodeElement"/> searching</param>
        /// <returns>Found <see cref="CodeElement"/> if any, <c>null</c> otherwise</returns>
        private CodeElement findElement(string[] pathParts, int pathLength)
        {
            if (pathLength == 0)
                return null;

            var elements = findElements(pathParts, pathLength - 1);

            var lastPart = pathParts[pathLength - 1];
            if (pathLength > 1 && (lastPart == Naming.ClassCtorName || lastPart == Naming.CtorName))
            {
                //change to constructor name
                lastPart = pathParts[pathLength - 2];
            }


            //find desired element
            foreach (var element in elements)
            {
                var elementName=element.Name;
                if (lastPart == elementName)
                {
                    return element;
                }
            }

            //element hasn't been found
            return null;
        }

        /// <summary>
        /// Find enumeration of <see cref="CodeElement"/> specified by first pathLength pathParts
        /// </summary>
        /// <param name="pathParts">Parth parts specifiing name of <see cref="CodeElement"/></param>
        /// <param name="pathLength">Length of prefix in pathParts used for <see cref="CodeElement"/> searching</param>
        /// <returns>Found <see cref="CodeElement"/> if any, <c>null</c> otherwise</returns>
        private IEnumerable<CodeElement> findElements(string[] pathParts, int pathLength)
        {
            var currentElements = _searchedAssembly.RootElements;

            if (pathLength == 0)
                return currentElements;

            //traverse all allowed part
            for (var i = 0; i < pathLength; ++i)
            {
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


            if (currentElements == null)
                return new CodeElement[0];

            return currentElements;
        }
    }
}
