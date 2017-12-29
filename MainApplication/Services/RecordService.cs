using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTV6.Models;
using iTV6.Models.Stations;
using iTV6.Utils;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Collections;
using System.Net;
using System.Collections.ObjectModel;

namespace iTV6.Services
{
    public class RecordService
    {
        public RecordService()
        {
             TaskList = new ObservableCollection<DownloadTask>();
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
        public ObservableCollection<DownloadTask> TaskList { get; }
        /// <summary>
        /// 创建下载任务
        /// </summary>
        public async Task<bool> CreateDownload(string channel, string source, Uri requestUri, DateTime startTime, DateTime endTime)
        {
            var folder = await GetMyFolderAsync();
            TaskList.Add(new DownloadTask(channel, source, requestUri, folder, startTime, endTime));

            return true;
        }
        /// <summary>
        /// 获取存储文件夹
        /// </summary>
        public async static Task<StorageFolder> GetMyFolderAsync()
        {
            StorageFolder folder = null;
            string path = SettingService.Instance["FilePath"].ToString();
            StorageFolder defaultfolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("iTV6_Download", CreationCollisionOption.OpenIfExists);
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    folder = await StorageFolder.GetFolderFromPathAsync(path);
                }
                catch
                {
                    folder = defaultfolder;
                }
            }
            else
            {
                folder = defaultfolder;
            }
            return folder;
        }
    }
}
