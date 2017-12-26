using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace iTV6.Services
{
    public class NotificationService
    {
        private static NotificationService _instance;
        /// <summary>
        /// 获取通知和磁贴服务实例，实例为单例
        /// </summary>
        public static NotificationService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NotificationService();
                return _instance;
            }
        }

        /// <summary>
        /// 由节目生成通知
        /// </summary>
        /// <param name="program"></param>
        public static void NewToast(Models.Program program)
        {
            string name = program.Name;
            string program_name = program.Channel.Name;
            string toastVisual = $@"
<visual>
  <binding template='ToastGeneric'>
    <text>{name}</text>
    <text>节目名称：{program_name}</text>
</binding>
</visual>";

            string toastXmlString =
$@"<toast>
    {toastVisual}
</toast>";
            XmlDocument toastXml = new XmlDocument();
            toastXml.LoadXml(toastXmlString);
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

        }
    }
}
