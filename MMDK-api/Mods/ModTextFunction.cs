using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.FileSystemGlobbing;
using MMDK.Util;
using MMDK.Mods;
using WebSocket4Net.Command;

namespace MMDK.Mods
{
    /// <summary>
    /// 文本应答的一些功能
    /// </summary>
    public class ModTextFunction : Mod
    {
        Dictionary<Regex, HandleCommandEvent> cmds = new Dictionary<Regex, HandleCommandEvent>();


        #region 语料
        //string duiP2f = "pairc2.txt";
        //string duiP1f = "pairc.txt";
        //Dictionary<string, string[]> cf = new Dictionary<string, string[]>();
        //Dictionary<string, string[]> cf2 = new Dictionary<string, string[]>();

        string randomch = "随机-随机汉字.txt";
        string randomChar = "";

        string gongshouName = "gongshou.txt";
        List<string> gongshou = new List<string>();

        string qianzeName = "gengshuang.txt";
        List<string> qianze1 = new List<string>();
        List<string> qianze2 = new List<string>();

        string jokeName = "jokes.txt";
        List<string> jokes = new List<string>();
        List<string> jokesEvent = new List<string>();
        List<string> jokesOrg = new List<string>();
        List<string> jokesEnemy = new List<string>();

        string junkf = "spam.txt";
        List<List<string>> junks = new List<List<string>>();

        string symbolf = "symboltemplate.txt";
        Dictionary<string, List<string>> symbollist = new Dictionary<string, List<string>>();
        #endregion



        public bool Init(string[] args)
        {
            cmds.Add(new Regex(@"^反转(\S+)", RegexOptions.Singleline), handleReverse);
            cmds.Add(new Regex(@"^乱序(\S+)", RegexOptions.Singleline), handleShuffle);
            cmds.Add(new Regex(@"^(.+)攻(.+)受"), handleGongshou);
            cmds.Add(new Regex(@"^随机(\d+)(?:\*(\d+))?"), handleRandomString);
            cmds.Add(new Regex(@"^(\d+)切(?:(\d+)次)(.+)"), handleCutString);
            cmds.Add(new Regex(@"^讽刺(.+)"), handleJoke);




            string PluginPath = Config.Instance.ResourceFullPath("ModePath");
            randomChar = FileManager.Read($"{PluginPath}/{randomch}").Trim();

            // gongshou
            gongshou = new List<string>();
            var res = FileManager.ReadLines($"{PluginPath}/{gongshouName}");
            string thistmp = "";
            foreach (var line in res)
            {
                if (line.Trim() == "$$$$$$$$" && !string.IsNullOrWhiteSpace(thistmp))
                {
                    gongshou.Add(thistmp);
                    thistmp = "";
                }
                else
                {
                    thistmp += line + "\r\n";
                }
            }
            if (!string.IsNullOrWhiteSpace(thistmp)) gongshou.Add(thistmp);

            // qianze
            qianze1 = new List<string>();
            qianze2 = new List<string>();
            int pos = 0;
            res = FileManager.ReadLines($"{PluginPath}/{qianzeName}");
            foreach (var line in res)
            {
                if (line.Trim().StartsWith("#1"))
                {
                    pos = 1;
                    continue;
                }
                else if (line.Trim().StartsWith("#2"))
                {
                    pos = 2;
                    continue;
                }

                if (pos == 1) qianze1.Add(line.Trim());
                else if (pos == 2) qianze2.Add(line.Trim());
            }

            // joke
            jokes = new List<string>();
            jokesOrg = new List<string>();
            jokesEvent = new List<string>();
            jokesEnemy = new List<string>();
            res = FileManager.ReadLines($"{PluginPath}/{jokeName}");
            string tmpline = "";
            foreach (var line in res)
            {
                if (line.Trim().StartsWith("#"))
                {
                    if (!string.IsNullOrEmpty(tmpline))
                    {
                        bool find = false;
                        if (tmpline.Contains("【部门】")) { jokesOrg.Add(tmpline); find = true; }
                        if (tmpline.Contains("【事件】")) { jokesEvent.Add(tmpline); find = true; }
                        if (tmpline.Contains("【敌国】")) { jokesEnemy.Add(tmpline); find = true; }
                        if (!find) jokes.Add(tmpline);
                    }
                    tmpline = "";
                    continue;
                }
                else
                {
                    tmpline += $"{line.Trim()}\r\n";
                }
            }


            string[] lines;
            //// duilian
            //var lines = FileManager.readLines($"{PluginPath}/{duiP1f}");
            //foreach (var line in lines)
            //{
            //    var items = line.Split('\t');
            //    var items2 = items[1].Split(',');
            //    cf[items[0]] = items2;
            //}
            //lines = FileManager.readLines($"{PluginPath}/{duiP2f}");
            //foreach (var line in lines)
            //{
            //    var items = line.Split('\t');
            //    var items2 = items[1].Split(',');
            //    cf2[items[0]] = items2;
            //}


            // junk
            if (File.Exists($"{PluginPath}/{junkf}"))
            {
                lines = File.ReadAllLines($"{PluginPath}/{junkf}", Encoding.UTF8);

                List<string> nowline = new List<string>();
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (nowline.Count > 0)
                        {
                            junks.Add(nowline);
                            nowline = new List<string>();
                        }
                    }
                    else
                    {
                        nowline.Add(line.Trim());
                    }
                }
                if (nowline.Count > 0)
                {
                    junks.Add(nowline);
                }
            }


