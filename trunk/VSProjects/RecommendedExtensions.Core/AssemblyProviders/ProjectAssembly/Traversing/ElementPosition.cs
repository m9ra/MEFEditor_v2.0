using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;

using MEFEditor.Interoperability;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Representation of element traversing position.
    /// <remarks>This is required because of compound elements (like namespaces)</remarks>
    /// </summary>
    public class ElementPosition
    {
        /// <summary>
        /// Gets a value indicating whether this position reached its end.
        /// </summary>
        /// <value><c>true</c> if this position reached its end; otherwise, <c>false</c>.</value>
        public bool IsEnd { get { return !MissingNameParts.Any(); } }

        /// <summary>
        /// Parts of code element name that has to be still processed.
        /// </summary>
        public readonly IEnumerable<string> MissingNameParts;

        /// <summary>
        /// Traversed element.
        /// </summary>
        public readonly CodeElement Element;

        /// <summary>
        /// Position of traversed element.
        /// </summary>
        /// <param name="element">Traversed element.</param>
        /// <param name="missingName">Missing parts of name traversing.v</param>
        public ElementPosition(CodeElement element, IEnumerable<string> missingName)
        {
            Element = element;
            MissingNameParts = missingName;
        }

        /// <summary>
        /// Extends elements by given part constraint (if constraint is <c>null</c> all elements are returned).
        /// </summary>
        /// <param name="elements">Elements to be extended.</param>
        /// <param name="part">Extending part constraint.</param>
        /// <returns>Extended element positions.</returns>
        public static IEnumerable<ElementPosition> ExtendElements(CodeElements elements, string part)
        {
            return ExtendElements(elements.Cast<CodeElement>(), part);
        }

        /// <summary>
        /// Extends elements by given part constraint (if constraint is <c>null</c> all elements are returned).
        /// </summary>
        /// <param name="elements">Elements to be extended.</param>
        /// <param name="part">Extending part constraint.</param>
        /// <returns>Extended element positions.</returns>
        public static IEnumerable<ElementPosition> ExtendElements(IEnumerable<CodeElement> elements, string part)
        {
            var result = new List<ElementPosition>();
            foreach (var element in elements)
            {
                var name = element.Name();
                if (name.Contains('.'))
                {
                    //test compound namespace
                    if (!name.StartsWith(part))
                        //element doesn't match
                        continue;

                    var parts = name.Split('.').Skip(1);
                    result.Add(new ElementPosition(element, parts));
                }
                else if (name == part)
                {
                    //element name matches
                    result.Add(new ElementPosition(element, new string[0]));
                }
            }

            return result;
        }

        /// <summary>
        /// Extends elements by given part constraint (if constraint is <c>null</c> all elements are returned).
        /// </summary>
        /// <param name="elements">Elements to be extended.</param>
        /// <param name="part">Extending part constraint.</param>
        /// <returns>Extended element positions.</returns>
        public static IEnumerable<ElementPosition> ExtendElements(IEnumerable<ElementPosition> elements, string part)
        {
            var resultElements = new List<ElementPosition>();
            foreach (var element in elements)
            {
                if (element.IsEnd)
                {
                    //extend by its children
                    resultElements.AddRange(element.ExtendByChildren(part));
                    continue;
                }

                var extended = element.TryExtend(part);
                if (extended == null)
                    //position doesn't match to given part.
                    continue;

                //it has to be extended more times.
                resultElements.Add(extended);
            }

            return resultElements;
        }

        /// <summary>
        /// Try extend current position with given name.
        /// </summary>
        /// <param name="name">Extending constraint (if <c>null</c> any node succeeded). 
        /// It has to be free of generic arguments.</param>
        /// <returns>Extended position if name matches current position, <c>null</c> otherwise.</returns>
        public ElementPosition TryExtend(string name)
        {
            if (MissingNameParts.FirstOrDefault() == name || name == null)
            {
                return new ElementPosition(Element, MissingNameParts.Skip(1));
            }

            return null;
        }

        /// <summary>
        /// Gets children that satisfies constraint on given part.
        /// </summary>
        /// <param name="part">Extending part that is free of generics. If null, all children are returned.</param>
        /// <returns>Children that satisfies part extension.</returns>
        public IEnumerable<ElementPosition> ExtendByChildren(string part)
        {
            var children = Element.Children();
            return ExtendElements(children, part);
        }


    }
}
