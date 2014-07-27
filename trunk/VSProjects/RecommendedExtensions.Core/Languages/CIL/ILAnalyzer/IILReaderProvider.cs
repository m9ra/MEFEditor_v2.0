using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace RecommendedExtensions.Core.Languages.CIL.ILAnalyzer
{
    /// <summary>
    /// Used from answer at: http://stackoverflow.com/questions/14243284/how-can-i-retrieve-string-literals-using-reflection
    /// </summary>
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
