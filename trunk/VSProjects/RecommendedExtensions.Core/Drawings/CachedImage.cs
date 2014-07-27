using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media.Imaging;

namespace RecommendedExtensions.Core.Drawings
{
    /// <summary>
    /// Utility class for caching image data
    /// </summary>
    public class CachedImage
    {
        /// <summary>
        /// Converted image data
        /// </summary>
        public readonly BitmapSource ImageData;

        /// <summary>
        /// Initialize new instance of cached image
        /// </summary>
        /// <param name="source">Source to be converted</param>
        public CachedImage(System.Drawing.Bitmap source)
        {
            ImageData = DrawingTools.Convert(source);
        }
    }
}