            // symbols
            lines = FileManager.ReadLines($"{PluginPath}/{symbolf}");
            symbollist = new Dictionary<string, List<string>>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("/"))
                {
                    var items = line.Trim().Split('\t');
                    if (items.Length >= 2)
                    {
                        if (!symbollist.ContainsKey(items[0])) symbollist[items[0]] = new List<string>();
                        symbollist[items[0]].Add(items[1]);
                    }
                }
            }

            return true;
        }
        public void Exit()
        {

        }



        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            message = message.Trim();



            foreach (var cmd in cmds)
            {
                var m = cmd.Key.Match(message);
                if (m.Success)
                {
                    string res = cmd.Value(m, userId, groupId);
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        results.Add(res);
                        return true;
                    }
                }
            }

            return false;

        }



        private string handleJoke(Match matchResults, long userId, long groupId)
        {
            string Jokeres = "";
            try
            {
                // 笑话输入格式：“事件：A，好人：B，坏人：C，地点：D”
                var items = matchResults.Groups[1].Value.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (items.Length >= 1)
                {
                    Dictionary<string, string> pairs = new Dictionary<string, string>();
                    foreach (var item in items)
                    {
                        var pair = item.Split(new char[] { ':', '：', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (pair.Length == 2) pairs[pair[0].Trim()] = pair[1].Trim();
                    }
                    if (pairs.Count > 0)
                    {
                        try
                        {
                            List<string> usingjokes = new List<string>();
                            if (pairs.ContainsKey("敌国")) usingjokes.AddRange(jokesEnemy);
                            if (pairs.ContainsKey("部门")) usingjokes.AddRange(jokesOrg);
                            if (pairs.ContainsKey("事件")) usingjokes.AddRange(jokesEvent);
                            if (usingjokes.Count <= 0) usingjokes.AddRange(jokes);
                            int find = 100;
                            int index = MyRandom.Next(usingjokes.Count);
                            do
                            {
                                Jokeres = usingjokes[index];
                                foreach (var pair in pairs)
                                {
                                    Jokeres = Jokeres.Replace($"【{pair.Key}】", pair.Value);
                                }
                                if (Jokeres.Contains("【"))
                                {
                                    index = (index + 1) % usingjokes.Count;
                                    find -= 1;
                                }
                                else
                                {
                                    break;
                                }
                            } while (find >= 0);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Log(ex.Message + "\r\n" + ex.StackTrace);
                        }

                    }
                }
            }
            catch { }

            return Jokeres;
        }

        private string handleCutString(Match matchResults, long userId, long groupId)
        {
            string sTarget = matchResults.Groups[3].Value;
            string sTime = matchResults.Groups[2].Value;
            string sNum = matchResults.Groups[1].Value;

            string res = sTarget.Trim();
            // K切多次
            int runTime = 0;
            int cutNum = 0;
            if (!int.TryParse(sNum, out cutNum)) return "";
            if (cutNum < 1) return "";
            if (!int.TryParse(sTime, out runTime)) return "";
            if (runTime < 1) runTime = 1;

            runTime = Math.Min(runTime, 5);
            for (int i = 0; i < runTime; i++)
            {
                res =  StaticUtil.ShuffleString(res, cutNum);
            }
            return res;
        }

        private string handleRandomString(Match matchResults, long userId, long groupId)
        {
            int rows = int.Parse(matchResults.Groups[1].Value);
            int columns = matchResults.Groups[2].Success ? int.Parse(matchResults.Groups[2].Value) : 1; // 如果没有列数，默认为1
            if (rows < 1 || columns < 1 || rows > 100 || columns > 100 || rows * columns > 2000)
            {
                return $"输入太多，溢出来了！";
            }
            return GenerateRandomStringHans(rows, columns);
        }

        private string handleGongshou(Match matchResults, long userId, long groupId)
        {
            string gong = matchResults.Groups[1].Value.Trim();
            string shou = matchResults.Groups[2].Value.Trim();
            string result = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(gong) && !string.IsNullOrWhiteSpace(shou) && gongshou.Count > 0)
                {
                    result = gongshou[MyRandom.Next(gongshou.Count)];
                    result = result.Replace("<攻>", gong).Replace("<受>", shou);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex.Message + "\r\n" + ex.StackTrace);
            }

            return result;
        }

        private string handleShuffle(Match matchResults, long userId, long groupId)
        {
            return StaticUtil.ShuffleString(matchResults.Groups[1].Value);
        }

        private string handleReverse(Match matchResults, long userId, long groupId)
        {
            char[] charArray = matchResults.Groups[1].Value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private string handleSalad(Match matchResults, long userId, long groupId)
        {
            string WordSaladres = "";
            try
            {
                
                string WordSaladresKey = matchResults.Groups[1].Value;
                if (string.IsNullOrWhiteSpace(WordSaladresKey)) return "";


                WordSaladresKey = WordSaladresKey.Trim();
                foreach (var para in junks)
                {
                    if (para.Count > 0)
                    {
                        WordSaladres += para[MyRandom.Next(para.Count)] + "\r\n";
                    }
                }
                WordSaladres = WordSaladres.Replace("【E】", DateTime.Now.Year.ToString());
                WordSaladres = WordSaladres.Replace("【B】", new string[] { "朋友", "小伙伴", "网友" }[MyRandom.Next(3)]);
                WordSaladres = WordSaladres.Replace("【A】", WordSaladresKey);

                if (!string.IsNullOrWhiteSpace(WordSaladres))
                {
                    WordSaladres = WordSaladres.Replace("【D】", Config.Instance.App.Avatar.myQQ.ToString()).Replace("【C】", Config.Instance.App.Avatar.askName);

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex.Message + "\r\n" + ex.StackTrace);
            }
            
            return WordSaladres;
        }






        //public string getDui(string sin)
        //{

        //    sin = sin.Trim();
        //    string sout = "";

        //    for (int i = 0; i < sin.Length; i++)
        //    {
        //        if (i + 1 < sin.Length && cf2.ContainsKey(sin.Substring(i, 2)))
        //        {
        //            sout += cf2[sin.Substring(i, 2)][rand.Next(cf2[sin.Substring(i, 2)].Length)];
        //            i += 1;
        //        }
        //        else if (cf.ContainsKey(sin[i].ToString()))
        //        {
        //            sout += cf[sin[i].ToString()][rand.Next(cf[sin[i].ToString()].Length)];
        //        }
        //        //else if("３")
        //        else if ("123456789".Contains(sin[i]))
        //        {
        //            sout = $"{sout}{10 - int.Parse(sin[i].ToString())}";
        //        }
        //        else if ("abcdefghijklmnopqrstuvwxyz".Contains(sin[i]))
        //        {
        //            sout += "abcdefghijklmnopqrstuvwxyz"[rand.Next(26)];
        //        }
        //        else if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(sin[i]))
        //        {
        //            sout += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rand.Next(26)];
        //        }
        //        else if ("あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ".Contains(sin[i]))
        //        {
        //            sout += "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ"[rand.Next(71)];
        //        }
        //        else if ("アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ".Contains(sin[i]))
        //        {
        //            sout += "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ"[rand.Next(71)];
        //        }
        //        else
        //        {
        //            sout += sin[i];
        //        }
        //    }
        //    return sout;




        //}




        


        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="columns">列数</param>
        /// <returns>生成的字符串</returns>
        string GenerateRandomStringHans(int rows, int columns)
        {
            if (string.IsNullOrWhiteSpace(randomChar)) return "";
            StringBuilder sb = new StringBuilder();

            // 生成随机字符
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    char rc = randomChar[MyRandom.Next(randomChar.Length)]; 
                    sb.Append(rc);
                }
                sb.AppendLine(); 
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="columns">列数</param>
        /// <returns>生成的字符串</returns>
        string GenerateRandomString(int rows, int columns)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            // 生成随机字符
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    // 生成随机字符（可以根据需求修改字符范围）
                    char randomChar = (char)random.Next('A', 'Z' + 1); // 生成大写字母
                    sb.Append(randomChar);
                }
                sb.AppendLine(); // 每行结束后换行
            }

            return sb.ToString();
        }









        public string getSymbolDeal(string str)
        {
            string res = "";

            try
            {
                foreach (var sb in symbollist)
                {
                    if (str.StartsWith(sb.Key))
                    {
                        str = str.Substring(sb.Key.Length);
                        if (string.IsNullOrWhiteSpace(str)) return "";

                        var temp = sb.Value[MyRandom.Next(sb.Value.Count)];
                        if (temp.StartsWith("【W】"))     // num and english char
                        {
                            // total 10 + 26 + 26 = 62
                            temp = temp.Substring(3);
                            int singnum = temp.Length / 62;
                            foreach (var ch in str)
                            {
                                try
                                {
                                    int index = -1;
                                    if (ch >= '0' && ch <= '9') index = ch - '0';// res += temp[(int)(ch - '0')];
                                    else if (ch >= 'a' && ch <= 'z') index = 10 + ch - 'a';
                                    else if (ch >= 'A' && ch <= 'Z') index = 36 + ch - 'A';
                                    if (index < 0 || singnum <= 0)
                                    {
                                        res += ch;
                                    }
                                    else
                                    {
                                        res += temp.Substring(index * singnum, singnum);
                                    }
                                }
                                catch
                                {
                                    res += ch;
                                }
                            }
                        }
                        else if (temp.StartsWith("【N】"))        // just num
                        {
                            temp = temp.Substring(3);
                            int maxnum = temp.Length - 1;
                            int trywholenum = -1;
                            int.TryParse(str, out trywholenum);
                            if (trywholenum >= 0 && trywholenum <= maxnum)
                            {
                                // whole num single sym
                                res = temp[trywholenum].ToString();
                            }
                            else
                            {
                                // each num single char
                                foreach (var ch in str)
                                {
                                    try
                                    {
                                        int index = -1;
                                        if (ch >= '0' && ch <= '9') index = ch - '0';
                                        if (index < 0)
                                        {
                                            res += ch;
                                        }
                                        else
                                        {
                                            res += temp.Substring(index, 1);
                                        }
                                    }
                                    catch
                                    {
                                        res += ch;
                                    }
                                }
                            }
                        }
                        else if (temp.StartsWith("【E】"))        // english char
                        {
                            // total 26 + 26 = 52
                            temp = temp.Substring(3);
                            int singnum = temp.Length / 52;
                            if (sb.Key.Contains("空心字母")) singnum = 4;
                            foreach (var ch in str)
                            {
                                try
                                {
                                    int index = -1;
                                    if (ch >= 'a' && ch <= 'z') index = ch - 'a';
                                    else if (ch >= 'A' && ch <= 'Z') index = 26 + ch - 'A';
                                    if (index < 0 || singnum <= 0)
                                    {
                                        res += ch;
                                    }
                                    else
                                    {

                                        res += temp.Substring(index * singnum, singnum);
                                    }
                                }
                                catch
                                {
                                    res += ch;
                                }
                            }
                        }
                        else if (temp.Contains("阿"))          // single word repeat
                        {
                            foreach (var ch in str)
                            {
                                try
                                {
                                    res += temp.Replace('阿', ch);
                                }
                                catch
                                {
                                    res += ch;
                                }
                            }
                        }
                        else if (temp.Contains("【1】"))
                        {
                            if (temp.Contains("【2】"))
                            {
                                // double content
                                res = temp.Replace("【1】", str.Substring(0, str.Length / 2)).Replace("【2】", str.Substring(str.Length / 2));

                            }
                            else
                            {
                                // single content
                                res = temp.Replace("【1】", str);
                            }
                        }


                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex.Message + "\r\n" + ex.StackTrace);
            }




            return res;
        }







    }
}
