using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace FFXIVTimeStampParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FetchMode fetch_mode;
        private string api_key;

        public MainWindow()
        {
            InitializeComponent();
            fetch_mode = FetchMode.api;

            var parser = new IniParser.FileIniDataParser();
            var data = parser.ReadFile("settings.ini");
            api_key = data.Global["API_KEY"];
            Textbox_api.Text = api_key;
        }

        private enum FetchMode
        {
            api,
            directly
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //            string[] input = new string[255];
            var prefix = prefixField.Text;
            var input = inputField.Text;
            TimeSpan? startTime = new TimeSpan();
            TimeSpan adj_24 = new TimeSpan(24, 0, 0);

            /// 配信開始時刻の設定
            startTime = SetStartTime(fetch_mode);
            if (startTime == null) { return; }


            /// 時刻を読み込み配信開始時刻との差分を計算
            try
            {
                var lines = input.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                var head = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                int index = Array.IndexOf(head, "STARTTIME") + 1;

                int count = 0;
                var output = "";
                foreach (var line in lines)
                {
                    if (count == 0)
                    {
                        count += 1;
                        continue;
                    }

                    TimeSpan? time = TimeSpan.Parse(line.Split(new String[] { " ", "  ", "   " }, StringSplitOptions.RemoveEmptyEntries)[index]);

                    time -= startTime;
                    if (time < TimeSpan.Zero)
                    {
                        time += adj_24;
                    }

                    output += String.Format("{0}  {1}{2,3}", time, prefix, count);
                    output += "\r\n";
                    count += 1;
                }

                outputField.Text = output;

            }
            catch (Exception exception)
            {
                Alert(exception);
            }
        }

        private TimeSpan? SetStartTime(FetchMode mode)
        {
            if (fetch_mode == FetchMode.api)
            {
                try
                {
                    var api_key = Textbox_api.Text;
                    var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = api_key
                    });

                    var video_id = Textbox_videoid.Text;
                    VideosResource.ListRequest request = youtubeService.Videos.List("liveStreamingDetails");
                    request.Id = video_id;

                    var response = request.Execute();
                    DateTime actual_startTime = (DateTime)response.Items.First().LiveStreamingDetails.ActualStartTime;
                    return TimeSpan.Parse(actual_startTime.ToString("T"));
                }
                catch (Exception exception)
                {
                    if (exception.Message.Contains("API key not valid."))
                    {
                        Alert(exception, "Google.Apis.Requests.RequestError\r\nAPI key not valid. Please pass a valid API key. [400]");
                    }
                    else if (exception.Message == "Sequence contains no elements")
                    {
                        Alert(exception, "video not found.");
                    }
                    else
                    {
                        Alert(exception);
                    }
                    return null;
                }
            }
            else
            {
                TimeSpan st;
                if (!TimeSpan.TryParse(startTimeField.Text, out st))
                {
                    st = TimeSpan.Parse("00:00:00");
                }
                return st;
            }
        }

        private void Alert(Exception e = null, string text = "Something wrong")
        {
            string messageBoxText = text + "\n\n raw message\n----------\n" + e.ToString();
            string caption = "FFXIVTimeStampParser";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            inputField.Text = "STARTTIME   TITLE\n2021 / 09 / 10  --:--:--  ALEXANDER";
            outputField.Text = "--:--:--  Try 1";
        }

        private void Radio_streamid_Checked(object sender, RoutedEventArgs e)
        {
            fetch_mode = FetchMode.api;
            Label_api.IsEnabled = true;
            Textbox_api.IsEnabled = true;
            Label_videoid.IsEnabled = true;
            Textbox_videoid.IsEnabled = true;

            Label_streamstarts.IsEnabled = false;
            startTimeField.IsEnabled = false;
        }

        private void Radio_startstime_Checked(object sender, RoutedEventArgs e)
        {
            fetch_mode = FetchMode.directly;
            Label_api.IsEnabled = false;
            Textbox_api.IsEnabled = false;
            Label_videoid.IsEnabled = false;
            Textbox_videoid.IsEnabled = false;

            Label_streamstarts.IsEnabled = true;
            startTimeField.IsEnabled = true;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }
    }
}
