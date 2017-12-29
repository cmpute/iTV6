using iTV6.Mvvm;
using iTV6.Views;
using iTV6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTV6.Services;
using Windows.UI.Popups;

namespace iTV6.ViewModels
{
    public class RecordingsViewModel : ViewModelBase
    {
        public override async void OnNavigatedTo(object parameter)
        {
            if (parameter is Tuple<Channel, SourceRecord>)  // 选择的节目、选择的源
            {
                var tpar = parameter as Tuple<Channel, SourceRecord>;
                var channel = tpar.Item1;
                var source = tpar.Item2;
                await new RecordDialog(channel.Name, source.StationName, source.Source).ShowAsync();
            }
        }

        public DelegateCommand CustomRecord => new DelegateCommand(async () =>
        {
            await new MessageDialog("请到频道页面选取节目和视频源，然后点击右上角的的“添加录播”按钮。", "提示").ShowAsync();
            NavigationService.ShellNavigation.Navigate<ChannelsPage>();
        });
    }
}
