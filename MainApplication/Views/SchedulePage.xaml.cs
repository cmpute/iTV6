using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace iTV6.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SchedulePage : Page
    {
        public SchedulePage()
        {
            this.InitializeComponent();
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            //所有时间均以北京时间为准，输入为7天以内的时间
            System.DateTime presentTime = DateTime.Now;
            System.DateTime starttime = new System.DateTime(int.Parse(inputyear.Text), int.Parse(inputmonth.Text), 
                     int.Parse(inputdate.Text), int.Parse(inputhour.Text), int.Parse(inputminute.Text), int.Parse(inputsecond.Text));

            //回放的目标频道，从节目列表或者节目单中获取，目前为输入
            //格式与TsinghuaTV.cs中vid一致。例: "cctv5hd"
            string sourcechannel = inputchannel.Text;

            //即为TsinghuaTV.cs中，MediaSource的内容
            //引号内的固定字符串为北邮人回放网站的前后缀，三个变量分别为即时时间、现在时间和频道
            url.Text = "http://media2.neu6.edu.cn/review/" + ConvertDateTimeToInt(starttime).ToString() + "-" + ConvertDateTimeToInt(presentTime).ToString() + "-jlu_" + sourcechannel+".m3u8";
            
        }

        /// <summary>
        /// 时间转时间戳函数，输出为10位的long型整数
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            //时间戳的0点，格林尼治时间1970年1月1日0时。
            System.DateTime zeroTime = new System.DateTime(1970, 1, 1, 8, 00, 00);
            long t = (time.Ticks - zeroTime.Ticks) / 10000000;    
            return t;
        }
    }
}
