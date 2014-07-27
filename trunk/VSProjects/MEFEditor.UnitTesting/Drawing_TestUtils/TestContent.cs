using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

using MEFEditor.Drawing;

namespace MEFEditor.UnitTesting.Drawing_TestUtils
{
    class TestContent : ContentDrawing
    {
        private readonly DiagramItem _item;

        internal TestContent(DiagramItem item)
            : base(item)
        {
            _item = item;

            var border = new Border();
            border.Width = getIntProperty("width");
            border.Height = getIntProperty("height");

            Child = border;
        }

        private int getIntProperty(string name)
        {
            var value = _item.Definition.GetProperty(name).Value;
            return int.Parse(value);
        }


    }
}
