using iTV6.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.ViewModels
{
    public class ConnectionStatusViewModel : ViewModelBase
    {
        public override void OnNavigatedTo(object paramter)
        {
            Message = paramter.ToString();
        }

        private string _Message = "请检查您的网络连接";
        /// <summary>
        /// 提醒消息
        /// </summary>
        public string Message
        {
            get { return _Message; }
            set { Set(ref _Message, value); }
        }
    }
}
