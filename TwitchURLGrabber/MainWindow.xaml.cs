using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TwitchURLGrabber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LightTIRCClient IRCClient;

        private ObservableCollection<URLListItem> Urls;

        public MainWindow()
        {
            InitializeComponent();

            Urls = new ObservableCollection<URLListItem>();

            URLListView.ItemsSource = Urls;

            IRCClient = new LightTIRCClient(Settings.Default.Channel, Settings.Default.User, Settings.Default.Token);
            IRCClient.Message += IRCClient_Message;
        }

        void IRCClient_Message(object source, MessageEventArgs args)
        {
            foreach (string url in ParseUrlsFromMessage(args.Message).Distinct())
            {
                URLListView.Dispatcher.Invoke(new Action(() =>
                    {
                        if (Urls.Any(u => u.URL == url))
                        {
                            Urls.Single(u => u.URL == url).TotalCount++;
                        }
                        else
                        {
                            Urls.Add(new URLListItem(url));
                        }
                    }));
            }
        }

        private IEnumerable<string> ParseUrlsFromMessage(string message)
        {
            foreach (var word in message.Split(' '))
            {
                if (word.StartsWith("http://"))
                {
                    yield return word;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IRCClient.Connect();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var link = sender as Hyperlink;
            OpenBrowser(link);
        }

        private static void OpenBrowser(Hyperlink link)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = link.NavigateUri.ToString();
            proc.Start();
        }
    }
}
