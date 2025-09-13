using Kugua.Core;
using System.Text;

namespace Kugua.Generators
{
    /// <summary>
    /// 苏联笑话的模板填充相关
    /// </summary>
    public class Joke
    {     
        public string raw;
        List<JokeTemplate> templates;

        public static Dictionary<string, List<Joke>> Jokes;
        //public static List<string> Keys;
        static Trie Keys = new Trie();
        static List<string> KeysList = new List<string>();

        /// <summary>
        /// joke库的初始化
        /// </summary>
        /// <param name="data"></param>
        public static void Init(string[] data)
        {
            string tmpline = "";
            bool firstLine = true;
            foreach (var line in data)
            {
                if (firstLine)
                {
                    var keys = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    KeysList = keys.ToList();
                    for (int i = 0; i < keys.Length; i++)
                    {

                        Keys.Insert(keys[i], i);
                    }
                    //Keys = new List<string>(keystr);
                    Jokes = new Dictionary<string, List<Joke>>();
                    firstLine = false;
                    continue;
                }

                if (line.Trim().StartsWith("#"))
                {
                    if (!string.IsNullOrEmpty(tmpline))
                    {
                        bool find = false;
                        AddJoke(tmpline);
                    }
                    tmpline = "";
                    continue;
                }
                else
                {
                    tmpline += $"{line.Trim()}\r\n";
                }
            }


        }


