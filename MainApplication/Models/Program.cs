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
        /// 用来表示节目的唯一标识
        /// </summary>
        public string UniqueId => Name + '\x0B' + Episode;

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

        /// <summary>
        /// 所属频道
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// 合并两个节目信息，并返回信息量更充足的信息
        /// </summary>
        public static Program operator | (Program p1, Program p2)
        {
            if (string.IsNullOrWhiteSpace(p1.Name))
                return p2;
            if (string.IsNullOrWhiteSpace(p2.Name))
                return p1;
            return p1;
        }
    }
}
