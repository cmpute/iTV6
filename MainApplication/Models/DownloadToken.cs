using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using iTV6.Mvvm;
using iTV6.Background;
using MsgPack.Serialization;

namespace iTV6.Models
{
    /// <summary>
    /// 保存后台任务对应的详细信息
    /// </summary>
    public class DownloadToken : BindableBase
    {
        /// <summary>
        /// 录播频道
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// 视频来源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTimeOffset EndTime => LinkedSchedule.StartTime.Add(LinkedSchedule.ScheduleSpan);

        /// <summary>
        /// 与本对象相关的计划信息
        /// </summary>
        [MessagePackIgnore] // 由Service手动关联
        public RecordSchedule LinkedSchedule { get; set; }
    }
}
