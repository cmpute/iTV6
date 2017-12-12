using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            //const string ChannelListXPath = "/html[1]/body[1]/div[6]/div[1]/ul[1]/li";
            const string ChannelNameXPath = "/li";
            Uri UriBase = new Uri("https://www.tvmao.com/program");
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync("http://www.tvmao.com/program/duration");
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);
            //parse channelHref
            List<Tuple<Models.Channel, Uri>> ChannelHref = new List<Tuple<Models.Channel, Uri>>();
            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection channelNodeList = document.DocumentNode.SelectNodes("/html[1]");
            HtmlNode temp = null;
            string channelName = null;
            Uri uriChannel = null;
            Channel ch = null;
            foreach(HtmlNode channelNode in channelNodeList)
            {
                int i = 5;
                temp = HtmlNode.CreateNode(channelNode.OuterHtml);
                channelName = temp.SelectSingleNode(ChannelNameXPath).InnerText;
                Uri.TryCreate(UriBase,temp.SelectSingleNode(ChannelNameXPath).Attributes["href"].Value,out uriChannel);
                SpellCode gs = new SpellCode();
                ch.UniqueId = gs.GetSpellCode(channelName);
                ch.Name = channelName;
                ChannelHref.Add(new Tuple<Models.Channel,Uri>(ch,uriChannel));
            }
            //parse programList
            //Dictionary<Channel,List<Program>> ProgramList = new Dictionary<Channel,List<Program>>();
            await Task.CompletedTask;

#else
            await Task.CompletedTask;
#endif
        }
    }
}
