using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.UnitTesting.AssemblyProviders_TestUtils
{
    /// <summary>
    /// Utility methods for testing editing of source codes.
    /// </summary>
    public static class SourceUtils
    {
        /// <summary>
        /// Writes the specified part of code at given position.
        /// And ensures that results is equal to expectedCode.
        /// </summary>
        /// <param name="code">The code where write will be proceeded.</param>
        /// <param name="position">The position.</param>
        /// <param name="toWrite">Part of code to be written.</param>
        /// <param name="expectedCode">The expected code.</param>
        /// <returns>SourceTest.</returns>
        public static SourceTest Write(this string code, int position, string toWrite, string expectedCode)
        {
            var test = new SourceTest(code);

            return test.Write(position, toWrite, expectedCode);
        }

        /// <summary>
        /// Moves the specified part of code at given position.
        /// And ensures that results is equal to expectedCode.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="p1">Starting position of part to move.</param>
        /// <param name="len">The length of part to move.</param>
        /// <param name="p2">The target position of move.</param>
        /// <param name="expectedCode">The expected code.</param>
        /// <returns>SourceTest.</returns>
        public static SourceTest Move(this string code,int p1, int len, int p2, string expectedCode)
        {
            var test = new SourceTest(code);

            return test.Move(p1, len, p2, expectedCode);
        }
    }
}
