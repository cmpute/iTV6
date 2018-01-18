using iTV6.Models;
using iTV6.Mvvm;
using iTV6.Services;
using iTV6.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace iTV6.ViewModels
{
    /// <summary>
    /// 为非实例化的XAML对象提供全局的ViewModel
    /// </summary>
    public class StaticViewModel : ViewModelBase
    {
        /// <summary>
        /// 将节目添加到日历提醒
        /// </summary>
        public DelegateCommand<Models.Program> AddToCalendar { get; } = new DelegateCommand<Models.Program>(async (program) =>
        {
            switch (await CalendarService.Instance.CreateAppoint(program))
            {
                case CalendarService.Messages.Sucess:
                    await new MessageDialog($"成功为节目 {program} 添加日历提醒", "添加提醒成功").ShowAsync();
                    break;
                case CalendarService.Messages.NotInitialized:
                    await new MessageDialog("添加日历失败，请稍后重试", "提醒").ShowAsync();
                    break;
                case CalendarService.Messages.AlreadyExists:
                    await new MessageDialog("节目提醒已存在", "添加日历提醒失败").ShowAsync();
                    break;
            };
        });

        /// <summary>
        /// 切换频道的收藏状态
        /// </summary>
        public DelegateCommand<Channel> ToggleFavouriteChannel { get; } = new DelegateCommand<Channel>((channel) =>
        {
            if (CollectionService.Instance.CheckChannel(channel))
                CollectionService.Instance.RemoveChannel(channel);
            else
                CollectionService.Instance.AddChannel(channel);
        });

        /// <summary>
        /// 切换节目的收藏状态
        /// </summary>
        public DelegateCommand<Models.Program> ToggleFavouriteProgram { get; } = new DelegateCommand<Models.Program>((program) =>
        {
            if (CollectionService.Instance.CheckProgram(program))
                CollectionService.Instance.RemoveProgram(program);
            else
                CollectionService.Instance.AddProgram(program);
        });

        /// <summary>
        /// 将节目固定为磁贴
        /// </summary>
        public DelegateCommand<Channel> PinChannelToStart { get; } = new DelegateCommand<Channel>(async (channel) =>
        {
            if(await TileService.Instance.PinChannel(channel))
                await new MessageDialog($"已将 {channel.Name} 固定到开始菜单", "添加磁贴成功").ShowAsync();
            else
                await new MessageDialog($"固定磁贴失败，请检查权限设置", "添加磁贴失败").ShowAsync();
        });

        /// <summary>
        /// 从频道触发视频录制
        /// </summary>
        public DelegateCommand<Channel> TriggerChannelRecording { get; } = new DelegateCommand<Channel>(async (channel) =>
        {
            var sources = ChannelsViewModel.Instance.Programs.Source as List<ChannelTypeAdapter>;
            foreach (var source in sources.SelectMany(adapter => adapter))
                if(source.ProgramInfo.Channel == channel)
                {
                    // 取第一个视频源作为默认视频源
                    await new RecordDialog(channel, source.MediaSources.First()).ShowAsync();
                    break;
                }
        });

        /// <summary>
        /// 从节目触发视频录制
        /// </summary>
        public DelegateCommand<Models.Program> TriggerProgramRecording { get; } = new DelegateCommand<Models.Program>((program) =>
        {
            RecordService.Instance.StartRecording(program.Channel, ChannelsViewModel.Instance.SelectedSource, program.StartTime, program.Duration);
            NavigationService.ShellNavigation.Navigate<RecordingsPage>();
        });

        /// <summary>
        /// 删除录播任务
        /// </summary>
        public DelegateCommand<DownloadToken> DeleteRecording { get; } = new DelegateCommand<DownloadToken>((token) =>
        {
            RecordService.Instance.DeleteRecording(token);
        });
    }
}
