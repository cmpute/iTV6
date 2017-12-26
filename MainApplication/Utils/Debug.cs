using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.Utils
{
    public static class Debug
    {
        public static async Task DebugMethod()
        {
#if DEBUG
            // 在这里写或者调用你需要调试的代码
            // TestFunction();
            // Debugger.Break(); //手动打断点
            await Task.CompletedTask;
#else // 以下代码不要改动
            await Task.CompletedTask;
#endif
        }
    }
}
