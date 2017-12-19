using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack；
using System.Net.Http;

namespace iTV6.Services
{
    public class ScheduleService
    {
        private ScheduleService() { }
        private static string result { get; set; };
        private Uri UriInit { get; set; };
        private static ScheduleService _instance;
        /// <summary>
        /// 获取节目单服务实例，实例为单例
        /// </summary>
        public static ScheduleService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ScheduleService();
                return _instance;
            }
        }
        public async static Task<string> GetSchedule()
        {
            Uri UriBase = new Uri("https://www.tvmao.com/program");
            Uri UriInit = new Uri("http://www.tvmao.com/program/duration");
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(UriInit);
            result = SuperEncoding.UTF8.GetString(response);
            return result;
        }
        public static List<Tuple<Models.Channel, Uri>> GetURL(string result,Uri UriInit)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            //parse channelHref
            List<Tuple<Models.Channel, Uri>> ChannelHref = new List<Tuple<Models.Channel, Uri>>();
            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection channelNodeList = rootNode.SelectNodes("body/div[@class='page-content clear']/div[@class='chlsnav']/ul[@class='r']/li");
            HtmlNode temp = null;
            string channelName = null;
            Uri uriChannel = null;
            foreach (HtmlNode channelNode in channelNodeList)
            {
                temp = HtmlNode.CreateNode(channelNode.OuterHtml);
                channelName = temp.InnerText;
                if (temp.Attributes.Contains("class"))
                {
                    uriChannel = UriInit;
                }
                else
                {
                    temp = HtmlNode.CreateNode(temp.FirstChild.OuterHtml);
                    Uri.TryCreate(UriBase, temp.Attributes["href"].Value, out uriChannel);
                }
                string uniqueKey = SuperEncoding.GetSpellCode(channelName);
                Models.Channel ch = null;
                ch = Models.Channel.GetChannel(uniqueKey, channelName, Models.ChannelType.Central);
                ChannelHref.Add(new Tuple<Models.Channel, Uri>(ch, uriChannel));
            }
            return ChannelHref;
        }
    }
}
