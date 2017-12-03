using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 为电视源类提供缓存机制的实现
    /// </summary>
    public abstract class TelevisionStationBase : ITelevisionStation
    {
        public abstract string IdentifierName { get; }

        protected bool _cached = false; //判断是否获取过节目列表
        protected IEnumerable<ProgramSource> _cache;
        
        public async Task<IEnumerable<ProgramSource>> GetChannelList(bool force = false)
        {
            if (!_cached || force)
            {
                _cache = await GetNewChannelList();
                _cached = true;
            }
            return _cache;
        }

        /// <summary>
        /// 内部实现从网络获取节目列表的功能
        /// </summary>
        /// <returns></returns>
        protected abstract Task<IEnumerable<ProgramSource>> GetNewChannelList();
        public abstract Task<bool> CheckConnectivity();
    }
}
