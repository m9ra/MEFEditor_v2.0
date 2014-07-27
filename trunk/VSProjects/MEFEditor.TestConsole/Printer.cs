using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;
using RecommendedExtensions.Core.Languages.CIL;

using MEFEditor.TypeSystem;



namespace MEFEditor.TestConsole
{
    /// <summary>
    /// Prints Instruction Analyzing Language program in readable format.
    /// </summary>
    public static class Printer
    {

        /// <summary>
        /// Namespaces that are contracted in output.
        /// </summary>
        static readonly string[] usings = new[]{
            typeof(List<>).Namespace,
            typeof(VMStack).Namespace,
            "System.ComponentModel.Composition.Hosting"
        };

        #region Color definitions

        /// <summary>
        /// Color of by separator.
        /// </summary>
        internal static ConsoleColor BySeparatorColor = ConsoleColor.Red;

        /// <summary>
        /// Color of equals.
        /// </summary>
        internal static ConsoleColor EqualsSeparatorColor = ConsoleColor.Red;

        /// <summary>
        /// Color of if.
        /// </summary>
        internal static ConsoleColor IfSeparatorColor = ConsoleColor.Red;

        /// <summary>
        /// Color of arguments delimiter.
        /// </summary>
        internal static ConsoleColor ArgumentDelimiterColor = ConsoleColor.Red;

        /// <summary>
        /// Color of comments.
        /// </summary>
        internal static ConsoleColor CommentColor = ConsoleColor.DarkGray;

        /// <summary>
        /// Color of opcode.
        /// </summary>
        internal static ConsoleColor OpcodeColor = ConsoleColor.White;

        /// <summary>
        /// Color of argument type.
        /// </summary>
        internal static ConsoleColor ArgumentTypeColor = ConsoleColor.Magenta;

        /// <summary>
        /// Color of argument.
        /// </summary>
        internal static ConsoleColor ArgumentColor = ConsoleColor.DarkMagenta;

        /// <summary>
        /// Color of method.
        /// </summary>
        internal static ConsoleColor MethodColor = ConsoleColor.Cyan;

        /// <summary>
        /// Color of string.
        /// </summary>
        internal static ConsoleColor StringColor = ConsoleColor.Green;

        /// <summary>
        /// Color of number.
        /// </summary>
        internal static ConsoleColor NumberColor = ConsoleColor.DarkGreen;

        /// <summary>
        /// Color of bool.
        /// </summary>
        internal static ConsoleColor BoolColor = ConsoleColor.DarkGreen;

        /// <summary>
        /// Color of variable.
        /// </summary>
        internal static ConsoleColor VariableColor = ConsoleColor.DarkYellow;

        /// <summary>
        /// Color of label.
        /// </summary>
        internal static ConsoleColor LabelColor = ConsoleColor.Yellow;

        /// <summary>
        /// Color of code.
        /// </summary>
        internal static ConsoleColor CodeColor = ConsoleColor.Gray;

        #endregion

        #region Printing services

