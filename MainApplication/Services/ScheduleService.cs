using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;

namespace iTV6.Services
{
    public class ScheduleService
    {
        private ScheduleService() { }
        private Dictionary<string, string> _districtCodes = new Dictionary<string, string>();
        private Dictionary<string, List<Models.Program>> _schedule = new Dictionary<string, List<Models.Program>>();
        public IReadOnlyDictionary<string, List<Models.Program>> Schedule => _schedule;
        public IReadOnlyDictionary<string, string> DistrictToCode => _districtCodes;

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
        /// <param name="code">区域代码</param>
        /// <param name="Dow">第几天</param>
        public async Task SetScheduleForDistrict(string code, int Dow = -1)
        {
            List<Task> tasks = new List<Task>();
            for (int time = 0; time < 24; time += 2)
            {
                Uri UriTemp = GetUri(code, time, Dow);
                await SetScheduleFromPage(UriTemp);
                //tasks.Add(SetScheduleFromPage(UriTemp));
            }
            // await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 根据电视猫页面链接获取节目单
        /// </summary>
        /// <param name="href">节目单链接</param>
        private async Task SetScheduleFromPage(Uri href)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(href);
            string result = Encoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection channelNode = rootNode.SelectNodes("/body[1]/div[@class='page-content clear']/div[@class='timeline clear']/table[@class='timetable']/table[@class='timetable']");
            foreach (HtmlNode channel in channelNode)
            {
                HtmlNode chNode = channel.SelectSingleNode("./tr[1]/td[@class='tdchn']");
                HtmlNodeCollection proCollect = channel.SelectNodes("./tr[1]/td[@class='tdpro']");
                string chName = chNode.InnerText;
                string uniqueKey = SuperEncoding.GetSpellCode(chName);
                Models.Channel chn = Models.Channel.GetChannel(uniqueKey, chName);
                List<Models.Program> pro = new List<Models.Program>();
                if (!_schedule.ContainsKey(chn.UniqueId))
                    _schedule.Add(chn.UniqueId, pro);

                foreach (HtmlNode prog in proCollect)
                {
                    HtmlNode progInfo = prog.SelectSingleNode("./td[1]");
                    string progName = WebUtility.HtmlDecode(prog.FirstChild.InnerText).Trim();
                    string startTime = WebUtility.HtmlDecode(prog.LastChild.InnerText).Trim();

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
                        _schedule[chn.UniqueId].Add(pgm);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取频道区域与代码关系表
        /// </summary>
        public async Task SetDistrictToCode()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync("http://www.tvmao.com/program");
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            HtmlNode rootNode = document.DocumentNode;
            _districtCodes.Add("央视", "cctv");
            _districtCodes.Add("卫视", "satellite");
            _districtCodes.Add("数字付费", "digital");
            _districtCodes.Add("香港", "honkong");
            _districtCodes.Add("澳门", "macau");
            _districtCodes.Add("台湾", "taiwan");
            _districtCodes.Add("境外", "foreign");

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
                _districtCodes.Add(disName, disCode);
            }
        }
    }
}
    
