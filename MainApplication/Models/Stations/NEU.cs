using HtmlAgilityPack;
using iTV6.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Models.Stations
{
    public class NEU : TelevisionStationBase, IScheduleStation, IPlaybackStation
    {
        public override string IdentifierName => "东北大学";

        protected override async Task<IEnumerable<ProgramSource>> GetNewChannelList()
        {
            var result = new List<ProgramSource>();
            try
            {
                HttpClient client = new HttpClient();
                var buffer = await client.GetStreamAsync("http://hdtv.neu6.edu.cn/");
                HtmlDocument doc = new HtmlDocument();
                doc.Load(buffer);

                var channelNodes = doc.DocumentNode.SelectNodes("//div[@class='entry-content']/table/tr/td");
                foreach(var chNode in channelNodes)
                {
                    if (chNode.ChildNodes.Count <= 2)
                        continue;
                    string chName = chNode.FirstChild.InnerText.Trim();
                    string chLink = chNode.ChildNodes["a"].GetAttributeValue("href", string.Empty);
                    string chCode = chLink.Substring(chLink.LastIndexOf("=") + 1);
                    
                    string chCodeUnified = chCode;
                    if (chCode.StartsWith("jlu"))
                        chCodeUnified = chCode.Substring(4);
                    else if (chCode.StartsWith("hls"))
                    {
                        // 计算频道ID
                        string editName = chName.Replace("+","").Replace("-","");
                        if (chName.EndsWith("卫视高清"))
                            editName = editName.Replace("卫视高清", "hd");
                        else
                        {
                            editName = editName.Replace("卫视", "tv");
                            editName = editName.Replace("高清", "hd");
                        }
                        chCodeUnified = SuperEncoding.GetSpellCode(editName, false);
                    }

                    Channel channel = Channel.GetChannel(chCodeUnified, chName);
                    result.Add(new ProgramSource()
                    {
                        IsThumbAvaliable = true,
                        //小的缩略图：http://hdtv.neu6.edu.cn/wall/img/{chCode}_s.png
                        ThumbImage = new Uri($"http://hdtv.neu6.edu.cn/wall/img/{chCode}.png"),
                        MediaSource = new Uri($"http://media2.neu6.edu.cn/hls/{chCode}.m3u8"),
                        MediaSourceTag = chCode,
                        IsMediaAvaliable = true,
                        SourceStation = this,
                        ProgramInfo = new Program()
                        {
                            Name = chName,
                            Channel = channel
                        }
                    });
                    LoggingService.Debug("Television", $"[NEU]{chName:10} : {chCodeUnified:10}");
                }
            }
            catch (Exception e)
            {
                LoggingService.Debug("Television", e.Message, Windows.Foundation.Diagnostics.LoggingLevel.Error);
                System.Diagnostics.Debugger.Break();
            }
            return result;
        }

        public Task<IEnumerable<Program>> GetSchedule(Channel channel, bool force = false)
        {
            throw new NotImplementedException();
        }

        public async Task<Uri> GetPlaybackSource(Channel channel, DateTimeOffset start, DateTimeOffset end)
        {
            await Task.CompletedTask;
            var timezero = new DateTime(1970, 1, 1, 8, 0, 0); // UTC时间
            string startTick = start.ToUnixTimeSeconds().ToString();
            string endTick = end.ToUnixTimeSeconds().ToString();
            string link = $"http://media2.neu6.edu.cn/review/program-{startTick}-{endTick}-{channel.UniqueId}.m3u8";
            LoggingService.Debug("Television", "回看地址：" + link);
            return new Uri(link);
        }

        public override async Task<bool> CheckConnectivity()
        {
            return await Utils.Connection.TestConnectivity("hdtv.neu6.edu.cn");
        }
    }
}
