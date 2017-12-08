using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTV6.Services;


namespace iTV6.Utils
{
    public static class Debug
    {
        public static async void DebugMethod()
        {
#if DEBUG
   
            RecordService TT = new RecordService();
            Uri MediaSource = new Uri($"https://iptv.tsinghua.edu.cn/hls/cctv1.m3u8");
            TT.download(MediaSource);




        await Task.CompletedTask;
#else
            await Task.CompletedTask;
#endif
        }
    }
}

