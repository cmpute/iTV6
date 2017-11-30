using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Utils
{
    public static class Debug
    {
        public static async void DebugMethod()
        {
#if DEBUG
            // 需要执行的函数
            var station = new iTV6.Models.Stations.AHAU();
            var result = await station.GetChannelList();
            System.Diagnostics.Debugger.Break();
            await Task.CompletedTask;
#else
            await Task.CompletedTask;
#endif
        }
    }
}
