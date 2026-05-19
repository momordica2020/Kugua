using Kugua.Mods.Base;
using System.Text.RegularExpressions;

namespace Kugua.Algorithms.Chinese
{
    public partial class 周易占卜
    {
        Dictionary<string, string[]> 卦辞原文 = new Dictionary<string, string[]>();
        Dictionary<string, string[,]> 爻辞原文 = new Dictionary<string, string[,]>();

        蓍草卜筮 卜者;
        public void 初始化()
        {
            try
            {
                var lines = FileSystem.ReadResourceLines("Zhouyi");
                string nowGuaNum = "";
                int nowline = 0;
                foreach (var line in lines)
                {
                    nowline += 1;
                    if (line.StartsWith("0") || line.StartsWith("1"))
                    {
                        nowGuaNum = line.Trim();
                        卦辞原文[nowGuaNum] = new string[7];
                        爻辞原文[nowGuaNum] = new string[6, 4];
                        nowline = 0;
                    }
                    else
                    {
                        if (nowline == 1)
                        {
                            var items = line.Trim().Split(' ');
                            卦辞原文[nowGuaNum][0] = items[0];
                            卦辞原文[nowGuaNum][1] = items[1];
                            卦辞原文[nowGuaNum][2] = items[2];
                        }
                        else if (nowline == 2) 卦辞原文[nowGuaNum][3] = line.Trim().Substring(卦辞原文[nowGuaNum][0].Length + 1);
                        else if (nowline == 3) 卦辞原文[nowGuaNum][4] = line.Trim().Substring(3);
                        else if (nowline == 4) 卦辞原文[nowGuaNum][5] = line.Trim().Substring(3);
                        else if (nowline == 5) 卦辞原文[nowGuaNum][6] = line.Trim().Substring(3);
                        else if (nowline >= 6) 爻辞原文[nowGuaNum][(nowline - 6) / 4, (nowline - 6) % 4] = line.Trim().Substring(3);
                    }
                }


                卜者 = new 蓍草卜筮();
            }
            catch (Exception ex) {
                提示(ex);
            }
        }

        void 提示(Exception ex)
        {
            Kugua.Core.Logger.Log(ex);
        }



        /// <summary>
        /// 让bot替你摇一卦，后面可加缘由也可不加
        /// 占卜/占卜今日运势
        /// </summary>
        /// <returns></returns>
        public string 解读卦象(MessageContext context, string[] param)
        {
            int[] 本卦爻;
            string 本卦 = "";
            string 之卦 = "";

            本卦爻 = 卜者.卜卦.ToArray();
            //Logger.Log($"{string.Join(",", 本卦爻)}");

            List<int> 不变爻 = new List<int>();
            List<int> 变爻 = new List<int>();
            string 之卦爻 = "";
            for (int i = 0; i < 6; i++)
            {
                本卦 += (本卦爻[i] == 6 || 本卦爻[i] == 8) ? '0' : '1';
                if (本卦爻[i] == 6)
                {
                    变爻.Add(i);
                    之卦爻 = 之卦爻 + "7";
                }
                else if (本卦爻[i] == 9)
                {
                    变爻.Add(i);
                    之卦爻 = 之卦爻 + "8";
                }
                else
                {
                    不变爻.Add(i);
                    之卦爻 += 本卦爻[i];
                }
                之卦 += (之卦爻[i] == '6' || 之卦爻[i] == '8') ? '0' : '1';
            }

            string 解卦描述 = "";
            switch (变爻.Count)
            {
                case 0:
                    解卦描述 = $"{取卦名(本卦)}卦不变\r\n" +
                        $"{取卦辞(本卦)}";
                    break;
                case 1:
                    解卦描述 = $"{取卦名(本卦)}之{取卦名(之卦)}\r\n" +
                        $"变爻：" +
                        $"{取爻名(变爻[0], 本卦[变爻[0]])}\r\n" +
                        $"{取爻辞(本卦, 变爻[0])}";
                    break;
                case 2:
                    解卦描述 = $"{取卦名(本卦)}之{取卦名(之卦)}\r\n" +
                        $"变爻有二：" +
                        $"{取爻名(变爻[0], 本卦[变爻[0]])}、" +
                        $"{取爻名(变爻[1], 本卦[变爻[1]])}\r\n" +
                        $"{取爻辞(本卦, 变爻[0])}\r\n{取爻辞(本卦, 变爻[1])}";
                    break;
                case 3:
                    解卦描述 = $"{取卦名(本卦)}之{取卦名(之卦)}\r\n" +
                        $"变爻有三：" +
                        $"{取爻名(变爻[0], 本卦[变爻[0]])}、" +
                        $"{取爻名(变爻[1], 本卦[变爻[1]])}、" +
                        $"{取爻名(变爻[2], 本卦[变爻[2]])}\r\n" +
                        $"{取卦辞(本卦)}\r\n{取卦辞(之卦)}"; break;
                case 4:
                    解卦描述 = $"{取卦名(本卦)}之{取卦名(之卦)}\r\n" +
                        $"变爻有四：" +
                        $"{取爻名(变爻[0], 本卦[变爻[0]])}、" +
                        $"{取爻名(变爻[1], 本卦[变爻[1]])}、" +
                        $"{取爻名(变爻[2], 本卦[变爻[2]])}、" +
                        $"{取爻名(变爻[3], 本卦[变爻[3]])}\r\n" +
                        $"{取爻辞(之卦, 变爻[0])}\r\n{取爻辞(之卦, 变爻[1])}";
                    break;
                case 5:
                    解卦描述 = $"{取卦名(本卦)}之{取卦名(之卦)}\r\n" +
                        $"变爻有五：" +
                        $"{取爻名(变爻[0], 本卦[变爻[0]])}、" +
                        $"{取爻名(变爻[1], 本卦[变爻[1]])}、" +
                        $"{取爻名(变爻[2], 本卦[变爻[2]])}、" +
                        $"{取爻名(变爻[3], 本卦[变爻[3]])}、" +
                        $"{取爻名(变爻[4], 本卦[变爻[4]])}\r\n" +
                        $"{取爻辞(之卦, 变爻[0])}";
                    break;
                case 6:
                    解卦描述 = $"{取卦名(本卦)}\r\n之{取卦名(之卦)}，六爻皆变\r\n" +
                        $"{取卦辞(之卦)}";
                    break;
                default:
                    break;
            }
            return 解卦描述;
        }


