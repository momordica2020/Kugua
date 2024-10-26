﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MeowMiraiLib.GenericModel;

namespace MMDK.Util
{
    public class ModeHelper
    {
        public string replacefile = "replacewords.txt";
        string modeIndexName = "_index.txt";
        //string modePrivateName = "_mode_private.txt";
        //string modeGroupName = "_mode_group.txt";
        string defaultAnswerName = "_defaultanswer.txt";
        string sstvName = "sstv.jpg";
        string PluginPath;

        List<string> sstv = new List<string>();
        Dictionary<string, string> wordReplace = new Dictionary<string, string>();
        public Dictionary<string, ModeInfo> modedict = new Dictionary<string, ModeInfo>();
        List<string> defaultAnswers = new List<string>();
        //public Dictionary<long, string> privatemode = new Dictionary<long, string>();
        //public Dictionary<long, string> groupmode = new Dictionary<long, string>();
        public Random rand = new Random();
        MD5 md5 = MD5.Create();

        string chaosv = "混沌-名词.txt";
        string chaosm = "混沌-情绪词.txt";
        string chaosw = "混沌-小万邦部分.txt";
        List<string[]> chaosWord = new List<string[]>();
        List<string> chaosMotion = new List<string>();
        List<string> chaosXwb = new List<string>();

        string yunjief = "云杰说道.txt";
        List<string> yjsd = new List<string>();

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

        string penName = "pen.txt";
        List<string> penlist = new List<string>();

        string duiP2f = "pairc2.txt";
        string duiP1f = "pairc.txt";
        Dictionary<string, string[]> cf = new Dictionary<string, string[]>();
        Dictionary<string, string[]> cf2 = new Dictionary<string, string[]>();

        string junkf = "spam.txt";
        List<List<string>> junks = new List<List<string>>();

        string symbolf = "symboltemplate.txt";
        Dictionary<string, List<string>> symbollist = new Dictionary<string, List<string>>();


        string picsave = "picsave.txt";
        string piclingtang = "lingtang.jpg";
        List<string> pics = new List<string>();


        bool isAskme(string msg)
        {
            if (msg.StartsWith(Config.Instance.appConfig.Avatar.askName))
            {
                return true;
            }
            return false;
        }

        public void Init()
        {
            try
            {
                // load modes
                PluginPath = Config.Instance.ResourceFullPath("ModePath");
                modedict = new Dictionary<string, ModeInfo>();
                List<string> modelines = FileUtil.readLines($"{PluginPath}/{modeIndexName}").ToList();
                foreach (var line in modelines)
                {
                    var items = line.Split('\t');
                    string modeName = items[0].Trim();
                    try
                    {
                        string[] modeConfigs;
                        if (items.Length >= 2)
                        {
                            modeConfigs = items[1].Trim().Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);

                        }
                        else
                        {
                            modeConfigs = new string[1] { "默认" };
                        }
                        modedict[modeName] = new ModeInfo(modeName, modeConfigs, FileUtil.readLines($"{PluginPath}/{modeName}.txt").ToList());
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Log($"模式行[ {line} ]加载失败，{ex.Message}\r\n{ex.StackTrace}");
                    }
                }

                // replace
                wordReplace = new Dictionary<string, string>();
                var lines = FileUtil.readLines($"{PluginPath}/{replacefile}");
                foreach (var line in lines)
                {
                    var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        wordReplace[items[1]] = items[0];
                    }
                }


                //// group mode config
                //groupmode = new Dictionary<long, string>();
                //List<string> grouplines = FileUtil.readLines($"{PluginPath}/{modeGroupName}").ToList();
                //foreach (var line in grouplines)
                //{
                //    var items = line.Split('\t');
                //    if (items.Length >= 2)
                //    {
                //        groupmode[long.Parse(items[0])] = items[1].Trim();
                //    }
                //}
                //// private mode config
                //privatemode = new Dictionary<long, string>();
                //List<string> privatelines = FileUtil.readLines($"{PluginPath}/{modePrivateName}").ToList();
                //foreach (var line in privatelines)
                //{
                //    var items = line.Split('\t');
                //    if (items.Length >= 2)
                //    {
                //        privatemode[long.Parse(items[0])] = items[1].Trim();
                //    }
                //}

                // motions
                chaosMotion = FileUtil.readLines($"{PluginPath}/{chaosm}").ToList();
                // verb
                var wordlines = FileUtil.readLines($"{PluginPath}/{chaosv}").ToList();
                foreach (var line in wordlines)
                {
                    chaosWord.Add(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                // xwb
                chaosXwb = FileUtil.readLines($"{PluginPath}/{chaosw}").ToList();

                // yunjieshuodao
                yjsd = FileUtil.readLines($"{PluginPath}/{yunjief}").ToList();

                // random
                randomChar = FileUtil.readText($"{PluginPath}/{randomch}").Trim();

                // default
                defaultAnswers = FileUtil.readLines($"{PluginPath}/{defaultAnswerName}").ToList();

                // sstv
                sstv = FileUtil.readLines($"{PluginPath}/{sstvName}").ToList();

                // pics
                pics = FileUtil.readLines($"{PluginPath}/{picsave}").ToList();


                new Thread(workInitModes).Start();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message + "\r\n" + e.StackTrace);
            }
        }


