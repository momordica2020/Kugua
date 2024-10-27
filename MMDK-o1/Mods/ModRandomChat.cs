using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MMDK.Util;
using SuperSocket.ClientEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MMDK.Mods
{
    /// <summary>
    /// 随机回复，龚诗似的
    /// </summary>
    public class ModRandomChat : Mod
    {
        Random rand = new Random();



        public string replacefile = "replacewords.txt";
        string modeIndexName = "_index.txt";
        //string modePrivateName = "_mode_private.txt";
        //string modeGroupName = "_mode_group.txt";
        string defaultAnswerName = "_defaultanswer.txt";
        string sstvName = "sstv.jpg";
        string PluginPath;

        List<string> sstv = new List<string>();
        Dictionary<string, string> wordReplace = new Dictionary<string, string>();
        Dictionary<string, ModeInfo> modedict = new Dictionary<string, ModeInfo>();
        List<string> defaultAnswers = new List<string>();
        //public Dictionary<long, string> privatemode = new Dictionary<long, string>();
        //public Dictionary<long, string> groupmode = new Dictionary<long, string>();
        MD5 md5 = MD5.Create();

        string chaosv = "混沌-名词.txt";
        string chaosm = "混沌-情绪词.txt";
        string chaosw = "混沌-小万邦部分.txt";
        List<string[]> chaosWord = new List<string[]>();
        List<string> chaosMotion = new List<string>();
        List<string> chaosXwb = new List<string>();

        string yunjief = "云杰说道.txt";
        List<string> yjsd = new List<string>();



        string penName = "pen.txt";
        List<string> penlist = new List<string>();






        string picsave = "picsave.txt";
        string piclingtang = "lingtang.jpg";
        List<string> pics = new List<string>();



        public bool Init(string[] args)
        {
            try
            {

                rand = new Random();

                string PluginPath = Config.Instance.ResourceFullPath("ModePath");


                // pen
                var penlist = FileManager.readLines($"{PluginPath}/{penName}").ToList();










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
                List<string> modelines = FileManager.readLines($"{PluginPath}/{modeIndexName}").ToList();
                foreach (var line in modelines)
                {
                    var items = line.Split('\t');
                    string modeName = items[0].Trim();
                    try
                    {
                        string[] modeConfigs;
                        if (items.Length >= 2)
                        {
                            modeConfigs = items[1].Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);

                        }
                        else
                        {
                            modeConfigs = new string[1] { "默认" };
                        }
                        modedict[modeName] = new ModeInfo(modeName, modeConfigs, FileManager.readLines($"{PluginPath}/{modeName}.txt").ToList());
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Log($"模式行[ {line} ]加载失败，{ex.Message}\r\n{ex.StackTrace}");
                    }
                }
                modedict["测试"] = new ModeInfo { name = "测试", config = { "默认" } };

                // replace
                wordReplace = new Dictionary<string, string>();
                var lines = FileManager.readLines($"{PluginPath}/{replacefile}");
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
                chaosMotion = FileManager.readLines($"{PluginPath}/{chaosm}").ToList();
                // verb
                var wordlines = FileManager.readLines($"{PluginPath}/{chaosv}").ToList();
                foreach (var line in wordlines)
                {
                    chaosWord.Add(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                // xwb
                chaosXwb = FileManager.readLines($"{PluginPath}/{chaosw}").ToList();

                // yunjieshuodao
                yjsd = FileManager.readLines($"{PluginPath}/{yunjief}").ToList();

                // random


                // default
                defaultAnswers = FileManager.readLines($"{PluginPath}/{defaultAnswerName}").ToList();

                // sstv
                sstv = FileManager.readLines($"{PluginPath}/{sstvName}").ToList();

                // pics
                pics = FileManager.readLines($"{PluginPath}/{picsave}").ToList();


                //new Thread(workInitModes).Start();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e.Message + "\r\n" + e.StackTrace);
            }


            return true;
        }

        public void Exit()
        {
            try
            {
                FileManager.writeLines($"{PluginPath}/{picsave}", pics);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }


        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            message = message.Trim();

            


            if (message.StartsWith("模式列表"))
            {
                string modeindexs = printModeList();
                modeindexs += "~输入“xx模式on”即可切换模式~";

                results.Add(modeindexs);
                return true;
            }

            bool isGroup = groupId > 0;
            var user = Config.Instance.GetPlayerInfo(userId);
            var group = Config.Instance.GetGroupInfo(groupId);
            Regex modereg = new Regex("(\\S+)模式\\s*(on)", RegexOptions.IgnoreCase);
            var moderes = modereg.Match(message);
            string mode = "";
            if (moderes.Success)
            {
                try
                {
                    mode = moderes.Groups[1].ToString();
                    bool chooseResult = true;
                    if (string.IsNullOrWhiteSpace(mode)) chooseResult = false;
                    else if (!modePublic(mode))
                    {
                        if (isGroup && (!GroupHasAdminAuthority(groupId)))chooseResult = false;
                        if (!isGroup && (!UserHasAdminAuthority(groupId))) chooseResult = false;
                    }


                    if (chooseResult)
                    {
                        if (groupId > 0)
                        {
                            // group
                            GroupClearAndRefreshChatTag(group, mode);
                        }
                        else
                        {
                            // private
                            UserClearAndRefreshChatTag(user, mode);
                        }
                        results.Add($"~{Config.Instance.App.Avatar.askName}的{mode}模式启动~");
                    }
                    else
                    {
                        results.Add($"~我还没有这个模式~");
                        results.Add(printModeList());
                        results.Add($"~输入“xx模式on”即可切换模式~");
                        return true;
                    }
                    

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            string chatModeName = "";
            if (!isGroup)
            {
                // 私聊发言
                chatModeName = GetChatModeName(user.Tag);
            }
            else
            {
                // 群内
                chatModeName = GetChatModeName(group.Tag);
            }
            string answer = "";
            
            switch (chatModeName)
            {
                case "正常":
                case "混沌": answer += getAnswerChaos(userId, message); 
                    break;
                case "小万邦": answer += getGong(); 
                    break;



                case "喷人": 
                    results.AddRange(getPen(groupId, userId)); 
                    return true;
                case "测试": 
                    results.AddRange(getHistoryReact(groupId, userId)); 
                    return true;



                default: 
                    answer += getAnswerWithMode(userId, message, chatModeName); 
                    break;
            }
            if (!string.IsNullOrWhiteSpace(answer))
            {
                results.Add(answer);
                return true;
            }
            return false;
        }







        private bool UserBanned(long userId)
        {
            var user = Config.Instance.GetPlayerInfo(userId);
            if (user.Is("黑名单")) return true;
            if (user.Type == PlayerType.Blacklist) return true;
            return false;

        }

        private bool GroupBanned(long groupId)
        {
            var group = Config.Instance.GetGroupInfo(groupId);
            if (group.Is("黑名单")) return true;
            if (group.Type == PlaygroupType.Blacklist) return true;
            return false;

        }


        string GetChatModeName(string tagString)
        {
            if(string.IsNullOrWhiteSpace(tagString)) return null;
            var tags=tagString.Split(',',StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                if (modedict.TryGetValue(tag.Replace("模式", "").Trim(), out var mode))
                {

                    return mode.name;
                }
            }
            
            return "";
        }

        IEnumerable<string> GetAllChatTags()
        {
            List<string> tags = new List<string>();
            foreach (var key in modedict.Keys)
            {
                tags.Add($"{key}模式");
            }

            return tags;
        }


        void GroupClearAndRefreshChatTag(Playgroup group, string newTag)
        {
            foreach (var tag in GetAllChatTags())
            {
                group.DeleteTag(tag);
            }
            string tagName = $"{newTag.Replace("模式", "").Trim()}模式";
            group.SetTag(tagName);
        }

        void UserClearAndRefreshChatTag(Player user, string newTag)
        {
            foreach (var tag in GetAllChatTags())
            {
                user.DeleteTag(tag);
            }
            string tagName = $"{newTag.Replace("模式", "").Trim()}模式";
            user.SetTag(tagName);
        }

        bool UserHasAdminAuthority(long userId)
        {
            if (userId <= 0) return false;
            if (userId == Config.Instance.App.Avatar.adminQQ) return true;
            var user = Config.Instance.GetPlayerInfo(userId);
            if (user.Is("管理员")) return true;
            if (user.Type == PlayerType.Admin) return true;
            return false;
        }

        bool GroupHasAdminAuthority(long groupId)
        {
            if (groupId <= 0) return false;
            if (groupId == Config.Instance.App.Avatar.adminGroup) return true;
            var group = Config.Instance.GetGroupInfo(groupId);
            if (group.Is("测试")) return true;
            if (group.Type == PlaygroupType.Test) return true;
            return false;
        }



        public bool modePublic(string modeName)
        {
            if (string.IsNullOrWhiteSpace(modeName)) return false;
            if (modeExist(modeName) && !modedict[modeName].config.Contains("隐藏")) return true;
            else return false;
        }

        public bool modeExist(string modeName)
        {
            if (string.IsNullOrWhiteSpace(modeName) || !modedict.ContainsKey(modeName))
            {
                // mode not exist!
                //Logger.Instance.Log("mode " + modeName + " not exist.");
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









        public IEnumerable<string> getPen(long group, long user)
        {
            try
            {
                int num = rand.Next(2, 10);
                List<string> res = new List<string>();
                while (num-- > 0)
                {
                    res.Add(penlist[rand.Next(penlist.Count)].Trim());
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
                    string[] lines = FileManager.readLines(files[findex]).ToArray();
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
}
