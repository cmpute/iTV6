using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTV6.Mvvm;
using System.Collections.ObjectModel;
using iTV6.Models;
using iTV6.Services;

namespace iTV6.ViewModels
{
    public class CollectionViewModel : ViewModelBase
    {
        public CollectionViewModel() { }

        public ObservableCollection<Channel> ChannelList => CollectionService.Instance.ChannelList;
        public ObservableCollection<string> ProgramList => CollectionService.Instance.ProgramList;
    }
}
