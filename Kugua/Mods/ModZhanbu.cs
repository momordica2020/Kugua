using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Kugua
{
    /// <summary>
    /// 占卜
    /// </summary>
    public class ModZhanbu : Mod
    {
        Dictionary<string, string[]> guaci = new Dictionary<string, string[]>();
        Dictionary<string, string[,]> yaoci = new Dictionary<string, string[,]>();





        public override bool Init(string[] args)
        {
            try
            {
                ModCommands[new Regex("^占卜(.*)", RegexOptions.Singleline)] = getZhouYi;


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
                        guaci[nowGuaNum] = new string[7];
                        yaoci[nowGuaNum] = new string[6, 4];
                        nowline = 0;
                    }
                    else
                    {
                        if (nowline == 1)
                        {
                            items = line.Trim().Split(' ');
                            guaci[nowGuaNum][0] = items[0];
                            guaci[nowGuaNum][1] = items[1];
                            guaci[nowGuaNum][2] = items[2];
                        }
                        else if (nowline == 2) guaci[nowGuaNum][3] = line.Trim().Substring(guaci[nowGuaNum][0].Length + 1);
                        else if (nowline == 3) guaci[nowGuaNum][4] = line.Trim().Substring(3);
                        else if (nowline == 4) guaci[nowGuaNum][5] = line.Trim().Substring(3);
                        else if (nowline == 5) guaci[nowGuaNum][6] = line.Trim().Substring(3);
                        else if (nowline >= 6) yaoci[nowGuaNum][(nowline - 6) / 4, (nowline - 6) % 4] = line.Trim().Substring(3);
                    }
                }
            }catch(Exception) { }
            


            return true;
        }

        public void Exit()
        {
            
        }

       



        /// <summary>
        /// 取卦名
        /// </summary>
        /// <param name="gua"></param>
        /// <returns></returns>
        public string getGuaming(string gua)
        {
            return $"{guaci[gua][0]}({guaci[gua][1]}，{guaci[gua][2]})";
        }

        /// <summary>
        /// 取卦辞
        /// </summary>
        /// <param name="gua"></param>
        /// <returns></returns>
        public string getGuaci(string gua)
        {
            return $"★{guaci[gua][3]}\r\n{guaci[gua][4]}";
            //return $"★{guaci[gua][3]}\r\n{guaci[gua][4]}\r\n{guaci[gua][6]}";
        }

        /// <summary>
        /// 取爻辞
        /// </summary>
        /// <param name="gua"></param>
        /// <param name="yao"></param>
        /// <returns></returns>
        public string getYaoci(string gua, int yao)
        {
            return $"★{getYaoPos(yao, gua[yao])}，{yaoci[gua][yao, 0]}\r\n{yaoci[gua][yao, 1]}";
            //return $"★{getYaoPos(yao, gua[yao])}，{yaoci[gua][yao, 0]}\r\n{yaoci[gua][yao, 1]}\r\n{yaoci[gua][yao, 3]}";
        }

        /// <summary>
        /// 取得卦象
        /// </summary>
        /// <returns></returns>
        public string getZhouYi(MessageContext context, string[] param)
        {
            string yao = "";
            string gua1 = "";
            string gua2 = "";
            for (int i = 0; i < 6; i++)
            {
                yao += getYao();
            }

            List<int> notchanges = new List<int>();
            List<int> changes = new List<int>();
            string yao2 = "";
            for (int i = 0; i < 6; i++)
            {
                gua1 += (yao[i] == '6' || yao[i] == '8') ? '0' : '1';
                if (yao[i] == '6')
                {
                    changes.Add(i);
                    yao2 = yao2 + "7";
                }
                else if (yao[i] == '9')
                {
                    changes.Add(i);
                    yao2 = yao2 + "8";
                }
                else
                {
                    notchanges.Add(i);
                    yao2 += yao[i];
                }
                gua2 += (yao2[i] == '6' || yao2[i] == '8') ? '0' : '1';
            }

            string result = "";
            switch (changes.Count)
            {
                case 0:
                    result = $"主卦：{getGuaming(gua1)}，无变卦\r\n" +
                        $"{getGuaci(gua1)}";
                    break;
                case 1:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有一：{getYaoPos(changes[0], gua1[changes[0]])}\r\n" +
                        $"{getYaoci(gua1, changes[0])}";
                    break;
                case 2:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有二：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}\r\n" +
                        $"{getYaoci(gua1, changes[0])}\r\n{getYaoci(gua1, changes[1])}";
                    break;
                case 3:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有三：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}\r\n" +
                        $"{getGuaci(gua1)}\r\n{getGuaci(gua2)}"; break;
                case 4:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有四：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}、{getYaoPos(changes[3], gua1[changes[3]])}\r\n" +
                        $"{getYaoci(gua2, changes[0])}\r\n{getYaoci(gua2, changes[1])}";
                    break;
                case 5:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有五：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}、{getYaoPos(changes[3], gua1[changes[3]])}、{getYaoPos(changes[4], gua1[changes[4]])}\r\n" +
                        $"{getYaoci(gua2, changes[0])}";
                    break;
                case 6:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n六爻皆变\r\n" +
                        $"{getGuaci(gua2)}";
                    break;
                default:
                    break;
            }
            return result;
        }












        #region 蓍草筮法

        

        int 大衍之数 = 50;
        class 蓍草
        {

        }

        List<蓍草> 全部蓍草;
        List<蓍草> 挂出;
        List<蓍草> 揲出;

        List<int> 卜卦()
        {
            全部蓍草 = new List<蓍草>();
            for (int i = 0; i < 大衍之数; i++) 全部蓍草.Add(new 蓍草());
            挂出 = new List<蓍草>();
            揲出 = new List<蓍草>();
            List<int> 爻 = new List<int>();

            for(int i = 0;i <6;i++)
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
            for(int i = 0; i < 3; i++)
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

        int minnum = 1;
        /// <summary>
        /// 爻描述
        /// </summary>
        /// <param name="num"></param>
        /// <param name="yinyang"></param>
        /// <returns></returns>
        public string getYaoPos(int num, char yinyang)
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
        /// 取爻
        /// </summary>
        /// <returns></returns>
        int getYao()
        {
            // 大衍之数五十
            int allnum = 50;
            // 其用四十有九
            allnum -= 1;

            // 十有八变而得卦
            allnum = Bian(allnum);
            //Debug.WriteLine(allnum);
            allnum = Bian(allnum);
            //Debug.WriteLine(allnum);
            allnum = Bian(allnum);

            //Debug.WriteLine(allnum);
            allnum /= 4;

            return allnum;
        }
       
        /// <summary>
        /// 変爻
        /// </summary>
        /// <param name="allnum"></param>
        /// <returns></returns>
        int Bian(int allnum)
        {
            int left = 0;
            int right = 0;
            int middle = 0;

            middle = 1; // 挂一以象人

            left = MyRandom.Next(1, allnum);
            if (left < minnum)
            {
                left += minnum;
                right -= minnum;
            }
            else if (right < minnum)
            {
                right += minnum;
                left -= minnum;
            }
            right -= 1;
            middle += 1;
            int leftmod = left % 4;
            if (leftmod == 0) leftmod = 4;
            left -= leftmod;
            middle += leftmod;
            int rightmod = right % 4;
            if (rightmod == 0) rightmod = 4;
            right -= rightmod;
            middle += rightmod;

            return left + right;
        }

    internal record struct NewStruct(int v, object Item2)
    {
        public static implicit operator (int v, object)(NewStruct value)
        {
            return (value.v, value.Item2);
        }

        public static implicit operator NewStruct((int v, object) value)
        {
            return new NewStruct(value.v, value.Item2);
        }
    }

 
}


}
