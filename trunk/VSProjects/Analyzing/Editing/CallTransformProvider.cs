﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class CallTransformProvider:TransformProvider
    {
        public abstract Transformation RemoveArgument(int argumentIndex);
    }
}
