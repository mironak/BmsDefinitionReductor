using System;
using System.Text.RegularExpressions;

namespace BmsDefinitionReductor.Class
{
    static public class BmsManager
    {
        /// <summary>
        /// BMSコマンド
        /// </summary>
        public enum BmsCommand
        {
            NONE,
            WAV,
            MAIN,
        }

        /// <summary>
        /// コマンドを取得する。
        /// </summary>
        /// <param name="line">BMSファイルの行データ</param>
        /// <returns>BMSコマンド</returns>
        static public BmsCommand GetLineCommand(string line)
        {
            if (String.IsNullOrEmpty(line))
            {
                return BmsCommand.NONE;
            }

            if(line[0] != '#')
            {
                return BmsCommand.NONE;
            }

            if (IsWav(line))
            {
                return BmsCommand.WAV;
            }

            if (IsMain(line))
            {
                return BmsCommand.MAIN;
            }

            return BmsCommand.NONE;
        }

        static private bool IsWav(string line)
        {
            Regex rgxWav = new Regex(@"^#WAV", RegexOptions.IgnoreCase);
            if (rgxWav.Match(line).Success)
            {
                return true;
            }
            return false;
        }

        static private bool IsMain(string line)
        {
            Regex rgxMain = new Regex(@"^#[0-9][0-9][0-9]", RegexOptions.IgnoreCase);
            if (rgxMain.Match(line).Success)
            {
                // BGMレーン
                if ((line[4] == '0') && (line[5] == '1'))
                {
                    return true;
                }

                // 譜面レーン
                if ((line[4] == '1') || (line[4] == '2'))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// WAVコマンド行のテキストを分離
        /// "#WAV01 a.wav" -> ("01", "a.wav")
        /// </summary>
        /// <param name="line">BMSファイルの行データ</param>
        /// <returns>定義番号とwavファイル名</returns>
        static public (string, string) GetWavData(string line)
        {
            string[] arr = line.Split(new[] { ' ' }, 2);
            return (arr[0].Substring(4, 2), arr[1]);
        }

        /// <summary>
        /// line内のoldDefをnewDefに置換する
        /// </summary>
        /// <param name="line">BMSファイルの行データ</param>
        /// <param name="oldDef">置き換えられる定義番号</param>
        /// <param name="newDef">新しい定義番号</param>
        static public void ChangeDefinition(ref string line, string oldDef, string newDef)
        {
            const int DataStart = 7;
            string dest = line.Substring(0, DataStart);

            for (int i = DataStart; i < (line.Length - 1); i += 2)
            {
                string writeVal = line.Substring(i, 2);
                if(writeVal.Equals(oldDef))
                {
                    writeVal = newDef;
                }
                dest += writeVal;
            }
            line = dest;
        }
    }
}
