using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace AssemblyProviders.CIL.ILAnalyzer
{
    public interface IILReaderProvider
    {
        byte[] GetMethodBody();

        FieldInfo ResolveField(int metadataToken);
        MemberInfo ResolveMember(int metadataToken);

        MethodBase ResolveMethod(int metadataToken);
        byte[] ResolveSignature(int metadataToken);

        string ResolveString(int metadataToken);
        Type ResolveType(int metadataToken);
    }
}
