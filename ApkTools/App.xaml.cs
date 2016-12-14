using System;

using System.Windows;

namespace ApkTools {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        public static string args;

        private void Application_Startup(object sender, StartupEventArgs e) {
            if(e.Args!=null && e.Args.Length == 1) {
                args = e.Args[0];
            }
        }

    }
}
