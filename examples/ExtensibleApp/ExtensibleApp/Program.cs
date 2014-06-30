using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ExtensibleApp
{
    class Program
    {        
        static void Main()
        {
            var app = App.Compose();
            app.Run();

            Console.ReadKey();
        }
    }
}
