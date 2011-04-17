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
            Console.Write(message + "("+clazz.FullName+")");
            Console.ResetColor();
            Console.WriteLine();
        }
        public static void ExampleMethodPrint(string message, MethodBase method)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message + "(" + method.Name + ")");
            Console.ResetColor();
        }
        public static void DumpObjectProperties(Object obj)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Type tp = obj.GetType();
            PropertyInfo[] infos = tp.GetProperties();
            foreach (PropertyInfo prop in infos)
            {
                Console.WriteLine(prop.Name + " : " + prop.GetValue(obj, null).ToString());
            }
            Console.ResetColor();
        }
    }
}
