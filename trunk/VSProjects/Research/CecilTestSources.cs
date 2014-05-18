using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using MEFEditor;

/// <summary>
/// Is used for loading via Mono.Cecil
/// </summary>
static class CecilTestSources
{
    static string data;

    static CecilTestSources()
    {
        data = "initialized";
    }

    static string ForLoop()
    {
        string str = "";
        for (int i = 0; i < 10; ++i)
        {
            str += "a";
        }

        return str;
    }

    static string CrossStart()
    {
        var x = "x";
        return x + CrossCall() + data;
    }

    static string CrossCall()
    {
        return "Cross";
    }

    static string IFaceTest(SimpleIface iface)
    {
        return iface.Test();
    }

    static void RunIfaceTest()
    {
        var iface = new IfaceImplementation();
        var result = IFaceTest(iface);
    }

    static string GenericIfaceTest<T>(T a, GenericIface<T> t)
    {
        return t.Test<string>(a, "abc");
    }

    static void RunGenericIfaceTest()
    {
        var iface = new GenericIfaceImplementation<int>();
        var result = GenericIfaceTest<int>(5, iface);
    }

    static T SimpleGenericTest<T>(T input)
    {
        return input;
    }

    static void RunSimpleGenericTest()
    {
        var result = SimpleGenericTest<string>("abcd");
    }
}

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
}

[Export("SelfExport")]
class CECILComponent2
{
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