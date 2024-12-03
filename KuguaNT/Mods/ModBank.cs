using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChatGPT.Net;


namespace Kugua
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

            ModCommands.Add(new ModCommand(new Regex(@"^发币(.+)"), AddBotMoney));
            ModCommands.Add(new ModCommand(new Regex(@"^给(.+)补贴(.+)"), Grant));
            //ModCommands.Add(new ModCommand(new Regex(@"^捐(.+)"),Donate));
            //ModCommands.Add(new ModCommand(new Regex(@"^供养(.+)"), Donate2));
            ModCommands.Add(new ModCommand(new Regex(@"^(富人榜|富豪榜)"), showRichest));

            ModCommands.Add(new ModCommand(new Regex(@"^(穷人榜)"), showPoorest));

            ModCommands.Add(new ModCommand(new Regex(@"^给(.+)转(.+)"), PostMoney));




            return true;
        }

        //private string Donate2(MessageContext context, string[] param)
        //{
            
        //}

        //private string Donate(MessageContext context, string[] param)
        //{
            
        //}


        private string Grant(MessageContext context, string[] param)
        {
            if (!Config.Instance.UserHasAdminAuthority(context.userId)) return "";
            
            
            long targetqq = -1;
            if (!long.TryParse(param[1], out targetqq)) return $"{param[1]}?不认识";
            var money = StaticUtil.ConvertToBigInteger(param[2]);
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
                string target = param[1];
                long targetqq = -1;
                if (!long.TryParse(target, out targetqq))
                {
                    // nick name -> qq
                    //targetqq = bank.getID(target, msg.fromGroup);
                    // targetqq = getQQNumFromGroup(group, target.Trim());
                }
                string res = "";
                if (targetqq <= 0)
                {
                    res = $"系统里找不到昵称 {target} ，转币失败。可以输入qq号码直接转";
                }
                else
                {
                    BigInteger money = StaticUtil.ConvertToBigInteger(param[2]);
                    BigInteger succeedMoney = TransMoney(context.userId, targetqq.ToString(), money, out res);
                }

                if (!string.IsNullOrWhiteSpace(res))
                {
                    return res;
                }
            }
            catch { }
            return "";
        }


        public string AddBotMoney(MessageContext context, string[] param)
        {
            if (Config.Instance.UserHasAdminAuthority(context.userId))
            {
                BigInteger money = StaticUtil.ConvertToBigInteger(param[1]);
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
                answer += $"{u.Name} 新到账 {money}元。{unitName}余额：{u.Money.ToHans()}";
            }
            catch(Exception ex)
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
                int maxmoney = 114;
                int minmoney = 30;
                // success
                BigInteger money = MyRandom.Next(minmoney, maxmoney);
                u.Money += money;
                u.LastSignTime = DateTime.Now;
                u.SignTimes += 1;
                return $"您今日领取失业补助{money}枚{unitName}，现在账上一共{u.Money.ToHans()}枚";
            }
            else
            {
                return $"嗨嗨嗨，今天领过了";
            }
        }



        public string getUserInfo(string userqq)
        {
            var u = Config.Instance.UserInfo(userqq);
            return $"您的账上共有{u.Money.ToHans()}枚{unitName}。共领取失业补助{u.SignTimes}次，今日失业补助{(u.LastSignTime >= DateTime.Today ? "已领取" : "还未领取")}";
        }

        /// <summary>
        /// 查看账户余额
        /// </summary>
        /// <param name="userqq"></param>
        /// <returns></returns>
        public BigInteger GetMoney(string userqq)
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
                var users = Config.Instance.players.Values.Where(p => p.UseTimes > 0).ToList();
                users.Sort((left, right) =>
                {
                    return -1 * left.Money.CompareTo(right.Money);
                });

                sb.Append($"富 豪 榜 (基尼系数{StaticUtil.CalculateGiniCoefficient(users.Select(u=>u.Money).ToList())}) \r\n");
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
                var users = Config.Instance.players.Values.Where(p=>p.UseTimes > 0).ToList();
                users.Sort((left, right) =>
                {
                    return left.Money.CompareTo(right.Money);
                });

                sb.Append($"穷 人 榜 (基尼系数{StaticUtil.CalculateGiniCoefficient(users.Select(u=>u.Money).ToList())})\r\n");
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
                    money = StaticUtil.ConvertToBigInteger(items[3]);
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
