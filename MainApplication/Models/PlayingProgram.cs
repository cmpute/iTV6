using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 正在播放的电视节目的Model
    /// </summary>
    public class PlayingProgram : AvailableProgram
    {
        /// <summary>
        /// 预览图
        /// </summary>
        public Uri ThumbImage { get; set; }

        /// <summary>
        /// 是否有可用的预览图
        /// </summary>
        public bool IsThumbAvaliable { get; set; }
    }
}
