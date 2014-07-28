using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Utility class that helps with parsing different kinds of path.
    /// </summary>
    public class PathInfo
    {
        /// <summary>
        /// Regex that is used for replacing type parameters in fullname.
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

        /// <summary>
        /// The name of path (in form of Namespace{T}.Name{T2}).
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The signature of path (in form of Namespace{}.Name{}).
        /// </summary>
        public readonly string Signature;

        /// <summary>
        /// The short signature of path (in form of Namespace{}.Name).
        /// </summary>
        public readonly string ShortSignature;

        /// <summary>
        /// The generic arguments of path.
        /// </summary>
        public readonly List<string> GenericArgs = new List<string>();

        /// <summary>
        /// Gets a value indicating whether this instance has generic arguments.
        /// </summary>
        /// <value><c>true</c> if this instance has generic arguments; otherwise, <c>false</c>.</value>
        public bool HasGenericArguments { get { return GenericArgs.Count > 0; } }

        /// <summary>
        /// Gets the pre path signature (in form of Namespace{}).
        /// </summary>
        /// <value>The pre path signature.</value>
        public string PrePathSignature
        {
            get
            {
                //we can take last index, because it is short signature
                var lastDelimiter = ShortSignature.LastIndexOf(Naming.PathDelimiter);
                if (lastDelimiter < 0)
                    //there is no prepath
                    return "";

                return ShortSignature.Substring(0, lastDelimiter);
            }
        }

        /// <summary>
        /// Gets the last part signature (in form of Name).
        /// </summary>
        /// <value>The last part signature.</value>
        public string LastPartSignature
        {
            get
            {
                //we can take last index, because it is short signature
                var lastDelimiter = ShortSignature.LastIndexOf(Naming.PathDelimiter);
                if (lastDelimiter < 0)
                    //there is no prepath
                    return "";

                return ShortSignature.Substring(lastDelimiter + 1);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PathInfo" /> class.
        /// </summary>
        /// <param name="name">The path name.</param>
        public PathInfo(string name)
        {
            Name = name;
            Signature = parseSignature(Name, GenericArgs);
            ShortSignature = getShortSignature(Signature);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathInfo" /> class.
        /// </summary>
        /// <param name="type">The type which name defines current path.</param>
        public PathInfo(Type type)
            : this(TypeDescriptor.Create(type).TypeName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathInfo" /> class.
        /// </summary>
        /// <param name="path">The extended path.</param>
        /// <param name="extendingName">Extending name.</param>
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



        /// <summary>
        /// Gets the signature from given path name.
        /// </summary>
        /// <param name="pathName">Name of the path.</param>
        /// <returns>System.String.</returns>
        public static string GetSignature(string pathName)
        {
            var list = new List<string>();
            return parseSignature(pathName, list);
        }

        /// <summary>
        /// Gets signature of given descriptor.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <returns>System.String.</returns>
        public static string GetSignature(TypeDescriptor typeDescriptor)
        {
            var list = new List<string>();
            return parseSignature(typeDescriptor.TypeName, list);
        }

        /// <summary>
        /// Gets the non generic path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        public static string GetNonGenericPath(string path)
        {
            return GenericMatcher.Replace(path, "");
        }

        /// <summary>
        /// Creates name according to current generic arguments.
        /// </summary>
        /// <returns>Created name.</returns>
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

        /// <summary>
        /// Appends the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>PathInfo.</returns>
        public static PathInfo Append(PathInfo path, string suffix)
        {
            if (suffix == null)
                return path;

            if (path == null)
            {
                return new PathInfo(suffix);
            }
            else
            {
                return new PathInfo(path, suffix);
            }
        }

        /// <summary>
        /// Gets the short signature.
        /// </summary>
        /// <param name="signature">The signature.</param>
        /// <returns>System.String.</returns>
        private string getShortSignature(string signature)
        {
            if (!signature.EndsWith(">"))
                return signature;

            var methodTypeArgsStart = signature.LastIndexOf('<');

            return signature.Substring(0, methodTypeArgsStart);
        }

        /// <summary>
        /// Parses the signature.
        /// </summary>
        /// <param name="extendingName">Name of the extending.</param>
        /// <param name="genericArgs">The generic arguments.</param>
        /// <returns>System.String.</returns>
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
