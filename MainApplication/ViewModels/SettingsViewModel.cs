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
        public List<string> MediaSources { get; } = SettingService.Instance.MediaSources;//从service获得节目源列表
        private string _poriorSource = null;
        public string PriorSource
        {
            get { return _poriorSource; }
            set
            {
                Set(ref _poriorSource, value);
                SettingService.SetValue("PriorSource", _poriorSource);
            }
        }
        //录播文件存储地址
        private string _filePath = null;//录播保存地址
        public string FilePath
        {
            get { return _filePath; }
            set {
                Set(ref _filePath, value);
                SettingService.SetValue("FilePath", _filePath);
            }
        }
        //主题
        public List<string> ThemeList { get; } = SettingService.Instance.ThemeList;//从service获得主题列表
        private string _selectedTheme = null;
        public string SelectedTheme {
            get { return _selectedTheme;}
            set {
                Set(ref _selectedTheme,value);
                SettingService.SetValue("Theme", _selectedTheme);
            }
        }
        //是否发送桌面通知
        private bool _sendDesktopNotifications = false;
        public bool SendDesktopNotifications
        {
            get { return _sendDesktopNotifications;}
            set {
                Set(ref _sendDesktopNotifications, value);
                SettingService.SetValue("SendDesktopNotifications", _sendDesktopNotifications);
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
                SettingService.SetValue("UseSystemCalender", _useSystemCalender);
            }
        }
        //构造函数
        public SettingsViewModel()
        {
            if (SettingService.ContainsKey("PriorSource"))//读取设置：默认视频源
                PriorSource = SettingService.GetValue("PriorSource").ToString();
            else
                PriorSource = MediaSources.First();
            if (SettingService.ContainsKey("FilePath"))//读取设置：存储路径
                FilePath = SettingService.GetValue("FilePath").ToString();
            if (SettingService.ContainsKey("Theme"))//读取设置：主题
                SelectedTheme = SettingService.GetValue("Theme").ToString();
            else
                SelectedTheme = ThemeList.First();
            if (SettingService.ContainsKey("SendDesktopNotifications"))//读取设置：是否发送桌面通知
                SendDesktopNotifications = (bool)SettingService.GetValue("SendDesktopNotifications");
            else
                SendDesktopNotifications = false;
            if (SettingService.ContainsKey("UseSystemCalender"))//读取设置：是否使用系统日历
                UseSystemCalender = (bool)SettingService.GetValue("UseSystemCalender");
            else
                UseSystemCalender = false;
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
