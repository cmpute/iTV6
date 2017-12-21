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
            Uri UriInit = new Uri("http://www.tvmao.com/program/duration/cctv/w"+(int)DateTime.Now.DayOfWeek+"-h"+ DateTime.Now.Hour.ToString() + ".html");
            
            HttpClient client = new HttpClient();
            var response = await client.GetByteArrayAsync(UriInit);
            string result = SuperEncoding.UTF8.GetString(response);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(result);

           

            await Task.CompletedTask;

#else
            await Task.CompletedTask;
#endif
        }
    }
}
