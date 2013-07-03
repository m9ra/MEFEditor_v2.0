using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem
{

    public class Settings
    {
        internal readonly Type[] DirectTypes;
        internal readonly Dictionary<VersionedName, DirectMethod<MethodID, InstanceInfo>> DirectMethods = new Dictionary<VersionedName, DirectMethod<MethodID, InstanceInfo>>();

        public Settings(params Type[] directTypes)
        {
            DirectTypes = directTypes;
        }

        /// <summary>
        /// Add direct method
        /// NOTE:
        ///     Can override direct methods for specified direct types
        /// </summary>
        public void AddDirectMethod<Type>(MethodID methodID,InstanceInfo[] arguments, DirectMethod<MethodID, InstanceInfo> directMethod)
        {
            var thisType = typeof(Type);
            var thisInfo=new InstanceInfo(thisType.FullName);
            var allArguments=new InstanceInfo[]{thisInfo}.Concat(arguments).ToArray();

            var name= Name.From(methodID, allArguments);
            DirectMethods.Add(name, directMethod);
        }
    }
}
