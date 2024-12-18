

using System.Text.RegularExpressions;

using static Kugua.ModRoulette;
using System.Text;
using Kugua.Integrations.NTBot;
using System.Numerics;


namespace Kugua
{
    /// <summary>
    /// 老虎机模块
    /// </summary>
    public class ModSlotMachine : Mod
    {
        public Dictionary<string, byte[]> emojis = new Dictionary<string, byte[]>();
        public Dictionary<string, object> playerLock = new Dictionary<string, object>();
        public Dictionary<string, GamePlayerHistory> history = new Dictionary<string, GamePlayerHistory>();

        private static readonly Lazy<ModSlotMachine> instance = new Lazy<ModSlotMachine>(() => new ModSlotMachine());
        public static ModSlotMachine Instance => instance.Value;
        private ModSlotMachine()
        {


        }
        public override bool Init(string[] args)
        {
            try
            {
                var lines = LocalStorage.ReadResourceLines("game/slot_user.txt");
                foreach (var line in lines)
                {
                    GamePlayerHistory user = new GamePlayerHistory();
                    user.Init(line);
                    history[user.id] = user;
                }


                var emojiss = Directory.GetFiles($"{Config.Instance.ResourceRootPath}{Path.DirectorySeparatorChar}game{Path.DirectorySeparatorChar}emojis", "*.png");
                foreach (var f in emojiss)
                {
                    byte[] pngBytes = System.IO.File.ReadAllBytes(f);
                    emojis[f] = pngBytes;
                }



                ModCommands.Add(new ModCommand(new Regex(@"^老虎(.+)?"),StartSlotGame));

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return true;
        }
        public override void Save()
        {

            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var user in history.Values)
                {
                    sb.Append($"{user.ToString()}\r\n");
                }
                LocalStorage.writeText(Config.Instance.ResourceFullPath("game/slot_user.txt"), sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

        }

        public string UserHistory(string id)
        {
            if (history.ContainsKey(id))
            {
                var h = history[id];
                return $"玩老虎{h.playnum}次，共下{h.money.ToHans()}，胜率{h.winnum}-{h.losenum}({h.winP}%)";
            }
            return "没有老虎游戏记录";
        }

        /// <summary>
        /// 玩🎰
        /// 老虎1000
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string StartSlotGame(MessageContext context, string[] param)
        {
            BigInteger money = 1;
            if (param.Length < 2) money = 1;
            money = StaticUtil.ConvertToBigInteger(param[1]);

            var tuser = Config.Instance.UserInfo(context.userId);
            if (tuser.Money < money)
            {
                // money not enough.
                return ($"{ModBank.unitName}不够，余额：{tuser.Money.ToHans()}");
            }


            if (!playerLock.ContainsKey(context.userId)) playerLock[context.userId] = new object();
            lock (playerLock[context.userId])
            {
                try
                {
                    if (ModBank.Instance.TransMoney(context.userId, Config.Instance.BotQQ, money, out string msg) != money)
                    {
                        return ($"{msg}");
                    }
                    BigInteger userNowMoney = tuser.Money;    // 防止多线程时候，后面钱数显示bug



                    // 节省资源，不做gif生成了。但是及算方式依然是一样的
                    var emojis = RollSymPlay.GenerateEmoji(out var rollres);
                    //var gifBase64 = RollSymPlay.GenerateGif(out var rollres);




                    int score = 0;
                    //Logger.Log($"!{rollres.Count}");
                    if (rollres.Count == 9)
                    {
                        for (int i = 1; i < 2; i++)
                        {
                            //Logger.Log($"!{rollres[i][0]} {rollres[i+3][0]} {rollres[i+6][0]}");
                            if (rollres[i][0] == rollres[i + 3][0] && rollres[i][0] == rollres[i + 6][0])
                            {
                                // 3
                                score += rollres[i + 3][1];
                            }
                            else if (rollres[i][0] == rollres[i + 3][0] || rollres[i + 3][0] == rollres[i + 6][0])
                            {
                                // 2
                                score += rollres[i + 3][2];

                            }
                        }
                        //if (rollres[0][0] == rollres[4][0] && rollres[4][0] == rollres[8][0])
                        //{
                        //    // 3
                        //    score += rollres[0][1];
                        //}
                        //else if (rollres[0][0] == rollres[4][0] || rollres[4][0] == rollres[8][0])
                        //{
                        //        // 2
                        //        //score += rollres[4][2];
                        //}
                        //if (rollres[2][0] == rollres[4][0] && rollres[4][0] == rollres[6][0])
                        //{
                        //    // 3
                        //    score += rollres[2][1];
                        //}
                        //else if (rollres[2][0] == rollres[4][0] || rollres[4][0] == rollres[6][0])
                        //{
                        //        // 2
                        //        //score += rollres[4][2];
                        //}
                    }



                    if (!history.ContainsKey(context.userId)) history[context.userId] = new GamePlayerHistory();
                    history[context.userId].money += money;
                    history[context.userId].id = context.userId;
                    history[context.userId].playnum++;

                    string resultString = $"你投了{money.ToHans()}{ModBank.unitName}，";
                    if (score <= 0)
                    {
                        resultString += $"很遗憾，没中，余额{userNowMoney.ToHans()}";
                    }
                    else
                    {
                        history[context.userId].winnum++;
                        BigInteger getmoney = money * (score);
                        if (ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, getmoney, out string msg2) != getmoney)
                        {
                            resultString += $"得到{score}分！但是{msg2}，余额{tuser.Money.ToHans()}";
                        }
                        else
                        {
                            resultString += $"得到{score}分！恭喜赚得{getmoney.ToHans()}{ModBank.unitName}，余额{tuser.Money.ToHans()}";
                        }
                    }

                    var sendOutMsg = new Message[]
                    {
                        new At(context.userId),
                        new Text(emojis + "\n" + resultString),
                    };
                    context.SendBack(sendOutMsg);
                    // save();
                }
                catch (Exception e)
                {
                }
            }

            return null;
        }

        public override void Exit()
        {
            Save();
        }


    }

}
