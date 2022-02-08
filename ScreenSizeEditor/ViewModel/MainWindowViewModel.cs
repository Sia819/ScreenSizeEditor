using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VRChat_ScreenSizeEdit.Common;
using VRChat_ScreenSizeEdit.Model;
using VRChat_ScreenSizeEdit.View;
using Point = System.Drawing.Point;

#pragma warning disable CA1822

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable RedundantNameQualifier
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming

namespace VRChat_ScreenSizeEdit.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Public Properties

        public int XAxis { get; set; }
        public int YAxis { get; set; }

        public int TimerCounted { get; set; }
        public bool IsTimerOn { get => (setWindow_Timer is null || setWindow_Timer.IsEnabled); set => _ = value; }
        public bool IsExcludeSize { get; set; }
        public Common.ObservableList<Model.ScreenSettings.Resolution> Resolutions { get; }
        public int AddRes_Height { get; set; }
        public int AddRes_Width { get; set; }
        public int List_SelectedIndex { get; set; }
        public string TargetWindowName_TextBox { get; set; }

        #endregion Public Properties

        #region Private Fields

        private int XAxis_Exclude = -1;
        private int YAxis_Exclude = -1;
        private readonly Model.ScreenSettings _settings;
        private DispatcherTimer setWindow_Timer;

        #endregion Private Fields

        public MainWindowViewModel()
        {
            _settings = new Model.ScreenSettings();
            Resolutions = _settings.Resolutions;
            Resolutions.CollectionChanged += CollectionChangedEvent; // 
            if (Resolutions.Count > 0)
            { // 해상도 리스트를 가져옵니다.
                this.XAxis = Resolutions[0].Width;
                this.YAxis = Resolutions[0].Height;
            }
            else
            { // 해상도 리스트가 비어있는 경우 1920x1080 을 기본으로 추가합니다.
                this.XAxis = 1920;
                this.YAxis = 1080;
            }

            IsExcludeSize = true;   // 타이틀바 사이즈 제외여부 기본설정
        }



        public ICommand Change_Button => new RelayCommand(() =>
        {
            var rect = new Rectangle();
            IntPtr winPtr;
            WinApi.GetWindowRect(winPtr = WinApi.FindWindow(null, "vrchat"), ref rect);
            if (winPtr == IntPtr.Zero) return;
            bool needChangeSize;
            if (IsExcludeSize)
                needChangeSize = rect.Width - rect.Left != XAxis + XAxis_Exclude || rect.Height - rect.Top != YAxis + YAxis_Exclude;
            else
                needChangeSize = rect.Width - rect.Left != XAxis || rect.Height - rect.Top != YAxis;

            // change vrchat window size
            if (!needChangeSize) return;
            if (IsExcludeSize)
            {
                if (XAxis_Exclude == -1) // checking "XAxis_Exclude" is not yet decided
                {
                    var clientSize = new Rectangle();
                    WinApi.GetClientRect(WinApi.FindWindow(null, "vrchat"), ref clientSize);
                    XAxis_Exclude = (rect.Width - rect.Left) - clientSize.Width;
                    YAxis_Exclude = (rect.Height - rect.Top) - clientSize.Height;
                }
                WinApi.SetWindowPos(WinApi.FindWindow(null, "vrchat"),
                    IntPtr.Zero,
                    0,
                    0,
                    XAxis + XAxis_Exclude,
                    YAxis + YAxis_Exclude,
                    0);
            }
            else
                WinApi.SetWindowPos(WinApi.FindWindow(null, "vrchat"), IntPtr.Zero, 0, 0, XAxis, YAxis, 0);

        });
        public ICommand Timer_Button => new RelayCommand(() =>
        {
            // TimerInit
            if (setWindow_Timer is null)
            {
                setWindow_Timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(700)
                };
                setWindow_Timer.Tick += SetWindowRefresh_Timer_Tick;          //이벤트 추가
            }

            if (setWindow_Timer.IsEnabled)
            {
                setWindow_Timer.Stop();
                RaisePropertyChanged(nameof(IsTimerOn));

            }
            else
            {
                setWindow_Timer.Start();
                RaisePropertyChanged(nameof(IsTimerOn));
            }
        });

        public ICommand TimerTextCopyPaste => new RelayCommand(() =>
        {
            Thread t1 = new Thread(new ThreadStart(() => 
            {
                Thread.Sleep(7000);
                IntPtr hWnd = WinApi.FindWindow(null, "vrchat");

                WinApi.SendMessage(hWnd, );

            }));

            t1.Start();

        });

        public ICommand Add_Button => new RelayCommand(() =>
        {
            Resolutions.Add(new Model.ScreenSettings.Resolution(AddRes_Width, AddRes_Height));
            _settings.SaveToFile();
        });

        public ICommand Remove_Button => new RelayCommand(() =>
        {
            if (List_SelectedIndex < 0) return;
            int order = List_SelectedIndex;
            Resolutions.RemoveAt(order);
            _settings.SaveToFile();
            if (Resolutions.Count > order)
                List_SelectedIndex = order;
            else
                List_SelectedIndex = order - 1;
        });

        public ICommand Up_Button => new RelayCommand(() =>
        {
            if (List_SelectedIndex <= 0) return;
            int order = List_SelectedIndex;
            var temp = Resolutions[order];
            Resolutions.RemoveAt(order);
            Resolutions.Insert(order - 1, temp);
            _settings.SaveToFile();
            List_SelectedIndex = order - 1;
        });

        public ICommand Down_Button => new RelayCommand(() =>
        {
            if (List_SelectedIndex < 0) return;
            if (Resolutions.Count <= List_SelectedIndex + 1) return;
            int order = List_SelectedIndex;
            var temp = Resolutions[order];
            Resolutions.RemoveAt(order);
            Resolutions.Insert(order + 1, temp);
            _settings.SaveToFile();
            List_SelectedIndex = order + 1;
        });
        public ICommand Debug_Button => new RelayCommand(() =>
        {
            var rect = new Rectangle();
            WinApi.GetClientRect(WinApi.FindWindow(null, "vrchat"), ref rect);
            // 가로 + 16, 세로 + 39
        });
        public ICommand FindWindow_Button => new RelayCommand(() =>
        {
            Overlay overlayWindow = new();
            overlayWindow.ShowDialog();
        });

        #region Private Function

        private void SetWindowRefresh_Timer_Tick(object sender, EventArgs e)
        {
            TimerCounted += 1;
            Change_Button.Execute(null);
        }
        private void CollectionChangedEvent(object o, EventArgs e)
        {
            if (o is not ObservableList<ScreenSettings.Resolution> a || a[0].Width == XAxis ||
                YAxis == a[0].Height) return;
            XAxis = a[0].Width;
            YAxis = a[0].Height;
        }

        #endregion
    }
}









