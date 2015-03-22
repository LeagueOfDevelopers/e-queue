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
using Android.Content.PM;
using System.Threading.Tasks;

namespace App5
{
    [Activity(Label = "DisplayingQueue", Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class DisplayingQueue : Activity
    {
        Socket Socket;
        TextView watch;
        TextView Counter1;
        TextView Counter2;
        Button benter;
        ListView MainList;
        ListView SecondList;
        public List<client> Base
        {
            get;
            set;
        }
        public string[] TranslatedRequests = new string[7];

        AlertDialog.Builder dialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.WatchQueue);

            Socket = SCT.Socket;
            Base = new List<client>();

            watch = (TextView)FindViewById(Resource.Id.textClock);
            benter = (Button)FindViewById(Resource.Id.benter);
            MainList = (ListView)FindViewById(Resource.Id.mainlist);
            SecondList = (ListView)FindViewById(Resource.Id.secondlist);
            Counter1 = (TextView)FindViewById(Resource.Id.counter1);
            Counter2 = (TextView)FindViewById(Resource.Id.counter2);
        }
        protected override void OnResume()
        {
            Loop();
            RefreshLoop();
            base.OnResume();
        }
        async void Loop()
        {
            while (true)
            {
                await Task.Delay(1000);
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
            }
        }
        async void RefreshLoop()
        {
            while (true)
            {
                await Task.Delay(5000);
                bool q = false;
                try
                {
                    LoadCg();
                    LoadBD();
                }
                catch (SocketException)
                { 
                    dialog = new AlertDialog.Builder(this);
                    dialog.SetTitle("Connection");
                    dialog.SetMessage("Соединение потеряно");
                    dialog.SetPositiveButton("OK", delegate
                    {
                        StartActivity(typeof(MainActivity));
                        this.Finish();
                    });
                    dialog.Show();
                    break;
                }
            }
        }
        void LoadCg()
        {
            string str = requests.GetCg.ToString();
            byte[] buf = Encoding.ASCII.GetBytes(str);
            Socket.Send(buf);
            buf = new byte[6];
            Socket.Receive(new byte[1], 1, SocketFlags.None);
            Socket.Receive(buf, 6, SocketFlags.None);
            int msgsize = int.Parse(Encoding.ASCII.GetString(buf, 0, 6));
            buf = new byte[msgsize];
            int offset = 0;
            bool q;
            do
            {
                int geted = Socket.Receive(buf, offset, msgsize, SocketFlags.None);
                if (geted != msgsize)
                {
                    offset = geted;
                    msgsize -= geted;
                    q = true;
                }
                else q = false;
            } while (q);
            str = Encoding.Unicode.GetString(buf);
            TranslatedRequests = str.Split(';');
        }
        void LoadBD()
        {
            Base.Clear();
            string str = requests.GetBD.ToString();
            byte[] buf = Encoding.ASCII.GetBytes(str);
            Socket.Send(buf);
            buf = new byte[6];
            Socket.Receive(new byte[1], 1, SocketFlags.None);
            Socket.Receive(buf, 6, SocketFlags.None);
            str = Encoding.ASCII.GetString(buf, 0, 6);
            int msgsize = int.Parse(str);
            buf = new byte[msgsize];
            int offset = 0;
            bool q;
            do
            {
                int geted = Socket.Receive(buf, offset, msgsize, SocketFlags.None);
                if (geted != msgsize)
                {
                    offset = geted;
                    msgsize -= geted;
                    q = true;
                }
                else q = false;
            } while (q);
            str = Encoding.ASCII.GetString(buf, 0, buf.Length);
            string[] ClientBuf = str.Split(';');
            for (int i = 0; i < ClientBuf.Length; i++)
            {
                try
                {
                    string[] buf2 = ClientBuf[i].Split('_');
                    int n = int.Parse(buf2[0]);
                    string p = TranslatingReq(buf2[1]);
                    DateTime d = DateTime.Parse(buf2[2]);
                    Base.Add(new client(n, p, d));
                }
                catch { }
            }
            Counter1.Text = "Всего: " + Base.Count.ToString();
        }
        string TranslatingReq(string p)
        {
            switch (ParseReq(p))
            {
                case requests.prop1:
                    return TranslatedRequests[0];
                case requests.prop2:
                    return TranslatedRequests[1];
                case requests.prop3:
                    return TranslatedRequests[2];
                case requests.prop4:
                    return TranslatedRequests[3];
                case requests.prop5:
                    return TranslatedRequests[4];
                case requests.prop6:
                    return TranslatedRequests[5];
                default:
                    return "";
            }
        }
        requests ParseReq(string value)// Parse из string в request
        {
            try
            {
                return (requests)Enum.Parse(typeof(requests), value);
            }
            catch { return requests.enumErr; }
        }
        void ShowMessage(string title, string message, bool exit)
        {
            dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetPositiveButton("OK", delegate
            {
                if (exit)
                    this.Finish();
            });
            dialog.Show();
        }
    }
    public class client
    {
        int num;
        string purpose;
        DateTime timeOfEnter;
        public client(int n, string p, DateTime E)
        {
            num = n;
            purpose = p;
            timeOfEnter = E;
        }
        public int Number
        {
            get { return num; }
        }
        public string Purpose
        {
            get { return purpose; }
        }
        public DateTime TimeOfEnter
        {
            get { return timeOfEnter; }
        }
        public string ToString()
        {
            return String.Format("{0,5:d}_{1,10:s}_{2}", Number, Purpose, TimeOfEnter);
        }

    }
}