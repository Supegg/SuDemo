using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuUtil
{
    public class ConsoleUtil
    {
        public static void WriteWithColor(string text, ConsoleColor forgroundColor, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = forgroundColor;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void WriteLineWithColor(string text, ConsoleColor forgroundColor, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = forgroundColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
