using Kugua.Integrations.Generators.Base;

namespace Kugua.Integrations.Generators
{
    /// <summary>
    /// 攻受文模板填充相关
    /// </summary>
    public class Gongshou
    {
        public static DScript GongshouTemplate = new DScript();
        //static List<List<string>> junks = new List<List<string>>();
        public static void Init(string data)
        {
            GongshouTemplate.Load(data);
           
        }



        public static string Get(string gong, string shou)
        {
            string res = "";
            try
            {
                if (string.IsNullOrWhiteSpace(gong)) return res;
                if (string.IsNullOrWhiteSpace(shou)) return res;
                if(GongshouTemplate.AnonymousTemplates==null || GongshouTemplate.AnonymousTemplates.Count<=0)
                {
                    Logger.Log($"攻受模块没有数据。");
                    return res;
                }


                List<DValue> param = [
                    new DValue("【攻】", [gong]),
                    new DValue("【受】", [shou]),
                ];
                // 从攻受模板列表中随机选择一个
                res = GongshouTemplate.GetResult(null, param);

                //var template = GongshouTemplate.AnonymousTemplates[MyRandom.Next(GongshouTemplate.AnonymousTemplates.Count)];
                //res = template.Result(param);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return res;
        }


    }
}
