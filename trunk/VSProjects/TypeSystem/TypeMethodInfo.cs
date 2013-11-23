using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class TypeMethodInfo
    {
        public readonly string MethodName;
        public readonly bool IsStatic;
        /// <summary>
        /// Determine that method cannot be invoked directly
        /// </summary>
        public readonly bool IsAbstract;
        public readonly ParameterTypeInfo[] Parameters;
        public readonly InstanceInfo DeclaringType;
        public readonly InstanceInfo ReturnType;
        public readonly MethodID MethodID;
        public readonly bool HasGenericParameters;


        public bool HasThis { get { return !IsStatic; } }

        public readonly PathInfo Path;

        public TypeMethodInfo(InstanceInfo declaringType, string methodName, InstanceInfo returnType, ParameterTypeInfo[] parameters, bool isStatic, bool hasGenericParams = false, bool isAbstract = false)
        {
            if (declaringType == null)
                throw new ArgumentNullException("thisType");

            DeclaringType = declaringType;

            MethodName = methodName;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
            Parameters = parameters;
            ReturnType = returnType;
            MethodID = Naming.Method(declaringType, methodName, false, parameters);
            if (IsAbstract)
                MethodID = new MethodID(MethodID.MethodString, true);

            Path = new PathInfo(declaringType.TypeName + "." + methodName);
            HasGenericParameters = hasGenericParams;
        }



        public TypeMethodInfo MakeGenericMethod(PathInfo searchPath)
        {
            if (searchPath.Signature != Path.Signature)
                throw new NotSupportedException("Cannot make generic method from incompatible path info");

            var translations = new Dictionary<string, string>();
            for (int i = 0; i < searchPath.GenericArgs.Count; ++i)
            {
                var genericParam = Path.GenericArgs[i];
                var genericArg = searchPath.GenericArgs[i];

                translations.Add(genericParam, genericArg);
            }

            //TODO translate parameters, return type,..
            var generic = new TypeMethodInfo(
                translate(DeclaringType, translations), searchPath.Name.Split('.').Last(),
                translate(ReturnType, translations), Parameters,
                IsStatic, false, IsAbstract);

            return generic;
        }

        private InstanceInfo translate(InstanceInfo info, Dictionary<string, string> translations)
        {
            //TODO
            var name = info.TypeName;
            foreach (var pair in translations)
            {
                if (name == pair.Key)
                    name = pair.Value;

                name = name.Replace(pair.Key, pair.Value);
            }
            return new InstanceInfo(name);
        }
    }
}
