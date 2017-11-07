using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Web.Http;
using Windows.Data.Json;
using iTV6.Services;
using iTV6.Models;

namespace iTV6.Models.Stations
{
    public class TsinghuaTV : ITelevisionStation
    {
        public string IdentifierName => "清华";

        private bool _fetched = false; //判断是否获取过节目列表
        private List<Tuple<Channel, string /* Vid */>> _vidList;
        private Dictionary<Channel, List<Program>> _programList;
        public async Task<IEnumerable<PlayingProgram>> GetChannelList(bool force = false)
        {
            // TODO: 继续分析https://iptv.tsinghua.edu.cn/status.txt，里面有在线观看人数
            try
            {
                HttpClient client = new HttpClient();
                // 频道列表json
                var buffer = await client.GetBufferAsync(new Uri("https://iptv.tsinghua.edu.cn/channels.json"));
                var channelstr = Encoding.UTF8.GetString(buffer.ToArray(), 0, (int)buffer.Length);
                // 节目单的json
                buffer = await client.GetBufferAsync(new Uri("https://iptv.tsinghua.edu.cn/epg/todayepg.json"));
                var epgstr = Encoding.UTF8.GetString(buffer.ToArray(), 0, (int)buffer.Length);

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
                        Channel ch = category == "广播" ? Channel.GetChannel(name, ChannelType.Radio) : Channel.GetChannel(name);
                        _vidList.Add(new Tuple<Channel, string>(ch, vid));
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
                var result = new List<PlayingProgram>();
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
                        result.Add(new PlayingProgram()
                        {
                            ThumbImage = new Uri($"http://iptv.tsinghua.edu.cn/snapshot//{vid}.jpg"),
                            MediaSource = new Uri($"https://iptv.tsinghua.edu.cn/hls/{vid}.m3u8"),
                            SourceStation = this,
                            ProgramInfo = current
                        });
                    }
                }
                _fetched = true;
                return result;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message, "Error");
                System.Diagnostics.Debugger.Break();
                return new List<PlayingProgram>();
            }
        }

        public async Task<IEnumerable<Program>> GetProgramList(Channel channel, bool force = false)
        {
            if (!_fetched)
                await GetChannelList(force);
            return _programList[channel];
        }
    }
}
