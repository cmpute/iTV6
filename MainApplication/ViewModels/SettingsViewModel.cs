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
        public List<SourceRecord> MediaSources { get; } = new List<SourceRecord>();
        private SourceRecord _PoriorSource = null;
        public SourceRecord PriorSource
        {
            get { return _PoriorSource; }
            set
            {
                Set(ref _PoriorSource, value);
            }
        }

        private string _FilePath="...";//录播保存地址
        public string FilePath
        {
            get { return _FilePath; }
            set { Set(ref _FilePath, value); }
        }

        public SettingsViewModel()
        {
            MediaSources = TelevisionService.Instance.AvaliablePrograms.First().MediaSources;//这里的source好像不大对啊，有各种后缀
            PriorSource = MediaSources.First();
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