        public Joke(string template)
        {
            templates = new List<JokeTemplate>();
            if (string.IsNullOrWhiteSpace(template)) return;
            raw = template.Trim();


            // parse
            JokeTemplate jt = null;
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == '【')
                {
                    // new condition
                    jt = new JokeTemplate
                    {
                        raw = "",
                        index = i,
                        len = 0,
                        conditions = new List<(string, string)>(),
                    };
                }
                else if (raw[i] == '】')
                {
                    if (jt == null) continue;
                    jt.len = i - jt.index + 1;
                    jt.raw = raw.Substring(jt.index, jt.len);
                    var vs = jt.raw.Substring(1, jt.raw.Length - 2).Trim().Split('=');
                    foreach (var v in vs)
                    {
                        jt.conditions.Add((GetUniKey(v), v));
                    }
                    templates.Add(jt);

                }
            }


        }

        /// <summary>
        ///  回溯算法填满关键词列表，即如果列表[1,2,3,4],输入[1,3]填充为[1,3][1,2,3][1,3,4][1,2,3,4]
        /// </summary>
        /// <param name="keywords"></param>
        static List<List<string>> GenerateFillSequences(List<string> startKeywords)
        {
            // 确定起始关键词序列在常量列表中的索引
            List<int> startIndexes = new List<int>();
            foreach (var key in startKeywords)
            {
                int index = KeysList.IndexOf(key);
                if (index == -1)
                {
                    throw new ArgumentException($"Start keyword '{key}' not found in the constant keyword list.");
                }
                startIndexes.Add(index);
            }
            startIndexes.Sort(); // 按索引排序以确保顺序正确
            List<string> correctedKeywords = new List<string>();
            foreach (var index in startIndexes)
            {
                correctedKeywords.Add(KeysList[index]);
            }

            //// 检查起始关键词序列的顺序是否符合常量列表的顺序
            //for (int i = 1; i < startIndexes.Count; i++)
            //{
            //    if (startIndexes[i] <= startIndexes[i - 1])
            //    {
            //        throw new ArgumentException("Start keywords are not in the correct order in the constant keyword list.");
            //    }
            //}

            // 从最后一个起始关键词的索引开始扩展
            int startIndex = startIndexes[startIndexes.Count - 1];

            // 初始化结果列表
            List<List<string>> result = new List<List<string>>();

            // 初始化当前序列为起始关键词序列
            List<string> current = new List<string>(correctedKeywords);

            // 回溯生成序列
            Backtrack(KeysList, startIndex + 1, current, result);

            return result;
        }

        static void Backtrack(List<string> keywords, int start, List<string> current, List<List<string>> result)
        {
            // 保存当前序列
            result.Add(new List<string>(current));

            for (int i = start; i < keywords.Count; i++)
            {
                // 选择当前关键词
                current.Add(keywords[i]);

                // 递归生成后续序列
                Backtrack(keywords, i + 1, current, result);

                // 回溯，移除当前关键词
                current.RemoveAt(current.Count - 1);
            }
        }

        public static string GetRandomJoke(List<(string key, string val)> conditions)
        {
            StringBuilder result = new StringBuilder();
            string unikey = GetUniKey(string.Join(" ", conditions.Select(x => x.key)));
            if (Jokes.ContainsKey(unikey))
            {
                var jokes = Jokes[unikey].ToList();
                if (jokes.Count > 0)
                {
                    var joke = jokes[MyRandom.Next(jokes.Count)];
                    int indexInRaw = 0;
                    for (int i = 0; i < joke.templates.Count; i++)
                    {
                        var t = joke.templates[i];
                        if (t.index > 0) result.Append(joke.raw.Substring(indexInRaw, t.index - indexInRaw));
                        foreach (var c in t.conditions)
                        {
                            if (string.IsNullOrWhiteSpace(c.Item1))
                            {
                                // not need any key.
                                result.Append(c.Item2);
                                break;
                            }
                            else
                            {
                                bool find = false;
                                foreach (var kv in conditions)
                                {

                                    if (!c.Item1.Contains('#') && c.Item1 == kv.key)
                                    {
                                        // match
                                        result.Append(c.Item2.Replace(kv.key, kv.val));
                                        find = true;
                                        break;
                                    }
                                }
                                if (find) break;
                            }
                        }
                        indexInRaw = t.index + t.len;

                    }
                    if (indexInRaw < joke.raw.Length) result.Append(joke.raw.Substring(indexInRaw));

                }
            }

            return result.ToString();
        }







        /// <summary>
        /// 用于索引模板是否可匹配。返回索引用的字符串
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string GetUniKey(string cmd)
        {
            string keystr = "";

            // 匹配关键词
            List<string> matchedKeywords = Keys.SearchKeywords(cmd);

            return string.Join("#", matchedKeywords);
        }

        public static void AddJoke(string jokeTemplateString)
        {
            if (Jokes == null) Jokes = new Dictionary<string, List<Joke>>();
            if (jokeTemplateString.Length > 0)
            {
                Joke j = new Joke(jokeTemplateString);

                List<string> kwords = new List<string>();

                //List<string> unikeys = new List<string>();
                kwords.Add("");
                foreach (var jt in j.templates)
                {
                    List<string> kwords2 = new List<string>();
                    HashSet<string> ths = new HashSet<string>();
                    foreach (var jtc in jt.conditions)
                    {
                        ths.Add(jtc.Item1.Trim());
                    }
                    foreach (var thsk in ths)
                    {

                        for (int i = 0; i < kwords.Count; i++)
                        {
                            var nk = GetUniKey(kwords[i] + "#" + thsk);
                            if (!kwords2.Contains(nk)) kwords2.Add(nk);
                            if (string.IsNullOrWhiteSpace(thsk) && !kwords2.Contains(kwords[i])) kwords2.Add(kwords[i]);
                        }

                    }
                    kwords = kwords2;
                }
                foreach (var k in kwords)
                {
                    var uk = GetUniKey(k);
                    if (string.IsNullOrWhiteSpace(uk)) continue;
                    var filluks = GenerateFillSequences(uk.Split('#').ToList());
                    foreach (var u in filluks)
                    {
                        string ustr = string.Join('#', u);
                        if (!Jokes.ContainsKey(ustr)) Jokes[ustr] = new List<Joke>();
                        Jokes[ustr].Add(j);
                    }

                }
                if (jokeTemplateString.Contains("即将访问"))
                {

                }
            }


        }

















        class JokeTemplate
        {
            public int index;
            public int len;
            public string raw;
            public List<(string, string)> conditions;
        }

        class Trie
        {
            class TrieNode
            {
                public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
                public int Index { get; set; } = -1; // 记录关键词在输入列表中的索引
            }

            private readonly TrieNode root = new TrieNode();
            List<string> keywords = new List<string>();





            /// <summary>
            /// 插入关键词到 Trie
            /// </summary>
            /// <param name="word"></param>
            /// <param name="index"></param>
            public void Insert(string word, int index)
            {
                keywords.Add(word);
                var node = root;
                foreach (char ch in word)
                {
                    if (!node.Children.ContainsKey(ch))
                    {
                        node.Children[ch] = new TrieNode();
                    }
                    node = node.Children[ch];
                }
                node.Index = index; // 标记关键词在输入中的索引
            }

            /// <summary>
            /// 从字符串中查找所有关键词，保持输入顺序
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public List<string> SearchKeywords(string text)
            {
                var matchedIndexes = new HashSet<int>();
                int i = 0;

                while (i < text.Length)
                {
                    var node = root;
                    int longestMatchIndex = -1;
                    int endIndex = i;

                    // 从当前位置开始尝试匹配
                    for (int j = i; j < text.Length; j++)
                    {
                        if (node.Children.ContainsKey(text[j]))
                        {
                            node = node.Children[text[j]];
                            if (node.Index != -1)
                            {
                                longestMatchIndex = node.Index; // 更新当前最长匹配的索引
                                endIndex = j;                 // 更新结束索引
                            }
                        }
                        else
                        {
                            break; // 无法继续匹配
                        }
                    }

                    if (longestMatchIndex != -1)
                    {
                        matchedIndexes.Add(longestMatchIndex); // 添加匹配到的关键词索引
                        i = endIndex + 1;                     // 跳过已匹配的部分
                    }
                    else
                    {
                        i++; // 未匹配任何关键词，继续下一字符
                    }
                }
                List<string> res = new List<string>();
                var c = matchedIndexes.ToList();
                c.Sort();
                foreach (var item in c)
                {
                    res.Add(keywords[item]);
                }
                return res;
            }
        }






    }
}
