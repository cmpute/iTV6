using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace iTV6.Converters
{
    /// <summary>
    /// 直接传递对象，这个可以起到类型的直接转换效果
    /// </summary>
    public class PassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => value;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => value;
    }
}
