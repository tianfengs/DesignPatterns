using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Singelton_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 创建WpfExeLoadUtil实例
        /// </summary>
        Singleton wpfUtil = Singleton.Instence;

        public MainWindow()
        {
            InitializeComponent();

            // 1. 加载游戏到Panel
            //string exePath = @"Debug.win32\helloworld.exe";
            string exePath = "notepad.exe";
            wpfUtil.StartAndEmbedProcess(exePath, WindowsFormsHost1, _hostPanel, ActualWidth, ActualHeight);
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            // 2. 刷新界面，监控缩放，自适应整个Panel载体
            wpfUtil.WindowDock(ActualWidth, ActualHeight, 1.1, 1.25);
            base.OnRender(drawingContext);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 3. 关闭进程
            wpfUtil.CloseApp();
        }
    }
}
