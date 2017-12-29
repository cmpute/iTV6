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
using Windows.UI.Popups;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace iTV6.Views
{
    public sealed partial class RecordDialog : ContentDialog
    {
        string Channel = null;
        string Source = null;
        Uri URI = null;
        public bool Completed = false;
        public RecordDialog()
        {
            this.InitializeComponent();
        }
        public RecordDialog(string channel, string source, Uri uri)
        {
            Channel = channel;
            Source = source;
            URI = uri;
            this.InitializeComponent();
        }
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var startTime = StartDatePicker.Date.Value.Date.Add(StartTimePicker.Time);
            var endTime = EndDatePicker.Date.Value.Date.Add(EndTimePicker.Time);
            if (startTime < endTime)
            {
                var folder = await RecordService.GetMyFolderAsync();
                await RecordService.Instance.CreateDownload(Channel, Source, URI, startTime, endTime);
                Completed = true;
                this.Hide();
            }
            else
            {
                MessageBlock.Text = "请重新选择开始时间和结束时间";
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Completed = true;
            this.Hide();
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            Text_Channel.Text = Channel;
            Text_Source.Text = Source;
            StartDatePicker.Date = EndDatePicker.Date = DateTimeOffset.Now;
        }
    }
}
