using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using iTV6.Services;
using iTV6.Models;

namespace iTV6.Models.Stations
{
    public class THU : TelevisionStationBase, IScheduleStation
    {
        public override string IdentifierName => "清华";

        private List<Tuple<Channel, string /* Vid */>> _vidList;
        private Dictionary<Channel, List<Program>> _programList;
        protected override async Task<IEnumerable<ProgramSource>> GetNewChannelList()
        {
            // TODO: 继续分析https://iptv.tsinghua.edu.cn/status.txt，里面有在线观看人数
            var result = new List<ProgramSource>();
            try
            {
                HttpClient client = new HttpClient();
                // 频道列表json
                var buffer = await client.GetByteArrayAsync("https://iptv.tsinghua.edu.cn/channels.json");
                var channelstr = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                // 节目单的json
                buffer = await client.GetByteArrayAsync("https://iptv.tsinghua.edu.cn/epg/todayepg.json");
                var epgstr = Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Length);

                var timezero = new DateTime(1970, 1, 1, 8, 0, 0); // UTC时间

                // 解析频道列表
                _vidList = new List<Tuple<Channel, string>>();
                var root = JsonObject.Parse(channelstr).GetNamedArray("Categories");
                foreach (var catchild in root)
                {
                    var catobj = catchild.GetObject();
                    var category = catobj.GetNamedString("Name");
                    // XXX: 按照网站的分类来还是按照自己的分类来？
                    var list = catobj.GetNamedArray("Channels");
                    foreach (var channel in list)
                    {
                        var channelobj = channel.GetObject();
                        var name = channelobj.GetNamedString("Name");
                        var vid = channelobj.GetNamedString("Vid");
                        Channel ch = null;
                        if (category == "广播")
                        {
                            name += "广播"; // 电视和广播可能重名。加上广播来避免用额外的ID来表示频道
                            ch = Channel.GetChannel(vid, name, ChannelType.Radio);
                        }
                        else
                            ch = Channel.GetChannel(vid, name);
                        _vidList.Add(new Tuple<Channel, string>(ch, vid));
                        System.Diagnostics.Debug.WriteLine($"[THU|{category}]{name:10} : {vid:10}");
                    }
                }
                
                // 解析节目单
                _programList = new Dictionary<Channel, List<Program>>();
                var epgroot = JsonObject.Parse(epgstr).GetObject();
                foreach (var channel in epgroot)
                {
                    string vid = channel.Key;
                    var match = _vidList.Where(x => x.Item2 == vid);
                    if (match.Count() == 0)
                        continue; // 没有匹配的vid
                    Channel ch = match.First().Item1;
                    var todaylist = channel.Value.GetArray();
                    List<Program> list = new List<Program>();
                    foreach(var period in todaylist)
                    {
                        var programobj = period.GetObject();
                        list.Add(new Program()
                        {
                            // TODO: 分离出名称与集数
                            Name = System.Text.RegularExpressions.Regex.Unescape(programobj.GetNamedString("title")),
                            StartTime = timezero.Add(TimeSpan.FromSeconds(programobj.GetNamedNumber("start"))),
                            Duration = TimeSpan.FromSeconds(programobj.GetNamedNumber("stop") - programobj.GetNamedNumber("start")),
                            Channel = ch
                        });
                    }
                    _programList.Add(ch, list);
                }

                // 解析当前节目
                foreach (var catchild in root)
                {
                    var list = catchild.GetObject().GetNamedArray("Channels");
                    foreach(var channel in list)
                    {
                        var channelobj = channel.GetObject();
                        var vid = channelobj.GetNamedString("Vid");
                        var match = _vidList.Where(x => x.Item2 == vid);
                        if (match.Count() == 0)
                            continue; // 没有匹配的vid
                        Channel ch = match.First().Item1;
                        Program current = null;
                        if (!_programList.ContainsKey(ch))
                            continue; // CGTN高清亲测没有节目单= =
                        foreach (var program in _programList[ch])
                            if(DateTime.Now.Subtract(program.StartTime) < program.Duration)
                            {
                                current = program;
                                break;
                            }
                        result.Add(new ProgramSource()
                        {
                            ThumbImage = new Uri($"http://iptv.tsinghua.edu.cn/snapshot//{vid}.jpg"),
                            IsThumbAvaliable = true,
                            MediaSource = new Uri($"https://iptv.tsinghua.edu.cn/hls/{vid}.m3u8"),
                            IsMediaAvaliable = true,
                            SourceStation = this,
                            ProgramInfo = current
                        });
                    }
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message, "Error");
                System.Diagnostics.Debugger.Break();
            }
            return result;
        }

        public async Task<IEnumerable<Program>> GetSchedule(Channel channel, bool force = false)
        {
            if (!_cached || force)
                await GetChannelList(force);
            if (_programList.ContainsKey(channel))
                return _programList[channel];
            else
                return Enumerable.Empty<Program>();
        }
        
        /// <summary>
        /// 统一频道列表
        /// </summary>
        /// <param name="name">从网站扒取的频道名称</param>
        /// <returns>统一后的名称，用作字典的键</returns>
        public static string GetUniqueChannelName(string name)
        {
            // 硬编码列表，遇到特例时手动加进来
            switch (name)
            {
                case "第一剧场":
                    return "CCTV-第一剧场";
                case "世界地理":
                    return "CCTV-世界地理";
                case "风云剧场":
                    return "CCTV-风云剧场";
                case "风云音乐":
                    return "CCTV-风云音乐";
                case "风云足球":
                    return "CCTV-风云足球";
                case "国防军事":
                    return "CCTV-国防军事";
                case "怀旧剧场":
                    return "CCTV-怀旧剧场";
                default:
                    return name;
            }
        }
    }
}
