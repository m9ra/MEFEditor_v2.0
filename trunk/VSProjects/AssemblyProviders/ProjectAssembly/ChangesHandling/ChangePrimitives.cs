using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;

namespace AssemblyProviders.ProjectAssembly.ChangesHandling
{
    /// <summary>
    /// Describes kind of change.
    /// </summary>
    enum ChangeKind
    {
        /// <summary>
        /// Object has been removed.
        /// </summary>
        removed = -1,
        /// <summary>
        /// Object has been changed.
        /// </summary>
        changed = 0,
        /// <summary>
        /// Object has been added.
        /// </summary>
        added = 1,
        /// <summary>
        /// Body of object has been changed.
        /// </summary>
        bodyChanged
    };


    /// <summary>
    /// Encapsulates informatin needed for line change event resolving.
    /// </summary>
    class LineChange
    {
        public readonly ProjectItem Item;
        /// <summary>
        /// length of document catched when change occures
        /// </summary>
        public readonly int DocumentLength;
        /// <summary>
        /// absolute offset of change start
        /// </summary>
        public readonly int Start;
        /// <summary>
        /// absolute offset of change ending
        /// </summary>
        public readonly int End;

        internal int Shortening;
        public LineChange(Document doc, int docLen, int changeStart, int changeEnd)
        {
            Item = doc.ProjectItem;
            DocumentLength = docLen;
            Start = changeStart;
            End = changeEnd;
        }
    }


    /// <summary>
    /// Represent change on ElementNode. Is hashed according to ElementNode.
    /// Equal is for change with same Node and same Kind, except removed kind, which equals all kinds
    /// </summary>
    class ElementChange
    {
        public readonly ElementNode Node;
        public readonly ChangeKind Kind;

        /// <summary>
        /// Create change wrap, also set Node.Removed - because of invalidating previous changes
        /// </summary>
        /// <param name="node"></param>
        /// <param name="change"></param>
        public ElementChange(ElementNode node, ChangeKind change)
        {
            Node = node;
            Kind = change;
            if (Kind == ChangeKind.removed) Node.Removed = true;
        }

        public override string ToString()
        {
            string str = Kind.ToString();

            if (!Node.Removed) str += " " + Node.Element.FullName;
            return str;
        }
        public override bool Equals(object obj)
        {
            var chg = obj as ElementChange;
            if (chg == null) return false;

            if (chg.Node != Node) return false;

            if (chg.Kind != ChangeKind.removed && Kind != ChangeKind.removed) return chg.Kind == Kind;
            return true; //because of erasing change queue with remove changes equals ChangeKind.removed with all kinds
        }
        public override int GetHashCode()
        {
            return Node.GetHashCode();
        }
    }


}
