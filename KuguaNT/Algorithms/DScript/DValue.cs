namespace Kugua.Algorithms.DScript
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

        //public int replaceCount = 0;
        public const int MaxReplaceCount = 10000;

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
        public string Result(IEnumerable<DValue> ExistParams = null, int nowCount = 0)
        {
            string result = GetTemplate;
            if (ExistParams == null || ExistParams.Count() <= 0) return result;
            bool final = true;
            // 防止全局循环卡死
            if (nowCount++ > MaxReplaceCount) return result;
            int loopCount = 255; // 防止单次替换循环卡死
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
                        result = result.Substring(0, beginIndex) + param.Result(ExistParams, nowCount) + result.Substring(beginIndex + param.Name.Length);
                        if(resultOri != result) final = false;
                    }
                }
            } while (final == false && loopCount > 0);
            

            return result;
        }



        


    }
}



