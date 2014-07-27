using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// Code taken from
    /// http://stackoverflow.com/questions/2784624/reversed-sorted-dictionary
    /// </summary>
    /// <typeparam name="T">Compared type.</typeparam>
    public sealed class ReverseComparer<T> : IComparer<T>
    {
        /// <summary>
        /// The wrapped comparer
        /// </summary>
        private readonly IComparer<T> wrapped;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReverseComparer{T}"/> class.
        /// </summary>
        public ReverseComparer() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReverseComparer{T}"/> class.
        /// </summary>
        /// <param name="inner">The inner.</param>
        public ReverseComparer(IComparer<T> inner)
        {
            this.wrapped = inner ?? Comparer<T>.Default;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
        int IComparer<T>.Compare(T x, T y) { return wrapped.Compare(y, x); }
    }
}
