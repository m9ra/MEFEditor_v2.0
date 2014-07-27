using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using MEFEditor.Drawing;

using MEFEditor.UnitTesting.TypeSystem_TestUtils;

namespace MEFEditor.TestConsole.Drawings
{
    public class ContentDrawer
    {
        /// <summary>
        /// Type which content will be provided by content provider
        /// </summary>
        public readonly string DrawedType;

        /// <summary>
        /// Determine that this drawer is used when no matching drawer for drawed type is found
        /// </summary>
        public bool IsDefaultDrawer { get { return DrawedType == null || DrawedType == ""; } }

        public readonly DrawingCreator Provider;

        public ContentDrawer(string drawedType, DrawingCreator provider)
        {
            DrawedType = drawedType;
            Provider = provider;
        }
    }
}
