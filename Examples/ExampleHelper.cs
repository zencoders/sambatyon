/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Examples
{
    /// <summary>
    /// Static class containing some static helper method used during example execution.
    /// </summary>
    class ExampleHelper
    {
        /// <summary>
        /// This method prints on the Console the name of the examples module (set of examples method)
        /// </summary>
        /// <param name="message">Description of the example set</param>
        /// <param name="clazz">Type reference of the example module</param>
        public static void ExampleSetPrint(string message,Type clazz)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write(message + "("+clazz.FullName+")");
            Console.ResetColor();
            Console.WriteLine();
        }
        /// <summary>
        /// This method prints on the Console the name of the example method
        /// </summary>
        /// <param name="message">Description of the actions executed by the example</param>
        /// <param name="method">Method type for the example</param>
        public static void ExampleMethodPrint(string message, MethodBase method)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message + "(" + method.Name + ")");
            Console.ResetColor();
        }
        /// <summary>
        /// This method recursively dump object properties in key-value format and return it in a string.
        /// </summary>
        /// <param name="obj">Object to dump on string</param>
        /// <returns>The string representing the object in key-value format</returns>
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
        /// <summary>
        /// Public method used to dump object on console.
        /// This method uses the private method <see cref="dumpObjectOnString"/>
        /// </summary>
        /// <param name="obj">Object to dump</param>
        public static void DumpObjectProperties(Object obj)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(dumpObjectOnString(obj));
            Console.ResetColor();
        }
    }
}
