using Kugua.Core;
using NvAPIWrapper.Native.Display;

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

        public List<string> Templates;

        public DValue(string name = "")
        {
            Name = name;
            Templates = new List<string>();
        }

        public DValue(IEnumerable<string> templates)
        {
            Name = "";
            Templates=templates.ToList();
        }

        public DValue(string name, IEnumerable<string> templates) : this(name)
        {
            Templates = templates.ToList();
        }


        /// <summary>
        /// 根据模板随即选择，并替换掉占位符，得到结果
        /// </summary>
        /// <param name="ExistParams"></param>
        /// <returns></returns>
        public string Result(IEnumerable<DValue> ExistParams = null)
        {
            
            string result = "";

            if(Templates.Count > 0) 
            {
                result = Templates[MyRandom.Next(Templates.Count)];
            }
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

        public const string SimAnonymousSplit = "#";
        public const string SimNamedSplitL = "[";
        public const string SimNamedSplitR = "]";


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
                        targetTemplate.Templates.Add(line.Trim());
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
    }
}



