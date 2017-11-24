using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace iTV6.Mvvm
{
    /// <summary>
    /// 所有ViewModel均继承此基类。该类提供ViewModel中常用的成员。
    /// </summary>
    public class ViewModelBase : BindableBase
    {
        public Windows.UI.Xaml.Controls.Page Host { get; set; }

        public virtual async void OnNavigatedTo(object paramter) { await Task.CompletedTask; }
        public virtual async void OnNavigatedFrom(object paramter) { await Task.CompletedTask; }
    }
}
