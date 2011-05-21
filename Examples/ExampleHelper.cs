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
        private static string dumpObjectOnString(Object obj)
        {
            StringBuilder outputBuilder=new StringBuilder();
            Type tp = obj.GetType();
            PropertyInfo[] infos = tp.GetProperties();
            foreach (PropertyInfo prop in infos)
            {
                object propValue = prop.GetValue(obj, null);                
                outputBuilder.AppendLine(prop.Name + " : " + propValue.ToString());
                if (propValue is System.Collections.ICollection)
                {
                    System.Collections.ICollection col = propValue as System.Collections.ICollection;
                    int k=0;
                    foreach (object colObj in col)
                    {                        
                        outputBuilder.Append(k++).Append(") ");
                        outputBuilder.Append(dumpObjectOnString(colObj));
                    }
                    outputBuilder.AppendLine("#EndCollection "+prop.Name);
                } 
            }
            return outputBuilder.ToString();
        }
        public static void DumpObjectProperties(Object obj)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(dumpObjectOnString(obj));
            Console.ResetColor();
        }
    }
}
