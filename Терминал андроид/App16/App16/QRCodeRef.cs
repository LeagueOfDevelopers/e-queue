using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Graphics;
using System.Threading;

namespace App16
{
    [Activity(Label = "EQueue", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class QRCodeDisplayingReference : Activity
    {
        TextView watch;
        TextView number;
        ImageView qrcode;
        TextView internetAddress;
        Button bexit;
        bool q;
        bool w; // ���������� ��������� �����������
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.QRCodeRef);

            watch = (TextView)FindViewById(Resource.Id.textClock); watch.SystemUiVisibility = (StatusBarVisibility)Android.Views.SystemUiFlags.HideNavigation;
            number = (TextView)FindViewById(Resource.Id.textView8); number.Text = StaticData.ChangedNumber.ToString();
            qrcode = (ImageView)FindViewById(Resource.Id.imageqrcode); qrcode.SetImageBitmap(StaticData.QRCode);
            bexit = (Button)FindViewById(Resource.Id.bexit3); bexit.Click += bexit_Click;
            internetAddress = (TextView)FindViewById(Resource.Id.textView10); internetAddress.Text = String.Format("��������� ���������� ������� �� ����� http://studok.misis.ru/{0:d} ", StaticData.ChangedNumber);

            q = true; w = true;

            Sleep();

            RefreshLoop();
            CheckConnectionLoop();

            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
        }

        private async void Sleep()
        {
            await Task.Delay(30000);
            w = false;
            StartActivity(typeof(DisplayingQueue));
            this.Finish();
        }

        private async void RefreshLoop()
        {
            while (true)
            {
                watch.Text = "�����:\n" + DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(200);
            }
        }

        private void bexit_Click(object sender, EventArgs e)
        {
            w = false;
            StartActivity(typeof(DisplayingQueue));
            this.Finish();
        }

        private async void CheckConnectionLoop()
        {
            while (true)
            {
                if (!SCT.SocketConnected() && q)
                {
                    StaticData.StopUpdating();
                    try { ShowMessage("������", "�������� ���������� � ��������.", true); }
                    catch { }
                    q = false;
                }
                await Task.Delay(100);
            }
        }

        private void ShowMessage(string title, string message, bool exit)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
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