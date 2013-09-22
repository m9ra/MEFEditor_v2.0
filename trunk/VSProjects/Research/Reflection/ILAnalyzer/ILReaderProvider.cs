using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace TypeExperiments.Reflection.ILAnalyzer
{

    internal class ILReaderProvider : IILReaderProvider
    {
        public ILReaderProvider(MethodInfo method)
        {
            this.Method = method;
            this.MethodBody = method.GetMethodBody();
            this.MethodModule = method.Module;
        }

        public MethodInfo Method { get; private set; }

        public MethodBody MethodBody { get; private set; }

        public Module MethodModule { get; private set; }

        public byte[] GetMethodBody()
        {
            return this.MethodBody.GetILAsByteArray();
        }

        public FieldInfo ResolveField(int metadataToken)
        {
            return this.MethodModule.ResolveField(metadataToken);
        }

        public MemberInfo ResolveMember(int metadataToken)
        {
            return this.MethodModule.ResolveMember(metadataToken);
        }

        public MethodBase ResolveMethod(int metadataToken)
        {
            return this.MethodModule.ResolveMethod(metadataToken);
        }

        public byte[] ResolveSignature(int metadataToken)
        {
            return this.MethodModule.ResolveSignature(metadataToken);
        }

        public string ResolveString(int metadataToken)
        {
            return this.MethodModule.ResolveString(metadataToken);
        }

        public Type ResolveType(int metadataToken)
        {
            return this.MethodModule.ResolveType(metadataToken);
        }
    }
}
