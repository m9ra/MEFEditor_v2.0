﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReturnTypeAttribute : Attribute
    {
        public InstanceInfo ReturnInfo { get; private set; }

        public ReturnTypeAttribute(Type returnType)
        {
            ReturnInfo = new InstanceInfo(returnType);
        }

        public ReturnTypeAttribute(string returnTypeFullname)
        {
            ReturnInfo = new InstanceInfo(returnTypeFullname);
        }
    }
}