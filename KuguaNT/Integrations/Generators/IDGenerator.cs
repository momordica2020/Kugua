using Kugua.Core;
using Kugua.Integrations.Generators.Base;
using Microsoft.AspNetCore.Identity.Data;
using System.Text;

namespace Kugua.Integrations.Generators
{
    /// <summary>
    /// 户籍信息生成
    /// </summary>
    public class IDGenerator
    {
        public static DScript IDTemplate = new DScript();
        //static List<List<string>> junks = new List<List<string>>();
        public static void Init(string data)
        {
            IDTemplate.Load(data);
           
        }



        public static string Get(string userName)
        {
            string res = "";
            try
            {
                if (string.IsNullOrWhiteSpace(userName)) return res;
                
                
                res += $"昵称：{userName}\r\n";
                res += $"真实姓名：{userName}\r\n";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return res;
        }




        public static string GetChineseName(int length)
        {
            return "";
        }

    }
}
