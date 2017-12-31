using System;
using System.Collections.Generic;
using SDebug = System.Diagnostics.Debug;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Level = Windows.Foundation.Diagnostics.LoggingLevel;

namespace iTV6.Services
{
    /// <summary>
    /// 提供日志记录相关服务，便于选择消息记录的实际实现方式
    /// </summary>
    public static class LoggingService
    {
        private static string[] _groups;

        static LoggingService()
        {
            MuteGroups(); // 取消指定组别消息的输出
        }

        private static void MuteGroups()
        {
            // XXX: 在这里注释掉不需要输出Debug消息的内容，如果有新增的分组也在这里添加
            _groups = new string[]{
                //"Television",
                "Models",
                "Xaml",
                "Normal",
                "Service",
                "Background",
                string.Empty // 方便注释
            };
        }

        public static void Debug(string group, string message, Level level = Level.Verbose,
            [CallerMemberName]string caller = null, [CallerFilePath]string path = null, [CallerLineNumber]int lineno = 0)
        {
            if (!_groups.Contains(group)) return;

            if((int)level >= 3)
                SDebug.WriteLine($"[{path}: {lineno}({caller})]<{group}> {message}", "Error");
            else SDebug.WriteLine($"[{path}: {lineno}({caller})]<{group}> {message}");
        }
    }
}
