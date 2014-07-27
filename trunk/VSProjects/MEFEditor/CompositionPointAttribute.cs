using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor
{
    /// <summary>
    /// Attribute used for marking explicit composition points. Those
    /// composition points will be displayed in edito's composition points menu. 
    /// </summary>    
    [AttributeUsage(AttributeTargets.Method |AttributeTargets.Constructor,AllowMultiple=false)]
    public class CompositionPointAttribute:Attribute
    {
        /// <summary>
        /// Arguments that will be passed into attributed function call.
        /// </summary>
        public readonly IEnumerable<object> Arguments;

        /// <summary>
        /// Specify arguments which will be passed into attributed function call.
        /// </summary>
        /// <param name="arguments">Arguments for entry method.</param>
        public CompositionPointAttribute(params object[] arguments)
        {
            Arguments = arguments;
        }
    }
}
