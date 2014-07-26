using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;


public class GenericClass<T>
{
    public M GenericMethod<M>(M test)
    {
        return test;
    }
}

public class TestClass
{
    public static readonly MethodInfo IndependentParamsInfo = typeof(TestClass).GetMethod("IndependentParams");

    public static readonly MethodInfo DependentParamsInfo = typeof(TestClass).GetMethod("DependentParams");

    public Dictionary<M, T> IndependentParams<M, T>()
    {
        throw new InvalidOperationException("Method is not supposed to be called");
    }

    public Dictionary<M, M> DependentParams<M, T>()
    {
        throw new InvalidOperationException("Method is not supposed to be called");
    }
}

class NamespaceClass<N1>
{
    internal class InnerClass<C1>
    {

    }

    internal class NamespaceClass2<N2>
    {
        internal class InnerClass<C2>
        {

        }
    }

}
