using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace iTV6.Background
{
    /// <summary>
    /// 提供前后台的消息传递服务
    /// </summary>
    public class RecordScheduleMessager
    {
        const string queueKey = "RecordChangedMessage";
        public static RecordScheduleMessager Instance { get; } = new RecordScheduleMessager();
        private RecordScheduleMessager()
        {
#if DEBUG
            ApplicationData.Current.LocalSettings.Values.Remove(queueKey);
#endif
            ApplicationData.Current.DataChanged += DataChanged;
        }
        ~RecordScheduleMessager()
        {
            ApplicationData.Current.DataChanged -= DataChanged;
        }

        private void DataChanged(ApplicationData sender, object args)
        {
            object existing = string.Empty;
            ApplicationData.Current.LocalSettings.Values.TryGetValue(queueKey, out existing);
            var messages = existing as string;
            if (string.IsNullOrEmpty(messages))
                return; // 非主动触发
            else
            {
                string content;
                var split = messages.IndexOf(';');
                if (split < 0)
                {
                    content = messages;
                    ApplicationData.Current.LocalSettings.Values[queueKey] = string.Empty;
                }
                else
                {
                    content = messages.Substring(0, split);
                    ApplicationData.Current.LocalSettings.Values[queueKey] = messages.Substring(split + 1);
                }
                
                var message = (RecordScheduleMessageType)Enum.ToObject(typeof(RecordScheduleMessageType), int.Parse(content.Substring(0, 1)));
                var key = content.Substring(1);
                Changed?.Invoke(key, message);
            }
        }
        /// <summary>
        /// 录播任务变动事件
        /// </summary>
        public event RecordScheduleChangedHandler Changed;
        /// <summary>
        /// 触发录播任务变动事件
        /// </summary>
        /// <param name="scheduleKey">发生变动的录播任务</param>
        /// <param name="message">对应的消息类型</param>
        public void Trigger(string scheduleKey, RecordScheduleMessageType message)
        {
            var content = ((int)message).ToString() + scheduleKey;
            object existing = string.Empty;
            ApplicationData.Current.LocalSettings.Values.TryGetValue(queueKey, out existing);
            if (string.IsNullOrEmpty(existing as string))
                ApplicationData.Current.LocalSettings.Values[queueKey] = content;
            else
                ApplicationData.Current.LocalSettings.Values[queueKey] += ';' + content;

            // 触发事件
            ApplicationData.Current.SignalDataChanged();
        }
    }

    /// <summary>
    /// 录播任务变动事件的处理函数
    /// </summary>
    /// <param name="scheduleKey">发生变动的录播任务</param>
    /// <param name="message">对应的消息类型</param>
    public delegate void RecordScheduleChangedHandler(string scheduleKey, RecordScheduleMessageType message);

    /// <summary>
    /// 录播任务变动的消息类型
    /// </summary>
    public enum RecordScheduleMessageType
    {
        Created,
        Deleted,
        StatusChanged,
        DownloadedOne
    }
}
