using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoogleTranslateFreeApi;
using Kugua.Integrations.NTBot;



namespace Kugua
{
    class ModTranslate : Mod
    {
        Dictionary<string, string> ctlist = new Dictionary<string, string>();


        public const string TranslateURL = "https://translate.google.cn/translate_a/single?client=gtx&dt=t&ie=UTF-8&oe=UTF-8&sl=auto&tl=zh-CN";

        private Regex regex = new Regex("(?<=\\[\\\").*?(?=\\\")");
        //替换掉翻译结果中的id
        private Regex rreplaceid = new Regex("\\[\\[\\[\\\"[0-9a-z]+\\\"\\,\\\"\\\"\\]");

        public override bool Init(string[] args)
        {
            try
            {
                ctlist = new Dictionary<string, string>();
                var lines = LocalStorage.ReadResourceLines("Translate");
                foreach (var line in lines)
                {
                    string[] vitem = line.Split('\t');
                    if (vitem.Length >= 2) ctlist[vitem[0]] = vitem[1];
                }



                ModCommands.Add(new Regex(@"^", RegexOptions.Singleline), Translate);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message + "\r\n" + e.StackTrace);
            }
            return true;
        }

        private string Translate(MessageContext context, string[] param)
        {
            try
            {
                // 翻译
               
                
            }
            catch (Exception ex)
            {

            }
            return "";
        }


        
    }
}
