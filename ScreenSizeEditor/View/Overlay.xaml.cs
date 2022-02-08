using System;
using System.Drawing;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Input;
using VRChat_ScreenSizeEdit.Common;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Generic;

namespace VRChat_ScreenSizeEdit.View
{
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        const int ENUM_CURRENT_SETTINGS = -1;

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            //public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        public Overlay()
        {
            InitializeComponent();
            //this.Topmost = true;

            //Thread refresh = new Thread(new ThreadStart(refreshTimer));
            //refresh.IsBackground = true;
            //refresh.Start();
        }

        private void Overlay_OnLoaded(object sender, RoutedEventArgs e)
        {
            Rectangle rect = new Rectangle();
            WinApi.GetWindowRect(WinApi.GetDesktopWindow(), ref rect);

            int mostTop = 0;
            int mostLeft = 0;
            int mostRight = 0;
            int mostBottom = 0;

            //foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            //{
            //    if (mostLeft > screen.Bounds.X)
            //        mostLeft = screen.Bounds.X;
            //    if (mostTop > screen.Bounds.Y)
            //        mostTop = screen.Bounds.Y;

            //}
            //foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            //{
            //    if (mostRight < screen.Bounds.X + screen.Bounds.Width - mostLeft)
            //        mostRight = screen.Bounds.X + screen.Bounds.Width - mostLeft;
            //    if (mostBottom < screen.Bounds.Y + screen.Bounds.Height - mostTop)
            //        mostBottom = screen.Bounds.Y + screen.Bounds.Height - mostTop;
            //}
            //var a = SystemParameters.VirtualScreenHeight;


            //윈폼API를 못써서 이렇게라도 구현함.
            mostLeft = 0;
            mostTop = 0;
            mostRight = (int)System.Windows.SystemParameters.WorkArea.Width;
            mostBottom = (int)System.Windows.SystemParameters.WorkArea.Height;

            this.Top = mostTop;
            this.Left = mostLeft;
            this.Width = mostRight;
            this.Height = mostBottom;
        }

        DateTime last = DateTime.Now;

        //private void refreshTimer()
        //{
        //    while (true)
        //    {
        //        System.Drawing.Point p = new System.Drawing.Point();
        //        WinApi.GetCursorPos(ref p);
        //        try
        //        {
        //            Dispatcher.Invoke(() =>
        //            {
        //                GridLeft.Width = new GridLength(p.X - Left);
        //                GridTop.Height = new GridLength(p.Y - Top);
        //            });
        //        }
        //        catch (System.Threading.Tasks.TaskCanceledException)
        //        {
        //            return;
        //        }
        //    }
        //}

        private void Overlay_OnMouseMove(object sender, MouseEventArgs e)
        {
            //if (DateTime.Now - last <= TimeSpan.FromMilliseconds(1))
            //    return;
            //else
            //    last = DateTime.Now;
            using (var d = Dispatcher.DisableProcessing())
            {
                GridLeft.Width = new GridLength(e.GetPosition(this).X);
                GridTop.Height = new GridLength(e.GetPosition(this).Y);
            }
        }

        private void Overlay_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }


    }
}


