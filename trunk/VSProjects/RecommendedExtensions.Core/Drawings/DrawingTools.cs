using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MEFEditor.Drawing;

namespace RecommendedExtensions.Core.Drawings
{
    /// <summary>
    /// Tools for usual drawing routines
    /// </summary>
    public static class DrawingTools
    {
        /// <summary>
        /// Cache for bitmap images
        /// </summary>
        private static readonly Dictionary<System.Drawing.Bitmap, BitmapSource> _bitmapCache = new Dictionary<System.Drawing.Bitmap, BitmapSource>();

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
        /// Code is extracted from sample usage of <see cref="BezierSpline"/> from 
        /// http://www.codeproject.com/Articles/31859/Draw-a-Smooth-Curve-through-a-Set-of-D-Points-wit
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Path GetSmoothPath(Point[] points)
        {
            // Get Bezier Spline Control Points.
            Point[] cp1, cp2;
            BezierSpline.GetCurveControlPoints(points, out cp1, out cp2);

            // Draw curve by Bezier.
            PathSegmentCollection lines = new PathSegmentCollection();
            for (int i = 0; i < cp1.Length; ++i)
            {
                lines.Add(new BezierSegment(cp1[i], cp2[i], points[i + 1], true));
            }
            PathFigure f = new PathFigure(points[0], lines, false);
            PathGeometry g = new PathGeometry(new PathFigure[] { f });
            Path path = new Path() { Stroke = Brushes.Red, StrokeThickness = 1, Data = g };

            return path;
        }

        /// <summary>
        /// Create StackPanel with bold heading TextBlock and normal TextBlock for text.
        /// </summary>
        /// <param name="heading">Heading text.</param>
        /// <param name="text">Normal text.</param>
        /// <returns>StackPanel with heading and text.</returns>
        internal static StackPanel GetHeadingText(string heading, string text)
        {
            var target = new StackPanel();

            AppendHeadingText(heading, text, target);

            return target;
        }

        internal static void AppendHeadingText(string heading, string text, StackPanel target)
        {
            var hblock = GetText(heading);
            hblock.FontWeight = FontWeights.Bold;

            var tblock = GetText(text);

            target.Children.Add(hblock);
            target.Children.Add(tblock);
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

        /// <summary>
        /// Set image to given target
        /// </summary>
        /// <param name="target">Target for image</param>
        /// <param name="image">Image that will be set</param>
        internal static void SetImage(Image target, CachedImage image)
        {
            target.Source = image.ImageData;
        }

        /// <summary>
        /// Convert bitmap into wpf usable bitmap source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static BitmapSource Convert(System.Drawing.Bitmap source)
        {
            BitmapSource result;
            if (!_bitmapCache.TryGetValue(source, out result))
            {
                result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                _bitmapCache[source] = result;
            }

            return result;
        }
    }
}
