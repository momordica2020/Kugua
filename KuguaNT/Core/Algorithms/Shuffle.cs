using System.Text;

namespace Kugua.Core.Algorithms
{
    /// <summary>
    /// 洗牌算法
    /// </summary>
    public class Shuffle
    {


        /// <summary>
        /// 原位替换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        public static void FisherYates<T>(IList<T> input)
        {
            // Fisher-Yates 洗牌算法，完全打乱
            Random random = new Random();
            for (int i = input.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1); // 生成随机索引
                                               // 交换
                T temp = input[i];
                input[i] = input[j];
                input[j] = temp;
            }
        }



        /// <summary>
        /// 将结果放在新的List<typeparamref name="T"/>里返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<T> FisherYates2<T>(IList<T> input)
        {
            List<T> res = new List<T>(input);
            FisherYates(res);
            return res;
        }


        //public static void FisherYates(char[] input)
        //{
        //    // Fisher-Yates 洗牌算法，完全打乱
        //    for (int i = input.Length - 1; i > 0; i--)
        //    {
        //        int j = MyRandom.Next(0, i + 1); // 生成随机索引
        //                                         // 交换
        //        char temp = input[i];
        //        input[i] = input[j];
        //        input[j] = temp;
        //    }
        //}

        //public static void FisherYates(List<bool> input)
        //{
        //    // Fisher-Yates 洗牌算法，完全打乱
        //    for (int i = input.Count - 1; i > 0; i--)
        //    {
        //        int j = MyRandom.Next(0, i + 1); // 生成随机索引
        //                                         // 交换
        //        var temp = input[i];
        //        input[i] = input[j];
        //        input[j] = temp;
        //    }
        //}

        //public static void FisherYates(string[] input)
        //{
        //    // Fisher-Yates 洗牌算法，完全打乱
        //    for (int i = input.Length - 1; i > 0; i--)
        //    {
        //        int j = MyRandom.Next(0, i + 1); // 生成随机索引
        //                                         // 交换
        //        string temp = input[i];
        //        input[i] = input[j];
        //        input[j] = temp;
        //    }
        //}
        //public static void FisherYates(bool[] input)
        //{
        //    // Fisher-Yates 洗牌算法，完全打乱
        //    for (int i = input.Length - 1; i > 0; i--)
        //    {
        //        int j = MyRandom.Next(0, i + 1); // 生成随机索引
        //                                         // 交换
        //        bool temp = input[i];
        //        input[i] = input[j];
        //        input[j] = temp;
        //    }
        //}
        /// <summary>
        /// 洗牌算法
        /// </summary>
        /// <param name="str">需打乱的字符串</param>
        /// <returns>打乱结果</returns>
        public static string ShuffleString(string str, int time = 0)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;



            if (time < 1)
            {
                char[] array = str.ToCharArray(); // 将字符串转换为字符数组
                FisherYates(array);
                return new string(array); // 返回新的乱序字符串
            }
            else
            {
                // 只随机切牌time轮  算法3 - 不均匀切
                time = Math.Min(time, str.Length - 1);
                bool[] cuts = new bool[str.Length - 1];

                var stringBuilder = new StringBuilder();
                for (int i = 0; i < cuts.Length; i++) cuts[i] = i < time;
                FisherYates(cuts);
                List<string> parts = new List<string>();

                // 切割字符串
                int startIndex = 0;
                for (int i = 1; i < str.Length; i++)
                {
                    if (cuts[i - 1])
                    {
                        parts.Add(str.Substring(startIndex, i - startIndex));
                        startIndex = i;
                    }

                }
                parts.Add(str.Substring(startIndex));
                var pparts = parts.ToArray();
                FisherYates(pparts);
                return string.Concat(pparts);

                //// 只随机切牌time轮  算法2 - 均匀切
                //time = Math.Min(time, str.Length);
                //int partLength = str.Length / time;
                //List<string> parts = new List<string>();

                //// 切割字符串
                //for (int i = 0; i < time; i++)
                //{
                //    // 计算切割的开始和结束索引
                //    int startIndex = i * partLength;
                //    // 处理最后一部分，确保包含所有剩余字符
                //    int length = (i == time - 1) ? str.Length - startIndex : partLength;

                //    // 提取子字符串并添加到列表中
                //    parts.Add(str.Substring(startIndex, length));
                //}

                //// 打乱切割后的部分
                //List<string> shuffledParts = parts.OrderBy(x => rand.Next()).ToList();

                //// 合并打乱后的部分为最终字符串
                //return string.Concat(shuffledParts);

                //// 只随机切牌time轮 算法1 - 切后拼后切
                //for (int i = 0; i < Math.Min(time, str.Length*2); i++)
                //{
                //    int cutPosition = rand.Next(1, str.Length); 
                //    string leftPart = str.Substring(0, cutPosition);
                //    if (rand.Next(0, 2) > 0) leftPart = new string(leftPart.Reverse().ToArray());
                //    string rightPart = str.Substring(cutPosition);
                //    if (rand.Next(0, 2) > 0) rightPart = new string(rightPart.Reverse().ToArray());
                //    str = rightPart + leftPart;
                //}
                //// 合并打乱后的部分为最终字符串
                //return str;
            }
        }
    }
}
