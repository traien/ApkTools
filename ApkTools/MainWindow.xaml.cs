using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ApkTools {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private delegate void ApkInfoDelegate(ApkInfo info);
        private delegate void InstallDelegate(string info);

        BitmapImage bitImg;
        string apkFile;

        public MainWindow() {
            InitializeComponent();

            if (App.args != null) {
                apkFile = App.args;
                RefreshApkInfo();
            } else {
                ScreenCap();
            }
        }

        void ScreenCap() {
            _refresh.IsEnabled = false;
            new Thread(new ThreadStart(AdbScreenCap)).Start();
        }

        void AdbScreenCap() {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "screencap.bat";
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            Process.Start(pi).WaitForExit();

            Dispatcher.Invoke(RefreshImage);
        }

        void RefreshImage() {
            bitImg = GetImage("screen.png");
            _img.Source = bitImg;
            _refresh.IsEnabled = true;
        }

        void CopyToClipBoard() {
            if (bitImg != null)
                Clipboard.SetImage(bitImg);
        }

        void RefreshApkInfo() {
            new Thread(new ThreadStart(ApkInfoThread)).Start();
        }

        void ApkInfoThread() {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "cmd.exe";
            pi.Arguments = "/c aapt.exe d badging " + apkFile + " > dump.txt";
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            pi.RedirectStandardOutput = false;
            Process p = Process.Start(pi);
            p.WaitForExit();
            ApkInfo apkInfo = new ApkInfo();
            using (StreamReader sr = new StreamReader("dump.txt")) {
                string line = null;
                while ((line = sr.ReadLine()) != null) {
                    apkInfo.Parse(line);
                }
            }

            // unzip apk
            if (Directory.Exists("apk"))
                Directory.Delete("apk", true);
            pi = new ProcessStartInfo();
            pi.FileName = "unzip.bat";
            pi.Arguments = apkFile;
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            pi.RedirectStandardOutput = true;
            p = Process.Start(pi);
            p.WaitForExit();

            //get sign
            if (Directory.Exists("apk")) {
                pi = new ProcessStartInfo();
                pi.FileName = "getsign.bat";
                pi.UseShellExecute = false;
                pi.CreateNoWindow = true;
                pi.RedirectStandardOutput = true;
                p = Process.Start(pi);
                apkInfo.sign = "";
                for (int i = 0; i < 3; i++)
                    apkInfo.sign += p.StandardOutput.ReadLine() + "\n";
                p.WaitForExit();
            }

            Dispatcher.Invoke((ApkInfoDelegate)GetInfoComplete, apkInfo);
        }

        void GetInfoComplete(ApkInfo info) {
            _name.Content = info.appName;
            _version.Content = info.versionName;
            _sign.Text = info.sign;
            BitmapImage bi = GetImage("apk\\" + info.icon);
            _icon.Source = bi;
        }

        BitmapImage GetImage(string path) {
            byte[] byteArray = File.ReadAllBytes(path);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(byteArray);
            bi.EndInit();
            return bi;
        }

        void InstallApk() {
            _install.IsEnabled = false;
            _installInfo.Text = "";
            new Thread(new ThreadStart(InstallThread)).Start();
        }

        void InstallThread() {
            if (File.Exists("install.apk"))
                File.Delete("install.apk");
            File.Copy(apkFile, Environment.CurrentDirectory + "\\install.apk");
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "install.bat";
            pi.Arguments = "install.apk";
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            pi.RedirectStandardOutput = true;
            Process p = Process.Start(pi);
            string str = null;
            while ((str = p.StandardOutput.ReadLine()) != null)
                Dispatcher.Invoke((InstallDelegate)InstallProgress, str + "\n");
            p.WaitForExit();

            Dispatcher.Invoke(InstallComplete);
        }

        void InstallProgress(string str) {
            _installInfo.Text += str;
            _scorllviewer.ScrollToBottom();
        }

        void InstallComplete() {
            _install.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (sender == _refresh) {
                ScreenCap();
            } else if (sender == _copy) {
                CopyToClipBoard();
            } else if (sender == _install) {
                InstallApk();
            }
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                apkFile = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                RefreshApkInfo();
            }
        }
    }
}
