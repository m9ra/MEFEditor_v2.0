using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtensionPoints;

using System.ComponentModel.Composition;

namespace Main
{
    [ExportMetadata("Output", "Console")]
    [Export(typeof(ILogger))]
    class ConsoleLogger:ILogger 
    {  
        public void Log(string message)
        { 
            Console.WriteLine(message);
        }
    }
}
