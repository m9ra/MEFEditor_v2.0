using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTesting.AssemblyProviders_TestUtils
{
    public static class SourceUtils
    {
        public static SourceTest Write(this string code, int position, string toWrite, string expectedCode)
        {
            var test = new SourceTest(code);

            return test.Write(position, toWrite, expectedCode);
        }

        public static SourceTest Move(this string code,int p1, int len, int p2, string expectedValue)
        {
            var test = new SourceTest(code);

            return test.Move(p1, len, p2, expectedValue);
        }
    }
}
