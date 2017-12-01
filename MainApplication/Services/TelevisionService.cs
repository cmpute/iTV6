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
    public class TelevisionService
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
            yield return new AHAU();
        }
        /// <summary>
        /// 获取视频资源来源的列表
        /// </summary>
        public List<ITelevisionStation> TelevisionStations { get; }

        /// <summary>
        /// 当前节目列表
        /// </summary>
        public IEnumerable<PlayingProgram> AvaliablePrograms
        {
            get
            {
                return Async.InvokeAndWait(async () => {
                    var tasks = TelevisionStations.Select(station => station.GetChannelList()).ToArray();
                    var taskrets = await Task.WhenAll(tasks);
                    var result = new List<PlayingProgram>();
                    foreach(var ret in taskrets)
                        result.AddRange(ret);
                    return result;
                });
            }
        }
    }
}
