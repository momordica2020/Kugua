using Kugua.Core;
using Kugua.Integrations.Generators.Base;
using Microsoft.AspNetCore.Identity.Data;
using System.Text;

namespace Kugua.Integrations.Generators
{
    /// <summary>
    /// 另一种狗屁不通论文文档模板填充相关
    /// </summary>
    public class SpamText2
    {
        static DScript SpamTemplate = new DScript();

        public static void Init(string data)
        {
            SpamTemplate.Load(data);

        }



        public static string Get(string topic, int length = 500)
        {
            StringBuilder res = new StringBuilder();
            try
            {
                if (string.IsNullOrWhiteSpace(topic)) return "";

                List<DValue> param = [
                    new DValue("x", [topic]),
                ];
                int maxCount = 100;
                while(maxCount-- > 0 && res.Length < length)
                {
                    res.Append(SpamTemplate.GetResult(null, param));
                    if (MyRandom.NextDouble <= 0.05) res.Append("\r\n");
                }
                //return SpamTemplate.GetDefaultResult(null, param);
                return res.ToString();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return "";
        }


    }
}
