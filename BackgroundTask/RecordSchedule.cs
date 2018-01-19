using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace iTV6.Background
{
    /// <summary>
    /// 在设置中保存的录播计划
    /// </summary>
    public sealed class RecordSchedule
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
            set
            {
                _container.Values[nameof(Status)] = (byte)value;
                RecordScheduleMessager.Instance.Trigger(Key, RecordScheduleMessageType.StatusChanged);
            }
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
        /// 保存最终文件的路径
        /// </summary>
        public string SavePath
        {
            get { return _container.Values[nameof(SavePath)] as string; }
            set { _container.Values[nameof(SavePath)] = value; }
        }

        /// <summary>
        /// 生成的唯一标识符
        /// </summary>
        public string Key => _key;

        /// <summary>
        /// 开始下载下一个需要下载的文件
        /// </summary>
        /// <returns>后台下载任务</returns>
        internal async Task<DownloadOperation> GenerateDownloadTask()
        {
            try
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
                        var file = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(path);
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
            catch (Exception e)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("生成下载任务时出错：" + e.Message);
                System.Diagnostics.Debugger.Break();
#endif
                return null;
            }
        }

        /// <summary>
        /// 拼接下载的视频片段
        /// </summary>
        /// <returns>该异步任务返回文件是否连续</returns>
        internal async Task<bool> ConcatenateSegments(string filePath)
        {
            System.Diagnostics.Debug.WriteLine("开始连接缓存");
            var continuous = true;
            var folder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync(_key);
            var files = await folder.GetFilesAsync();
            var targetfolder = await StorageFolder.GetFolderFromPathAsync(filePath.Substring(0, filePath.LastIndexOf('\\')));
            var target = await targetfolder.CreateFileAsync(filePath.Substring(filePath.LastIndexOf('\\') + 1), CreationCollisionOption.ReplaceExisting);
            using (var ws = await target.OpenAsync(FileAccessMode.ReadWrite))
            {
                int code = -1;
                foreach (var file in files.OrderBy(file => file.Name))
                {
                    var currentcode = int.Parse(string.Concat(file.Name.Where(ch => Char.IsDigit(ch))));
                    if (code >= 0 && currentcode - code != 1)
                        continuous = false;
                    code = currentcode;
                        
                    await ws.WriteAsync(await FileIO.ReadBufferAsync(file));
                    await file.DeleteAsync();
                }
                await ws.FlushAsync();
            }

            Status = ScheduleStatus.Completed;
            return continuous;
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
