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
        /// <remarks> TODO: 这个属性是不是应该是Channel的？ </remarks>
        public Uri ThumbImage { get; set; }
    }
}
