namespace Kugua.Core.Chinese
{
    internal static class ChineseNumber
    {


        private static readonly string[] ChnNum = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        private static readonly string[] UnitSection = { "", "万", "亿" }; // 节权位
        private static readonly string[] UnitDigit = { "", "十", "百", "千" }; // 权位

        /// <summary>
        /// 中文数字映射表
        /// </summary>
        private static readonly Dictionary<char, int> ChineseDigitMap = new()
        {
            { '〇', 0 },
            { '零', 0 },
            { '一', 1 },
            { '二', 2 }, { '两', 2 },
            { '三', 3 },
            { '四', 4 },
            { '五', 5 },
            { '六', 6 },
            { '七', 7 },
            { '八', 8 },
            { '九', 9 },

        };



        /// <summary>
        /// 替换中文数字字符为对应阿拉伯数字，一一对应
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertChineseDigitsToArabic(string input)
        {
            foreach (var pair in ChineseDigitMap)
            {
                input = input.Replace(pair.Key.ToString(), pair.Value.ToString());
            }
            return input;
        }




        /// <summary>
        /// 将汉字写的数字字符串转化成int，兼容本来就是阿拉伯数字的串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int ConvertToNumber(string input)
        {
            int result = 0;
            int tempNumber = 0;
            bool hasTen = false;


            if (int.TryParse(input, out result))
            {
                return result;
            }
            Dictionary<char, int> ChineseDigitMap2 = new()
            {   
                // 中文常见数字
                { '〇', 0 }, { '零', 0 },
                { '一', 1 }, { '壹', 1 }, { '壱', 1 }, { '〡', 1 }, // 异体
                { '二', 2 }, { '贰', 2 }, { '弐', 2 }, { '貮', 2 }, { '两', 2 }, // 异体
                { '三', 3 }, { '叁', 3 }, { '参', 3 }, { '仨', 3 }, // 异体和俗称
                { '四', 4 }, { '肆', 4 }, { '〤', 4 }, // 异体
                { '五', 5 }, { '伍', 5 }, { '〥', 5 }, // 异体
                { '六', 6 }, { '陆', 6 }, { '〦', 6 }, // 异体
                { '七', 7 }, { '柒', 7 }, { '〧', 7 }, // 异体
                { '八', 8 }, { '捌', 8 }, { '〨', 8 }, // 异体
                { '九', 9 }, { '玖', 9 }, { '〩', 9 }, // 异体
                { '十', 10 }, { '拾', 10 }, { '廿', 20 }, { '卅', 30 }, { '卌', 40 }, // 异体和简写
            };
            foreach (char ch in input)
            {
                if (ChineseDigitMap2.TryGetValue(ch, out int value))
                {
                    if (value == 10) // 遇到“十”
                    {
                        if (tempNumber == 0)
                        {
                            tempNumber = 10; // “十”单独出现表示10
                        }
                        else
                        {
                            tempNumber *= 10; // 如“二十”表示2*10
                        }
                        hasTen = true;
                    }
                    else
                    {
                        if (hasTen) // 如果前面是“十”
                        {
                            tempNumber += value; // 累加个位数字，如“二十八”
                            result += tempNumber; // 累计到结果
                            tempNumber = 0; // 重置临时值
                            hasTen = false;
                        }
                        else
                        {
                            tempNumber = value; // 普通数字直接赋值
                        }
                    }
                }
                else
                {
                    // 如果无法匹配，返回 0 表示无效
                    return 0;
                }
            }

            // 最后一部分累加
            result += tempNumber;

            return result;
        }

        public static string GetChineseDigital(long num)
        {
            if (num == 0) return "零";

            string result = "";
            int sectionIndex = 0;

            while (num > 0)
            {
                long section = num % 10000;
                string sectionStr = SectionToChinese(section);

                // 拼接节权位（万、亿）
                if (section != 0)
                    result = sectionStr + UnitSection[sectionIndex] + result;
                else
                    result = "零" + result; // 处理中间的零节

                num /= 10000;
                sectionIndex++;
            }
            result = result.Replace("一十", "十"); // 处理十位上的“一”
            return result.Trim('零'); // 去除首尾多余的零
        }

        private static string SectionToChinese(long section)
        {
            string s = "";
            for (int i = 0; i < 4 && section > 0; i++)
            {
                long digit = section % 10;
                if (digit != 0)
                    s = ChnNum[digit] + UnitDigit[i] + s;
                else if (s != "" && !s.StartsWith("零"))
                    s = "零" + s; // 处理段内零
                section /= 10;
            }
            return s;
        }
    }
}