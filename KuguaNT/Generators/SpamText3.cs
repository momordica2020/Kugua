using Kugua.Core;
using Microsoft.AspNetCore.Identity.Data;
using System.Text;

namespace Kugua.Generators
{
    /// <summary>
    /// 另一种狗屁不通论文文档模板填充相关
    /// </summary>
    public class SpamText3
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
                    new DValue("主题", [topic]),
                    new DValue("小编", ["我苦"]),
                ];
                return SpamTemplate.GetDefaultResult(null, param);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return "";
        }


    }
}
