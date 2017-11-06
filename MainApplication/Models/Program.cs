using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 电视节目Model
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 节目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 播放时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 节目时长
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 节目集数
        /// </summary>
        public string Episode { get; set; }
    }
}
