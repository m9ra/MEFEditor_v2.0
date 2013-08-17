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
        internal readonly Dictionary<string, MethodItem> DirectMethods = new Dictionary<string, MethodItem>();

        public Settings(params Type[] directTypes)
        {
            DirectTypes = directTypes;
        }

        /// <summary>
        /// Add direct method
        /// NOTE:
        ///     Can override direct methods for specified direct types
        /// </summary>
        public void AddDirectMethod<Type>(MethodID methodID,ParameterInfo[] parameters, DirectMethod<MethodID, InstanceInfo> directMethod)
        {
            var thisType = typeof(Type);
            var thisInfo=new InstanceInfo(thisType.FullName);
            var paramsInfo = from par in parameters select par.StaticInfo;
            var allArguments=new InstanceInfo[]{thisInfo}.Concat(paramsInfo).ToArray();

            var info = new TypeMethodInfo(thisType.FullName,methodID.MethodName, parameters, false);

            var method = new MethodItem(new DirectGenerator(directMethod), info);
            DirectMethods.Add(info.Path, method);
        }
    }
}
