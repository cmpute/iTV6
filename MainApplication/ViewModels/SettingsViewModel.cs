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
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace iTV6.ViewModels
{
    class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            // 由于默认值不是常量，因此需要动态注册
            SettingService.Instance.RegisterSetting(this, nameof(PriorSource), null, MediaSources.First());
            SettingService.Instance.RegisterSetting(this, nameof(Theme), null, ThemeList.First());
            SettingService.Instance.RegisterSetting(this, nameof(ReminderSpanAhead), null, TimeSpan.FromMinutes(5));
            SettingService.Instance.ApplySettingAttributes(this);
            ClearCacheFiles = new DelegateCommand(async () =>
            {
                await CacheService.ClearCache();
                await new MessageDialog("成功清空缓存", "成功").ShowAsync();
                CacheFileSize = 0;
            }, () => CacheFileSize > 0);
        }

        public override async void OnNavigatedTo(object paramter)
        {
            CacheFileSize = await CacheService.ComputeCacheSize() / 1024f;
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

        private bool _enableCalendarNotification = false;
        /// <summary>
        /// 是否发送桌面通知
        /// </summary>
        [SettingProperty(DefaultValue = false)]
        public bool EnableCalendarNotification
        {
            get { return _enableCalendarNotification; }
            set { Set(ref _enableCalendarNotification, value); }
        }

        private TimeSpan _reminderSpanAhead;
        /// <summary>
        /// 节目提醒的提前时间
        /// </summary>
        public TimeSpan ReminderSpanAhead
        {
            get { return _reminderSpanAhead; }
            set { Set(ref _reminderSpanAhead, value); }
        }

        /// <summary>
        /// 可选的提前时间
        /// </summary>
        public List<ReminderTimeAdapter> AvailableTime { get; } = new List<ReminderTimeAdapter>();

        private float _cacheFileSize;
        /// <summary>
        /// 缓存文件大小，单位为KB
        /// </summary>
        public float CacheFileSize
        {
            get { return _cacheFileSize; }
            set
            {
                Set(ref _cacheFileSize, value);
                ClearCacheFiles.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 点了“浏览”之后，选取文件夹
        /// </summary>
        public async void BrowseButtonTapped()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                FilePath = folder.Path;
            }
            LoggingService.Debug("Xaml", "选取文件夹：" + FilePath);
        }

        public DelegateCommand ClearCalendarItems { get; } = new DelegateCommand(async () =>
          {
              if (await CalendarService.Instance.DeleteAllAppointments() == CalendarService.Messages.Sucess)
                  await new MessageDialog("成功删除日历中的所有节目", "成功").ShowAsync();
              else
                  await new MessageDialog("清除日历中的所有节目失败，请稍后重试", "提醒").ShowAsync();
          });

        public DelegateCommand ClearCacheFiles { get; }
    }

    public class ReminderTimeAdapter
    {
        public string DisplayText { get; set; }
        public TimeSpan TimeAhead { get; set; }
    }
}
