using iTV6.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace iTV6.Services
{
    public class SettingService
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

        // FIXME: 这里一直保持引用可能会导致内存泄漏，考虑改成WeakReference，并增加DisposeSettingAttributes方法
        private Dictionary<string, List<object>> registeredObject = new Dictionary<string, List<object>>();

        private static void SetProperty(object obj, string propertyName, object value)
            => obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        private static object GetProperty(object obj, string propertyName)
            => obj.GetType().GetProperty(propertyName).GetValue(obj);

        /// <summary>
        /// 注册一个设置选项，并自动绑定属性。注意：属性名即为设置键值，相同设置项需要对应相同的属性名
        /// </summary>
        /// <param name="obj">绑定来源</param>
        /// <param name="propertyName">绑定的属性名称，请使用<code>nameof</code>关键字</param>
        /// <param name="settingKey">绑定属性对应的键值，若为空则与propertyName相同</param>
        /// <param name="defaultValue">设置的默认值</param>
        /// <param name="roaming">是否设置为漫游同步属性</param>
        public void RegisterSetting(object obj, string propertyName,
            string settingKey = null, object defaultValue = null, bool roaming = false)
        {
            if (settingKey == null)
                settingKey = propertyName;
            // 设置初始值
            var settings = roaming ? roamingSettings : localSettings;
            if (settings.ContainsKey(settingKey))
                SetProperty(obj, propertyName, settings[settingKey]);
            else
            {
                if (defaultValue != null)
                {
                    SetProperty(obj, propertyName, defaultValue);
                    settings[settingKey] = defaultValue;
                }
            }
            // 添加到注册对象表
            if (!registeredObject.ContainsKey(propertyName))
                registeredObject[propertyName] = new List<object>();
            registeredObject[propertyName].Add(obj);

            // 注册响应事件
            if (obj is INotifyPropertyChanged)
                (obj as INotifyPropertyChanged).PropertyChanged += (sender, e) =>
                {
                    var newValue = GetProperty(sender, propertyName);
                    if (e.PropertyName == propertyName)
                        settings[settingKey] = newValue;
                    foreach (var notifyobj in registeredObject[propertyName])
                        if (!ReferenceEquals(notifyobj, sender))
                            SetProperty(notifyobj, propertyName, newValue);
                };

            // TODO: 考虑利用为ApplicationData.Current.LocalSettings.Values.MapChanged事件也添加更新
        }

        /// <summary>
        /// 获取设置值
        /// </summary>
        /// <param name="key">设置键值</param>
        /// <param name="otherwise">当设置未初始化时的值</param>
        public object Get(string key, object otherwise)
        {
            if (localSettings.ContainsKey(key))
                return localSettings[key];
            else if (roamingSettings.ContainsKey(key))
                return roamingSettings[key];
            else
                return otherwise;
        }

        /// <summary>
        /// 将类型中标注过<see cref="SettingPropertyAttribute"/>的设置属性进行绑定
        /// </summary>
        /// <param name="target"></param>
        public void ApplySettingAttributes(object instance)
        {
            foreach (var member in instance.GetType().GetMembers())
            {
                var attr = member.GetCustomAttribute<SettingPropertyAttribute>();
                if (attr != null)
                    RegisterSetting(instance, member.Name, attr.SettingKey, attr.DefaultValue, attr.Roaming);
            }
        }

        /// <summary>
        /// 重置应用设置
        /// </summary>
        public void ResetSettings()
        {
            localSettings.Values.Clear();
            roamingSettings.Values.Clear();
            throw new NotImplementedException();
            // TODO: 更新registeredObject中的各对象
        }
    }

    /// <summary>
    /// 为属性注册提供便利的接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SettingPropertyAttribute : Attribute
    {
        public String SettingKey { get; set; } = null;
        public object DefaultValue { get; set; } = null;
        public bool Roaming { get; set; } = false;
        public SettingPropertyAttribute() { }
    }
}
