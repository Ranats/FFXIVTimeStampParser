using System;
using System.Collections.Generic;
using System.Linq;
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

namespace FFXIVTimeStampParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //            string[] input = new string[255];
            var prefix = prefixField.Text;
            var input = inputField.Text;
            TimeSpan startTime;
            if(!TimeSpan.TryParse(startTimeField.Text, out startTime))
            {
                startTime = TimeSpan.Parse("00:00:00");
            }
            TimeSpan adj_24 = new TimeSpan(24, 0, 0);

            outputField.Text = startTime.ToString("T");



            try
            {
                var lines = input.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                var head = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                int index = Array.IndexOf(head,"STARTTIME") + 1;

                int count = 0;
                var output = "";
                foreach (var line in lines)
                {
                    if (count == 0)
                    {
                        count += 1;
                        continue;
                    }

                    TimeSpan time = TimeSpan.Parse(line.Split(new String[] { " ", "  ", "   " }, StringSplitOptions.RemoveEmptyEntries)[index]);

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

        private void Alert(Exception e)
        {
            string messageBoxText = "Something wrong\n\n" + e.ToString();
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

    }
}
