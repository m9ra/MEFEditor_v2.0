﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    public delegate void DirectMethod(AnalyzingContext context);


    public abstract class MachineSettingsBase
    {
        /// <summary>
        /// Event fired before interpretation by machine starts
        /// </summary>
        public event Action BeforeInterpretation;

        /// <summary>
        /// Event fired after interpretation is ended
        /// </summary>
        public event Action AfterInterpretation;

        /// <summary>
        /// Determine that instance described by given info is represented by
        /// <see cref="DirectInstance"/>
        /// </summary>
        /// <param name="info">Checked info</param>
        /// <returns><c>true</c> if instance is represented by <see cref="DirectInstance"/>, false otherwise</returns>
        public abstract bool IsDirect(InstanceInfo info);

        public abstract InstanceInfo GetNativeInfo(Type literalType);

        public abstract bool IsTrue(Instance condition);

        public abstract MethodID GetSharedInitializer(InstanceInfo sharedInstanceInfo);

        internal void FireBeforeInterpretation()
        {
            if (BeforeInterpretation != null)
                BeforeInterpretation();
        }

        internal void FireAfterInterpretation()
        {
            if (AfterInterpretation != null)
                AfterInterpretation();
        }
    }
}