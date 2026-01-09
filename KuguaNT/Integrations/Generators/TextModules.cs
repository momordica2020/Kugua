using Kugua.Core;
using Kugua.Integrations.Generators.Base;
using System.Text.RegularExpressions;

namespace Kugua.Integrations.Generators
{
    /// <summary>
    /// 基于模板的文本生成
    /// </summary>
    public class TextModules
    {

        public static DScript TTemplate = new DScript();
        

        public static void Init(string data)
        {
            TTemplate.Load(data);
        }


        //public static string ReplaceVN(string str)
        //{
        //    return ReplaceKey(str, new Regex("vn"), () =>
        //    {
        //        var vns = new List<string>();
        //        int count = GetRandomNumber(4) + 1;
        //        for (int i = 0; i < count; i++)
        //        {
        //            vns.Add(GetVerb() + GetNoun());
        //        }
        //        return string.Join("，", vns);
        //    });
        //}

      



        /// <summary>
        /// 随机生成指定主题关键词和长度的议论文章
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="essayNum"></param>
        /// <returns></returns>
        public static string Get(string theme, int essayNum = 500)
        {
            int beginNum = (int)(essayNum * 0.15);
            int bodyNum = (int)(essayNum * 0.7);
            int endNum = (int)(essayNum * 0.15);


            var themeVar = new DValue("x", [theme]);

            string title = TTemplate.GetResult("title",[themeVar]);
            string begin = "";
            string body = "";
            string end = "";




            int tryCount = 2000;

            while (begin.Length < beginNum && tryCount-- > 0)
            {
                begin += TTemplate.GetResult("begin", [themeVar]) ;
            }

            while (body.Length < bodyNum && tryCount-- > 0)
            {
                body += TTemplate.GetResult("body", [themeVar]);
            }

            while (end.Length < endNum && tryCount-- > 0)
            {
                end += TTemplate.GetResult("end", [themeVar]);
            }

            return $"{title}\r\n{begin}\r\n{body}\r\n{end}";
        }
    }


    
}
