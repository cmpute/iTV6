using iTV6.Mvvm;
using iTV6.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.ViewModels
{
    public class RecordingsViewModel : ViewModelBase
    {
        public DelegateCommand CustomRecord => new DelegateCommand(() =>
        {
            new RecordDialog().ShowAsync();
        });
    }
}
