using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace iTV6.Background
{
    /// <summary>
    /// 定时录播任务的定时触发器
    /// </summary>
    public sealed class ScheduleTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            throw new NotImplementedException();
        }
    }
}
