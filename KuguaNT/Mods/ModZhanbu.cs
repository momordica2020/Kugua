using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Kugua.Mods
{
    /// <summary>
    /// 占卜
    /// </summary>
    public class ModZhanbu : Mod
    {
        Dictionary<string, string[]> 卦辞原文 = new Dictionary<string, string[]>();
        Dictionary<string, string[,]> 爻辞原文 = new Dictionary<string, string[,]>();

        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex("^占卜(.*)", RegexOptions.Singleline), 解读卦象));


                var lines = LocalStorage.ReadResourceLines("Zhouyi");
                string nowGuaNum = "";
                int nowline = 0;
                string[] items;
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
                            items = line.Trim().Split(' ');
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
            }
            catch(Exception) { }
            


            return true;
        }



        /// <summary>
        /// 让bot替你摇一卦，后面可加缘由也可不加
        /// 占卜/占卜今日运势
        /// </summary>
        /// <returns></returns>
        public string 解读卦象(MessageContext context, string[] param)
        {
            int[] 本卦爻;
            string 本卦卦象 = "";
            string 之卦卦象 = "";

            本卦爻 = 卜卦().ToArray();

            List<int> 不变爻 = new List<int>();
            List<int> 变爻 = new List<int>();
            string 之卦爻 = "";
            for (int i = 0; i < 6; i++)
            {
                本卦卦象 += (本卦爻[i] == 6 || 本卦爻[i] == 8) ? '0' : '1';
                if (本卦爻[i] == '6')
                {
                    变爻.Add(i);
                    之卦爻 = 之卦爻 + "7";
                }
                else if (本卦爻[i] == '9')
                {
                    变爻.Add(i);
                    之卦爻 = 之卦爻 + "8";
                }
                else
                {
                    不变爻.Add(i);
                    之卦爻 += 本卦爻[i];
                }
                之卦卦象 += (之卦爻[i] == '6' || 之卦爻[i] == '8') ? '0' : '1';
            }

            string 解卦描述 = "";
            switch (变爻.Count)
            {
                case 0:
                    解卦描述 = $"主卦：{取卦名(本卦卦象)}，无变卦\r\n" +
                        $"{取卦辞(本卦卦象)}";
                    break;
                case 1:
                    解卦描述 = $"主卦：{取卦名(本卦卦象)}\r\n变卦：{取卦名(之卦卦象)}\r\n" +
                        $"变爻有一：{取爻名(变爻[0], 本卦卦象[变爻[0]])}\r\n" +
                        $"{取爻辞(本卦卦象, 变爻[0])}";
                    break;
                case 2:
                    解卦描述 = $"主卦：{取卦名(本卦卦象)}\r\n变卦：{取卦名(之卦卦象)}\r\n" +
                        $"变爻有二：{取爻名(变爻[0], 本卦卦象[变爻[0]])}、{取爻名(变爻[1], 本卦卦象[变爻[1]])}\r\n" +
                        $"{取爻辞(本卦卦象, 变爻[0])}\r\n{取爻辞(本卦卦象, 变爻[1])}";
                    break;
                case 3:
                    解卦描述 = $"主卦：{取卦名(本卦卦象)}\r\n变卦：{取卦名(之卦卦象)}\r\n" +
                        $"变爻有三：{取爻名(变爻[0], 本卦卦象[变爻[0]])}、{取爻名(变爻[1], 本卦卦象[变爻[1]])}、{取爻名(变爻[2], 本卦卦象[变爻[2]])}\r\n" +
                        $"{取卦辞(本卦卦象)}\r\n{取卦辞(之卦卦象)}"; break;
                case 4:
                    解卦描述 = $"主卦：{取卦名(本卦卦象)}\r\n变卦：{取卦名(之卦卦象)}\r\n" +
                        $"变爻有四：{取爻名(变爻[0], 本卦卦象[变爻[0]])}、{取爻名(变爻[1], 本卦卦象[变爻[1]])}、{取爻名(变爻[2], 本卦卦象[变爻[2]])}、{取爻名(变爻[3], 本卦卦象[变爻[3]])}\r\n" +
                        $"{取爻辞(之卦卦象, 变爻[0])}\r\n{取爻辞(之卦卦象, 变爻[1])}";
                    break;
                case 5:
                    解卦描述 = $"主卦：{取卦名(本卦卦象)}\r\n变卦：{取卦名(之卦卦象)}\r\n" +
                        $"变爻有五：{取爻名(变爻[0], 本卦卦象[变爻[0]])}、{取爻名(变爻[1], 本卦卦象[变爻[1]])}、{取爻名(变爻[2], 本卦卦象[变爻[2]])}、{取爻名(变爻[3], 本卦卦象[变爻[3]])}、{取爻名(变爻[4], 本卦卦象[变爻[4]])}\r\n" +
                        $"{取爻辞(之卦卦象, 变爻[0])}";
                    break;
                case 6:
                    解卦描述 = $"主卦：{取卦名(本卦卦象)}\r\n变卦：{取卦名(之卦卦象)}\r\n六爻皆变\r\n" +
                        $"{取卦辞(之卦卦象)}";
                    break;
                default:
                    break;
            }
            return 解卦描述;
        }



        /// <summary>
        /// 取卦名
        /// </summary>
        /// <param name="卦象"></param>
        /// <returns></returns>
        public string 取卦名(string 卦象)
        {
            return $"{卦辞原文[卦象][0]}({卦辞原文[卦象][1]}，{卦辞原文[卦象][2]})";
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




        #region 蓍草筮法



        int 大衍之数 = 50;
        class 蓍草
        {

        }

        List<蓍草> 全部蓍草;
        List<蓍草> 挂出;
        List<蓍草> 揲出;

        public List<int> 卜卦()
        {
            全部蓍草 = new List<蓍草>();
            for (int i = 0; i < 大衍之数; i++) 全部蓍草.Add(new 蓍草());
            挂出 = new List<蓍草>();
            揲出 = new List<蓍草>();
            List<int> 爻 = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                爻.Add(卜爻());
            }

            return 爻;
        }

        int 卜爻()
        {
            // 其用四十有九
            挂出.Add(全部蓍草.First());
            全部蓍草.Remove(全部蓍草.First());

            // 三易
            for (int i = 0; i < 3; i++)
            {
                四营();
            }
            // 查数画爻
            int 数 = 全部蓍草.Count / 4;
            回收蓍草();

            return 数;
        }

        void 四营()
        {
            分而为二(全部蓍草, out var 左, out var 右);
            挂一(右);
            揲之以四(左);
            揲之以四(右);
            归奇于扐(左);
            归奇于扐(右);
            挂();
        }


        void 分而为二(List<蓍草> 总, out List<蓍草> 左, out List<蓍草> 右)
        {
            var 划分处 = MyRandom.Next(1, 总.Count - 1);
            左 = 总.GetRange(0, 划分处);
            右 = 总.GetRange(划分处, 总.Count - 划分处);
            总.Clear();
        }

        void 挂一(List<蓍草> 草)
        {
            挂出.Add(草.First());
            草.Remove(草.First());
        }

        void 揲之以四(List<蓍草> 草)
        {
            while (草.Count > 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    揲出.Add(草.First());
                    草.Remove(草.First());
                }
            }
        }

        void 归奇于扐(List<蓍草> 草)
        {
            挂出.AddRange(草);
            草.Clear();
        }

        void 挂()
        {
            全部蓍草.AddRange(揲出.ToArray());
            揲出.Clear();
        }

        void 回收蓍草()
        {
            全部蓍草.AddRange(挂出.ToArray());
            挂出.Clear();
        }
        #endregion

    

}


}
