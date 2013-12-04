using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem.Runtime
{
    public class Array<ItemType>
        where ItemType : InstanceWrap
    {
        private readonly Dictionary<string, InstanceWrap> _data = new Dictionary<string, InstanceWrap>();

        public int Length { get; private set; }

        public Array(int length)
        {
            //TODO multidimensional array
            Length = length;
        }

        public Array(IEnumerable data, AnalyzingContext context)
        {
            int i=0;
            foreach (var item in data)
            {
                var toSet = item as InstanceWrap;
                if (toSet==null)
                {
                    var instance=item as Instance;
                    if(instance==null){
                        instance=context.Machine.CreateDirectInstance(item);
                    }
                    toSet = new InstanceWrap(instance);
                }
             

                set_Item(i,toSet as ItemType);
                ++i;
            }
            Length = _data.Count;
        }

        public void set_Item(int index, ItemType instance)
        {
            _data[getKey(index)] = instance;
        }

        public ItemType get_Item(int index)
        {
            var key = getKey(index);

            InstanceWrap value;
            _data.TryGetValue(key, out value);

            return value as ItemType;
        }

        private string getKey(int index)
        {
            return index.ToString();
        }

        internal ResultType Unwrap<ResultType>()
        {
            //TODO multidimensional array
            var elementType=typeof(ResultType).GetElementType();
            var array=Array.CreateInstance(elementType, Length);

            for (int i = 0; i < Length; ++i)
            {
                var item = get_Item(i);

                object value;
                if (elementType.IsAssignableFrom(item.Wrapped.GetType()))
                {
                    value = item.Wrapped;
                }
                else
                {
                    value = item.Wrapped.DirectValue;
                }

                array.SetValue(value, i);
            }

            var result = (ResultType)(object)array;
            return result;
        }
    }
}
