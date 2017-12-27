using iTV6.Models;
using iTV6.Mvvm;
using iTV6.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.ViewModels
{
    /// <summary>
    /// 为非实例化的XAML对象提供ViewModel（如各种Template）
    /// </summary>
    public class StaticViewModel : ViewModelBase
    {
        /// <summary>
        /// 将节目添加到日历提醒
        /// </summary>
        public DelegateCommand<Models.Program> AddToCalendar { get; } = new DelegateCommand<Models.Program>(
            (program) => CalendarService.Instance.CreateAppoint(program));

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
    }
}
