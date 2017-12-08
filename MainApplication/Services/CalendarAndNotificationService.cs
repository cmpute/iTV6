using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Appointments;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace iTV6.Services
{
    class CalendarAndNotificationService
    {
        /// <summary>
        /// 由节目生成日历通知
        /// </summary>
        /// <param name="program"></param>
        public async void createAppoint(Models.Program program)
        {
            var ap_store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);
            var ap_c = await ap_store.CreateAppointmentCalendarAsync("节目提醒");
            var appointment = initialAppoint(program);
            await ap_c.SaveAppointmentAsync(appointment);
        }
        /// <summary>
        /// 由节目生成约会
        /// </summary>
        /// <param name="program"></param>
        /// <returns></returns>
        public Appointment initialAppoint(Models.Program program) {
            var ap = new Windows.ApplicationModel.Appointments.Appointment();
            ap.Subject = program.Name;
            ap.StartTime = program.StartTime;
            ap.Duration = new TimeSpan(1, 0, 0, 0);
            ap.AllDay = true;
            ap.Details = program.Episode;
            return ap;
        }
        /// <summary>
        /// 删除所有约会
        /// </summary>
        /// <param name="apc"></param>
        /// <returns></returns>
        public static async Task deleteAllAppointments(AppointmentCalendar apc)
        {
            var aps = await apc.FindAppointmentsAsync(DateTime.Now.AddYears(-10), TimeSpan.FromDays(365 * 20));
            foreach (var a in aps)
            {
                await apc.DeleteAppointmentAsync(a.LocalId);
            }
        }
        /// <summary>
        /// 由节目生成通知
        /// </summary>
        /// <param name="program"></param>
        public static void new_toast(Models.Program program)
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
            Windows.UI.Notifications.ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

        }
    }
}
