using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace TypeSystem
{

    public delegate void AssemblyAction(AssemblyProvider provider);

    public abstract class AssemblyCollectionBase : IEnumerable<AssemblyProvider>
    {

        private List<AssemblyProvider> _assemblies=new List<AssemblyProvider>();

        public event AssemblyAction OnAdd;

        public event AssemblyAction OnRemove;


        protected void addAssembly(AssemblyProvider assembly)
        {
            _assemblies.Add(assembly);

            if (OnAdd != null)
            {
                OnAdd(assembly);
            }
        }

        protected void removeAssembly(AssemblyProvider assembly)
        {
            _assemblies.Remove(assembly);

            if (OnRemove != null)
            {
                OnRemove(assembly);
            }
        }

        public IEnumerator<AssemblyProvider> GetEnumerator()
        {
            return _assemblies.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _assemblies.GetEnumerator();
        }
    }
}
