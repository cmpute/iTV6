using System;
using Windows.UI.Xaml.Controls;
using iTV6.Services;
using iTV6.Background;
using iTV6.Models;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace iTV6.Views
{
    public sealed partial class RecordDialog : ContentDialog
    {
        Channel _channel = null;
        SourceRecord _source = null;

        public RecordDialog()
        {
            this.InitializeComponent();
        }
        public RecordDialog(Channel channel, SourceRecord source) : this()
        {
            _channel = channel;
            _source = source;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var immediate = UseNow.IsChecked ?? false;
            var startTime = immediate ? DateTime.Now : StartDatePicker.Date.Value.Date.Add(StartTimePicker.Time);
            var endTime = EndDatePicker.Date.Value.Date.Add(EndTimePicker.Time);
            var span = endTime.Subtract(startTime);
            if (startTime < endTime && (immediate || startTime > DateTime.Now))
            {
                RecordService.Instance.StartRecording(_channel, _source, startTime, span);
                NavigationService.ShellNavigation.Navigate<RecordingsPage>();
                this.Hide();
            }
            else
                MessageBlock.Text = "时间选择不当，请重新选择开始时间和结束时间";
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            Text_Channel.Text = _channel.Name;
            Text_Source.Text = _source.StationName;
            StartDatePicker.Date = DateTimeOffset.Now;
            EndDatePicker.Date = DateTimeOffset.Now.Add(TimeSpan.FromMinutes(5));
        }
    }
}