        public bool HandleMessage(long user, long group, string question,  ref List<string> results, bool hasAt=false)
        {          
            // 功能介绍
            if (hasAt && new string[] { "用法", "介绍", "功能", "选项", "帮助", "配置", "设定", "菜单" }.Contains(question))
            {
                results.Add( getWelcomeString());
                
                return true;
            }

            if (hasAt && question.StartsWith("设置") && user==Config.Instance.appConfig.Avatar.adminQQ && group > 0)
            {
                string cmd = question.Substring(2);
                if (cmd.StartsWith("模式"))
                {
                    // group tag change
                    cmd = cmd.Substring(2);
                    if (cmd.StartsWith("+") || cmd.StartsWith("加"))
                    {
                        string newmode = cmd.Substring(1).Trim();
                        Config.Instance.GroupAddTag(group, newmode);
                        results.Add( $"已添加群tag:{newmode}");
                        return true;

                    }
                    else if (cmd.StartsWith("-") || cmd.StartsWith("减"))
                    {
                        string newmode = cmd.Substring(1).Trim();
                        Config.Instance.groupDeleteTag(group, newmode);

                        results.Add( $"已删除群tag:{newmode}");
                        return true;
                    }
                }

                if (cmd.StartsWith("拉黑") || cmd.StartsWith("屏蔽"))
                {
                    // group tag change
                    cmd = cmd.Substring(2);
                    if (cmd.StartsWith("+") || cmd.StartsWith("加"))
                    {

                        var targetItem = cmd.Substring(1).Trim().Split(new char[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                        if (targetItem.Length >= 1)
                        {
                            long targetUser;
                            long.TryParse(targetItem[0], out targetUser);
                            Config.Instance.PlayerSetTag(targetUser, "屏蔽");

                            if (targetItem.Length >= 2)
                            {
                                long targetTime = 10;
                                //int maxNum;
                                long.TryParse(targetItem[1], out targetTime);
                                //int.TryParse(targetItem[1], out maxNum);
                                Config.Instance.PlayerSetTag(targetUser, $"有限：{targetTime} {targetTime} {DateTime.Now.Ticks}");
                            }


                            results.Add($"已处理{targetUser}");
                            return true;
                        }
                    }
                    else if (cmd.StartsWith("-") || cmd.StartsWith("减"))
                    {
                        var targetItem = cmd.Substring(1).Trim().Split(new char[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                        if (targetItem.Length >= 1)
                        {
                            long targetUser;
                            long.TryParse(targetItem[0], out targetUser);
                            Config.Instance.PlayerSetTag(targetUser, "");

                            results.Add($"已处理{targetUser}");
                            return true;

                        }
                    }
                }
            }

            if (hasAt && question.Contains("模式列表"))
            {
                string modeindexs = printModeList();
                modeindexs += "~输入“xx模式on”即可切换模式~";

                results.Add(modeindexs);
                return true;
            }
            Regex modereg = new Regex("(\\S+)模式\\s*(on|off)", RegexOptions.IgnoreCase);
            var moderes = modereg.Match(question);
            if (hasAt && moderes.Success)
            {
                try
                {
                    string mode = moderes.Groups[1].ToString();
                    string swit = moderes.Groups[2].ToString().ToLower();
                    if (swit == "off") mode = "混沌";
                    if (!modePublic(mode))
                    {
                        if ((Config.Instance.GetGroupInfo(group).Type==PlaygroupType.Test && (mode == "测试" || mode == "喷人"))
                          || (Config.Instance.GroupIs(group, "姨") && (mode == "姨"))
                          || (Config.Instance.GroupIs(group, "六蛋") && (mode == "六蛋"))
                        )
                        {
                            // pass
                        }
                        else
                        {
                            results.Add($"~我还没有这个模式~");
                            results.Add(printModeList());
                            results.Add($"~输入“xx模式on”即可切换模式~");
                            return true;
                        }
                    }
                    if(group > 0)
                    {
                        // group
                        Config.Instance.GetGroupInfo(group).Tag = mode;
                    }
                    else
                    {
                        // private
                        Config.Instance.GetPlayerInfo(user).Tag = mode;
                    }
                    results.Add($"~{Config.Instance.appConfig.Avatar.askName}的{mode}模式启动~");

                    return true;
                }
                catch { }
            }

            if (hasAt && question.StartsWith("讽刺"))
            {
                string res = "";
                try
                {
                    var items = question.Substring(2).Trim().Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 1)
                    {
                        Dictionary<string, string> pairs = new Dictionary<string, string>();
                        foreach (var item in items)
                        {
                            var pair = item.Split(new char[] { ':', '：', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (pair.Length == 2) pairs[pair[0]] = pair[1];
                        }
                        if (pairs.Count > 0)
                        {
                            res = getJoke(pairs);
                        }
                    }
                }
                catch { }
                if (!string.IsNullOrWhiteSpace(res))
                {
                    results.Add(res);
                    return true;
                }
            }

            if (hasAt && question.StartsWith("乱序"))
            {
                try
                {
                    question = question.Substring(2).Trim();
                    string res = getShuffle(question);
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        results.Add(res);
                        return true;
                    }
                }
                catch
                {

                }

            }

            if (hasAt && question.StartsWith("什么是"))
            {

                try
                {
                    string key = question.Substring(3).Trim();
                    while (key.EndsWith("？") || key.EndsWith("?")) key = key.Substring(0, key.Length - 1);
                    string res = getSpam(key);
                    if (res.Length > 0)
                    {
                        res = res.Replace("【D】", Config.Instance.appConfig.Avatar.myQQ.ToString()).Replace("【C】", Config.Instance.appConfig.Avatar.askName);
                        results.Add(res);
                        return true;

                    }
                }
                catch
                {

                }

            }



            // 对联
            if (hasAt && question.StartsWith("上联"))
            {
                question = question.Substring(2).Trim();
                try
                {
                    string res = getDui(question);
                    if (string.IsNullOrWhiteSpace(res))
                    {
                        res = "？";
                    }
                    else
                    {
                        res = "下联  " + res;
                    }
                    results.Add(res);
                    return true;
                }
                catch
                {

                }
            }

            // 反转
            if (hasAt && question.StartsWith("反转"))
            {
                question = question.Substring(2).Trim();
                try
                {
                    string res = "";
                    foreach (var c in question) res = c + res;
                    res = res.Replace("\n\r", "\r\n");
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        results.Add(res);
                        return true;
                    }
                }
                catch
                {

                }
            }


            // 随机汉字
            if (hasAt && question.StartsWith("随机"))
            {
                question = question.Replace("随机", "").Trim();
                int time = 1;
                int num = 1;
                if (question.Contains("*"))
                {
                    try
                    {
                        var item = question.Split('*');
                        num = int.Parse(item[0]);
                        time = int.Parse(item[1]);
                    }
                    catch { }
                }
                try
                {
                    num = int.Parse(question);
                }
                catch { }
                string res = "";
                if (time > 0 && time < 300 && num > 0 && num < 300 && num * time < 1500)
                {
                    res = getRandomCharSentence(time, num);
                }
                else if (num * time <= 0)
                {
                    res = "？";
                }
                else
                {
                    res = "太多了，溢出来了！";
                }
                results.Add(res);
                return true;
            }

            // 攻受
            Regex gs = new Regex("(.+)攻(.+)受");
            var matchgs = gs.Match(question);
            if (hasAt && matchgs.Success)
            {
                try
                {
                    string res = getGongshou(matchgs.Groups[1].ToString(), matchgs.Groups[2].ToString());
                    if (res.Length > 0)
                    {
                        results.Add(res);
                        return true;

                    }
                }
                catch { }
            }


            if (hasAt && question == "状态" && 
                (Config.Instance.appConfig.Avatar.adminQQ == user 
                || Config.Instance.GetPlayerInfo(user).Type==PlayerType.Admin))
            {
                string rmsg = "";
                bool isGroup = group > 0 ? true : false;
                if (Config.Instance.GroupIs(group, "测试"))
                {
                    DateTime startTime = Config.Instance.appConfig.Log.StartTime;
                    rmsg += $"本次启动时间：{startTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - startTime).TotalDays.ToString("0.00")}天)\r\n";
                    rmsg += $"重启了{Config.Instance.appConfig.Log.beginTimes}次\r\n";
                    rmsg += $"加了{Config.Instance.appConfig.Log.numGroup}个群\r\n";
                    rmsg += $"在群里被乐{ Config.Instance.appConfig.Log.playTimeGroup }次\r\n";
                    rmsg += $"在私聊被乐{ Config.Instance.appConfig.Log.playTimePrivate }次\r\n";
                }

                if (isGroup) rmsg += $"在本群的配置是：{( string.IsNullOrWhiteSpace(Config.Instance.GetGroupInfo(group).Tag) ? "*平平无奇*":Config.Instance.GetGroupInfo(group).Tag)}\r\n";
                if (isGroup && (Config.Instance.GroupIs(group, "测试") || (Config.Instance.GroupIs(group, "闲聊")))) rmsg += $"在本群闲聊是{Config.Instance.GetGroupInfo(group).Tag}模式\r\n";
                else rmsg += $"目前是{Config.Instance.GetGroupInfo(user).Tag}模式\r\n";
                results.Add(rmsg);
                return true;
            }

            //if (question.StartsWith("赛博灵堂") && Config.Instance.groupIs(group, "测试"))
            //{
            //    if (msg.imgs.Count > 0)
            //    {
            //        string url = msg.imgs[0].url;
            //        string localpath = Path.GetFullPath($"{PluginPath}{msg.from}_tmp.jpg");
            //        Bitmap b = new Bitmap(PluginPath + piclingtang);
            //        Graphics g = Graphics.FromImage(b);
                    
            //        WebClient wc = new WebClient();

            //        wc.DownloadFile(url, localpath);
            //        Bitmap get = new Bitmap(localpath);
            //        ImageHelper.setGray(get);
            //        g.DrawImage(get, new Rectangle(230, 70, 50, 70), new Rectangle(0, 0, get.Width, get.Height),GraphicsUnit.Pixel);
            //        get.Dispose();
            //        msg.imgs.Clear();
            //        MessageImage msgimg = new MessageImage();
            //        msgimg.path = localpath;
            //        msg.imgs.Add(msgimg);
            //        g.Dispose();
            //        b.Save(localpath);
            //        msg.str = "";
            //        msg.ats.Clear();
            //        msg.faces.Clear();
            //        BOT.sendBack(msg);
            //    }
            //    else
            //    {
            //        msg.str = "输入 赛博灵堂 后面跟一张图片，即可生成赛博灵堂，转载请注明“独人13”，谢谢，，，";
            //        BOT.sendBack(msg);
            //    }
            //}

            // 特殊符号操作
            try
            {
                string res = getSymbolDeal(question);
                if (!string.IsNullOrWhiteSpace(res))
                {
                    results.Add(res);
                    return true;
                }
            }
            catch
            {

            }






            
            if (hasAt || group <= 0 || (Config.Instance.GetGroupInfo(group).Type==PlaygroupType.Test && rand.Next(0,100) < 5))
            {
                string modeName = "";
                if (group > 0)
                {
                    modeName = Config.Instance.GetGroupInfo(group).Tag;
                }
                else
                {
                    // private
                    modeName = Config.Instance.GetPlayerInfo(user).Tag;
                }
                string answer = "";
                switch (modeName)
                {
                    case "正常":
                    case "混沌": answer += getAnswerChaos(user, question); break;
                    case "小万邦": answer += getGong(); break;
                    case "喷人": results.AddRange(getPen(group, user));return true;
                    case "测试": results.AddRange(getHistoryReact(group, user)); return true;

                    default: answer += getAnswerWithMode(user, question, modeName); break;
                }
                if (!string.IsNullOrWhiteSpace(answer))
                {
                    results.Add(answer);
                    return true;
                }


            }

            return false;

        }

        //public void getRandomPic(Message msg)
        //{
        //    msg.imgs.Clear();
        //    MessageImage img = new MessageImage();
        //    img.url = pics[rand.Next(pics.Count)];
        //    msg.imgs.Add(img);
        //    msg.str = "";
        //    msg.ats.Clear();
        //    msg.faces.Clear();
        //}

        //public void savePic(Message msg)
        //{
        //    foreach(var img in msg.imgs)
        //    {
        //        pics.Add(img.url);
        //    }
        //}

        /// <summary>
        /// bot的欢迎文本
        /// </summary>
        /// <returns></returns>
        public string getWelcomeString()
        {
            return "" +
                $"想在群里使用，就at我或者打字开头加“{Config.Instance.appConfig.Avatar.askName}”，再加内容。私聊乐我的话直接发内容。\r\n" +
                "以下是群常用功能。私聊可以闲聊。\r\n" +
                //"~状态查看：“状态”\r\n" +
                //"~模式更换：“模式列表”、“xx模式on”\r\n" +
                "掷骰：“rd 成功率”“r3d10 攻击力”\r\n" +
                "多语翻译：“汉译法译俄 xxxx”\r\n" +
                //"~天气预报：“北京明天天气”\r\n" +
                "B站live搜索：“绘画区谁在播”“虚拟区有多少B限”“xxx在播吗”\r\n" +
                //"~赛马：“赛马介绍”“签到”“个人信息”\r\n" +
                "生成攻受文：“A攻B受”\r\n" +
                //"~生成谴责：“A谴责B的C”\r\n" +
                // "~生成笑话：“讽刺 本国=A国，好人=甲，坏人=乙，事件=xx”\r\n" +
                //"生成随机汉字：“随机5*4”\r\n" +
                "周易占卜：“占卜 xxx”\r\n";
        }


        public void workInitModes()
        {

            try
            {
                // gongshou
                gongshou = new List<string>();
                var res = FileUtil.readLines($"{PluginPath}/{gongshouName}");
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
                res = FileUtil.readLines($"{PluginPath}/{qianzeName}");
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
                res = FileUtil.readLines($"{PluginPath}/{jokeName}");
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

                // pen
                penlist = FileUtil.readLines($"{PluginPath}/{penName}").ToList();

                // duilian
                var lines = FileUtil.readLines($"{PluginPath}/{duiP1f}" );
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    var items2 = items[1].Split(',');
                    cf[items[0]] = items2;
                }
                lines = FileUtil.readLines($"{PluginPath}/{duiP2f}" );
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    var items2 = items[1].Split(',');
                    cf2[items[0]] = items2;
                }

                // junk
                if (File.Exists($"{PluginPath}/{junkf}" ))
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
                lines = FileUtil.readLines($"{PluginPath}/{symbolf}" );
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


                //// cangtou
                //lines = FileHelper.readLines(path + pyf, Encoding.UTF8);
                //foreach (var line in lines)
                //{
                //    var items = line.Trim().Split(' ');
                //    if (items.Length >= 2)
                //    {
                //        char ch = items[0][0];
                //        for (int i = 1; i < items.Length; i++)
                //        {
                //            string pyall = items[i];
                //            string pyori = pyall;
                //            if ("12345".Contains(pyori.Last()))
                //            {
                //                pyori = pyori.Substring(0, pyori.Length - 1);
                //            }
                //            if (!py.ContainsKey(ch)) py[ch] = new List<string>();
                //            py[ch].Add(pyall);
                //            //py[ch]
                //        }
                //    }
                //}
                //lines = FileHelper.readLines(path + cangtou5f, Encoding.UTF8);
                //foreach (var line in lines)
                //{
                //    var ttmp = line.Trim();
                //    if (ttmp.Length > 0)
                //    {
                //        char targetch = ttmp[0];
                //        if (!cangtou5.ContainsKey(targetch)) cangtou5[targetch] = new List<string>();
                //        cangtou5[targetch].Add(ttmp);
                //        if (py.ContainsKey(targetch))
                //        {
                //            foreach (var pyi in py[targetch])
                //            {
                //                if (!cangtou5py.ContainsKey(pyi)) cangtou5py[pyi] = new List<string>();
                //                cangtou5py[pyi].Add(ttmp);
                //                string pyiori = pyi.Substring(0, pyi.Length - 1);
                //                if (!cangtou5py.ContainsKey(pyiori)) cangtou5py[pyiori] = new List<string>();
                //                cangtou5py[pyiori].Add(ttmp);
                //            }
                //        }
                //        targetch = ttmp[ttmp.Length - 1];
                //        if (!cangwei5.ContainsKey(targetch)) cangwei5[targetch] = new List<string>();
                //        cangwei5[targetch].Add(ttmp);
                //        if (py.ContainsKey(targetch))
                //        {
                //            foreach (var pyi in py[targetch])
                //            {
                //                if (!cangwei5py.ContainsKey(pyi)) cangwei5py[pyi] = new List<string>();
                //                cangwei5py[pyi].Add(ttmp);
                //                string pyiori = pyi.Substring(0, pyi.Length - 1);
                //                if (!cangwei5py.ContainsKey(pyiori)) cangwei5py[pyiori] = new List<string>();
                //                cangwei5py[pyiori].Add(ttmp);
                //            }
                //        }
                //    }
                //}
                //lines = FileHelper.readLines(path + cangtou7f, Encoding.UTF8);
                //foreach (var line in lines)
                //{
                //    var ttmp = line.Trim();
                //    if (ttmp.Length > 0)
                //    {
                //        char targetch = ttmp[0];
                //        if (!cangtou7.ContainsKey(targetch)) cangtou7[targetch] = new List<string>();
                //        cangtou7[targetch].Add(ttmp);
                //        if (py.ContainsKey(targetch))
                //        {
                //            foreach (var pyi in py[targetch])
                //            {
                //                if (!cangtou7py.ContainsKey(pyi)) cangtou7py[pyi] = new List<string>();
                //                cangtou7py[pyi].Add(ttmp);
                //                string pyiori = pyi.Substring(0, pyi.Length - 1);
                //                if (!cangtou7py.ContainsKey(pyiori)) cangtou7py[pyiori] = new List<string>();
                //                cangtou7py[pyiori].Add(ttmp);
                //            }
                //        }
                //        targetch = ttmp[ttmp.Length - 1];
                //        if (!cangwei7.ContainsKey(targetch)) cangwei7[targetch] = new List<string>();
                //        cangwei7[targetch].Add(ttmp);
                //        if (py.ContainsKey(targetch))
                //        {
                //            foreach (var pyi in py[targetch])
                //            {
                //                if (!cangwei7py.ContainsKey(pyi)) cangwei7py[pyi] = new List<string>();
                //                cangwei7py[pyi].Add(ttmp);
                //                string pyiori = pyi.Substring(0, pyi.Length - 1);
                //                if (!cangwei7py.ContainsKey(pyiori)) cangwei7py[pyiori] = new List<string>();
                //                cangwei7py[pyiori].Add(ttmp);
                //            }
                //        }
                //    }
                //}

                //// szm
                //lines = File.ReadAllLines(path + szmf, Encoding.UTF8);
                //foreach (var line in lines)
                //{
                //    var items = line.Trim().Split('\t');
                //    if (items.Length >= 2)
                //    {
                //        if (!szm.ContainsKey(items[1])) szm[items[1]] = new List<string>();
                //        szm[items[1]].Add(items[0]);
                //    }
                //}

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        public bool modePublic(string modeName)
        {
            if (modeExist(modeName) && !modedict[modeName].config.Contains("隐藏")) return true;
            else return false;
        }

        public bool modeExist(string modeName)
        {
            if (!modedict.ContainsKey(modeName))
            {
                // mode not exist!
                Logger.Instance.Log("mode " + modeName + " not exist.");
                return false;
            }
            return true;
        }

       


        public string printModeList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var mode in modedict)
            {
                if (mode.Value.config.Contains("隐藏"))
                {
                    // hide
                }
                else
                {
                    sb.Append($"{mode.Key}模式\r\n");
                }

            }
            return sb.ToString();
        }



        /// <summary>
        /// 按照模式随机生成回复
        /// 模式是在配置文件里添加的，bot初始化时会从中读取要加载的模式，然后把句子都扔进内存来缓存
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string getAnswerWithMode(long user, string question, string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return "";
            if (modedict.ContainsKey(mode))
            {
                return modedict[mode].getRandomSentence(question);
            }
            return "";
        }

        /// <summary>
        /// 龚诗 bot 特有的模拟
        /// </summary>
        /// <returns></returns>
        public string getGong()
        {
            StringBuilder sb = new StringBuilder();

            int snum = rand.Next(1, 5);
            for (int i = 0; i < snum; i++)
            {
                int wnum = rand.Next(1, 5);
                int nowlen = 0;
                for (int j = 0; j < wnum; j++)
                {
                    string s = chaosXwb[rand.Next(chaosXwb.Count)];
                    sb.Append(s);
                    nowlen += s.Length;
                    if (nowlen > 15) break;
                }
                if (i < snum - 1) sb.Append("，");
                else sb.Append("。？！"[rand.Next(3)]);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 混沌模式的组句，比其他模式稍复杂些。从2个库中按概率抽取内容，整体上接近小万邦的同时加入新词
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getChaosRandomSentence(string str)
        {
            string[] sgn = new string[] { "\r\n", "。", "？", "！", "…", "——", "??", "...", "：", "?!", "???", "!!", "！！！" };
            string result = "";
            byte[] md5data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            int sentences = rand.Next(1, 6);

            for (int i = 0; i < sentences; i++)
            {
                int thislen = rand.Next(0, 11);
                StringBuilder thissentence = new StringBuilder();
                int wordnum = 0;
                while (thissentence.Length < thislen && wordnum < 5)
                {
                    wordnum++;
                    if (rand.Next(0, 100) > 80)
                    {
                        thissentence.Append(chaosWord[rand.Next(0, chaosWord.Count - 1)][0]);
                    }
                    else
                    {
                        thissentence.Append(chaosXwb[rand.Next(0, chaosXwb.Count - 1)]);
                    }
                }
                thissentence.Append(sgn[rand.Next(sgn.Length)]);
                result += thissentence.ToString();
            }

            return result;
        }

        public string getRandomCharSentence(int time, int num)
        {
            string result = "";

            for (int i = 0; i < time; i++)
            {
                for (int j = 0; j < num; j++)
                {
                    result += randomChar[rand.Next(randomChar.Length)];
                }
                result += " ";
            }

            return result;
        }

        public string getGongshou(string gong, string shou)
        {
            string result = "";

            try
            {
                if (!string.IsNullOrWhiteSpace(gong) && !string.IsNullOrWhiteSpace(shou) && gongshou.Count > 0)
                {
                    result = gongshou[rand.Next(gongshou.Count)];
                    result = result.Replace("<攻>", gong).Replace("<受>", shou);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex.Message + "\r\n" + ex.StackTrace);
            }



            return result;
        }

        public string getJoke(Dictionary<string, string> pairs)
        {
            string result = "";

            try
            {
                List<string> usingjokes = new List<string>();
                if (pairs.ContainsKey("敌国")) usingjokes.AddRange(jokesEnemy);
                if (pairs.ContainsKey("部门")) usingjokes.AddRange(jokesOrg);
                if (pairs.ContainsKey("事件")) usingjokes.AddRange(jokesEvent);
                if (usingjokes.Count <= 0) usingjokes.AddRange(jokes);
                int find = 100;
                int index = rand.Next(usingjokes.Count);
                do
                {
                    result = usingjokes[index];
                    foreach (var pair in pairs)
                    {
                        result = result.Replace($"【{pair.Key}】", pair.Value);
                    }
                    if (result.Contains("【"))
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



            return result;
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

                        var temp = sb.Value[rand.Next(sb.Value.Count)];
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

        public string getSpam(string key)
        {
            string result = "";

            try
            {
                foreach (var para in junks)
                {
                    if (para.Count > 0)
                    {
                        result += para[rand.Next(para.Count)] + "\r\n";
                    }
                }
                result = result.Replace("【E】", DateTime.Now.Year.ToString());
                result = result.Replace("【B】", new string[] { "朋友", "小伙伴", "网友" }[rand.Next(3)]);
                result = result.Replace("【A】", key);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex.Message + "\r\n" + ex.StackTrace);
            }


            return result;
        }

        public string getDui(string sin)
        {

            sin = sin.Trim();
            string sout = "";

            for (int i = 0; i < sin.Length; i++)
            {
                if (i + 1 < sin.Length && cf2.ContainsKey(sin.Substring(i, 2)))
                {
                    sout += cf2[sin.Substring(i, 2)][rand.Next(cf2[sin.Substring(i, 2)].Length)];
                    i += 1;
                }
                else if (cf.ContainsKey(sin[i].ToString()))
                {
                    sout += cf[sin[i].ToString()][rand.Next(cf[sin[i].ToString()].Length)];
                }
                //else if("３")
                else if ("123456789".Contains(sin[i]))
                {
                    sout = $"{sout}{10 - int.Parse(sin[i].ToString())}";
                }
                else if ("abcdefghijklmnopqrstuvwxyz".Contains(sin[i]))
                {
                    sout += "abcdefghijklmnopqrstuvwxyz"[rand.Next(26)];
                }
                else if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(sin[i]))
                {
                    sout += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rand.Next(26)];
                }
                else if ("あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ".Contains(sin[i]))
                {
                    sout += "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ"[rand.Next(71)];
                }
                else if ("アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ".Contains(sin[i]))
                {
                    sout += "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ"[rand.Next(71)];
                }
                else
                {
                    sout += sin[i];
                }
            }
            return sout;




        }


        static string sym = "，。、；：【】？“”‘’《》！￥…—{}[]()+=-/*!@#$%^&_|,.?:;/\\'\" \t\r\n";
        public static bool isSymbol(char ch)
        {
            return sym.Contains(ch);
        }
        /// <summary>
        /// 去除字符串中的中英文标点和特殊字符
        /// </summary>
        /// <param name="ori"></param>
        /// <returns></returns>
        public static string removeSymbol(string ori)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var c in ori)
            {
                if (!isSymbol(c)) sb.Append(c);
            }
            return sb.ToString();
        }


        public string getShuffle(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            string dealdatas = removeSymbol(str);
            StringBuilder res = new StringBuilder();
            foreach (char c in str)
            {
                if (isSymbol(c)) res.Append(c);
                else
                {
                    int index = rand.Next(dealdatas.Length);
                    res.Append(dealdatas[index]);
                    dealdatas = dealdatas.Remove(index, 1);
                }
            }

            return res.ToString();
        }

        public IEnumerable<string> getPen(long group, long user)
        {
            try
            {
                int num = rand.Next(2, 10);
                List<string> res = new List<string>();
                while (num-- > 0)
                {
                    res.Add( penlist[rand.Next(penlist.Count)].Trim());
                }
                return res;
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message + "\r\n" + e.StackTrace);
                return new List<string>();
            }
        }

