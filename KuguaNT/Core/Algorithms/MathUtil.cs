namespace Kugua.Core.Algorithms
{
    internal static class MathUtil
    {



        /// <summary>
        /// 计算最大公约数（GCD）
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long GCD(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// 计算最小公倍数（LCM）
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long LCM(long a, long b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }
    }
}