
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;


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


        public Dictionary<string, RHUser> users = new Dictionary<string, RHUser>();
        public Dictionary<string, RHHorse> horses = new Dictionary<string, RHHorse>();
        public Dictionary<string, RHMatch> matches = new Dictionary<string, RHMatch>();

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
                    ModCommands[new Regex(@"^个人信息$")] = getUserGameInfo;
                    ModCommands[new Regex(@"^赛马$")] = playGame;
                    ModCommands[new Regex(@"^胜率榜$")] = showBigWinner;
                    ModCommands[new Regex(@"^败率榜$")] = showBigLoser;
                    ModCommands[new Regex(@"^赌狗榜$")] = showMostPlayTime;
                    ModCommands[new Regex(@"^^(\d+)\s*号(.+)")] = AddBet;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
            return true;
        }

        private string AddBet(MessageContext context, string[] param)
        {
            int roadnum = 0;
            BigInteger money = StaticUtil.ConvertToBigInteger(param[2].Trim());
            if (int.TryParse(param[1], out roadnum)
             && money > 0)
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
                    Logger.Log(ex);
                }
            }
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
                Logger.Log(ex);
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
                Logger.Log(ex);
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
                sb.Append("赌 狗 榜 \r\n");

                int maxnum = 10;
                var dogs = new Dictionary<string, BigInteger>();
                foreach (var u in users.Values) { if (!dogs.ContainsKey(u.id)) dogs[u.id] = 0; dogs[u.id] += u.wintime + u.losetime; }
                foreach (var u in ModRoulette.Instance.history) { if (!dogs.ContainsKey(u.Key)) dogs[u.Key] = 0; dogs[u.Key] +=u.Value.playnum; }
                foreach (var u in ModDiceGame.Instance.history) { if (!dogs.ContainsKey(u.Key)) dogs[u.Key] = 0; dogs[u.Key] += u.Value.playnum; }
                foreach (var u in ModSlotMachine.Instance.history) { if (!dogs.ContainsKey(u.Key)) dogs[u.Key] = 0; dogs[u.Key] += u.Value.playnum; }
                var v = dogs.Select(d => (d.Key, d.Value)).ToList();
                v.Sort((left, right) =>
                {
                    if (left.Value < right.Value)
                        return 1;
                    else if (left.Value == right.Value)
                        return 0;
                    else
                        return -1;
                });
                for (int i = 0; i < Math.Min(v.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{Config.Instance.UserInfo(v[i].Key).Name},赌了{v[i].Value}次\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

        /// <summary>
        /// 个人游戏记录
        /// </summary>
        /// <param name="userqq"></param>
        /// <returns></returns>
        public string getUserGameInfo(MessageContext context, string[] param)
        {
            
            return ModBank.Instance.getUserInfo(context.userId) + "\n"
                + ModRaceHorse.Instance.UserHistory(context.userId) + "\n"
                + ModRoulette.Instance.UserHistory(context.userId) + "\n"
                + ModSlotMachine.Instance.UserHistory(context.userId) + "\n"
                + ModDiceGame.Instance.UserHistory(context.userId) + "\n";
            //outputMessage(group, userqq, $"您在赌马上消费过{u.hrmoney}枚{BTCActor.unitName}，共下注{u.losetime+u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%");
        }


        /// <summary>
        /// 个人赛马记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string UserHistory(string id)
        {
            if (users.ContainsKey(id))
            {
                var h = users[id];
                return $"玩赛马{h.losetime + h.wintime}次，共下注{h.hrmoney.ToHans()}，胜率{h.wintime}-{h.losetime}({Math.Round(h.getWinPercent(), 2)}%)";
            }
            return "没有赛马游戏记录";
        }


        public string getIntroduction(MessageContext context, string[] param)
        {
            return $"赛🐎游戏介绍：\r\n" +
                $"输入“赛马”开始一局比赛\r\n" +
                $"在比赛开始时会有下注时间，输入“x号y”可以向x号马下注y元\r\n" +
                $"比赛开始后自动演算，比赛期间不接收指令，每个群同时只开一局\r\n" +
                $"胜者获得的收益=[在赌中马上的投注*赔率+（所有人没中的钱数/赌中的总人数）]*95%\r\n" +
                $"其中押1匹，倍率=5，押两匹，倍率=3\r\n" +
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
