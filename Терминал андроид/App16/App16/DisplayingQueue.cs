using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Hardware;

namespace App16
{
    [Activity(Label = "DisplayingQueue", Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class DisplayingQueue : Activity
    {
        TextView watch;
        TextView textMessage;
        TextView textInfo0;
        TextView textInfo1;
        TextView textInfo2;
        TextView textInfo3;
        TextView textInfo4;
        ListView listView1;
        ListAdapter adapter;
        Button benter;
        bool q;//переменная для проверки соединения(отсечка)
        bool w; // переменная дляотмены перезапуска

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.WatchQueue);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            textMessage = (TextView)FindViewById(Resource.Id.txtMessage);
            textInfo0 = (TextView)FindViewById(Resource.Id.textInfo0);
            textInfo1 = (TextView)FindViewById(Resource.Id.textInfo1);
            textInfo2 = (TextView)FindViewById(Resource.Id.textInfo2);
            textInfo3 = (TextView)FindViewById(Resource.Id.textInfo3);
            textInfo4 = (TextView)FindViewById(Resource.Id.textInfo4);
            listView1 = (ListView)FindViewById(Resource.Id.listView1);
            benter = (Button)FindViewById(Resource.Id.benter); benter.Tag = 1; benter.Click += button_Click;

            q = true; w = true;

            RefreshLoop();
            UpdatingRef();
            CheckConnectionLoop();

            Window.DecorView.SystemUiVisibility = StaticData.Flags;
            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
        }

        private async void RefreshLoop()
        {
            while (true)
            {
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                UpdateInfo();
                await Task.Delay(200);
            }
        }
        private void UpdateInfo()
        {
            try
            {
                textMessage.Text = StaticData.Config.Last();
                textInfo0.Text = String.Format("{0:d}", StaticData.Number);
                textInfo1.Text = String.Format("Всего в очереди: {0:d}", StaticData.MainCount + StaticData.FastCount);
                textInfo2.Text = String.Format("По длительным вопросам: {0:d}", StaticData.MainCount);
                textInfo3.Text = String.Format("По коротким вопросам: {0:d}", StaticData.FastCount);
                textInfo4.Text = String.Format("Среднее время приема: {0:d} мин", StaticData.MiddleTime);
            }
            catch { }
        }
        private async void UpdatingRef()
        {
            while (true)
            {
                adapter = new ListAdapter(this, StaticData.ReferencesOnDisplay);
                listView1.Adapter = adapter;
                await Task.Delay(15000);
            }
        }

        private async void CheckConnectionLoop()
        {
            while (true)
            {
                    if (!SCT.SocketConnected()&&q)
                    {
                        StaticData.StopUpdating();
                        try { ShowMessage("Ошибка", "Потеряно соединение с сервером.", true); }
                        catch { }
                        await Task.Delay(3000);
                        q = false;
                        StartActivity(typeof(MainActivity));
                        this.Finish();
                    }
                    await Task.Delay(3000);
            }
        }

        private void ShowMessage(string title, string message, bool exit)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetCancelable(false);
            dialog.SetPositiveButton("OK", delegate
            {
                if (exit)
                {
                    StartActivity(typeof(MainActivity));
                    w = false;
                    this.Finish();
                }
            });
            dialog.Show();


        }

        private void button_Click(object sender, EventArgs e)
        {
            View q = (View)sender;
            switch ((int)q.Tag)
            {
                case 1:
                    w = false;
                    StartActivity(typeof(Buttons));
                    this.Finish();
                    break;
            }
        }

        public override void OnBackPressed()
        {
            Window.DecorView.SystemUiVisibility = StaticData.Flags;
        }
        private void DecorView_SystemUiVisibilityChange(object sender, View.SystemUiVisibilityChangeEventArgs e)
        {
            Thread.Sleep(50);
            Window.DecorView.SystemUiVisibility = StaticData.Flags;
        }
        protected override void OnPause()
        {
            base.OnPause();
            if (w)
            {
                Intent intent = new Intent(Intent.ActionMain);
                intent.AddCategory("EQueueLouncher");
                this.Finish();
                StaticData.StopUpdating();
                StartActivity(intent);
            }
        }
    }
}