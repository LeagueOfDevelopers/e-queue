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
using System.Threading;

namespace App5
{
    [Activity(Label = "DisplayingQueue", Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class DisplayingQueue : Activity
    {
        TextView watch;
        Button benter;
        TextView textInfo1;
        TextView textInfo2;
        TextView textInfo3;
        TextView textInfo4;

        int MainCount;
        int FastCount;
        int MiddleTime;
        string[,] References;

        public string[] TranslatedRequests = new string[7];

        AlertDialog.Builder dialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.WatchQueue);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            benter = (Button)FindViewById(Resource.Id.benter);
            textInfo1 = (TextView)FindViewById(Resource.Id.textInfo1);
            textInfo2 = (TextView)FindViewById(Resource.Id.textInfo2);
            textInfo3 = (TextView)FindViewById(Resource.Id.textInfo3);
            textInfo4 = (TextView)FindViewById(Resource.Id.textInfo4);
            MainCount = 0;
            FastCount = 0;
            MiddleTime = 0;
            References = new string[1, 2];
        }
        protected override void OnResume()
        {
            Loop();
            new Thread(new ParameterizedThreadStart(RefreshLoop)).Start();
            base.OnResume();
        }
        async void Loop()
        {
            while (true)
            {
                await Task.Delay(500);
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                UpdateInfo();
            }
        }
        void RefreshLoop(Object ob)
        {
            while (true)
            {
                try
                {
                    LoadMainCount();
                    LoadFastCount();
                    LoadTimeWait();
                    LoadReferences();
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
                Thread.Sleep(5000);
            }
        }

        private void UpdateInfo()
        {
            textInfo1.Text = String.Format("Всего в очереди: {0:d}", MainCount+FastCount);
            textInfo2.Text = String.Format("По длительным вопросам: {0:d}", MainCount);
            textInfo3.Text = String.Format("По коротким вопросам: {0:d}", FastCount);
            textInfo4.Text = String.Format("Среднее время приема: {0:d} мин", MiddleTime);
        }
        private void LoadReferences()
        {
            SCT.Send(requests.GetRe.ToString());
            string tmp1 = SCT.Receive();
            string[] tmp2 = tmp1.Split(';');
            if (tmp1 == " ")
                References = new string[1, 2];
            else
            {
                References = new string[tmp1.Length, 2];
                for (int i = 0; i < tmp2.Length; i++)
                {
                    string[] tmp3 = tmp2[i].Split('_');
                    References[i, 0] = tmp3[1];
                    References[i, 1] = tmp3[5];
                }
            }
        }
        private void LoadTimeWait()
        {
            SCT.Send(requests.GetMT.ToString());
            MiddleTime = int.Parse(SCT.Receive());
        }
        private void LoadFastCount()
        {
            SCT.Send(requests.GetFa.ToString());
            string tmp = SCT.Receive();
            if (tmp == " ")
                FastCount = 0;
            else MainCount = tmp.Split(';').Length;
        }
        private void LoadMainCount()
        {
            SCT.Send(requests.GetMa.ToString());
            string tmp = SCT.Receive();
            if (tmp == " ")
                MainCount = 0;
            else MainCount = tmp.Split(';').Length;
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