        /// <summary>
        /// Prints the variables of given <see cref="CallContext"/>.
        /// </summary>
        /// <param name="context">The context which variables will be printed.</param>
        public static void PrintVariables(CallContext context)
        {
            foreach (var variable in context.Variables)
            {
                var value = context.GetValue(variable);
                var args = string.Format("{0} = {1}", variable, value);

                printSeparatedArguments(args, "=", EqualsSeparatorColor);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Print text with specified color on console output.
        /// </summary>
        /// <param name="color">Color of printed output.</param>
        /// <param name="text">Text printed to output.</param>
        /// <param name="formatArgs">Format arguments for printed text.</param>
        public static void Print(ConsoleColor color, string text, params object[] formatArgs)
        {
            var output = string.Format(text, formatArgs);
            foreach (var ns in usings)
            {
                output = output.Replace(ns, "@");
            }

            Console.ForegroundColor = color;
            Console.Write(output);
        }

        /// <summary>
        /// Prints the highlighted form of Instruction Analyzing Language.
        /// </summary>
        /// <param name="code">The IAL code.</param>
        public static void PrintIAL(string code)
        {
            code = code.Replace("{", "{{").Replace("}", "}}");
            var lines = code.Split('\n');

            foreach (var line in lines)
            {
                var isInstructionLine = line.Length > 0 && char.IsLetter(line[0]);
                if (isInstructionLine)
                {
                    printInstruction(line);
                }
                else
                {
                    printComment(line);
                }
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Print count-times new line on console output.
        /// </summary>
        /// <param name="count">Number of printed new lines.</param>
        public static void PrintLines(int count = 1)
        {
            for (int i = 0; i < count; ++i)
                Print(ConsoleColor.White, "\n");
        }

        /// <summary>
        /// Print text with specified color on console output. Text is suffixed by new line character.
        /// </summary>
        /// <param name="color">Color of printed output.</param>
        /// <param name="text">Text printed to output.</param>
        /// <param name="formatArgs">Format arguments for printed text.</param>
        public static void Println(ConsoleColor color, string text, params object[] formatArgs)
        {
            Print(color, text + "\n", formatArgs);
        }

        /// <summary>
        /// Format indented source.
        /// </summary>
        /// <param name="source">Source to be formatted.</param>
        /// <returns>Formatted source.</returns>
        public static void PrintCode(string source)
        {
            var result = new StringBuilder();

            var lines = source.Replace("\r", "").Split('\n');

            var currIndent = 0;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (line.Contains('}'))
                {
                    --currIndent;
                }

                if (currIndent < 0)
                {
                    currIndent = 0;
                }

                var indentedLine = "".PadLeft(currIndent * 3, ' ') + line.Trim();
                result.AppendLine(indentedLine);

                if (line.Contains('{'))
                {
                    ++currIndent;
                }
            }

            Print(CodeColor, "{0}", result);
        }

        #endregion


        /// <summary>
        /// Prints the comment.
        /// </summary>
        /// <param name="comment">The comment.</param>
        static void printComment(string comment)
        {
            if (comment.StartsWith("["))
            {
                //label
                printArgument(comment);
                Console.WriteLine(":");
            }
            else
            {
                //comment, whitespace
                Print(CommentColor, comment);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Prints the instruction.
        /// </summary>
        /// <param name="instructionLine">The instruction line.</param>
        static void printInstruction(string instructionLine)
        {
            var instruction = instructionLine.Split(new char[] { ' ' }, 2);
            var opCode = instruction[0].Trim();
            Print(OpcodeColor, "  " + opCode.PadRight(15));

            if (instruction.Length > 1)
            {
                var argumentsPart = instruction[1];
                switch (opCode)
                {
                    case "ensure_init":
                        printSeparatedArguments(argumentsPart, " by ", BySeparatorColor);
                        break;
                    case "jmp":
                        printSeparatedArguments(argumentsPart, " if ", IfSeparatorColor);
                        break;
                    default:
                        printArguments(argumentsPart);
                        break;
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Prints arguments that are separated by given separator.
        /// </summary>
        /// <param name="argumentsPart">The arguments part.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="separatorColor">Color of the separator.</param>
        static void printSeparatedArguments(string argumentsPart, string separator, ConsoleColor separatorColor)
        {
            var separedParts = argumentsPart.Split(new string[] { separator }, 2, StringSplitOptions.None);
            printArguments(separedParts[0]);
            if (separedParts.Length > 1)
            {
                Print(separatorColor, separator);
                printArguments(separedParts[1]);
            }
        }

        /// <summary>
        /// Prints the arguments.
        /// </summary>
        /// <param name="argumentsPart">The arguments part of instruction.</param>
        static void printArguments(string argumentsPart)
        {
            var arguments = splitOutOfBrackets(',', argumentsPart);
            var first = true;
            foreach (var argument in arguments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.ForegroundColor = ArgumentDelimiterColor;
                    Console.Write(",");
                }
                printArgument(argument.Trim());
            }
        }

        /// <summary>
        /// Prints the argument.
        /// </summary>
        /// <param name="argumentPart">The argument part of instruction.</param>
        static void printArgument(string argumentPart)
        {
            if (argumentPart[0] == '[')
            {
                var index = argumentPart.IndexOf(']') + 1;
                var typePart = argumentPart.Substring(0, index);
                var valuePart = argumentPart.Substring(index);
                printValue(typePart, valuePart);
            }
            else
            {
                Print(MethodColor, argumentPart);
            }
        }

        /// <summary>
        /// Prints the value of given type.
        /// </summary>
        /// <param name="typePart">The type part of instruction.</param>
        /// <param name="valuePart">The value part of instruction.</param>
        static void printValue(string typePart, string valuePart)
        {
            typePart = typePart.Trim();
            valuePart = valuePart.Trim();
            switch (typePart)
            {
                case "[Variable]":
                    Print(VariableColor, valuePart);
                    break;
                case "[Label]":
                    Print(LabelColor, valuePart);
                    break;
                case "[System.String]":
                    Print(StringColor, "\"{0}\"", valuePart);
                    break;
                case "[System.Int32]":
                    Print(NumberColor, valuePart);
                    break;
                case "[System.Boolean]":
                    Print(BoolColor, valuePart);
                    break;
                case "[Method]":
                    Print(MethodColor, valuePart);
                    break;
                case "[InstanceInfo]":
                    Print(ArgumentTypeColor, valuePart);
                    break;
                default:
                    Print(ArgumentTypeColor, typePart);
                    Print(ArgumentColor, valuePart);
                    break;
            }
        }

        /// <summary>
        /// Splits the given data according to split character. Brackets are considered.
        /// </summary>
        /// <param name="splitCharacter">The split character.</param>
        /// <param name="data">The data.</param>
        /// <returns>Split data.</returns>
        private static string[] splitOutOfBrackets(char splitCharacter, string data)
        {
            var result = new List<string>();
            var bracketDetph = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                var ch = data[i];

                if (bracketDetph == 0 && ch == splitCharacter)
                {
                    //split data
                    var token1 = data.Substring(0, i);
                    var token2 = data.Substring(i + 1);
                    result.Add(token1);
                    data = token2;
                    i = -1;
                    continue;
                }

                switch (ch)
                {
                    case '<':
                    case '[':
                    case '(':
                        ++bracketDetph;
                        break;
                    case '>':
                    case ']':
                    case ')':
                        --bracketDetph;
                        break;
                }
            }
            result.Add(data.Trim());

            return result.ToArray();
        }
    }
}
