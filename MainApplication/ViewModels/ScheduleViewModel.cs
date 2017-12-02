using iTV6.Mvvm;
using iTV6.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.ViewModels
{
    public class ScheduleViewModel : ViewModelBase
    {
        public DelegateCommand CustomPlayback => new DelegateCommand(() =>
        {
            new PlaybackDialog().ShowAsync();
        }, () => true);
    }
}
