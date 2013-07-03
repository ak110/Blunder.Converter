using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShogiCore.Notation;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace ShogiCore.Converter {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow(string[] args) {
            InitializeComponent();
            // フォルダ指定時は初期設定する
            if (args.Length == 1 && System.IO.Directory.Exists(args[0])) {
                textBoxSrc.Text = System.IO.Path.GetFullPath(args[0]);
            }
        }

        private void Window_Closed_1(object sender, EventArgs e) {

        }

        private void Window_DragOver_1(object sender, DragEventArgs e) {
            textBoxSrc_DragOver(sender, e);
        }

        private void Window_Drop_1(object sender, DragEventArgs e) {
            textBoxSrc_Drop(sender, e);
        }

        private void textBoxSrc_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                if (System.IO.Directory.Exists(((string[])e.Data.GetData(DataFormats.FileDrop)).FirstOrDefault())) {
                    e.Effects = DragDropEffects.Copy;
                }
            }
            e.Handled = true;
        }

        private void textBoxSrc_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                textBoxSrc.Text = ((string[])e.Data.GetData(DataFormats.FileDrop)).FirstOrDefault();
            }
            e.Handled = true;
        }

        private void textBoxSrc_TextChanged(object sender, TextChangedEventArgs e) {
            string path = System.IO.Path.GetFullPath(textBoxSrc.Text);
            if (textBoxSrc.Text != path) {
                textBoxSrc.Text = path;
            }
            textBoxDst.Text = path + "_変換先";
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e) {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.ShowNewFolderButton = false;
                if (System.IO.Directory.Exists(textBoxSrc.Text)) {
                    dialog.SelectedPath = textBoxSrc.Text;
                }
                dialog.Description = "変換元フォルダの選択";
                System.Windows.Forms.NativeWindow nativeWindow = new System.Windows.Forms.NativeWindow();
                nativeWindow.AssignHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                if (dialog.ShowDialog(nativeWindow) == System.Windows.Forms.DialogResult.OK) {
                    textBoxSrc.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// 変換
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, RoutedEventArgs e) {
            if (radioButtonSFEN.IsChecked.Value) {
                ConvertTo(new SFENNotationWriter(), ".sfen");
            } else if (radioButtonKIF.IsChecked.Value) {
                ConvertTo(new KifuNotationWriter(KifuNotationWriter.Mode.KIF), ".kif");
            } else {
                ConvertTo(new PCLNotationWriter(), ".csa");
            }
        }

        /// <summary>
        /// 棋譜変換
        /// </summary>
        private void ConvertTo(IStringNotationWriter notationWriter, string extension) {
            string srcDir = System.IO.Path.GetFullPath(textBoxSrc.Text);
            string dstDir = System.IO.Path.GetFullPath(textBoxDst.Text);

            textBoxLog.Clear();
            WriteLog("変換を開始します。");

            // 変換処理
            NotationLoader loader = new NotationLoader();
            foreach (string srcFile in System.IO.Directory.GetFiles(srcDir, "*", System.IO.SearchOption.AllDirectories)) {
                string dstFile = srcFile.Replace(srcDir, dstDir);
                if (srcFile == dstFile) continue;

                dstFile = System.IO.Path.ChangeExtension(dstFile, extension);

                try {
                    string str = System.IO.File.ReadAllText(srcFile, Encoding.Default);
                    string data = notationWriter.WriteToString(loader.Load(str));
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dstFile));
                    System.IO.File.WriteAllText(dstFile, data, Encoding.Default);
                    WriteLog(srcFile.Substring(srcDir.Length + 1) + ": 変換成功。");
                } catch (Exception e) {
                    logger.Warn("棋譜読み込み失敗: " + srcFile, e);
                    WriteLog(srcFile.Substring(srcDir.Length + 1) + ": 変換失敗。(" + e.Message + ")");
                }
            }

            WriteLog("変換を終了しました。");
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        private void WriteLog(string msg) {
            logger.Debug(msg);
            textBoxLog.AppendText(DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff] ") + msg + Environment.NewLine);
            textBoxLog.ScrollToEnd();
            textBoxLog.Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }
}
