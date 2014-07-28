using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

/// <summary>
/// Test class with generic parameter loaded by Mono.Cecil..
/// </summary>
/// <typeparam name="T">Generic parameter</typeparam>
public class GenericClass<T>
{
    /// <summary>
    /// Method with generic parameter.
    /// </summary>
    /// <typeparam name="M">Generic parameter.</typeparam>
    /// <param name="test">Test value.</param>
    /// <returns>Test value.</returns>
    public M GenericMethod<M>(M test)
    {
        return test;
    }
}

/// <summary>
/// Test class with test cases loaded by Mono.Cecil.
/// </summary>
public class TestClass
{
    /// <summary>
    /// The independent parameters information
    /// </summary>
    public static readonly MethodInfo IndependentParamsInfo = typeof(TestClass).GetMethod("IndependentParams");

    /// <summary>
    /// The dependent parameters information
    /// </summary>
    public static readonly MethodInfo DependentParamsInfo = typeof(TestClass).GetMethod("DependentParams");

    /// <summary>
    /// Independents the parameters.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns>Dictionary&lt;M, T&gt;.</returns>
    /// <exception cref="System.InvalidOperationException">Method is not supposed to be called</exception>
    public Dictionary<M, T> IndependentParams<M, T>()
    {
        throw new InvalidOperationException("Method is not supposed to be called");
    }

    /// <summary>
    /// Dependents the parameters test case.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns>Dictionary&lt;M, M&gt;.</returns>
    /// <exception cref="System.InvalidOperationException">Method is not supposed to be called</exception>
    public Dictionary<M, M> DependentParams<M, T>()
    {
        throw new InvalidOperationException("Method is not supposed to be called");
    }
}


/// <summary>
/// Test class with test cases loaded by Mono.Cecil.
/// </summary>
/// <typeparam name="N1">The type of the n1.</typeparam>
class NamespaceClass<N1>
{
    /// <summary>
    /// Class InnerClass.
    /// </summary>
    /// <typeparam name="C1">The type of the c1.</typeparam>
    internal class InnerClass<C1>
    {

    }

    /// <summary>
    /// Class NamespaceClass2.
    /// </summary>
    /// <typeparam name="N2">The type of the n2.</typeparam>
    internal class NamespaceClass2<N2>
    {
        /// <summary>
        /// Class InnerClass.
        /// </summary>
        /// <typeparam name="C2">The type of the c2.</typeparam>
        internal class InnerClass<C2>
        {

        }
    }

}
