using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// List sorting multiple values according to key.
    /// </summary>
    /// <typeparam name="TKey">Type of sortable key.</typeparam>
    /// <typeparam name="TValue">Type of stored values.</typeparam>
    public class SortedMultiList<TKey, TValue> : SortedList<TKey, List<TValue>>
    {
        /// <summary>
        /// All stored values sorted according to key.
        /// </summary>
        /// <value>The multi values.</value>
        public IEnumerable<TValue> MultiValues
        {
            get
            {
                foreach (var values in Values)
                {
                    foreach (var value in values)
                        yield return value;
                }
            }
        }

        /// <summary>
        /// Add value according to key into multiple valuea collection.
        /// </summary>
        /// <param name="key">Added key.</param>
        /// <param name="value">Added value.</param>
        public void MultiAdd(TKey key, TValue value)
        {
            List<TValue> values;
            if (!TryGetValue(key, out values))
            {
                values = new List<TValue>();
                this[key] = values;
            }

            values.Add(value);
        }
    }
}
