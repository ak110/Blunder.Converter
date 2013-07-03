using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ShogiCore.Converter {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        private void Application_Startup_1(object sender, StartupEventArgs e) {
            new MainWindow(e.Args).Show();
        }
    }
}
