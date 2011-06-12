using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;

namespace wpf_player
{
    class SizeToMarginConverter: MarkupExtension,IMultiValueConverter
    {
        public int LeftMargin
        {
            get;
            set;
        }
        public int BottomMargin
        {
            get;
            set;
        }
        public int TopMargin
        {
            get;
            set;
        }
        public int RightMargin
        {
            get;
            set;
        }
        public SizeToMarginConverter()
        {
            LeftMargin = 0;
            BottomMargin = 0;
            TopMargin = 0;
            RightMargin = 0;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        #region IValueConverter

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

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness val = (Thickness)value;            
            return new object[]{0};

        }

        #endregion
    }
}
