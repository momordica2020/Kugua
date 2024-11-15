using MeowMiraiLib;
using MeowMiraiLib.Msg;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using System.Text;
using System.Text.RegularExpressions;
using static MeowMiraiLib.Msg.Sender.GroupMessageSender;


namespace Kugua
{

    /// <summary>
    /// 赛马模块
    /// </summary>
    class ModRaceHorse : Mod
    {
        private static readonly Lazy<ModRaceHorse> instance = new Lazy<ModRaceHorse>(() => new ModRaceHorse());
        public static ModRaceHorse Instance => instance.Value;
        private ModRaceHorse()
        {


        }




        //string matchPath = "DataRacehorse/match/";
        object matchMutex = new object();


        public Dictionary<long, RHUser> users = new Dictionary<long, RHUser>();
        public Dictionary<string, RHHorse> horses = new Dictionary<string, RHHorse>();
        public Dictionary<long, RHMatch> matches = new Dictionary<long, RHMatch>();

        public TimeSpan raceBegin = new TimeSpan(21, 0, 0);
        public TimeSpan raceEnd = new TimeSpan(23, 0, 0);






        public override bool Init(string[] args)
        {
            lock (matchMutex)
            {
                try
                {
                    var lines = LocalStorage.ReadResourceLines("RaceUser");
                    foreach (var line in lines)
                    {
                        RHUser user = new RHUser();
                        user.parse(line);
                        users[user.id] = user;
                    }


                    lines = LocalStorage.ReadResourceLines("RaceHorse");
                    foreach (var line in lines)
                    {
                        RHHorse horse = new RHHorse(line);
                        horses[horse.name] = horse;
                    }



                    ModCommands[new Regex(@"^赛马(介绍|玩法)$")] = getIntroduction;
                    ModCommands[new Regex(@"^个人信息$")] = getRHInfo;
                    ModCommands[new Regex(@"^赛马$")] = playGame;
                    ModCommands[new Regex(@"^胜率榜$")] = showBigWinner;
                    ModCommands[new Regex(@"^败率榜$")] = showBigLoser;
                    ModCommands[new Regex(@"^赌狗榜$")] = showMostPlayTime;
                    ModCommands[new Regex(@"^^(\d+)\s*号\s*(\d+)")] = AddBet;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }
            return true;
        }

        private string AddBet(MessageContext context, string[] param)
        {
            int roadnum = 0;
            int money = 0;
            if (int.TryParse(param[1], out roadnum)
             && int.TryParse(param[2], out money))
            {
                if (matches.TryGetValue(context.groupId, out var matchInfo))
                {
                    if (!users.ContainsKey(context.userId)) users[context.userId] = new RHUser(context.userId);
                    var u = users[context.userId];
                    string result = matchInfo.bet(u, roadnum, money);
                    if (string.IsNullOrWhiteSpace(result)) return "";
                    context.SendBackPlain(result, true);
                    return null;
                }
            }

            return "";
        }

        private string playGame(MessageContext context, string[] param)
        {
            int num = 5;
            int len = 100;
            //clients[context.userId] = context.client;
            if (!matches.ContainsKey(context.groupId)) matches[context.groupId] = new RHMatch(context.groupId);
            matches[context.groupId].context = context;
            matches[context.groupId].ReStart(num, len);

            // 用null中止后续解析
            return null;
        }

        public override void Exit()
        {
            try
            {
                foreach (var match in matches)
                {
                    match.Value.StopRaceLoop();
                }
                Save();
            }
            catch { }

        }



        public override void Save()
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
                    LocalStorage.writeText(Config.Instance.ResourceFullPath("RaceUser"), sb.ToString());
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



        public string showBigWinner(MessageContext context, string[] param)
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

        public string showBigLoser(MessageContext context, string[] param)
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
        public string showMostPlayTime(MessageContext context, string[] param)
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
        public string getRHInfo(MessageContext context, string[] param)
        {
            if (!users.ContainsKey(context.userId)) users[context.userId] = new RHUser(context.userId);
            var u = users[context.userId];
            return $"{ModBank.Instance.getUserInfo(context.userId)}\n您在赌马上消费过{u.hrmoney}枚{ModBank.unitName}，共下注{u.losetime + u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%";
            //outputMessage(group, userqq, $"您在赌马上消费过{u.hrmoney}枚{BTCActor.unitName}，共下注{u.losetime+u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%");
        }

        public string getIntroduction(MessageContext context, string[] param)
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




        //internal void SendMessageToClient(long groupId, int userId, string s)
        //{
        //    new GroupMessage(groupId, [
        //        new Plain(s)
        //        ]).Send(client);
        //}

    }

}
