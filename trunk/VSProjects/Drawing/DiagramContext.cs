using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class DiagramContext
    {
        internal readonly DrawingProvider Provider;

        internal readonly DiagramDefinition Diagram;

        internal DiagramContext(DrawingProvider provider, DiagramDefinition diagram)
        {
            Provider = provider;
            Diagram = diagram;
        }

        internal IEnumerable<DiagramItemDefinition> RootItemDefinitions
        {
            get
            {
                var result = new HashSet<DiagramItemDefinition>(Diagram.ItemDefinitions);

                foreach (var item in Diagram.ItemDefinitions)
                {
                    foreach (var slot in item.Slots)
                    {
                        foreach (var reference in slot.References)
                        {
                            var referencedItem = Diagram.GetItemDefinition(reference.DefinitionID);
                            result.Remove(referencedItem);
                        }
                    }
                }

                return result;
            }
        }
    }
}
