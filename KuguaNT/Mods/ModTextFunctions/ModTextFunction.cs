using Kugua.Core;
using Kugua.Generators;
using Microsoft.JSInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Kugua.Mods
{

    /// <summary>
    /// 文本应答的一些功能
    /// </summary>
    public class ModTextFunction : Mod
    {


        #region 语料
        //string duiP2f = "pairc2.txt";
        //string duiP1f = "pairc.txt";
        //Dictionary<string, string[]> cf = new Dictionary<string, string[]>();
        //Dictionary<string, string[]> cf2 = new Dictionary<string, string[]>();

        string randomch = "随机-随机汉字.txt";
        string randomChar = "";     

        string symbolf = "symboltemplate.txt";
        Dictionary<string, List<string>> symbollist = new Dictionary<string, List<string>>();
        #endregion

        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^反转(.+)", RegexOptions.Singleline), handleReverse));
            ModCommands.Add(new ModCommand(new Regex(@"^逐行反转(.+)", RegexOptions.Singleline), handleReverseByLine));
            ModCommands.Add(new ModCommand(new Regex(@"^大写(.+)", RegexOptions.Singleline), handleToUpper));
            ModCommands.Add(new ModCommand(new Regex(@"^小写(.+)", RegexOptions.Singleline), handleToLower));
            ModCommands.Add(new ModCommand(new Regex(@"^乱序(.+)", RegexOptions.Singleline), handleShuffle));
            ModCommands.Add(new ModCommand(new Regex(@"^(.+)攻(.+)受", RegexOptions.Singleline), handleGongshou));
            ModCommands.Add(new ModCommand(new Regex(@"^随机(\d+)(?:\*(\d+))?", RegexOptions.Singleline), handleRandomString));
            ModCommands.Add(new ModCommand(new Regex(@"^(\d+)切(?:(\d+)次)?(.+)", RegexOptions.Singleline), handleCutString));
            ModCommands.Add(new ModCommand(new Regex(@"^讽刺(.+)", RegexOptions.Singleline), handleJoke));
            ModCommands.Add(new ModCommand(new Regex(@"^历史上的(\S+)", RegexOptions.Singleline), handleHistoryToday));
            ModCommands.Add(new ModCommand(new Regex(@"^什么是[∶|:|：|\s]+(\S+)", RegexOptions.Singleline), handleSalad));
            ModCommands.Add(new ModCommand(new Regex(@"^火星文[∶|:|：|\s]+(.+)", RegexOptions.Singleline), handleHX));
            ModCommands.Add(new ModCommand(new Regex(@"^研究一下[∶|:|：|\s]+(\S+)", RegexOptions.Singleline), handlePaper));
            ModCommands.Add(new ModCommand(new Regex(@"^云杰说道[∶|:|：|\s]+(\S+)", RegexOptions.Singleline), handlePaper2));
            ModCommands.Add(new ModCommand(new Regex(@"^营销号[∶|:|：|\s]+(\S+)", RegexOptions.Singleline), handlePaper3));



            string PluginPath = Config.Instance.FullPath("ModePath");
            randomChar = LocalStorage.Read($"{PluginPath}/{randomch}").Trim();


            //// qianze
            //qianze1 = new List<string>();
            //qianze2 = new List<string>();
            //int pos = 0;
            //res = LocalStorage.ReadLines($"{PluginPath}/{qianzeName}");
            //foreach (var line in res)
            //{
            //    if (line.Trim().StartsWith("#1"))
            //    {
            //        pos = 1;
            //        continue;
            //    }
            //    else if (line.Trim().StartsWith("#2"))
            //    {
            //        pos = 2;
            //        continue;
            //    }

            //    if (pos == 1) qianze1.Add(line.Trim());
            //    else if (pos == 2) qianze2.Add(line.Trim());
            //}


            // gongshou
            Gongshou.Init(LocalStorage.Read($"{PluginPath}/gongshou.txt"));

            // joke
            Joke.Init(LocalStorage.ReadLines($"{PluginPath}/jokes.txt"));

            // spam
            SpamText.Init(LocalStorage.Read($"{PluginPath}/spam.txt"));

            // text module
            TextModules.Init(LocalStorage.Read($"{PluginPath}/data_hyly.txt"));

            // id module
            IDGenerator.Init(LocalStorage.Read($"{PluginPath}/data_id.txt"));

            SpamText2.Init(LocalStorage.Read($"{PluginPath}/data_hyly2.txt"));
            SpamText3.Init(LocalStorage.Read($"{PluginPath}/data_hyly3.txt"));
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


            
            


            //// symbols
            //lines = LocalStorage.ReadLines($"{PluginPath}/{symbolf}");
            //symbollist = new Dictionary<string, List<string>>();
            //foreach (var line in lines)
            //{
            //    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("/"))
            //    {
            //        var items = line.Trim().Split('\t');
            //        if (items.Length >= 2)
            //        {
            //            if (!symbollist.ContainsKey(items[0])) symbollist[items[0]] = new List<string>();
            //            symbollist[items[0]].Add(items[1]);
            //        }
            //    }
            //}



            

            return true;
        }

        private string handlePaper(MessageContext context, string[] param)
        {
            string keyword = param[1];

            var res = TextModules.Get(keyword);

            return res;
        }

        private string handlePaper2(MessageContext context, string[] param)
        {
            string keyword = param[1];

            var res = $"云杰说道：暂且不知诸位朋友哪位，{SpamText2.Get(keyword)}";

            return res;
        }

        private string handlePaper3(MessageContext context, string[] param)
        {
            string keyword = param[1];

            var res = SpamText3.Get(keyword);

            return res;
        }


        /// <summary>
        /// 文本转煋文
        /// 火星文 错的不是我，是世界！
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleHX(MessageContext context, string[] param)
        {
            try
            {
                return Util.HanToHx(param[1]);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            return "";

        }

        public void Exit()
        {

        }


       

        /// <summary>
        /// 历史上的哪天？
        /// 历史上的今天/历史上的昨天/历史上的3月1日
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleHistoryToday(MessageContext context, string[] param)
        {
            string date = param[1].Trim();
            DateTime checkDate = Util.GetDateFromHans(date);
            string res = $"历史上的{checkDate.ToString("MM月dd日")}：\r\n";
            try
            {

                using (FileStream fs = new FileStream(Config.Instance.FullPath("history_in_today.txt"), FileMode.Open))
                {
                    using (StreamReader streamReader = new StreamReader(fs, Encoding.UTF8))
                    {

                        try
                        {
                            string line = "";
                            int num = 0;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                var lineitem = line.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                //Logger.Log(lineitem.Length + "\t" + line);
                                if (lineitem.Length >= 5)
                                {
                                    var y = lineitem[0];
                                    var m = int.Parse(lineitem[1]);
                                    var d = int.Parse(lineitem[2]);
                                    var type = lineitem[3];
                                    var data = lineitem[4];

                                    if (m == checkDate.Month && d == checkDate.Day && type == "1")
                                    {
                                        res += $"{y}年：{data}\r\n";
                                        if (num++ > 10) break;
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            // 使用这里发送，加个过滤
            if (!string.IsNullOrWhiteSpace(res))
            {
                context.SendBackText(res, true, true);
                res = null;
            }

            return res;
        }


        /// <summary>
        /// zz笑话生成器，输入格式：“事件=A，好人=B，坏人=C，坏人2=D，本国=E，敌国=F”也可以不全填
        /// 讽刺 好人=bot，坏人=我，事件=乐bot
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleJoke(MessageContext context, string[] param)
        {
            try
            {
                // 笑话输入格式：“事件=A，好人=B，坏人=C，坏人2=D，本国=E，敌国=F”
                var items = param[1].Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (items.Length >= 1)
                {
                    List<(string, string)> pairs = new List<(string, string)>();
                    foreach (var item in items)
                    {
                        var pair = item.Split(new char[] { ':', '：', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (pair.Length == 2) pairs.Add((pair[0], pair[1]));
                    }
                    if (pairs.Count > 0)
                    {
                        try
                        {
                            var joke = Joke.GetRandomJoke(pairs);
                            return joke;
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                }
            }
            catch { }

            return "";
        }

        /// <summary>
        /// 打乱字符串，N切M次表示循环操作M次，每次把原文分成N段再重排序
        /// 3切123456/2切3次abcdefg
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleCutString(MessageContext context, string[] param)
        {
            string sTarget = param[3];
            string sTime = param[2];
            string sNum = param[1];

            string target = sTarget.Trim();
            // K切多次
            int runTime = 0;
            int cutNum = 0;
            int.TryParse(sNum, out cutNum);
            if (cutNum < 1) return "";
            int.TryParse(sTime, out runTime);
            if (runTime < 1) runTime = 1;

            runTime = Math.Min(runTime, 5);
            for (int i = 0; i < runTime; i++)
            {
                target = Util.ShuffleString(target, cutNum) + "\r\n";
            }
            return target;
        }

        /// <summary>
        /// 来点随机汉字
        /// 随机20/随机4*5
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleRandomString(MessageContext context, string[] param)
        {
            int rows = 1;
            int columns = 1;
            int.TryParse(param[1], out columns);    // 如果没有列数，默认为1
            int.TryParse(param[2], out rows);
            if (columns < 1) columns = 1;
            if (rows < 1) rows = 1;

            if (rows * columns > 2500)
            {
                // return $"输入太多，溢出来了！";
                int maxrow = 2500 / columns;
                if (maxrow <= 0) { rows = 1; columns = 2500; }
                else rows= maxrow;
            }
            return GenerateRandomStringHans(rows, columns);
        }

        /// <summary>
        /// 根据模板生成攻受文
        /// A攻B受
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleGongshou(MessageContext context, string[] param)
        {
            string result = "";
            try
            {
                string gong = param[1];
                string shou = param[2];
                if (string.IsNullOrWhiteSpace(gong) || string.IsNullOrWhiteSpace(shou)
                    || gong.Length > 20 || shou.Length > 20)
                {
                    return "";
                }
                result = Gongshou.Get(gong, shou);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return result;
        }

        /// <summary>
        /// 乱序字符串
        /// 乱序12345
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleShuffle(MessageContext context, string[] param)
        {
            return Util.ShuffleString(param[1]);
        }


        /// <summary>
        /// 反转字符串
        /// 反转12345
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleReverse(MessageContext context, string[] param)
        {
            char[] charArray = param[1].ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// 逐行反转字符串
        /// 逐行反转 第一行\n第二行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleReverseByLine(MessageContext context, string[] param)
        {
            string[] sArray = param[1].Split('\n');
            Array.Reverse(sArray);
            return string.Join("\r\n",sArray);
        }

        /// <summary>
        /// 英文转大写
        /// 大写 abc
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleToUpper(MessageContext context, string[] param)
        {
            return param[1].ToUpper();
        }

        /// <summary>
        /// 英文转小写
        /// 小写 ABC
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleToLower(MessageContext context, string[] param)
        {
            return param[1].ToLower();
        }


        /// <summary>
        /// 垃圾文生成器
        /// 什么是：赛马
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleSalad(MessageContext context, string[] param)
        {
            return SpamText.Get(param[1]);
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
                Logger.Log(ex.Message + "\r\n" + ex.StackTrace);
            }




            return res;
        }







    }
}
