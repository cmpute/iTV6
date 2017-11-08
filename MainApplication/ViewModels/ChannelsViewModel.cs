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
                var adapter = new ChannelTypeAdapter();
                adapter.AddRange(group);
                string typeName = "";
                if(group.Key.HasFlag(ChannelType.Central))
                    typeName = "央视";
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
        }

        public CollectionViewSource Programs { get; set; }

        private PlayingProgram _selectedProgram;
        /// <summary>
        /// 选择播放的频道（节目）
        /// </summary>
        public PlayingProgram SelectedProgram
        {
            get => _selectedProgram;
            set
            {
                Set(ref _selectedProgram, value);
                Schedule = Async.InvokeAndWait(async() => await
                    TelevisionService.Instance.TelevisionStations.First().GetSchedule(value.ProgramInfo.Channel));
            }
        }

        /// <summary>
        /// 选中频道的节目列表
        /// </summary>
        private IEnumerable<Models.Program> _schedule;
        public IEnumerable<Models.Program> Schedule
        {
            get => _schedule;
            set => Set(ref _schedule, value);
        }
    }

    /// <summary>
    /// 按频道类型分类的组
    /// </summary>
    class ChannelTypeAdapter : List<AvailableProgram>
    {
        public string Type { get; set; }
    }
    /// <summary>
    /// 按频道名称拼音首字母分类的组
    /// </summary>
    class ChannelAlphabetAdapter : List<AvailableProgram> { /* TODO: 实现按拼音分类 */ }
}
