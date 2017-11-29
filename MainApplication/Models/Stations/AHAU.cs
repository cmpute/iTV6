using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace iTV6.Models.Stations
{
    public class AHAU : ITelevisionStation
    {
        public string IdentifierName => "安徽农大";
        public bool IsScheduleAvailable => false;

        public async Task<IEnumerable<PlayingProgram>> GetChannelList(bool force = false)
        {
            try
            {
                HttpClient client = new HttpClient();
                var buffer = await client.GetStreamAsync(new Uri("http://itv.ahau.edu.cn/"));
                HtmlDocument doc = new HtmlDocument();
                doc.Load(buffer, SuperEncoding.GB2312);
                var channelNodes = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div");
                string group = null;
                foreach(var cNode in channelNodes)
                {
                    if (cNode.GetAttributeValue("class", string.Empty) == "col-xs-6 col-sm-3 col-md-3 text-left")
                    {
                        var linkNode = cNode.SelectSingleNode(".//a");
                        var link = linkNode.GetAttributeValue("href", string.Empty);
                        if (link.IndexOf("type=rtmp") >= 0) continue;
                        System.Diagnostics.Debug.WriteLine(link);
                    }
                    else
                        group = cNode.InnerText;
                }
                System.Diagnostics.Debugger.Break();
                return new List<PlayingProgram>();

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message, "Error");
                System.Diagnostics.Debugger.Break();
                return new List<PlayingProgram>();
            }
        }
        public Task<IEnumerable<Program>> GetSchedule(Channel channel, bool force = false)
        {
            throw new NotImplementedException("Schedule Not Avaliable");
        }
    }
}