        public IEnumerable<string> getHistoryReact(long group, long userqq)
        {
            List<string> result = new List<string>();

            string historyPath = Path.GetFullPath($"{Config.Instance.ResourceFullPath("HistoryPath")}/group");
            var files = Directory.GetFiles(historyPath, "*.txt");
            int maxtime = 10;
            try
            {
                if (files.Length <= 0) return result;
                while (maxtime-- > 0)
                {
                    int findex = rand.Next(files.Length);
                    string[] lines = FileUtil.readLines(files[findex]).ToArray();
                    if (lines.Length < 100) continue;
                    int begin = rand.Next(lines.Length - 5);
                    int maxnum = rand.Next(1, 5);
                    int num = lines.Length - begin;// rand.Next(10, lines.Length - begin);
                    bool find = false;
                    string targetuser = "";
                    for (int i = 0; i < num; i++)
                    {
                        try
                        {
                            var items = lines[begin + i].Trim().Split('\t');
                            if (items.Length >= 3)
                            {
                                //string ban = "2715126750 2045098852 188618935 2854196310 287859992 2963959417";
                                //if (ban.Contains(items[1])) continue;
                                if (targetuser.Length > 0 && targetuser != items[1]) continue;
                                targetuser = items[1];
                                string msg = items[2].Trim();
                                if (msg.Contains("2715126750") || msg.Contains("2045098852")) continue;
                                bool isSstv = false;
                                foreach (var word in sstv)
                                {
                                    if (!string.IsNullOrWhiteSpace(word) && msg.Contains(word))
                                    {
                                        isSstv = true;
                                        break;
                                    }
                                }
                                if (isSstv) continue;
                                msg = Regex.Replace(msg, "\\[CQ\\:[^\\]]+\\]", "");
                                if (string.IsNullOrWhiteSpace(msg.Trim())) continue;
                                result.Add(msg);
                                find = true;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.Log(e.Message + "\r\n" + e.StackTrace);
                        }
                        maxnum -= 1;
                        if (maxnum <= 0) break;

                    }
                    if (find)
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message + "\r\n" + e.StackTrace);
            }


            return result;
        }


        public string getAnswerGong(long user, string question)
        {
            string msg = "";
            if (rand.Next(0, 100) < 85)
            {
                msg = getChaosRandomSentence(question) + getMotionString();
            }
            else
            {
                if (msg.Length <= 0 || rand.Next(1, 100) < 40)
                {
                    msg = getSaoHua() + getMotionString();
                }
            }

            return msg;
        }

        /// <summary>
        /// 混沌模式的回复
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        public string getAnswerChaos(long user, string question)
        {
            string answer = "";
            string msg = "";
            if (rand.Next(0, 100) < 85)
            {
                msg = getChaosRandomSentence(question) + getMotionString();
            }
            else
            {
                //answer = getZhidaoAnswer(question);
                //if (answer.Length > 0)
                //{
                //    msg = answer + "..." + getMotionString();
                //}
                if (msg.Length <= 0 || rand.Next(1, 100) < 40)
                {
                    msg = getSaoHua() + getMotionString();
                }
            }

            return msg;
        }


        /// <summary>
        /// 获取骚话（情话）
        /// </summary>
        /// <returns></returns>
        public string getSaoHua()
        {
            try
            {
                return defaultAnswers[rand.Next(defaultAnswers.Count)].Trim();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message + "\r\n" + e.StackTrace);
                return "";
            }
        }

