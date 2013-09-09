using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Core;

namespace TypeSystem
{
    internal delegate void ChangeEvent(MethodID name);

    public abstract class AssemblyProvider
    {
        #region Template method API

        public abstract GeneratorBase GetMethodGenerator(MethodID method);

        protected TypeServices TypeServices { get; private set; }

        public abstract SearchIterator CreateRootIterator();

        #endregion

        protected void ReportInvalidation(MethodID name)
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

        internal void SetServices(TypeServices services)
        {
            if (TypeServices != null)
            {
                throw new NotSupportedException("Cannot set services twice");
            }

            TypeServices = services;
        }

        internal void UnloadServices()
        {
            TypeServices = null;
        }


    }
}
