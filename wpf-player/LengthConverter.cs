using System;
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
