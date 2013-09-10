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

        public readonly static TypeParameterInfo StringParam1 = TypeParameterInfo.Create("p", InstanceInfo.Create<string>());

        public readonly static TypeParameterInfo IntParam1 = TypeParameterInfo.Create("n", InstanceInfo.Create<int>());


        

        public readonly static MethodDescription Void_NoParam = new MethodDescription(InstanceInfo.Void, false);

        public readonly static MethodDescription Void_StringParam = new MethodDescription(InstanceInfo.Void, false, StringParam1);

        public readonly static MethodDescription Int_IntParam = MethodDescription.CreateInstance<int>(IntParam1);

        public readonly static MethodDescription String_NoParam = MethodDescription.CreateInstance<string>();

        public readonly static MethodDescription String_StringParam = MethodDescription.CreateInstance<string>(StringParam1);

        public readonly static MethodDescription StaticString_StringParam = MethodDescription.CreateStatic<string>(StringParam1);

        public readonly static MethodDescription StaticInitializer = Void_NoParam;

        public readonly static MethodDescription Ctor_NoParam = Void_NoParam;

        public readonly static MethodDescription Ctor_StringParam = Void_StringParam;



        public readonly static string EntryMethodPath = "Test.EntryMethod";

        public readonly static MethodDescription Entry_NoParam = Void_NoParam;

        public readonly static TypeMethodInfo EntryInfo = Entry_NoParam.CreateInfo(EntryMethodPath);
    }
}
