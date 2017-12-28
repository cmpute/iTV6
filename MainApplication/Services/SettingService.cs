using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace iTV6.Services
{
    class SettingService
    {
        public readonly string[] ThemeList = { "浅色", "深色", "蓝色" };

        private SettingService() { }
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

        private IPropertySet localSettings = ApplicationData.Current.LocalSettings.Values;
        private IPropertySet roamingSettings = ApplicationData.Current.RoamingSettings.Values;

        private Dictionary<string, List<INotifyPropertyChanged>> registeredObject = new Dictionary<string, List<INotifyPropertyChanged>>();

        private static void SetProperty(object obj, string propertyName, object value)
            => obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        private static object GetProperty(object obj, string propertyName)
            => obj.GetType().GetProperty(propertyName).GetValue(obj);

        /// <summary>
        /// 注册一个设置选项，并自动绑定属性。注意：属性名即为设置键值，相同设置项需要对应相同的属性名
        /// </summary>
        /// <param name="obj">绑定来源</param>
        /// <param name="propertyName">绑定的属性名称，请使用<code>nameof</code>关键字</param>
        /// <param name="defaultValue">设置的默认值</param>
        /// <param name="roaming">是否设置为漫游同步属性</param>
        public void RegisterSetting(INotifyPropertyChanged obj, string propertyName, object defaultValue = null, bool roaming = false)
        {
            // 设置初始值
            var settings = roaming ? roamingSettings : localSettings;
            if (settings.ContainsKey(propertyName))
                SetProperty(obj, propertyName, settings[propertyName]);
            else
            {
                if (defaultValue != null)
                {
                    SetProperty(obj, propertyName, defaultValue);
                    settings[propertyName] = defaultValue;
                }
            }
            // 添加到注册对象表
            if (!registeredObject.ContainsKey(propertyName))
                registeredObject[propertyName] = new List<INotifyPropertyChanged>();
            registeredObject[propertyName].Add(obj);

            // 注册响应事件
            obj.PropertyChanged += (sender, e) =>
            {
                var newValue = GetProperty(sender, propertyName);
                if (e.PropertyName == propertyName)
                    settings[propertyName] = newValue;
                foreach (var notifyobj in registeredObject[propertyName])
                    if(!ReferenceEquals(notifyobj, sender))
                        SetProperty(notifyobj, propertyName, newValue);
            };
        }

        public object this[string key]
        {
            get
            {
                if (localSettings.ContainsKey(key))
                    return localSettings[key];
                else if (roamingSettings.ContainsKey(key))
                    return roamingSettings[key];
                else return null;
            }
        }
    }
}
