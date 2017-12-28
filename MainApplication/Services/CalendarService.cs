using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Storage;

namespace iTV6.Services
{
    // TODO: 在设置界面添加清空提醒的按钮，以及提前多久进行提醒的选项
    // TODO: 另外，提醒应该可以选择是单次提醒还是每周提醒
    public class CalendarService
    {
        const string containerKey = "Appointments";
        private ApplicationDataContainer _container =
            ApplicationData.Current.LocalSettings.CreateContainer(containerKey, ApplicationDataCreateDisposition.Always);

        private AppointmentCalendar _calendar;
        private async void InitializeAppointmentCalendar()
        {
            var store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);
            _calendar = await store.CreateAppointmentCalendarAsync("节目提醒");
            System.Diagnostics.Debug.WriteLine($"成功获得日历对象");
#if DEBUG
            // 在调试状态下每次都清空节目提醒
            await DeleteAllAppointments();
#endif
            // 同步字典与日历内容，处理用户自行将提醒删除的情况
            foreach (var program in _container.Values.Keys)
                if (await _calendar.GetAppointmentAsync(_container.Values[program] as string) == null)
                    _container.Values.Remove(program);
        }
        private CalendarService()
        {
            InitializeAppointmentCalendar();
#if DEBUG
            // 在调试状态下每次都清空节目提醒
            _container.Values.Clear();
#endif
        }
        
        /// <summary>
        /// 获取日历服务实例，实例为单例
        /// </summary>
        public static CalendarService Instance { get; } = new CalendarService();

        /// <summary>
        /// 由节目生成日历提醒
        /// </summary>
        /// <param name="program"></param>
        public async Task<Messages> CreateAppoint(Models.Program program)
        {
            if (_calendar == null)
                return Messages.NotInitialized;
            if (_container.Values.Keys.Contains(program.UniqueId))
                return Messages.AlreadyExists;
            var appointment = new Appointment()
            {
                Subject = program.Name,
                StartTime = program.StartTime,
                Duration = program.Duration,
                AllowNewTimeProposal = false,
                Details = $"第{program.Episode}集",
                Location = program.Channel.Name,
                Reminder = TimeSpan.FromMinutes(10)
            };
            await _calendar.SaveAppointmentAsync(appointment);
            _container.Values.Add(program.UniqueId, appointment.LocalId);
            System.Diagnostics.Debug.WriteLine($"为节目{program}添加日历提醒成功，ID为{appointment.LocalId}");
            return Messages.Sucess;
        }

        /// <summary>
        /// 删除所有日历提醒
        /// </summary>
        /// <returns>是否成功完成操作</returns>
        public async Task<Messages> DeleteAllAppointments()
        {
            if (_calendar == null)
                return Messages.NotInitialized;
            var appointments = await _calendar.FindAppointmentsAsync(DateTime.Now.AddYears(-10), TimeSpan.FromDays(365 * 20));
            foreach (var ap in appointments)
                await _calendar.DeleteAppointmentAsync(ap.LocalId);
            return Messages.Sucess;
        }

        public enum Messages
        {
            Sucess,
            NotInitialized, // 日历对象尚未初始化
            AlreadyExists // 节目提醒已存在
            
        }
    }
}
