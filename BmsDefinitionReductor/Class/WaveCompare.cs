using NAudio.Wave;

namespace BmsDefinitionReductor.Class
{
    static public class WaveCompare
    {
        public delegate float ValidComparator(float[] x, float[] y);

        /// <summary>
        /// 波形の一致判定を行う。
        /// </summary>
        /// <param name="wav1">WAVファイル名1</param>
        /// <param name="wav2">WAVファイル名2</param>
        /// <param name="comparator">評価関数</param>
        /// <param name="r2val">一致率の最小値(0-1)</param>
        /// <returns>一致</returns>
        static public bool IsMatch(AudioFileReader reader1, AudioFileReader reader2, ValidComparator comparator, float r2val)
        {
            if (!IsSameSetting(reader1, reader2))
            {
                return false;
            }

            var readBufferA = new float[reader1.WaveFormat.SampleRate * reader1.WaveFormat.Channels];
            var readBufferB = new float[reader2.WaveFormat.SampleRate * reader2.WaveFormat.Channels];

            reader1.Position = 0;
            reader2.Position = 0;

            while (true)
            {
                int bufferAResidual = reader1.Read(readBufferA, 0, readBufferA.Length);
                int bufferBResidual = reader2.Read(readBufferB, 0, readBufferB.Length);

                if((bufferAResidual <= 0) && (bufferBResidual <= 0))
                {
                    break;
                }

                if(CalculateMatchRate(readBufferA, readBufferB, reader1.WaveFormat.Channels, comparator) < r2val)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 設定が同じかの判定を行う。
        /// </summary>
        /// <param name="r1">WAVファイル1</param>
        /// <param name="r2">WAVファイル2</param>
        /// <returns>設定一致</returns>
        static private bool IsSameSetting(AudioFileReader r1, AudioFileReader r2)
        {
            if (r1.WaveFormat.SampleRate != r2.WaveFormat.SampleRate)
            {
                return false;
            }

            if (r1.WaveFormat.Channels != r2.WaveFormat.Channels)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 2つのWAVデータの一致率を求める。
        /// </summary>
        /// <param name="wav1">WAVデータ1配列</param>
        /// <param name="wav2">WAVデータ2配列</param>
        /// <param name="channelNum">WAVのチャネル数</param>
        /// <param name="comparator">評価関数</param>
        /// <returns>一致率</returns>
        static private float CalculateMatchRate(float[] wav1, float[] wav2, int channelNum, ValidComparator comparator)
        {
            var wav1Ch = new float[wav1.Length / channelNum];
            var wav2Ch = new float[wav2.Length / channelNum];
            float minimumMatchRate = 1.0F;

            for(int i = 0; i < channelNum; i++)
            {
                for(int j = i; j < wav1.Length; j+=channelNum)
                {
                    wav1Ch[j / channelNum] = wav1[j];
                    wav2Ch[j / channelNum] = wav2[j];
                }

                float matchRate = comparator(wav1Ch, wav2Ch);
                if(matchRate < minimumMatchRate)
                {
                    minimumMatchRate = matchRate;
                }
            }
            return minimumMatchRate;
        }
    }
}
