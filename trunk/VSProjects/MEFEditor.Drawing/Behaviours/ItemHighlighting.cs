using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace MEFEditor.Drawing.Behaviours
{
    class ItemHighlighting
    {
        internal static void Attach(DiagramItem item)
        {
            item.MouseDown += (sender, args) => highlightItem(item);

        }

        private static void highlightItem(DiagramItem item)
        {
            item.IsHighlighted = true;
        }
    }
}