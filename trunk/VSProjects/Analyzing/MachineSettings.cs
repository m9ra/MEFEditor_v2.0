using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public delegate void DirectMethod(AnalyzingContext context);

    public class MachineSettings
    {
        internal readonly Type[] DirectTypes;
        internal readonly Dictionary<VersionedName, DirectMethod> DirectMethods = new Dictionary<VersionedName, DirectMethod>();

        public MachineSettings(params Type[] directTypes)
        {
            DirectTypes = directTypes;
        }

        /// <summary>
        /// Add direct method
        /// NOTE:
        ///     Can override direct methods for specified direct types
        /// </summary>
        /// <param name="identifier">Method identifier for versioned name</param>
        /// <param name="method">Added method</param>
        public void AddDirectMethod(string identifier, DirectMethod method)
        {
            var name = new VersionedName(identifier, 0);
            DirectMethods.Add(name,method);
        }
    }
}
