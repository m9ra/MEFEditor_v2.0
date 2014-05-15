using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using Drawing;

namespace TypeSystem
{
    public static class UserInteraction
    {
        public static readonly string AcceptName = ".accept";

        public static readonly string ExcludeName = ".exclude";

        /// <summary>
        /// TODO: setter is needed for testing purposes
        /// </summary>
        public static Instance DraggedInstance { get; set; }
    }
}
