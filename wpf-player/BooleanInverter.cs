using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;

namespace wpf_player
{
    class BooleanInverter : MarkupExtension, IValueConverter
    {
        #region IValueConverter Membri di

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((value is bool) && (targetType == typeof(bool)))
            {
                bool v = (bool)value;
                return !v;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
