using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;

namespace iTV6.Background
{
    /// <summary>
    /// 定时录播任务的定时触发器
    /// </summary>
    public sealed class ScheduleTask : IBackgroundTask
    {
        // TODO: 后台定时器有15分钟的误差，因此若需准确开始需要提前15分钟
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // 寻找对应的下载计划
            RecordSchedule schedule = null;
            foreach (var item in RecordScheduleManager.Container.Values)
                if ((Guid)item.Value == taskInstance.Task.TaskId)
                    schedule = new RecordSchedule(item.Key);

            if (schedule == null)
            {
                System.Diagnostics.Debug.WriteLine("未找到对应的下载计划", "Error");
                return;
            }
            if (schedule.Status == ScheduleStatus.Terminated)
            {
                System.Diagnostics.Debug.WriteLine("下载计划被终止");
                return;
            }

            var _defer = taskInstance.GetDeferral();
            // 生成下一个下载，若下载完毕则直接处理文件
            var operation = await schedule.GenerateDownloadTask();
            if (operation == null)
            {
                // TODO: 什么情况下会发生？
                System.Diagnostics.Debugger.Break();
            }
            _defer.Complete();
        }

        /// <summary>
        /// 生成绑定了后台任务的定时出发器
        /// </summary>
        internal static BackgroundTaskBuilder CreateBackgroundTaskBuilder(out Guid taskId, TimeSpan interval)
        {
            interval = interval.Add(TimeSpan.FromSeconds(1)); //避免刚好设置成15分钟时无法触发的情况
            var timeInt = (uint)interval.TotalMinutes;
            if (timeInt < 15)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#else
                throw new InvalidOperationException("不支持15分钟以内的录播计划，请直接保持程序打开在前台");
#endif
            }

            var builder = new BackgroundTaskBuilder();
            builder.TaskEntryPoint = typeof(ScheduleTask).FullName;
            builder.SetTrigger(new TimeTrigger(timeInt, true));
            var taskReg = builder.Register();
            taskId = taskReg.TaskId;
            // TODO: 用builder.AddCondition(condition)增加运行条件，如果有网才继续运行

            return builder;
        }
    }
}
