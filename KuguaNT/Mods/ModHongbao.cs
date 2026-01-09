using Kugua.Integrations.NTBot;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Prophecy;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Timers;
using Kugua.Core;
using Kugua.Mods.Base;

namespace Kugua.Mods
{

    /// <summary>
    /// 红包模块
    /// </summary>
    public class ModHongbao : Mod
    {
        System.Timers.Timer TaskTimer;

        List<(string, Hongbao)> hongbaos = new List<(string, Hongbao)>();
        object hongbaoMutex = new object();

        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^发(\d+个)?红包(\S+)(.*)$", RegexOptions.Singleline), handleHongbao));
            ModCommands.Add(new ModCommand(new Regex(@"^开$", RegexOptions.Singleline), handleGetHongbao));
            ModCommands.Add(new ModCommand(new Regex(@"^开$", RegexOptions.Singleline), handleGetHongbao, _needAsk:false));

            TaskTimer = new(1000 * 10); // 10s
            TaskTimer.AutoReset = true;
            TaskTimer.Start();
            TaskTimer.Elapsed += TaskTimer_Elapsed;


            return true;
        }


        // 为了把超时红包返还
        private void TaskTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            lock (hongbaoMutex)
            {
                foreach (var h in hongbaos)
                {
                    if ((DateTime.Now - h.Item2.beginTime).TotalMinutes > 30)
                    {
                        ModBank.Instance.TransMoney(Config.Instance.BotQQ, h.Item1,h.Item2.leftMoney, out _);
                        hongbaos.Remove(h);
                    }
                }
            }

        }

        //public async override Task<bool> HandleMessagesDIY(MessageContext context)
        //{
        //    //Logger.Log("= " + context.recvMessages.ToTextString());
        //    if (context.recvMessages.ToTextString().Trim() == "开")
        //    {
        //        handleGetHongbao(context, new string[] { "开" });
        //        return true;
        //    }

        //    return false;
        //}
            // 领群内红包
            private string handleGetHongbao(MessageContext context, string[] param)
        {
            try
            {
                string idbegin = $"{context.groupId}_";
                lock (hongbaoMutex)
                {
                    foreach (var bb in hongbaos)
                    {
                        if (bb.Item1.StartsWith(idbegin))
                        {
                            var b = bb.Item2;
                            var getMoney = b.Get(context.userId);
                            if (getMoney > 0)
                            {
                                var bitems = bb.Item1.Split('_');
                                var from = bitems[1];
                                var fname = Config.Instance.UserInfo(from).Name;
                                ModBank.Instance.TransMoney(Config.Instance.BotQQ, context.userId, getMoney, out _);
                                context.SendBackText($"你领了{fname}的红包：{getMoney.ToHans()}{ModBank.unitName}", true);
                                if (b.left <= 0)
                                {
                                    context.SendBackText($"{fname}的{b.getFinish()}", false);

                                    hongbaos.Remove(bb);

                                    break;
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }


        /// <summary>
        /// 随机红包，后接金额+文本
        /// 发红包1000 恭喜发财
        /// </summary>
        private string handleHongbao(MessageContext context, string[] param)
        {
            try
            {
                int num = 0;
                BigInteger money = 0;
                var text = "";
                Logger.Log($"{string.Join(",", param)}");
                int.TryParse(param[1].TrimEnd('个'), out num);
                if (num <= 0 || num > 1000) num = 3;
                money = Core.Util.ConvertToBigInteger(param[2]);
                text = param[3];

                if (string.IsNullOrWhiteSpace(text))text= "恭喜发财";
                text = text.Trim();
                if (money <= 0) return "";
                else if(ModBank.Instance.TransMoney(context.userId, Config.Instance.BotQQ, money, out _) == money)
                {
                    lock (hongbaoMutex)
                    {
                        // good
                        hongbaos.Add(($"{context.groupId}_{context.userId}_{MyRandom.Next(1000, 2000)}", new Hongbao(num, money)));

                        context.SendBackImage(ImageUtil.GetHongbao(text));
                    }
                }
                else
                {
                    return $"没钱了，账上余额{Config.Instance.UserInfo(context.userId).Money.ToHans()}";
                }
            }
            catch(Exception e)
            {
                Logger.Log(e);
            }
            return null;
        }





        public class Hongbao
        {
            BigInteger money;
            public DateTime beginTime;
            public int totalNum = 3;

            List<BigInteger> baos;
            public int left{get{return baos==null?0:baos.Count();}}
            public BigInteger leftMoney
            {
                get
                {
                    BigInteger res = 0;
                    foreach (var b in baos) res += b;
                    return res;
                }
            }

            Dictionary<string, BigInteger> persons = new Dictionary<string, BigInteger>();

            public Hongbao(int _totalNum, BigInteger _money)
            {
                money = _money;
                if (_totalNum <= 0 || _totalNum > 100) _totalNum = 3;
                totalNum = _totalNum;
                baos = Distribute(money, totalNum);
                beginTime = DateTime.Now;
            }

            static List<BigInteger> Distribute(BigInteger total, int n = 3)
            {
                if (n <= 1) return new List<BigInteger> { total };
                if (total <= n) return Enumerable.Repeat(new BigInteger(1), (int)total).ToList();
                
                var res = new List<BigInteger>();
                while(n >0 && total > n)
                {
                    BigInteger canGet = total - n + 1;
                    var get = MyRandom.Next(1, canGet);
                    res.Add(get);
                    total -= get;
                    n--;
                    if(total == n)
                    {
                        while (n-- > 0) res.Add(1); break;
                    }
                    if(n == 1)
                    {
                        res.Add(total);
                        break;
                    }
                }
                // Fisher-Yates 洗牌算法，完全打乱
                for (int i = res.Count - 1; i > 0; i--)
                {
                    int j = MyRandom.Next(0, i + 1);
                    var temp = res[i];
                    res[i] = res[j];
                    res[j] = temp;
                }
                return res;
            }

            public BigInteger Get(string name)
            {
                if (!persons.ContainsKey(name) && left>0)
                {
                    persons.Add(name, baos.First());
                    BigInteger num = baos.First();
                    baos.RemoveAt(0);
                    return num;
                }
                return -1;
            }

            public string getFinish()
            {
                string biguser = "";
                BigInteger bigmoney = 0;
                foreach(var item in persons)
                {
                    if (item.Value > bigmoney)
                    {
                        bigmoney = item.Value;
                        biguser = item.Key;
                    }
                }
                return $"{totalNum}个红包{(DateTime.Now - beginTime).TotalSeconds:F0}秒被领完。运气王是{Config.Instance.UserInfo(biguser).Name}({bigmoney.ToHans()}{ModBank.unitName})";
            }
        }




    }


}
