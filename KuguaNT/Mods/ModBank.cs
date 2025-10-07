using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChatGPT.Net;
using Kugua.Core;


namespace Kugua.Mods
{
    /// <summary>
    /// 银行
    /// </summary>
    public class ModBank : Mod
    {
        private static readonly Lazy<ModBank> instance = new Lazy<ModBank>(() => new ModBank());
        public static ModBank Instance => instance.Value;
        private ModBank()
        {


        }

        public static string unitName = "马币";



        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^签到$"),DailyAttendance));
            //ModCommands.Add(new ModCommand(new Regex(@"^修炼$"), DailyWork));


            ModCommands.Add(new ModCommand(new Regex(@"^发币(.+)"), AddBotMoney));
            ModCommands.Add(new ModCommand(new Regex(@"^放生(.+)"), RemoveBotMoney));
            //ModCommands.Add(new ModCommand(new Regex(@"^给(.+)补贴(.+)"), Grant));
            ModCommands.Add(new ModCommand(new Regex(@"^捐(.+)"),Donate));
            //ModCommands.Add(new ModCommand(new Regex(@"^供养(.+)"), Donate2));
            ModCommands.Add(new ModCommand(new Regex(@"^(.*)给(.+)转(.+)"), PostMoney));




            ModCommands.Add(new ModCommand(new Regex(@"^(富人榜|富豪榜)"), showRichest));

            ModCommands.Add(new ModCommand(new Regex(@"^(穷人榜)"), showPoorest));



            return true;
        }

        private string DailyWork(MessageContext context, string[] param)
        {
            var u = Config.Instance.UserInfo(context.userId);
            if (context.IsAdminUser)
            {
                BigInteger maxmoney = u.Money / 25;
                if (maxmoney < 1000) maxmoney = 1000;
                BigInteger minmoney = u.Money / 50;
                if (maxmoney < 1) maxmoney = 1;
                // success
                BigInteger money = MyRandom.Next(minmoney, maxmoney);
                money = Util.Floor(money, 2);
                u.Money += money;
                u.LastSignTime = DateTime.Now;
                u.SignTimes += 1;
                return $"您今日修炼得到{money.ToHans()}{unitName}，现在账上总额{u.Money.ToHans()}";
            }
            return "";
        }

