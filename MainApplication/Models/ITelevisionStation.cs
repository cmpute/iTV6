using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 各IPTV源需要提供的功能接口。
    /// </summary>
    /// <remarks>
    /// <see cref="ITelevisionStation"/>的派生类都应该遵循单例模式，请通过TelevisionService进行访问。
    /// </remarks>
    public interface ITelevisionStation
    {
        /// <summary>
        /// 来源的标识名称
        /// </summary>
        string IdentifierName { get; }

        /// <summary>
        /// 异步获取频道列表以及正在播放的节目信息
        /// </summary>
        /// <param name="force">是否强制刷新缓存</param>
        Task<IEnumerable<PlayingProgram>> GetChannelList(bool force = false);

        /// <summary>
        /// 异步获取节目单
        /// </summary>
        /// <param name="channel">需要获取节目单的频道</param>
        /// <param name="force">是否强制刷新缓存</param>
        Task<IEnumerable<Program>> GetSchedule(Channel channel, bool force = false);
    }
}
