using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using EnvDTE80;

namespace MEFEditor.Interoperability
{

    /// <summary>
    /// Wrapper class for <see cref="CodeElement"/> that doesn't suffer for unexpected changes
    /// and exceptions. It can also tracks precise changes based on watching text changes.
    /// </summary>
    public class ElementNode
    {
        /// <summary>
        /// Children of current node.
        /// </summary>
        readonly Dictionary<CodeElement, ElementNode> _children = new Dictionary<CodeElement, ElementNode>();

        /// <summary>
        /// Tags stored for current node.
        /// </summary>
        readonly Dictionary<string, object> _tags = new Dictionary<string, object>();

        /// <summary>
        /// CodeElements cached from wrapped element.
        /// </summary>
        CodeElements _elements;

        /// <summary>
        /// File item manager which owns this element node.
        /// </summary>
        FileItemManager _owner;

        /// <summary>
        /// Element which belongs to this node.
        /// </summary>
        public readonly CodeElement Element;

        /// <summary>
        /// Determine that current node is root.
        /// </summary>
        /// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
        public bool IsRoot { get { return Element == null; } }

        /// <summary>
        /// Children of current node.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<ElementNode> Children { get { return _children.Values; } }

        /// <summary>
        /// Absolute offset start of wrapped element.
        /// </summary>
        private int Start;

        /// <summary>
        /// Absolute offset end of wrapped element.
        /// </summary>
        private int End;

