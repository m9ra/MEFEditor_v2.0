using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

class CecilComponent
{
    [CompositionPoint]
    public CecilComponent()
    {
        var cat = new DirectoryCatalog("TestPath.exe");

    }
}