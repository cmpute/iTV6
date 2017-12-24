using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;

namespace iTV6.Services
{
    public class ScheduleService
    {
        private ScheduleService()
        {
            SetScheduleForSingleDistrict("cctv");
        }
        private static Dictionary<string, List<Models.Program>> Schedule;
        private static Dictionary<string, string> DistrictToCode;
        private static int[] timestamp = { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22 };

        public static Uri GetUri(string code, int time, int DoW = -1)
        {
            if(DoW == -1)
            {
                DoW = (int)System.DateTime.Now.DayOfWeek;
                if(DoW == 0)
                {
                    DoW = 7;
                }
            }
            Uri UriTemp = new Uri("http://www.tvmao.com/program/duration/" + code + "/w" + DoW + "-h" + time + ".html");
            return UriTemp; 
        }

        public static void SetScheduleForSingleDistrict(string code,int Dow = -1)
        {
            foreach(int time in timestamp)
            {
                Uri UriTemp = GetUri(code, time, Dow);
                SetScheduleFromSinglePage(UriTemp);
            }
        }

        public static async void SetScheduleFromSinglePage(Uri href)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(href);
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
                Models.Channel chn = null;
                chn = Models.Channel.GetChannel(uniqueKey, chName, Models.Channel.GetChannelTypeByName(chName));
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
        }


        
        public async void SetDistrictToCode()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync("http://www.tvmao.com/program/duration/cctv/w7-h16.html");
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            HtmlNode rootNode = document.DocumentNode;
            DistrictToCode.Add("央视", "cctv");
            DistrictToCode.Add("卫视", "satellite");
            DistrictToCode.Add("数字付费", "digital");
            DistrictToCode.Add("香港", "honkong");
            DistrictToCode.Add("澳门", "macau");
            DistrictToCode.Add("台湾", "taiwan");
            DistrictToCode.Add("境外", "foreign");

            HtmlNodeCollection NodeList = rootNode.SelectNodes("/body[1]/div[@class = 'pgnav_wrap']/div[@class='lev2 clear']/form[@class='lt ml10']/select[@name='prov']");
            NodeList = NodeList[0].ChildNodes;
            int id = 0;
            string disName = null;
            string disCode = null;
            foreach (HtmlNode nd in NodeList)
            {
                HtmlNode node = HtmlNode.CreateNode(nd.OuterHtml);

                if (node.Attributes.Contains("value"))
                {
                    disCode = node.Attributes["value"].Value;
                    continue;
                }
                disName = node.InnerText;
                if (disName == null || disCode == null || disName == "" || disCode == "" || disCode == "0")
                {
                    continue;
                }
                DistrictToCode.Add(disName, disCode);
                id += 1;
                DistrictToCode.Count();
            }

        }
         
    }
}
    
