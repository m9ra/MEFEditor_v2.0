using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeExperiments.Core
{
    internal abstract class InternalType
    {
        public TypeName Name { get; private set; }
        public InternalAssembly Assembly { get; private set; }

        public InternalType(TypeName name, InternalAssembly assembly)
        {
            Assembly = assembly;
            Assembly.RegisterType(name, this);
        }


        /// <summary>
        /// Returns true if given instance can be unwrapped. Unwrapped object is returned in obj.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal virtual bool TryUnwrap<T>(Instance instance, out T obj)
        {
            obj = default(T);
            return false;
        }

        /// <summary>
        /// When overriden wrap given object into instance
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal virtual Instance Wrap(object obj)
        {
            throw new NotImplementedException();
        }

        internal virtual Instance ConstructInstance(params object[] args)
        {
            throw new NotImplementedException();
        }
        
  

        #region Invocation handling
        
        protected virtual Instance _invoke(Instance thisInstance, string methodName, params Instance[] args)
        {
            throw new NotImplementedException();
        }

        protected Instance _baseInvoke(Instance thisInstance, string methodName, params Instance[] args)
        {
            throw new NotImplementedException();
        }


        private void beforeInvoke(Instance thisInstance, string methodName, params Instance[] args)
        {

        }

        private void afterInvoke(Instance thisInstance, string methodName,Instance result, params Instance[] args)
        {

        }


        public Instance Invoke(Instance thisInstance, string methodName, params Instance[] args)
        {
            beforeInvoke(thisInstance, methodName, args);
            var result=_invoke(thisInstance, methodName, args);
            afterInvoke(thisInstance, methodName,result, args);
            return result;
        }
        #endregion
    }
}
