using System;
using System.Reflection;
using System.Reflection.Emit;


namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public sealed class ILInstruction
    {
        public int Offset
        {
            get;
            set;
        }


        public OpCode OpCode
        {
            get;
            set;
        }


        public object Data
        {
            get;
            set;
        }


        public bool IsMethodCall
        {
            get
            {
                return this.Data is MethodInfo;
            }
        }


        public bool IsConstructorCall
        {
            get
            {
                return this.Data is MethodInfo;
            }
        }


        public override string ToString()
        {
            return string.Format("{0} : {1} {2}", this.Offset.ToString("X4"), this.OpCode, FormatData());
        }


        private string FormatData()
        {
            if (this.Data == null) return "";

            MethodInfo methodInfo = this.Data as MethodInfo;
            if (methodInfo != null)
            {
                return methodInfo.ToPrettyString();
            }

            ConstructorInfo constructorInfo = this.Data as ConstructorInfo;
            if (constructorInfo != null)
            {
                return constructorInfo.ToPrettyString();
            }

            if (this.Data is string)
            {
                return "\"" + this.Data + "\"";
            }
            return this.Data.ToString();
        }
    }
}
