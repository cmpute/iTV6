using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTV6.Models;
using iTV6.Models.Stations;

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
            yield return new TsinghuaTV();
        }
        /// <summary>
        /// 获取视频资源来源的列表
        /// </summary>
        public List<ITelevisionStation> TelevisionStations { get; }

        /// <summary>
        /// 当前节目列表，键为频道名
        /// </summary>
        public Dictionary<string, PlayingProgram> AvaliablePrograms { get; } = new Dictionary<string, PlayingProgram>();
    }
}
