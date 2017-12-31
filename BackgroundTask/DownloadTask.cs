using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace iTV6.Background
{
    /// <summary>
    /// 生成下载任务的后台任务
    /// </summary>
    public sealed class DownloadTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var details = taskInstance.TriggerDetails as BackgroundTransferCompletionGroupTriggerDetails;
            if (details == null)
            {
                System.Diagnostics.Debug.WriteLine("下载任务非下载完成后触发", "Error");
                return;
            }
            
            var _defer = taskInstance.GetDeferral();
            // 寻找对应的下载计划
            RecordSchedule schedule = null;
            foreach (var item in RecordScheduleManager.Container.Values)
                if ((Guid)item.Value == taskInstance.Task.TaskId)
                    schedule = new RecordSchedule(item.Key);

            if(schedule == null)
            {
                System.Diagnostics.Debug.WriteLine("未找到对应的下载计划", "Error");
                return;
            }

            // 生成下一个下载，若下载完毕则直接处理文件
            var operation = await schedule.GenerateDownloadTask();
            if(operation == null)
            {
                schedule.Status = ScheduleStatus.Decoding;
                await schedule.ConcatenateSegments();
                // TODO: 生成下载成功的消息提醒
            }
            _defer.Complete();
        }


        /// <summary>
        /// 判断下载任务是否失败
        /// </summary>
        /// <param name="download">下载任务</param>
        // TODO: 如果任务失败的话考虑重新开始或者直接将计划任务设置成失败
        private bool IsFailed(DownloadOperation download)
        {
            BackgroundTransferStatus status = download.Progress.Status;
            if (status == BackgroundTransferStatus.Error || status == BackgroundTransferStatus.Canceled)
                return true;

            ResponseInformation response = download.GetResponseInformation();
            if (response.StatusCode != 200)
                return true;

            return false;
        }

        /// <summary>
        /// 生成绑定了后台任务的后台下载器
        /// </summary>
        internal static BackgroundDownloader CreateBackgroundDownloader(out Guid taskId)
        {
            var completionGroup = new BackgroundTransferCompletionGroup();

            var builder = new BackgroundTaskBuilder();
            builder.TaskEntryPoint = typeof(DownloadTask).FullName;
            builder.SetTrigger(completionGroup.Trigger);
            var taskReg = builder.Register();
            taskId = taskReg.TaskId;

            return new BackgroundDownloader(completionGroup);
        }
    }
}
