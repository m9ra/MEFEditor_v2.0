using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RecommendedExtensions.Core.Languages.CSharp;

namespace MEFEditor.UnitTesting.AssemblyProviders_TestUtils
{
    /// <summary>
    /// Test class for source changes handling.
    /// </summary>
    public class SourceTest
    {
        /// <summary>
        /// The tested strips manager.
        /// </summary>
        StripManager _strips;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceTest"/> class.
        /// </summary>
        /// <param name="test">The test.</param>
        internal SourceTest(string test)
        {
            _strips = new StripManager(test);
        }

        /// <summary>
        /// Writes the specified part of code at given position.
        /// And ensures that results is equal to expectedCode.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="toWrite">Part of code to be written.</param>
        /// <param name="expectedCode">The expected code.</param>
        /// <returns>SourceTest.</returns>
        internal SourceTest Write(int position, string toWrite, string expectedCode)
        {
            _strips.Write(position, toWrite);

            Assert.AreEqual(expectedCode, _strips.Data);
            return this;
        }

        /// <summary>
        /// Moves the specified part of code at given position.
        /// And ensures that results is equal to expectedCode.
        /// </summary>
        /// <param name="p1">Starting position of part to move.</param>
        /// <param name="len">The length of part to move.</param>
        /// <param name="p2">The target position of move.</param>
        /// <param name="expectedCode">The expected code.</param>
        /// <returns>SourceTest.</returns>
        internal SourceTest Move(int p1, int len, int p2, string expectedCode)
        {
            _strips.Move(p1, len, p2);

            Assert.AreEqual(expectedCode, _strips.Data);
            return this;
        }
    }
}
