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
using System.Collections.ObjectModel;

namespace iTV6.ViewModels
{
    public class RecordingsViewModel : ViewModelBase
    {
        public RecordingsViewModel() { }

        public IEnumerable<DownloadToken> TaskList => RecordService.Instance.TaskList;

        public DelegateCommand CustomRecord => new DelegateCommand(async () =>
        {
            await new MessageDialog("请到频道页面选取节目和视频源，然后点击右上角的的“添加录播”按钮。", "提示").ShowAsync();
            NavigationService.ShellNavigation.Navigate<ChannelsPage>();
        });

        private DownloadToken _selectedTask;
        public DownloadToken SelectedTask
        {
            get { return _selectedTask;}
            set { Set(ref _selectedTask, value);}
        }

        public void DeleteTask()
        {
            throw new NotImplementedException();
        }
    }
}
