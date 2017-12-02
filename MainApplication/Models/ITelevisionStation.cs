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
        Task<IEnumerable<ProgramSource>> GetChannelList(bool force = false);
    }

    /// <summary>
    /// 能够提供节目单的电视源
    /// </summary>
    public interface IScheduleStation
    {
        /// <summary>
        /// 异步获取节目单
        /// </summary>
        /// <param name="channel">需要获取节目单的频道</param>
        /// <param name="force">是否强制刷新缓存</param>
        Task<IEnumerable<Program>> GetSchedule(Channel channel, bool force = false);
    }

    /// <summary>
    /// 能够获取回放的电视源
    /// </summary>
    public interface IPlaybackStation
    {
        /// <summary>
        /// 获取某频道从指定时间到指定时间的视频地址
        /// </summary>
        /// <param name="channel">需要获取的频道</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>回放视频的地址</returns>
        Task<Uri> GetPlaybackSource(Channel channel, DateTimeOffset start, DateTimeOffset end);
    }
    // TODO: 其实应该是传入频道代号参数来产生链接的，因为存在多视频源情况。但是这里图方便，直接使用channel本身的ID去生成了
}
