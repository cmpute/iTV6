using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace iTV6.Services
{
    /// <summary>
    /// 新建后台任务
    /// </summary>
    public class BackgroundService
    {
        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(
            Type taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Unspecified || status == BackgroundAccessStatus.Denied)
                return null;

            // 取消已有任务
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
                if (cur.Value.Name == taskName)
                    cur.Value.Unregister(true);

            // 建立任务对象
            var builder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint.FullName
            };

            builder.SetTrigger(trigger);

            if (condition != null)
                builder.AddCondition(condition);
            
            BackgroundTaskRegistration task = builder.Register();

            Debug.WriteLine($"成功注册 {taskName} 后台任务");

            return task;
        }
    }
}
