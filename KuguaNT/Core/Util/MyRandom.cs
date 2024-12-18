using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kugua
{
    class MyRandom
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create(); // 静态实例
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

                // 计算生成的随机范围
                int range = maxValue - minValue;

                // 使用 byte 数组存储随机字节
                byte[] randomNumber = new byte[4]; // 4 字节可以表示一个 32 位整数
                _rng.GetBytes(randomNumber); // 填充随机字节

                // 将字节转换为无符号整数
                uint uintRandomNumber = BitConverter.ToUInt32(randomNumber, 0);

                // 返回范围内的随机整数
                return (int)(uintRandomNumber % (uint)range) + minValue; // 使用模运算限制范围并偏移
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
            try
            {
                // 使用 byte 数组存储随机字节
                byte[] randomNumber = new byte[8]; // 8 字节可以表示一个 64 位整数
                _rng.GetBytes(randomNumber); // 填充随机字节

                // 将字节转换为无符号 64 位整数
                ulong ulongRandomNumber = BitConverter.ToUInt64(randomNumber, 0);

                // 将其映射到 [0.0, 1.0)
                return (ulongRandomNumber / (double)(ulong.MaxValue + 1.0));
            }
            catch (Exception ex)
            {
                // 异常处理，返回安全默认值
                Logger.Log(ex);
            }
            return 0.0;
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
                // 随机选择一个字符
                int randomIndex = Next(0, chars.Length);
                stringChars[i] = chars[randomIndex];
            }
            return new string(stringChars);
        }
    }
}
