using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace BmsDefinitionReductor.Class
{
    public sealed class FileList
    {
        /// <summary>
        /// "#WAV"の番号とwavファイル名
        /// </summary>
        public class WavFiles
        {
            public string Num { get; set; }
            public int NumInteger { get; set; }   //処理用(毎回変換しなくて済むように)
            public string Name { get; set; }
        }

        ObservableCollection<WavFiles> _wavFileList = new ObservableCollection<WavFiles>();
        string _bmsFileName;
        string _bmsDirectory;

        public FileList(string bmsFileName)
        {
            _bmsFileName = bmsFileName;
            _bmsDirectory = System.IO.Path.GetDirectoryName(_bmsFileName);
        }

        /// <summary>
        /// BMSファイルに登録されている"#WAV"の一覧を作成する。
        /// </summary>
        /// <returns>"#WAV"の一覧</returns>
        public ObservableCollection<WavFiles> CreateFileList()
        {
            using (StreamReader sr = new StreamReader(_bmsFileName, Encoding.GetEncoding("UTF-8")))
            {
                string line = "";

                while ((line = sr.ReadLine()) != null)
                {
                    if (BmsManager.GetLineCommand(line) == BmsManager.BmsCommand.WAV)
                    {
                        (string def, string fname)wavData = BmsManager.GetWavData(line);
                        WavFiles item = new WavFiles { 
                            Num = wavData.def, 
                            NumInteger = RadixConvert.ZZToInt(wavData.def), 
                            Name = _bmsDirectory + "\\" + wavData.fname };
                        _wavFileList.Add(item);
                    }
                }
                if(_wavFileList.Count == 0)
                {
                    throw new ArgumentException();
                }
            }
            return _wavFileList;
        }

        /// <summary>
        /// BMSファイルに登録されている"#WAV"の一覧を取得する。
        /// </summary>
        /// <returns>"#WAV"の一覧</returns>
        public ObservableCollection<WavFiles> GetFileList()
        {
            return _wavFileList;
        }
    }
}
