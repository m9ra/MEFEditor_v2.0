using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Runtime.Building;

namespace TypeSystem.Runtime
{
    public class DataTypeDefinition : RuntimeTypeDefinition
    {
        /// <summary>
        /// Fields defined in type definition
        /// </summary>
        private readonly List<Field> _fields = new List<Field>();

        /// <summary>
        /// Fullname of defined type
        /// </summary>
        internal protected string FullName { get; protected set; }

        protected void Simulate<T>()
        {
            FullName = typeof(T).FullName;
        }

        internal override InstanceInfo TypeInfo
        {
            get { return new InstanceInfo(FullName); }
        }

        internal void RegisterField(Field directProperty, out string storage)
        {
            _fields.Add(directProperty);
            storage = string.Format("@prop_{0}_{1}_{2}", _fields.Count, directProperty, GetType());
        }

        internal override IEnumerable<RuntimeMethodGenerator> GetMethods()
        {
            //TODO resolve inheritance
            return ContainingAssembly.GetMethodGenerators(this);
        }
    }
}
