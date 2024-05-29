using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowResizer
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GetProcessList();
        }

        private void GetProcessList()
        {
            this.ProcessComboBox.Items.Clear();
            // ウィンドウタイトルがNull or Empty 以外のプロセスを取得する
            var processes = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)).ToList();
            foreach (var process in processes)
            {
                this.ProcessComboBox.Items.Add(process.MainWindowTitle);
            }
        }

        private void ProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedWindowTitle = ProcessComboBox.SelectedItem as string;
            if (selectedWindowTitle != null)
            {
                var selectedProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle == selectedWindowTitle);
                if (selectedProcess != null)
                {
                    RECT rect;
                    GetWindowRect(selectedProcess.MainWindowHandle, out rect);

                    // 一時的にスライダーのイベントを削除
                    // 理由:ドロップダウンからアイテムを選択したときにスライダーの値変更イベントも呼ばれてしまうため
                    WSlider.ValueChanged -= WSlider_ValueChanged;
                    HSlider.ValueChanged -= HSlider_ValueChanged;

                    // スライダーの値を更新
                    WSlider.Value = rect.Right - rect.Left;
                    HSlider.Value = rect.Bottom - rect.Top;

                    // イベントを再度追加
                    WSlider.ValueChanged += WSlider_ValueChanged;
                    HSlider.ValueChanged += HSlider_ValueChanged;
                }
            }
        }

        private void WSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var selectedWindowTitle = this.ProcessComboBox.Text;
            var selectedProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle == selectedWindowTitle);
            if (selectedProcess != null)
            {
                RECT rect;
                GetWindowRect(selectedProcess.MainWindowHandle, out rect);
                MoveWindow(selectedProcess.MainWindowHandle, rect.Left, rect.Top, (int)this.WSlider.Value, rect.Bottom - rect.Top, true);
            }
        }
        private void HSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var selectedWindowTitle = this.ProcessComboBox.Text;
            var selectedProcess = Process.GetProcesses().FirstOrDefault(p => p.MainWindowTitle == selectedWindowTitle);
            if (selectedProcess != null)
            {
                RECT rect;
                GetWindowRect(selectedProcess.MainWindowHandle, out rect);
                MoveWindow(selectedProcess.MainWindowHandle, rect.Left, rect.Top, rect.Right - rect.Left, (int)this.HSlider.Value, true);
            }
        }

        private void Get_p_Click(object sender, RoutedEventArgs e)
        {
            GetProcessList();
            WSlider.Value = 10;
            HSlider.Value = 10;
        }
    }
}