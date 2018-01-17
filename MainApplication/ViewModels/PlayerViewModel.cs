using iTV6.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTV6.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        public PlayerViewModel()
        {
            PlayCustomSource = new DelegateCommand(() =>
            {
                PlayerSource = new Uri(CustomSource);
            }, () => Uri.IsWellFormedUriString(CustomSource, UriKind.Absolute));
        }

        private Uri _PlayerSource;
        public Uri PlayerSource
        {
            get { return _PlayerSource; }
            set { Set(ref _PlayerSource, value); }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { Set(ref _Title, value); }
        }

        private string _CustomSource;
        public string CustomSource
        {
            get { return _CustomSource; }
            set
            {
                Set(ref _CustomSource, value);
                PlayCustomSource.RaiseCanExecuteChanged();
            }
        }

        public override void OnNavigatedTo(object paramter)
        {
            if (paramter is Uri) // 直接是视频地址
            {
                PlayerSource = paramter as Uri;
                Title = PlayerSource.ToString();
            }
            else if (paramter is Tuple<Uri, string>) // 视频地址 + 标题
            {
                var tpar = paramter as Tuple<Uri, string>;
                PlayerSource = tpar.Item1;
                Title = tpar.Item2;
            }
        }

        // XXX: 如果直接在声明时便赋值貌似会导致CanExecute不会被调用
        // 而如果在构造函数里面声明便不会。。。有点迷
        public DelegateCommand PlayCustomSource { get; } 
    }
}
