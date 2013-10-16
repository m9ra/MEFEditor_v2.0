using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Drawing.Behaviours
{
    class ZOrdering
    {
        internal static void Attach(FrameworkElement element, ElementGroup group)
        {
            group.Elements.AddFirst(element);
            element.MouseDown += (sender, args) => sendFront(element,group);

            applyZOrdering(group);
        }

        private static void sendFront(FrameworkElement element, ElementGroup group)
        {
            group.Elements.Remove(element);
            group.Elements.AddFirst(element);

            applyZOrdering(group);
        }

        private static void applyZOrdering(ElementGroup group)
        {
            var currentIndex = 0;
            foreach (var element in group.Elements)
            {
                --currentIndex;
                Panel.SetZIndex(element, currentIndex);
            }
        }
    }

    class ElementGroup
    {
        internal LinkedList<FrameworkElement> Elements = new LinkedList<FrameworkElement>();
    }
}
