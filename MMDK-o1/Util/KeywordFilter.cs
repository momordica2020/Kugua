using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace MMDK.Util
{
    public class KeywordFilter
    {
        private static readonly Lazy<KeywordFilter> instance = new Lazy<KeywordFilter>(() => new KeywordFilter());
        private static readonly object lockObject = new object(); // 用于线程安全
        private bool isLoaded;

        public List<string[]> keywords;

        private KeywordFilter()
        {
            isLoaded = false;
        }

        public static KeywordFilter Instance => instance.Value;


        public bool Init()
        {
            if (isLoaded) return true;
            try
            {
                /// 敏感词过滤
                keywords = new List<string[]>();
                var lines = FileUtil.readLines(Config.Instance.ResourceFullPath("Sensitive"));
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    if (items.Length >= 2)
                    {
                        keywords.Add([items[0], items[1]]);
                    }
                    else if (items.Length >= 1)
                    {
                        keywords.Add([items[0], "" ]);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return false;
            }
            isLoaded = true;
            return true;
        }
        public string Filtering(string input)
        {
            if(!isLoaded)
            {
                return input;
            }
            try
            {
                foreach (var w in keywords)
                {
                    input = input.Replace(w[0], w[1]);
                }
            }
            catch { }
            return input;
        }

    }
}
