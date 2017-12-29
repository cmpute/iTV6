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
        public SettingsViewModel()
        {
            // 由于默认值不是常量，因此需要动态注册
            SettingService.Instance.RegisterSetting(this, nameof(PriorSource), null, MediaSources.First());
            SettingService.Instance.RegisterSetting(this, nameof(Theme), null, ThemeList.First());
            SettingService.Instance.ApplySettingAttributes(this);
        }

        public IEnumerable<string> MediaSources { get; } =
            TelevisionService.Instance.TelevisionStations.Select((station) => station.IdentifierName);

        private string _poriorSource = null;
        /// <summary>
        /// 默认视频源
        /// </summary>
        public string PriorSource
        {
            get { return _poriorSource; }
            set { Set(ref _poriorSource, value); }
        }

        private string _filePath = null;
        /// <summary>
        /// 录播文件存储地址
        /// </summary>
        [SettingProperty]
        public string FilePath
        {
            get { return _filePath; }
            set { Set(ref _filePath, value); }
        }

        public IEnumerable<string> ThemeList { get; } = SettingService.Instance.ThemeList;//从service获得主题列表
        private string _selectedTheme = null;
        /// <summary>
        /// 主题
        /// </summary>
        public string Theme
        {
            get { return _selectedTheme; }
            set { Set(ref _selectedTheme, value); }
        }

        private bool _nightMode = false;
        /// <summary>
        /// 夜间模式
        /// </summary>
        [SettingProperty]
        public bool NightMode
        {
            get { return _nightMode; }
            set { Set(ref _nightMode, value); }
        }

        private bool _sendDesktopNotifications = false;
        /// <summary>
        /// 是否发送桌面通知
        /// </summary>
        [SettingProperty(DefaultValue = false)]
        public bool SendDesktopNotifications
        {
            get { return _sendDesktopNotifications; }
            set { Set(ref _sendDesktopNotifications, value); }
        }

        private bool _useSystemCalender = false;
        /// <summary>
        /// 是否使用系统日历
        /// </summary>
        [SettingProperty(DefaultValue = false)]
        public bool UseSystemCalender
        {
            get { return _useSystemCalender; }
            set { Set(ref _useSystemCalender, value); }
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
                FilePath = folder.Path;
            }
            System.Diagnostics.Debug.WriteLine(FilePath);
        }
    }
}
