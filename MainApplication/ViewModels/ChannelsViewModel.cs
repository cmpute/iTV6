using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.UI.Xaml.Data;
using iTV6.Models;
using iTV6.Mvvm;
using iTV6.Services;
using iTV6.Utils;
using Windows.UI.Xaml;

namespace iTV6.ViewModels
{
    public class ChannelsViewModel : ViewModelBase
    {
        public ChannelsViewModel()
        {
            // 将节目列表进行分组
            Programs = new CollectionViewSource();
            var currentPrograms = TelevisionService.Instance.AvaliablePrograms;
            var channelAdapters = new List<ChannelTypeAdapter>();
            foreach(var group in currentPrograms.GroupBy(program => program.ProgramInfo.Channel.Type))
            {
                // TODO: 进一步完善分组
                var adapter = new ChannelTypeAdapter();
                adapter.AddRange(group);
                string typeName = "";
                if(group.Key.HasFlag(ChannelType.Central))
                    typeName = "央视";
                else if (group.Key.HasFlag(ChannelType.Beijing))
                    typeName = "北京";
                else if(group.Key.HasFlag(ChannelType.Local))
                    typeName = "地方";
                else if (group.Key.HasFlag(ChannelType.Special))
                    typeName = "特别";
                else if (group.Key.HasFlag(ChannelType.Radio))
                    typeName = "广播";
                if (group.Key.HasFlag(ChannelType.Hd))
                    typeName += "高清";
                else if (group.Key.HasFlag(ChannelType.Standard))
                    typeName += "标清";
                adapter.Type = typeName + "频道";
                channelAdapters.Add(adapter);
            }
            Programs.Source = channelAdapters;
            Programs.IsSourceGrouped = true;

            AddRecordTask = new DelegateCommand(() =>
            {
                NavigationService.ShellNavigation.Navigate<Views.RecordingsPage>(new Tuple<MultisourceProgram,SourceRecord,Uri>(SelectedProgram,SelectedSource,SelectedSource.Source));
            }, () => (SelectedProgram != null && SelectedSource != null));
            // 监听收藏的变动
            CollectionService.Instance.ChannelListChanged += (sender, e) =>
            {
                if (SelectedProgram != null)
                    IsCurrentChannelFavourite = CollectionService.Instance.CheckChannel(SelectedProgram.ProgramInfo.Channel);
            };
            CollectionService.Instance.ProgramListChanged += (sender, e) =>
            {
                if (SelectedProgram != null)
                    IsCurrentProgramFavourite = CollectionService.Instance.CheckProgram(SelectedProgram.ProgramInfo);
            };

            // 绑定功能按钮的Command
            ToggleSidePanel = new DelegateCommand(() =>
            {
                VisualStateManager.GoToState(Host, _IsSidePanelExpaneded ? "SideCollapsed" : "SideExpanded", true);
                _IsSidePanelExpaneded = !_IsSidePanelExpaneded;
            }, () => SelectedProgram != null);
        }
    
        // 这个方法只是为了实现在刚进入时播放界面是被折叠的。这样的实现避免设计器的设计困难，也能避免实现过于繁琐
        public void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // 设定初始VisualState
            if (SelectedProgram == null)
                VisualStateManager.GoToState(Host, "SideCollapsed", true);
        }

        public override void OnNavigatedTo(object paramter)
        {
            if (paramter is Channel)
                foreach (MultisourceProgram program in Programs.View)
                    if (program.ProgramInfo.Channel == paramter)
                    {
                        SelectedProgram = program;
                        break;
                    }
        }

        public CollectionViewSource Programs { get; set; }

        private MultisourceProgram _selectedProgram;
        /// <summary>
        /// 选择播放的频道（节目）
        /// </summary>
        public MultisourceProgram SelectedProgram
        {
            get { return _selectedProgram; }
            set
            {
                Set(ref _selectedProgram, value);
                OnSelectedProgramChanged();
            }
        }

        private SourceRecord _selectedSource;
        /// <summary>
        /// 选择播放的视频源
        /// </summary>
        public SourceRecord SelectedSource
        {
            get { return _selectedSource; }
            set
            {
                Set(ref _selectedSource, value);
            }
        }

        private async void OnSelectedProgramChanged()
        {
            //获得节目单，默认使用第一个可用的节目单源
            var channel = SelectedProgram.ProgramInfo.Channel;
            Schedule = await TelevisionService.Instance.GetSchedule(channel);

            //更新收藏状况
            IsCurrentChannelFavourite = CollectionService.Instance.CheckChannel(channel);
            IsCurrentProgramFavourite = CollectionService.Instance.CheckProgram(SelectedProgram.ProgramInfo);
            AddRecordTask.RaiseCanExecuteChanged();
            //选择默认来源
            SelectedSource = SelectedProgram.MediaSources.First();

            //展开侧面面板
            ToggleSidePanel.RaiseCanExecuteChanged();
            if (!_IsSidePanelExpaneded)
            {
                _IsSidePanelExpaneded = true;
                if (Host != null)
                    VisualStateManager.GoToState(Host, "SideExpanded", true);
                else
                    HostLoaded.Action = (host) => VisualStateManager.GoToState(host, "SideExpanded", true);
            }
        }

        /// <summary>
        /// 选中频道的节目列表
        /// </summary>
        private IEnumerable<Models.Program> _schedule;
        public IEnumerable<Models.Program> Schedule
        {
            get { return _schedule; }
            set { Set(ref _schedule, value); }
        }

        private bool _IsCurrentChannelFavourite;
        /// <summary>
        /// 当前频道是否被收藏
        /// </summary>
        public bool IsCurrentChannelFavourite
        {
            get { return _IsCurrentChannelFavourite; }
            set { Set(ref _IsCurrentChannelFavourite, value); }
        }

        public DelegateCommand AddRecordTask { get; }

        private bool _IsCurrentProgramFavourite;
        /// <summary>
        /// 当前节目是否被收藏
        /// </summary>
        public bool IsCurrentProgramFavourite
        {
            get { return _IsCurrentProgramFavourite; }
            set { Set(ref _IsCurrentProgramFavourite, value); }
        }
        
        /// <summary>
        /// 收起或展开侧边栏（视频播放）
        /// </summary>
        public DelegateCommand ToggleSidePanel { get; }
        private bool _IsSidePanelExpaneded = false;
    }

    /// <summary>
    /// 按频道类型分类的组
    /// </summary>
    class ChannelTypeAdapter : List<MultisourceProgram>
    {
        public string Type { get; set; }
    }
    /// <summary>
    /// 按频道名称拼音首字母分类的组
    /// </summary>
    class ChannelAlphabetAdapter : List<MultisourceProgram> { /* TODO: 实现按拼音分类 */ }
}
