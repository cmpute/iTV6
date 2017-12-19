using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using iTV6.Services;
using iTV6.Models;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace iTV6.Views
{
    public sealed partial class RecordDialog : ContentDialog
    {
        public RecordDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var channel = ChannelPicker.SelectedItem as Channel;
            var startTime = StartDatePicker.Date.Value.Date.Add(StartTimePicker.Time);
            var endTime = EndDatePicker.Date.Value.Date.Add(EndTimePicker.Time);
            //var source = await TelevisionService.Instance.GetPlaybackSource(channel, startTime, endTime);
            //NavigationService.ShellNavigation.Navigate<PlayerPage>(new Tuple<Uri, string>(source, "回看：" + channel.Name));
            this.Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private async void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            var pbStation = TelevisionService.Instance.TelevisionStations.First(station => station is IPlaybackStation) as ITelevisionStation;
            ChannelPicker.ItemsSource = (await pbStation.GetChannelList()).Select(program => program.ProgramInfo.Channel).Distinct();
            ChannelPicker.SelectedIndex = 0;
            StartDatePicker.Date = EndDatePicker.Date = DateTimeOffset.Now;
        }
    }
}
