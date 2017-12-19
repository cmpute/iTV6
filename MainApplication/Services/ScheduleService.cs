using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
/**
namespace iTV6.Services
{
    public class ScheduleService
    {
        private ScheduleService() { }
        private static Task<string> uritxt;
        
        private List<Tuple<Models.Channel, Uri>> ChannelHref { get; set; }
        private static Uri UriBase = new Uri("https://www.tvmao.com/program");
        private static Uri UriInit = new Uri("http://www.tvmao.com/program/duration");
        //private static List<Tuple<Models.Channel, List<Program>>> AllSchedule { get; set; }

        private static void GetUriTxt()
        {
            uritxt = ReadFromPage(UriInit);
        }

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
        public async static Task<string> ReadFromPage(Uri uri)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(uri);
            string result = SuperEncoding.UTF8.GetString(response);
            return result;
        }
        
        public static void ParseURL(string uritxt,Uri UriInit)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(uritxt);

            //parse channelHref
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
        }

        public static void ParseSingleChannelSchedule(Uri url)
        {
            Task<string> webtxt = ReadFromPage(url);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(webtxt);
            List<Models.Program> ChannelSchedule = new List<Models.Program>();
            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection channelNodeList = rootNode.SelectNodes("body/div[@class='page-content clear']/div[@class='pgmain']/div[@class='epg mt30 mb10 clear']/ul[@id='pgrow']");

            HtmlNode temp = null;
            
        }

        public static void ParseMultiChannelSchedule(string webtxt,List<Tuple<Models.Channel,Uri>> ChannelHref)
        {


        }
    }
}
    **/
