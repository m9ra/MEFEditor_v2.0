using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TypeSystem;

using AssemblyProviders.CSharp;

using UnitTesting.TypeSystem_TestUtils;

namespace UnitTesting.AssemblyProviders_TestUtils
{
    static class Tools
    {
        public static void AssertPath(this string pathName, string signature, params string[] genericArgs)
        {
            var path = new PathInfo(pathName);

            Assert.AreEqual(pathName, path.Name, "Path name parsing");
            Assert.AreEqual(signature, path.Signature, "Signature parsing");
            CollectionAssert.AreEquivalent(path.GenericArgs, genericArgs, "Generic arguments parsing");
        }

        public static void AssertFullname(this Type type, string fullname)
        {
            var descriptor = TypeDescriptor.Create(type);

            var typeName = descriptor.TypeName;
            Assert.AreEqual(fullname, typeName, "Type name parsing mismatched");
        }

        public static void AssertTokens(this string source, params string[] tokens)
        {
            var lexer = new Lexer(new Source(source, Method.EntryInfo));
            var actualTokens = lexer.GetTokens();
            var actualStringValues = (from token in actualTokens select token.Value).ToArray();

            var message = string.Format("Actual tokens: '{0}'", string.Join("', '", actualStringValues));
            CollectionAssert.AreEqual(tokens, actualStringValues, message);
        }

        public static void AssertName<T>(string typeName)
        {
            var info = TypeDescriptor.Create<T>();

            Assert.AreEqual(typeName, info.TypeName);
        }
    }
}
