using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor
{
    /// <summary>
    /// Example class used for extensibility tutorial in Master Thesis.
    /// Notice that native implementation of class is empty. It could be used
    /// only for MEFEditor purposes, when neccessary extensions are loaded.
    /// </summary>
    public class Diagnostic
    {
        /// <summary>
        /// Empty native property
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Empty native method
        /// </summary>
        public void Start()
        {
            //we do nothing in native implementation
            //for this class
        }

        /// <summary>
        /// Empty native method
        /// </summary>
        public void Stop()
        {
            //we do nothing in native implementation
            //for this class
        }

        /// <summary>
        /// Empty native method
        /// </summary>
        /// <param name="objs">parameter with variable count of arguments</param>
        /// <returns>Default Integer value for native implementation</returns>
        public int Accept(params object[] objs)
        {
            //we do nothing in native implementation
            //for this class

            return default(int);
        }
    }
}
