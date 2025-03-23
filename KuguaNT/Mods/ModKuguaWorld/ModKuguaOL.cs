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


            TaskTimer = new(1000 * 60); //ms
            TaskTimer.AutoReset = true;
            TaskTimer.Start();
            TaskTimer.Elapsed += TaskTimer_Elapsed;

            return true;
        }

        private void TaskTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            
        }

        public override async Task<bool> HandleMessagesDIY(MessageContext context)
        {
            if (context.Group.Is("游戏"))
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

            if (context.Texts.StartsWith("启动"))
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

    }
    public delegate string HandleKuguaOlCommandEvent(MessageContext context, string[] param, MessageContext context2);
    public class KuguaOlCommand
    {
        public string Name;
        public string[] Params;
        public MessageContext context;
        public HandleKuguaOlCommandEvent Callback;

        public bool DealReact(MessageContext reactContext)
        {
            try
            {
                string callbackResult = Callback(context, Params, reactContext);
                if (callbackResult == null)
                {
                    return true;
                }

            }catch(Exception ex)
            {
                Logger.Log(ex);
                
            }
            return false;
        }
    }
}
