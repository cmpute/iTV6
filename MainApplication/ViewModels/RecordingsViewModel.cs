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
        Uri URI = null;
        string Channel = null;
        string Source = null;

        public override void OnNavigatedTo(object parameter)
        {
            if (parameter is Tuple<MultisourceProgram, SourceRecord, Uri>)  // 选择的节目、选择的源和地址
            {
                var tpar = parameter as Tuple<MultisourceProgram, SourceRecord, Uri>;
                Channel = tpar.Item1.ProgramInfo.Channel.Name;
                Source = tpar.Item2.StationName.ToString();
                URI = tpar.Item3 as Uri;
                new RecordDialog(Channel, Source, URI).ShowAsync();
            }
        }
        public DelegateCommand CustomRecord => new DelegateCommand(async () =>
        {
            await new MessageDialog("请到频道页面选取节目和视频源，然后点击右上角的的“添加录播”按钮。", "提示").ShowAsync();
            NavigationService.ShellNavigation.Navigate<ChannelsPage>();
        });
    }
}
