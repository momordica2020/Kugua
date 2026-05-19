using Kugua.Algorithms;
using Kugua.Core;
using NvAPIWrapper.Native.Display;
using System.Numerics;

namespace Kugua.Algorithms.DScript
{

    /// DScript文件格式
    /// [xxx]
    /// 10 a是好的
    /// 15 我喜欢a
    /// 15 还是a，a就好了。
    /// 
    /// [a]
    /// 苦音
    /// 桅子
    /// 枙子
    /// 
    /// #
    /// a是xxx
    /// a是
    /// xxx是a是xxx
    /// #
    /// 哈哈
    /// 笑死
    /// 就这
    /// 
    /// <main>
    /// xxx
    /// 
    /// 用中括号[ ]括住的表示下列各行为同一个命名参数的可选值，
    /// 如果没有中括号括住作为开头，而是用 # 分割，那么#之下读入的各行存入一个匿名参数里
    /// 如果在行前标记数字和空白，这个数字表示该行的预期在随机结果中的频数，用于控制各项的出现频率，如果不标记默认值是1
    /// <main>是主启动位置，脚本如果自动执行，会以该参数开始进行模板解析，参数名为main
    /// 

    /// <summary>
    /// 脚本文件，根据特定DScript语法规则进行读取录入。
    /// 
    /// </summary>
    public class DScriptFile
    {

        /// <summary>
        /// 可选，脚本内定义的命名模板
        /// </summary>
        public Dictionary<string, DValue> NamedTemplates = new Dictionary<string, DValue>();

        /// <summary>
        /// 可选，脚本内未命名的模板
        /// </summary>
        public List<DValue> AnonymousTemplates = new List<DValue>();


        /// <summary>
        /// 随机选择一个匿名模板
        /// </summary>
        public DValue RandomAnonymousTemplate
        {
            get
            {
                if (AnonymousTemplates == null || AnonymousTemplates.Count == 0) return null;
                return AnonymousTemplates[MyRandom.Next(AnonymousTemplates.Count)];

            }
        }

        public const string SimAnonymousSplit = "#";
        public const string SimNamedSplitL = "[";
        public const string SimNamedSplitR = "]";
        public const string SimStart = "main";
        public const string SimNewLine = "#";


        public DScriptFile()
        {

        }


        public bool Load(string ScriptString)
        {
            var success = false;

            try
            {
                if (string.IsNullOrWhiteSpace(ScriptString)) return false;
                DValue targetTemplate = null;
                foreach (var line in ScriptString.Split("\n"))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    else if (line.Trim() == SimAnonymousSplit)
                    {
                        // begin of anonymous template
                        AnonymousTemplates.Add(new DValue());
                        targetTemplate = AnonymousTemplates.Last();
                    }
                    else if (line.Trim().StartsWith(SimNamedSplitL) && line.Trim().EndsWith(SimNamedSplitR))
                    {
                        // begin of named template
                        string name = line.Trim().Substring(SimNamedSplitL.Length, line.Trim().Length - SimNamedSplitL.Length - SimNamedSplitR.Length);
                        NamedTemplates[name] = new DValue(name);
                        targetTemplate = NamedTemplates[name];
                    }
                    else
                    {
                        if (targetTemplate == null)
                        {
                            // 要是开头没有内容，默认作为首个匿名模板
                            AnonymousTemplates.Add(new DValue());
                            targetTemplate = AnonymousTemplates.Last();
                        }
                        // 模板行的开头用数字和\t表示该行模板的频数，没有的话默认为10
                        long freq = DValue.DefaultFrequent;
                        string template = line;
                        if (template.Contains('\t'))
                        {
                            if (!long.TryParse(template.Split('\t').First(), out freq))
                            {
                                freq = DValue.DefaultFrequent;
                            }
                            else
                            {
                                template = template.Substring(template.IndexOf('\t'));
                            }
                        }
                        targetTemplate.Add(template.Trim().Replace(SimNewLine,"\r\n"),freq);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                success = false;
            }

            return success;

        }


        /// <summary>
        /// 根据模板生成填充结果。
        /// 如果不输入key，默认尝试使用命名启动变量[main]或随机的匿名变量作为初始模板
        /// </summary>
        /// <param name="entryKey"></param>
        /// <param name="ExtraParams"></param>
        /// <returns></returns>
        public string GetResult(string entryKey = null, List<DValue> ExtraParams = null)
        {
            List<DValue> Params = new List<DValue>();
            if (ExtraParams != null) Params.AddRange(ExtraParams);
            if (NamedTemplates.Count > 0) Params.AddRange(NamedTemplates.Values);

            if(entryKey != null)
            {
                return NamedTemplates[entryKey].Result(Params);
            }
            if (entryKey == null && NamedTemplates.ContainsKey(SimStart))
            {
                return NamedTemplates[SimStart].Result(Params);
            }
            if(entryKey == null && AnonymousTemplates.Count > 0) 
            {
                return RandomAnonymousTemplate.Result(Params);
            }
            return "";
        }
    }
}



