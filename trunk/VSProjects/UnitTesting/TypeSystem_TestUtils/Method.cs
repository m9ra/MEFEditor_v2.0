using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem;

namespace UnitTesting.TypeSystem_TestUtils
{
    public static class Method
    {

        public readonly static ParameterTypeInfo StringParam1 = ParameterTypeInfo.Create("p", TypeDescriptor.Create<string>());

        public readonly static ParameterTypeInfo IntParam1 = ParameterTypeInfo.Create("n", TypeDescriptor.Create<int>());

        public readonly static ParameterTypeInfo StringICollectionParam1 = ParameterTypeInfo.Create("c", TypeDescriptor.Create<ICollection<string>>());



        public readonly static MethodDescription Void_NoParam = new MethodDescription(TypeDescriptor.Void, false);

        public readonly static MethodDescription Void_StringParam = new MethodDescription(TypeDescriptor.Void, false, StringParam1);

        public readonly static MethodDescription Int_IntParam = MethodDescription.CreateInstance<int>(IntParam1);

        public readonly static MethodDescription Int_NoParam = MethodDescription.CreateInstance<int>();

        public readonly static MethodDescription Void_IntParam = MethodDescription.CreateInstance<int>(IntParam1);

        public readonly static MethodDescription String_NoParam = MethodDescription.CreateInstance<string>();

        public readonly static MethodDescription String_StringParam = MethodDescription.CreateInstance<string>(StringParam1);

        public readonly static MethodDescription StaticString_StringParam = MethodDescription.CreateStatic<string>(StringParam1);

        public readonly static MethodDescription StaticString_NoParam = MethodDescription.CreateStatic<string>();

        public readonly static MethodDescription StaticVoid_StringParam = new MethodDescription(TypeDescriptor.Void, true, StringParam1);

        public readonly static MethodDescription StaticInitializer = Void_NoParam;

        public readonly static MethodDescription Ctor_NoParam = Void_NoParam;

        public readonly static MethodDescription Ctor_StringParam = Void_StringParam;

        public readonly static MethodDescription StringICollection_StringICollectionParam = MethodDescription.CreateInstance<ICollection<string>>(StringICollectionParam1);


        public readonly static string EntryClass = "Test";

        public readonly static string EntryMethodPath = EntryClass + ".EntryMethod";

        public readonly static MethodDescription Entry_NoParam = Void_NoParam;

        public readonly static TypeMethodInfo EntryInfo = Entry_NoParam.CreateInfo(EntryMethodPath);
    }
}
