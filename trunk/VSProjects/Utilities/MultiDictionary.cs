using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class MultiDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _data = new Dictionary<TKey, HashSet<TValue>>();

        public IEnumerable<TKey> Keys { get { return _data.Keys; } }


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

        public void Add(TKey key, IEnumerable<TValue> values)
        {
            foreach (var value in values)
            {
                Add(key, value);
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            HashSet<TValue> values;
            if (!_data.TryGetValue(key, out values))
            {
                return false;
            }

            return values.Remove(value);
        }

        public IEnumerable<TValue> Get(TKey key)
        {
            HashSet<TValue> storage;
            if (!_data.TryGetValue(key, out storage))
            {
                return new TValue[0];
            }

            return storage;
        }

        public void Clear()
        {
            _data.Clear();
        }

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
