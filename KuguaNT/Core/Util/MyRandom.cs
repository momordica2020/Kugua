using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kugua
{
    class MyRandom
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();


        public static uint getNextUInt()
        {
            byte[] randomNumber = new byte[4];
            _rng.GetBytes(randomNumber);
            return BitConverter.ToUInt32(randomNumber, 0);
        }
        //public static int getNextInt()
        //{
        //    return (int)getNextUInt();
        //}
        public static ulong getNextULong()
        {
            // 使用 byte 数组存储随机字节
            byte[] randomNumber = new byte[8];
            _rng.GetBytes(randomNumber);
            return BitConverter.ToUInt64(randomNumber, 0);
        }


        public static BigInteger getNextBigInteger(int size)
        {
            // 使用 byte 数组存储随机字节
            byte[] randomNumber = new byte[size];
            _rng.GetBytes(randomNumber);
            return new BigInteger(randomNumber);
        }


        //public static long getNextLong()
        //{
        //    return (long)getNextULong();
        //}




        //
        /// <summary>
        ///  生成范围在 min 和 max 之间的（高随机度的）随机整数
        ///  注意！maxValue值到达不了，实际范围在[min,max-1]
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue">注意！maxValue值到达不了，实际范围在[min,max-1]</param>
        /// <returns></returns>
        public static int Next(int minValue, int maxValue)
        {
            try
            {
                if (minValue < int.MinValue) minValue = int.MinValue;
                if (maxValue > int.MaxValue) maxValue = int.MaxValue;
                if (minValue == maxValue) return minValue;
                if (minValue > maxValue)
                {
                    int tmp = minValue;
                    minValue = maxValue;
                    maxValue = tmp;
                }

                int range = maxValue - minValue;

                return (int)(getNextUInt() % (uint)range) + minValue;
            }
            catch (Exception ex)
            {

            }
            return minValue;


        }



        //
        /// <summary>
        ///  生成范围在 min 和 max 之间的（高随机度的）BigInteger整数
        ///  注意！maxValue值到达不了，实际范围在[min,max-1]
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue">注意！maxValue值到达不了，实际范围在[min,max-1]</param>
        /// <returns></returns>
        public static BigInteger Next(BigInteger minValue, BigInteger maxValue)
        {
            try
            {
                if (minValue == maxValue) return minValue;
                if (minValue > maxValue)
                {
                    BigInteger tmp = minValue;
                    minValue = maxValue;
                    maxValue = tmp;
                }

                BigInteger range = maxValue - minValue;
                var r = getNextBigInteger(range.ToByteArray().Length);
                while (r < range) r += range;
                return (r % range) + minValue;
            }
            catch (Exception ex)
            {

            }
            return minValue;


        }


        public static long Next(long minValue, long maxValue)
        {
            try
            {
                if (minValue < long.MinValue) minValue = long.MinValue;
                if (maxValue > long.MaxValue) maxValue = long.MaxValue;
                if (minValue == maxValue) return minValue;
                if (minValue > maxValue)
                {
                    long tmp = minValue;
                    minValue = maxValue;
                    maxValue = tmp;
                }

                ulong range = (ulong)(maxValue - minValue);


                return (long)(getNextULong() % range) + minValue;
            }
            catch (Exception ex)
            {

            }
            return minValue;


        }




        /// <summary>
        /// 生成随机整数（不带范围）
        /// </summary>
        /// <returns></returns>
        public static int Next()
        {
            return Next(0, int.MaxValue);
        }



        /// <summary>
        /// 生成随机[0~maxValue)之间的整数（不包含maxValue）
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        /// <summary>
        /// 生成随机[0~maxValue)之间的整数（不包含maxValue）
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static long Next(long maxValue)
        {
            return Next(0, maxValue);
        }

        public static int Next(uint maxValue)
        {
            return (int)Next(0, (long)maxValue);
        }

        public static int Next(IEnumerable<object> items)
        {
            return items == null ? 0 : Next(0, items.Count());
        }


        /// <summary>
        /// 0.0~1.0
        /// </summary>
        /// <returns></returns>
        public static double NextDouble()
        {
            var d = Math.Abs((double)getNextULong() / ulong.MaxValue);
            //Logger.Log(d.ToString());
            return d;

        }

        /// <summary>
        /// 生成指定长度的随机可打印字符串
        /// </summary>
        /// <param name="length">随机字符串的长度</param>
        /// <returns></returns>
        public static string NextString(int length)
        {
            const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789"; // 可打印字符集
            char[] stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[Next(chars.Length)];
            }
            return new string(stringChars);
        }
    }
}
