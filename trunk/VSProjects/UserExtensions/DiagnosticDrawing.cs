using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;

using System.Windows.Media;
using System.Windows.Controls;

namespace UserExtensions
{
    public class DiagnosticDrawing : ContentDrawing
    {
        public DiagnosticDrawing(DiagramItem item) :
            base(item)
        {
            //vytvoříme panel, ve kterém zobrazíme informace i položce
            var layout = new StackPanel();
            layout.Background = Brushes.Green;
            //vzhled bude určen panelem layout
            Child = layout;

            //vytvoříme nadpis označující typ instance
            var headline = new TextBlock();
            headline.Text = Definition.DrawedType;
            headline.FontSize *= 2;
            layout.Children.Add(headline);

            //zobrazíme veškerá dostupné vlastnosti zobrazované instance
            foreach (var property in Definition.Properties)
            {
                //vytvoříme textovou reprezentaci hodnoty vlastnosti
                var block = new TextBlock();
                block.Text = property.Name + ": " + property.Value;

                //a zobrazíme ji ve schématu kompozice
                layout.Children.Add(block);
            }

            //přidáme canvas, do kterého budeme zobrazovat
            //akceptované instance
            var slotCanvas = new SlotCanvas();
            layout.Children.Add(slotCanvas);

            //k naplnění canvasu potřebujeme definici 
            //patřičného slotu
            var slotDefinition = Definition.Slots.FirstOrDefault();
            Item.FillSlot(slotCanvas, slotDefinition);
        }
    }
}
