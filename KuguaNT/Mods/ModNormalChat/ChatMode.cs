using System.Text;
using Kugua.Core;


namespace Kugua.Mods.ModNormalChat
{
    public partial class ModNormalChat
    {
        /// <summary>
        /// 闲聊模式的回复基类
        /// </summary>
        class ChatMode
        {
            public string name;
            public List<string> config;
            List<string> sentences;
            // public int 
            public ChatMode()
            {
                config = new List<string>();
                sentences = new List<string>();
            }

            public ChatMode(string _name, ICollection<string> _config, ICollection<string> _sentences)
            {
                name = _name;
                config = _config.ToList();
                sentences = _sentences.ToList();
            }

            public string getRandomSentence(string seed = "")
            {

                int maxsnum = 5;
                int maxslen = 7;
                int maxwordnum = 4;

                if (config.Contains("单句"))
                {
                    maxslen = 1;
                    maxwordnum = 1;
                    maxsnum = 1;
                }
                if (config.Contains("句内不拼接"))
                {
                    maxwordnum = 1;
                    maxslen = 1;
                }


                string result = "";
                //byte[] md5data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                string[] sgn1 = new string[] { ",", "，", "；", "、" };
                string[] sgn2 = new string[] { "\r\n", "。", "。", "。", "？", "！", "…", "——" };
                string[] sgn3 = new string[] { "\r\n", "。", "？", "！", "…", "——", "??", "...", "：", "?!", "???", "!!", "！！！" };

                int sn = MyRandom.Next(1, maxsnum);

                for (int i = 0; i < sn; i++)
                {
                    int thislen = MyRandom.Next(1, maxslen);
                    StringBuilder thissentence = new StringBuilder();
                    int wordnum = 0;
                    while (thissentence.Length < thislen && wordnum < maxwordnum)
                    {
                        wordnum++;
                        thissentence.Append(sentences[MyRandom.Next(0, sentences.Count - 1)]);
                    }
                    if (thissentence.Length > 0 && !sgn1.Contains(thissentence.ToString().Last().ToString()) && !sgn2.Contains(thissentence.ToString().Last().ToString()))
                    {
                        if (config.Contains("无标点")) thissentence.Append(" ");
                        else thissentence.Append(sgn1[MyRandom.Next(sgn1.Length)]);
                        result += thissentence.ToString();
                        if (result.Length > 0)
                        {
                            if (config.Contains("无标点")) ;
                            else if (config.Contains("乱打标点")) result = result.Substring(0, result.Length - 1) + sgn3[MyRandom.Next(sgn3.Length)];
                            else result = result.Substring(0, result.Length - 1) + sgn2[MyRandom.Next(sgn2.Length)];
                        }

                    }
                    else
                    {
                        result += thissentence.ToString();
                    }
                }
                if (string.IsNullOrWhiteSpace(result))
                {
                    if (config.Contains("无标点")) result = " ";
                    else if (config.Contains("乱打标点")) result = result.Substring(0, result.Length - 1) + sgn3[MyRandom.Next(sgn3.Length)];
                    else result = result.Substring(0, result.Length - 1) + sgn2[MyRandom.Next(sgn2.Length)];
                }


                return result;
            }
        }



    }
}
