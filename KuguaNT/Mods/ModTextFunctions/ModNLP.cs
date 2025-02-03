
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;




namespace Kugua
{
    /// <summary>
    /// 自然语言相关模型
    /// </summary>
    internal class ModNLP : Mod
    {
        ViterbiModel model;
        private List<string> pinyinMapping;


        //private static readonly Lazy<ModNLP> instance = new Lazy<ModNLP>(() => new ModNLP());
        //public static ModNLP Instance => instance.Value;
        //private ModNLP()
        //{


        //}




        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex(@"^谐音(.+)", RegexOptions.Singleline), handleXieyin));




                model = new ViterbiModel(this);
                //string updateFile = "input.txt"; // 这里是输入的文本文件
                string modelData = Config.Instance.FullPath("NLP_MODEL1");
                string pinyinUTF8 = Config.Instance.FullPath("Pinyin");
                pinyinMapping = new List<string>();
                foreach (var line in File.ReadLines(pinyinUTF8))
                {
                    pinyinMapping.Add(line.Trim());
                }

                var load = model.LoadModel(modelData);
                if (!load)
                {
                    //model.TrainModel(updateFile);
                    //model.SaveModel(modelData);

                    //model = new ViterbiModel();
                    //model.LoadModel(modelData);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }


            return true;
        }

        /// <summary>
        /// 首字母谐音
        /// 谐音awsl/谐音哈哈
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleXieyin(MessageContext context, string[] param)
        {
            string target = param[1];
            if (!string.IsNullOrWhiteSpace(target))
            {
                string py = getPinyinFirstList(target);
                if (string.IsNullOrWhiteSpace(py)) return "";
                var bestSequence = model.GetSamePinyinSentnse(py);
                return bestSequence;
            }
            return "";
        }

        private string getPinyinFirstList(string input)
        {
            string output = "";
            if (model != null)
            {


                foreach (var c in input)
                {
                    string pinyinfull = GetPinyinSingle(c);
                    if (!string.IsNullOrWhiteSpace(pinyinfull))
                    {
                        output += pinyinfull[0];
                    }
                    else
                    {
                        output += c;
                    }
                }





            }
            return output;
        }



        // 汉字获取拼音首字母
        public string GetPinyinSingle(char character)
        {
            if (IsHan(character)) // 汉字范围
            {
                int index = character - 0x4E00; // 计算索引
                if (index >= 0 && index < pinyinMapping.Count && !string.IsNullOrWhiteSpace(pinyinMapping[index]))
                {
                    return pinyinMapping[index]; // 返回首字母
                }
            }
            if (character >= 'a' && character <= 'z') return character.ToString().Replace("i", "y").Replace("u", "w").Replace("v", "w");
            if (character >= 'A' && character <= 'Z') return character.ToString().ToLower().Replace("i", "y").Replace("u", "w").Replace("v", "w");
            return "";
        }

        public static bool IsHan(char character)
        {
            return character >= 0x4E00 && character <= 0x9FFF;
        }



    }

}
