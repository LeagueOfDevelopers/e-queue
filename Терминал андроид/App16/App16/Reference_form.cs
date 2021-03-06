using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Graphics;

namespace App16
{
    [Activity(Label = "EQueue", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class ReferenceForm : Activity
    {
        TextView watch;
        Button bexit, bsend;
        EditText t1, t2, t3, t4, t5;
        LinearLayout mainlayout;
        bool q;
        bool exit; // ���������� ��������� �����������
        Thread SleepThread;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Reference);

            watch = (TextView)FindViewById(Resource.Id.textClock); watch.SystemUiVisibility = (StatusBarVisibility)Android.Views.SystemUiFlags.HideNavigation;
            t1 = (EditText)FindViewById(Resource.Id.t1);
            t2 = (EditText)FindViewById(Resource.Id.t2);
            t3 = (EditText)FindViewById(Resource.Id.t3);
            t4 = (EditText)FindViewById(Resource.Id.t4);
            t5 = (EditText)FindViewById(Resource.Id.t5);
            bsend = (Button)FindViewById(Resource.Id.bsend); bsend.Click += button_Click; bsend.Tag = 1;
            bexit = (Button)FindViewById(Resource.Id.bexit3); bexit.Click += button_Click; bexit.Tag = 0;
            mainlayout = (LinearLayout)FindViewById(Resource.Id.mainlayout); mainlayout.Click += button_Click; mainlayout.Tag = 3;

            q = true; exit = true;

            SleepThread = new Thread(new ParameterizedThreadStart(Sleep));
            SleepThread.Start();

            RefreshLoop();
            CheckConnectionLoop();

            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
        }

        void Sleep(object ob)
        {
            Thread.Sleep(120000);
            exit = false;
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

        private void button_Click(object sender, EventArgs e)
        {
            View s = (View)sender;
            switch(int.Parse(s.Tag.ToString()))
            {
                case 1:
                    if(CheckValidation()) SendReference();
                    break;
                case 0:
                    exit = false;
                    StartActivity(typeof(Buttons));
                    this.Finish();
                    break;
            }
            SleepThread.Abort();
            SleepThread = new Thread(new ParameterizedThreadStart(Sleep));
            SleepThread.Start();
        }
        private bool CheckValidation()
        {
            if (t1.Text.Length != 6)
            { ShowMessage("������", "���������� ����� ������������� ������ ������� �� 6 ����", false); return false; }
            if (t2.Text == "" || t3.Text == "")
            { ShowMessage("������", "���� � ��� � ����� �������� ����������� ������ ���� ���������", false); return false; }
            if(t2.Text.Contains('_')||t2.Text.Contains(';')||t3.Text.Contains('_')||t3.Text.Contains(';')||t4.Text.Contains('_')||t4.Text.Contains(';')||t5.Text.Contains('_')||t5.Text.Contains(';'))
            { ShowMessage("������", "������ �� ������ ��������� ������� ';' � '_'", false);return false; }
            DateTime dt;
            if(!DateTime.TryParse(t3.Text, out dt))
            { ShowMessage("������", "����������� ������� ���� ��������", false); return false; }
            return true;
        }
        private async void SendReference()
        {
            while (true)
            {
                if (SCT.SCTisFree)
                {
                    SCT.SCTisFree = false;
                    SCT.Send(requests.Refc1.ToString());
                    SCT.Send(t1.Text);
                    StaticData.ChangedNumber = int.Parse(t1.Text);
                    SCT.Send(String.Format("{0}_{1}_{2}_{3}", t2.Text, DateTime.Parse(t3.Text).ToString("dd.MM.yyyy"), t4.Text, t5.Text));
                    if (bool.Parse(SCT.Receive()))
                    {
                        exit = false;
                        StaticData.CreateQRCode(String.Format("�� ������� ��������� ������ �� �������.\n��������� ���� ������:\n����� ������������� ������: {0}\n���: {1}\n���� ��������: {2}\n����������� �������: {3}\n�������������� ����������: {4}\n��� ��������� ���������� � ����������� ������� ������� �� ����� http://studok.misis.ru \n\n�������� ���!", t1.Text, t2.Text, t3.Text, t4.Text, t5.Text));
                        StartActivity(typeof(QRCodeDisplayingReference));
                        SCT.SCTisFree = true;
                        this.Finish();
                    }
                    else
                    {
                        SCT.SCTisFree = true;
                        ShowMessage("������", "�� ������� ��������� ��������, ��������� �������", false);
                    }
                    break;
                }
                await Task.Delay(500);
            }
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
                    exit = false;
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
            if (exit)
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