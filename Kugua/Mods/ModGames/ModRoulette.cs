using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;

using System.Text.RegularExpressions;
using MeowMiraiLib.Msg;
using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components;
using ChatGPT.Net.DTO.ChatGPT;
using static System.Net.WebRequestMethods;
using static MeowMiraiLib.Msg.Sender.GroupMessageSender;


namespace Kugua
{
    public class RInfo
    { }


    public class ModRoulette : Mod
    {

        public Dictionary<long, RouletteGame> info = new Dictionary<long, RouletteGame>();


        private static readonly Lazy<ModRoulette> instance = new Lazy<ModRoulette>(() => new ModRoulette());
        public static ModRoulette Instance => instance.Value;
        private ModRoulette()
        {


        }
        public override bool Init(string[] args)
        {
            try
            {

                ModCommands[new Regex(@"^\s*轮盘\s*(\d+)")] = StartGame;
                //ModCommands[new Regex(@"^\s*加入\s*(\d+)")] = JoinGame;
                ModCommands[new Regex(@"射我")] = ShootMe;
                ModCommands[new Regex(@"射他")] = ShootHim;

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return true;
        }
        private string StartGame(MessageContext context, string[] param)
        {
            try
            {
                if (!info.ContainsKey(context.groupId))
                {
                    info[context.groupId] = new RouletteGame();
                }
                var g = info[context.groupId];
                var res =  g.Start(context.userId, int.Parse(param[1]));
                context.SendBackPlain(res, false);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                
            }

            return null;
        }


  
        private string ShootHim(MessageContext context, string[] param)
        {
            if (info.ContainsKey(context.groupId))
            {
                var g = info[context.groupId];
                var res = g.ShootOther(context.userId);
                context.SendBackPlain(res, false);
            }


            return "";
        }

        private string ShootMe(MessageContext context, string[] param)
        {
            if (info.ContainsKey(context.groupId))
            {
                var g = info[context.groupId];
                var res = g.ShootSelf(context.userId);
                context.SendBackPlain(res, false);
            }

            return "";

        }









        // 玩家类
        public class RoulettePlayer
        {
            public long id;
            public string name;
            public int hp;
            public int money;

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


            public string Start(long p, int money)
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

            public string Join(long p, int money)
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
                    return $"余额不足？你还剩{Config.Instance.UserInfo(p).Money}{ModBank.unitName}";
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
                
                res = $"{Config.Instance.UserInfo(p).Name}已成功加入恶魔轮盘，投了{money}{ModBank.unitName}\r\n";
                if (round <= 0 && p1 != null && p2 != null)
                {
                    // begin! game

                    res += NextRound();
                }

                return res;
            }


            public string ShootSelf(long from)
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
                    return "";
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



            public string ShootOther(long from)
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
                    desc = $"没子弹了\r\n";
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
                StaticUtil.FisherYates(chamber);

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

                if (p1.IsAlive && !p2.IsAlive)
                {
                    res +=  Reward(p1);
                }
                else if (p2.IsAlive && !p1.IsAlive)
                {

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


                var res = $"{winner.name}活到了最后！而{loser.name}被爆了{loser.money}枚{ModBank.unitName}\r\n";
                long reward = 0;
                reward = (long)((loser.money + winner.money) * 1.5);

                
                
                if(ModBank.Instance.TransMoney(Config.Instance.BotQQ, winner.id, reward, out var msg) != reward)
                {
                    res += msg;
                }
                res += $"{winner.name}赚了{reward}{ModBank.unitName}，余额{Config.Instance.UserInfo(winner.id).Money}";
                return res;
            }
        }

    }


}


