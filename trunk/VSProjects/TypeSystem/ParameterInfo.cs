using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem
{
    public class ParameterInfo
    {
        public readonly string Name;
        public InstanceInfo StaticInfo;


        public ParameterInfo(string name, InstanceInfo staticInfo)
        {
            Name = name;
            StaticInfo = staticInfo;
        }
    }
}
