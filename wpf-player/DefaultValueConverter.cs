using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace wpf_player
{
    public class DefaultValueConverter: MarkupExtension,IValueConverter
    {
        public object DefaultValue
        {
            get;
            set;
        }
        public object ShownValue
        {
            get;
            set;
        }
        public DefaultValueConverter()
        { }       
        #region IValueConverter

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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
