using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using iTV6.Mvvm;
using iTV6.Services;
using iTV6.Views;
using iTV6.Utils;

namespace iTV6.ViewModels
{
    public sealed class ShellViewModel : ViewModelBase
    {
        public ShellViewModel() { }
        
        public void FrameLoaded(object sender, RoutedEventArgs e)
        {
            // 注册外层菜单的导航服务
            NavigationService.ShellNavigation = new NavigationService((Host as Shell).NavigationFrame);
            NavigationService.ShellNavigation.Navigated += (csender, ce) =>
              {
                  if (ce.NavigatedPageType == typeof(ChannelsPage))
                      SelectedMenuIndex = 0;
              };
            NavigateChannels.Execute();
        }

        private static async Task<bool> CheckConnection()
        {
            if (await Connection.TestIPv6Connectivity())
                return true;
            var tasks = TelevisionService.Instance.TelevisionStations.Select(station => station.CheckConnectivity());
            var result = await Task.WhenAll(tasks);
            return result.Any(res => res);
        }

        public DelegateCommand NavigateChannels { get; } = new DelegateCommand(() => {
        if (Async.InvokeAndWait(async () => await CheckConnection()))
                NavigationService.ShellNavigation.Navigate<ChannelsPage>();
            else
                NavigationService.ShellNavigation.Navigate<ConnectionStatusPage>("请检查IPv6的连接");
        }); // 频道
        public DelegateCommand NavigateCollection { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<CollectionPage>()); // 收藏
        public DelegateCommand NavigateSchedule { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<SchedulePage>()); // 节目单
        public DelegateCommand NavigateRecordings { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<RecordingsPage>()); // 录播
        public DelegateCommand NavigateAbout { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<AboutPage>()); // 关于
        public DelegateCommand NavigateSettings { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<SettingsPage>()); // 设置

        public List<NavigationItem> NavigationItems { get; } = new List<NavigationItem>();

        private int _SelectedMenuIndex;
        public int SelectedMenuIndex
        {
            get { return _SelectedMenuIndex; }
            set { Set(ref _SelectedMenuIndex, value); }
        }
    }

    /// <summary>
    /// 导航菜单项的模型，由于比较小，因此不放在Model内了。
    /// </summary>
    public class NavigationItem
    {
        public string Name { get; set; }
        public string Glyph { get; set; }
        public DelegateCommand Navigate { get; set; }
    }
}