        public static string 取Unicode卦象字符(string 卦名)
        {
            string[] 文王卦序 = {
                "乾", "坤", "屯", "蒙", "需", "讼", "师", "比",
                "小畜", "履", "泰", "否", "同人", "大有", "谦", "豫",
                "随", "蛊", "临", "观", "噬嗑", "贲", "剥", "复",
                "无妄", "大畜", "颐", "大过", "坎", "离", "咸", "恒",
                "遁", "大壮", "晋", "明夷", "家人", "睽", "蹇", "解",
                "损", "益", "夬", "姤", "萃", "升", "困", "井",
                "革", "鼎", "震", "艮", "渐", "归妹", "丰", "旅",
                "巽", "兑", "涣", "节", "中孚", "小过", "既济", "未济"
            };
            return char.ConvertFromUtf32(Convert.ToInt32("4DC0", 16) + Array.IndexOf(文王卦序, 卦名));
        }

        /// <summary>
        /// 取卦名
        /// </summary>
        /// <param name="卦象"></param>
        /// <returns></returns>
        public string 取卦名(string 卦象)
        {
            string 卦名 = 卦辞原文[卦象][0];
            return $"{取Unicode卦象字符(卦名)}{卦名}";
            //return $"{卦辞原文[卦象][0]}({卦辞原文[卦象][1]}，{卦辞原文[卦象][2]})";
        }

        /// <summary>
        /// 取卦辞
        /// </summary>
        /// <param name="卦象"></param>
        /// <returns></returns>
        public string 取卦辞(string 卦象)
        {
            return $"★{卦辞原文[卦象][3]}\r\n{卦辞原文[卦象][4]}";
            //return $"★{guaci[gua][3]}\r\n{guaci[gua][4]}\r\n{guaci[gua][6]}";
        }
        /// <summary>
        /// 爻描述
        /// </summary>
        /// <param name="num"></param>
        /// <param name="yinyang"></param>
        /// <returns></returns>
        public string 取爻名(int num, char yinyang)
        {
            if (yinyang == '1')
            {
                switch (num)
                {
                    case 0: return "初九";
                    case 1: return "九二";
                    case 2: return "九三";
                    case 3: return "九四";
                    case 4: return "九五";
                    case 5: return "上九";
                    default: break;
                }
            }
            else if (yinyang == '0')
            {
                switch (num)
                {
                    case 0: return "初六";
                    case 1: return "六二";
                    case 2: return "六三";
                    case 3: return "六四";
                    case 4: return "六五";
                    case 5: return "上六";
                    default: break;
                }
            }
            return "";
        }

        /// <summary>
        /// 取爻辞
        /// </summary>
        /// <param name="卦名"></param>
        /// <param name="爻数"></param>
        /// <returns></returns>
        public string 取爻辞(string 卦名, int 爻数)
        {
            return $"★{取爻名(爻数, 卦名[爻数])}，{爻辞原文[卦名][爻数, 0]}\r\n{爻辞原文[卦名][爻数, 1]}";
            //return $"★{getYaoPos(yao, gua[yao])}，{yaoci[gua][yao, 0]}\r\n{yaoci[gua][yao, 1]}\r\n{yaoci[gua][yao, 3]}";
        }




        
    }
}
