﻿
using System.Drawing;
using System.Security.Cryptography;

using System.Text;
using System.Text.RegularExpressions;
using Kugua.Core;
using Kugua.Integrations.AI;
using Kugua.Integrations.NTBot;
using Microsoft.VisualBasic;
using SuperSocket.ClientEngine;


namespace Kugua.Mods
{
    /// <summary>
    /// 随机回复，龚诗似的
    /// </summary>
    public class ModRandomChat : Mod
    {
        //private static readonly Lazy<ModRandomChat> instance = new Lazy<ModRandomChat>(() => new ModRandomChat());
        //public static ModRandomChat Instance => instance.Value;


        string modeIndexName = "_index.txt";

        string PluginPath;

        Dictionary<string, string> wordReplace = new Dictionary<string, string>();
        Dictionary<string, ModeInfo> modedict = new Dictionary<string, ModeInfo>();


        MD5 md5 = MD5.Create();

        List<string> defaultAnswers = new List<string>();
        List<string[]> chaosWord = new List<string[]>();
        List<string> chaosMotion = new List<string>();
        List<string> chaosXwb = new List<string>();
        List<string> bencao = new List<string>();
        List<string> xy_drugs = new List<string>();
        List<string> xy_disease = new List<string>();
        List<string> xy_treatment = new List<string>();
        List<string> penlist = new List<string>();


        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex(@"^模式列表"), printModeList));
                ModCommands.Add(new ModCommand(new Regex(@"^(清空|清除|删除)记忆"), clearMemory));
                ModCommands.Add(new ModCommand(new Regex(@"^prompt=(.*)", RegexOptions.Singleline), setPrompt));




                ModCommands.Add(new ModCommand(new Regex(@"^(\S+)\s*模式\s*(on)", RegexOptions.IgnoreCase), selectMode));


                string PluginPath = Config.Instance.FullPath("ModePath");

                // pen
                penlist = LocalStorage.ReadLines($"{PluginPath}/pen.txt").ToList();


                defaultAnswers = LocalStorage.ReadLines($"{PluginPath}/_defaultanswer.txt").ToList();


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
                // load modes

                modedict = new Dictionary<string, ModeInfo>();
                List<string> modelines = LocalStorage.ReadLines($"{PluginPath}/{modeIndexName}").ToList();
                foreach (var line in modelines)
                {
                    var items = line.Split('\t');
                    string modeName = items[0].Trim();
                    try
                    {
                        string[] modeConfigs;
                        if (items.Length >= 2)
                        {
                            modeConfigs = items[1].Trim().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        }
                        else
                        {
                            modeConfigs = new string[1] { "默认" };
                        }
                        modedict[modeName] = new ModeInfo(modeName, modeConfigs, LocalStorage.ReadLines($"{PluginPath}/{modeName}.txt").ToList());
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"模式行[ {line} ]加载失败，{ex.Message}\r\n{ex.StackTrace}");
                    }
                }
                modedict["测试"] = new ModeInfo { name = "测试", config = { "隐藏" } };
                // modedict["AI"] = new ModeInfo { name = "AI", config = { "隐藏" } };
                modedict["喷人"] = new ModeInfo { name = "喷人", config = { } };
                modedict["语音"] = new ModeInfo { name = "语音", config = { } };
                modedict["本草"] = new ModeInfo { name = "本草", config = { } };
                //modedict["西医"] = new ModeInfo { name = "西医", config = { } };


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
                chaosMotion = LocalStorage.ReadLines($"{PluginPath}/混沌-情绪词.txt").ToList();
                // verb
                var wordlines = LocalStorage.ReadLines($"{PluginPath}/混沌-名词.txt").ToList();
                foreach (var line in wordlines)
                {
                    chaosWord.Add(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }
                // xwb
                chaosXwb = LocalStorage.ReadLines($"{PluginPath}/混沌-小万邦部分.txt").ToList();


                // bencao
                bencao = LocalStorage.ReadLines($"{PluginPath}/本草.csv").ToList();

                // 西医
                xy_disease = LocalStorage.ReadLines($"{PluginPath}/西医病名.txt").ToList();
                xy_drugs = LocalStorage.ReadLines($"{PluginPath}/西医西药.txt").ToList();
                xy_treatment = LocalStorage.ReadLines($"{PluginPath}/西医检查.txt").ToList();


            }
            catch (Exception e)
            {
                Logger.Log(e);
            }


            return true;
        }




        /// <summary>
        /// 设置AI的预设文本
        /// prompt=你是一个猫娘
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string setPrompt(MessageContext context, string[] param)
        {
            string prompt = param[1].Trim();
            LLM.Instance.SetPrompt(context.groupId, context.userId, prompt);
            return $"*以更新AI模式的prompt，并重置了记忆！开始对话吧";
        }

        /// <summary>
        /// 切换到某个模式
        /// 小万邦模式on
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string selectMode(MessageContext context, string[] param)
        {
            try
            {
                string modeName = param[1];
                if (string.IsNullOrWhiteSpace(modeName))
                {
                    // 输入不合法
                    return printModeList(context, param);
                }
                //ModeInfo mode = null;
                if (modedict.TryGetValue(modeName, out ModeInfo mode))
                {
                    // 模式存在
                    if (mode.config.Contains("隐藏"))
                    {
                        // 隐藏模式，且没有相应权限就不启动
                        if (
                             !context.IsAdminGroup

                            //||(!isGroup && !UserHasAdminAuthority(groupId))
                            )
                        {
                            return printModeList(context, param);
                        }
                    }
                    // 切换模式tag
                    if (context.IsGroup)
                    {
                        // group
                        var group = Config.Instance.GroupInfo(context.groupId);
                        if(group != null)
                        {
                            group.Tags.RemoveWhere(t => t.EndsWith("模式"));
                           group.Tags.Add($"{mode.name}模式");
                        }
                    }
                    else
                    {
                        // private
                        var user = Config.Instance.UserInfo(context.userId);
                        if (user != null)
                        {
                            user.Tags.RemoveWhere(t => t.EndsWith("模式"));
                            user.Tags.Add($"{mode.name}模式");
                        }
                    }
                    return $"~{Config.Instance.App.Avatar.askName}的{mode.name}模式启动~";
                }
                else
                {
                    // 没有这个模式
                    return $"~我还没有{modeName}模式~\n{printModeList(context, param)}";
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }


        /// <summary>
        /// 清空AI的历史记忆并重置prompt
        /// 清空记忆/删除记忆/清除记忆
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string clearMemory(MessageContext context, string[] param)
        {
            LLM.Instance.ClearMemory(context.groupId, context.userId);
            LLM.Instance.SaveMemory();
            return $"*以清空AI模式下与你的聊天历史记录，并恢复默认prompt";
        }


        public async override Task<bool> HandleMessagesDIY(MessageContext context)
        {
           // var message = context.recvMessages.ToTextString().Trim();

            // 以下部分无需输入任何内容即可触发！！！！！！！！！！！！！1111111
            if (context.IsGroup && context.Group.Is("少话"))return true;
            if (context.IsPrivate && context.User.Is("少话")) return true;

            ModeInfo modeTrigger = null;
            if (context.IsGroup && context.IsAskme)
            {
                // 群内
                modeTrigger = getGroupMode(context.Group);
            }
            else if(context.IsPrivate)
            {
                // 私聊发言
                modeTrigger = getUserMode(context.User);
            }
            if (modeTrigger == null)
            {
                // 没找到模式
                return false;
            }
            else
            {
                var msg = context.recvMessages.ToTextString().Trim();
                
                if (handleChatResults(modeTrigger, context, out IEnumerable<string> chatResult))
                {
                    if (chatResult.Count() == 1) context.SendBackText(chatResult.First(), true, true);
                    else
                    {
                        foreach (var s in chatResult)
                        {
                            context.SendBackText(s, false, true);
                        }
                    }

                    return true;
                }
            }


            return false;
        }









        /// <summary>
        /// 按模式处理输入并返回结果串
        /// 里面每个模式的返回值都不应为null
        /// </summary>
        /// <param name="mode">模式对象</param>
        /// <param name="context">上下文</param>
        /// <param name="results">输出结果</param>
        /// <returns>若结果不空，返回true，空则返回false</returns>

        bool handleChatResults(ModeInfo mode, MessageContext context, out IEnumerable<string> results)
        {
            List<string> answer = new List<string>();
            try
            {
                string modeName = mode.name;
                switch (modeName)
                {
                    case "混沌":
                        answer.Add(getAnswerChaos(context.recvMessages.ToTextString()));
                        break;
                    case "正常":
                        //string uName = Config.Instance.UserInfo(context.userId).Name;
                        //if (string.IsNullOrWhiteSpace(uName)) uName = "提问者";
                        //GPT.Instance.OllamaReply(context);
                        
                        var res = LLM.Instance.HSChat(context);
                        if (!string.IsNullOrWhiteSpace(res))
                        {
                            context.SendBackText(res, true, true);
                        }
                        else
                        {
                            // next wait for image input
                            WaitNext(context, new ModCommand(null, descImage, _needAsk: false, _useImage: true));
                        }
                        break;

                    case "小万邦":
                        answer.Add(getGong());
                        break;
                    case "本草":
                        answer.Add(bencao[MyRandom.Next(bencao.Count)]);
                        break;
                    //case "西医":
                    //    answer.Add(getXiyi());
                    //    break;

                    case "喷人":
                        answer.AddRange(getPen());
                        break;
                    case "测试":
                        answer.AddRange(getHistoryReact(context));
                        break;




                    case "语音":
                        // string gong = getGong();
                        var r = getHistoryReact(context, false);
                        string sendString = "";
                        foreach (var rs in r)
                        {
                            sendString += rs + "。";
                            if (sendString.Length > 50)
                            {
                                LLM.Instance.Talk(context, sendString);
                                sendString = "";
                            }

                        }
                        if (sendString.Length > 0) LLM.Instance.Talk(context, sendString);
                        break;
                    default:
                        answer.Add(mode.getRandomSentence());
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }


            results = answer.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            return results.Count() > 0;
        }

        private string descImage(MessageContext context, string[] param)
        {
            string res = "";

            if (context.IsImage)
            {
                res = LLM.Instance.HSGetImgDesc(context.PNG1Base64, "详细描述此图，并解释其深层含义和意图", "png");
            }
            if (!string.IsNullOrWhiteSpace(res))
            {
                return res;
            }
            else
            {
                return null;
            }

        }

        private ModeInfo getUserMode(Player player)
        {
            if (player==null || player.Tags == null) return null;
            foreach (var tag in player.Tags)
            {
                if (tag.EndsWith("模式"))
                {
                    string findName = tag.Substring(0, tag.Length - 2);
                    if (modedict.TryGetValue(findName, out ModeInfo mode))
                    {
                        return mode;
                    }
                }
            }
            return null;
        }

        private ModeInfo getGroupMode(Playgroup group)
        {
            if (group == null || group.Tags != null)
            {
                foreach (var tag in group.Tags)
                {
                    if (tag.EndsWith("模式"))
                    {
                        string findName = tag.Substring(0, tag.Length - 2);
                        ModeInfo mode = null;
                        if (modedict.TryGetValue(findName, out mode))
                        {
                            return mode;
                        }
                    }
                }

            }
            if (modedict.TryGetValue("正常", out var val)) return val;
            return null;
        }


        //string GetChatModeName(string tagString)
        //{
        //    if(string.IsNullOrWhiteSpace(tagString)) return null;
        //    var tags=tagString.Split(',',StringSplitOptions.RemoveEmptyEntries);
        //    foreach (var tag in tags)
        //    {
        //        if (modedict.TryGetValue(tag.Replace("模式", "").Trim(), out var mode))
        //        {

        //            return mode.name;
        //        }
        //    }

        //    return "";
        //}

        //IEnumerable<string> GetAllChatTags()
        //{
        //    List<string> tags = new List<string>();
        //    foreach (var key in modedict.Keys)
        //    {
        //        tags.Add($"{key}模式");
        //    }

        //    return tags;
        //}


        //void GroupClearAndRefreshChatTag(Playgroup group, string newTag)
        //{
        //    foreach (var tag in GetAllChatTags())
        //    {
        //        group.DeleteTag(tag);
        //    }
        //    string tagName = $"{newTag.Replace("模式", "").Trim()}模式";
        //    group.SetTag(tagName);
        //}

        //void UserClearAndRefreshChatTag(Player user, string newTag)
        //{
        //    foreach (var tag in GetAllChatTags())
        //    {
        //        user.DeleteTag(tag);
        //    }
        //    string tagName = $"{newTag.Replace("模式", "").Trim()}模式";
        //    user.SetTag(tagName);
        //}

        //bool UserHasAdminAuthority(string userId)
        //{
        //    if (string.IsNullOrWhiteSpace(userId)) return false;
        //    if (userId == Config.Instance.App.Avatar.adminQQ) return true;
        //    var user = Config.Instance.UserInfo(userId);
        //    if (user.Is("管理员")) return true;
        //    if (user.Type == PlayerType.Admin) return true;
        //    return false;
        //}

        //bool GroupHasAdminAuthority(string groupId)
        //{
        //    if (string.IsNullOrWhiteSpace(groupId)) return false;
        //    if (groupId == Config.Instance.App.Avatar.adminGroup) return true;
        //    var group = Config.Instance.GroupInfo(groupId);
        //    if (group.Is("测试")) return true;
        //    if (group.Type == PlaygroupType.Test) return true;
        //    return false;
        //}




        /// <summary>
        /// 查看bot支持的闲聊模式
        /// 模式列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string printModeList(MessageContext context, string[] param)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"模式列表：\r\n");
            foreach (var mode in modedict)
            {
                if (!mode.Value.config.Contains("隐藏"))
                {
                    sb.Append($"{mode.Key} | ");
                }

            }
            sb.Append($"~输入“xx模式on”即可切换模式~");
            return sb.ToString();
        }



        public string getXiyi()
        {

            var disease = "";
            var drug = "";
            var treatment = "";

            int num = MyRandom.Next(1, 4);
            for (int i = 0; i < num; i++) disease += xy_disease[MyRandom.Next(xy_disease.Count)] + "，";
            num = MyRandom.Next(1, 4);
            for (int i = 0; i < num; i++) drug += xy_drugs[MyRandom.Next(xy_drugs.Count)] + "，";
            num = MyRandom.Next(1, 4);
            for (int i = 0; i < num; i++) treatment += xy_treatment[MyRandom.Next(xy_treatment.Count)] + "，";

            return $"查了下{treatment}怀疑你得了{disease}建议吃{drug}祝你早日康复！";
        }

        /// <summary>
        /// 龚诗 bot 特有的模拟
        /// </summary>
        /// <returns></returns>
        public string getGong()
        {
            StringBuilder sb = new StringBuilder();

            int snum = MyRandom.Next(1, 5);
            for (int i = 0; i < snum; i++)
            {
                int wnum = MyRandom.Next(1, 5);
                int nowlen = 0;
                for (int j = 0; j < wnum; j++)
                {
                    string s = chaosXwb[MyRandom.Next(chaosXwb.Count)];
                    sb.Append(s);
                    nowlen += s.Length;
                    if (nowlen > 15) break;
                }
                if (i < snum - 1) sb.Append("，");
                else sb.Append("。？！"[MyRandom.Next(3)]);
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
            int sentences = MyRandom.Next(1, 6);

            for (int i = 0; i < sentences; i++)
            {
                int thislen = MyRandom.Next(0, 11);
                StringBuilder thissentence = new StringBuilder();
                int wordnum = 0;
                while (thissentence.Length < thislen && wordnum < 5)
                {
                    wordnum++;
                    if (MyRandom.Next(0, 100) > 80)
                    {
                        thissentence.Append(chaosWord[MyRandom.Next(0, chaosWord.Count - 1)][0]);
                    }
                    else
                    {
                        thissentence.Append(chaosXwb[MyRandom.Next(0, chaosXwb.Count - 1)]);
                    }
                }
                thissentence.Append(sgn[MyRandom.Next(sgn.Length)]);
                result += thissentence.ToString();
            }

            return result;
        }









        public IEnumerable<string> getPen()
        {
            try
            {
                int num = MyRandom.Next(2, 10);
                List<string> res = new List<string>();
                while (num-- > 0)
                {
                    res.Add(penlist[MyRandom.Next(penlist.Count)].Trim());
                }
                return res;
            }
            catch (Exception e)
            {
                Logger.Log(e);
                return new List<string>();
            }
        }



        /// <summary>
        /// 用群聊记录来随机回应
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> getHistoryReact(MessageContext context, bool sendXML_Json = true)
        {
            List<string> result = new List<string>();
            try
            {
                var files = HistoryManager.GetGroupHistoryFiles();
                int maxtime = 10;

                if (files.Length <= 0) return result;
                while (maxtime-- > 0)
                {
                    int findex = MyRandom.Next(files.Length);
                    string[] lines = LocalStorage.ReadLines(files[findex]).ToArray();
                    if (lines.Length < 100) continue;
                    int begin = MyRandom.Next(lines.Length - 5);
                    int maxnum = MyRandom.Next(1, 5);
                    int num = lines.Length - begin;// MyRandom.Next(10, lines.Length - begin);
                    bool find = false;
                    string targetuser = "";
                    for (int i = 0; i < num; i++)
                    {
                        try
                        {
                            var items = lines[begin + i].Trim().Split('\t');
                            if (items.Length >= 3)
                            {
                                if (items[1] == Config.Instance.BotQQ) continue;
                                if (targetuser.Length > 0 && targetuser != items[1]) continue;
                                targetuser = items[1];
                                string msg = items[2].Trim();



                                // 过滤CQ代码
                                msg = Regex.Replace(msg, "\\[CQ\\:[^\\]]+\\]", "");

                                // json 直接发
                                if (sendXML_Json && msg.Contains("{\"app\""))
                                {
                                    _ = context.SendBack([new JsonData { data = msg }]);
                                    continue;
                                }

                                // xml直接发
                                if (sendXML_Json && msg.Contains("xml"))
                                {
                                    var mth = new Regex(@"<\?xml.*?\?>|<([^>]+)>(.*?)<\/\1>", RegexOptions.Singleline).Match(msg);
                                    if (mth.Success)
                                    {
                                        string xmlstr = mth.Groups[0].Value;
                                        _ = context.SendBack([new XmlData { data = xmlstr }]);
                                        continue;
                                    }

                                }
                                // 过滤呼唤词
                                if (msg.StartsWith(Config.Instance.BotName)) msg = msg.Substring(Config.Instance.BotName.Length);
                                if (msg.StartsWith("苦瓜")) msg = msg.Substring(2);
                                if (msg.StartsWith("小电酱")) msg = msg.Substring(3);
                                if (msg.StartsWith("小崽子")) msg = msg.Substring(3);

                                // 过严格过滤器
                                if (!Filter.Instance.IsPass(msg, FilterType.Strict)) continue;

                                // 转换emoji
                                msg = Util.ConvertEmoji(msg);
                                if (!string.IsNullOrWhiteSpace(msg))
                                {
                                    result.Add(msg);
                                    find = true;
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e);
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
                Logger.Log(e);
            }


            return result;
        }




        /// <summary>
        /// 混沌模式的回复
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        public string getAnswerChaos(string question)
        {
            string answer = "";
            string msg = "";
            if (MyRandom.Next(0, 100) < 85)
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
                if (msg.Length <= 0 || MyRandom.Next(1, 100) < 40)
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
                return defaultAnswers[MyRandom.Next(defaultAnswers.Count)].Trim();
            }
            catch (Exception e)
            {
                Logger.Log(e);
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
            if (MyRandom.Next(0, 100) > 66)
            {
                res = $"({chaosMotion[MyRandom.Next(0, chaosMotion.Count - 1)]})";
            }

            return res;
        }





        class ModeInfo
        {
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
