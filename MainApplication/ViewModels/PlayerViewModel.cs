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
        private Uri _PlaySource;
        public Uri PlaySource
        {
            get { return _PlaySource; }
            set { Set(ref _PlaySource, value); }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { Set(ref _Title, value); }
        }

        public override void OnNavigatedTo(object paramter)
        {
            if (paramter is Uri)
            {
                PlaySource = paramter as Uri;
                Title = PlaySource.ToString();
            }
            else if (paramter is Tuple<Uri, string>)
            {
                var tpar = paramter as Tuple<Uri, string>;
                PlaySource = tpar.Item1;
                Title = tpar.Item2;
            }
        }
    }
}
