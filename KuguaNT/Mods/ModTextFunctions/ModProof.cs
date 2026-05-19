using Kugua.Algorithms;
using Kugua.Core;
using Kugua.Mods.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;




namespace Kugua.Mods.ModTextFunctions
{

    /// <summary>
    /// 字符串数字论证
    /// </summary>
    public class ModProof : Mod
    {
        Dictionary<string, int> bhdict = new Dictionary<string, int>();
        MathProof mathProof;
        List<int> HOMO_NUMBERS = [1, 1, 4, 5, 1, 4];


        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex(@"^数字论证(.+)", RegexOptions.Singleline), GetProof));

                mathProof = new MathProof();

                bhdict = new Dictionary<string, int>();
                var lines = FileSystem.ReadResourceLines("Bihua");
                foreach (var line in lines)
                {
                    string[] vitem = line.Split('\t');
                    if (vitem.Length >= 2) bhdict[vitem[0]] = int.Parse(vitem[1]);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            return true;
        }

        /// <summary>
        /// homo特有的数字论证
        /// 数字论证犬走椛/数字论证12dora
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string GetProof(MessageContext context, string[] param)
        {
            var message = param[1].Trim();
            long trynum;
            bool succeed = false;
            if (long.TryParse(message, out trynum))
            {
                // 纯数字
                var proofChain = mathProof.Proof(trynum,HOMO_NUMBERS);
                if (proofChain.Count > 0)
                {
                    return $"{trynum}={string.Join("=", proofChain)}\r\nQ.E.D";
                }
            }
            else
            {
                const string numb = "0123456789零一二三四五六七八九十壹贰叁肆伍陆柒捌玖";
                const string engc = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string symb = "\t\r\n []【】！@#￥%…&*（）+=-—_!@#$%^&*()|/\\。、，？?“”\"',";
                //bool num = false;
                bool eng = false;
                bool chn = false;
                var tmp = new List<(char c , int v)>(); 
                foreach(var c in message)
                {
                    //long sum = 0;
                    if (symb.Contains(c)) continue;
                    if(numb.Contains(c))
                    {
                        tmp.Add((c, numb.IndexOf(c) % 10 ));
                        //num = true;
                        continue;
                    }
                    if (engc.Contains(c))
                    {
                        tmp.Add((c, engc.IndexOf(c) % 26 + 1));
                        eng = true;
                        continue;
                    }
                    if(bhdict.TryGetValue(c.ToString(), out var cv))
                    {
                        tmp.Add((c, cv));
                        chn = true;
                        continue;
                    }
                }

                if (tmp.Count > 0) {
                    var trysum = tmp.Sum(a => a.v);
                    string transDescription = "";
                    if (eng || chn)
                    {
                        transDescription = "按";
                        if (eng) transDescription += $"字母序号，";
                        if (chn) transDescription += $"笔画数，";
                        transDescription += $"就是{string.Join("+", tmp)}";
                    }
                    else transDescription = $"就是{string.Join("+", tmp)}";
                    
                    var proofChain = mathProof.Proof(trynum, HOMO_NUMBERS);
                    if (proofChain.Count > 0)
                    {
                        return $"{transDescription}={trynum}={string.Join("=", proofChain)}\r\nQ.E.D";
                    }
                }
            }
            return $"论不出来";
        }
       
    }


}
