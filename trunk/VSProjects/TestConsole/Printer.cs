using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;
using AssemblyProviders.CIL;

using TypeSystem;



namespace TestConsole
{
    /// <summary>
    /// Prints Instruction Analyzing Language program in readable format
    /// </summary>
    static class Printer
    {
        static readonly string[] usings = new[]{
            typeof(List<>).Namespace,
            typeof(VMStack).Namespace,
            "System.ComponentModel.Composition.Hosting"
        };

        static ConsoleColor BySeparatorColor = ConsoleColor.Red;
        static ConsoleColor EqualsSeparatorColor = ConsoleColor.Red;
        static ConsoleColor IfSeparatorColor = ConsoleColor.Red;
        static ConsoleColor ArgumentDelimiterColor = ConsoleColor.Red;

        static ConsoleColor CommentColor = ConsoleColor.DarkGray;
        static ConsoleColor OpcodeColor = ConsoleColor.White;

        static ConsoleColor ArgumentTypeColor = ConsoleColor.Magenta;
        static ConsoleColor ArgumentColor = ConsoleColor.DarkMagenta;

        static ConsoleColor MethodColor = ConsoleColor.Cyan;
        static ConsoleColor StringColor = ConsoleColor.Green;
        static ConsoleColor NumberColor = ConsoleColor.DarkGreen;
        static ConsoleColor BoolColor = ConsoleColor.DarkGreen;
        static ConsoleColor VariableColor = ConsoleColor.DarkYellow;
        static ConsoleColor LabelColor = ConsoleColor.Yellow;

        internal static void PrintVariables(CallContext context)
        {
            foreach (var variable in context.Variables)
            {
                var value = context.GetValue(variable);
                var args = string.Format("{0} = {1}", variable, value);

                printSeparatedArguments(args, "=", EqualsSeparatorColor);
                Console.WriteLine();
            }
        }

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
