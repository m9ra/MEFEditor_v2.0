using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;

using ExtensibleApp.Interfaces;

namespace ExtensibleApp
{
    [Export("Logger",typeof(ILogger))]
    class ConsoleLogger : ILogger
    {
        public void Write(string message)
        {
            Console.WriteLine("LOG: " + message);
        }
    }
}
