using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

using MEFEditor.TypeSystem;

namespace MEFEditor.UnitTesting.TypeSystem_TestUtils
{
    /// <summary>
    /// Utility class that defines bunch of method signatures that could be used for testing method definitions.
    /// There is used convention ReturnType_Type1Type2Param that means signature with ReturnType return value
    /// and two parameters with.
    /// </summary>
    public static class Method
    {

        /// <summary>
        /// The string param1.
        /// </summary>
        public readonly static ParameterTypeInfo StringParam1 = ParameterTypeInfo.Create("p", TypeDescriptor.Create<string>());

        /// <summary>
        /// The string param2.
        /// </summary>
        public readonly static ParameterTypeInfo StringParam2 = ParameterTypeInfo.Create("p2", TypeDescriptor.Create<string>());

        /// <summary>
        /// The string param3.
        /// </summary>
        public readonly static ParameterTypeInfo StringParam3 = ParameterTypeInfo.Create("p3", TypeDescriptor.Create<string>());

        /// <summary>
        /// The object param1.
        /// </summary>
        public readonly static ParameterTypeInfo ObjectParam1 = ParameterTypeInfo.Create("p", TypeDescriptor.ObjectInfo);

        /// <summary>
        /// The int param1.
        /// </summary>
        public readonly static ParameterTypeInfo IntParam1 = ParameterTypeInfo.Create("n", TypeDescriptor.Create<int>());

        /// <summary>
        /// The string ICollection param1.
        /// </summary>
        public readonly static ParameterTypeInfo StringICollectionParam1 = ParameterTypeInfo.Create("c", TypeDescriptor.Create<ICollection<string>>());



        /// <summary>
        /// The void no parameter.
        /// </summary>
        public readonly static MethodDescription Void_NoParam = new MethodDescription(TypeDescriptor.Void, false);

        /// <summary>
        /// The void string parameter.
        /// </summary>
        public readonly static MethodDescription Void_StringParam = new MethodDescription(TypeDescriptor.Void, false, StringParam1);

        /// <summary>
        /// The void string string string parameter.
        /// </summary>
        public readonly static MethodDescription Void_StringStringStringParam = new MethodDescription(TypeDescriptor.Void, false, StringParam1, StringParam2, StringParam3);

        /// <summary>
        /// The void object parameter.
        /// </summary>
        public readonly static MethodDescription Void_ObjectParam = new MethodDescription(TypeDescriptor.Void, false, ObjectParam1);

        /// <summary>
        /// The object no parameter.
        /// </summary>
        public readonly static MethodDescription Object_NoParam = MethodDescription.CreateInstance<object>();

        /// <summary>
        /// The int int parameter.
        /// </summary>
        public readonly static MethodDescription Int_IntParam = MethodDescription.CreateInstance<int>(IntParam1);

        /// <summary>
        /// The int no parameter.
        /// </summary>
        public readonly static MethodDescription Int_NoParam = MethodDescription.CreateInstance<int>();

        /// <summary>
        /// The void int parameter.
        /// </summary>
        public readonly static MethodDescription Void_IntParam = MethodDescription.CreateInstance<int>(IntParam1);

        /// <summary>
        /// The string no parameter.
        /// </summary>
        public readonly static MethodDescription String_NoParam = MethodDescription.CreateInstance<string>();

        /// <summary>
        /// The string string parameter.
        /// </summary>
        public readonly static MethodDescription String_StringParam = MethodDescription.CreateInstance<string>(StringParam1);

        /// <summary>
        /// The static string string parameter.
        /// </summary>
        public readonly static MethodDescription StaticString_StringParam = MethodDescription.CreateStatic<string>(StringParam1);

        /// <summary>
        /// The static string int parameter.
        /// </summary>
        public readonly static MethodDescription StaticString_IntParam = MethodDescription.CreateStatic<string>(IntParam1);

        /// <summary>
        /// The static string no parameter.
        /// </summary>
        public readonly static MethodDescription StaticString_NoParam = MethodDescription.CreateStatic<string>();

        /// <summary>
        /// The static void string parameter.
        /// </summary>
        public readonly static MethodDescription StaticVoid_StringParam = new MethodDescription(TypeDescriptor.Void, true, StringParam1);

        /// <summary>
        /// The static initializer.
        /// </summary>
        public readonly static MethodDescription StaticInitializer = Void_NoParam;

        /// <summary>
        /// The ctor no parameter.
        /// </summary>
        public readonly static MethodDescription Ctor_NoParam = Void_NoParam;

        /// <summary>
        /// The ctor string parameter.
        /// </summary>
        public readonly static MethodDescription Ctor_StringParam = Void_StringParam;

        /// <summary>
        /// The string ICollection string ICollection parameter.
        /// </summary>
        public readonly static MethodDescription StringICollection_StringICollectionParam = MethodDescription.CreateInstance<ICollection<string>>(StringICollectionParam1);


        /// <summary>
        /// The entry class.
        /// </summary>
        public readonly static string EntryClass = "Test";

        /// <summary>
        /// The entry method path.
        /// </summary>
        public readonly static string EntryMethodPath = EntryClass + ".EntryMethod";

        /// <summary>
        /// The entry_ no parameter.
        /// </summary>
        public readonly static MethodDescription Entry_NoParam = Void_NoParam;

        /// <summary>
        /// The entry information.
        /// </summary>
        public readonly static TypeMethodInfo EntryInfo = Entry_NoParam.CreateInfo(EntryMethodPath);
    }
}
