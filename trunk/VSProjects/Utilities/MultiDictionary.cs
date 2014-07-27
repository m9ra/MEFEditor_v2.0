using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// Class MultiDictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public class MultiDictionary<TKey, TValue>
    {
        /// <summary>
        /// The _data
        /// </summary>
        private readonly Dictionary<TKey, HashSet<TValue>> _data = new Dictionary<TKey, HashSet<TValue>>();

        /// <summary>
        /// Gets the keys of dictionary.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<TKey> Keys { get { return _data.Keys; } }


        /// <summary>
        /// Adds the value to specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if value was added, <c>false</c> otherwise.</returns>
        public bool Add(TKey key, TValue value)
        {
            HashSet<TValue> storage;
            if (!_data.TryGetValue(key, out storage))
            {
                storage = new HashSet<TValue>();
                _data[key] = storage;
            }

            return storage.Add(value);
        }

        /// <summary>
        /// Adds multiple values to specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public void Add(TKey key, IEnumerable<TValue> values)
        {
            foreach (var value in values)
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// Sets mutliple values to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public void Set(TKey key, IEnumerable<TValue> values)
        {
            _data.Remove(key);

            foreach (var value in values)
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// Removes the specified value of the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if value was removed, <c>false</c> otherwise.</returns>
        public bool Remove(TKey key, TValue value)
        {
            HashSet<TValue> values;
            if (!_data.TryGetValue(key, out values))
            {
                return false;
            }

            return values.Remove(value);
        }

        /// <summary>
        /// Gets values of specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Values of specified key.</returns>
        public IEnumerable<TValue> Get(TKey key)
        {
            HashSet<TValue> storage;
            if (!_data.TryGetValue(key, out storage))
            {
                return new TValue[0];
            }

            return storage;
        }

        /// <summary>
        /// Clears all keys and its values.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }

        /// <summary>
        /// Gets all stored values.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var values in _data.Values)
                {
                    foreach (var value in values)
                    {
                        yield return value;
                    }
                }
            }
        }

    }
}
