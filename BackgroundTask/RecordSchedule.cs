using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Background
{
    /// <summary>
    /// 在设置中保存的录播计划
    /// </summary>
    public class RecordSchedule
    {
        private string _playlistUri;
        private string _uriPrefix;
        /// <summary>
        /// 直播文件(.m3u8/.flv）的Uri地址
        /// </summary>
        public string PlaylistUri
        {
            get { return _playlistUri; }
            set
            {
                _playlistUri = value;
                _uriPrefix = _playlistUri.Remove(_playlistUri.LastIndexOf('/'));
            }
        }

        /// <summary>
        /// 开始录制的时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 计划录播的时间
        /// </summary>
        public TimeSpan ScheduledTime { get; set; }

        /// <summary>
        /// 录播状态
        /// </summary>
        public ScheduleStatus Status { get; set; }
    }

    public enum ScheduleStatus
    {
        Scheduled, // 计划中，未开始
        Downloading, // 正在下载中
        Completed, // 下载完毕
        Failed, // 下载失败
        Terminated // 用户终止
    }
}
