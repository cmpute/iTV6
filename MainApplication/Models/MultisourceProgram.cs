using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models
{
    /// <summary>
    /// 多个视频来源的节目
    /// </summary>
    public class MultisourceProgram
    {

        // 重名计数器, 现在没有用到，预计用来自动编号用
        private Dictionary<string, int> counter = new Dictionary<string, int>();

        /// <summary>
        /// 预览图
        /// </summary>
        public Uri ThumbImage { get; set; }

        /// <summary>
        /// 是否有可用的预览图
        /// </summary>
        public bool IsThumbAvaliable { get; set; }

        /// <summary>
        /// 媒体资源地址列表
        /// </summary>
        public List<SourceRecord> MediaSources { get; } = new List<SourceRecord>();
        
        /// <summary>
        /// 节目信息
        /// </summary>
        public Program ProgramInfo { get; set; }

        /// <summary>
        /// 添加节目源
        /// </summary>
        public void AddSource(ProgramSource program)
        {
            if (ProgramInfo == null)
            {
                IsThumbAvaliable = program.IsThumbAvaliable;
                ThumbImage = program.ThumbImage;
                ProgramInfo = program.ProgramInfo;
                MediaSources.Add(new SourceRecord(program.SourceStation.IdentifierName, program.MediaSource));
                counter.Add(program.SourceStation.IdentifierName, 1);
            }
            else
            {
                if (ProgramInfo != null && ProgramInfo.Channel != program.ProgramInfo.Channel)
                    throw new InvalidOperationException("添加的节目不属于同一个频道");
                ProgramInfo |= program.ProgramInfo;
                var idn = program.SourceStation.IdentifierName;
                if (!counter.ContainsKey(idn))
                    counter.Add(idn, 0);
                if(!string.IsNullOrWhiteSpace(program.MediaSourceTag))
                    MediaSources.Add(new SourceRecord($"{idn}-{program.MediaSourceTag}", program.MediaSource));
                else
                    MediaSources.Add(new SourceRecord($"{idn}", program.MediaSource));
                counter[idn]++;

                if(!IsThumbAvaliable && program.IsThumbAvaliable)
                {
                    IsThumbAvaliable = true;
                    ThumbImage = program.ThumbImage;
                }
            }
        }
    }

    public class SourceRecord
    {
        string _name;
        Uri _source;

        public SourceRecord(string stationName, Uri source)
        {
            _name = stationName;
            _source = source;
        }

        /// <summary>
        /// 标识名称。当一个站点有多个同频道的源时需要进行区分
        /// </summary>
        public string StationName => _name;

        /// <summary>
        /// 媒体资源地址
        /// </summary>
        public Uri Source => _source;
    }
}
