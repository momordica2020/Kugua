using Kugua.Core;
using Kugua.Integrations.AI;
using Newtonsoft.Json;
using System.Data;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Kugua.Mods{


    public partial class GameXiuxian
    {
        

        public static string AGgetEnemys(string area, int maxnum)
        {
            string res = "";
            var enemylists = new List<string>();
            if (maxnum <= 1)
            {
                enemylists.Add(AGwordSingle($"{area}场景里的敌人"));
            }
            else
            {
                enemylists = AGwordlist($"{area}场景里的{maxnum}种敌人");
            }
            if (enemylists.Where(e => e == area).FirstOrDefault() is string enemyWrongName) enemylists.Remove(enemyWrongName);
            for(int i = 0; i < Math.Min(maxnum, enemylists.Count); i++)
            {
                res += $"{MyRandom.Next(1, 3)}个{enemylists[i]},";
            }

            return res;
        }
        
       

        public static string AGSaleItems(XiuxianUser user, List<XiuxianItem> items, string action)
        {
            string desc = "";
            string itemsdesc = "";
            if (items.Count <= 1) itemsdesc = $"{items[0].name}({items[0].desc})";
            else itemsdesc = $"{string.Join("、", items.Select(it => it.name))}";

            string propName = "灵石";
            if (action == "炼化") propName = "灵力";
            if (!user.prop.ContainsKey(propName)) user.prop[propName] = 0;
            var oldval = user.prop[propName];
            var newdata = AGsetNum($"{user.LevelName}{user.race}{action}{itemsdesc}之后改变的{propName}值(-10~+10)，你只需要输出变化的数值，比如+3，不要输出额外解释");
            if (newdata > 0) newdata = BigInteger.Max(newdata, oldval * newdata / 80);
            else newdata = BigInteger.Min(newdata, oldval * newdata / 80);
            user.prop[propName] += newdata;
            
            desc = AGdesc($"{user.race}{user.FullName}{action}了{itemsdesc}，获得了{newdata.ToSci()}{propName}。");



            return desc;
        }
        public static string AGUseItems(XiuxianUser user, List<XiuxianItem> items,string action)
        {
            string desc = "";
            string propsdesc = GetUserPropDescRandomNumber(user);
            string propnamesdesc = GetUserPropNameRandomNumber(user);
            string itemsdesc = "";
            if (items.Count <= 1) itemsdesc = $"{items[0].name}({items[0].desc})";
            else itemsdesc = $"{string.Join("、", items.Select(it => it.name))}";
  
            var props = AGwordlist($"{user.LevelName}{user.race}{action}{itemsdesc}影响了哪些属性？待选项：{propnamesdesc}");
            List<(string, BigInteger)> pval =new List<(string, BigInteger)> ();
            foreach(var prop in props)
            {
                    
                BigInteger oldval = 0;
                if(user.prop.ContainsKey(prop))oldval = user.prop[prop];
                var newdata = AGsetNum($"{user.LevelName}{user.race}{action}{itemsdesc}之后的{prop}值改变量(-10~+10)，使用者目前属性列表：{propsdesc}，,你只需要输出一个结果的数值，比如+3，不要输出额外解释");
                if (newdata > 0) newdata = BigInteger.Max(newdata, oldval * newdata / 80);
                else newdata = BigInteger.Min(newdata, oldval * newdata / 80);
                
                user.prop[prop] = oldval + newdata;
                pval.Add((prop, newdata));
            }
            desc = AGdesc($"{user.race}{user.FullName}{action}{itemsdesc}，影响：{string.Join(",", pval.Select(e => $"{e.Item1}{(e.Item2>0?"+":"")}{e.Item2.ToSci()}"))}。");



            return desc;
        }

        public static string AGrace(XiuxianUser user, string action)
        {
            string race = "";
            if (!names.ContainsKey("种族")) names["种族"] = new List<string>();
            var list = names["种族"];
            race = AGword($"昵称{user.FullName}的修炼者在{MyRandom.NextString(NovelScene)}世界的种族是什么？可以不拘一格", list);
            if (!list.Contains(race)) list.Add(race);

            return race;
        }

        public static string AGarea(XiuxianUser user, string action)
        {
            string area = "";

            if (!names.ContainsKey("地区")) names["地区"] = new List<string>();
            var list = names["地区"];
            area = AGword($"{user.FullName}在{MyRandom.NextString(NovelScene)}风格小说里{action}的地区？可以不拘一格");
            if (!list.Contains(area)) list.Add(area);

            return area;
        }

        public static string AGlevel(string race, int level)
        {
            int jlevel = 0;
            List<string> jlavelTopName = new List<string>();// 顶层境界名，比如xx初级和xx圆满只记录一个xx
            List<string> jlavelAllName = new List<string>();

            string levelDataKey = $"{race}-境界";
            if (!names.ContainsKey(levelDataKey))
            {
                names.Add(levelDataKey, new List<string>());
            }
            foreach (var nameItem in names[levelDataKey])
            {
                string name = nameItem.Split('\t')[0];
                int num = int.Parse(nameItem.Split('\t')[1]);
                jlavelTopName.Add(name);
                if (num <= 1) jlavelAllName.Add($"{name}");
                else if (num == 2) jlavelAllName.AddRange([$"{name}初期",$"{name}后期"]);
                else if (num == 3) jlavelAllName.AddRange([$"{name}初期", $"{name}中期", $"{name}后期"]);
                else if (num == 4) jlavelAllName.AddRange([$"半步{name}", $"{name}初期", $"{name}中期", $"{name}后期"]);
            }
            while (level > jlavelAllName.Count)
            {
                jlavelTopName.Add(AGword($"{race}族第{jlavelTopName.Count}级的修仙境界名称？", jlavelTopName));
                string name = jlavelTopName.Last();
                int num = MyRandom.Next(2, 5); 
                if (num == 2) jlavelAllName.AddRange([$"{name}初期", $"{name}后期"]);
                else if (num == 3) jlavelAllName.AddRange([$"{name}初期", $"{name}中期", $"{name}后期"]);
                else if (num == 4) jlavelAllName.AddRange([$"半步{name}", $"{name}初期", $"{name}中期", $"{name}后期"]);
                names[levelDataKey].Add($"{name}\t{num}");
            }
            return jlavelAllName[level - 1];
        }

        public static string AGdesc(string use, int maxlen = 150)
        {
            //string res;
            string style = MyRandom.NextString(NovelStyles);
            return LLM.Instance.HSSendSingle($"作为文章修改器，把情节润色为相应{style}风格的段落，你只能返回纯文本，尽量在{maxlen}字以内，必须包含我给出的所有数值参数。必须使用{style}风格",
                $"{use}");

        }

        public static BigInteger AGsetNum(string use)
        {
            BigInteger res = 0;
            BigInteger.TryParse(LLM.Instance.HSSendSingle($"作为{MyRandom.NextString(NovelScene)}RPG数值计算器模块，你只能返回代表结果的数值，如果遇到模糊就随便给个数字即可，不要做任何解释，不输出任何额外内容。",
                $"{use}"),out res);
            return res;
        }


        public static string AGwordSingle(string use)
        {
            //string res;
            return LLM.Instance.HSSendSingle($"扮演{MyRandom.NextString(NovelScene)}撰写bot，生成相应的词语，你不要做任何解释，如果遇到模糊就随便生成即可，只能返回纯文本的词语生成结果，不同项使用中文逗号隔开。",
                $"{use}").Split([',', '，'], StringSplitOptions.TrimEntries).First();

        }
        public static List<string> AGwordlist(string use)
        {
            //string res;
            return LLM.Instance.HSSendSingle($"扮演{MyRandom.NextString(NovelScene)}撰写bot，生成相应的词语序列，你不要做任何解释，如果遇到模糊就随便生成即可，只能返回纯文本，不同项使用中文逗号隔开。",
                $"{use}").Split([',', '，'], StringSplitOptions.TrimEntries).ToList();

        }

        public static string AGchooseOne(string use, IEnumerable<string> exists)
        {
            //string res;
            return LLM.Instance.HSSendSingle($"扮演{MyRandom.NextString(NovelScene)}撰写bot，帮我选取一个词汇，你只能返回选择的结果结果词汇，如果遇到模糊就随便给个词即可，不要做任何解释。",
                $"{use}{((exists == null || exists.Count() <= 0) ? "" : $" 待选列表：{string.Join("、", exists)}")}");

        }




        public static string AGword(string use, IEnumerable<string> exists = null)
        {
            //string res;成“
            string res = LLM.Instance.HSSendSingle($"作为{MyRandom.NextString(NovelScene)}小说辅助生成器模块，生成相应的专有词汇，你只能返回一个结果词汇，不要与已有的重复。如果遇到模糊就随便给个词即可，不要做任何解释和额外输出。",
                $"{use}{((exists == null || exists.Count()<=0) ? "" : $" 已有的包括：{string.Join("、", exists)}")}");

            if (res.Contains("：")) res = res.Substring(res.IndexOf("：")).Trim('：');
            int firstQuote = res.IndexOf('“');
            int secondQuote = res.IndexOf('”', firstQuote + 1);
            if (firstQuote != -1 && secondQuote != -1 && secondQuote > firstQuote)
            {
                res = res.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
            }
            return res;
        }


        public static bool AGjudge(XiuxianItem item, string use)
        {
            //string res;
            string res= LLM.Instance.HSSendSingle($"作为RPG规则判断者，你只能返回单个数字表示判断结果，1是允许，0是不许。",
                $"物品：{item.name}({item.desc}),{use}");
            return res.Trim() == "1";
        }

    }
}