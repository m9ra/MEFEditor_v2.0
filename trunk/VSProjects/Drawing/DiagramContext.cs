using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing
{
    public class DiagramContext
    {
        private DiagramItem _highlightedItem;

        internal readonly DiagramDefinition Diagram;

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

        public readonly DrawingProvider Provider;

        public IEnumerable<DiagramItem> Items { get { return Provider.Engine.Items; } }



        internal DiagramContext(DrawingProvider provider, DiagramDefinition diagram)
        {
            Provider = provider;
            Diagram = diagram;
        }

        internal void HintPosition(DiagramItem hintContext, DiagramItem hintedItem, Point point)
        {
            Provider.Engine.HintPosition(hintContext, hintedItem, point);
        }

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
