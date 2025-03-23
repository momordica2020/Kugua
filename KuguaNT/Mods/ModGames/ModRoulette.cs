

using System.Text.RegularExpressions;
using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components;
using ChatGPT.Net.DTO.ChatGPT;
using static System.Net.WebRequestMethods;
using System.Text;
using System.Numerics;
using Kugua.Core;


namespace Kugua.Mods
{


    /// <summary>
    /// 恶魔轮盘模块
    /// </summary>
    public class ModRoulette : Mod
    {

        public Dictionary<string, RouletteGame> info = new Dictionary<string, RouletteGame>();
        public Dictionary<string, GamePlayerHistory> history = new Dictionary<string, GamePlayerHistory>();


        private static readonly Lazy<ModRoulette> instance = new Lazy<ModRoulette>(() => new ModRoulette());
        public static ModRoulette Instance => instance.Value;
        private ModRoulette()
        {


        }


        public override bool Init(string[] args)
        {
            try
            {
                ModCommands.Add(new ModCommand(new Regex(@"^\s*轮盘介绍\s*"), IntroduceRouletteGame));
                ModCommands.Add(new ModCommand(new Regex(@"^\s*轮盘(.+)"), StartRouletteGame));
                //ModCommands.Add(new ModCommand(new Regex(@"^\s*加入\s*(\d+)"),JoinGame));
                ModCommands.Add(new ModCommand(new Regex(@"^\s*射我"), ShootMe));
                ModCommands.Add(new ModCommand(new Regex(@"^\s*射他"), ShootHim));



                var lines = LocalStorage.ReadResourceLines("game/roulette_user.txt");
                foreach (var line in lines)
                {
                    GamePlayerHistory user = new GamePlayerHistory();
                    user.Init(line);
                    history[user.id] = user;
                }

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
                LocalStorage.writeText(Config.Instance.FullPath("game/roulette_user.txt"), sb.ToString());
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
                return $"玩轮盘{h.playnum}次，共下{h.money.ToHans()}，胜率{h.winnum}-{h.losenum}({h.winP}%)";
            }
            return "没有轮盘游戏记录";
        }

        /// <summary>
        /// 恶魔轮盘游戏介绍’
        /// 轮盘介绍
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string IntroduceRouletteGame(MessageContext context, string[] param)
        {
            var res = $"恶魔轮盘pvp游戏介绍：\r\n";
            res += $"2人参与后自动启动，回合制轮流操作，谁存活到底获得双方投币之和*1.5\r\n";
            res += $"开局会显示本轮弹总数和实/空的数量，装填顺序随机\r\n";
            res += $"输入“轮盘10”投10{ModBank.unitName}加入游戏\r\n";
            res += $"输入“射我”开枪射自己，若是空弹则下一回合还是你操作\r\n";
            res += $"输入“射他”射对手，且下一回合轮到对手操作\r\n";

            return res ;
        }


        /// <summary>
        /// 在本群开始一局轮盘
        /// 轮盘N
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string StartRouletteGame(MessageContext context, string[] param)
        {
            try
            {
                if (!info.ContainsKey(context.groupId))
                {
                    info[context.groupId] = new RouletteGame();
                }
                var g = info[context.groupId];
                BigInteger money = Util.ConvertToBigInteger(param[1]);
                if(money < 0)
                {
                    var res =  g.Start(context.userId, money);
                    context.SendBackText(res, false);
                }
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                
            }

            return null;
        }


        /// <summary>
        /// 射对方
        /// 射他
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string ShootHim(MessageContext context, string[] param)
        {
            if (info.ContainsKey(context.groupId))
            {
                var g = info[context.groupId];
                var res = g.ShootOther(context.userId);
                context.SendBackText(res, false);
            }


            return null;
        }


        /// <summary>
        /// 射自己
        /// 射我
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string ShootMe(MessageContext context, string[] param)
        {
            if (info.ContainsKey(context.groupId))
            {
                var g = info[context.groupId];
                var res = g.ShootSelf(context.userId);
                context.SendBackText(res, false);
            }

            return null;

        }





        // 玩家类
        public class RoulettePlayer
        {
            public string id;
            public string name;
            public int hp;
            public BigInteger money;

            public bool IsAlive => hp > 0;


            public void TakeDamage(int damage)
            {
                hp -= damage;
                if (hp < 0) hp = 0;
            }
        }

