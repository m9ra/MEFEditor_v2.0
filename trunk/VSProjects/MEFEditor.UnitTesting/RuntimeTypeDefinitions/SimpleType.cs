using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.UnitTesting.RuntimeTypeDefinitions
{
    /// <summary>
    /// Type definition used for testing purposes.
    /// </summary>
    public class SimpleType : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<string> TestProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleType"/> class.
        /// </summary>
        public SimpleType()
        {
            FullName = "SimpleType";
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="data">The data.</param>
        public void _method_ctor(string data)
        {
            TestProperty.Set(data);
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="concated">The concated.</param>
        /// <returns>System.String.</returns>
        public string _method_Concat(string concated = "CallDefault")
        {
            return TestProperty.Get() + "_" + concated;
        }

        /// <summary>
        /// Draws the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        protected override void draw(InstanceDrawer services)
        {
            services.PublishField("TestProperty", TestProperty);
            services.ForceShow();

        }
    }
}
