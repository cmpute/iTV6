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
        public async void Download(string channel, string source, Uri RequestUri, StorageFolder storageFolder, DateTime startTime, DateTime endTime)
        {
            //当前时间
            DateTime currentTime = System.DateTime.Now;
            //起始时间与当前时间的间隔
            TimeSpan intervalTime = startTime - currentTime;
            bool startTimeOK = (intervalTime.TotalMilliseconds < 5000);
            //等到系统时间到达startTime，才开始录制工作
            while (!startTimeOK)
            {
                await Task.Delay(5000);
                currentTime = System.DateTime.Now;
                intervalTime = startTime - currentTime;
                startTimeOK = (intervalTime.TotalMilliseconds < 5000);   //认为差距时间在5s内，则已到达预定时间，开始录制
            }
            //录制时间的长度
            TimeSpan durationTime = endTime - startTime;
            //需要改进
            int m3u8Time;  
            //源是清华的话，每个m3u8文件记录的长度为1分钟，单位：min
            if(source == "清华")
            {
                m3u8Time = 1;
            }
            else//其他源需要协商
            {
                m3u8Time = 1;
            }
            int recordNum = (int)(durationTime.TotalMinutes) / m3u8Time;
            //创建目录，创建文件流
            StorageFile sampleFile = null;
            String storageName = channel + "_" + source + "_" + 
                              startTime.Year.ToString() + "_" + startTime.Month.ToString() + "_" + startTime.Day.ToString() + "_" + startTime.Hour.ToString() + "_" + startTime.Minute.ToString();  //文件名
            sampleFile = await storageFolder.CreateFileAsync(storageName + ".ts", CreationCollisionOption.OpenIfExists);
            var stream = await sampleFile.OpenAsync(FileAccessMode.ReadWrite);//得到文件流
            //每m3u8Time个时间，获取一次url，得到的ts文件写入文件流中
            for (int j=0;j<recordNum;j++)
            {
                //StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                // await storageFolder.CreateFileAsync("sample.txt", CreationCollisionOption.ReplaceExisting);
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
                //sampleFile = await storageFolder.CreateFileAsync("sample.ts", CreationCollisionOption.ReplaceExisting);
                
                foreach (object i in tsContent)
                {
                    tempTs = i.ToString();
                    if (tempTs.StartsWith("http"))//m3u8中的可能就是完整地址
                        tempTsUri = new Uri(tempTs);
                    else//其他情况需要加上前面一段
                        tempTsUri = new Uri(uri.Substring(0, hlsPos + ("hls/").Length) + tempTs, UriKind.RelativeOrAbsolute);
                    http = new Windows.Web.Http.HttpClient();

                    buffer = await http.GetBufferAsync(tempTsUri);
                 
                    using (var outputStream = stream.GetOutputStreamAt(stream.Size))//获得输出文件流，并定位到流的末端
                    {
                        using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                        {
                            dataWriter.WriteBuffer(buffer);//将buffer写入文件流
                            await dataWriter.StoreAsync();
                            await outputStream.FlushAsync();
                        }
                    }
                }
                
                await Task.Delay(m3u8Time*60*1000);
                
            }
            stream.Dispose();//关闭文件流


            //StorageFile sampleFile = await storageFolder.GetFileAsync("sample.txt");

        }

        //返回.m3u8文件中所有.ts文件的名字
        //目前根据\n的位置，已经能得到完整有效的.ts文件名
        public ArrayList TSposition(string text)
        {
            ArrayList newlist = new ArrayList();
            int i = 0;
            int tempPos = 0;
            while (i < text.Length)
            {
                int ts_length = 0;
                tempPos = text.IndexOf(".ts", i);
                if (tempPos == -1)
                    break;
                ts_length = tempPos - text.LastIndexOf("\n", tempPos) - 1 + 3;//从当前.ts的位置向前找“\n”，从而得到.ts文件名的长度
                newlist.Add(text.Substring(tempPos - ts_length + 3, ts_length));
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
