// #define DEBUG_SCHEDULE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using iTV6.Models;
using ScheduleList = System.Collections.Generic.Dictionary<
    iTV6.Models.Channel, System.Collections.Generic.List<iTV6.Models.Program>>;

namespace iTV6.Services
{
    public class ScheduleService
    {
        private ScheduleService() { }

        public static ScheduleService Instance { get; } = new ScheduleService();

        /// <summary>
        /// 计算节目单页面的网址
        /// </summary>
        /// <param name="code">区域代码</param>
        /// <param name="time">小时</param>
        /// <param name="DoW">星期几, -1代表今天</param>
        /// <returns></returns>
        private static Uri GetUri(string code, int time, int DoW = -1)
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

        /// <summary>
        /// 获取指定区域的当天节目单
        /// </summary>
        /// <param name="districtCode">区域代码</param>
        /// <param name="Dow">第几天</param>
        public async Task<ScheduleList> GetScheduleToday(string districtCode, int Dow = -1)
        {
            List<Task<ScheduleList>> tasks = new List<Task<ScheduleList>>();
            for (int time = 0; time < 24; time += 2)
            {
                Uri UriTemp = GetUri(districtCode, time, Dow);
                tasks.Add(GetScheduleFromPage(UriTemp));
            }
            var results = await Task.WhenAll(tasks);
            var resultList = new ScheduleList();
            foreach (var schedule in results)
            {
                foreach (var item in schedule)
                {
                    if (!resultList.ContainsKey(item.Key))
                        resultList.Add(item.Key, new List<Models.Program>());
                    resultList[item.Key].AddRange(item.Value);
                }
            }
            return resultList;
        }

        /// <summary>
        /// 获取指定区域、时间的节目单
        /// </summary>
        public async Task<ScheduleList> GetSchedule(string districtCode, int hour, int DoW = -1)
            => await GetScheduleFromPage(GetUri(districtCode, hour, DoW));

        /// <summary>
        /// 根据电视猫页面链接获取节目单
        /// </summary>
        /// <param name="href">节目单链接</param>
        private async Task<ScheduleList> GetScheduleFromPage(Uri href)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(href);
            string result = Encoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            var schedules = new ScheduleList();

            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection channelNode = rootNode.SelectNodes("/body[1]/div[@class='page-content clear']/div[@class='timeline clear']/table[@class='timetable']/table[@class='timetable']");
            foreach (HtmlNode channel in channelNode)
            {
                HtmlNode chNode = channel.SelectSingleNode("./tr[1]/td[@class='tdchn']");
                HtmlNodeCollection proCollect = channel.SelectNodes("./tr[1]/td[@class='tdpro']");
                string chName = chNode.InnerText;
                string chLink = chNode.FirstChild.GetAttributeValue("href", "--");

                var chLinkSIdx = chLink.IndexOf('-') + 1;
                var key = chLink.Substring(chLinkSIdx, chLink.LastIndexOf('-') - chLinkSIdx);
                var uniqueKey = key.ToLower();
                Channel chn = Channel.GetChannel(key, chName);
                chn.LogoID = key;

                if (schedules.ContainsKey(chn))
                {
#if DEBUG && DEBUG_SCHEDULE
                    System.Diagnostics.Debugger.Break(); // 存在重复频道，需要调试
#endif
                    continue;
                }
                List<Models.Program> proList = new List<Models.Program>();
                schedules.Add(chn, proList);

                foreach (HtmlNode prog in proCollect)
                {
                    string progName = WebUtility.HtmlDecode(prog.FirstChild.InnerText).Trim();
                    string startTime = WebUtility.HtmlDecode(prog.LastChild.InnerText).Trim();

                    var judgeLast = startTime.Trim().Distinct();
                    if (judgeLast.Count() == 1 && judgeLast.First() == '.')
                    {
                        if (progName == startTime)
                        {
                            // 只有省略号的情况
                            var texts = prog.GetAttributeValue("title", string.Empty).Split(new char[] { ' ' }, 2);
                            proList.Add(new Models.Program()
                            {
                                Name = texts[1],
                                StartTime = Convert.ToDateTime(texts[0])
                            });
                        }
                        else
                        {
                            var timestr = prog.GetAttributeValue("title", string.Empty);
                            DateTime time;
                            if (!DateTime.TryParse(timestr, out time))
                                time = Convert.ToDateTime(timestr.Split(new char[] { ' ' }, 2)[0]);

                            // 节目时间为省略号的情况
                            proList.Add(new Models.Program()
                            {
                                Name = progName,
                                StartTime = time
                            });
                        }
                    }
                    else
                    {
                        // 正常情况
                        proList.Add(new Models.Program()
                        {
                            Name = progName,
                            StartTime = Convert.ToDateTime(startTime)
                        });
                    }
                }
            }

            return schedules;
        }
        
        /// <summary>
        /// 获取频道区域与代码关系表
        /// </summary>
        public async Task<Dictionary<string,string>> GetDistrictCodeMap()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync("http://www.tvmao.com/program");
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            var codes = new Dictionary<string, string>();
            HtmlNode rootNode = document.DocumentNode;
            codes.Add("央视", "cctv");
            codes.Add("卫视", "satellite");
            codes.Add("数字付费", "digital");
            codes.Add("香港", "honkong");
            codes.Add("澳门", "macau");
            codes.Add("台湾", "taiwan");
            codes.Add("境外", "foreign");

            HtmlNodeCollection NodeList = rootNode.SelectNodes("/body/div[@class='pgnav_wrap']/div[@class='lev2 clear']/form[@class='lt ml10']/select[@name='prov']");
            string disName = null;
            string disCode = null;
            foreach (HtmlNode node in NodeList[0].ChildNodes)
            {
                if (node.Attributes.Contains("value"))
                {
                    disCode = node.Attributes["value"].Value;
                    continue;
                }
                disName = node.InnerText;
                if (string.IsNullOrWhiteSpace(disName) || string.IsNullOrWhiteSpace(disCode) || disCode == "0")
                {
                    continue;
                }
                codes.Add(disName, disCode);
            }

            return codes;
        }
    }
}
    