        /// <summary>
        /// 捐钱给bot，积累赛博阴德
        /// 我苦捐100
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string Donate(MessageContext context, string[] param)
        {
            string answer = "";

            try
            {
                
                BigInteger money = Util.ConvertToBigInteger(param[1]);

                if (money > 0)
                {

                    var u = Config.Instance.UserInfo(context.userId);
                    if (u == null) return "";

                    money = BigInteger.Min(money, u.Money);
                    if(u.Money <= 0)
                    {
                        answer += $"虽然您身无分文，但我知道您是个好人";
                    }
                    else if(money < 0)
                    {
                        answer += $"反向募捐？笑死，你赛博阴德没有了";
                    }
                    else if(money == 0)
                    {
                        //answer += $"？";
                    }
                    else
                    {
                        TransMoney(context.userId, Config.Instance.BotQQ, money, out _);
                        answer += $"{Config.Instance.BotName}捐了{money.ToHans()}{unitName}！阿弥陀佛，阿弥陀佛！善哉，善哉！余额：{u.Money.ToHans()}";
                    }
                    
                }
                

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return answer;
        }



        //private string Donate2(MessageContext context, string[] param)
        //{

        //}

        //private string Donate(MessageContext context, string[] param)
        //{

        //}


        private string Grant(MessageContext context, string[] param)
        {
            if (!context.IsAdminUser) return "";
            
            
            long targetqq = -1;
            if (!long.TryParse(param[1], out targetqq)) return $"{param[1]}?不认识";
            var money = Util.ConvertToBigInteger(param[2]);
            if(money > 0)
            {
                string res = "";
                // 补贴
                BigInteger succeedMoney = TransMoney(Config.Instance.BotQQ, targetqq.ToString(), money, out res);
                //racehorse.dailyAttendance(group, user);
                return res;
            }
            else
            {
                return "？";
            }
        }
        

        /// <summary>
        /// 转账
        /// 给123456789转1000
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string PostMoney(MessageContext context, string[] param)
        {
            try
            {
                // 发起人信息
                string from = param[1]; 
                long fromqq = -1;
                if (from == "你" && context.IsAdminUser)
                {
                    // 让bot作为主体来转账
                    fromqq = long.Parse(Config.Instance.BotQQ);
                }
                else
                {
                    // 默认是发起者自己给别人转账
                    fromqq = long.Parse(context.userId);
                }

                // 目标信息
                string target = param[2];
                long targetqq = -1;
                if (!long.TryParse(target, out targetqq))
                {
                    
                    // nick name -> qq
                    //targetqq = bank.getID(target, msg.fromGroup);
                    // targetqq = getQQNumFromGroup(group, target.Trim());
                }

                if (targetqq <= 0)
                {
                    if (target == "你")
                    {
                        targetqq = long.Parse(Config.Instance.BotQQ);
                    }
                    else if (context.IsAdminUser && target == "我" && from=="你")
                    {
                        // 借贷？
                        targetqq = long.Parse(context.userId);
                    }
                    else if (target.Length > 10)
                    {
                        return "";
                    }
                    else
                    {
                        context.SendBackText($"系统里找不到昵称 {target} ，转币失败。可以输入qq号码直接转");
                        return "";
                    }
                    
                }


                string res = "";
                if(fromqq>0 &&  targetqq>0) 
                {
                    BigInteger money = Util.ConvertToBigInteger(param[3]);
                    BigInteger succeedMoney = TransMoney(fromqq.ToString(), targetqq.ToString(), money, out res);
                }

                if (!string.IsNullOrWhiteSpace(res))
                {
                    return res;
                }
            }
            catch { }
            return "";
        }

        // 发币
        public string AddBotMoney(MessageContext context, string[] param)
        {
            if (context.IsAdminUser)
            {
                BigInteger money = Util.ConvertToBigInteger(param[1]);
                if (money > 0)
                {
                    var message = AddMoney(context.groupId, Config.Instance.BotQQ, money);
                    //racehorse.dailyAttendance(group, user);
                    return message;
                }
            }

            return "";
        }

        /// <summary>
        /// 给特定玩家铸币！
        /// </summary>
        /// <param name="group"></param>
        /// <param name="userqq"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public string AddMoney(string group, string userqq, BigInteger money)
        {
            string answer = "";
            try
            {

                var u = Config.Instance.UserInfo(userqq);

                u.Money += money;
                answer += $"{u.Name} 新到账 {money.ToHans()}元。{unitName}余额：{u.Money.ToHans()}";
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
            return answer;
        }

        // 放生了
        private string RemoveBotMoney(MessageContext context, string[] param)
        {
            string answer = "";

            try
            {
                if (context.IsAdminUser)
                {
                    BigInteger money = Util.ConvertToBigInteger(param[1]);

                    if (money > 0)
                    {
                        
                        var u = Config.Instance.UserInfo(Config.Instance.BotQQ);

                        money = BigInteger.Min(money, u.Money);
                        u.Money -= money;
                        answer += $"{Config.Instance.BotName}放生了{money.ToHans()}{unitName}！阿弥陀佛，阿弥陀佛！善哉，善哉！余额：{u.Money.ToHans()}";
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return answer;
        }


        /// <summary>
        /// 每日签到，领取低保
        /// 签到
        /// </summary>
        /// <param name="group"></param>
        /// <param name="userqq"></param>
        public string DailyAttendance(MessageContext context, string[] param)
        {
            var u = Config.Instance.UserInfo(context.userId);
            if (u.LastSignTime < DateTime.Today)
            {
                BigInteger maxmoney = u.Money / 25;
                if (maxmoney < 1000) maxmoney = 1000;
                BigInteger minmoney = u.Money / 50;
                if (maxmoney < 1) maxmoney = 1;
                // success
                BigInteger money = MyRandom.Next(minmoney, maxmoney);
                money = Util.Floor(money, 2);
                u.Money += money;
                u.LastSignTime = DateTime.Now;
                u.SignTimes += 1;
                return $"您今日领取失业补助{money.ToHans()}{unitName}，现在账上总额{u.Money.ToHans()}";
            }
            else
            {
                return $"嗨嗨嗨，今天{u.LastSignTime.ToString("HH:mm:ss")}领过了";
            }
        }


        /// <summary>
        /// 打印账户当前金额数据
        /// </summary>
        /// <param name="userqq"></param>
        /// <returns></returns>
        public string ShowUserAccountInfo(string userqq)
        {
            var u = Config.Instance.UserInfo(userqq);
            return $"您的账上共有{u.Money.ToHans()}枚{unitName}。共领取失业补助{u.SignTimes}次，今日失业补助{(u.LastSignTime >= DateTime.Today ? "已领取" : "还未领取")}";
        }

        /// <summary>
        /// 查看账户余额
        /// </summary>
        /// <param name="userqq"></param>
        /// <returns></returns>
        public BigInteger ShowBalance(string userqq)
        {
            var user = Config.Instance.UserInfo(userqq);
            return user.Money;
        }

        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="fromqq">发起转账的用户QQ</param>
        /// <param name="targetqq">接收转账的用户QQ</param>
        /// <param name="money">转账金额</param>
        /// <param name="message">转账结果信息</param>
        /// <returns>成功钱数，失败为0</returns>
        public BigInteger TransMoney(string fromqq, string targetqq, BigInteger money, out string message)
        {
            message = "";
            if (money <= 0)
            {
                message = "只允许正向转币";
                return 0;
            }

            var user1 = Config.Instance.UserInfo(fromqq);
            var user2 = Config.Instance.UserInfo(targetqq);

            if (user1.Money < money)
            {
                if(fromqq == Config.Instance.BotQQ)
                {
                    // bot向外转账可以负数

                }
                else
                {
                    message = $"您的余额不足。当前余额{user1.Money.ToHans()}{unitName}";
                    return 0;
                }

               
            }

            message = $"您向{targetqq}转了{money.ToHans()}枚{unitName}，";
            BigInteger user1OldMoney = user1.Money;
            BigInteger user2OldMoney = user2.Money;
            try
            {
                
                checked
                {
                    user1.Money -= money;
                    user2.Money += money;
                }
            }
            catch (OverflowException)
            {
                message += $"转币失败：{user1}或{user2}的{unitName}溢出，所转数额{money.ToHans()}，发起者余额{user1.Money.ToHans()}，接收者余额{user2.Money.ToHans()}。";
                Logger.Log(message);
                user1.Money = user1OldMoney; // 恢复余额
                user2.Money = user2OldMoney; // 恢复余额
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                message += $"银行被橄榄了，你钱没了！请带截图联系bot管理者{Config.Instance.App.Avatar.adminQQ}";
                return 0;
            }
            message += $"转币成功，您的{unitName}余额{user1.Money.ToHans()}，对方余额{user2.Money.ToHans()}";

            return money;
        }

        public bool ProcessTransfer(Player user1, Player user2, BigInteger money)
        {
            


           

            // 可以记录转账记录
            // WriteRecord(new BankRecord(user1.QQ, user2.QQ, money, "转账", "成功"));
            return true;


        }





        /// <summary>
        /// 富人榜
        /// 富人榜/富豪榜
        /// </summary>
        /// <returns></returns>
        public string showRichest(MessageContext context, string[] param)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int maxnum = 10;
                var users = Config.Instance.users.Values.Where(p => p.UseTimes > 0).ToList();
                users.Sort((left, right) =>
                {
                    return -1 * left.Money.CompareTo(right.Money);
                });

                sb.Append($"富 豪 榜 (基尼系数{Util.CalculateGiniCoefficient(users.Select(u=>u.Money).ToList())}) \r\n");
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{users[i].Name},{users[i].Money.ToHans()}枚\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return "";
            }
        }



        /// <summary>
        /// 穷人榜
        /// 穷人榜
        /// </summary>
        /// <returns></returns>
        public string showPoorest(MessageContext context, string[] param)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int maxnum = 10;
                var users = Config.Instance.users.Values.Where(p=>p.UseTimes > 0).ToList();
                users.Sort((left, right) =>
                {
                    return left.Money.CompareTo(right.Money);
                });

                sb.Append($"穷 人 榜 (基尼系数{Util.CalculateGiniCoefficient(users.Select(u=>u.Money).ToList())})\r\n");
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{users[i].Name},{users[i].Money.ToHans()}枚\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return "";
            }
        }

        /// <summary>
        /// 给bot转钱
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public bool GetPay(string uid, BigInteger price)
        {
            if(ShowBalance(uid) < price)
            {
                return false;
            }
            else
            {
                var res = TransMoney(uid, Config.Instance.BotQQ, price, out _);
                if(res == price)return true;
                else return false;
            }
        }




    }


    #region 遗弃



    /// <summary>
    /// 转账记录
    ///  * 暂时不用
    /// </summary>
    class BankRecord
    {
        public string src;
        public string tar;
        public DateTime time;
        public BigInteger money;
        public string reason;
        public string result;

        public BankRecord()
        {
            src = "";
            tar = "";
            time = DateTime.Now;
            money = 0;
            reason = "";
            result = "";
        }

        public BankRecord(string _src, string _tar, BigInteger _money, string _reason, string _result)
        {
            src = _src;
            tar = _tar;
            money = _money;
            time = DateTime.Now;
            reason = _reason;
            result = _result;
        }

        public BankRecord(string line)
        {
            parse(line);
        }

        public void parse(string line)
        {
            try
            {
                var items = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (items.Length >= 6)
                {
                    src = items[0];
                    tar = items[1];
                    time = DateTime.ParseExact(items[2], "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    money = Util.ConvertToBigInteger(items[3]);
                    reason = items[4];
                    result = items[5];
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public override string ToString()
        {
            return $"{src}\t{tar}\t{time.ToString("yyyy-MM-dd HH:mm:ss")}\t{money}\t{reason}\t{result}";
        }
    }
    
    
    
    #endregion


}
