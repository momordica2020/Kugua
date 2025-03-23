
using System.Text.RegularExpressions;

using System.Text;
using NvAPIWrapper.Display;
using System;
using Kugua.Integrations.NTBot;
using System.Numerics;
using Kugua.Core;


namespace Kugua.Mods
{

    /// <summary>
    /// 掷骰子猜大小
    /// </summary>
    public class ModDiceGame : Mod
    {
        public Dictionary<string, object> matchLock = new Dictionary<string, object>();
        public Dictionary<string, GamePlayerHistory> history = new Dictionary<string, GamePlayerHistory>();
        public object matchInfoLock = new object();
        //public string diceGifPath = "game/dice/";

        private static readonly Lazy<ModDiceGame> instance = new Lazy<ModDiceGame>(() => new ModDiceGame());
        public static ModDiceGame Instance => instance.Value;
        public string DataFile = "game/cp_user.txt";
        private ModDiceGame()
        {


        }
        public override bool Init(string[] args)
        {
            try
            {
                var lines = LocalStorage.ReadResourceLines(DataFile);
                foreach (var line in lines)
                {
                    GamePlayerHistory user = new GamePlayerHistory();
                    user.Init(line);
                    history[user.id] = user;
                }




                ModCommands.Add(new ModCommand(new Regex(@"^(.+)押(.+)"), StartDiceGame));

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
                LocalStorage.writeText(Config.Instance.FullPath(DataFile), sb.ToString());
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
                return $"玩大小{h.playnum}次，共下注{h.money.ToHans()}，胜率{h.winnum}-{h.losenum}({h.winP}%)";
            }
            return "没有大小游戏记录";
        }


        /// <summary>
        /// 掷骰子游戏（大小单双123456）
        /// 100押大/20000押6
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string StartDiceGame(MessageContext context, string[] param)
        {
            BigInteger money = 1;
            money = Util.ConvertToBigInteger(param[1]);
            if (money < 1) return "";
            string betDesc = param[2].Trim();

            var tuser = Config.Instance.UserInfo(context.userId);
            if (tuser.Money < money)
            {
                // money not enough.
                return ($"{ModBank.unitName}不够，余额：{tuser.Money.ToHans()}");
            }


            
            try
            {
                if (ModBank.Instance.TransMoney(context.userId, Config.Instance.BotQQ, money, out string msg) != money)
                {
                    return ($"请求失败。{msg}");
                }

                BigInteger winMoney = 0;
                lock (matchInfoLock)
                {
                    int val = context.SendBackDice().Result;

                    //var val = MyRandom.Next(1, 7);// = 1~6
                 
                        var bet = new List<int>();
                    if (betDesc.StartsWith("单"))
                    {
                        bet.AddRange(new int[] { 1, 3, 5 });
                    }
                    else if (betDesc.StartsWith("双"))
                    {
                        bet.AddRange(new int[] { 2, 4, 6 });
                    }
                    else if (betDesc.StartsWith("大"))
                    {
                        bet.AddRange(new int[] { 4, 5, 6 });
                    }
                    else if (betDesc.StartsWith("小"))
                    {
                        bet.AddRange(new int[] { 1, 2, 3 });
                    }
                    else
                    {
                        int nn = 0;
                        int.TryParse(betDesc, out nn);
                        if (nn >= 1 && nn <= 6)
                        {
                            bet.Add(nn);
                        }
                        else
                        {
                            // 识别失败
                            return $"识别失败，请输入单/双/大/小/1/2/3/4/5/6 其中一项喵";
                        }                    
                    }
                    foreach (var bi in bet)
                    {
                        if (bi == val)
                        {
                            BigInteger multi = (bet.Count == 1 ? 5 : 2);
                            winMoney = multi * money;
                        }
                    }
                   // context.SendBack([
                   //     new Image($"file://{Config.Instance.ResourceFullPath(diceGifPath)}{val}.gif")
                   //]);
                }
                if (!history.ContainsKey(context.userId)) history[context.userId] = new GamePlayerHistory();
                history[context.userId].money += money;
                history[context.userId].id = context.userId;
                history[context.userId].playnum++;
                // win

                var result = "";
                if (winMoney > 0)
                {
                    var mres = ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, winMoney, out var errmsg);
                    if (mres != winMoney)
                    {
                        // 转账出错
                        result = $"转币出错，{mres}";
                    }
                    else
                    {
                        result = $"中了！赢得{winMoney.ToHans()}{ModBank.unitName},余额{Config.Instance.UserInfo(context.userId).Money.ToHans()}";
                    }
                    
                    history[context.userId].winnum++;
                    // good.
                }
                else
                {
                    result = $"很遗憾，，，";
                }
               
                context.SendBackText(result, true);
                return null;
                
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            

            return null;
        }

        public override void Exit()
        {
            Save();
        }


    }


}
