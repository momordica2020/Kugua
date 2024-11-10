using MeowMiraiLib;
using MeowMiraiLib.Msg;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using MMDK.Util;
using MMDK_api.Mods;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace MMDK.Mods
{

    /// <summary>
    /// 赛马模块
    /// </summary>
    class ModRaceHorse : Mod, ModWithMirai
    {
        private static readonly Lazy<ModRaceHorse> instance = new Lazy<ModRaceHorse>(() => new ModRaceHorse());
        public static ModRaceHorse Instance => instance.Value;
        private ModRaceHorse()
        {


        }




        //string matchPath = "DataRacehorse/match/";
        object matchMutex = new object();


        public Dictionary<long, Client> clients = new Dictionary<long, Client>();
        public Dictionary<long, RHUser> users = new Dictionary<long, RHUser>();
        public Dictionary<string, RHHorse> horses = new Dictionary<string, RHHorse>();
        public Dictionary<long, RHMatch> matches = new Dictionary<long, RHMatch>();

        public TimeSpan raceBegin = new TimeSpan(21, 0, 0);
        public TimeSpan raceEnd = new TimeSpan(23, 0, 0);






        public bool Init(string[] args)
        {
            lock (matchMutex)
            {
                try
                {
                    var lines = FileManager.ReadResourceLines("RaceUser");
                    foreach (var line in lines)
                    {
                        RHUser user = new RHUser();
                        user.parse(line);
                        users[user.id] = user;
                    }


                    lines = FileManager.ReadResourceLines("RaceHorse");
                    foreach (var line in lines)
                    {
                        RHHorse horse = new RHHorse(line);
                        horses[horse.name] = horse;
                    }





                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }
            return true;
        }

        public void Exit()
        {
            try
            {
                foreach (var match in matches)
                {
                    match.Value.StopRaceLoop();
                }
                save();
            }
            catch { }

        }

        public bool HandleText(long userId, long groupId, string cmd, List<string> results)
        {
            try
            {   
                if (string.IsNullOrWhiteSpace(cmd)) return false;
                var isGroup = groupId > 0;
               
                if (!users.ContainsKey(userId)) users[userId] = new RHUser(userId);
                RHUser user = users[userId];

                //BOT.log("赛马 "+cmd);
                cmd = cmd.Trim();
                var cmdFilter = Regex.Match(cmd, @"^赛马(介绍|玩法)", RegexOptions.Singleline);
                if (cmdFilter.Success)
                {
                    results.Add(getIntroduction());
                    return true;
                }

                cmdFilter = Regex.Match(cmd, @"^个人信息", RegexOptions.Singleline);
                if (cmdFilter.Success)
                {
                    results.Add($"{ModBank.Instance.getUserInfo(userId)}\r\n{getRHInfo(userId)}");
                    return true;
                }


                cmdFilter = Regex.Match(cmd, @"^赛马", RegexOptions.Singleline);
                if (isGroup && cmdFilter.Success)
                {
                    int num = 5;
                    int len = 100;
                    clients[userId] = Target;
                    if (!matches.ContainsKey(groupId)) matches[groupId] = new RHMatch(groupId);
                    matches[groupId].ReStart(num, len);
                    return true;
                }

                cmdFilter = Regex.Match(cmd, @"^胜率榜", RegexOptions.Singleline);
                if (cmdFilter.Success)
                {
                    results.Add($"{showBigWinner()}");
                    return true;
                }

                cmdFilter = Regex.Match(cmd, @"^败率榜", RegexOptions.Singleline);
                if (cmdFilter.Success)
                {
                    results.Add($"{showBigLoser()}");
                    return true;
                }

                cmdFilter = Regex.Match(cmd, @"^赌狗榜", RegexOptions.Singleline);
                if (cmdFilter.Success)
                {
                    results.Add($"{showMostPlayTime()}");
                    return true;
                }

                cmdFilter = Regex.Match(cmd, @"^(\d+)号(\d+)", RegexOptions.Singleline);
                if (isGroup && cmdFilter.Success)
                {
                    try
                    {
                        int roadnum = 0;
                        int money = 0;
                        if (int.TryParse(cmdFilter.Groups[1].Value, out roadnum)
                         && int.TryParse(cmdFilter.Groups[2].Value, out money))
                        {
                            if (matches.TryGetValue(groupId, out var matchInfo))
                            {
                                string result = matchInfo.bet(user, roadnum, money);
                                if (string.IsNullOrWhiteSpace(result)) return false;
                                results.Add(result);
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Log(ex);
                    }
                }

               




                    
                

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }

            return false;
        }



        public async Task<bool> OnFriendMessageReceive(FriendMessageSender s, Message[] e, Client Target)
        {
            return false;
        }

        public async Task<bool> OnGroupMessageReceive(GroupMessageSender s, Message[] e, Client Target)
        {
            return false;
        }

        public void save()
        {
            lock (matchMutex)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var user in users.Values)
                    {
                        sb.Append($"{user.ToString()}\r\n");
                    }
                    FileManager.writeText(Config.Instance.ResourceFullPath("RaceUser"), sb.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }
        }








        public string isAllow(long group)
        {
            DateTime time = DateTime.Now;
            DateTime nightRaceBegin = new DateTime(time.Year, time.Month, time.Day) + raceBegin;
            DateTime nightRaceEnd = new DateTime(time.Year, time.Month, time.Day) + raceEnd;

            if (time >= nightRaceBegin && time <= nightRaceEnd)
            {
                return "";
            }
            string res = $"夜间赛事起止时间为{nightRaceBegin.ToString("HH:mm")}-{nightRaceEnd.ToString("HH:mm")}";
            return res;
        }



        public string showBigWinner()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    if (left.getWinPercent() < right.getWinPercent())
                        return 1;
                    else if (left.getWinPercent() == right.getWinPercent())
                    {
                        if (left.getPlayTime() < right.getPlayTime()) return 1;
                        else if (left.getPlayTime() > right.getPlayTime()) return -1;
                        else return 0;
                    }
                    else
                        return -1;
                });
                sb.Append("赛 🐎 胜 率 榜 \r\n");
                int showtime = 0;
                int index = 0;
                int maxnum = 10;
                ulong mintime = 5;
                while (showtime < maxnum && index < users.Count)
                {
                    ulong playtime = users[index].wintime + users[index].losetime;
                    if (playtime > mintime)
                    {
                        sb.Append($"{showtime + 1}:{Config.Instance.UserInfo(users[index].id).Name},{Math.Round(users[index].getWinPercent(), 2)}%({users[index].wintime}/{playtime})\r\n");
                        showtime += 1;
                    }
                    index += 1;
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            return "";
            //save();
        }

        public string showBigLoser()
        {
            try
            {
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    if (left.getLosePercent() < right.getLosePercent())
                        return 1;
                    else if (left.getWinPercent() == right.getWinPercent())
                    {
                        if (left.getPlayTime() < right.getPlayTime()) return 1;
                        else if (left.getPlayTime() > right.getPlayTime()) return -1;
                        else return 0;
                    }
                    else
                        return -1;
                });

                StringBuilder sb = new StringBuilder();
                sb.Append("赛 🐎 败 率 榜 \r\n");
                int showtime = 0;
                int index = 0;
                int maxnum = 10;
                ulong mintime = 5;
                while (showtime < maxnum && index < users.Count)
                {
                    ulong playtime = users[index].wintime + users[index].losetime;
                    if (playtime > mintime)
                    {
                        sb.Append($"{showtime + 1}:{Config.Instance.UserInfo(users[index].id).Name},{Math.Round(users[index].getLosePercent(), 2)}%({users[index].losetime}/{playtime})\r\n");
                        showtime += 1;
                    }
                    index += 1;
                }

                //for (int i = 0; i < Math.Min(users.Count, 10); i++)
                //{
                //    sb.Append($"{i + 1}:{getQQNick(users[i].qq)},{Math.Round(users[i].getLosePercent(), 2)}%({users[i].losetime}/{users[i].wintime + users[i].losetime})\r\n");
                //}
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            return "";

        }


        /// <summary>
        /// 赌狗榜
        /// </summary>
        public string showMostPlayTime()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("赛 🐎 赌 狗 榜 \r\n");

                int maxnum = 10;
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    if (left.getPlayTime() < right.getPlayTime())
                        return 1;
                    else if (left.getPlayTime() == right.getPlayTime())
                        return 0;
                    else
                        return -1;
                });
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{Config.Instance.UserInfo(users[i].id).Name},赌了{users[i].wintime + users[i].losetime}次\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            return "";
        }

        /// <summary>
        /// 个人赌马记录
        /// </summary>
        /// <param name="userqq"></param>
        /// <returns></returns>
        public string getRHInfo(long userqq)
        {
            if (!users.ContainsKey(userqq)) users[userqq] = new RHUser(userqq);
            var u = users[userqq];
            return $"您在赌马上消费过{u.hrmoney}枚{ModBank.unitName}，共下注{u.losetime + u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%";
            //outputMessage(group, userqq, $"您在赌马上消费过{u.hrmoney}枚{BTCActor.unitName}，共下注{u.losetime+u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%");
        }

        public string getIntroduction()
        {
            return $"赛🐎游戏介绍：\r\n" +
                $"输入“赛马”开始一局比赛\r\n" +
                $"在比赛开始时会有下注时间，输入x号y可以向x号马下注y元\r\n" +
                $"比赛开始后自动演算，比赛期间不接收指令\r\n" +
                $"其他指令包括“签到”“个人信息”“富豪榜”“穷人榜”“胜率榜”“败率榜”“赌狗榜”";
        }

        //public RHUser getUser(long id)
        //{
        //    return users[id];
        //}



        public List<RHHorse> getHorseInfos()
        {
            return horses.Values.ToList();
        }

        


        internal void showMessage(long groupId, int userId, string s)
        {
            new GroupMessage(groupId, [
                new Plain(s)
                ]).Send(client);
        }
    }


}
