using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using iTV6.Models;

namespace iTV6.Utils
{
    public static class Debug
    {
        public static async Task DebugMethod()
        {
#if DEBUG
            
            Uri UriBase = new Uri("https://www.tvmao.com/program/duration");
            Uri UriInit = new Uri("http://www.tvmao.com/program/duration/cctv/w"+(int)DateTime.Now.DayOfWeek+"-h"+ 0 + ".html");
            
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(UriInit);
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            Dictionary<Models.Channel, List<Models.Program>> Schedule = new Dictionary<Models.Channel, List<Models.Program>>();
            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection channelNode = rootNode.SelectNodes("/body[1]/div[@class='page-content clear']/div[@class='timeline clear']/table[@class='timetable']/table[@class='timetable']");
            HtmlNode channel = null;
            foreach (HtmlNode ch in channelNode)
            {
                channel = HtmlNode.CreateNode(ch.OuterHtml);
                HtmlNode chNode = channel.SelectSingleNode("tr[1]/td[@class='tdchn']");
                HtmlNodeCollection proCollect = channel.SelectNodes("tr[1]/td[@class='tdpro']");
                string chName = chNode.InnerText;
                string uniqueKey = SuperEncoding.GetSpellCode(chName);
                Channel chn = null;
                chn = Channel.GetChannel(uniqueKey, chName, Channel.GetChannelTypeByName(chName));
                List<Models.Program> pro = new List<Models.Program>();
                foreach(HtmlNode pg in proCollect)
                {
                    HtmlNode prog = HtmlNode.CreateNode(pg.OuterHtml);
                    DateTime starttime =Convert.ToDateTime(prog.Attributes["title"].Value);
                    HtmlNode progInfo = prog.SelectSingleNode("/td[1]/div[1]");
                    string progName = progInfo.InnerText;
                    Models.Program pgm = new Models.Program();
                    pgm.Name = progName;
                    pgm.StartTime = starttime;
                    pro.Add(pgm);
                }
            }

            await Task.CompletedTask;

#else
            await Task.CompletedTask;
#endif
        }
    }
}