        /// <summary>
        /// 获取随机的括弧情绪文本。例如（悲）（大嘘）这种
        /// </summary>
        /// <returns></returns>
        public string getMotionString()
        {
            string res = "";

            if (chaosMotion.Count <= 0) return res;
            if (rand.Next(0, 100) > 66)
            {
                res = $"({chaosMotion[rand.Next(0, chaosMotion.Count - 1)]})";
            }

            return res;
        }

        public void Dispose()
        {
            try
            {
                FileUtil.writeLines($"{PluginPath}/{picsave}", pics);
            }catch(Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            
        }
    }


    public class ModeInfo
    {
        Random rand = new Random((int)DateTime.Now.Ticks);
        public string name;
        public List<string> config;
        List<string> sentences;
        // public int 
        public ModeInfo()
        {
            config = new List<string>();
            sentences = new List<string>();
        }

        public ModeInfo(string _name, ICollection<string> _config, ICollection<string> _sentences)
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

            int sn = rand.Next(1, maxsnum);

            for (int i = 0; i < sn; i++)
            {
                int thislen = rand.Next(1, maxslen);
                StringBuilder thissentence = new StringBuilder();
                int wordnum = 0;
                while (thissentence.Length < thislen && wordnum < maxwordnum)
                {
                    wordnum++;
                    thissentence.Append(sentences[rand.Next(0, sentences.Count - 1)]);
                }
                if (thissentence.Length > 0 && !sgn1.Contains(thissentence.ToString().Last().ToString()) && !sgn2.Contains(thissentence.ToString().Last().ToString()))
                {
                    if (config.Contains("无标点")) thissentence.Append(" ");
                    else thissentence.Append(sgn1[rand.Next(sgn1.Length)]);
                    result += thissentence.ToString();
                    if (result.Length > 0)
                    {
                        if (config.Contains("无标点")) ;
                        else if (config.Contains("乱打标点")) result = result.Substring(0, result.Length - 1) + sgn3[rand.Next(sgn3.Length)];
                        else result = result.Substring(0, result.Length - 1) + sgn2[rand.Next(sgn2.Length)];
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
                else if (config.Contains("乱打标点")) result = result.Substring(0, result.Length - 1) + sgn3[rand.Next(sgn3.Length)];
                else result = result.Substring(0, result.Length - 1) + sgn2[rand.Next(sgn2.Length)];
            }


            return result;
        }
    }

}
