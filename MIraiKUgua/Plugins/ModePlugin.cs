using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MMDK.Core;
using MMDK.Struct;
using MMDK.Util;

namespace MMDK.Plugins
{
    class ModePlugin : Plugin
    {
        public string replacefile = "replacewords.txt";
        string modeIndexName = "_index.txt";
        string modePrivateName = "_mode_private.txt";
        string modeGroupName = "_mode_group.txt";
        string defaultAnswerName = "_defaultanswer.txt";

        string sstvName = "sstv.jpg";
        public List<string> sstv = new List<string>();

        Dictionary<string, string> wordReplace = new Dictionary<string, string>();
        public Dictionary<string, ModeInfo> modedict = new Dictionary<string, ModeInfo>();
        List<string> defaultAnswers = new List<string>();
        public Dictionary<long, string> privatemode = new Dictionary<long, string>();
        public Dictionary<long, string> groupmode = new Dictionary<long, string>();
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
        public ModePlugin() : base("Mode")
        {

        }
        public override void InitSource()
        {
            try
            {
                // load modes
                modedict = new Dictionary<string, ModeInfo>();
                List<string> modelines = FileHelper.readLines(PluginPath + modeIndexName).ToList();
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
                        modedict[modeName] = new ModeInfo(modeName, modeConfigs, FileHelper.readLines($"{PluginPath}\\{modeName}.txt").ToList());
                    }
                    catch (Exception ex)
                    {
                        FileHelper.Log($"模式行[ {line} ]加载失败，{ex.Message}\r\n{ex.StackTrace}");
                    }
                }

                // replace
                wordReplace = new Dictionary<string, string>();
                var lines = FileHelper.readLines(PluginPath + replacefile);
                foreach (var line in lines)
                {
                    var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        wordReplace[items[1]] = items[0];
                    }
                }


                // group mode config
                groupmode = new Dictionary<long, string>();
                List<string> grouplines = FileHelper.readLines(PluginPath + modeGroupName).ToList();
                foreach (var line in grouplines)
                {
                    var items = line.Split('\t');
                    if (items.Length >= 2)
                    {
                        groupmode[long.Parse(items[0])] = items[1].Trim();
                    }
                }
                // private mode config
                privatemode = new Dictionary<long, string>();
                List<string> privatelines = FileHelper.readLines(PluginPath + modePrivateName).ToList();
                foreach (var line in privatelines)
                {
                    var items = line.Split('\t');
                    if (items.Length >= 2)
                    {
                        privatemode[long.Parse(items[0])] = items[1].Trim();
                    }
                }

