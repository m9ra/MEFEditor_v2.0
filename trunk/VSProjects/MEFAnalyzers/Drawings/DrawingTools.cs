using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Drawing;

namespace MEFAnalyzers.Drawings
{
    static class DrawingTools
    {
        /// <summary>
        /// Delay for ToolTips, which are displayed immediately.
        /// </summary>
        public const int ToolTip_Quick = 25;


        /// <summary>
        /// Create TextBlock for given text.
        /// </summary>
        /// <param name="text">Text to be displayed in TextBlock.</param>
        /// <returns>Textblock with specifed text.</returns>
        public static TextBlock GetText(string text)
        {
            var block = new TextBlock();
            block.Text = text;
            return block;
        }

        /// <summary>
        /// Create StackPanel with bold heading TextBlock and normal TextBlock for text.
        /// </summary>
        /// <param name="heading">Heading text.</param>
        /// <param name="text">Normal text.</param>
        /// <returns>StackPanel with heading and text.</returns>
        internal static StackPanel GetHeadingText(string heading, string text)
        {
            var hblock = GetText(heading);
            hblock.FontWeight = FontWeights.Bold;

            var tblock = GetText(text);

            var res = new StackPanel();
            res.Children.Add(hblock);
            res.Children.Add(tblock);
            return res;
        }

        /// <summary>
        /// Set ToolTip from content with specified delay to parent.
        /// </summary>
        /// <param name="parent">Element where ToolTip will be displayed.</param>
        /// <param name="delay">Delay before ToolTip is displayed.</param>      
        /// <param name="content">Content to be displayed in ToolTip.</param>
        internal static void SetToolTip(FrameworkElement parent, FrameworkElement content)
        {
            var tip = new ToolTip();
            tip.Content = content;
            parent.ToolTip = tip;
            ToolTipService.SetInitialShowDelay(parent, ToolTip_Quick);
        }

        /// <summary>
        /// Set ToolTip from content with specified delay to parent.
        /// </summary>
        /// <param name="target">Target which ToolTip will be set.</param>
        internal static void SetToolTip(FrameworkElement target, string toolTipText)
        {
            SetToolTip(target, DrawingTools.GetText(toolTipText));
        }

        internal static void SetImage(Image target, System.Drawing.Bitmap image)
        {
            target.Source = DrawingTools.Convert(image);
        }

        internal static void SetIcon(Image target, System.Drawing.Bitmap image)
        {
            SetImage(target, image);
        }

        /// <summary>
        /// Convert bitmap into wpf usable bitmap source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static BitmapSource Convert(System.Drawing.Bitmap source)
        {
            var result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            return result;
        }
    }
}
