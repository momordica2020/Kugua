﻿using System.Text;

namespace Kugua.Core
{
    public enum FilterType
    {
        None,   // 无过滤
        Normal, // 一般模式杜绝词语出现
        Strict, // 严格模式杜绝单字出现
    }
    /// <summary>
    /// bot发言的总过滤器
    /// </summary>
    public class Filter
    {

        // Trie节点定义
        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
            public string Replacement { get; set; }
            public bool IsEndOfWord { get; set; }
        }





        private static readonly Lazy<Filter> instance = new Lazy<Filter>(() => new Filter());
        //private static readonly object lockObject = new object(); // 用于线程安全
        private bool isLoaded;

        private readonly TrieNode rootNormal;
        private readonly TrieNode rootStrict;

        public static Filter Instance => instance.Value;
        private Filter()
        {
            rootNormal = new TrieNode();
            rootStrict = new TrieNode();
            isLoaded = false;
        }

        public bool Init()
        {
            if (isLoaded) return true;
            try
            {
                //string filterFile1 = Config.Instance.ResourceFullPath("FilterNormal");
                //string filterFile2 = Config.Instance.ResourceFullPath("FilterStrict");
                var fileLines = LocalStorage.ReadResourceLines("FilterNormal", true);
                foreach (var line in fileLines)
                {
                    string[] parts = line.Split("=>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    string keyword = parts[0];
                    string replacement = parts.Length > 1 ? parts[1] : null;
                    AddRuleNormal(keyword, replacement);
                }



                fileLines = LocalStorage.ReadResourceLines("FilterStrict", true);
                foreach (var line in fileLines)
                {
                    string[] parts = line.Split("=>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    string keyword = parts[0];
                    string replacement = parts.Length > 1 ? parts[1] : null;
                    AddRuleStrict(keyword, replacement);
                }
                isLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return isLoaded;
        }


        /// <summary>
        /// 添加规则到Trie
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="replacement"></param>
        private void AddRuleNormal(string keyword, string replacement)
        {
            TrieNode node = rootNormal;
            foreach (char c in keyword)
            {
                if (!node.Children.ContainsKey(c))
                {
                    node.Children[c] = new TrieNode();
                }
                node = node.Children[c];
            }
            node.IsEndOfWord = true;
            node.Replacement = replacement;
        }

        /// <summary>
        /// 添加规则到Trie
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="replacement"></param>
        private void AddRuleStrict(string keyword, string replacement)
        {
            TrieNode node = rootStrict;
            foreach (char c in keyword)
            {
                if (!node.Children.ContainsKey(c))
                {
                    node.Children[c] = new TrieNode();
                }
                node = node.Children[c];
            }
            node.IsEndOfWord = true;
            node.Replacement = replacement;
        }

        /// <summary>
        /// 字符串是否通过过滤？
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsPass(string input, FilterType type)
        {
            if (!isLoaded || type == FilterType.None || string.IsNullOrWhiteSpace(input)) return true;
            bool result = true;

            switch (type)
            {
                case FilterType.None:
                    result = true;
                    break;
                case FilterType.Normal:
                case FilterType.Strict:
                    string res = Filting(input, type);
                    if (res != input)
                    {
                        // filted!
                        Logger.Log($"以拦截！{input} => {res}");
                    }
                    result = input == res;
                    break;
                default:
                    break;
            }

            return result;
        }

        public string BannedTip(int len = -1)
        {
            if (len <= 0 || len > 10) len = MyRandom.Next(1, 10);
            return new string('█', len);
        }

        public string FiltingBySentense(string input, FilterType type)
        {
            //string BannedTipP= "【数据删除】";
            if (!isLoaded || type == FilterType.None || string.IsNullOrWhiteSpace(input)) return input;

            StringBuilder sb = new StringBuilder();
            int beginIndex = 0;
            for (int i = 1; i < input.Length; i++)
            {
                if (new char[] { '\n', '，', '。', '？', '：', '！', '…' }.Contains(input[i]))
                {
                    // cut forward
                    if (i > beginIndex)
                    {
                        var thisSentense = input.Substring(beginIndex, i - beginIndex);
                        if (IsPass(thisSentense, type))
                        {
                            sb.Append(thisSentense);
                            sb.Append(input[i]);
                        }
                        else
                        {
                            sb.Append(BannedTip(thisSentense.Length / 2));
                        }
                    }

                    beginIndex = i + 1;

                }
            }
            if (beginIndex < input.Length)
            {
                var thisSentense = input.Substring(beginIndex);
                if (IsPass(thisSentense, type))
                {
                    sb.Append(thisSentense);
                }
                else
                {
                    sb.Append(BannedTip(thisSentense.Length / 2));
                }
            }

            string res = sb.ToString();
            //res.Replace(BannedTipP + BannedTipP, BannedTip);
            return sb.ToString();
        }

        /// <summary>
        /// 过滤文本，返回过滤后的文本内容
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Filting(string input, FilterType type)
        {
            if (!isLoaded || type == FilterType.None || string.IsNullOrWhiteSpace(input)) return input;

            List<char> output = new List<char>();
            int index = 0;

            while (index < input.Length)
            {

                TrieNode node = null;
                if (FilterType.Normal == type)
                {
                    node = rootNormal;
                }
                else if (FilterType.Strict == type)
                {
                    node = rootStrict;
                }
                else
                {// error
                    return input;
                }
                int matchLength = 0;
                string replacement = null;

                for (int i = index; i < input.Length; i++)
                {
                    if (!node.Children.ContainsKey(input[i]))
                    {
                        break;
                    }
                    node = node.Children[input[i]];

                    if (node.IsEndOfWord)
                    {
                        matchLength = i - index + 1;
                        replacement = node.Replacement;
                    }
                }

                if (matchLength > 0)
                {
                    // 如果有替换词，则替换；否则直接跳过匹配部分（相当于删除）
                    if (replacement != null)
                    {
                        output.AddRange(replacement);
                    }
                    index += matchLength; // 跳过匹配的部分
                }
                else
                {
                    output.Add(input[index]);
                    index++;
                }
            }
            string res = new string(output.ToArray());
            //if(res != input)
            //{
            //    Logger.Log($"以过滤！{input} => {res}", LogType.Debug);
            //}
            return res;
        }

    }
}
