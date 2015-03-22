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

namespace App5
{
    public class SCT
    {
        
        static Socket Sock;
        public SCT(Socket sct)
        {
            Sock = sct;
        }
        public static Socket GetSock()
        {
                return Sock;
        }
    }
}