/*
 * 코드수정으로 인해 사용되지않는 Private Fields
   //private DispatcherTimer findWindow_Timer;
   //private IntPtr _lastFoundWindow_hWnd;
   //private IntPtr VRChat_Handle;
   //private Model.ScreenSettings settings = new Model.ScreenSettings();
 */

/*
 * FindWindow의 타이머 메커니즘 변경
private void FindWindow_Button_Command()
{
    if (findWindow_Timer is null)
    {
        findWindow_Timer = new DispatcherTimer();
        findWindow_Timer.Interval = TimeSpan.FromMilliseconds(1);
        findWindow_Timer.Tick += new EventHandler(FindWindow_Timer_Tick);
    }

    if (findWindow_Timer.IsEnabled)
    {
        findWindow_Timer.Stop();
    }
    else
    {
        findWindow_Timer.Start();
    }
}
*/

/*
 * FindWindow 타이머가 발생될 경우, WindowDC를 가져와 마우스 아래에 있는 윈도우에 대해 Rectangle을 
private void FindWindow_Timer_Tick(object sender, EventArgs e)
{
    Point point = new Point();
    Rectangle rect = new Rectangle();

    WinApi.GetCursorPos(ref point);                     // 마우스 포인터의 좌표를 가져옵니다.
    IntPtr mouseFocused_hWnd = WinApi.WindowFromPoint(point);  // 마우스 아래에 있는 윈도우 hWnd를 가져옵니다.

    #region 현재 탐색된 창이 유효한 창인지 체크
    if (mouseFocused_hWnd == WinApi.NULL) return;          // 창은 NULL이 아니어야 합니다.
    if (WinApi.IsWindow(mouseFocused_hWnd) == false) return;      // 또한 OS에 관한 한 유효한 창이어야 합니다.
    if (mouseFocused_hWnd == _lastFoundWindow_hWnd) return;  // 창이 이전에 발견한 창과 동일한지 확인.
    if (mouseFocused_hWnd == System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle) return; // 또한 기본 창 자체가 아니어야 합니다.
                                                                                                     // TODO : MainWindow의 자식 윈도우의 핸들이 아니어야 합니다.
    #endregion

    // 이전에 발견된 창이 있는 경우 자체 새로 고침을 지시해야 합니다.
    // 이것은 우리가 그린 하이라이트 효과를 제거하기 위해 수행됩니다.
    //if (_lastFoundWindow_hWnd != WinApi.NULL)
    {
        WinApi.InvalidateRect(_lastFoundWindow_hWnd, WinApi.NULL, true);
        WinApi.UpdateWindow(_lastFoundWindow_hWnd);
        WinApi.RedrawWindow(_lastFoundWindow_hWnd, null, WinApi.NULL, WinApi.RDW_FRAME | WinApi.RDW_INVALIDATE | WinApi.RDW_UPDATENOW | WinApi.RDW_ALLCHILDREN);
    }

    _lastFoundWindow_hWnd = mouseFocused_hWnd;

    WinApi.GetWindowRect(mouseFocused_hWnd, ref rect); // (가져온) 윈도우의 크기를 가져옵니다.
    //IntPtr foundWindowDC = WinApi.GetWindowDC(mouseFocused_hWnd);
    IntPtr foundWindowDC = WinApi.GetDC(IntPtr.Zero);  // (그리기 위한) 글로벌 윈도우의 DC를 가져옵니다.

    if (foundWindowDC == WinApi.NULL) return;

    //  using (Graphics g = Graphics.FromHdc(foundWindowDC))
    //  {
    //      //SolidBrush b = new SolidBrush(Color.FromArgb(20, 0, 0, 0));
    //      //g.FillRectangle(b, 0, 0, 1920, 1080);
    //  }

    {
        IntPtr hPreviewPen;   // 찾은 창의 DC에 있는 기존 펜의 핸들입니다.
        IntPtr hPreviewBrush; // 찾은 창의 DC에 있는 기존 브러시의 핸들입니다.

        IntPtr g_hRectanglePen = WinApi.CreatePen(WinApi.PenStyle.PS_SOLID, 3, WinApi.RGB(255, 0, 0));  // 생성된 펜을 DC에 선택하고 이전 펜을 백업합니다.

        hPreviewPen = WinApi.SelectObject(foundWindowDC, g_hRectanglePen);
        hPreviewBrush = WinApi.SelectObject(foundWindowDC, WinApi.GetStockObject(WinApi.StockObjects.HOLLOW_BRUSH));      // DC에 투명 브러시를 선택하고 이전 브러시를 백업합니다.
        //WinApi.Rectangle(foundWindowDC, 0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top); // 찾은 창의 전체 창 영역을 덮는 DC에 직사각형을 그립니다.
        WinApi.Rectangle(foundWindowDC, rect.X, rect.Y, rect.Right - rect.X, rect.Bottom - rect.Y); // 찾은 창의 전체 창 영역을 덮는 DC에 직사각형을 그립니다.
        WinApi.SelectObject(foundWindowDC, hPreviewPen);                                       // 찾은 창의 DC에 이전 펜과 브러시를 다시 삽입합니다.
        WinApi.SelectObject(foundWindowDC, hPreviewBrush);
    }


    WinApi.ReleaseDC(mouseFocused_hWnd, foundWindowDC);
}
*/