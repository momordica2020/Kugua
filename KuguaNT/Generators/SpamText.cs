using Kugua.Core;
using Microsoft.AspNetCore.Identity.Data;
using System.Text;

namespace Kugua.Generators
{
    /// <summary>
    /// 营销号文档模板填充相关
    /// </summary>
    public class SpamText
    {
        static DScript SpamTemplate = new DScript();

        public static void Init(string data)
        {
            SpamTemplate.Load(data);

        }



        public static string Get(string topic)
        {
            string res = "";
            try
            {
                if (string.IsNullOrWhiteSpace(topic)) return res;

                List<DValue> param = [
                    new DValue("【A】", [topic]),
                    new DValue("【B】", [new DValue(["朋友", "小伙伴", "网友"]).Result()]),
                    new DValue("【C】", [Config.Instance.App.Avatar.askName]),
                    new DValue("【D】", [Config.Instance.App.Avatar.myQQ.ToString()]),
                    new DValue("【E】", [DateTime.Now.Year.ToString()]),
                ];

                //Logger.Log($"template count = {SpamTemplate.AnonymousTemplates.Count}");
                foreach (var template in SpamTemplate.AnonymousTemplates)
                {
                    res += $"{template.Result(param)}\r\n";
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return res;
        }


    }
}
