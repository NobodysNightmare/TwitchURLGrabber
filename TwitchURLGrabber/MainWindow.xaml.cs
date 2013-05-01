﻿using System;
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
        private int DisconnectCount;
        private int MessageCount;

        private ObservableCollection<URLListItem> Urls = new ObservableCollection<URLListItem>();
        private ObservableCollection<string> Messages = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            URLListView.ItemsSource = Urls;
            MessageList.ItemsSource = Messages;

            IRCClient = new LightTIRCClient(Settings.Default.Channel, Settings.Default.User, Settings.Default.Password);
            IRCClient.Message += IRCClient_Message;
            IRCClient.Connected += IRCClient_Connected;
            IRCClient.Disconnected += IRCClient_Disconnected;
        }

        void IRCClient_Message(object source, MessageEventArgs args)
        {
            foreach (string url in ParseUrlsFromMessage(args.Message).Distinct())
            {
                URLListView.Dispatcher.Invoke(new Action(() =>
                {
                    if (!Urls.Any(u => u.URL == url))
                    {
                        Urls.Insert(0, new URLListItem(DateTime.Now, url));
                    }

                    var item = Urls.Single(u => u.URL == url);
                    item.TotalCount++;
                    item.AddSender(args.User);
                }));
            }

            MessageCountText.Dispatcher.Invoke(new Action(() =>
            {
                MessageCountText.Text = string.Format("Messages: {0:n0}", ++MessageCount);
            }));

            MessageList.Dispatcher.Invoke(new Action(() =>
            {
                Messages.Add(string.Format("{0}: {1}", args.User, args.Message));
                if (Messages.Count > Settings.Default.MessageBufferSize)
                {
                    Messages.RemoveAt(0);
                }
            }));
        }

        void IRCClient_Connected(object sender, EventArgs e)
        {
            StatusText.Dispatcher.Invoke(new Action(() =>
            {
                StatusText.Text = string.Format("Connected since {0}", DateTime.Now.ToShortTimeString());
            }));
        }

        void IRCClient_Disconnected(object sender, EventArgs e)
        {
            StatusText.Dispatcher.Invoke(new Action(() =>
            {
                StatusText.Text = string.Format("Disconnected. Trying to reconnect...");
            }));

            DisconnectCountText.Dispatcher.Invoke(new Action(() =>
            {
                DisconnectCountText.Text = string.Format("Disconnects: {0:n0}", ++DisconnectCount);
            }));
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
            URLListView.SelectedItem = Urls.Single(u => u.URL == link.NavigateUri.ToString());
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
