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
using System.Windows.Markup;
using System.Windows.Data;

namespace wpf_player
{
    /// <summary>
    /// Converter class that converts seconds length to a more human readable string label.
    /// </summary>
    public class LengthConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LengthConverter() { }
        /// <summary>
        /// Converts the value in seconds to a label in the form <c>MM:SS</c>.
        /// </summary>
        /// <param name="value">Time value in seconds</param>
        /// <param name="targetType">Unused param</param>
        /// <param name="parameter">Unused param</param>
        /// <param name="culture">Unused param</param>
        /// <returns>Human readable string for the time value</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double ll;
            try
            {
                ll = (double)value;
            }
            catch (InvalidCastException)
            {
                ll = (int)value;
            }
            TimeSpan ts = TimeSpan.FromSeconds(ll);
            StringBuilder sb = new StringBuilder();
            sb.Append(((long)ts.TotalMinutes).ToString().PadLeft(2, '0'));
            sb.Append(":");
            sb.Append(ts.Seconds.ToString().PadLeft(2, '0'));
            return sb.ToString();
        }
        /// <summary>
        /// Performs inverse conversion. This method splits the string and convert them in a time value expressed in seconds
        /// </summary>
        /// <param name="value">String containing time value information in the form <c>MM:SS</c></param>
        /// <param name="targetType">Unused param</param>
        /// <param name="parameter">Unused param</param>
        /// <param name="culture">Unused param</param>
        /// <returns>The time value in seconds</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            string[] splitted = str.Split(':');
            if (splitted.Length == 2)
            {
                TimeSpan ts = TimeSpan.FromMinutes(Double.Parse(splitted[0]));
                TimeSpan ts_sec = TimeSpan.FromSeconds(Double.Parse(splitted[1]));
                ts.Add(ts_sec);
                return (int)ts.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Method needed for the extension system. Return the current instance.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns>The current instance of the converter</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
