using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

namespace MEFEditor.TypeSystem.Runtime
{
    /// <summary>
    /// Runtime direct representation of .NET arrays. It can handle wrapping and unwrapping between native arrays.
    /// </summary>
    /// <typeparam name="ItemType">The type of the item type.</typeparam>
    public class Array<ItemType> : IEnumerable<ItemType>, System.Collections.IEnumerable
        where ItemType : InstanceWrap
    {
        /// <summary>
        /// The stored data.
        /// </summary>
        private readonly Dictionary<string, ItemType> _data = new Dictionary<string, ItemType>();

        /// <summary>
        /// Gets the length of array.
        /// </summary>
        /// <value>The length.</value>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the set item method identifier.
        /// </summary>
        /// <value>The set item method identivier.</value>
        public MethodID SetItemMethod
        {
            get
            {
                var index = ParameterTypeInfo.Create("index", TypeDescriptor.Create<int>());
                var item = ParameterTypeInfo.Create("item", TypeDescriptor.Create<ItemType>());
                return Naming.Method(TypeDescriptor.Create("Array<" + item.Type.TypeName + ",1>"), Naming.IndexerSetter, false, index, item);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Array{ItemType}"/> class.
        /// </summary>
        /// <param name="length">The length of array.</param>
        public Array(int length)
        {            
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Array{ItemType}"/> class from given data.
        /// </summary>
        /// <param name="data">The data which will initialize current array.</param>
        /// <param name="context">The context.</param>
        public Array(IEnumerable data, AnalyzingContext context)
        {
            int i = 0;
            foreach (var item in data)
            {
                //handle wrapping
                var toSet = item as InstanceWrap;
                if (toSet == null)
                {
                    var instance = item as Instance;
                    if (instance == null)
                    {
                        instance = context.Machine.CreateDirectInstance(item);
                    }
                    toSet = new InstanceWrap(instance);
                }

                //set item
                set_Item(i, toSet as ItemType);
                ++i;
            }
            Length = _data.Count;
        }

        #region Supported array members

        /// <summary>
        /// Set value at given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="instance">The instance.</param>
        public void set_Item(int index, ItemType instance)
        {
            _data[getKey(index)] = instance;
        }

        /// <summary>
        /// Get value from given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The instance.</returns>
        public ItemType get_Item(int index)
        {
            var key = getKey(index);

            ItemType value;
            _data.TryGetValue(key, out value);

            return value;
        }

        #endregion

        #region IEnumerable implementations

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        /// <inheritdoc />
        public IEnumerator<ItemType> GetEnumerator()
        {
            return _data.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.Values.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Get value from given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Requested value.</returns>
        private string getKey(int index)
        {
            return index.ToString();
        }

        /// <summary>
        /// Unwraps array to native form.
        /// </summary>
        /// <typeparam name="ResultType">The type of the result type.</typeparam>
        /// <returns>Unwrapped array.</returns>
        internal ResultType Unwrap<ResultType>()
        {
            var elementType = typeof(ResultType).GetElementType();
            var array = Array.CreateInstance(elementType, Length);

            for (int i = 0; i < Length; ++i)
            {
                var item = get_Item(i);

                object value;
                if (typeof(Instance).IsAssignableFrom(elementType))
                {
                    //instance shouldn't been unwrapped
                    value = item.Wrapped;
                }
                else if (item.Wrapped is DirectInstance)
                {
                    value = item.Wrapped.DirectValue;
                }
                else
                {
                    value = item.Wrapped;
                }

                var castedValue = TypeUtilities.DynamicCast(value, elementType);
                array.SetValue(castedValue, i);
            }

            var result = (ResultType)(object)array;
            return result;
        }
    }
}
