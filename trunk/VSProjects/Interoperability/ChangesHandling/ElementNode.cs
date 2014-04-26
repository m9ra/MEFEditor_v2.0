using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using EnvDTE80;

namespace Interoperability
{

    /// <summary>
    /// used for wraping elements, because of changes handling
    /// </summary>
    public class ElementNode
    {
        /// <summary>
        /// Children of current node
        /// </summary>
        readonly Dictionary<CodeElement, ElementNode> _children = new Dictionary<CodeElement, ElementNode>();

        /// <summary>
        /// Tags stored for current node
        /// </summary>
        readonly Dictionary<string, object>  _tags = new Dictionary<string, object>();

        /// <summary>
        /// CodeElements cached from wrapped element
        /// </summary>
        CodeElements _elements;

        /// <summary>
        /// File item manager which owns this element node
        /// </summary>
        FileItemManager _owner;

        /// <summary>
        /// Element which belongs to this node
        /// </summary>
        public readonly CodeElement Element;
        /// <summary>
        /// Node is root ElementNode
        /// </summary>
        public bool IsRoot { get { return Element == null; } }

        /// <summary>
        /// Absolute offset start of wrapped element
        /// </summary>
        private int Start;
        /// <summary>
        /// Absolute offset end of wrapped element
        /// </summary>
        private int End;
        /// <summary>
        /// Indicates that this span was already removed
        /// </summary>
        public bool Removed;

        internal ElementNode(CodeElement element, FileItemManager owner)
        {
            _owner = owner;
            if (element == null) throw new ArgumentNullException("element");
            Element = element;

            if (!element.NoWatchedChildren()) 
                _elements = element.Children();

            refreshOffset(); //initialize offset
        }

        internal ElementNode(CodeElements codeElements, FileItemManager owner)
        {
            _owner = owner;
            if (codeElements == null) throw new ArgumentNullException("codeElements");
            _elements = codeElements;
        }


        public void SetTag(string tagName, object tag)
        {
            _tags[tagName] = tag;
        }

        public object GetTag(string tagName)
        {
            object result;
            _tags.TryGetValue(tagName, out result);
            return result;
        }

        /// <summary>
        /// refresh Start,End offsets
        /// return true, if offsets differs from stored ones, else return false
        /// </summary>
        /// <returns></returns>
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
                        if (f2.MustImplement) return false; //doesnt have body
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
        /// Check which descendants were added/removed or are misplaced according to ApplyEdits
        /// </summary>
        /// <returns></returns>
        internal List<ElementChange> CheckChildren(List<ElementChange> result = null)
        {
            if (result == null) result = new List<ElementChange>();

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

                if (!el.IsWatched()) continue; //we are watching only few element types

                if (oldElements.Remove(el)) continue; //element is already registered      

                //else element was added
                var nChild = new ElementNode(el, _owner);
                _children.Add(el, nChild);
                result.Add(new ElementChange(nChild, ChangeKind.added));
            }

            //remove children of removed elements
            foreach (var remElement in oldElements)
            {
                var remChild = _children[remElement];
                _children.Remove(remElement);

                result.Add(new ElementChange(remChild, ChangeKind.removed));
                remChild.RemoveChildren(result);
            }

            //recursively check other children
            foreach (var child in _children.Values)
            {
                if (child.refreshOffset()) result.Add(new ElementChange(child, ChangeKind.changed)); //mistake in line change handling
                child.CheckChildren(result);
            }

            return result;
        }

        /// <summary>
        /// Apply position changes and return list of changed descendants
        /// Element is changed, if no its children wrap whole change        
        /// </summary>
        /// <param name="change"></param>
        /// <param name="result"></param>
        /// <returns></returns>
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

            if (needFullMatch) result.Add(new ElementChange(this, ChangeKind.changed));

            return result;
        }


        /// <summary>
        /// Remove all children recursively. These removings are in returned changes
        /// </summary>
        /// <returns></returns>
        internal List<ElementChange> RemoveChildren(List<ElementChange> result = null)
        {
            if (result == null) result = new List<ElementChange>();
            foreach (var child in _children.Values)
            {
                result.Add(new ElementChange(child, ChangeKind.removed));
                child.RemoveChildren(result); //recursively remove
            }
            _children.Clear();

            return result;
        }

        /// <summary>
        /// return true, if change was inside this element.
        /// NOTE: Removing is handled in checkChildren, here are checked only inside changes
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
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
        
        public override string ToString()
        {
            var itm = Element.ProjectItem;
            var textDoc = itm.Document.Object() as TextDocument;
            EditPoint editPoint = textDoc.StartPoint.CreateEditPoint();
            string bufferText = editPoint.GetText(textDoc.EndPoint).Replace("\r", "");
            return "Start: " + Start + " " + bufferText.Substring(Start - 1, End - Start);
        }

        internal List<ElementChange> NamespaceChange(List<ElementChange> result = null)
        {
            if (result == null) result = new List<ElementChange>();
            foreach (var child in _children.Values)
            {
                result.Add(new ElementChange(child, ChangeKind.changed));
                child.NamespaceChange(result);
            }
            return result;
        }
    }
}
