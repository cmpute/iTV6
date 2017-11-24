﻿using iTV6.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace iTV6.Services
{
    /// <summary>
    /// 该类满足相比于Frame自带的更加复杂的Navigation服务
    /// </summary>
    public class NavigationService
    {
        // TODO: 完成后退功能
        // TODO: 调查一下Frame的Navigate是否会造成资源浪费

        public Frame _root;

        public NavigationService(Frame frame)
        {
            _root = frame;
        }
        
        public static NavigationService ShellNavigation { get; set; }

        // public event EventHandler<NavigationEventArgs> Navigated;

        public void Navigate<T>() where T : class
        {
            ((_root.Content as Page)?.DataContext as ViewModelBase)?.OnNavigatedFrom(null);
            _root.Navigate(typeof(T));
            ((_root.Content as Page)?.DataContext as ViewModelBase)?.OnNavigatedTo(null);
        }

        public void Navigate<T>(object paramter) where T : class
        {
            ((_root.Content as Page)?.DataContext as ViewModelBase)?.OnNavigatedFrom(paramter);
            _root.Navigate(typeof(T), paramter);
            ((_root.Content as Page)?.DataContext as ViewModelBase)?.OnNavigatedTo(paramter);
        }
    }
}
