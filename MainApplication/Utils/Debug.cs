using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using iTV6.Models;
using iTV6.Services;

namespace iTV6.Utils
{
    public static class Debug
    {
        public static async Task DebugMethod()
        {
#if DEBUG
            /**
            Uri UriBase = new Uri("https://www.tvmao.com/program/duration");
            int Dat = (int)System.DateTime.Now.DayOfWeek;
            if(Dat == 0)
            {
                Dat = 7;
            }
            int[] time = { 0, 2, 4, 6,8,10,12,14,16,18,20,22 };
            string[] code = { "cctv" };
            Dictionary<string, List<Models.Program>> Schedule = new Dictionary<string, List<Models.Program>>();
            foreach (int i in time)
            {
                Uri UriInit = new Uri("http://www.tvmao.com/program/duration/"+code[0]+"/w" + Dat + "-h" + i + ".html");

                HttpClient client = new HttpClient();
                var response = await client.GetByteArrayAsync(UriInit);
                string result = SuperEncoding.UTF8.GetString(response);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(result);
                

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
                    if (!Schedule.ContainsKey(chn.UniqueId))
                    {
                        Schedule.Add(chn.UniqueId, pro);
                    }

                    foreach (HtmlNode pg in proCollect)
                    {
                        HtmlNode prog = HtmlNode.CreateNode(pg.OuterHtml);
                        HtmlNode progInfo = prog.SelectSingleNode("/td[1]");
                        string progName = progInfo.FirstChild.InnerText.Replace("&nbsp;", "");
                        string startTime = progInfo.LastChild.InnerText.Replace("&nbsp;", "");

                        if (string.Equals(startTime, ".."))
                        {
                            continue;
                        }
                        else
                        {
                            Models.Program pgm = new Models.Program();
                            DateTime starttime = Convert.ToDateTime(startTime);
                            pgm.Name = progName;
                            pgm.StartTime = starttime;
                            Schedule[chn.UniqueId].Add(pgm);
                        }
                    }
                }
                Schedule.Count();
            }
            **/
            Services.ScheduleService.GetSchedule();
            await Task.CompletedTask;

#else
            await Task.CompletedTask;
#endif
        }
    }
}
