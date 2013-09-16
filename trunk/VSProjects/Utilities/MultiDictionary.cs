using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class MultiDictionary<TKey,TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _data = new Dictionary<TKey, HashSet<TValue>>();


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

        public IEnumerable<TValue> GetExports(TKey key)
        {
              HashSet<TValue> storage;
              if (!_data.TryGetValue(key, out storage))
              {
                  return new TValue[0];
              }

              return storage;
        }
    }
}
