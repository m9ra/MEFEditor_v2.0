using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor
{
    /// <summary>
    /// Example class used for extensibility tutorial in Master Thesis.
    /// </summary>
    public class ProviderTest
    {
        /// <summary>
        /// Method returning name of assembly, where method is implemented
        /// </summary>
        /// <returns>Name of defining assembly</returns>
        public static string GetDefiningAssemblyName()
        {
            return "MEFEditor";
        }
    }
}
