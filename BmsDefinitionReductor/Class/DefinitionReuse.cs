using System;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using static BmsDefinitionReductor.Class.FileList;
using static BmsDefinitionReductor.Class.WaveCompare;
using NAudio.Wave;

namespace BmsDefinitionReductor.Class
{
    public sealed class DefinitionReuse
    {
        const int DefinitionStart = 1;
        const int DefinitionEnd = 1296; // ZZ

        ObservableCollection<WavFiles> _fileList;
        int[] _replaces = new int[DefinitionEnd];
        AudioFileReader[] _readers = new AudioFileReader[DefinitionEnd];

        int _startPoint = DefinitionStart;
        int _endPoint = DefinitionEnd;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileList"></param>
        public DefinitionReuse(ObservableCollection<WavFiles> fileList)
        {
            _fileList = fileList;
        }

        /// <summary>
        /// 定義の使いまわし処理。
        /// </summary>
        /// <param name="bmsFileName">読み込むBMSファイル名</param>
        /// <param name="progress">進捗確認用</param>
        /// <param name="comparator">比較関数</param>
        /// <param name="r2Val">しきい値</param>
        /// <param name="saveFileName">保存するファイル名</param>
        /// <param name="defStart">置換対象定義範囲先頭</param>
        /// <param name="defEnd">置換対象定義範囲終端</param>
        public void ReductDefinition(
            string bmsFileName, 
            IProgress<int> progress, 
            ValidComparator comparator, 
            float r2Val, 
            string saveFileName, 
            int defStart, 
            int defEnd)
        {
            progress.Report(0);

            _startPoint = Math.Max(_fileList[0].NumInteger, defStart);
            _endPoint = Math.Min(_fileList[_fileList.Count-1].NumInteger, defEnd);

            for (int i = 0; i < _fileList.Count; i++)
            {
                _readers[i] = new AudioFileReader(_fileList[i].Name);
            }

            CreateReplaceTable(progress, comparator, r2Val);
            progress.Report(80);

            string writeData = ReplaceBmsFile(bmsFileName);
            progress.Report(90);

            WriteBmsFile(saveFileName, writeData);
            progress.Report(100);
        }

        /// <summary>
        /// 置換用テーブル作成
        /// </summary>
        /// <param name="progress">進捗確認用</param>
        /// <param name="comparator">比較関数</param>
        /// <param name="r2val">しきい値</param>
        /// <param name="defStart">置換対象定義範囲先頭</param>
        /// <param name="defEnd">置換対象定義範囲終端</param>
        private void CreateReplaceTable(IProgress<int> progress, ValidComparator comparator, float r2val)
        {
            for (int i = 0; i < _fileList.Count; i++)
            {
                int iVal = _fileList[i].NumInteger;
                if (iVal < _startPoint)
                {
                    continue;
                }
                if (_endPoint < iVal)
                {
                    return;
                }
                // 置換済みは無視
                if (_replaces[iVal] != 0)
                {
                    continue;
                }
                _replaces[iVal] = iVal;

                for (int j = i + 1; j < _fileList.Count; j++)
                {
                    int jVal = _fileList[j].NumInteger;
                    if (_endPoint < jVal)
                    {
                        break;
                    }

                    // 置換済みは無視
                    if (_replaces[jVal] != 0)
                    {
                        continue;
                    }

                    // 同一ファイルは無視
                    if (_fileList[i].Name.Equals(_fileList[j].Name))
                    {
                        _replaces[jVal] = -1;
                        continue;
                    }

                    // 一致したら既出の番号に置換
                    if (WaveCompare.IsMatch(_readers[i], _readers[j], comparator, r2val))
                    {
                        _replaces[jVal] = iVal;
                    }
                }

                progress.Report((int)((float)i / _fileList.Count * 80));
            }
        }

        /// <summary>
        /// BMSファイルの置換
        /// </summary>
        /// <param name="bmsFileName">読み込むBMSファイル名</param>
        /// <returns>出力データ</returns>
        private string ReplaceBmsFile(string bmsFileName)
        {
            string writeData = "";

            using (StreamReader sr = new StreamReader(bmsFileName, Encoding.GetEncoding("shift_jis")))
            {
                string line = "";

                while ((line = sr.ReadLine()) != null)
                {
                    BmsManager.BmsCommand command = BmsManager.GetLineCommand(line);
                    if (command != BmsManager.BmsCommand.MAIN)
                    {
                        // 置換対象でない行はそのままコピー
                        writeData += line;
                        writeData += "\n";
                        continue;
                    }

                    // 置換
                    for (int k = _startPoint; k < (_endPoint+1); k++)
                    {
                        if (_replaces[k] <= 0)
                        {
                            continue;
                        }
                        BmsManager.ChangeDefinition(ref line, RadixConvert.IntToZZ(k), RadixConvert.IntToZZ(_replaces[k]));
                    }
                    writeData += line;
                    writeData += "\n";
                }
            }
            return writeData;
        }

        /// <summary>
        /// BMSファイルを出力する。
        /// </summary>
        /// <param name="saveFileName">保存するファイル名</param>
        /// <param name="writeData">出力したいデータ</param>
        private void WriteBmsFile(string saveFileName, string writeData)
        {
            using (StreamWriter sw = new StreamWriter(saveFileName, false, Encoding.GetEncoding("shift_jis")))
            {
                sw.Write(writeData);
            }
        }
    }
}
