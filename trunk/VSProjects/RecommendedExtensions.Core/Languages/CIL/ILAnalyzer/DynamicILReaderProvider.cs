using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;



namespace RecommendedExtensions.Core.Languages.CIL.ILAnalyzer
{
    /// <summary>
    /// Taken from answer at: http://stackoverflow.com/questions/14243284/how-can-i-retrieve-string-literals-using-reflection.
    /// </summary>
    internal class DynamicILReaderProvider : IILReaderProvider
    {
        /// <summary>
        /// The type rid prefix.
        /// </summary>
        public const int TypeRidPrefix = 0x02000000;

        /// <summary>
        /// The method rid prefix.
        /// </summary>
        public const int MethodRidPrefix = 0x06000000;

        /// <summary>
        /// The field rid prefix.
        /// </summary>
        public const int FieldRidPrefix = 0x04000000;

        /// <summary>
        /// The runtime dynamic method type.
        /// </summary>
        public static readonly Type RuntimeDynamicMethodType;

        /// <summary>
        /// The file length field.
        /// </summary>
        private static readonly FieldInfo fileLengthField = typeof(ILGenerator).GetField("m_length", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The intermediate language bytes field.
        /// </summary>
        private static readonly FieldInfo IntermediateLanguageBytesField = typeof(ILGenerator).GetField("m_ILStream", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The bake byte array method.
        /// </summary>
        private static readonly MethodInfo bakeByteArrayMethod = typeof(ILGenerator).GetMethod("BakeByteArray", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The dynamic scope indexor.
        /// </summary>
        private static readonly PropertyInfo dynamicScopeIndexor;

        /// <summary>
        /// The dynamic scope field.
        /// </summary>
        private static readonly FieldInfo dynamicScopeField;

        /// <summary>
        /// The generic method information type.
        /// </summary>
        private static readonly Type genericMethodInfoType;

        /// <summary>
        /// The generic method handle field.
        /// </summary>
        private static readonly FieldInfo genericMethodHandleField;

        /// <summary>
        /// The generic method context field.
        /// </summary>
        private static readonly FieldInfo genericMethodContextField;

        /// <summary>
        /// The variable argument method type.
        /// </summary>
        private static readonly Type varArgMethodType;

        /// <summary>
        /// The variable argument method method.
        /// </summary>
        private static readonly FieldInfo varArgMethodMethod;

        /// <summary>
        /// The generic field information type.
        /// </summary>
        private static readonly Type genericFieldInfoType;

        /// <summary>
        /// The generic field information handle.
        /// </summary>
        private static readonly FieldInfo genericFieldInfoHandle;

        /// <summary>
        /// The generic field information context.
        /// </summary>
        private static readonly FieldInfo genericFieldInfoContext;

        /// <summary>
        /// The owner field.
        /// </summary>
        private static readonly FieldInfo ownerField;

        /// <summary>
        /// The dynamic scope.
        /// </summary>
        private object dynamicScope;

        /// <summary>
        /// The generator.
        /// </summary>
        private ILGenerator generator;

        /// <summary>
        /// Initializes static members of the <see cref="DynamicILReaderProvider" /> class.
        /// </summary>
        static DynamicILReaderProvider()
        {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            dynamicScopeIndexor = Type.GetType("System.Reflection.Emit.DynamicScope").GetProperty("Item", bindingFlags);
            dynamicScopeField = Type.GetType("System.Reflection.Emit.DynamicILGenerator").GetField("m_scope", bindingFlags);

            varArgMethodType = Type.GetType("System.Reflection.Emit.VarArgMethod");
            varArgMethodMethod = varArgMethodType.GetField("m_method", bindingFlags);

            genericMethodInfoType = Type.GetType("System.Reflection.Emit.GenericMethodInfo");
            genericMethodHandleField = genericMethodInfoType.GetField("m_methodHandle", bindingFlags);
            genericMethodContextField = genericMethodInfoType.GetField("m_context", bindingFlags);

            genericFieldInfoType = Type.GetType("System.Reflection.Emit.GenericFieldInfo", false);
            if (genericFieldInfoType != null)
            {
                genericFieldInfoHandle = genericFieldInfoType.GetField("m_fieldHandle", bindingFlags);
                genericFieldInfoContext = genericFieldInfoType.GetField("m_context", bindingFlags);
            }
            else
            {
                genericFieldInfoHandle = genericFieldInfoContext = null;
            }

            RuntimeDynamicMethodType = typeof(DynamicMethod).GetNestedType("RTDynamicMethod", BindingFlags.NonPublic);
            ownerField = RuntimeDynamicMethodType.GetField("m_owner", bindingFlags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicILReaderProvider" /> class.
        /// </summary>
        /// <param name="method">The method.</param>
        private DynamicILReaderProvider(DynamicMethod method)
        {
            this.Method = method;
            this.generator = method.GetILGenerator();
            this.dynamicScope = dynamicScopeField.GetValue(this.generator);
        }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <value>The method.</value>
        public DynamicMethod Method { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Object" /> with the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>System.Object.</returns>
        internal object this[int token]
        {
            get
            {
                return dynamicScopeIndexor.GetValue(this.dynamicScope, new object[] { token });
            }
        }

        /// <summary>
        /// Creates the specified method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>DynamicILReaderProvider.</returns>
        /// <exception cref="System.ArgumentNullException">method</exception>
        public static DynamicILReaderProvider Create(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            DynamicMethod dynamicMethod = method as DynamicMethod;
            if (dynamicMethod != null)
            {
                return new DynamicILReaderProvider(dynamicMethod);
            }

            Type methodType = method.GetType();
            if (RuntimeDynamicMethodType.IsAssignableFrom(methodType))
            {
                return new DynamicILReaderProvider(ownerField.GetValue(method) as DynamicMethod);
            }

            return null;
        }

        /// <summary>
        /// Gets the method body in byte code.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        public byte[] GetMethodBody()
        {
            byte[] data = null;
            ILGenerator ilgen = this.Method.GetILGenerator();

            try
            {
                data = (byte[])bakeByteArrayMethod.Invoke(ilgen, null) ?? new byte[0];
            }
            catch (TargetInvocationException)
            {
                int length = (int)fileLengthField.GetValue(ilgen);
                data = new byte[length];
                Array.Copy((byte[])IntermediateLanguageBytesField.GetValue(ilgen), data, length);
            }

            return data;
        }

        /// <summary>
        /// Resolves the field.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>FieldInfo.</returns>
        public FieldInfo ResolveField(int metadataToken)
        {
            object tokenValue = this[metadataToken];
            if (tokenValue is RuntimeFieldHandle)
            {
                return FieldInfo.GetFieldFromHandle((RuntimeFieldHandle)tokenValue);
            }

            if (tokenValue.GetType() == DynamicILReaderProvider.genericFieldInfoType)
            {
                return FieldInfo.GetFieldFromHandle(
                    (RuntimeFieldHandle)genericFieldInfoHandle.GetValue(tokenValue),
                    (RuntimeTypeHandle)genericFieldInfoContext.GetValue(tokenValue));
            }

            return null;
        }

        /// <summary>
        /// Resolves the member.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>MemberInfo.</returns>
        public MemberInfo ResolveMember(int metadataToken)
        {
            if ((metadataToken & TypeRidPrefix) != 0)
            {
                return this.ResolveType(metadataToken);
            }

            if ((metadataToken & MethodRidPrefix) != 0)
            {
                return this.ResolveMethod(metadataToken);
            }

            if ((metadataToken & FieldRidPrefix) != 0)
            {
                return this.ResolveField(metadataToken);
            }

            return null;
        }

        /// <summary>
        /// Resolves the method.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>MethodBase.</returns>
        public MethodBase ResolveMethod(int metadataToken)
        {
            object tokenValue = this[metadataToken];
            DynamicMethod dynamicMethod = tokenValue as DynamicMethod;
            if (dynamicMethod != null)
            {
                return dynamicMethod;
            }

            if (tokenValue is RuntimeMethodHandle)
            {
                return MethodBase.GetMethodFromHandle((RuntimeMethodHandle)this[metadataToken]);
            }

            if (tokenValue.GetType() == DynamicILReaderProvider.genericFieldInfoType)
            {
                return MethodBase.GetMethodFromHandle(
                    (RuntimeMethodHandle)genericMethodHandleField.GetValue(tokenValue),
                    (RuntimeTypeHandle)genericMethodContextField.GetValue(tokenValue));
            }

            if (tokenValue.GetType() == DynamicILReaderProvider.varArgMethodType)
            {
                return DynamicILReaderProvider.varArgMethodMethod.GetValue(tokenValue) as MethodInfo;
            }

            return null;
        }

        /// <summary>
        /// Resolves the signature.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>System.Byte[].</returns>
        public byte[] ResolveSignature(int metadataToken)
        {
            return this[metadataToken] as byte[];
        }
        /// <summary>
        /// Resolves the string.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>System.String.</returns>
        public string ResolveString(int metadataToken)
        {
            return this[metadataToken] as string;
        }

        /// <summary>
        /// Resolves the type.
        /// </summary>
        /// <param name="metadataToken">The metadata token.</param>
        /// <returns>Type.</returns>
        public Type ResolveType(int metadataToken)
        {
            return Type.GetTypeFromHandle((RuntimeTypeHandle)this[metadataToken]);
        }
    }

}
