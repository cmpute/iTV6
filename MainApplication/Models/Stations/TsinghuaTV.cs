using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models.Stations
{
    public class TsinghuaTV : ITelevisionStation
    {
        public async Task<IEnumerable<Channel>> GetChannelList()
        {
            // TODO: 实现频道列表抓取
            return await Task.Run(() => new List<Channel>());
        }

        public async Task<IEnumerable<Program>> GetProgramList(Channel channel)
        {
            // TODO: 实现节目列表抓取
            return await Task.Run(() => new List<Program>());
        }
    }
}