                // motions
                chaosMotion = FileHelper.readLines(PluginPath + chaosm).ToList();
                // verb
                var wordlines = FileHelper.readLines(PluginPath + chaosv).ToList();
                foreach (var line in wordlines)
                {
                    chaosWord.Add(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                // xwb
                chaosXwb = FileHelper.readLines(PluginPath + chaosw).ToList();

                // yunjieshuodao
                yjsd = FileHelper.readLines(PluginPath + yunjief).ToList();

                // random
                randomChar = FileHelper.readText(PluginPath + randomch).Trim();

                // default
                defaultAnswers = FileHelper.readLines(PluginPath + defaultAnswerName).ToList();

                // sstv
                sstv = FileHelper.readLines(PluginPath + sstvName).ToList();


                new Thread(workInitModes).Start();
            }
            catch (Exception e)
            {
                FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
            }
        }
        public override void HandleMessage(Message msg)
        {
            string question = BOT.getAskCmd(msg);
            //if (string.IsNullOrWhiteSpace(question)) return;
            long user = msg.from;
            long group = msg.fromGroup;


            // 功能介绍
            if (new string[] { "用法", "介绍", "功能", "选项", "帮助", "配置", "设定", "菜单" }.Contains(question))
            {
                msg.str = getWelcomeString();
                BOT.sendBack(msg, true);
                return;
            }

            if (question.StartsWith("设置") && config.personIs(user, "管理员") && msg.fromGroup > 0)
            {
                string cmd = question.Substring(2);

                if (cmd.StartsWith("紧急"))
                {
                    cmd = cmd.Substring(2);
                    if (cmd.StartsWith("中止") || cmd.StartsWith("停止"))
                    {
                        //sendPrivate(config.masterQQ, "已紧急中止。");
                        msg.str = $"Well, It shall be done.";
                        BOT.sendBack(msg);
                        config["ignoreall"] = "1";
                        config.save();
                        return;
                    }
                    else if (cmd.StartsWith("恢复"))
                    {
                        config["ignoreall"] = "0";
                        config["testonly"] = "0";
                        msg.str = $"Hello, Tencent.";
                        BOT.sendBack(msg);
                        config.save();
                        return;
                    }
                    else if (cmd.StartsWith("封闭"))
                    {
                        // sendPrivate(config.masterQQ, "已封闭。仅测试群和管理员账号可响应。");

                        config["ignoreall"] = "0";
                        config.save();
                        msg.str = $"Closing myself.";
                        BOT.sendBack(msg);
                        config.save();
                        return;
                    }
                    return;
                }


                if (cmd.StartsWith("模式"))
                {
                    // group tag change
                    cmd = cmd.Substring(2);
                    if (cmd.StartsWith("+") || cmd.StartsWith("加"))
                    {
                        string newmode = cmd.Substring(1).Trim();
                        config.groupAddTag(group, newmode);
                        msg.str = $"已添加群tag:{newmode}";
                        BOT.sendBack(msg);
                        config.save();
                        return;

                    }
                    else if (cmd.StartsWith("-") || cmd.StartsWith("减"))
                    {
                        string newmode = cmd.Substring(1).Trim();
                        config.groupDeleteTag(group, newmode);

                        msg.str = $"已删除群tag:{newmode}";
                        BOT.sendBack(msg);
                        config.save();
                        return;
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
                            config.personAddTag(targetUser, "屏蔽");

                            if (targetItem.Length >= 2)
                            {
                                long targetTime = 10;
                                //int maxNum;
                                long.TryParse(targetItem[1], out targetTime);
                                //int.TryParse(targetItem[1], out maxNum);
                                config.personAddTag(targetUser, $"有限：{targetTime} {targetTime} {DateTime.Now.Ticks}");
                            }



                            msg.str = $"已处理{targetUser}";
                            BOT.sendBack(msg);
                            return;
                        }
                    }
                    else if (cmd.StartsWith("-") || cmd.StartsWith("减"))
                    {
                        var targetItem = cmd.Substring(1).Trim().Split(new char[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                        if (targetItem.Length >= 1)
                        {
                            long targetUser;
                            long.TryParse(targetItem[0], out targetUser);
                            config.personDeleteTag(targetUser, "屏蔽");

                            msg.str = $"已处理{targetUser}";
                            BOT.sendBack(msg);
                            return;
                        }
                    }
                }

                //if (cmd.StartsWith("拳交"))
                //{
                //    int qjnum = 0;
                //    int.TryParse(cmd.Substring(2), out qjnum);
                //    config.useGroupMsgBuf = qjnum;
                //    sendGroup(group, -1, $"目前AMM值:{qjnum}");
                //    return true;
                //}


            }




            if (question.Contains("模式列表"))
            {
                string modeindexs = printModeList();
                modeindexs += "~输入“xx模式on”即可切换模式~";
                msg.str = modeindexs;
                BOT.sendBack(msg, true);
                return;
            }
            Regex modereg = new Regex("(\\S+)模式\\s*(on|off)", RegexOptions.IgnoreCase);
            var moderes = modereg.Match(question);
            if (moderes.Success)
            {
                try
                {
                    string mode = moderes.Groups[1].ToString();
                    string swit = moderes.Groups[2].ToString().ToLower();
                    if (swit == "off") mode = "混沌";
                    if (!modePublic(mode))
                    {
                        if ((config.groupIs(group, "测试") && (mode == "测试" || mode == "喷人"))
                          || (config.groupIs(group, "姨") && (mode == "姨"))
                          || (config.groupIs(group, "六蛋") && (mode == "六蛋"))
                        )
                        {
                            // pass
                        }
                        else
                        {
                            string modeindexs = "我还没有这个模式";

                            msg.str = modeindexs;
                            BOT.sendBack(msg, true);

                            modeindexs = printModeList();
                            modeindexs += "~输入“xx模式on”即可切换模式~";

                            msg.str = modeindexs;
                            BOT.sendBack(msg, true);
                            return;
                        }
                    }
                    if(msg.fromGroup > 0)
                    {
                        // group
                        setGroupMode(group, mode);
                    }
                    else
                    {
                        // private
                        setUserMode(user, mode);
                    }
                    msg.str = $"~{config["askname"]}的{mode}模式启动~";

                    BOT.sendBack(msg, true);
                    return;
                }
                catch { }
            }

            if (question.StartsWith("讽刺"))
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
                    msg.str = res;
                    BOT.sendBack(msg, true);
                    return;
                }
            }

