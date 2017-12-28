using iTV6.Models;
using iTV6.Mvvm;
using iTV6.Services;
using iTV6.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Data;

namespace iTV6.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        public DelegateCommand CustomPlayback => new DelegateCommand(() =>
        {
            new PlaybackDialog().ShowAsync();
        });

        public CollectionViewSource ScheduleChannelList { get; set; } = new CollectionViewSource();

        public override async void OnNavigatedTo(object paramter)
        {
            ShowChannelLoading = true;
            // 加载区域列表
            var districts = await ScheduleService.Instance.GetDistrictCodeMap();
            var channelCollection = new ObservableCollection<ChannelDistrictAdapter>();
            ScheduleChannelList.Source = channelCollection;
            ScheduleChannelList.IsSourceGrouped = true;

            // 加载每个区域的频道
            ChannelLoadingMaximum = districts.Count;
            // 用下面的方法的话并发数太高，不少网页都会超时
            // await Task.WhenAll(districts.Select((item) => FetchDistrict(item.Key, item.Value)));
            foreach (var item in districts)
                await FetchDistrict(item.Key, item.Value);
            ShowChannelLoading = false;
        }

        private async Task FetchDistrict(string name, string code)
        {
            try
            {
                var list = await ScheduleService.Instance.GetDailySchedule(code);
                var adapter = new ChannelDistrictAdapter(list.Keys, name);
                (ScheduleChannelList.Source as IList<ChannelDistrictAdapter>).Add(adapter);
            }
            catch(Exception e)
            {
#if DEBUG
                new MessageDialog(e.Message, $"在获取{name}区域列表时发生{e.GetType()}类型的异常").ShowAsync();
                System.Diagnostics.Debugger.Break();
#else
                new MessageDialog(e.Message, $"在获取{name}区域列表时发生{e.GetType()}类型的异常").ShowAsync();
#endif
            }
            finally
            {
                ChannelLoadingProgress += 1;
            }
        }

        private Channel _selectedChannel;
        /// <summary>
        /// 选中的频道
        /// </summary>
        public Channel SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                Set(ref _selectedChannel, value);
                FetchSchedule();
            }
        }

        private int _selectedDoW = (int)DateTime.Now.DayOfWeek;
        /// <summary>
        /// 选中的是星期几
        /// </summary>
        public int SelectedDow
        {
            get { return _selectedDoW; }
            set { Set(ref _selectedDoW, value);
                FetchSchedule();
            }
        }

        public ObservableCollection<Models.Program> SchedulePrograms { get; set; } = new ObservableCollection<Models.Program>();

        private async void FetchSchedule()
        {
            SchedulePrograms.Clear();
            string district = null;
            foreach (var group in ScheduleChannelList.Source as IEnumerable<ChannelDistrictAdapter>)
                if (group.Contains(SelectedChannel))
                    district = group.District;
            var districtMap = await ScheduleService.Instance.GetDistrictCodeMap();
            var result = await ScheduleService.Instance.GetDailySchedule(districtMap[district], (DayOfWeek)Enum.ToObject(typeof(DayOfWeek), SelectedDow));
            foreach (var ch in result.Keys)
                if (ch == SelectedChannel)
                {
                    // TODO: 第一次加载的时候会出现节目重复的情况
                    foreach (var program in result[ch])
                        SchedulePrograms.Add(program);
                    break;
                }
            ShowSelectChannel = false;
        }

        private bool _showChannelLoading;
        /// <summary>
        /// 是否显示正在加载频道列表的进度条
        /// </summary>
        public bool ShowChannelLoading
        {
            get { return _showChannelLoading; }
            set { Set(ref _showChannelLoading, value); }
        }

        private double _channelLoadingProgress;
        /// <summary>
        /// 加载频道列表的进度条位置
        /// </summary>
        public double ChannelLoadingProgress
        {
            get { return _channelLoadingProgress; }
            set { Set(ref _channelLoadingProgress, value); }
        }

        private double _channelLoadingMaximum;
        /// <summary>
        /// 加载频道列表的进度条最大值
        /// </summary>
        public double ChannelLoadingMaximum
        {
            get { return _channelLoadingMaximum; }
            set { Set(ref _channelLoadingMaximum, value); }
        }

        private bool _showSelectChannel = true;
        /// <summary>
        /// 是否显示请选择频道的提示
        /// </summary>
        public bool ShowSelectChannel
        {
            get { return _showSelectChannel; }
            set { Set(ref _showSelectChannel, value); }
        }
    }

    /// <summary>
    /// 按频道类型分类的组
    /// </summary>
    class ChannelDistrictAdapter : List<Channel>
    {
        public string District { get; set; }

        public ChannelDistrictAdapter(IEnumerable<Channel> channels, string district) : base(channels)
        {
            District = district;
        }
    }
}
