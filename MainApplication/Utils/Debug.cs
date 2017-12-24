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
#else
            await Task.CompletedTask;
#endif
            }
        }
    }
}
