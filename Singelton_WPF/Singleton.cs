
namespace Singelton_WPF
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    /// <summary>
    /// Wpf 嵌入 exe界面工具类
    /// 
    /// wpf加载时，需要使用WindowsFormsHost承载Winform控件Panel，把exe窗口加载到Panel上，例如：
    /// 1、加载进程：WpfExeLoadUtil.StartAndEmbedProcess(exePath, WindowsFormsHost1, _hostPanel, ActualWidth, ActualHeight, 0.8);
    /// 2、界面刷新：
    /// protected override void OnRender(DrawingContext drawingContext)
    /// {
    ///     WpfExeLoadUtil.WindowDock(WpfExeLoadUtil._process, ActualWidth, ActualHeight, 0.8);
    ///     base.OnRender(drawingContext);
    /// }
    /// 3、关闭进程：WpfExeLoadUtil.CloseApp(WpfExeLoadUtil._process);
    /// </summary>
    public class Singleton
    {
        #region 声明调用user32.dll中的函数
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);
        #endregion

        /// <summary>
        /// 加载的游戏进程
        /// </summary>
        public Process _process;

        /// <summary>
        /// 把exe界面嵌入到Panel句柄
        /// </summary>
        /// <param name="processPath">exe进程的路径</param>
        /// <param name="windowsFormsHost">父窗口panel的父WindowsFormsHost，用来承载Winform控件Panel</param>
        /// <param name="hostPanel">父窗口panel</param>
        /// <param name="acWidth">当前窗口宽度</param>
        /// <param name="acHeight">当前窗口高度</param>
        /// <param name="widthScale">与父窗口宽的比例，默认=1，一般是0——1之间</param>
        /// <param name="heightScale">与父窗口高的比例，默认=1，一般是0——1之间</param>
        /// <returns></returns>
        public bool StartAndEmbedProcess(string processPath, WindowsFormsHost windowsFormsHost, Panel hostPanel
            , double acWidth, double acHeight, double widthScale = 1, double heightScale = 1)
        {
            bool isStartAndEmbedSuccess = false;

            if (windowsFormsHost != null)
            {
                hostPanel = new System.Windows.Forms.Panel();
                windowsFormsHost.Child = hostPanel;
            }

            _process = System.Diagnostics.Process.Start(processPath);
            //ShowWindowAsync(_process.MainWindowHandle, 0);//子窗口隐藏 

            // 确保可获得句柄
            new Task(() =>
            {
                while (true)
                {
                    if (_process.MainWindowHandle != (IntPtr)0)
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
            }).Start();

            Thread.Sleep(1000);

            isStartAndEmbedSuccess = EmbedApp(hostPanel, acWidth, acHeight, widthScale, heightScale);
            if (!isStartAndEmbedSuccess)
            {
                CloseApp();
            }

            return isStartAndEmbedSuccess;
        }

        /// <summary>
        /// 将外部进程嵌入到当前程序
        /// </summary>
        /// <param name="hostPanel">父窗口panel</param>
        /// <param name="acWidth">当前窗口宽度</param>
        /// <param name="acHeight">当前窗口高度</param>
        /// <param name="widthScale">与父窗口宽的比例，默认=1，一般是0——1之间</param>
        /// <param name="heightScale">与父窗口高的比例，默认=1，一般是0——1之间</param>
        /// <returns></returns>
        private bool EmbedApp(Panel hostPanel, double acWidth, double acHeight, double widthScale = 1, double heightScale = 1)
        {
            // 是否嵌入成功标志，用作返回值
            bool isEmbedSuccess = false;
            // 外部进程句柄
            IntPtr processHwnd = _process.MainWindowHandle;

            // 容器句柄
            IntPtr panelHwnd = hostPanel.Handle;

            if (processHwnd != (IntPtr)0 && panelHwnd != (IntPtr)0)
            {
                // 把本窗口句柄与具体窗口句柄关联
                int setTime = 0;
                while (!isEmbedSuccess && setTime < 10)
                {
                    Thread.Sleep(100);
                    setTime++;
                    isEmbedSuccess = (SetParent(processHwnd, panelHwnd) != (IntPtr)0);
                }

                // 放置exe窗口的位置，把exe窗口的标题和边框都放到容器界面的外面
                WindowDock(acWidth, acHeight, widthScale, heightScale);
            }

            return isEmbedSuccess;
        }

        /// <summary>
        /// 结束进程
        /// </summary>
        /// <returns></returns>
        public void CloseApp()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
                _process.Close();
            }
        }

        /// <summary>
        /// 把Exe进程窗口镶嵌到父窗口。
        /// 例如： 
        /// WindowDock(process, 0.8);
        /// </summary>
        /// <param name="acWidth">当前窗口宽度</param>
        /// <param name="acHeight">当前窗口高度</param>
        /// <param name="widthScale">与父窗口宽的比例，默认=1，一般是0——1之间</param>
        /// <param name="heightScale">与父窗口高的比例，默认=1，一般是0——1之间</param>
        public void WindowDock(double acWidth, double acHeight, double widthScale = 1, double heightScale = 1)
        {
            double currWS = widthScale;
            double currHS = heightScale;
            //MoveWindow(process.MainWindowHandle, -6, -30, (int)(ActualWidth * 5 / 6), (int)(ActualHeight), true);
            if (_process != null)
            {
                MoveWindow(_process.MainWindowHandle, -6, -38,
                    (int)(acWidth * currWS), (int)(acHeight * currHS), true);
            }
        }

        public static Singleton Instence { get { return lazy.Value; } }
        private readonly static Lazy<Singleton> lazy = new Lazy<Singleton>(() => new Singleton());
    }
}


