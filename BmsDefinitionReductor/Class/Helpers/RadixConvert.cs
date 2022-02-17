using System;

namespace BmsDefinitionReductor.Class
{
    static public class RadixConvert
    {
        const int ZZRadix = 36;

        /// <summary>
        /// 数値を2桁の36進数に変換
        /// </summary>
        /// <param name="dec"></param>
        /// <returns></returns>
        static public string IntToZZ(int dec)
        {
            return new string(new char[]
            {
                IntToZ(dec / ZZRadix),
                IntToZ(dec % ZZRadix),
            });
        }

        /// <summary>
        /// 2桁の36進数(文字列)を数値に変換
        /// </summary>
        /// <param name="zz"></param>
        /// <returns></returns>
        static public int ZZToInt(string zz)
        {
            int result = ZToInt(Convert.ToChar(zz[0])) * ZZRadix + ZToInt(Convert.ToChar(zz[1]));
            return result;
        }

        /// <summary>
        /// 数値を1桁の36進数に変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static private Char IntToZ(int value)
        {
            if((0 <= value) && (value <= 9))
            {
                return (Char)(value + '0');
            }

            if ((10 <= value) && (value < ZZRadix))
            {
                return (Char)(value - 10 + 'A');
            }

            return (Char)'0';
        }

        /// <summary>
        /// 1桁の36進数(文字列)を数値に変換
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static private int ZToInt(Char c)
        {
            // 0-9
            if(('0' <= c) && (c <= '9'))
            {
                return c - '0';
            }

            // A-Z
            if (('A' <= c) && (c <= 'Z'))
            {
                return c - 'A' + 10;
            }

            return 0;
        }
    }
}
