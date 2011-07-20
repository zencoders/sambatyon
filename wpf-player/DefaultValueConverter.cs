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
using System.Windows.Data;
using System.Windows.Markup;

namespace wpf_player
{
    /// <summary>
    /// Converter Class that converts a default value to a given value.
    /// </summary>
    public class DefaultValueConverter: MarkupExtension,IValueConverter
    {
        /// <summary>
        /// Generic default value
        /// </summary>
        public object DefaultValue
        {
            get;
            set;
        }
        /// <summary>
        /// Value to be shown
        /// </summary>
        public object ShownValue
        {
            get;
            set;
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultValueConverter()
        { }       
        #region IValueConverter
        /// <summary>
        /// Converts default value to the value to be shown
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString().Equals(DefaultValue.ToString()))
            {
                return ShownValue;
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// Converts back the shown value to the default value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString().Equals(ShownValue.ToString()))
            {
                return DefaultValue;
            }
            else
            {
                return value;
            }
        }

        #endregion
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
