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

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(String))
                throw new NotSupportedException();
            // 由于目前情况比较少，就暂时不用反射来实现了
            if (value is DateTime)
                return ((DateTime)value).ToString(FormatString);
            else if (value is float)
                return ((float)value).ToString(FormatString);
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
