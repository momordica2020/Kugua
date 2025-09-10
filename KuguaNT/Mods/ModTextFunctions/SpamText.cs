using Kugua.Core;
using Kugua.Mods.ModTextFunctions;
using Microsoft.AspNetCore.Identity.Data;
using System.Text;

namespace Kugua
{
    /// <summary>
    /// 营销号文档模板填充相关
    /// </summary>
    public class SpamText
    {     
        static List<DValue> SpamTemplates = new List<DValue>();
        //static List<List<string>> junks = new List<List<string>>();
        public static void Init(string[] data)
        {
            //List<string> nowline = new List<string>();
            foreach (var line in data)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if(SpamTemplates.Count <= 0 || SpamTemplates.Last().Templates.Count > 0)
                    {
                        Logger.Log($"new ! {SpamTemplates.Count}");
                        SpamTemplates.Add(new DValue());
                    }
                }
                else
                {
                    if (SpamTemplates.Count <= 0) SpamTemplates.Add(new DValue());
                    SpamTemplates.Last().Templates.Add(line);
                    Logger.Log($"{SpamTemplates.Count}");
                }
            }
        }



        public static string GetRandomSpam(string topic)
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

                Logger.Log($"template count = {SpamTemplates.Count}");
                foreach (var template in SpamTemplates)
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
