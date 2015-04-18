using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Media;

using ZXing.QrCode;
using ZXing;
using ZXing.Common;

using Refractored.Xam.TTS;

namespace App16
{
    enum requests { prop1, prop2, prop3, prop4, prop5, prop6, prop7, prop8, prop9, SetCg, Refc1, Refc2, GetMa, GetMH, GetRe, GetRH, GetFa, GetFH, GetCM, GetCF, GetRF, GetCg, GetRN, GetMT, GetMC, GetFC, GetRC, GeMHC, GeFHC, GeRHC, GetNu, Updat, EOSes, enumErr }

    class StaticData
    {
        public static StatusBarVisibility Flags { get { return flags; } set { flags = value; } }
        static StatusBarVisibility flags = (StatusBarVisibility)Android.Views.SystemUiFlags.Immersive
               | (StatusBarVisibility)Android.Views.SystemUiFlags.Fullscreen
               | (StatusBarVisibility)Android.Views.SystemUiFlags.HideNavigation;

        public static int Size { get; set; }
        public static List<string> Config { get; set; }
        public StaticData(int size)
        {
            Size = size;
            Config = new List<string>();
            References = new List<ReferenceOne>();
            ReferencesOnDisplay = new List<ReferenceOne>();
        }

        //Работа с обновлением данных:
        public static Thread RefLoop { get; set; }
        public static int Number {get;set;}
        public static int MainCount {get;set;}
        public static int FastCount {get;set;}
        public static int MiddleTime { get; set; }
        public static List<ReferenceOne> References { get; set; }
        public static List<ReferenceOne> ReferencesOnDisplay { get; set; }
        public static void StartUpdating()
        {
            if (RefLoop==null || RefLoop.ThreadState != ThreadState.Running)
            {
                RefLoop = new Thread(new ParameterizedThreadStart(ThreadRefLoop));
                RefLoop.Start();
            }
        }
        public static void StopUpdating()
        {
            RefLoop.Abort();
            SCT.Send(requests.EOSes.ToString());
        }
        private static void ThreadRefLoop(Object ob)
        {
            while (true)
            {
                if (SCT.SCTisFree)
                {
                    SCT.SCTisFree = false;
                    LoadConfig();
                    LoadNumber();
                    LoadMainCount();
                    LoadFastCount();
                    LoadTimeWait();
                    LoadReferences();
                    DisplayReferences();
                    SCT.SCTisFree = true;
                    Thread.Sleep(15000);
                }
            }
        }
        private static void CheckNumber(int num)
        {
            if (Number != num)
            {
                Player.InviteNextNumber(num);
            }
        }
        private static void LoadConfig()
        {
            SCT.Send(requests.GetCg.ToString());
            SetConfig(SCT.Receive());
        }
        private static void LoadNumber()
        {
            SCT.Send(requests.GetNu.ToString());
            try
            {
                int num = int.Parse(SCT.Receive());
                CheckNumber(num);
                Number = num;
            }
            catch { };
        }
        private static void LoadMainCount()
        {
            SCT.Send(requests.GetMC.ToString());
            try { MainCount = int.Parse(SCT.Receive()); }
            catch { };
        }
        private static void LoadFastCount()
        {
            SCT.Send(requests.GetFC.ToString());
            try { FastCount = int.Parse(SCT.Receive()); }
            catch { };
        }
        private static void LoadTimeWait()
        {
            SCT.Send(requests.GetMT.ToString());
            try { MiddleTime = int.Parse(SCT.Receive()); }
            catch { };
        }
        private static void LoadReferences()
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
                    ReferenceOne r = new ReferenceOne();
                    r.Number = tmp3[1];
                    switch (int.Parse(tmp3[8]))
                    {
                        case 1:
                            r.Status = "Не готова";
                            break;
                        case 2:
                            r.Status = "Недостаточно данных";
                            break;
                        case 3:
                            r.Status = "Готова";
                            break;
                    }
                    References.Add(r);
                }
                catch { }
            }
        }
        private static void DisplayReferences()
        {
            ReferencesOnDisplay.Clear();
            foreach (ReferenceOne x in References)
            {
                if (x.Status != "Не готова")
                    ReferencesOnDisplay.Add(x);
            }
        }
        public static void SetConfig(string msg)
        {
            try
            {
                string[] a = msg.Split(';');
                List<string> tmp = new List<string>(a);
                Config = tmp;
            }
            catch { }
        }

        //транспорт переменных между activities
        public static int ChangedButton { get; set; }
        public static int ChangedNumber { get; set; }

        //создание qrcode:
        public static Bitmap QRCode { get; set; }
        public static void CreateQRCode(string content)
        {
            var qrCodeWriter = new QRCodeWriter();
            var hints = new Dictionary<EncodeHintType, Object>();
            hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            BitMatrix bm = qrCodeWriter.encode(content, ZXing.BarcodeFormat.QR_CODE, 450, 450, hints);
            QRCode = Bitmap.CreateBitmap(450, 450, Bitmap.Config.Argb8888);
            for (int i = 0; i < 450; i++)
            {
                BitArray ba = bm.getRow(i, new BitArray());
                for (int j = 0; j < 450; j++)
                    QRCode.SetPixel(i, j, ba[j] ? Color.Black : Color.White);
            }
        }
    }
}