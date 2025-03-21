﻿using MeowMiraiLib;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;

using System.Text.RegularExpressions;
using MeowMiraiLib.Msg;

using static MeowMiraiLib.Msg.Sender.GroupMessageSender;
using static Kugua.ModRoulette;
using System.Text;
using NvAPIWrapper.Display;
using System;


namespace Kugua
{
    public class ModDiceGame : Mod
    {
        public Dictionary<long, object> matchLock = new Dictionary<long, object>();
        public Dictionary<long, GamePlayerHistory> history = new Dictionary<long, GamePlayerHistory>();
        public object matchInfoLock = new object();
        public string diceGifPath = "game/dice/";

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




                ModCommands[new Regex(@"^骰子\s*(\d+)\s*押\s*(\S+)")] = StartGame;

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
                LocalStorage.writeText(Config.Instance.ResourceFullPath(DataFile), sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

        }

        public string UserHistory(long id)
        {
            if (history.ContainsKey(id))
            {
                var h = history[id];
                return $"玩猜大小{h.playnum}次，共下注{h.money}，胜率{h.winnum}-{h.losenum}({h.winP}%)";
            }
            return "没有猜大小游戏记录";
        }



        private string StartGame(MessageContext context, string[] param)
        {
            long money = 1;
            long.TryParse(param[1], out money);
            string betDesc = param[2].Trim();

            var tuser = Config.Instance.UserInfo(context.userId);
            if (tuser.Money < money)
            {
                // money not enough.
                return ($"{ModBank.unitName}不够，余额：{tuser.Money}");
            }


            
            try
            {
                if (ModBank.Instance.TransMoney(context.userId, Config.Instance.BotQQ, money, out string msg) != money)
                {
                    return ($"下注失败。{msg}");
                }

                long winMoney = 0;
                lock (matchInfoLock)
                {
                    var val = MyRandom.Next(1, 7);// = 1~6
                 
                        var bet = new List<int>();
                        if (betDesc == "单")
                        {
                            bet.AddRange(new int[] { 1, 3, 5 });
                        }
                        else if (betDesc == "双")
                        {
                            bet.AddRange(new int[] { 2, 4, 6 });
                        }
                        else if (betDesc == "大")
                        {
                            bet.AddRange(new int[] { 4, 5, 6 });
                        }
                        else if (betDesc == "小")
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

                    }
                    foreach (var bi in bet)
                    {
                        if (bi == val)
                        {
                            double multi = (bet.Count == 1 ? 5 : 2);
                            winMoney = (long)(multi * money);
                        }
                    }
                    context.SendBack([
                        new Image(null, null,$"{Config.Instance.ResourceFullPath(diceGifPath)}{val}.gif")
                   ]);
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
                        result = $"转账出错，{mres}";
                    }
                    else
                    {
                        result = $"中了！赢得{winMoney}{ModBank.unitName},余额{Config.Instance.UserInfo(context.userId).Money}";
                    }
                    
                    history[context.userId].winnum++;
                    // good.
                }
                else
                {
                    result = $"很遗憾，，，";
                }
               
                context.SendBackPlain(result, true);
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
