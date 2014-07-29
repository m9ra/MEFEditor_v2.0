using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace RecommendedExtensions.Core.Languages.CIL.ILAnalyzer
{
    /// <summary>
    /// Taken from answer at: http://stackoverflow.com/questions/14243284/how-can-i-retrieve-string-literals-using-reflection.
    /// </summary>
    internal class ILReaderProvider : IILReaderProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ILReaderProvider" /> class.
        /// </summary>
        /// <param name="method">The method.</param>
        public ILReaderProvider(MethodInfo method)
        {
            this.Method = method;
            this.MethodBody = method.GetMethodBody();
            this.MethodModule = method.Module;
        }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <value>The method.</value>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Gets the method body.
        /// </summary>
        /// <value>The method body.</value>
        public MethodBody MethodBody { get; private set; }

        /// <summary>
        /// Gets the method module.
        /// </summary>
        /// <value>The method module.</value>
        public Module MethodModule { get; private set; }

        /// <summary>
        /// Gets the method body in byte code.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        public byte[] GetMethodBody()
        {
            return this.MethodBody.GetILAsByteArray();
        }

        /// <summary>
        /// Resolves the field.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>FieldInfo.</returns>
        public FieldInfo ResolveField(int metadataToken)
        {
            return this.MethodModule.ResolveField(metadataToken);
        }

        /// <summary>
        /// Resolves the member.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>MemberInfo.</returns>
        public MemberInfo ResolveMember(int metadataToken)
        {
            return this.MethodModule.ResolveMember(metadataToken);
        }

        /// <summary>
        /// Resolves the method.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>MethodBase.</returns>
        public MethodBase ResolveMethod(int metadataToken)
        {
            return this.MethodModule.ResolveMethod(metadataToken);
        }

        /// <summary>
        /// Resolves the signature.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>System.Byte[].</returns>
        public byte[] ResolveSignature(int metadataToken)
        {
            return this.MethodModule.ResolveSignature(metadataToken);
        }

        /// <summary>
        /// Resolves the string.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>System.String.</returns>
        public string ResolveString(int metadataToken)
        {
            return this.MethodModule.ResolveString(metadataToken);
        }

        /// <summary>
        /// Resolves the type.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>Type.</returns>
        public Type ResolveType(int metadataToken)
        {
            return this.MethodModule.ResolveType(metadataToken);
        }
    }
}
