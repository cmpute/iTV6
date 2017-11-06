using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 各IPTV源需要提供的功能接口
    /// </summary>
    public interface ITelevisionStation
    {
        /// <summary>
        /// 异步获取频道列表
        /// </summary>
        Task<IEnumerable<Channel>> GetChannelList();

        /// <summary>
        /// 异步获取节目单
        /// </summary>
        /// <param name="channel">需要获取节目单的频道</param>
        Task<IEnumerable<Program>> GetProgramList(Channel channel);
    }
}
