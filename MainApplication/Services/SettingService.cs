using System.Collections.Generic;
using Windows.Storage;

namespace iTV6.Services
{
    class SettingService
    {

        public List<string> MediaSources { get; } = new List<string>();
        public List<string> ThemeList = new List<string>();

        private SettingService()
        {
            MediaSources.Add("清华"); MediaSources.Add("中国农大"); MediaSources.Add("东北大学"); MediaSources.Add("北邮人");
            ThemeList.Add("浅色"); ThemeList.Add("深色"); ThemeList.Add("蓝色");
        }
        private static SettingService _instance;
        /// <summary>
        /// 获取设置服务实例，实例为单例
        /// </summary>
        public static SettingService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingService();
                return _instance;
            }
        }
        static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            if (container.Values[key] != null)
            {
                return container.Values[key];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 设置指定键的值
        /// </summary>
        /// <param name="key">键名称</param>
        /// <param name="value">值</param>
        public static void SetValue(string key, object value)
        {
            container.Values[key] = value;
        }
        /// <summary>
        /// 指示应用容器内是否存在某键
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            if (container.Values[key] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