        public class RouletteGame
        {
            public RoulettePlayer p1;
            public RoulettePlayer p2;
            
 
            public List<bool> chamber; // 子弹装填情况
            public int currentPlayerIndex = 0;
            public int round = 0;


            public string Start(string p, BigInteger money)
            {
                if(round > 0)
                {
                    // is running
                    return $"比赛中，勿扰，，，";
                }

                if (money <= 0)
                {
                    return $"钱数什么情况？？？";
                }

                return Join(p, money);
            }

            public string Join(string p, BigInteger money)
            {
                string res = "";

                if ((p1 != null && p1.id == p) || (p2 != null && p2.id == p))
                {
                    // exist
                    return  $"不必重复参加";
                }
                else if (p1 != null && p2 != null)
                {
                    // full
                    return $"两名玩家已齐";
                }
                

                if (ModBank.Instance.TransMoney(p, Config.Instance.BotQQ, money, out var msg) != money)
                {
                    // 转账失败
                    return $"余额不足？你还剩{Config.Instance.UserInfo(p).Money.ToHans()}{ModBank.unitName}";
                }


                // join
                if (p1==null)
                {
                    p1 = new RoulettePlayer
                    {
                        id = p,
                        name = Config.Instance.UserInfo(p).Name,
                        money = money,
                        hp = 4
                    };
                    if (string.IsNullOrWhiteSpace(p1.name)) p1.name = p1.id.ToString();
                }else if (p2 == null)
                {
                    p2 = new RoulettePlayer
                    {
                        id = p,
                        name = Config.Instance.UserInfo(p).Name,
                        money = money,
                        hp = 4
                    };
                    if (string.IsNullOrWhiteSpace(p2.name)) p2.name = p2.id.ToString();
                }
                if (!ModRoulette.Instance.history.ContainsKey(p)) ModRoulette.Instance.history[p] = new GamePlayerHistory();
                ModRoulette.Instance.history[p].money += money;
                ModRoulette.Instance.history[p].id = p;

                res = $"{Config.Instance.UserInfo(p).Name}已成功加入恶魔轮盘，投了{money.ToHans()}{ModBank.unitName}\r\n";
                if (round <= 0 && p1 != null && p2 != null)
                {
                    // begin! game

                    res += NextRound();
                }

                return res;
            }


            public string ShootSelf(string from)
            {
                if (round <= 0)
                {
                    // not start.
                    return null;
                }
                

                string res = "";
                RoulettePlayer fromp = null;
                
                if(p1.id== from && currentPlayerIndex == 1)
                {
                    fromp = p1;
                }
                else if(p2.id == from && currentPlayerIndex == 2) 
                {
                    fromp = p2;
                }
                else
                {
                    // not avliable
                    return null ;
                }

                if (fromp.IsAlive)
                {
                    if(FireAt(fromp, out var desc))
                    {
                        res += desc;// $"{getHp(fromp.hp)}{fromp.name}💥🔫❗️\r\n";
                        if (!fromp.IsAlive)
                        {
                            // 死了
                            res += GameOver();
                            return res;
                        }
                        else
                        {
                            // 对手轮
                            currentPlayerIndex = 3 - currentPlayerIndex;    // 1=>2, 2=>1
                            res += NextRound();

                        }
                    }
                    else
                    {
                        // 空
                        res += desc;//$"{getHp(fromp.hp)}{fromp.name}💦🔫\r\n";
                        // 获得新一轮
                        res += NextRound();


                    }
                }

                return res;
            }



            public string ShootOther(string from)
            {
                if (round <= 0)
                {
                    // not start.
                    return null;
                }


                string res = "";
                RoulettePlayer target = null;

                if (p1.id == from && currentPlayerIndex == 1)
                {
                    target = p2;
                }
                else if (p2.id == from && currentPlayerIndex == 2)
                {
                    target = p1;
                }
                else
                {
                    // not avliable
                    return "";
                }

                if (target.IsAlive)
                {
                    if (FireAt(target, out var desc))
                    {
                        res += desc;// $"{getHp(target.hp)}{target.name}💥🔫❗️\r\n";
                        if (!target.IsAlive)
                        {
                            // 死了
                            res += GameOver();
                            return res;
                        }
                        else
                        {
                            // 对手轮
                            currentPlayerIndex = 3 - currentPlayerIndex;    // 1=>2, 2=>1
                            res += NextRound();
                        }
                    }
                    else
                    {
                        // 空
                        res += desc;// $"{getHp(target.hp)}{target.name}💦🔫\r\n";
                        // 对手轮
                        currentPlayerIndex = 3 - currentPlayerIndex;    // 1=>2, 2=>1
                        res += NextRound();
                    }
                }

                return res;
            }


