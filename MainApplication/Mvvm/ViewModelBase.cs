using iTV6.Utils;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace iTV6.Mvvm
{
    /// <summary>
    /// 所有ViewModel均继承此基类。该类提供ViewModel中常用的成员。
    /// </summary>
    public class ViewModelBase : BindableBase
    {
        private Page _host;
        public Page Host
        {
            get { return _host; }
            set
            {
                _host = value;
                HostLoaded.Invoke(value);
            }
        }
        public DeferedAction<Page> HostLoaded { get; } = new DeferedAction<Page>();

        public virtual async void OnNavigatedTo(object paramter) { await Task.CompletedTask; }
        public virtual async void OnNavigatedFrom(object paramter) { await Task.CompletedTask; }
    }
}
