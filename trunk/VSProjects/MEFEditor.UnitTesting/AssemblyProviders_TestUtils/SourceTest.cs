using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RecommendedExtensions.Core.Languages.CSharp;

namespace MEFEditor.UnitTesting.AssemblyProviders_TestUtils
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
            _strips.Move(p1, len, p2);

            Assert.AreEqual(expectedValue, _strips.Data);
            return this;
        }

    }
}
