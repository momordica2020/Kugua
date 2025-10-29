using Kugua.Core;
using System.Text;

namespace Kugua.Generators
{
    /// <summary>
    /// 解梦的随机模板填充相关
    /// </summary>
    public class DreamText
    {
        static DScript DreamTemplate = new DScript();

        public static void Init(string data)
        {
            DreamTemplate.Load(data);
        }



        public static string Get(string verb)
        {
            try
            {
                List<DValue> param = [
                    new DValue("【key】", [$"{verb}"]),
                   // new DValue("$FFF$", [$""]),
                ];
                string res = DreamTemplate.GetResult(null, param);
                if (res.Contains("【"))
                {
                    res = "我不知道";
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
