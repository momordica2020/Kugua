using MeowMiraiLib;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using MMDK.Util;
using System.Text.RegularExpressions;
using MeowMiraiLib.Msg;
using MMDK.Mods;
using static MeowMiraiLib.Msg.Sender.GroupMessageSender;

namespace MMDK.Mods
{
    public class ModSlotMachine : Mod
    {
        public Dictionary<string, byte[]> emojis = new Dictionary<string, byte[]>();
        public Dictionary<long, DateTime> playerLock = new Dictionary<long, DateTime>();


        private static readonly Lazy<ModSlotMachine> instance = new Lazy<ModSlotMachine>(() => new ModSlotMachine());
        public static ModSlotMachine Instance => instance.Value;
        private ModSlotMachine()
        {


        }
        public override bool Init(string[] args)
        {
            try
            {
                var emojiss = Directory.GetFiles($"{Config.Instance.ResourceRootPath}{Path.DirectorySeparatorChar}game{Path.DirectorySeparatorChar}emojis", "*.png");
                foreach (var f in emojiss)
                {
                    byte[] pngBytes = System.IO.File.ReadAllBytes(f);
                    emojis[f] = pngBytes;
                }



                ModCommands[new Regex(@"^老虎机\s*(\d+)?")] = StartGame;

            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
            return true;
        }

        private string StartGame(MessageContext context, string[] param)
        {
            long money = 1;
            if (param.Length < 2) money = 1;
            else if (!long.TryParse(param[1], out money)) money = 1;

            var tuser = Config.Instance.UserInfo(context.userId);
            if (tuser.Money < money)
            {
                // money not enough.
                return ($"{ModBank.unitName}不够，余额：{tuser.Money}");
            }


            if (!playerLock.ContainsKey(context.userId)) playerLock[context.userId] = DateTime.Now;
            if (Monitor.TryEnter(playerLock[context.userId]))
            {
                try
                {
                    if (ModBank.Instance.TransMoney(context.userId, Config.Instance.BotQQ, money, out string msg) != money)
                    {
                        return ($"{msg}");
                    }
                    long userNowMoney = tuser.Money;    // 防止多线程时候，后面钱数显示bug
                    var gifBase64 = RollSymPlay.GenerateGif(out var rollres);
                    int score = 0;
                    //Logger.Instance.Log($"!{rollres.Count}");
                    if (rollres.Count == 9)
                    {
                        for (int i = 1; i < 2; i++)
                        {
                            //Logger.Instance.Log($"!{rollres[i][0]} {rollres[i+3][0]} {rollres[i+6][0]}");
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

                    string resultString = $"你投了{money}{ModBank.unitName}，";
                    if (score <= 0)
                    {
                        resultString += $"很遗憾，没中，余额{userNowMoney}";
                    }
                    else
                    {
                        long getmoney = money * (score);
                        if (ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, getmoney, out string msg2) != getmoney)
                        {
                            resultString += $"得到{score}分！但是{msg2}，余额{tuser.Money}";
                        }
                        else
                        {
                            resultString += $"得到{score}分！恭喜赚得{getmoney}{ModBank.unitName}，余额{tuser.Money}";
                        }
                    }

                    var sendOutMsg = new Message[]
                    {
                        new At(context.userId,null),
                        new Image(null,null,null, gifBase64),
                        new Plain(resultString),
                    };
                    context.SendBack(sendOutMsg);
                    // save();
                }
                finally
                {
                    // 无论如何，都释放锁

                    Monitor.Exit(playerLock[context.userId]);
                    Console.WriteLine("Thread exited critical section");
                }
                return null;
            }
            else
            {
                // 如果未能获取锁，输出并退出
                return null;
            }
        }

        public override void Exit()
        {
            //Save();
        }


    }
}
