using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTV6.Mvvm;
using iTV6.Models;
using iTV6.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iTV6.ViewModels
{
    class SettingsViewModel : ViewModelBase
    {
        //默认视频源
        public List<string> MediaSources { get; } = new List<string>();
        private string _poriorSource = null;
        public string PriorSource
        {
            get { return _poriorSource; }
            set
            {
                Set(ref _poriorSource, value);
                Settings.SetValue("PriorSource", _poriorSource);
            }
        }
        //录播文件存储地址
        private string _filePath="...";//录播保存地址
        public string FilePath
        {
            get { return _filePath; }
            set {
                Set(ref _filePath, value);
                Settings.SetValue("FilePath", _filePath);
            }
        }
        //主题
        public List<string> ThemeList = new List<string>();
        private string _selectedTheme = null;
        public string SelectedTheme {
            get { return _selectedTheme;}
            set {
                Set(ref _selectedTheme,value);
                Settings.SetValue("Theme", _selectedTheme);
            }
        }
        //是否发送桌面通知
        private bool _sendDesktopNotifications = false;
        public bool SendDesktopNotifications
        {
            get { return _sendDesktopNotifications;}
            set {
                Set(ref _sendDesktopNotifications, value);
                Settings.SetValue("SendDesktopNotifications", _sendDesktopNotifications);
            }
        }
        //是否使用系统日历
        private bool _useSystemCalender = false;
        public bool UseSystemCalender
        {
            get { return _useSystemCalender; }
            set
            {
                Set(ref _useSystemCalender, value);
                Settings.SetValue("UseSystemCalender", _useSystemCalender);
            }
        }
        //构造函数
        public SettingsViewModel()
        {
            //MediaSources = TelevisionService.Instance.AvaliablePrograms.First().MediaSources;//这里的source好像不大对啊，有各种后缀
            //PriorSource = MediaSources.First();
            MediaSources.Add("清华"); MediaSources.Add("中国农大"); MediaSources.Add("东北大学"); MediaSources.Add("北邮人");
            ThemeList.Add("浅色"); ThemeList.Add("深色"); ThemeList.Add("蓝色");
            if (Settings.ContainsKey("PriorSource"))//读取设置：默认视频源
                PriorSource = Settings.GetValue("PriorSource").ToString();
            if (Settings.ContainsKey("FilePath"))//读取设置：存储路径
                FilePath = Settings.GetValue("FilePath").ToString();
            if (Settings.ContainsKey("Theme"))//读取设置：主题
                SelectedTheme = Settings.GetValue("Theme").ToString();
            else
                SelectedTheme = ThemeList.First();
            if (Settings.ContainsKey("SendDesktopNotifications"))//读取设置：是否发送桌面通知
                SendDesktopNotifications = (bool)Settings.GetValue("SendDesktopNotifications");
            if (Settings.ContainsKey("UseSystemCalender"))//读取设置：是否使用系统日历
                UseSystemCalender = (bool)Settings.GetValue("UseSystemCalender");
        }

        public async void BrowseButtonTapped()//点了“浏览”之后，选取文件夹
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                FilePath=folder.Path;
            }
            System.Diagnostics.Debug.WriteLine(FilePath);
        }
    }
}
