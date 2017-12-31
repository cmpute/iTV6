using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace iTV6.Background
{
    /// <summary>
    /// 在设置中保存的录播计划
    /// </summary>
    public class RecordSchedule
    {
        private string _key;
        private ApplicationDataContainer _container; // 每个对象与设置数据相对应
        internal RecordSchedule(string key)
        {
            _key = key;
            _container = RecordScheduleManager.Container.CreateContainer(key, ApplicationDataCreateDisposition.Always);
        }

        /// <summary>
        /// 直播文件(.m3u8/.flv）的Uri地址
        /// </summary>
        public string PlaylistUri
        {
            get { return _container.Values[nameof(PlaylistUri)] as string; }
            set { _container.Values[nameof(PlaylistUri)] = value; }
        }

        /// <summary>
        /// 开始录制的时间
        /// </summary>
        public DateTimeOffset StartTime
        {
            get { return (DateTimeOffset)_container.Values[nameof(StartTime)]; }
            set { _container.Values[nameof(StartTime)] = value; }
        }

        /// <summary>
        /// 计划录播的时间
        /// </summary>
        public TimeSpan ScheduleSpan
        {
            get { return (TimeSpan)_container.Values[nameof(ScheduleSpan)]; }
            set { _container.Values[nameof(ScheduleSpan)] = value; }
        }

        /// <summary>
        /// 录播状态
        /// </summary>
        public ScheduleStatus Status
        {
            get { return (ScheduleStatus)Enum.ToObject(typeof(ScheduleStatus), _container.Values[nameof(Status)]); }
            set { _container.Values[nameof(Status)] = (byte)value; }
        }

        /// <summary>
        /// 每个视频片段的预计长度，对应<code>EXT-X-TARGETDURATION</code>标记
        /// </summary>
        public TimeSpan SegmentLength
        {
            get { return (TimeSpan)_container.Values[nameof(SegmentLength)]; }
            set { _container.Values[nameof(SegmentLength)] = value; }
        }

        /// <summary>
        /// 开始下载下一个需要下载的文件
        /// </summary>
        /// <returns>后台下载任务</returns>
        internal async Task<DownloadOperation> GenerateDownloadTask()
        {
            // 如果超出录播时长则取消下载
            if (DateTimeOffset.Now.Subtract(StartTime) > ScheduleSpan)
            {
                System.Diagnostics.Debug.WriteLine("计划时间到，录播停止");
                return null;
            }

            // 下载最新列表
            HttpClient client = new HttpClient();
            var m3uUri = PlaylistUri;
            var m3u = await client.GetStringAsync(PlaylistUri);
            foreach (var line in m3u.Split('\n'))
                if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                {
                    var path = _key + "\\" + line;
                    var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(path);
                    if (file == null)
                    {
                        Guid taskId;
                        // 获取下载任务
                        var downloader = DownloadTask.CreateBackgroundDownloader(out taskId);
                        StorageFile storage = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);
                        var prefix = m3uUri.Remove(m3uUri.LastIndexOf('/') + 1);
                        var operation = downloader.CreateDownload(new Uri(prefix + line), storage);

                        // 保存任务ID并开始下载
                        RecordScheduleManager.Container.Values[_key] = taskId;
                        var task = operation.StartAsync();
                        downloader.CompletionGroup.Enable();
                        System.Diagnostics.Debug.WriteLine("开始下载文件到" + storage.Path);

                        return operation;
                    }
                }

            await Task.Delay(SegmentLength); // 等待刷新
            return await GenerateDownloadTask(); // 循环获取下一个文件
        }

        /// <summary>
        /// 拼接下载的视频片段
        /// </summary>
        internal async Task ConcatenateSegments()
        {
            System.Diagnostics.Debug.WriteLine("开始连接缓存");
            Status = ScheduleStatus.Completed;
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 标志录播计划的状态
    /// </summary>
    public enum ScheduleStatus
    {
        Scheduled, // 计划中，未开始
        Downloading, // 正在下载中
        Decoding, // 下载完毕，正在转码中
        Completed, // 录播成功结束
        Failed, // 下载失败
        Terminated // 用户终止
    }
}
