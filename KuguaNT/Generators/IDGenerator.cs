using Kugua.Core;
using Microsoft.AspNetCore.Identity.Data;
using System.Text;

namespace Kugua.Generators
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
                
                
                res = $"用户：{userName}";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return res;
        }


    }
}
