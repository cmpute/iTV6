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
using Windows.Storage;

namespace iTV6.ViewModels
{
    public class RecordingsViewModel : ViewModelBase
    {
        public RecordingsViewModel() { }

        public ObservableCollection<DownloadTask> TaskList => RecordService.Instance.TaskList;
        public ObservableCollection<DownloadTask> CompletedTaskList => RecordService.Instance.CompletedTaskList;

        public override async void OnNavigatedTo(object parameter)
        {
            if (parameter is Tuple<Channel, SourceRecord>)  // 选择的节目、选择的源
            {
                var tpar = parameter as Tuple<Channel, SourceRecord>;
                var channel = tpar.Item1;
                var source = tpar.Item2;
                var recordDialog = new RecordDialog(channel.Name, source.StationName, source.Source);
                while (!recordDialog.Completed)
                { await recordDialog.ShowAsync(); }
            }
        }

        public DelegateCommand CustomRecord => new DelegateCommand(async () =>
        {
            await new MessageDialog("请到频道页面选取节目和视频源，然后点击右上角的的“添加录播”按钮。", "提示").ShowAsync();
            NavigationService.ShellNavigation.Navigate<ChannelsPage>();
        });

        private DownloadTask _selectedTask;
        public DownloadTask SelectedTask
        {
            get { return _selectedTask;}
            set { Set(ref _selectedTask, value);}
        }

        private DownloadTask _selectedCompletedTask;
        public DownloadTask SelectedCompletedTask
        {
            get { return _selectedCompletedTask; }
            set { Set(ref _selectedCompletedTask, value); }
        }

        public async void DeleteTask()
        {
            if (SelectedTask != null)
            {
                SelectedTask.StopDownload();
                await SelectedTask.DeleteFile();
                TaskList.Remove(SelectedTask);
                SelectedTask = null;
            }
        }

        public async void DeleteCompletedTask()
        {
            if (SelectedCompletedTask != null)
            {
                await SelectedCompletedTask.DeleteFile();
                CompletedTaskList.Remove(SelectedCompletedTask);
                SelectedCompletedTask = null;
            }
        }

        public async void PlayCompletedTask()
        {
            if(SelectedCompletedTask != null)
            {
                StorageFile file = null;
                file = await SelectedCompletedTask.Folder.CreateFileAsync(SelectedCompletedTask.FileName + ".ts", CreationCollisionOption.OpenIfExists);
            
            var success = await Windows.System.Launcher.LaunchFileAsync(file);
            }
        }
    }
}
