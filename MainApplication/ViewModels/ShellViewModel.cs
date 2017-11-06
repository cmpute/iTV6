using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using iTV6.Mvvm;
using iTV6.Services;
using iTV6.Views;

namespace iTV6.ViewModels
{
    public sealed class ShellViewModel : ViewModelBase
    {
        public ShellViewModel() { }
        
        public void FrameLoaded(object sender, RoutedEventArgs e)
        {
            NavigationService.ShellNavigation = new NavigationService((Host as Shell).NavigationFrame);
        }

        public DelegateCommand NavigateChannels { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<ChannelsPage>());
        public DelegateCommand NavigateCollection { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<CollectionPage>());
        public DelegateCommand NavigateRecordings { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<RecordingsPage>());
        public DelegateCommand NavigateAbout { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<AboutPage>());
        public DelegateCommand NavigateSettings { get; } = new DelegateCommand(() =>
            NavigationService.ShellNavigation.Navigate<SettingsPage>());

        public List<NavigationItem> NavigationItems { get; } = new List<NavigationItem>();
    }

    public class NavigationItem
    {
        public string Name { get; set; }
        public string Glyph { get; set; }
        public DelegateCommand Navigate { get; set; }
    }
}
