using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

namespace App16
{
    [Activity(Label = "EQueue", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class InputingNumberFastRef : Activity
    {
        TextView watch;
        TextView message;
        EditText number;
        Button bEnterQueue;
        Button bexit;
        LinearLayout mainlayout;
        bool q, q2;//������ ��� �������� ����������, ������ ��� ���������� ���������� ����
        bool exit; // ���������� ��������� �����������
        string numstr;
        Thread SleepThread;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.InputingNumberFast);

            watch = (TextView)FindViewById(Resource.Id.textClock); watch.SystemUiVisibility = (StatusBarVisibility)Android.Views.SystemUiFlags.HideNavigation;
            message = (TextView)FindViewById(Resource.Id.txtMessage); message.Text = "������� ����� ������������� ������, ��������� ��� ������ �������";
            number = (EditText)FindViewById(Resource.Id.text_number);
            bEnterQueue = (Button)FindViewById(Resource.Id.bentque); bEnterQueue.Click += button_Click; bEnterQueue.Tag = 2; bEnterQueue.Text = "������� �������";
            bexit = (Button)FindViewById(Resource.Id.bexit2); bexit.Click += button_Click; bexit.Tag = 0;
            mainlayout = (LinearLayout)FindViewById(Resource.Id.mainlayout); mainlayout.Click += button_Click; mainlayout.Tag = 3;

            q = true; q2 = false; exit = true;

            SleepThread = new Thread(new ParameterizedThreadStart(Sleep));
            SleepThread.Start();

            RefreshLoop();
            CheckConnectionLoop();

            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
        }

        private void Sleep(object ob)
        {
            Thread.Sleep(30000);
            exit = false;
            this.Finish();
        }

        private async void RefreshLoop()
        {
            while (true)
            {
                watch.Text = "�����:\n" + DateTime.Now.ToString("HH:mm:ss");
                if (q2) { number.Text = numstr; q2 = false; }
                await Task.Delay(200);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            View a = (View)sender;
            switch((int)a.Tag)
            {
                case 2:
                    if (checkpass())
                        SendPurpose();
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
        private bool checkpass()
        {
            if (long.Parse(number.Text) == 8564431895)
            {
                exit = false;
                this.Finish();
                StaticData.StopUpdating();
                return false;
            } else return true;
        }
        private async void SendPurpose()
        {
            while (true)
                if (SCT.SCTisFree)
                {
                    if(number.Text.Length!=6){numstr = ""; q2 = true; ShowMessage("������", "���������� ����� ������������� ������ ������� �� 6 ����", false);}
                    else if (FindRef(true))
                    {
                        SCT.SCTisFree = false;
                        SCT.Send("prop" + StaticData.ChangedButton);
                        SCT.Send(number.Text);
                        StaticData.ChangedNumber = int.Parse(number.Text);
                        if (bool.Parse(SCT.Receive()))
                        {
                            exit = false;
                            StaticData.CreateQRCode(String.Format("�� ������� ������ � ������� �� �������� ��������, ��� �����: {0:s}.\n��� ��������� ���������� � ����������� ������� ������� �� ����� http://studok.misis.ru \n\n�������� ���!", number.Text));
                            StartActivity(typeof(QRCodeDisplaying));
                            SCT.SCTisFree = true;
                            this.Finish();
                        }
                        else
                        {
                            SCT.SCTisFree = true;
                            ShowMessage("������", "�� ������� ��������� ��������, ��������� �������", false);
                        }
                    }
                    else if (FindRef(false)) { numstr = ""; q2 = true; ShowMessage("������� �� ������", "������� � ������ ������� ���� �� ������. ��������� ������ ���������� �����.", false);}
                    else { numstr = ""; q2 = true; ShowMessage("������� �� ������", "������� � ������ ������� ����������� � ����. ���������� �������� ������ ��� ���.", false); }
                    break;
                } await Task.Delay(500);
        }
        private bool FindRef(bool onDisplay)
        {
            bool q = false;
            if (onDisplay)
            {
                for (int i = 0; i < StaticData.ReferencesOnDisplay.Count; i++)
                    try
                    {
                        if (int.Parse(StaticData.ReferencesOnDisplay[i].Number) == int.Parse(number.Text))
                        {
                            q = true;
                            break;
                        }
                    }
                    catch { }
                return q;
            }
            else
            {
                for (int i = 0; i < StaticData.References.Count; i++)
                    try
                    {
                        if (int.Parse(StaticData.References[i].Number) == int.Parse(number.Text))
                        {
                            q = true;
                            break;
                        }
                    }
                    catch { }
                return q;
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