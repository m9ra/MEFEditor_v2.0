using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace RecommendedExtensions.Core.Languages.CIL.ILAnalyzer
{
    /// <summary>
    /// Taken from answer at: http://stackoverflow.com/questions/14243284/how-can-i-retrieve-string-literals-using-reflection
    /// </summary>
    interface IILReaderProvider
    {
        /// <summary>
        /// Gets the method body in byte code.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        byte[] GetMethodBody();

        /// <summary>
        /// Resolves the field.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>FieldInfo.</returns>
        FieldInfo ResolveField(int metadataToken);

        /// <summary>
        /// Resolves the member.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>MemberInfo.</returns>
        MemberInfo ResolveMember(int metadataToken);

        /// <summary>
        /// Resolves the method.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>MethodBase.</returns>
        MethodBase ResolveMethod(int metadataToken);

        /// <summary>
        /// Resolves the signature.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>System.Byte[].</returns>
        byte[] ResolveSignature(int metadataToken);

        /// <summary>
        /// Resolves the string.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>System.String.</returns>
        string ResolveString(int metadataToken);

        /// <summary>
        /// Resolves the type.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>Type.</returns>
        Type ResolveType(int metadataToken);
    }
}
