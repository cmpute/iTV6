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

namespace iTV6.Services
{
    class RecordService
    {
        public RecordService() { }
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
        public async void download(Uri RequestUri,StorageFolder storageFolder)
        {
            //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = null;// await storageFolder.CreateFileAsync("sample.txt", CreationCollisionOption.ReplaceExisting);
            //获取m3u8文件
            Windows.Web.Http.HttpClient http = new Windows.Web.Http.HttpClient();
            var buffer = await http.GetBufferAsync(RequestUri);
            //处理m3u8文件，缓存至text中
            DataReader TempData = Windows.Storage.Streams.DataReader.FromBuffer(buffer);
            string text = TempData.ReadString(buffer.Length);
            //存储所有当前.ts文件的名字
            ArrayList tsContent = TSposition(text);
            string tempTs;
            string uri = RequestUri.ToString();
            int hlsPos = uri.IndexOf("hls");
            Uri tempTsUri;
            sampleFile = await storageFolder.CreateFileAsync("sample.ts", CreationCollisionOption.ReplaceExisting);
            var stream = await sampleFile.OpenAsync(FileAccessMode.ReadWrite);
            foreach (object i in tsContent)
            {
                tempTs = i.ToString();
                tempTsUri = new Uri(uri.Substring(0, hlsPos + ("hls/").Length) + tempTs, UriKind.RelativeOrAbsolute);
                http = new Windows.Web.Http.HttpClient();

                buffer = await http.GetBufferAsync(tempTsUri);
                using (var outputStream = stream.GetOutputStreamAt(stream.Size))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        dataWriter.WriteBuffer(buffer);
                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }                  
                }
            }
            stream.Dispose();

            //StorageFile sampleFile = await storageFolder.GetFileAsync("sample.txt");

        }

        //返回.m3u8文件中所有.ts文件的名字
        //目前版本中使用-14的操作，这是固定.ts文件的长度确定的，之后可以更改
        public ArrayList TSposition(string text)
        {
            ArrayList newlist = new ArrayList();
            int i = 0;
            int tempPos = 0;
            while (i < text.Length)
            {
                tempPos = text.IndexOf(".ts", i);
                if (tempPos == -1)
                    break;
                newlist.Add(text.Substring(tempPos - 14, 14 + (".ts").Length));
                i = tempPos + 3;
            }
            return newlist;
        }

        /// <summary>
        /// 获取存储文件夹
        /// </summary>
        public async static Task<StorageFolder> GetMyFolderAsync()
        {
            StorageFolder folder = null;
            string path = SettingService.GetValue("FilePath").ToString();
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
