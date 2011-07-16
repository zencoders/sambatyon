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
    public class LengthConverter : MarkupExtension, IValueConverter
    {
        public LengthConverter() { }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double ll;
            try
            {
                ll = (double)value;
            }
            catch (InvalidCastException invc)
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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