            if (question.StartsWith("乱序"))
            {
                try
                {
                    question = question.Substring(2).Trim();
                    string res = getShuffle(question);
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        msg.str = res;
                        BOT.sendBack(msg, true);
                        return;
                    }
                }
                catch
                {

                }

            }

            if (question.StartsWith("什么是"))
            {

                try
                {
                    string key = question.Substring(3).Trim();
                    while (key.EndsWith("？") || key.EndsWith("?")) key = key.Substring(0, key.Length - 1);
                    string res = getSpam(key);
                    if (res.Length > 0)
                    {
                        res = res.Replace("【D】", config["qq"]).Replace("【C】", config["askname"]);
                        msg.str = res;
                        BOT.sendBack(msg, true);
                        return;

                    }
                }
                catch
                {

                }

            }



            // 对联
            if (question.StartsWith("上联"))
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
                    msg.str = res;
                    BOT.sendBack(msg, true);
                    return;
                }
                catch
                {

                }
            }

            // 反转
            if (question.StartsWith("反转"))
            {
                question = question.Substring(2).Trim();
                try
                {
                    string res = "";
                    foreach (var c in question) res = c + res;
                    res = res.Replace("\n\r", "\r\n");
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        msg.str = res;
                        BOT.sendBack(msg, true);
                        return;
                    }
                }
                catch
                {

                }
            }

            Regex qz = new Regex("(.+)谴责(.+)的(.+)");
            var matchqz = qz.Match(question);
            if (matchqz.Success)
            {
                try
                {
                    string res = getQianze(matchqz.Groups[1].ToString(), matchqz.Groups[2].ToString(), matchqz.Groups[3].ToString());
                    if (res.Length > 0)
                    {
                        msg.str = res;
                        BOT.sendBack(msg, true);
                        return;
                    }
                }
                catch
                {

                }
            }


            // 随机汉字
            if (question.StartsWith("随机"))
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
                if (time > 0 && time < 200 && num > 0 && num < 200 && num * time < 1500)
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
                msg.str = res;
                BOT.sendBack(msg, true);
                return;
            }

            // 攻受
            Regex gs = new Regex("(.+)攻(.+)受");
            var matchgs = gs.Match(question);
            if (matchgs.Success)
            {
                try
                {
                    string res = getGongshou(matchgs.Groups[1].ToString(), matchgs.Groups[2].ToString());
                    if (res.Length > 0)
                    {
                        msg.str = res;
                        BOT.sendBack(msg, true);
                        return;

                    }
                }
                catch { }
            }


            if (question == "状态" && config.personIs(user, "管理员"))
            {
                string rmsg = "";
                bool isGroup = msg.fromGroup > 0 ? true : false;
                if (config.groupIs(group, "测试"))
                {
                    DateTime startTime = DateTime.Parse(config["starttime"]);
                    rmsg += $"首次启动时间：{startTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - startTime).TotalDays.ToString("0.00")}天)\r\n";
                    rmsg += $"本次启动时间：{config.thisStartTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - config.thisStartTime).TotalDays.ToString("0.00")}天)\r\n";
                    rmsg += $"重启了{config["startnum"]}次\r\n";
                    rmsg += $"加了{BOT.getGroupNum()}个群\r\n";
                    rmsg += $"在群里被乐{ config["playtimegroup"] }次\r\n";
                    rmsg += $"在私聊被乐{ config["playtimeprivate"] }次\r\n";
                }

                if (isGroup) rmsg += $"在本群的配置是：{(config.groupLevel.ContainsKey(group) ? string.Join("，", config.groupLevel[group]) : "*平平无奇*")}\r\n";
                if (isGroup && (config.groupIs(group, "测试") || (config.groupIs(group, "闲聊")))) rmsg += $"在本群闲聊是{ getGroupMode(group)}模式\r\n";
                else rmsg += $"目前是{getUserMode(user)}模式\r\n";

                msg.str = rmsg;
                BOT.sendBack(msg, true);
                return;
            }

            // 特殊符号操作
            try
            {
                string res = getSymbolDeal(question);
                if (!string.IsNullOrWhiteSpace(res))
                {
                    msg.str = res;
                    BOT.sendBack(msg, true);
                    return;
                }
            }
            catch
            {

            }





            if (BOT.isAskme(msg))
            {
                string modeName = "";
                if (msg.fromGroup > 0)
                {
                    modeName = getGroupMode(group);
                }
                else
                {
                    // private
                    modeName = getUserMode(user);
                }
                string answer = "";
                switch (modeName)
                {
                    //case "正常": answer += getAnswerNormal(user, question); break;
                    case "正常":
                    case "混沌": answer += getAnswerChaos(user, question); break;
                    case "小万邦": answer += getGong(); break;
                    case "喷人": answer += getPen(group, user); return; break;
                    case "测试": answer += getHistoryReact(group, user); return; break;
                    case "云杰": answer += getZYJ(question); break;
                    default: answer += getAnswerWithMode(user, question, modeName); break;
                }
                if (!string.IsNullOrWhiteSpace(answer))
                {
                    msg.str = answer;
                    BOT.sendBack(msg, true);
                    return;
                }


            }



        }


        /// <summary>
        /// bot的欢迎文本
        /// </summary>
        /// <returns></returns>
        public string getWelcomeString()
        {
            return "" +
                $"想在群里使用，就at我或者打字开头加“{config["askname"]}”，再加内容。私聊乐我的话直接发内容。\r\n" +
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
                var res = FileHelper.readLines(PluginPath + gongshouName);
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
                res = FileHelper.readLines(PluginPath + qianzeName);
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
                res = FileHelper.readLines(PluginPath + jokeName);
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
                penlist = FileHelper.readLines(PluginPath + penName).ToList();

                // duilian
                var lines = FileHelper.readLines(PluginPath + duiP1f);
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    var items2 = items[1].Split(',');
                    cf[items[0]] = items2;
                }
                lines = FileHelper.readLines(PluginPath + duiP2f);
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    var items2 = items[1].Split(',');
                    cf2[items[0]] = items2;
                }

                // junk
                if (File.Exists(PluginPath + junkf))
                {
                    lines = File.ReadAllLines(PluginPath + junkf, Encoding.UTF8);

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
                lines = FileHelper.readLines(PluginPath + symbolf);
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
                FileHelper.Log(ex);
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
                FileHelper.Log("mode " + modeName + " not exist.");
                return false;
            }
            return true;
        }

        public void setUserMode(long user, string modeName)
        {
            // if (!modeExist(modeName)) return;
            privatemode[user] = modeName;
            try
            {
                List<string> refreshMode = new List<string>();
                foreach (var k in privatemode.Keys) refreshMode.Add($"{k}\t{privatemode[k]}");
                File.WriteAllLines(PluginPath + modePrivateName, refreshMode.ToArray());
            }
            catch (Exception e)
            {
                FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        public void setGroupMode(long group, string modeName)
        {
            //if (!modeExist(modeName)) return;
            groupmode[group] = modeName;
            try
            {
                List<string> refreshMode = new List<string>();
                foreach (var k in groupmode.Keys) refreshMode.Add($"{k}\t{groupmode[k]}");
                File.WriteAllLines(PluginPath + modeGroupName, refreshMode.ToArray());
            }
            catch (Exception e)
            {
                FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
            }
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


        public string getGroupMode(long group)
        {
            string modeName = "混沌";
            if (groupmode.ContainsKey(group))
            {
                modeName = groupmode[group];
            }
            // if(!modeExist(modeName)) modeName = "混沌";
            return modeName;
        }

        public string getUserMode(long user)
        {
            string modeName = "混沌";
            if (privatemode.ContainsKey(user))
            {
                modeName = privatemode[user];
            }
            //if (!modeExist(modeName)) modeName = "混沌";
            return modeName;
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
            if (modedict.ContainsKey(mode))
            {
                return modedict[mode].getRandomSentence(question);
            }
            return "";
        }

        ///// <summary>
        ///// 正常模式的回复
        ///// 正常模式不会进行随机拼句回复，而是尽量爬取网上的有用信息来回应
        ///// </summary>
        ///// <param name="user"></param>
        ///// <param name="question"></param>
        ///// <returns></returns>
        //string getAnswerNormal(long user, string question)
        //{
        //    string answer = "";
        //    string msg = "";
        //    // 知识图谱功能
        //    var kganswer = baidu.getKGAnswer(question);
        //    if (kganswer.Length > 0)
        //    {
        //        kganswer = kganswer + modes.getMotionString();
        //        return kganswer;
        //    }

        //    msg = baidu.getZhidaoAnswer(question);
        //    if (msg.Length <= 0)
        //    {
        //        try
        //        {
        //            var tiebares = baidu.getBaiduTiebaAnswers(question);
        //            if (tiebares.Length > 0)
        //            {
        //                string tiebaanswer = tiebares[modes.rand.Next(0, tiebares.Length)].Trim();
        //                msg = tiebaanswer;
        //                //sendPrivate(masterQQ, question + "\r\n\r\n" + tiebaanswer);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            FileIOActor.log(ex);
        //        }
        //    }
        //    if (msg.Length <= 0)
        //    {
        //        msg = modes.getSaoHua();
        //    }

        //    return msg;
        //}

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
                FileHelper.Log(ex.Message + "\r\n" + ex.StackTrace);
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
                FileHelper.Log(ex.Message + "\r\n" + ex.StackTrace);
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
                FileHelper.Log(ex.Message + "\r\n" + ex.StackTrace);
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
                FileHelper.Log(ex.Message + "\r\n" + ex.StackTrace);
            }


            return result;
        }

        public string getQianze(string mine, string character, string action)
        {
            string result = "";

            try
            {
                if (!string.IsNullOrWhiteSpace(mine) && !string.IsNullOrWhiteSpace(character) && !string.IsNullOrWhiteSpace(action) && qianze1.Count > 0 && qianze2.Count > 0)
                {
                    // begin
                    result += $"记者：{character}{action}，{mine}对此有何回应？\r\n";
                    result += $"发言人：";
                    // #1
                    result += $"{qianze1[rand.Next(qianze1.Count)]}";
                    // #2
                    List<int> indexs = new List<int>();
                    for (int i = 0; i < qianze2.Count; i++) indexs.Add(i);
                    for (int i = 0; i < rand.Next(3, 6); i++)
                    {
                        int get = rand.Next(indexs.Count);
                        result += $"{qianze2[indexs[get]]}";
                        indexs.RemoveAt(get);
                    }
                    result = result.Replace("#M", mine).Replace("#N", character).Replace("#B", action);
                }
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex.Message + "\r\n" + ex.StackTrace);
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


        public string getZYJ(string sin)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("云杰说道：");
            int maxlong = 170;
            int snum = rand.Next(1, 7);
            for (int i = 0; i < snum; i++)
            {
                string tmp = yjsd[rand.Next(yjsd.Count)];
                sb.Append(tmp);
                if (sb.Length > maxlong) break;
            }

            return sb.ToString().Trim();
        }

        //public string getCangtou(string target)
        //{
        //    string res = "";
        //    if (target.Length <= 0) return "";
        //    try
        //    {
        //        if (target.Length > 50)
        //        {
        //            target = target.Substring(0, 50);
        //        }
        //        string ct5 = "";
        //        string ct7 = "";
        //        foreach (var ch in target)
        //        {
        //            if (cangtou5.ContainsKey(ch))
        //            {
        //                ct5 += $"{cangtou5[ch][rand.Next(cangtou5[ch].Count)]}\r\n";
        //            }
        //            else if (py.ContainsKey(ch))
        //            {
        //                bool find = false;
        //                foreach (var p in py[ch])
        //                {
        //                    if (cangtou5py.ContainsKey(p))
        //                    {
        //                        ct5 += $"{cangtou5py[p][rand.Next(cangtou5py[p].Count)]}\r\n";
        //                        find = true;
        //                        break;
        //                    }
        //                }
        //                if (!find)
        //                {
        //                    ct5 = "";
        //                    //res += $"(5)not found:{ch}\r\n";
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                ct5 = "";
        //                //res += $"(5)not found:{ch}\r\n";
        //                break;
        //            }
        //        }

        //        foreach (var ch in target)
        //        {
        //            if (cangtou7.ContainsKey(ch))
        //            {
        //                ct7 += $"{cangtou7[ch][rand.Next(cangtou7[ch].Count)]}\r\n";
        //            }
        //            else if (py.ContainsKey(ch))
        //            {
        //                bool find = false;
        //                foreach (var p in py[ch])
        //                {
        //                    if (cangtou7py.ContainsKey(p))
        //                    {
        //                        ct7 += $"{cangtou7py[p][rand.Next(cangtou7py[p].Count)]}\r\n";
        //                        find = true;
        //                        break;
        //                    }
        //                }
        //                if (!find)
        //                {
        //                    ct7 = "";
        //                    // res += $"(7)not found:{ch}\r\n";
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                ct7 = "";
        //                res += $"(7)not found:{ch}\r\n";
        //                break;
        //            }
        //        }
        //        if (ct7.Length > 0 && ct5.Length > 0) res = "\r\n" + (rand.Next(2) > 0 ? ct7 : ct5);
        //        else if (ct7.Length > 0) res = "\r\n" + ct7;
        //        else if (ct5.Length > 0) res = "\r\n" + ct5;
        //        else
        //        {
        //            // notfind
        //            res = "我做不到。我紫菜";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FileHelper.log(ex.Message + "\r\n" + ex.StackTrace);
        //        res = "我做不到。我紫菜";
        //    }
        //    return res;

        //}
        //public string getCangwei(string target)
        //{
        //    string res = "";

        //    if (target.Length <= 0) return "";
        //    try
        //    {
        //        if (target.Length > 50)
        //        {
        //            target = target.Substring(0, 50);
        //        }
        //        string ct5 = "";
        //        string ct7 = "";
        //        foreach (var ch in target)
        //        {
        //            if (cangwei5.ContainsKey(ch))
        //            {
        //                ct5 += $"{cangwei5[ch][rand.Next(cangwei5[ch].Count)]}\r\n";
        //            }
        //            else if (py.ContainsKey(ch))
        //            {
        //                bool find = false;
        //                foreach (var p in py[ch])
        //                {
        //                    if (cangwei5py.ContainsKey(p))
        //                    {
        //                        ct5 += $"{cangwei5py[p][rand.Next(cangwei5py[p].Count)]}\r\n";
        //                        find = true;
        //                        break;
        //                    }
        //                }
        //                if (!find)
        //                {
        //                    ct5 = "";
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                ct5 = "";
        //                break;
        //            }
        //        }

        //        foreach (var ch in target)
        //        {
        //            if (cangwei7.ContainsKey(ch))
        //            {
        //                ct7 += $"{cangwei7[ch][rand.Next(cangwei7[ch].Count)]}\r\n";
        //            }
        //            else if (py.ContainsKey(ch))
        //            {
        //                bool find = false;
        //                foreach (var p in py[ch])
        //                {
        //                    if (cangwei7py.ContainsKey(p))
        //                    {
        //                        ct7 += $"{cangwei7py[p][rand.Next(cangwei7py[p].Count)]}\r\n";
        //                        find = true;
        //                        break;
        //                    }
        //                }
        //                if (!find)
        //                {
        //                    ct7 = "";
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                ct7 = "";
        //                break;
        //            }
        //        }
        //        if (ct7.Length > 0 && ct5.Length > 0) res = "\r\n" + (rand.Next(2) > 0 ? ct7 : ct5);
        //        else if (ct7.Length > 0) res = "\r\n" + ct7;
        //        else if (ct5.Length > 0) res = "\r\n" + ct5;
        //        else
        //        {
        //            // notfind
        //            res = "我做不到。我紫菜";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FileHelper.log(ex.Message + "\r\n" + ex.StackTrace);
        //        res = "我做不到。我紫菜";
        //    }
        //    return res;



        //}

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

        public string getPen(long group, long user)
        {
            try
            {
                int num = rand.Next(2, 10);
                while (num-- > 0)
                {
                    Message msg = new Message();
                    msg.to = user;
                    msg.toGroup = group;
                    msg.str = penlist[rand.Next(penlist.Count)].Trim();
                    BOT.sendBack(msg, true);
                }
                return "";
            }
            catch (Exception e)
            {
                FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
                return "";
            }
        }

        public string getHistoryReact(long group, long userqq)
        {
            string result = "";

            string historyPath = BOT.GetHistoryManager().path + "/group/";
            var files = Directory.GetFiles(historyPath, "*.txt");
            int maxtime = 10;
            try
            {
                if (files.Length <= 0) return "1";
                while (maxtime-- > 0)
                {
                    int findex = rand.Next(files.Length);
                    string[] lines = FileHelper.readLines(files[findex]).ToArray();
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
                                string ban = "2715126750 2045098852 188618935 2854196310 287859992 2963959417";
                                if (ban.Contains(items[1])) continue;
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
                                //msg = Regex.Replace(msg, "\\[CQ\\:image[^\\]]+\\]", "");
                                Message msge = new Message();
                                msge.to = -1;
                                msge.toGroup = group;
                                msge.str = msg;
                                BOT.send(msge);
                                find = true;
                            }
                        }
                        catch (Exception e)
                        {
                            FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
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
                FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
            }


            return "2";
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

        //public string getSZM(string question)
        //{

        //        string ori = question.Trim();
        //        string res = "";
        //        try
        //        {
        //            int index = 0;
        //            while (true)
        //            {
        //                bool success = false;
        //                for (int i = Math.Min(10, ori.Length - index); i > 0; i--)
        //                {
        //                    if (szm.ContainsKey(ori.Substring(index, i)))
        //                    {
        //                        // success
        //                        res += szm[ori.Substring(index, i)][rand.Next(szm[ori.Substring(index, i)].Count)];
        //                        index += i;
        //                        success = true;
        //                        break;
        //                    }
        //                }
        //                if (index >= ori.Length) break;
        //                if (!success)
        //                {
        //                    res += ori[index];
        //                    index += 1;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            FileHelper.log(ex);
        //        }
        //        return res;

        //}


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
                FileHelper.Log(e.Message + "\r\n" + e.StackTrace);
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

        public override void Dispose()
        {

        }
    }


    class ModeInfo
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
