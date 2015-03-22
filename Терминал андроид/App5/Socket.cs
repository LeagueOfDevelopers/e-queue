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
        public SCT()
        {
            sct = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
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
    }
}