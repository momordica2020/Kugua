using Kugua.Core.Chinese;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Kugua.Core.Algorithms
{

    /// <summary>
    /// 大数计算
    /// </summary>
    internal static class BigUtil
    {

        /// <summary>
        /// 中文单位映射表
        /// </summary>
        private static readonly Dictionary<string, BigInteger> ChineseUnitMap = new()
        {
            { "十", 10 },
            { "百", 100 },
            { "千", 1000 },
            { "万", 10_000 },
            { "亿", 100_000_000 },
            { "兆", BigInteger.Pow(10, 12) },
            { "京", BigInteger.Pow(10, 16) },
            { "垓", BigInteger.Pow(10, 20) },
            { "秭", BigInteger.Pow(10, 24) },
            { "穰", BigInteger.Pow(10, 28) },
            { "沟", BigInteger.Pow(10, 32) },
            { "涧", BigInteger.Pow(10, 36) },
            { "正", BigInteger.Pow(10, 40) },
            { "载", BigInteger.Pow(10, 44) },
            { "极", BigInteger.Pow(10, 48) },
            { "无量大数", BigInteger.Pow(10, 52) },
            { "恒河沙", BigInteger.Pow(10, 56) },
            { "阿僧祇", BigInteger.Pow(10, 60) },
            { "那由他", BigInteger.Pow(10, 64) },
            { "不可思议", BigInteger.Pow(10, 68) },
            { "无量数", BigInteger.Pow(10, 72) },
            { "大数", BigInteger.Pow(10, 76) }
        };



   


        /// <summary>
        /// 大整数带小数除法，返回值是整数部分、小数部分、小数部分该除以的大小（分母）
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="keepN"></param>
        /// <returns></returns>
        public static (BigInteger intPart, BigInteger decPart, BigInteger decUnit) Div(BigInteger value1, BigInteger value2, int keepN = 2)
        {
            BigInteger res = value1;
            if (keepN <= 0) keepN = 1;
            var totalDigits2 = Math.Min((int)Math.Floor(BigInteger.Log10(value2) + 1), keepN);
            var delta = BigInteger.Pow(10, totalDigits2);
            res = value1 * delta / value2;
            var intpart = res / delta;
            var decpart = res - (res / delta * delta);


            return (intpart, decpart, delta);
        }


        /// <summary>
        /// 取得biginteger保留最高n位整数的结果
        /// </summary>
        /// <param name="value"></param>
        /// <param name="keepnum"></param>
        /// <returns></returns>
        public static BigInteger Floor(BigInteger value, int n = 1)
        {
            if (n < 1) n = 1;
            // 获取当前数值的位数
            int totalDigits = (int)Math.Floor(BigInteger.Log10(value) + 1);

            // 计算移除低位部分的倍数
            int digitsToRemove = totalDigits - n;
            if (digitsToRemove < 0) digitsToRemove = 0;

            // 保留最高 n 位
            BigInteger highestN = value / BigInteger.Pow(10, digitsToRemove) * BigInteger.Pow(10, digitsToRemove);

            return highestN;
        }


        /// <summary>
        /// 大整数带小数除法，保留小数点后N位，返回字符串
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="keepN"></param>
        /// <returns></returns>
        public static string DivToString(BigInteger val1, BigInteger val2, int keepN)
        {
            (BigInteger intPart, BigInteger decPart, BigInteger decUnit) = Div(val1, val2, keepN);
            return $"{intPart}.{decPart.ToString().PadLeft((int)Math.Floor(BigInteger.Log10(decUnit) + 1), '0')}";
        }

        /// <summary>
        /// 解析字符串为BigInteger
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static BigInteger ConvertFromString(string input)
        {
            if (input == null) return BigInteger.Zero;


            // 检查是否是科学计数法
            if (Regex.IsMatch(input, @"^[+-]?\d+(\.\d+)?[eE][+-]?\d+$"))
            {
                return ConvertFromSci(input);
            }

            BigInteger result = 0;
            BigInteger currentValue = 0;
            input = ChineseNumber.ConvertChineseDigitsToArabic(input.Trim());

            // 检查正负号
            bool isNegative = input.StartsWith("负");
            input = isNegative ? input.Substring(1) : input;

            var regex = new Regex(@"(\d+)(十|百|千|万|亿|兆|京|垓|秭|穰|沟|涧|正|载|极|无量大数|恒河沙|阿僧祇|那由他|不可思议|无量数|大数)?");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                string numberStr = match.Groups[1].Value;
                string unit = match.Groups[2].Value;

                BigInteger value = BigInteger.Parse(numberStr);

                if (ChineseUnitMap.ContainsKey(unit))
                {
                    currentValue += value * ChineseUnitMap[unit];
                }
                else
                {
                    currentValue += value;
                }

                if (ChineseUnitMap.ContainsKey(unit))
                {
                    result += currentValue;
                    currentValue = 0;
                }
            }

            result += currentValue;

            return isNegative ? -result : result;
        }





        /// <summary>
        /// 转换 BigInteger 为中文自然语言表示
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ConvertToChinese(BigInteger number)
        {
            if (number == 0) return "零";
            var unitMap = new List<(BigInteger Threshold, string Unit)>
            {
                (BigInteger.Pow(10, 76), "大数"),
                (BigInteger.Pow(10, 72), "无量数"),
                (BigInteger.Pow(10, 68), "不可思议"),
                (BigInteger.Pow(10, 64), "那由他"),
                (BigInteger.Pow(10, 60), "阿僧祇"),
                (BigInteger.Pow(10, 56), "恒河沙"),
                (BigInteger.Pow(10, 48), "极"),
                (BigInteger.Pow(10, 44), "载"),
                (BigInteger.Pow(10, 40), "正"),
                (BigInteger.Pow(10, 36), "涧"),
                (BigInteger.Pow(10, 32), "沟"),
                (BigInteger.Pow(10, 28), "穰"),
                (BigInteger.Pow(10, 24), "秭"),
                (BigInteger.Pow(10, 20), "垓"),
                (BigInteger.Pow(10, 16), "京"),
                (BigInteger.Pow(10, 12), "兆"),
                (100_000_000, "亿"),
                (10_000, "万"),
                (1_000, "千"),
                //(100, "百"),
                //(10, "十")
            };

            // 处理负数
            bool isNegative = number < 0;
            number = BigInteger.Abs(number);

            var result = new List<string>();
            foreach (var (threshold, unit) in unitMap)
            {
                if (number >= threshold)
                {
                    var value = number / threshold;
                    result.Add($"{value}{unit}");
                    number %= threshold;
                }
            }

            if (number > 0)
            {
                if (result.Count > 0 && result[^1] != "零")
                {
                    result.Add("零"); // 添加“零”连接符
                }
                result.Add(number.ToString());
            }

            return isNegative ? "负" + string.Join("", result) : string.Join("", result);
        }

        /// <summary>
        /// 转为以中文单位表示的字符串形式，例如12345 = 1万2千345
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToHans(this BigInteger number)
        {
            return ConvertToChinese(number);
        }


        /// <summary>
        /// 简化为科学计数法，但只对超过特定值的数字才进行简化
        /// </summary>
        /// <param name="number"></param>
        /// <param name="MinNum"></param>
        /// <returns></returns>
        public static string ConvertToSci(this BigInteger number, int MinNum = 100000)
        {
            if (number <= MinNum) return $"{number.ToString()}";
            int digitCount = number.ToString().Length; // 28 位
            int cutLen = int.Min(digitCount, 15);
            double mantissa = double.Parse(number.ToString().Substring(0, cutLen)) / Math.Pow(10, cutLen - 1); // 取前几位避免精度问题
            int exponent = digitCount - 1; // 指数为位数-1
            return $"{mantissa:F4} × 10^{exponent}";
        }

                


        /// <summary>
        /// 将科学计数法字符串解析为 BigInteger
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        private static BigInteger ConvertFromSci(string input)
        {
            var match = Regex.Match(input, @"^([+-]?\d+(\.\d+)?)[eE]([+-]?\d+)$");
            if (!match.Success)
            {
                throw new FormatException($"Invalid scientific notation: {input}");
            }

            // 提取科学计数法部分
            string baseValueStr = match.Groups[1].Value;
            int exponent = int.Parse(match.Groups[3].Value);

            // 分离小数部分
            string[] baseParts = baseValueStr.Split('.');
            BigInteger integerPart = BigInteger.Parse(baseParts[0]);
            BigInteger fractionalPart = baseParts.Length > 1 ? BigInteger.Parse(baseParts[1]) : BigInteger.Zero;
            int fractionalLength = baseParts.Length > 1 ? baseParts[1].Length : 0;

            // 调整指数
            exponent -= fractionalLength;

            // 构造 BigInteger
            BigInteger result = integerPart * BigInteger.Pow(10, Math.Max(0, exponent));
            if (fractionalPart > 0)
            {
                result += fractionalPart * BigInteger.Pow(10, Math.Max(0, exponent - fractionalLength));
            }

            if (exponent < 0)
            {
                throw new InvalidOperationException("Result is too small for BigInteger; consider a different type.");
            }

            return result;
        }
    }
}