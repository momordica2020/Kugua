using Kugua.Core;
using Kugua.Integrations.AI;
using Kugua.Integrations.NTBot;
using System.Text.RegularExpressions;
using System.Timers;

namespace Kugua.Mods
{
    public class ModKuguaOL : Mod
    {
        System.Timers.Timer TaskTimer;

        object CommandMutex = new object();
        List<KuguaOlCommand> Commands = new List<KuguaOlCommand>();

        Dictionary<string, Game2048> games = new Dictionary<string, Game2048>();

        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^2048$"), parseNew));

            ModCommands.Add(new ModCommand(new Regex(@"^修炼$"), handleXiulian, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^突破$"), handleTupo, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^查询$"), handleInfo, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^重开$"), handeRestartXiuxian, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^(吃|使用|熔炼|炼化|卖)(.+)$"), handleUse, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^全部(吃|使用|熔炼|炼化|卖)$"), handleUseAll, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^与(.+)对战$"), handleDuizhan, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^与(.+)双修$"), handleShuangxiu, _needAsk: false));

            TaskTimer = new(1000 * 60); //ms
            TaskTimer.AutoReset = true;
            TaskTimer.Start();
            TaskTimer.Elapsed += TaskTimer_Elapsed;

            GameXiuxian.Init();

            return true;
        }

        private void TaskTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            GameXiuxian.Save();

        }

        public override async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            if (context.Group != null && context.Group.Is("游戏"))
            {
                if (context.IsReact)
                {
                    
                    lock (CommandMutex)
                    {
                        for (int i = Commands.Count - 1; i >= 0; i--)
                        {
                            if (Commands[i].DealReact(context)) Commands.RemoveAt(i);
                        }
                    }
                    return true;
    
                }

            }


            return false;
        }

        /// <summary>
        /// 启动2048游戏
        /// 2048
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string parseNew(MessageContext context, string[] param)
        {
            try
            {
                //LLM.Instance.HSSendSingle("你正在主持一场","")
                if (!context.IsGroup || !context.Group.Is("游戏")) return "";
                if (!games.ContainsKey(context.groupId))
                {
                    // new game
                    games.Add(context.groupId, new Game2048());
                    games[context.groupId].Initialize();

                }
                AddSelectCommand(new KuguaOlCommand
                {
                    context = context,
                    Name = games[context.groupId].GetGridString(),
                    Params = ["129", "41", "125", "43"],
                    Callback = handleSelectionTest,
                });

                return null;
            } 
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

        void AddSelectCommand(KuguaOlCommand cmd)
        {
            lock (CommandMutex)
            {
                Commands.Add(cmd);
            }
            string msgid = cmd.context.SendBackText(cmd.Name,false,false,false).Result;
            foreach(var item in cmd.Params)
            {
                cmd.context.client.SendEmojiLike(msgid, int.Parse(item));
            }
           

        }

        private string handleSelectionTest(MessageContext context, string[] param, MessageContext context2)
        {
            var react = context2.React;
            if (react == null) return null;
            //Logger.Log("react.emoji.id = " + react.emoji.id);

            if (context.Texts.StartsWith("2048"))
            {
                // 2048playing
                if(games.TryGetValue(context.groupId, out var game))
                {
                    if (game.IsGameOver())
                    {
                        context.SendBackText($"{game.GetGridString()}\r\n游戏结束，总得分：{game.score}");
                        game.Initialize();
                    }
                    else
                    {
                        for(int i = 0; i < param.Length; i++)
                        {
                            if (react.emoji.id == param[i])
                            {
                                if(game.Move(i + 1))
                                {
                                    game.AddNewNumber();
                                    game.AddNewNumber();
                                    AddSelectCommand(new KuguaOlCommand
                                    {
                                        context = context,
                                        Name = games[context.groupId].GetGridString(),
                                        Params = ["129", "41", "125", "43"],
                                        Callback = handleSelectionTest,
                                    });
                                    //context.SendBackText($"{game.GetGridString()}");
                                }
                                else
                                {
                                    if (game.IsGameOver())
                                    {
                                        context.SendBackText($"游戏结束，总得分：{game.score}");
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }

                            }
                        }
                    
                    }
                    return null;
                }
            }
            else
            {
                context.SendBackText($"选了? {react.emoji.name}");
            }

                //if (react.emoji.id == param[0])
                //{
                //    context.SendBackText("选了1 🔥");
                //}
                //else if(react.emoji.id == param[1])
                //{
                //    context.SendBackText("选了2 💦");
                //}
                //else
                //{
                //    context.SendBackText($"选了? {react.emoji.name}");
                //}

                return "";
        }



        /// <summary>
        /// 进行修炼
        /// 修炼
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handleXiulian(MessageContext context, string[] param)
        {
            return GameXiuxian.Action(context.userId, MyRandom.NextString(["战斗", "修炼", "奇遇", "夺宝"]));

            //return null;
        }



        /// <summary>
        /// 突破
        /// 突破
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handleTupo(MessageContext context, string[] param)
        {
            return GameXiuxian.AddLevel(context.userId);

            //return null;
        }



        /// <summary>
        /// 查询修仙个人信息
        /// 查询
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handleInfo(MessageContext context, string[] param)
        {
            return GameXiuxian.Info(context.userId);

            //return null;
        }


        /// <summary>
        /// 重置（转生）成一个新的修仙者
        /// 重开
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handeRestartXiuxian(MessageContext context, string[] param)
        {
            return GameXiuxian.Restart(context.userId);

            //return null;
        }

        /// <summary>
        /// 使用装备，模糊匹配道具名称，别输入太长会被忽略
        /// 吃x/使用x/熔炼x/炼化x/卖x
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handleUse(MessageContext context, string[] param)
        {
            string action = param[1];
            string itemName = param[2];
            if (itemName.Length > 8) return "";
            return GameXiuxian.UseItem(context.userId, itemName, action);

            //return null;
        }


        /// <summary>
        /// 以特定方式消耗所有道具
        /// 全部吃/全部使用/全部熔炼/全部炼化/全部卖
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handleUseAll (MessageContext context, string[] param)
        {
            string action = param[1];
            return GameXiuxian.UseItem(context.userId, "", action);

            //return null;
        }


        /// <summary>
        /// 与特定玩家双修，可以输入q号或者修仙者名称
        /// 与287859992双修/与荧瞳双修
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handleShuangxiu(MessageContext context, string[] param)
        {
            string id2 = param[1];
            return GameXiuxian.getShuangxiu(context.userId, id2, "双修");

            //return null;
        }


        /// <summary>
        /// 与特定玩家对战，可以输入q号或者修仙者名称
        /// 与287859992对战/与荧瞳对战
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string handleDuizhan(MessageContext context, string[] param)
        {
            string id2 = param[1];
            return GameXiuxian.getShuangxiu(context.userId, id2, "对战");

            //return null;
        }
    }
    public delegate string HandleKuguaOlCommandEvent(MessageContext context, string[] param, MessageContext context2);
}
