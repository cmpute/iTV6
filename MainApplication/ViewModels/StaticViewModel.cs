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
        public DelegateCommand<Models.Program> AddToCalendar { get; } = new DelegateCommand<Models.Program>(
            (program) => CalendarService.Instance.CreateAppoint(program));
    }
}
