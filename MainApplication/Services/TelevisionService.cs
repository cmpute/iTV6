using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTV6.Models;
using iTV6.Models.Stations;
using iTV6.Utils;

namespace iTV6.Services
{
    public class TelevisionService : // 实际上没有必要继承，这里只是为了方便IntelliSence
        IScheduleStation, IPlaybackStation 
    {
        private TelevisionService()
        {
            TelevisionStations = new List<ITelevisionStation>(GetTelevisionStations());
        }

        private static TelevisionService _instance;
        /// <summary>
        /// 获取电视服务实例，实例为单例
        /// </summary>
        public static TelevisionService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TelevisionService();
                return _instance;
            }
        }
        
        private IEnumerable<ITelevisionStation> GetTelevisionStations()
        {
            // 硬编码列表
            yield return new THU();
            yield return new NEU();
            yield return new AHAU();
        }
        /// <summary>
        /// 获取视频资源来源的列表
        /// </summary>
        public List<ITelevisionStation> TelevisionStations { get; }

        /// <summary>
        /// 当前节目列表
        /// </summary>
        public IEnumerable<MultisourceProgram> AvaliablePrograms
        {
            get
            {
                return Async.InvokeAndWait(async () => {
                    var tasks = TelevisionStations.Select(station => station.GetChannelList()).ToArray();
                    var taskvals = await Task.WhenAll(tasks);

                    var mapping = new Dictionary<Channel, MultisourceProgram>();
                    foreach(var programList in taskvals)
                        foreach(var program in programList)
                        {
                            Channel ch = program.ProgramInfo.Channel;
                            if (!mapping.ContainsKey(ch))
                                mapping.Add(ch, new MultisourceProgram());
                            mapping[ch].AddSource(program);
                        }
                    return mapping.Values;
                });
            }
        }

        /// <summary>
        /// 提供统一的获取频道节目单接口
        /// </summary>
        /// <param name="channel">需要获取的频道</param>
        /// <param name="force">强制从网络获取</param>
        /// <returns>节目单</returns>
        public Task<IEnumerable<Models.Program>> GetSchedule(Channel channel, bool force = false) =>
            (TelevisionStations.First(station => station is IScheduleStation) as IScheduleStation).GetSchedule(channel, force);

        /// <summary>
        /// 提供统一的获取回放地址的接口
        /// </summary>
        /// <param name="channel">需要获取的频道</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>回放视频的地址</returns>
        public Task<Uri> GetPlaybackSource(Channel channel, DateTimeOffset start, DateTimeOffset end) =>
            (TelevisionStations.First(station => station is IPlaybackStation) as IPlaybackStation).GetPlaybackSource(channel, start, end);
    }
}
