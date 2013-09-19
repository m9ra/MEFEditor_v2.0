using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TypeSystem;

namespace UnitTesting.AssemblyProviders_TestUtils
{
    static class Extensions
    {
        public static void AssertPath(this string pathName, string signature, params string[] genericArgs)
        {
            var path = new PathInfo(pathName);

            Assert.AreEqual(pathName, path.Name, "Path name parsing");
            Assert.AreEqual(signature, path.Signature, "Signature parsing");
            CollectionAssert.AreEquivalent(path.GenericArgs, genericArgs, "Generic arguments parsing");
        }
    }
}
