using Kugua.Core;
using System.Text;

namespace Kugua.Generators
{
    /// <summary>
    /// 吃的的随机模板填充相关
    /// </summary>
    public class EatText
    {
        static DScript FoodTemplate = new DScript();

        public static void Init(string data)
        {
            FoodTemplate.Load(data);
            //Logger.Log($"{string.Join(",",FoodTemplate.NamedTemplates.Select(s=>$"{s.Key}={s.Value}"))}...{FoodTemplate.AnonymousTemplates.Count}");
        }



        public static string Get(string time, string verb)
        {
            try
            {
                List<DValue> param = [
                    new DValue("【time】", [$"{time}"]),
                    new DValue("【verb】", [$"{verb}"]),
                   // new DValue("$FFF$", [$""]),
                ];
                string res = FoodTemplate.GetResult(null, param);
                res = res.Replace("我", "【W】").Replace("你", "【N】").Replace("【W】", "你").Replace("【N】", "我");
                if (res.Contains("【"))
                {
                    // not merge
                    res = FoodTemplate.GetResult("【UNKNOWN】", param);
                    if (res.Contains("【")) res = "我不知道";
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return "";
        }


    }
}
