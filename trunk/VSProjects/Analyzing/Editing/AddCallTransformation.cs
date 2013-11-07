using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    class AddCallTransformation : Transformation
    {
        private readonly CallProvider _provider;

        internal AddCallTransformation(CallProvider provider)
        {
            _provider = provider;
        }

        protected override void apply(TransformationServices services)
        {
            var call = _provider(services);
       //     throw new NotImplementedException();
            services.Abort("Not implemented");
        }

        protected override bool commit()
        {
            throw new NotImplementedException();
        }
    }
}
