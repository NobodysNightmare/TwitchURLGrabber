﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TwitchURLGrabber
{
    class LightTIRCClient
    {
        public event ConnectedEventHandler Connected;
        public delegate void ConnectedEventHandler(object source, EventArgs args);

        public event DisconnectedEventHandler Disconnected;
        public delegate void DisconnectedEventHandler(object source, DisconnectedEventArgs args);

        public event MessageEventHandler Message;
        public delegate void MessageEventHandler(object source, MessageEventArgs args);

        private string Channel;
        private string Username;
        private string Password;

        private Thread ReceiveThread;
        private bool KeepRunning = true;

        private string ServerAddress
        {
            get { return string.Format("{0}.jtvirc.com", Channel); }
        }

        public LightTIRCClient(string channel, string username, string password)
        {
            Channel = channel;
            Username = username;
            Password = password;
        }

        public void Connect()
        {
            ReceiveThread = new Thread(ReceiveLoop);
            ReceiveThread.IsBackground = true;
            ReceiveThread.Name = "IRC Message-Receiver";
            ReceiveThread.Start();
        }

        private void ReceiveLoop()
        {
            while (KeepRunning)
            {
                try
                {
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(new DnsEndPoint(ServerAddress, 6667));
                    using (var stream = new NetworkStream(socket))
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        stream.ReadTimeout = 360000; //worst-case read-timeout; Twitch-pings occur every 5 minutes
                        writer.WriteLine("PASS {0}", Password);
                        writer.WriteLine("NICK {0}", Username);
                        writer.Flush();
                        while (KeepRunning)
                        {
                            var line = reader.ReadLine();
                            Console.WriteLine(line);

                            if (line != null)
                            {
                                ProcessIRCCommand(writer, line);
                            }
                            else
                            {
                                OnDisconnected(true);
                                return;
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    OnDisconnected(false);
                    Thread.Sleep(2000);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }

        private void ProcessIRCCommand(StreamWriter writer, string line)
        {
            var parts = line.Split(' ');
            if (line.StartsWith(":") && parts.Count() >= 3)
            {
                switch (parts[1])
                {
                    case "001":
                        OnConnected();
                        writer.WriteLine("JOIN #{0}", Channel);
                        writer.Flush();
                        break;
                    case "PRIVMSG":
                        if (IsMyChannel(parts))
                        {
                            OnMessage(BuildUsername(parts), BuildMessage(parts));
                        }
                        break;
                }
            }
            else
            {
                if (parts.First() == "PING")
                {
                    writer.WriteLine("PONG {0}", parts.Last());
                    writer.Flush();
                }
            }
        }

        private bool IsMyChannel(string[] parts)
        {
            return parts[2] == string.Format("#{0}", Channel);
        }

        private static string BuildUsername(string[] parts)
        {
            var endIndex = parts[0].IndexOf('!');
            if (endIndex < 0)
            {
                return parts[0];
            }

            return parts[0].Substring(1, endIndex - 1);
        }

        private static string BuildMessage(string[] parts)
        {
            return string.Join(" ", parts.Skip(3).ToArray()).Substring(1);
        }

        protected void OnMessage(string user, string message)
        {
            var handler = Message;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(user, message));
            }
        }

        protected void OnConnected()
        {
            var handler = Connected;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected void OnDisconnected(bool isFinal)
        {
            var handler = Disconnected;
            if (handler != null)
            {
                handler(this, new DisconnectedEventArgs(isFinal));
            }
        }
    }
}
