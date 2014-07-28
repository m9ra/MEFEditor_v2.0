using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Context of diagram definition.
    /// </summary>
    public class DiagramContext
    {
        /// <summary>
        /// The item that is highlighted.
        /// </summary>
        private DiagramItem _highlightedItem;

        /// <summary>
        /// The definition of current diagram.
        /// </summary>
        internal readonly DiagramDefinition Diagram;

        /// <summary>
        /// Gets or sets the highlighted item.
        /// </summary>
        /// <value>The highlighted item.</value>
        internal DiagramItem HighlightedItem
        {
            get
            {
                return _highlightedItem;
            }

            set
            {
                if (_highlightedItem == value)
                    return;

                if (_highlightedItem != null)
                {
                    setHighlightStatus(_highlightedItem, false);
                }

                _highlightedItem = value;
                setHighlightStatus(_highlightedItem, true);
            }
        }

        /// <summary>
        /// The provider of diagram drawings.
        /// </summary>
        public readonly DrawingProvider Provider;

        /// <summary>
        /// Gets the items that are displayed on diagram.
        /// </summary>
        /// <value>The displayed items.</value>
        public IEnumerable<DiagramItem> Items { get { return Provider.Engine.Items; } }



        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramContext"/> class.
        /// </summary>
        /// <param name="provider">The drawings provider.</param>
        /// <param name="diagram">The diagram definition.</param>
        internal DiagramContext(DrawingProvider provider, DiagramDefinition diagram)
        {
            Provider = provider;
            Diagram = diagram;
        }

        /// <summary>
        /// Hints the position of given item in context of hintContext.
        /// </summary>
        /// <param name="hintContext">The hint context where position hint is valid.</param>
        /// <param name="hintedItem">The hinted item.</param>
        /// <param name="position">The hinted position.</param>
        internal void HintPosition(DiagramItem hintContext, DiagramItem hintedItem, Point position)
        {
            Provider.Engine.HintPosition(hintContext, hintedItem, position);
        }

        /// <summary>
        /// Gets the root item definitions.
        /// </summary>
        /// <value>The root item definitions.</value>
        internal IEnumerable<DiagramItemDefinition> RootItemDefinitions
        {
            get
            {
                var result = new HashSet<DiagramItemDefinition>(Diagram.ItemDefinitions);
                foreach (var item in Diagram.ItemDefinitions)
                {
                    if (!result.Contains(item))
                        //item has already detected parent
                        //probably due to recursivity
                        continue;

                    foreach (var slot in item.Slots)
                    {
                        foreach (var reference in slot.References)
                        {
                            var referencedItem = Diagram.GetItemDefinition(reference.DefinitionID);
                            if (referencedItem == item)
                                //self referenced item 
                                //could be root item
                                continue;

                            result.Remove(referencedItem);
                        }
                    }                    
                }

                return result;
            }
        }

        /// <summary>
        /// Sets the highlight status.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        private void setHighlightStatus(DiagramItem item, bool status)
        {
            foreach (var join in Provider.Engine.Joins)
            {
                if (join.From.OwningItem == item || join.To.OwningItem == item)
                    join.IsHighlighted = status;
            }
        }
    }
}
