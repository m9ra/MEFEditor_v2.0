using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class PathInfo
    {
        public readonly string Name;

        public readonly string Signature;

        public readonly string ShortSignature;

        public bool HasGenericArguments { get { return GenericArgs.Count > 0; } }

        public readonly List<string> GenericArgs = new List<string>();

        public PathInfo(string name)
        {
            Name = name;
            Signature = parseSignature(Name);
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

            Signature = parseSignature(Name);
            ShortSignature = getShortSignature(Signature);
        }

        private string getShortSignature(string signature)
        {
            if (!signature.EndsWith(">"))
                return signature;

            var methodTypeArgsStart= signature.LastIndexOf('<');

            return signature.Substring(0, methodTypeArgsStart);
        }

        private string parseSignature(string extendingName)
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
                        GenericArgs.Add(argument.ToString());
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
    }
}
