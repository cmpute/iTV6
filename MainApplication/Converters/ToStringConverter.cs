using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace iTV6.Converters
{
    public class ToStringConverter : IValueConverter
    {
        public string FormatString { get; set; }
        public string Default { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(String))
                throw new NotSupportedException();
            // 由于目前情况比较少，就暂时不用反射来实现了
            var str = string.Empty;
            if (value != null)
            {
                if (value is DateTime)
                    str = ((DateTime)value).ToString(FormatString);
                else if (value is float)
                    str = ((float)value).ToString(FormatString);
                else
                    str = value.ToString();
            }
            return string.IsNullOrEmpty(str) ? Default : str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
