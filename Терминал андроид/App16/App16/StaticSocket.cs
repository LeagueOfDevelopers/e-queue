using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace App16
{
    public class SCT
    {
        public static bool SCTisFree { get; set; }
        public static IAsyncResult AR { get; set; }
        public static Socket Socket { get; set; }
        public static IPEndPoint IPEndPoint {get; set; }
        public static void Connecting()
        {
            try
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                AR = null;
                Socket.BeginConnect(IPEndPoint, new AsyncCallback(callback), Socket);
                SCTisFree = true;
            }
            catch { }
        }
        private static void callback(IAsyncResult r)
        {
            AR = r;
        }

        public static void Send(string msg)
        {
            try
            {
                byte[] buf1 = Encoding.Unicode.GetBytes(msg);
                msg = buf1.Length.ToString("D6");
                byte[] buf2 = Encoding.ASCII.GetBytes(msg);
                Socket.Send(buf2, 6, SocketFlags.None);
                Socket.Send(buf1, buf1.Length, SocketFlags.None);
            }
            catch { }
        }
        public static string Receive()
        {
            try
            {
                byte[] buf = new byte[6];
                int msgsize;
                int offset=0;
                bool q;
                Socket.Receive(buf, 6, SocketFlags.None);
                msgsize = int.Parse(Encoding.ASCII.GetString(buf));
                buf = new byte[msgsize];
                offset = 0;
                do
                {
                    int geted = Socket.Receive(buf, offset, msgsize, SocketFlags.None);
                    if (geted != msgsize)
                    {
                        offset = geted;
                        msgsize -= geted;
                        q = true;
                        Thread.Sleep(1000);
                    }
                    else q = false;
                } while (q);
                return Encoding.Unicode.GetString(buf);
            }
            catch { return " "; }
        }

        public static bool SocketConnected()
        {
            bool part1 = Socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (Socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }
}