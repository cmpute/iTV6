using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;

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
            if (details.Downloads.Any(op => IsFailed(op)))
            {
                // 下载失败
                schedule.Status = ScheduleStatus.Failed;

                // 生成下载失败的消息提醒
                XmlDocument failedToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                failedToastXml.GetElementsByTagName("text").Item(0).InnerText = "一项录播任务失败";
                ToastNotification failedToast = new ToastNotification(failedToastXml);
                ToastNotificationManager.CreateToastNotifier().Show(failedToast);
                return;
            }

            RecordScheduleMessager.Instance.Trigger(schedule.Key, RecordScheduleMessageType.DownloadedOne);
            var _defer = taskInstance.GetDeferral();
            if (schedule.Status == ScheduleStatus.Terminated)
            {
                System.Diagnostics.Debug.WriteLine("下载计划被终止");
                // 将已下载的缓存合并成文件保存下来
                schedule.Status = ScheduleStatus.Decoding;
                var continuous = await schedule.ConcatenateSegments();
                schedule.Status = ScheduleStatus.Terminated;
                return;
            }

            // 生成下一个下载，若下载完毕则直接处理文件
            var operation = await schedule.GenerateDownloadTask();
            if(operation == null)
            {
                schedule.Status = ScheduleStatus.Decoding;
                var continuous = await schedule.ConcatenateSegments();

                // 生成下载成功的消息提醒
                XmlDocument successToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                successToastXml.GetElementsByTagName("text").Item(0).InnerText =
                    "一项录播任务已经成功结束" + (continuous ? string.Empty : "，但可能录播不完整。");
                ToastNotification successToast = new ToastNotification(successToastXml);
                ToastNotificationManager.CreateToastNotifier().Show(successToast);
            }
            _defer.Complete();
        }


        /// <summary>
        /// 判断下载任务是否失败
        /// </summary>
        /// <param name="download">下载任务</param>
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
