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
using System.Windows;

namespace wpf_player
{
    /// <summary>
    /// Converter class used to convert position information to margin setup to move the position indication in the GUI
    /// </summary>
    class SizeToMarginConverter: MarkupExtension,IMultiValueConverter
    {
        /// <summary>
        /// Left Margin at the base position
        /// </summary>
        public int LeftMargin
        {
            get;
            set;
        }
        /// <summary>
        /// Bottom Margin at the base position
        /// </summary>
        public int BottomMargin
        {
            get;
            set;
        }
        /// <summary>
        /// Top margin at the base position
        /// </summary>
        public int TopMargin
        {
            get;
            set;
        }
        /// <summary>
        /// Right margin at the base position
        /// </summary>
        public int RightMargin
        {
            get;
            set;
        }
        /// <summary>
        /// Default constructor for this class. Sets all margins to zero.
        /// </summary>
        public SizeToMarginConverter()
        {
            LeftMargin = 0;
            BottomMargin = 0;
            TopMargin = 0;
            RightMargin = 0;
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

        #region IValueConverter

        /// <summary>
        /// Converts the buffer position information to margin information
        /// </summary>
        /// <param name="values">Three elements array. The first is the buffer portion, the second is the bar size
        /// and the third is the file size</param>
        /// <param name="targetType">Unused param</param>
        /// <param name="parameter">Unused Param</param>
        /// <param name="culture">Unused Param</param>
        /// <returns>Tickness information that will be used to setup margins of the position indicator</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            KeyValuePair<long,long> kv = (KeyValuePair<long,long>)values[0];
            double barSize = (double)values[1];
            long fileSize = (long)values[2];
            if (fileSize == 0) fileSize = 1;           
            long lm = (long)((((double)kv.Key) / fileSize) * barSize);
            long rm = (long)(barSize - ((double)(kv.Value) / fileSize) * barSize);
            Thickness tk = new Thickness(LeftMargin+lm, TopMargin, RightMargin+rm, BottomMargin);
            return tk;
        }
        /// <summary>
        /// Perform inverse conversion. Currently Not supported
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness val = (Thickness)value;            
            return new object[]{0};

        }

        #endregion
    }
}
