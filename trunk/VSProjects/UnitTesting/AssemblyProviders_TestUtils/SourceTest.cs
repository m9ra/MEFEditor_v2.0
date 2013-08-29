using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using AssemblyProviders.CSharp;

namespace UnitTesting.AssemblyProviders_TestUtils
{
    public class SourceTest
    {
        StripManager _strips;
        internal SourceTest(string test)
        {
            _strips = new StripManager(test);
        }
        
        internal SourceTest Write(int position, string data, string expectedValue)
        {
            _strips.Write(position, data);

            Assert.AreEqual(expectedValue, _strips.Data);
            return this;
        }

        internal SourceTest Move(int p1, int len, int p2, string expectedValue)
        {
            _strips.Move(p1, p2, len);

            Assert.AreEqual(expectedValue, _strips.Data);

            return this;
        }

    }
}
