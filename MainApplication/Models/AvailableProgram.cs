using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 有可播放资源的节目的Model
    /// </summary>
    public class AvailableProgram : Program
    {
        /// <summary>
        /// 媒体资源地址
        /// </summary>
        public Uri MediaSource { get; set; }
    }
}
