using System.Linq;

namespace BmsDefinitionReductor.Class
{
    static public class WaveValidation
    {
        /// <summary>
        /// 決定係数を計算する。
        /// https://ja.wikipedia.org/wiki/%E6%B1%BA%E5%AE%9A%E4%BF%82%E6%95%B0
        /// </summary>
        /// <param name="wav1">WAVデータ配列1</param>
        /// <param name="wav2">WAVデータ配列2</param>
        /// <returns>決定係数(0-1)</returns>
        static public float CalculateRSquared(float[] wav1, float[] wav2)
        {
            float wav1Average = wav1.Average();

            float rss = 0;
            float dss = 0;
            for (int i = 0; i < wav1.Length; i++)
            {
                float wav1RssTemp = wav1[i] - wav2[i];
                float wav2DssTemp = wav1[i] - wav1Average;
                rss += wav1RssTemp * wav1RssTemp;
                dss += wav2DssTemp * wav2DssTemp;
            }
            return 1.0F - (rss / dss);
        }
    }
}
