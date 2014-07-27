using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using MEFEditor;

/// <summary>
/// Test sources that are loaded in compiled way by Mono.Cecil.
/// <remarks>These test are used for usage in TestConsole - not in automation tests</remarks>
/// </summary>
static class CecilTestSources
{
    static string data;

    /// <summary>
    /// Test case
    /// </summary>
    static CecilTestSources()
    {
        data = "initialized";
    }

    /// <summary>
    /// Test case
    /// </summary>
    static string ForLoop()
    {
        string str = "";
        for (int i = 0; i < 10; ++i)
        {
            str += "a";
        }

        return str;
    }

    /// <summary>
    /// Test case
    /// </summary>
    static string CrossStart()
    {
        var x = "x";
        return x + CrossCall() + data;
    }

    /// <summary>
    /// Test case
    /// </summary>
    static string CrossCall()
    {
        return "Cross";
    }

    /// <summary>
    /// Test case
    /// </summary>
    static string IFaceTest(SimpleIface iface)
    {
        return iface.Test();
    }

    /// <summary>
    /// Test case
    /// </summary>
    static void RunIfaceTest()
    {
        var iface = new IfaceImplementation();
        var result = IFaceTest(iface);
    }

    /// <summary>
    /// Test case
    /// </summary>
    static string GenericIfaceTest<T>(T a, GenericIface<T> t)
    {
        return t.Test<string>(a, "abc");
    }

    /// <summary>
    /// Test case
    /// </summary>
    [CompositionPoint]
    static void RunGenericIfaceTest()
    {
        var iface = new GenericIfaceImplementation<int>();
        var result = GenericIfaceTest<int>(5, iface);
    }

    /// <summary>
    /// Test case
    /// </summary>
    [CompositionPoint]
    static void RunGenericTest()
    {
        var impl = new GenericIfaceImplementation<int>();
        impl.Test2<string>(4, "b", "c");
    }

    /// <summary>
    /// Test case
    /// </summary>
    static T SimpleGenericTest<T>(T input)
    {
        return input;
    }

    /// <summary>
    /// Test case
    /// </summary>
    static void RunSimpleGenericTest()
    {
        var result = SimpleGenericTest<string>("abcd");
    }

    /// <summary>
    /// Test case
    /// </summary>
    static void RunImplicitArrayTest()
    {
        var type = typeof(CecilComponent);
        var typeCat = new TypeCatalog(type);

    }
    /// <summary>
    /// Test case
    /// </summary>
    static void RunExplicitArrayTest()
    {
        var x = new string[]{
            "abc",
            "def"
        };

        var result = x[0] + x[1];
    }

    /// <summary>
    /// Test case
    /// </summary>
    static void RunFieldTest()
    {
        var obj = new CECILComponent2();
        var getterTest = obj.ExportField + "abc";

        //setter test
        obj.ExportField += "def";
    }
}

/// <summary>
/// Test component for loading with Mono.Cecil
/// </summary>
class CecilComponent
{
    [CompositionPoint]
    public CecilComponent()
    {
        var cat = new AssemblyCatalog("TestPath.exe");
    }
}

class CompositionPointTesting
{
    [CompositionPoint("testpath")]
    public void CompositionPoint_SingleArgument(string path)
    {
        var x = new DirectoryCatalog(path);
    }

    [CompositionPoint(typeof(CecilComponent))]
    public void CompositionPoint_TypeArgument(Type type)
    {
        var x = new TypeCatalog(type);
    }
}

interface SimpleIface
{
    string Test();
}

class IfaceImplementation : SimpleIface
{
    public string Test()
    {
        return "IfaceImplementation.Test";
    }
}


interface GenericIface<TIface>
{
    string Test<TMethod>(TIface a, TMethod b);
}

class GenericIfaceImplementation<TImpl> : GenericIface<TImpl>
{
    public string Test<TMethod>(TImpl a, TMethod b)
    {
        return "GenericIfaceImplementation.Test";
    }

    public string Test2<TMethod>(TImpl a, TMethod b, TMethod c)
    {
        return "GenericIfaceImplementation.Test";
    }
}

[Export("SelfExport")]
class CECILComponent2
{
    [Export("Contract0")]
    public string ExportField = "Default";

    [Export("Contract1")]
    public string ExportProperty { get; private set; }

    [Import("Contract2", AllowDefault = true)]
    public string ImportProperty { get; private set; }

    [ImportMany("Contract3")]
    public List<string> ImportManyPropertyCollection { get; private set; }

    [ImportMany("Contract4")]
    public IEnumerable<int> ImportManyPropertyEnumerable { get; private set; }

    [ImportMany("Contract5")]
    public string[] ImportManyPropertyArray { get; private set; }

    [ImportMany("Contract6")]
    public CustomCollection<string> ImportManyPropertyCustom { get; private set; }
}

class CustomCollection<T> : ICollection<T>
{
    private List<T> _storage = new List<T>();

    public void Add(T item)
    {
        _storage.Add(item);
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public int Count
    {
        get { throw new NotImplementedException(); }
    }

    public bool IsReadOnly
    {
        get { throw new NotImplementedException(); }
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}