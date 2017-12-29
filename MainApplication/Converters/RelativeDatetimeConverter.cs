using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace iTV6.Converters
{
    public class RelativeDatetimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(String))
                throw new NotSupportedException();
            var dt = (DateTime)value;
            var delta = dt - DateTime.Now;

            string message = "";
            var neg = delta.Milliseconds < 0;
            if (neg) delta = delta.Negate();
            if (delta.TotalDays > 1)
                message = $"{(int)(delta.TotalDays)}天{delta.Hours}小时";
            else if (delta.Hours >= 1)
                message = $"{delta.Hours}小时{delta.Minutes}分";
            else if (delta.Minutes >= 1)
                message = $"{delta.Minutes}分{delta.Seconds}秒";
            else
                message = $"{delta.Seconds}秒";
            if (neg)
                return "已播出" + message;
            else
                return message + "后";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
