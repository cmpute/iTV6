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
            
            Uri UriBase = new Uri("https://www.tvmao.com/program");
            Uri UriInit = new Uri("http://www.tvmao.com/program/duration");
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(UriInit);
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

            /**
            //parse channelHref
            List<Tuple<Models.Channel, Uri>> ChannelHref = new List<Tuple<Models.Channel, Uri>>();
            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection channelNodeList = rootNode.SelectNodes("body/div[@class='page-content clear']/div[@class='chlsnav']/ul[@class='r']/li");
            HtmlNode temp = null;
            string channelName = null;
            Uri uriChannel = null;
            foreach(HtmlNode channelNode in channelNodeList)
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
                Channel ch = null;
                ch = Channel.GetChannel(uniqueKey, channelName, ChannelType.Central);
                ChannelHref.Add(new Tuple<Models.Channel,Uri>(ch,uriChannel));
            }
            **/

            //parse programList for one single program
            HtmlNode rootNode = document.DocumentNode;
            HtmlNodeCollection programNodeList = rootNode.SelectNodes("body/div[@class='page-content clear']/div[@class='pgmain']/div[@class='epg mt30 mb10 clear']/ul[@id='pgrow']");
            HtmlNode pNode = programNodeList[0];
            programNodeList = pNode.ChildNodes;
            List<string> SingleChannelSchedule = new List<string>();
            HtmlNode temp = null;
            foreach(HtmlNode programNode in programNodeList)
            {
                temp = HtmlNode.CreateNode(programNode.OuterHtml);
                string ProgramInfo = temp.InnerText;
                SingleChannelSchedule.Add(ProgramInfo);
            }

            await Task.CompletedTask;

#else
            await Task.CompletedTask;
#endif
        }
    }
}
