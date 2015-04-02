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
        Thread RefLoop;
        TextView watch;
        Button benter;
        TextView textInfo1;
        TextView textInfo2;
        TextView textInfo3;
        TextView textInfo4;
        TextView textMessage;
        ListView listView1;
        List<Reference> References;
        ListAdapter adapter;
        int MainCount;
        int FastCount;
        int MiddleTime;
        bool q;

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
            textMessage = (TextView)FindViewById(Resource.Id.txtMessage);
            listView1 = (ListView)FindViewById(Resource.Id.listView1);
            RefLoop = new Thread(new ParameterizedThreadStart(RefreshLoop));
            RefLoop.Start();
            References = new List<Reference>();
            adapter = new ListAdapter(this, References);
            listView1.Adapter = adapter;
            benter.Click += button_Click;
            q = true;
            Loop();
            UpdatingRef();
            CheckConnectionLoop();
        }
        protected override void OnResume()
        {
            base.OnResume();
        }
        async void Loop()
        {
            while (true)
            {
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                UpdateInfo();
                await Task.Delay(500);
            }
        }
        async void CheckConnectionLoop()
        {
            while (true)
            {
                    if (!SocketConnected()&&q)
                    {
                        RefLoop.Abort();
                        ShowMessage("Ошибка", "Потеряно соединение с сервером.", true);
                        q = false;
                    }
                    await Task.Delay(100);
            }
        }
        bool SocketConnected()
        {
            bool part1 = SCT.Socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (SCT.Socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        async void UpdatingRef()
        {
            while (true)
            {
                adapter = new ListAdapter(this, References);
                listView1.Adapter = adapter;
                await Task.Delay(15000);
            }
        }
        void RefreshLoop(Object ob)
        {
            while (true)
            {
                LoadConfig();
                LoadMainCount();
                LoadFastCount();
                LoadTimeWait();
                LoadReferences();
                Thread.Sleep(15000);
            }
        }

        private void LoadConfig()
        {
            SCT.Send(requests.GetCg.ToString());
            Data.SetConfig(SCT.Receive());
        }
        private void UpdateInfo()
        {
            textMessage.Text = Data.Config[Data.Size-1];
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
            References.Clear();
            for (int i = 0; i < tmp2.Length; i++)
            {
                try
                {
                    string[] tmp3 = tmp2[i].Split('_');
                    Reference r = new Reference();
                    r.Number = tmp3[1];
                    switch(tmp3[5])
                    {
                        case "1":
                            r.Status = "Не готова";
                            break;
                        case "2":
                            r.Status = "Недостаточно данных";
                            break;
                        case "3":
                            r.Status = "Готова";
                            break;
                    }
                    References.Add(r);
                }
                catch { }
            }
        }
        private void LoadTimeWait()
        {
            SCT.Send(requests.GetMT.ToString());
            try { MiddleTime = int.Parse(SCT.Receive()); }
            catch { };
        }
        private void LoadFastCount()
        {
            SCT.Send(requests.GetFC.ToString());
            try { FastCount = int.Parse(SCT.Receive()); }
            catch { };
        }
        private void LoadMainCount()
        {
            SCT.Send(requests.GetMC.ToString());
            try { MainCount = int.Parse(SCT.Receive()); }
            catch { };
        }
        void ShowMessage(string title, string message, bool exit)
        {
            dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetCancelable(false);
            dialog.SetPositiveButton("OK", delegate
            {
                if (exit)
                {
                    StartActivity(typeof(MainActivity));
                    this.Finish();
                }
            });
            dialog.Show();
        }
        void button_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(Buttons));
        }
    }
}