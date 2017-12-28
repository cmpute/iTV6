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
        private ScheduleService()
        {
            _districtCodeCache = new CachedDictionary<string, string>(districtCodeFile);
            _scheduleCache = new CachedDictionary<Tuple<string, DayOfWeek, int>, ScheduleList>(scheduleFile);
        }

        /// <summary>
        /// 获取节目单服务实例，实例为单例
        /// </summary>
        public static ScheduleService Instance { get; } = new ScheduleService();

        #region 缓存相关

        const string districtCodeFile = "district_code_map.dat";
        const string scheduleFile = "schedule_map.dat";
        private CachedDictionary<string, string> _districtCodeCache;
        private CachedDictionary<Tuple<string, DayOfWeek, int>, ScheduleList> _scheduleCache;

        /// <summary>
        /// 读取节目单缓存
        /// </summary>
        public async Task RestoreCache()
        {
            await _districtCodeCache.Restore();
            await _scheduleCache.Restore();
        }

        /// <summary>
        /// 保存节目单缓存
        /// </summary>
        public async Task SaveCache()
        {
            // 节目单缓存只在本周有效
            var time = DateTime.Today;
            for (; time.DayOfWeek != DayOfWeek.Monday; time += TimeSpan.FromDays(1)) ;
            await _districtCodeCache.Save(time);
            await _scheduleCache.Save(time);
        }

        /// <summary>
        /// 清除节目单缓存
        /// </summary>
        public async Task ClearCache()
        {
            await _districtCodeCache.ClearCache();
            await _scheduleCache.ClearCache();
        }

        #endregion

        /// <summary>
        /// 计算节目单页面的网址
        /// </summary>
        /// <param name="code">区域代码</param>
        /// <param name="time">小时</param>
        /// <param name="DoW">星期几（数字）</param>
        /// <returns></returns>
        private static Uri GetUri(string code, int time, int DoW)
        {
            Uri UriTemp = new Uri($"http://www.tvmao.com/program/duration/{code}/w{DoW}-h{time}.html");
            System.Diagnostics.Debug.WriteLine("获取节目单: " + UriTemp.AbsoluteUri);
            return UriTemp; 
        }

        /// <summary>
        /// 获取指定区域一天的节目单
        /// </summary>
        /// <param name="districtCode">区域代码</param>
        /// <param name="Dow">星期几</param>
        public async Task<ScheduleList> GetDailySchedule(string districtCode, DayOfWeek? DoW = null)
        {
            // 建立并发任务
            List<Task<ScheduleList>> tasks = new List<Task<ScheduleList>>();
            for (int time = 0; time < 24; time += 2)
                tasks.Add(GetSchedule(districtCode, DoW, time));
            var results = await Task.WhenAll(tasks);

            // 合并获取结果
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

            // 排序并计算时长
            foreach (var programs in resultList.Values)
                ComputeProgramDuration(programs);
            return resultList;
        }

        /// <summary>
        /// 获取指定区域、时间的节目单
        /// </summary>
        public async Task<ScheduleList> GetSchedule(string districtCode, DayOfWeek? DoW = null, int? hour = null)
        {
            if (DoW == null)
                DoW = DateTime.Now.DayOfWeek;
            if (hour == null)
                hour = DateTime.Now.Hour;

            var key = new Tuple<string, DayOfWeek, int>(districtCode, DoW.Value, hour.Value);
            if (_scheduleCache.ContainsKey(key))
                return _scheduleCache[key]; // 有缓存

            int dowint = (int)DoW;
            if (dowint == 0)
                dowint = 7;
            TimeSpan diff = TimeSpan.FromDays((int)DoW - dowint);
            var result = await GetScheduleFromPage(GetUri(districtCode, hour.Value, dowint), diff);
            _scheduleCache.Add(key, result);
            return result;
        }

        /// <summary>
        /// 根据电视猫页面链接获取节目单
        /// </summary>
        /// <param name="href">节目单链接</param>
        /// <param name="daydiff">日期的偏差值</param>
        private async Task<ScheduleList> GetScheduleFromPage(Uri href, TimeSpan daydiff)
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
                    Models.Program program = null;
                    char[] space = { ' ' };

                    var judgeLast = startTime.Trim().Distinct();
                    if (judgeLast.Count() == 1 && judgeLast.First() == '.')
                    {
                        if (progName == startTime)
                        {
                            // 只有省略号的情况
                            var texts = prog.GetAttributeValue("title", string.Empty).Split(space, 2);
                            program = new Models.Program()
                            {
                                Name = texts[1].TrimStart(space),
                                StartTime = Convert.ToDateTime(texts[0])
                            };
                        }
                        else
                        {
                            // 节目时间为省略号的情况
                            var timestr = prog.GetAttributeValue("title", string.Empty);
                            DateTime time;
                            if (!DateTime.TryParse(timestr, out time))
                                // 部分情况下节目名称也会进来。。因此还是取空格前面的
                                time = Convert.ToDateTime(timestr.Split(space, 2)[0]);
                            program = new Models.Program()
                            {
                                Name = progName,
                                StartTime = time
                            };
                        }
                    }
                    else
                    {
                        // 节目名字靠边被省略的情况
                        if (prog.Attributes.Contains("title"))
                            progName = prog.Attributes["title"].DeEntitizeValue.Trim();
                        program = new Models.Program()
                        {
                            Name = progName,
                            StartTime = Convert.ToDateTime(startTime)
                        };
                    }
                    // 处理节目时间跨天的情况
                    if (href.AbsoluteUri.EndsWith("h0.html"))
                        if (program.StartTime > Convert.ToDateTime("02:00"))
                            program.StartTime -= TimeSpan.FromDays(1);
                    program.StartTime -= daydiff;

                    proList.Add(program);
                }
            }

            return schedules;
        }
        
        /// <summary>
        /// 获取频道区域与代码关系表
        /// </summary>
        public async Task<Dictionary<string /*频道名*/, string /*频道编号*/>> GetDistrictCodeMap()
        {
            if (this._districtCodeCache.Count > 0)
                return this._districtCodeCache; // 有缓存

            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync("http://www.tvmao.com/program/duration/100000"); //用异常页面提高加载速度
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            
            HtmlNode rootNode = document.DocumentNode;
            _districtCodeCache.Add("央视", "cctv");
            _districtCodeCache.Add("卫视", "satellite");
            _districtCodeCache.Add("数字付费", "digital");
            _districtCodeCache.Add("香港", "honkong");
            _districtCodeCache.Add("澳门", "macau");
            _districtCodeCache.Add("台湾", "taiwan");
            _districtCodeCache.Add("境外", "foreign");

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
                _districtCodeCache.Add(disName, disCode);
            }

            return _districtCodeCache;
        }

        /// <summary>
        /// 根据列表顺序计算出各节目的时长
        /// </summary>
        /// <param name="programs"></param>
        /// <remarks>TV猫的节目单中不含时长，因此要自行推算</remarks>
        public void ComputeProgramDuration(List<Models.Program> programs)
        {
            // 貌似没有Inplace的去重方法，这里用一个比较快的
            HashSet<Models.Program> set = new HashSet<Models.Program>(programs, Models.Program.TimeComparer);
            programs.Clear();
            programs.AddRange(set);
            programs.Sort(Models.Program.TimeComparer);
            for (int i = 1; i < programs.Count; i++)
                programs[i - 1].Duration = programs[i].StartTime - programs[i - 1].StartTime;
        }
    }
}
    
