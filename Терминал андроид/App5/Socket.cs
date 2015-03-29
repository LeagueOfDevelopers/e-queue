using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace App5
{
    public class SCT
    {
        static IAsyncResult ar;
        static Socket sct;
        public static IAsyncResult AR
        {
            get { return ar; }
        }
        public static IPEndPoint IPEndPoint
        {
            get;
            set;
        }
        public static Socket Socket
        {
            get { return sct; }
        }
        public static void Connecting()
        {
            try
            {
                sct = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ar = null;
                sct.BeginConnect(IPEndPoint, new AsyncCallback(callback), sct);
            }
            catch 
            {
                Thread.Sleep(5000);
            }
        }
        static void callback(IAsyncResult r)
        {
            ar = r;
        }

        public static void Send(string msg)
        {
            try
            {
                byte[] buf1 = Encoding.Unicode.GetBytes(msg);
                msg = buf1.Length.ToString("D6");
                byte[] buf2 = Encoding.ASCII.GetBytes(msg);
                sct.Send(buf2, 6, SocketFlags.None);
                sct.Send(buf1, buf1.Length, SocketFlags.None);
            }
            catch { }
        }
        public static string Receive()
        {
            try
            {
                byte[] buf = new byte[6];
                sct.Receive(buf, 6, SocketFlags.None);
                int msgsize = int.Parse(Encoding.ASCII.GetString(buf));
                buf = new byte[msgsize];
                int offset = 0;
                bool q;
                do
                {
                    int geted = sct.Receive(buf, offset, msgsize, SocketFlags.None);
                    if (geted != msgsize)
                    {
                        offset = geted;
                        msgsize -= geted;
                        q = true;
                    }
                    else q = false;
                } while (q);
                return Encoding.Unicode.GetString(buf);
            }
            catch { return " "; }
        }
    }
}