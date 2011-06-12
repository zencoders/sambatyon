using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using System.Net;

namespace wpf_player
{
    class PortValidationRule: ValidationRule
    {
        private bool checkInUse=true;
        public PortValidationRule()
        {            
        }
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
