using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace iTV6.Services
{
    public class NavigationService
    {
        public Frame _root;

        public NavigationService(Frame frame)
        {
            _root = frame;
        }
        
        public static NavigationService ShellNavigation { get; set; }

        public event EventHandler<NavigationEventArgs> Navigated;

        public Task Navigate<T>() where T : class
        {
            _root.Navigate(typeof(T));
            return Task.CompletedTask;
        }
    }
}
