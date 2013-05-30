using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.TypeBuilding;
namespace TypeSystem
{
    class ExampleOverride:TypeOverride<ExampleClass>
    {
        void _call_Method1()
        {
            Console.WriteLine("Overriden method1");
        }
    }
}
