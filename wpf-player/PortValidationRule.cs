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
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Net;

namespace wpf_player
{
    /// <summary>
    /// Validator used in peer configurator. This check if port is in uses or not and if it is a valid port or not.
    /// </summary>
    class PortValidationRule: ValidationRule
    {
        private bool checkInUse=true;
        /// <summary>
        /// Default constructor
        /// </summary>
        public PortValidationRule()
        {            
        }
        /// <summary>
        /// Flag that indicates if we want to check if the port is in use
        /// </summary>
        public bool CheckInUse
        {
            get
            {
                return checkInUse;

            }
            set
            {
                checkInUse = value;
            }
        }
        /// <summary>
        /// Validates the port value. It checks if the port number is valid (it is in the range [1024,65535]) and if it's
        /// already in use (if this check is enabled).
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="cultureInfo">Unused Param</param>
        /// <returns>A validation Result that indicates if the validation has been succeded or not</returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int port = 0; 
            if (int.TryParse((string)value,out port))
            {               
                if (port < 1024)
                {
                    return new ValidationResult(false, "Port number must be greater than 1023");
                }
                else if (port > 65536)
                {
                    return new ValidationResult(false, "Port number must be less than 65536");
                }
                else if (checkInUse)
                {
                    IPGlobalProperties ipProp = IPGlobalProperties.GetIPGlobalProperties();
                    IPEndPoint[] udps = ipProp.GetActiveUdpListeners();                
                    foreach (IPEndPoint ep in udps)
                    {
                        if (ep.Port == port)
                        { 
                            return new ValidationResult(false, "Port is in use"); 
                        }
                    }
                    return new ValidationResult(true, "Valid port number");
                } else 
                {
                    return new ValidationResult(true, "Valid port number");   
                }                
            }
            else
            {
                return new ValidationResult(false, "Port number must be an integer.");
            }
        }
    }
}
