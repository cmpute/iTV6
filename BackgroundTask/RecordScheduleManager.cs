using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace iTV6.Background
{
    /// <summary>
    /// 用来管理录播计划，前台与后台交互将主要通过这个类来实现
    /// </summary>
    public static class RecordScheduleManager
    {
        /// <summary>
        /// 保存录播计划的设置位置
        /// </summary>
        const string containerKey = "Schedule";
        public static ApplicationDataContainer Container => 
            ApplicationData.Current.LocalSettings.CreateContainer(
                containerKey, ApplicationDataCreateDisposition.Always);

        /// <summary>
        /// 获取目前所有的录播计划
        /// </summary>
        public static IEnumerable<RecordSchedule> GetSchedules()
            => Container.Containers.Values.Select((container) => new RecordSchedule(container.Name));

        /// <summary>
        /// 删除录播计划记录
        /// </summary>
        /// <param name="identifiers">删除的录播计划的标识符</param>
        public static void ClearSchedules(params string[] identifiers)
        {
            foreach(var id in identifiers)
            {
                var key = EncodeIndentifier(id);
                Container.DeleteContainer(key);
                Container.Values.Remove(key);
            }
        }

        /// <summary>
        /// 删除所有的录播记下
        /// </summary>
        public static void ClearSchedules()
            => ClearSchedules(Container.Values.Keys.ToArray());

        /// <summary>
        /// 创建新的录播计划
        /// </summary>
        /// <param name="identifier">标识名，每一个下载任务不应相同</param>
        /// <param name="playlistUri">直播文件的地址</param>
        /// <param name="startTime">开始录播的时间</param>
        /// <param name="recordSpan">录播时长</param>
        public static RecordSchedule CreateSchedule(
            string identifier, string playlistUri, DateTimeOffset startTime, TimeSpan recordSpan)
        {
            var key = EncodeIndentifier(identifier);
            var schedule = new RecordSchedule(key)
            {
                PlaylistUri = playlistUri,
                StartTime = startTime,
                ScheduleSpan = recordSpan,
                Status = ScheduleStatus.Scheduled
            };
            RecordScheduleMessager.Instance.Trigger(key, RecordScheduleMessageType.Created);
            return schedule;
        }

        /// <summary>
        /// 将标识符重新编码一方面便于压缩表示，另一方面避免非法字符
        /// </summary>
        /// <param name="identifier">标识符</param>
        /// <returns>压缩后的ID，长度为6位</returns>
        private static string EncodeIndentifier(string identifier)
        {
            // 某个hash算法
            UInt64 hash = 3074457345618258791ul;
            foreach (var ch in identifier)
            {
                hash += ch;
                hash *= 3074457345618258799ul;
            }
            return Convert.ToBase64String(BitConverter.GetBytes(hash)).TrimEnd('=');
        }

        /// <summary>
        /// 立即开始录播任务
        /// </summary>
        public static async void LaunchRecording(RecordSchedule schedule)
        {
            // 获取视频片段长度
            HttpClient client = new HttpClient();
            var m3u = await client.GetStringAsync(schedule.PlaylistUri);
            client.Dispose();
            const string tag = "#EXT-X-TARGETDURATION";
            var tag_index = m3u.IndexOf(tag) + tag.Length + 1;
            string interval = m3u.Substring(tag_index, m3u.IndexOf('\n', tag_index) - tag_index);
            schedule.SegmentLength = TimeSpan.FromSeconds(double.Parse(interval));

            // 创建下载任务
            await schedule.GenerateDownloadTask();
            schedule.Status = ScheduleStatus.Downloading;
        }

        /// <summary>
        /// 开始录播计划任务，定时开始下载
        /// </summary>
        /// <param name="schedule">任务对象</param>
        public static async void LaunchSchedule(RecordSchedule schedule)
        {
            // 获取后台运行权限，这个感觉不是必要的？
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.Denied)
            {
                // TODO: 权限被拒绝，需要提示用户
                return;
            }

            var key = schedule.Key;

            Guid taskId;
            var builder = ScheduleTask.CreateBackgroundTaskBuilder(
                out taskId, schedule.StartTime.Subtract(DateTimeOffset.Now));
            Container.Values[key] = taskId;
        }
       
        /// <summary>
        /// 终止下载计划
        /// </summary>
        public static void TerminateSchedule(RecordSchedule schedule)
            => schedule.Status = ScheduleStatus.Terminated;

        /// <summary>
        /// 删除下载计划的记录
        /// </summary>
        /// <param name="schedule">下载计划对象</param>
        public static void DeleteSchedule(RecordSchedule schedule)
        {
            Container.DeleteContainer(schedule.Key);
            Container.Values.Remove(schedule.Key);
            RecordScheduleMessager.Instance.Trigger(schedule.Key, RecordScheduleMessageType.Deleted);
        }
    }
}