            private bool FireAt(RoulettePlayer target, out string desc)
            {
                if (chamber.Count == 0)
                {
                    desc = $"没弹了\r\n";
                    return false;
                }

                bool isBullet = chamber[0];
                chamber.RemoveAt(0);

                if (isBullet)
                {
                    target.TakeDamage(1);
                    desc = $"{getHp(target.hp)}{target.name}💥🔫❗️\r\n";
                    return true; // 实弹命中
                }
                else
                {
                    desc = $"{getHp(target.hp)}{target.name}💦🔫\r\n";
                    return false; // 空包弹
                }
            }


            public string NextRound()
            {
                string res = "";

                if (round <= 0)
                {
                    // new game start
                    currentPlayerIndex = MyRandom.Next(1, 3);
                    res += SetupRound();
                    res += $"轮到{(currentPlayerIndex == 1 ? p1.name : p2.name)}了";
                }
                else
                {
                    if (chamber.Count <= 0)
                    {
                        // empty. reload
                        res += SetupRound();
                        
                    }
                    res += $"轮到{(currentPlayerIndex == 1 ? p1.name : p2.name)}了";
                }


                return res;
            }

            private string SetupRound()
            {
                int totalBullets = MyRandom.Next(3, 7); // 随机子弹数量 1 ~ 6
                int emptyBullets = MyRandom.Next(1, totalBullets); // 随机空包弹数量

                round++;

                chamber = new List<bool>();
                for (int i = 0; i < totalBullets - emptyBullets; i++) chamber.Add(true); // 装填实弹
                for (int i = 0; i < emptyBullets; i++) chamber.Add(false); // 装填空包弹
                Util.FisherYates(chamber);

                var res = $"第{round}轮开始！一共{totalBullets}发：{getBullets(totalBullets-emptyBullets,emptyBullets)}\r\n";

                res += $"{getHp(p1.hp)}{p1.name}🔫{p2.name}{getHp(p2.hp)}\r\n";

                return res;
            }

            private string getBullets(int real, int empty)
            {
                string res = "";


                for (int i = 0; i < real; i++) res += "💣";
                for (int i = 0; i < empty; i++) res += "😀";
                

                return res;
            }

            private string getHp(int num)
            {
                string res = "";

                for (int i = 0; i < num; i++) res += "❤";
                if (num == 0) res = "💀";

                return res;
            }



            private string GameOver()
            {
                string res = "游戏结束！";
                ModRoulette.Instance.history[p1.id].playnum++;
                ModRoulette.Instance.history[p2.id].playnum++;
                if (p1.IsAlive && !p2.IsAlive)
                {
                    ModRoulette.Instance.history[p1.id].winnum++;
                    res +=  Reward(p1);
                }
                else if (p2.IsAlive && !p1.IsAlive)
                {
                    ModRoulette.Instance.history[p2.id].winnum++;
                    res += Reward(p2);
                }

                round = 0;
                currentPlayerIndex = 0;
                chamber.Clear();
                
                p1 = null;
                p2 = null;


                return res;
            }

            private string Reward(RoulettePlayer winner)
            {
                RoulettePlayer loser = null;
                if (winner == p1) loser = p2;
                else if(winner == p2) loser = p1;


                var res = $"{winner.name}活到了最后！而{loser.name}被爆了{loser.money.ToHans()}枚{ModBank.unitName}\r\n";
                BigInteger reward = 0;
                reward = (BigInteger)((decimal)(loser.money + winner.money) * (decimal)1.5);

                
                
                if(ModBank.Instance.TransMoney(Config.Instance.BotQQ, winner.id, reward, out var msg) != reward)
                {
                    res += msg;
                }
                res += $"{winner.name}赢了{reward.ToHans()}{ModBank.unitName}，余额{Config.Instance.UserInfo(winner.id).Money.ToHans()}";
                return res;
            }
        }

    }


}


