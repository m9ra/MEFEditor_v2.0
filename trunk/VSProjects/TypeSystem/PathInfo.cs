using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class PathInfo
    {
        /// <summary>
        /// Regex that is used for replacing type parameters in fullname
        /// </summary>
        public static readonly Regex GenericMatcher = new Regex(@"
     <  
      (?>  [^><]+       | 
        < (?<Depth>)    |
        > (?<-Depth>) 
      )*     
     (?(Depth)(?!))   # Ensure that depth level is at zero
     >
", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public readonly string Name;

        public readonly string Signature;

        public readonly string ShortSignature;

        public readonly List<string> GenericArgs = new List<string>();

        public bool HasGenericArguments { get { return GenericArgs.Count > 0; } }

        /// <summary>
        /// Determine that path marks method with getter naming conventions
        /// </summary>
        public bool IsGetter
        {
            get
            {
                //TODO this workaround is not nice
                return Signature.Contains(Naming.PathDelimiter + Naming.GetterPrefix);
            }
        }

        public string PrePath
        {
            get
            {
                var lastPart = ShortSignature.Split('.').Last();

                return ShortSignature.Substring(0, ShortSignature.Length - lastPart.Length);
            }
        }

        public string LastPart
        {
            get
            {
                return ShortSignature.Substring(PrePath.Length);
            }
        }


        public PathInfo(string name)
        {
            Name = name;
            Signature = parseSignature(Name, GenericArgs);
            ShortSignature = getShortSignature(Signature);
        }

        public PathInfo(Type type)
            : this(TypeDescriptor.Create(type).TypeName)
        {
        }

        public PathInfo(PathInfo path, string extendingName)
        {
            if (path.Name == "")
            {
                Name = extendingName;
            }
            else
            {
                Name = string.Format("{0}.{1}", path.Name, extendingName);
            }

            Signature = parseSignature(Name, GenericArgs);
            ShortSignature = getShortSignature(Signature);
        }



        public static string GetSignature(string typeName)
        {
            var list = new List<string>();
            return parseSignature(typeName, list);
        }

        public static string GetSignature(TypeDescriptor typeDescriptor)
        {
            var list = new List<string>();
            return parseSignature(typeDescriptor.TypeName, list);
        }

        public static string GetNonGenericPath(string path)
        {
            return GenericMatcher.Replace(path, "");
        }

        /// <summary>
        /// Creates name according to current generic arguments
        /// </summary>
        /// <returns>Created name</returns>
        public string CreateName()
        {
            var result = new StringBuilder();

            var argIndex = 0;
            for (int i = 0; i < Signature.Length; ++i)
            {
                var ch = Signature[i];

                result.Append(ch);
                if (ch == '<' || ch == ',')
                {
                    result.Append(GenericArgs[argIndex]);
                    ++argIndex;
                }
            }

            return result.ToString();
        }

        public static PathInfo Append(PathInfo path, string suffix)
        {
            if (path == null)
            {
                return new PathInfo(suffix);
            }
            else
            {
                return new PathInfo(path, suffix);
            }
        }

        private string getShortSignature(string signature)
        {
            if (!signature.EndsWith(">"))
                return signature;

            var methodTypeArgsStart = signature.LastIndexOf('<');

            return signature.Substring(0, methodTypeArgsStart);
        }

        private static string parseSignature(string extendingName, List<string> genericArgs)
        {
            var argument = new StringBuilder();
            var parsedName = new StringBuilder();
            var bracketDepth = 0;

            //determine if char will be included to name according to bracket depth
            var include = true;
            //determine that argument has been read completely
            var argumentEnd = false;

            for (int i = 0; i < extendingName.Length; ++i)
            {
                var ch = extendingName[i];

                include = bracketDepth == 0;
                switch (ch)
                {
                    case '<':
                        ++bracketDepth;
                        break;

                    case ',':
                        //force including trailing comma between arguments
                        include = bracketDepth <= 1;
                        //on trailing comma there is argument end
                        argumentEnd = include;
                        break;
                    case '>':
                        --bracketDepth;
                        //force including ending bracket
                        include = bracketDepth == 0;
                        //on ending bracket there is argument end
                        argumentEnd = include;
                        break;
                }

                if (include)
                {
                    parsedName.Append(ch);
                    if (argumentEnd)
                    {
                        //add argument to collected ones
                        genericArgs.Add(argument.ToString());
                        argument.Clear();
                        argumentEnd = false;
                    }
                }
                else
                {
                    argument.Append(ch);
                }
            }

            return parsedName.ToString();
        }
    }
}
