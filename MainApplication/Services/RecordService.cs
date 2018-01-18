using iTV6.Models;
using iTV6.Background;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Windows.System.Threading;
using System.Collections.ObjectModel;

namespace iTV6.Services
{
    /// <summary>
    /// 录播任务的前台管理服务
    /// </summary>
    public class RecordService
    {
        private RecordService()
        {
            // 如果没有恢复TaskList话则手动恢复
            if (_TaskList.Count == 0)
                Utils.Async.InvokeAndWait(async () => await _TaskList.Restore());
                
            // 同步前后台的列表
            foreach(var schedule in RecordScheduleManager.GetSchedules())
            {
                var key = schedule.Key;
                if (_TaskList.ContainsKey(key))
                    _TaskList[key].LinkedSchedule = schedule;
                else
                {
                    var token = new DownloadToken()
                    {
                        Channel = Channel.GetChannel("unknown", "未知频道"),
                        Source = "未知来源",
                        LinkedSchedule = schedule
                    };
                    _TaskList.Add(schedule.Key, token);
                }
            }
            foreach(var item in _TaskList.ToList()) // 利用ToList方法创建副本
            {
                if (item.Value.LinkedSchedule == null)
                    _TaskList.Remove(item.Key);
            }

            // 初始化前台列表并添加事件监听
            TaskList = new ObservableCollection<DownloadToken>(_TaskList.Values);
        }

        private static RecordService _instance;
        /// <summary>
        /// 获取录播服务实例，实例为单例
        /// </summary>
        public static RecordService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecordService();
                return _instance;
            }
        }
        
        /// <summary>
        /// 下载任务列表
        /// </summary>
        /// <remarks>
        /// 由于需要先一步读取缓存，因此设为静态变量，在其他时候访问请通过实例属性
        /// </remarks>
        internal static CachedDictionary<string, DownloadToken> _TaskList { get; } =
            new CachedDictionary<string, DownloadToken>("tokens.dat");

        /// <summary>
        /// 录播任务列表
        /// </summary>
        public ObservableCollection<DownloadToken> TaskList;

        /// <summary>
        /// 在前台创建录播任务
        /// </summary>
        public DownloadToken StartRecording(Channel channel, SourceRecord source, DateTimeOffset startTime, TimeSpan span)
        {
            // 注册相关信息
            string identifer = channel.UniqueId + source.StationName + startTime.ToString();
            var schedule = RecordScheduleManager.CreateSchedule(identifer, source.Source.AbsoluteUri, startTime, span);
            var token = new DownloadToken()
            {
                Channel = channel,
                Source = source.StationName,
                LinkedSchedule = schedule
            };
            _TaskList.Add(schedule.Key, token);
            TaskList.Add(token);

            // 开始任务
            var diff = startTime.Subtract(DateTimeOffset.Now);
            if (diff > TimeSpan.FromMinutes(15))
            {
                RecordScheduleManager.LaunchSchedule(schedule);
                new MessageDialog("由于系统原因，后台定时任务的精度为15分钟，为保证录下您指定的时间段，将提前15分钟开始录制，请预留足够的硬盘空间", "提示").ShowAsync();
            }
            else
            {
                if (diff <= TimeSpan.Zero)
                    RecordScheduleManager.LaunchRecording(schedule);
                else
                {
                    // TODO: 由于ThreadPoolTimer会受到程序进入后台的影响，因此可以考虑在这里改用后台任务强行等待
                    new MessageDialog("由于系统不支持15分钟内的后台定时任务，请您在录制开始前将本程序保持开启并避免最小化！开始后即可关闭程序", "提示").ShowAsync();
                    ThreadPoolTimer.CreateTimer((timer) => RecordScheduleManager.LaunchRecording(schedule), diff);
                }
            }

            return token;
        }

        /// <summary>
        /// 终止后台录播任务
        /// </summary>
        public void TerminateRecording(DownloadToken token) =>
            RecordScheduleManager.TerminateSchedule(token.LinkedSchedule);

        /// <summary>
        /// 删除录播任务
        /// </summary>
        public void DeleteRecording(DownloadToken token)
        {
            var key = token.LinkedSchedule.Key;
            RecordScheduleManager.DeleteSchedule(token.LinkedSchedule);
            _TaskList.Remove(key);
            TaskList.Remove(token);
        }

        /// <summary>
        /// 获取存储文件夹
        /// </summary>
        //public async static Task<StorageFolder> GetMyFolderAsync()
        //{
        //    StorageFolder folder = null;
        //    string path = SettingService.Instance["FilePath"].ToString();
        //    StorageFolder defaultfolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("iTV6_Download", CreationCollisionOption.OpenIfExists);
        //    if (!string.IsNullOrEmpty(path))
        //    {
        //        try
        //        {
        //            folder = await StorageFolder.GetFolderFromPathAsync(path);
        //        }
        //        catch
        //        {
        //            folder = defaultfolder;
        //        }
        //    }
        //    else
        //    {
        //        folder = defaultfolder;
        //    }
        //    return folder;
        //}
    }
}
