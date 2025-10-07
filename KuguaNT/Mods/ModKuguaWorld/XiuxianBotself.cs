using Kugua.Core;
using Newtonsoft.Json;
using System.Data;
using System.Numerics;
using ZhipuApi;

namespace Kugua.Mods{
    public class XiuxianBotself
    {
        public XiuxianUser bot;
        public string targetGroup = "833246207";
        public DateTime lastChartTime = DateTime.Now;



        public XiuxianBotself()
        {

        }



        public string play()
        {
            try
            {
                if (Config.Instance.GroupWithTag("bot挂机") is string gid) targetGroup = gid;
                else targetGroup = null;
                if (targetGroup==null) return null;


                if (bot == null)
                {
                    string res = GameXiuxian.Cact(Config.Instance.BotQQ, MyRandom.NextString(["战斗", "修炼", "奇遇", "夺宝"]));
                    //botuser = GameXiuxian.users[Config.Instance.BotQQ];
                    bot = GameXiuxian.users[Config.Instance.BotQQ];
                    return res;
                }
                if(bot.CheckCooldown is string str)
                {
                    // cd
                }
                else
                {
                    if (MyRandom.NextDouble < 0.3) return null;
                    if (bot.items.Count >= 2)
                    {
                        return GameXiuxian.CuseItem(bot.id, "", MyRandom.NextString(["吃", "使用", "熔炼", "炼化", "卖"]));
                    }
                    if (GameXiuxian.GetLevelPower(bot.level)<= bot.prop["灵力"])
                    {
                        return GameXiuxian.Ctupo(bot.id);
                    }
                    else
                    {
                        if(MyRandom.NextDouble < 0.1 && bot.prop["灵力"] > 100)
                        {
                            return GameXiuxian.Cpaid(bot.id);
                        }
                        else
                        {
                            return GameXiuxian.Cact(Config.Instance.BotQQ, MyRandom.NextString(["战斗", "修炼", "奇遇", "夺宝"]));

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }
    }
}