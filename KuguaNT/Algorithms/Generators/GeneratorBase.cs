using Kugua.Algorithms.DScript;
using Kugua.Core;

namespace Kugua.Algorithms.Generators
{
    public abstract class GeneratorBase
    {

        public abstract void Init(string data);
        
        public abstract void Generate();
    }


    public class GeneratorWithDScript : GeneratorBase
    {
        protected DScriptFile Template = new DScriptFile();
        public override void Init(string data)
        {
            Template.Load(data);
        }
        public override void Generate()
        {
            // Implementation of generation logic
        }
    }






    public class GeneratorBase1
    {
        public string Path;
        static DScriptFile DreamTemplate = new DScriptFile();

        public static void Init(string data)
        {
            DreamTemplate.Load(data);
        }



        public static string Get(string verb)
        {
            try
            {
                List<DValue> param = [
                    new DValue("【key】", [$"{verb}"]),
                   // new DValue("$FFF$", [$""]),
                ];
                string res = DreamTemplate.GetResult(null, param);
                if (res.Contains("【"))
                {
                    res = "我不知道";
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return "";
        }
    }
}
