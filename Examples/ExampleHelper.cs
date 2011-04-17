using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Examples
{
    class ExampleHelper
    {
        public static void ExampleSetPrint(string message,Type clazz)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message + "("+clazz.FullName+")");
            Console.ResetColor();
        }
        public static void ExampleMethodPrint(string message, MethodBase method)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message + "(" + method.Name + ")");
            Console.ResetColor();
        }
    }
}
