using Kugua.Core;
using Kugua.Mods.ModTextFunctions;
using System.Text.RegularExpressions;

namespace Kugua
{
    /// <summary>
    /// 基于模板的文本生成
    /// </summary>
    public class TextModules
    {

        public static Dictionary<string, DValue> ExistParams = new Dictionary<string, DValue>();
        

        public static void Init(string[] slist)
        {
            var key = "";
            foreach(var line in slist)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (line.StartsWith("["))
                {
                    // key
                    key = line.Replace("[","").Replace("]","");
                    ExistParams[key] = new DValue(key);
                }
                else
                {
                    // value line
                    if(!string.IsNullOrWhiteSpace(key)) ExistParams[key].Templates.Add(line);
                }
            }
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
        /// 随机性生成
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="essayNum"></param>
        /// <returns></returns>
        public static string GenerateEssay(string theme, int essayNum = 500)
        {
            int beginNum = (int)(essayNum * 0.15);
            int bodyNum = (int)(essayNum * 0.7);
            int endNum = (int)(essayNum * 0.15);


            var themeVar = new DValue("xx", [theme]);
            var allParams = ExistParams.Values.ToList();
            allParams.Add(themeVar);

            string title = ExistParams["title"].Result(allParams);
            string begin = "";
            string body = "";
            string end = "";

            begin += ExistParams["begin"].Result(allParams);
            body += ExistParams["body"].Result(allParams);
            end += ExistParams["end"].Result(allParams);

            //while (begin.Length < beginNum)
            //{
            //    begin += new DValue("begin").Result(allParams);
            //}

            //while (body.Length < bodyNum)
            //{
            //    body += new DValue("body").Result(allParams);
            //}

            //while (end.Length < endNum)
            //{
            //    end += new DValue("end").Result(allParams);
            //}

            return $"{title}\r\n{begin}\r\n{body}\r\n{end}";
        }
    }


    
}
