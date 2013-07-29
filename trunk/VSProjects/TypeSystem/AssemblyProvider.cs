using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Core;

namespace TypeSystem
{
    internal delegate void ChangeEvent(VersionedName name);

    public abstract class AssemblyProvider
    {
        #region Template method API

        protected abstract string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo);

        protected abstract IInstructionGenerator getGenerator(string methodName);

        #endregion

        protected void ReportInvalidation(VersionedName name)
        {
            throw new NotImplementedException();
        }

        protected void AddComponent()
        {
            throw new NotImplementedException();
        }

        protected void RemoveComponent()
        {
            throw new NotImplementedException();
        }

        protected void StartTransaction()
        {
            throw new NotImplementedException();
        }

        protected void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        internal void HookChange(ChangeEvent changeEvent)
        {
            throw new NotImplementedException();
        }

        internal string ResolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            throw new NotImplementedException();
        }

        internal IInstructionGenerator<MethodID, InstanceInfo> GetGenerator(VersionedName methodName)
        {
            return getGenerator(methodName.Name);
        }
    }
}
