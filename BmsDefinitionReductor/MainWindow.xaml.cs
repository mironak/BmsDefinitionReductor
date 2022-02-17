using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using BmsDefinitionReductor.Class;
using NAudio.Wave;
using static BmsDefinitionReductor.Class.FileList;

namespace BmsDefinitionReductor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string _bmsFileName = "";
        FileList _fileList = null;
        readonly string OutputFileName = @"out.bms";
        private Progress<int> _progress;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            DefinitionReductButton.IsEnabled = false;
            LoadBmsButton.IsEnabled = true;
            FilesListView.IsEnabled = true;
            R2TextBox.IsEnabled = true;
            Definition_Start.IsEnabled = true;
            Definition_End.IsEnabled = true;

            _progress = new Progress<int>((percent) =>
            {
                DefinitionReductProgressBar.Value = percent;
                switch (percent)
                {
                    case 100:
                        StatusLabel.Content = "完了しました。";
                        DefinitionReductButton.IsEnabled = true;
                        LoadBmsButton.IsEnabled = true;
                        break;

                    default:
                        StatusLabel.Content = "実行中...";
                        break;
                }
            });
        }

        /// <summary>
        /// 読み込み処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadBms_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "BMSファイル (*.*)|*.*";

            try
            {
                if (dialog.ShowDialog() == true)
                {
                    _bmsFileName = dialog.FileName;

                    _fileList = new FileList(_bmsFileName);
                    FilesListView.ItemsSource = _fileList.CreateFileList();

                    DefinitionReductButton.IsEnabled = true;
                }
            }
            catch
            {
                MessageBox.Show("BMSファイルを読み込んでください。");
            }
        }

        /// <summary>
        /// wavリスト選択時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WavFiles item = (WavFiles)FilesListView.SelectedItem;
            try
            {
                AudioFileReader reader = new AudioFileReader(item.Name);
                WaveOut waveOut = new WaveOut();
                waveOut.Init(reader);
                waveOut.Play();
            }
            catch
            {
                MessageBox.Show("wavファイルがありません。");
            }
        }

        /// <summary>
        /// 開始ボタン処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DefinitionReuseButton_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログ表示
            var dialog = new SaveFileDialog();
            dialog.Filter = "出力BMSファイル(*.bms)|*.bms|全てのファイル(*.*)|*.*";
            dialog.FileName = OutputFileName;
            if (dialog.ShowDialog() == false)
            {
                return;
            }

            // 相関係数の取得
            float r2Val;
            try
            {
                r2Val = GetR2Val();
            }
            catch
            {
                MessageBox.Show("相関係数は0から1の間で入力してください。");
                return;
            }

            // 探索範囲の取得
            string startVal = Definition_Start.Text;
            string endVal = Definition_End.Text;
            try
            {
                CheckAreaVal(startVal, endVal);
            }
            catch
            {
                MessageBox.Show("定義は2桁の01-ZZで入力してください。");
                return;
            }

            // 処理実行開始
            BusyView(true);
            DefinitionReuse dr = new DefinitionReuse(_fileList.GetFileList());
            try
            {
                await Task.Run(() =>
                {
                    dr.ReductDefinition(
                        _bmsFileName,
                        _progress,
                        WaveValidation.CalculateRSquared,
                        r2Val,
                        dialog.FileName,
                        RadixConvert.ZZToInt(startVal),
                        RadixConvert.ZZToInt(endVal));
                });
            }
            catch
            {
                MessageBox.Show("ファイルが見つかりませんでした。");
                StatusLabel.Content = "BMSファイルを読み込んでください。";
            }
            finally
            {
                BusyView(false);
            }
        }
        
        /// <summary>
        /// 動作中表示
        /// </summary>
        /// <param name="isBusy"></param>
        private void BusyView(bool isBusy)
        {
            bool isEnable = !isBusy;
            DefinitionReductButton.IsEnabled = isEnable;
            LoadBmsButton.IsEnabled = isEnable;
            FilesListView.IsEnabled = isEnable;
            R2TextBox.IsEnabled = isEnable;
            Definition_Start.IsEnabled = isEnable;
            Definition_End.IsEnabled = isEnable;
        }

        /// <summary>
        /// 相関係数のしきい値を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private float GetR2Val()
        {
            float r2val = float.Parse(R2TextBox.Text);
            if (1.0 < r2val)
            {
                throw new ArgumentOutOfRangeException();
            }
            return r2val;
        }

        /// <summary>
        /// 対象の領域を確認する
        /// </summary>
        /// <param name="startVal"></param>
        /// <param name="endVal"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void CheckAreaVal(string startVal, string endVal)
        {
            if (startVal.Length != 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (endVal.Length != 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            var endIVal = RadixConvert.ZZToInt(endVal);
            var startIVal = RadixConvert.ZZToInt(startVal);

            if (endIVal <= startIVal)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (startIVal < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (RadixConvert.ZZToInt("ZZ") < endIVal)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
