using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 代表电视节目资源的Model
    /// </summary>
    public class ProgramSource
    {
        /// <summary>
        /// 预览图
        /// </summary>
        public Uri ThumbImage { get; set; }

        /// <summary>
        /// 是否有可用的预览图
        /// </summary>
        public bool IsThumbAvaliable { get; set; }
        
        /// <summary>
        /// 媒体资源地址
        /// </summary>
        public Uri MediaSource { get; set; }

        /// <summary>
        /// 当有多个视频源选择时给的标签
        /// </summary>
        public string MediaSourceTag { get; set; } = string.Empty;

        /// <summary>
        /// 是否有可用的媒体资源
        /// </summary>
        public bool IsMediaAvaliable { get; set; }

        /// <summary>
        /// 节目信息
        /// </summary>
        public Program ProgramInfo { get; set; }

        /// <summary>
        /// 节目来源的电视台
        /// </summary>
        public ITelevisionStation SourceStation { get; set; }
    }
}
