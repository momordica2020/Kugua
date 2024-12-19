using System.Text.RegularExpressions;

namespace Kugua
{
    /// <summary>
    /// 基于模板的文本生成
    /// </summary>
    public class TextModules
    {
        private static Dictionary<string, List<string>> data = new Dictionary<string, List<string>>
        {
            { "title", new List<string> { "标题1", "标题2", "标题3" } },
            { "noun", new List<string> { "名词1", "名词2", "名词3" } },
            { "verb", new List<string> { "动词1", "动词2", "动词3" } },
            { "adverb_1", new List<string> { "副词1", "副词2" } },
            { "adverb_2", new List<string> { "副词3", "副词4" } },
            { "phrase", new List<string> { "短语1", "短语2" } },
            { "sentence", new List<string> { "句子1", "句子2" } },
            { "parallel_sentence", new List<string> { "平行句子1", "平行句子2" } },
            { "beginning", new List<string> { "开头1", "开头2" } },
            { "body", new List<string> { "正文1", "正文2" } },
            { "ending", new List<string> { "结尾1", "结尾2" } }
        };

        private static Random random = new Random();

        private static int GetRandomNumber(int total)
        {
            return random.Next(total);
        }

        private static string GetRandom(List<string> list)
        {
            return list[GetRandomNumber(list.Count)];
        }

        public static string GetTitle() => GetRandom(data["title"]);

        public static string GetNoun() => GetRandom(data["noun"]);

        public static string GetVerb() => GetRandom(data["verb"]);

        public static string GetAdverb(int type)
        {
            return type switch
            {
                1 => GetRandom(data["adverb_1"]),
                2 => GetRandom(data["adverb_2"]),
                _ => string.Empty,
            };
        }

        public static string GetPhrase() => GetRandom(data["phrase"]);

        public static string GetSentence() => GetRandom(data["sentence"]);

        public static string GetParallelSentence() => GetRandom(data["parallel_sentence"]);

        public static string GetBeginning() => GetRandom(data["beginning"]);

        public static string GetBody() => GetRandom(data["body"]);

        public static string GetEnding() => GetRandom(data["ending"]);

        private static string ReplaceKey(string str, string key, string replacement)
        {
            return str.Replace(key, replacement);
        }

        private static string ReplaceKey(string str, Regex key, Func<string> replacement)
        {
            return key.Replace(str, _ => replacement());
        }

        public static string ReplaceXX(string str, string theme)
        {
            return ReplaceKey(str, "xx", theme);
        }

        public static string ReplaceVN(string str)
        {
            return ReplaceKey(str, new Regex("vn"), () =>
            {
                var vns = new List<string>();
                int count = GetRandomNumber(4) + 1;
                for (int i = 0; i < count; i++)
                {
                    vns.Add(GetVerb() + GetNoun());
                }
                return string.Join("，", vns);
            });
        }

        public static string ReplaceV(string str)
        {
            return ReplaceKey(str, "v", GetVerb());
        }

        public static string ReplaceN(string str)
        {
            return ReplaceKey(str, "n", GetNoun());
        }

        public static string ReplaceSS(string str)
        {
            return ReplaceKey(str, "ss", GetSentence());
        }

        public static string ReplaceSP(string str)
        {
            return ReplaceKey(str, "sp", GetParallelSentence());
        }

        public static string ReplaceP(string str)
        {
            return ReplaceKey(str, "p", GetPhrase());
        }

        public static string ReplaceAll(string str, string theme)
        {
            str = ReplaceVN(str);
            str = ReplaceV(str);
            str = ReplaceN(str);
            str = ReplaceSS(str);
            str = ReplaceSP(str);
            str = ReplaceP(str);
            str = ReplaceXX(str, theme);
            return str;
        }

        public static string GenerateEssay(string theme = "年轻人买房", int essayNum = 500)
        {
            int beginNum = (int)(essayNum * 0.15);
            int bodyNum = (int)(essayNum * 0.7);
            int endNum = (int)(essayNum * 0.15);

            string title = ReplaceAll(GetTitle(), theme);
            string begin = "";
            string body = "";
            string end = "";

            while (begin.Length < beginNum)
            {
                begin += ReplaceAll(GetBeginning(), theme);
            }

            while (body.Length < bodyNum)
            {
                body += ReplaceAll(GetBody(), theme);
            }

            while (end.Length < endNum)
            {
                end += ReplaceAll(GetEnding(), theme);
            }

            return $"{title}{Environment.NewLine}{begin}{Environment.NewLine}{body}{Environment.NewLine}{end}";
        }
    }
}
