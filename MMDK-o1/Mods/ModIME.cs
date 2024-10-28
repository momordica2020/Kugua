using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MMDK.Util;
using Newtonsoft.Json;

namespace MMDK.Mods
{
    class ModIME : Mod
    {

        private TrieNode root;
        private Dictionary<char, char> firstWord = new Dictionary<char, char>();

        


        public bool Init(string[] args)
        {
            try
            {
                root = new TrieNode();

                var data = FileManager.ReadResourceLines("IME");
                //firstWord = new Dictionary<char, char>();
                //for(int i = 0; i < data.Length; i++)
                //{
                //    var line = data[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                //    if (line.Length >= 2)
                //    {
                //        firstWord[line[0][0]] = line[1][0];
                //    }
                //}


                //var json = FileManager.Read($"{Config.Instance.App.ResourcePath}/{Config.Instance.App.Resources["IMETree"]}");
                //root = JsonConvert.DeserializeObject<TrieNode>(json);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            return true;
        }




        public void Exit()
        {
            
        }




        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            if (message.StartsWith("打字"))
            {
                message = message.Substring(2).Trim();
                if (string.IsNullOrWhiteSpace(message))
                {
                    return false;
                }



                return true;

            }



            return false;
        }



        /// <summary>
        /// 根据字符串生成首字母读法，例如  卧槽esu啊1  =  wcesway
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        string GetSentencePinyinFirstword(string sentence)
        {
            List<char> res = new List<char>();
            foreach(var c in sentence)
            {
                char val;
                if(firstWord.TryGetValue(c, out val))
                {
                    res.Add(val);
                }
            }
            return new string(res.ToArray());
        }


        // 插入拼音和汉字的对应关系
        void Insert(string pinyin, string character, int frequency)
        {
            TrieNode current = root;
            foreach (char ch in pinyin)
            {
                if (!current.Children.ContainsKey(ch))
                {
                    current.Children[ch] = new TrieNode();
                }
                current = current.Children[ch];
            }

            // 更新或添加汉字及其频率
            if (current.Words.ContainsKey(character))
            {
                current.Words[character] += frequency; // 增加频率
            }
            else
            {
                current.Words[character] = frequency; // 添加新汉字
            }
        }

        // 根据拼音前缀查找匹配的汉字
        public List<KeyValuePair<string, int>> Search(string prefix)
        {
            TrieNode current = root;
            foreach (char ch in prefix)
            {
                if (!current.Children.ContainsKey(ch))
                {
                    return new List<KeyValuePair<string, int>>(); // 如果没有匹配的前缀，返回空列表
                }
                current = current.Children[ch];
            }

            // 返回所有以该前缀为起始的汉字及其频率
            return current.Words.ToList();
        }

        // 生成最大概率的句子
        public string GenerateMaxProbabilitySentence(string input)
        {
            List<string> results = new List<string>();
            GenerateSentenceHelper(root, input, 0, "", results);

            // 返回概率最大的句子
            return results.OrderByDescending(s => s.Length).FirstOrDefault();
        }

        private void GenerateSentenceHelper(TrieNode current, string input, int index, string currentSentence, List<string> results)
        {
            if (index == input.Length)
            {
                results.Add(currentSentence); // 结束时保存当前句子
                return;
            }

            char initial = input[index];

            // 查找当前首字母对应的汉字
            if (current.Children.TryGetValue(initial, out var node))
            {
                foreach (var word in node.Words)
                {
                    // 递归生成句子
                    GenerateSentenceHelper(node, input, index + 1, currentSentence + word.Key, results);
                }
            }
        }




        public void SaveToFile(string filePath)
        {
            var json = JsonConvert.SerializeObject(root, Formatting.Indented);
            FileManager.writeText(filePath, json);
        }


        class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; set; } = new Dictionary<char, TrieNode>();
            public Dictionary<string, int> Words { get; set; } = new Dictionary<string, int>(); // 存储汉字及其频率
        }
    }
}
