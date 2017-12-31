using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace iTV6.Background
{
    /// <summary>
    /// 负责动态更新磁贴的任务
    /// </summary>
    public sealed class LiveTileTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            throw new NotImplementedException();
        }
    }
}
