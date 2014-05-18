using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Skip lat N elements from enumeration
        /// <remarks>
        /// Method implementation taken from forum at http://stackoverflow.com/questions/1779129/how-to-take-all-but-the-last-element-in-a-sequence-using-linq
        /// </remarks>
        /// </summary>
        /// <typeparam name="T">Type of enumeration elements</typeparam>
        /// <param name="source">Source of element </param>
        /// <param name="n">Number of skppied elements</param>
        /// <returns>Enumeration where the last one is skipped</returns>
        public static IEnumerable<T> SkipLastN<T>(this IEnumerable<T> source, int n)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems = false;
            var cache = new Queue<T>(n + 1);

            do
            {
                if (hasRemainingItems = it.MoveNext())
                {
                    cache.Enqueue(it.Current);
                    if (cache.Count > n)
                        yield return cache.Dequeue();
                }
            } while (hasRemainingItems);
        }

        /// <summary>
        /// Perform an action on each element of an IEnumerable<T>, optionally specifying a different action for the final item.
        /// 
        /// <remarks>
        /// Method implementation is taken from https://gist.github.com/matt-hickford/2781446
        /// </remarks>
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action, Action<T> finalAction = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            finalAction = finalAction ?? action;

            using (var iter = source.GetEnumerator())
            {
                if (iter.MoveNext())
                {
                    T item = iter.Current;
                    while (iter.MoveNext())
                    {
                        action(item);
                        item = iter.Current;
                    }
                    finalAction(item);
                }
            }
        }
    }
}
