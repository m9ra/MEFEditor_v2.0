using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeExperiments
{
    /// <summary>
    /// Prints Instruction Analyzing Language program in readable format
    /// </summary>
    static class PrinterIAL
    {
        static ConsoleColor BySeparatorColor = ConsoleColor.Red;
        static ConsoleColor IfSeparatorColor = ConsoleColor.Red;
        static ConsoleColor ArgumentDelimiterColor = ConsoleColor.Red;

        static ConsoleColor CommentColor = ConsoleColor.DarkGray;
        static ConsoleColor OpcodeColor = ConsoleColor.White;

        static ConsoleColor ArgumentTypeColor = ConsoleColor.Magenta;
        static ConsoleColor ArgumentColor = ConsoleColor.DarkMagenta;

        static ConsoleColor NameColor = ConsoleColor.Cyan;
        static ConsoleColor StringColor = ConsoleColor.Green;
        static ConsoleColor NumberColor = ConsoleColor.DarkGreen;
        static ConsoleColor BoolColor = ConsoleColor.DarkGreen;
        static ConsoleColor VariableColor = ConsoleColor.DarkYellow;
        static ConsoleColor LabelColor = ConsoleColor.Yellow;

        public static void Print(string code)
        {
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
                print(CommentColor, comment);
                Console.WriteLine();
            }
        }

        static void printInstruction(string instructionLine)
        {
            var instruction = instructionLine.Split(new char[] { ' ' }, 2);
            var opCode = instruction[0].Trim();
            print(OpcodeColor,"  " + opCode.PadRight(15));

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
                print(separatorColor, separator);
                printArguments(separedParts[1]);
            }
        }

        static void printArguments(string argumentsPart)
        {
            var arguments = argumentsPart.Split(',');
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
                print(NameColor, argumentPart);
            }
        }

        static void printValue(string typePart, string valuePart)
        {
            typePart = typePart.Trim();
            valuePart = valuePart.Trim();
            switch (typePart)
            {
                case "[Variable]":
                    print(VariableColor, valuePart);
                    break;
                case "[Label]":
                    print(LabelColor, valuePart);
                    break;
                case "[System.String]":
                    print(StringColor, "\"{0}\"", valuePart);
                    break;
                case "[System.Int32]":
                    print(NumberColor, valuePart);
                    break;
                case "[System.Boolean]":
                    print(BoolColor, valuePart);
                    break;
                default:
                    print(ArgumentTypeColor, typePart);
                    print(ArgumentColor, valuePart);
                    break;
            }
        }

        static void print(ConsoleColor color, string text, params string[] formatArgs)
        {
            Console.ForegroundColor = color;
            Console.Write(text, formatArgs);
        }
    }
}