        /// <summary>
        /// Indicates that this span was already removed.
        /// </summary>
        public bool Removed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementNode"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="owner">The owner.</param>
        /// <exception cref="System.ArgumentNullException">element</exception>
        internal ElementNode(CodeElement element, FileItemManager owner)
        {
            _owner = owner;
            if (element == null) throw new ArgumentNullException("element");
            Element = element;

            if (!element.NoWatchedChildren())
                _elements = element.Children();

            refreshOffset(); //initialize offset
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementNode"/> class.
        /// </summary>
        /// <param name="codeElements">The code elements.</param>
        /// <param name="owner">The owner.</param>
        /// <exception cref="System.ArgumentNullException">codeElements</exception>
        internal ElementNode(CodeElements codeElements, FileItemManager owner)
        {
            _owner = owner;
            if (codeElements == null) throw new ArgumentNullException("codeElements");
            _elements = codeElements;
        }


        /// <summary>
        /// Sets the tag for current node.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tag">The tag.</param>
        public void SetTag(string tagName, object tag)
        {
            _tags[tagName] = tag;
        }

        /// <summary>
        /// Gets the tag for current node..
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>System.Object.</returns>
        public object GetTag(string tagName)
        {
            object result;
            _tags.TryGetValue(tagName, out result);
            return result;
        }

        /// <summary>
        /// Refresh Start,End offsets
        /// return true, if offsets differs from stored ones, else return false.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool refreshOffset()
        {
            vsCMPart partS, partE;
            int nStart, nEnd;

            try
            {
                switch (Element.Kind)
                {
                    case vsCMElement.vsCMElementFunction:
                        partS = vsCMPart.vsCMPartBody;
                        partE = vsCMPart.vsCMPartWholeWithAttributes;//to get whole end

                        var f2 = Element as CodeFunction2;
                        if (f2.MustImplement) return false; //doesn't have body
                        break;

                    default:
                        partE = partS = vsCMPart.vsCMPartWholeWithAttributes;
                        break;
                }

                nStart = Element.GetStartPoint(partS).AbsoluteCharOffset;
                nEnd = Element.GetEndPoint(partE).AbsoluteCharOffset;
            }
            catch
            {
                nEnd = nStart = Start + 1; //because of returning true, until position will be available
                _owner.VS.Log.Warning("Cannot resolve position of {0}", Element.FullName);
            }

            if (nStart != Start || nEnd != End)
            {
                Start = nStart;
                End = nEnd;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Loads the direct children only.
        /// It is used for fast initial loading where we don't require
        /// changes handling.
        /// </summary>
        /// <returns>Collected namespaces.</returns>
        internal HashSet<string> LoadDirectChildrenOnly()
        {
            var collectedNamespaces = new HashSet<string>();
            if (_elements == null)
                //nothing to load
                return collectedNamespaces;

            _children.Clear();
            foreach (CodeElement element in _elements)
            {
                if (element.Kind == vsCMElement.vsCMElementImportStmt)
                {
                    var import = element as CodeImport;
                    collectedNamespaces.Add(import.Namespace);
                    continue;
                }

                if (!element.IsWatched())
                    //we are watching only few element types
                    continue;

                var child = new ElementNode(element, _owner);
                _children[element] = child;
            }

            return collectedNamespaces;
        }

        /// <summary>
        /// Check which descendants were added/removed or are misplaced according to ApplyEdits.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>List&lt;ElementChange&gt;.</returns>
        internal List<ElementChange> CheckChildren(List<ElementChange> result = null)
        {
            if (result == null)
                result = new List<ElementChange>();

            if (_elements == null)
                return result; //no watched children

            var oldElements = new HashSet<CodeElement>(_children.Keys);
            //Find added elements
            foreach (CodeElement el in _elements)
            {
                if (el.Kind == vsCMElement.vsCMElementImportStmt)
                {
                    _owner.ReportNamespace(el as CodeImport);
                    continue;
                }

                if (!el.IsWatched())
                    //we are watching only few element types
                    continue;

                if (oldElements.Remove(el))
                    //element is already registered      
                    continue;

                //else element was added
                var nChild = new ElementNode(el, _owner);
                _children[el] = nChild;
                result.Add(new ElementChange(nChild, ChangeKind.Added));
            }

            var hasRemovedChild = false;
            //remove children of removed elements
            foreach (var remElement in oldElements)
            {
                var remChild = _children[remElement];
                _children.Remove(remElement);

                result.Add(new ElementChange(remChild, ChangeKind.Removed));
                remChild.RemoveChildren(result);
                hasRemovedChild = true;
            }

            if (hasRemovedChild)
                //removed child mean change of parent
                result.Add(new ElementChange(this, ChangeKind.Changed));

            //recursively check other children
            foreach (var child in _children.Values)
            {
                if (child.refreshOffset()) result.Add(new ElementChange(child, ChangeKind.Changed)); //mistake in line change handling
                child.CheckChildren(result);
            }

            return result;
        }

        /// <summary>
        /// Apply position changes and return list of changed descendants
        /// Element is changed, if no its children wrap whole change.
        /// </summary>
        /// <param name="change">The change.</param>
        /// <param name="result">The result.</param>
        /// <returns>List&lt;ElementChange&gt;.</returns>
        internal List<ElementChange> ApplyEdit(LineChange change, List<ElementChange> result = null)
        {
            if (result == null) result = new List<ElementChange>();

            // if change was inside this element, try to delegate change on some child
            bool needFullMatch = !IsRoot && rearangeOffsets(change);

            foreach (var child in _children.Values)
            {
                child.ApplyEdit(change, result);
                if (needFullMatch)
                {
                    if (child.Start <= change.Start && child.End >= change.End)
                        needFullMatch = false; //this child contains whole change
                }
            }

            if (needFullMatch) result.Add(new ElementChange(this, ChangeKind.Changed));

            return result;
        }


        /// <summary>
        /// Remove all children recursively. These removings are in returned changes.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>List&lt;ElementChange&gt;.</returns>
        internal List<ElementChange> RemoveChildren(List<ElementChange> result = null)
        {
            if (result == null) result = new List<ElementChange>();
            foreach (var child in _children.Values)
            {
                result.Add(new ElementChange(child, ChangeKind.Removed));
                child.RemoveChildren(result); //recursively remove
            }
            _children.Clear();

            return result;
        }

        /// <summary>
        /// Rearange offsets according to registered changed. Return true, if change was inside this element.
        /// NOTE: Removing is handled in checkChildren, here are checked only inside changes.
        /// </summary>
        /// <param name="change">The change.</param>
        /// <returns><c>true</c> if change was inside element, <c>false</c> otherwise.</returns>
        private bool rearangeOffsets(LineChange change)
        {
            //  return shorting(change.Start, change.End, change.Shortening);         
            int sh = change.Shortening;

            int curStart = change.Start; //cur position before/after change - is same
            int curEndAft = change.End;
            int curEndBef = curEndAft - change.Shortening;

            if (End < curStart) return false; //change start after ending

            bool changed = End >= curStart && Start <= curStart;

            if (Start > curStart) Start -= sh;
            if (End > curStart) End -= sh;

            return changed;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var itm = Element.ProjectItem;
            var textDoc = itm.Document.Object() as TextDocument;
            EditPoint editPoint = textDoc.StartPoint.CreateEditPoint();
            string bufferText = editPoint.GetText(textDoc.EndPoint).Replace("\r", "");
            return "Start: " + Start + " " + bufferText.Substring(Start - 1, End - Start);
        }

        /// <summary>
        /// Reports change in namespace.
        /// </summary>
        /// <param name="result">Reported change.</param>
        /// <returns>List&lt;ElementChange&gt;.</returns>
        internal List<ElementChange> NamespaceChange(List<ElementChange> result = null)
        {
            if (result == null) result = new List<ElementChange>();
            foreach (var child in _children.Values)
            {
                result.Add(new ElementChange(child, ChangeKind.Changed));
                child.NamespaceChange(result);
            }
            return result;
        }
    }
}
