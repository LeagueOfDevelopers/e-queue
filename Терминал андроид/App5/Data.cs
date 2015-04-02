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

namespace App5
{
    class Data
    {
        static string[] config;
        public static string[] Config { get { return config; } }
        public static int Size { get; set; }
        public Data(int a) { Size = a; config = new string[10]; }
        public static void SetConfig(string msg)
        {
            config = new string[Size];
            string[] a = msg.Split(';');
            try
            {
                for (int i = 0; i < Size; i++)
                    config[i] = a[i];
            }
            catch { config = new string[Size]; }
        }
    }
}