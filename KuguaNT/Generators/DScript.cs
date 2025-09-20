using Kugua.Core;
using NvAPIWrapper.Native.Display;
using System.Numerics;

namespace Kugua.Generators
{
    /// <summary>
    /// 用于定型文模板填充用
    /// 每个DValue单元存储一个占位符关键词Name 和其对应的生成模板序列Templates。
    /// 在生成的时候，会随机从所有模板中抽取一个，然后循环利用传入的关键词序列信息给将占位符填满
    /// </summary>
    public class DValue
    {
        public string Name;

        private List<(long, string)> Templates;
        
        private long AllFrequent = 0;
        public const long DefaultFrequent = 10;

        public DValue(string name = "")
        {
            Name = name;
            Templates = new List<(long, string)>();
        }

        public DValue(string name, string singleValue) : this(name)
        {
            Add(singleValue, DefaultFrequent);
        }

        public DValue(List<string> templates) : this() 
        {
            foreach (var template in templates)
            {
                Add(template, DefaultFrequent);
            }
        }

        public DValue(string name, List<string> templates) : this(name)
        {
            foreach (var template in templates)
            {
                Add(template, DefaultFrequent);
            }
        }

        /// <summary>
        /// 添加新模板
        /// </summary>
        /// <param name="template"></param>
        /// <param name="frequent"></param>
        public void Add(string template, long frequent)
        {
            Templates.Add((frequent,template));
            AllFrequent += frequent;
        }

        /// <summary>
        /// 随机返回一个模板。会考虑模板的频数
        /// </summary>
        public string GetTemplate
        {
            get
            {
                if (Templates.Count > 0 && AllFrequent > 0)
                {
                    var target = MyRandom.Next(AllFrequent);
                    foreach (var (freq, template) in Templates)
                    {
                        if (target < freq)
                        {
                            return template;
                        }
                        else
                        {
                            target -= freq;
                        }
                    }
                }
                return "";
            }
           
        }

        //public DValue(IEnumerable<string> templates)
        //{
        //    Name = "";
        //}

        //public DValue(string name, IEnumerable<string> templates) : this(name)
        //{
        //    Templates = templates.ToList();
        //}


        /// <summary>
        /// 根据模板随即选择，并替换掉占位符，得到结果
        /// </summary>
        /// <param name="ExistParams"></param>
        /// <returns></returns>
        public string Result(IEnumerable<DValue> ExistParams = null)
        {
            
            string result = GetTemplate;
            if (ExistParams == null || ExistParams.Count() <= 0) return result;
            bool final = true;
            int loopCount = 255; // 防止循环卡死
            do
            {
                loopCount -= 1;
                final = true;
                foreach (var param in ExistParams)
                {
                    if (!string.IsNullOrWhiteSpace(param.Name) && result.Contains(param.Name))
                    {
                        int beginIndex = result.IndexOf(param.Name);
                        var resultOri = result + "";
                        result = result.Substring(0, beginIndex) + param.Result(ExistParams) + result.Substring(beginIndex + param.Name.Length);
                        if(resultOri != result) final = false;
                    }
                }
            } while (final == false && loopCount > 0);
            

            return result;
        }



        


    }

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
    public class DScript
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


        public DScript()
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

        public string GetDefaultResult(string entryKey = null, List<DValue> ExtraParams = null)
        {
            List<DValue> Params = new List<DValue>();
            if (ExtraParams != null) Params.AddRange(ExtraParams);
            if (NamedTemplates.Count > 0) Params.AddRange(NamedTemplates.Values);

            if (entryKey == null) entryKey = SimStart;

            if (NamedTemplates.ContainsKey(entryKey))
            {
                return NamedTemplates[entryKey].Result(Params);
            }
            else if(entryKey == null && AnonymousTemplates.Count > 0) 
            {
                return RandomAnonymousTemplate.Result(Params);
            }
            return "";
        }
    }
}



