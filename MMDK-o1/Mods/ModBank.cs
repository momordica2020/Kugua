using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeowMiraiLib;
using MMDK.Util;

namespace MMDK.Mods
{
    /// <summary>
    /// 银行
    /// </summary>
    public class ModBank : Mod
    {
        public static string unitName = "马币";





        public bool Init(string[] args)
        {

            return true;
        }

        public void Exit()
        {
            
        }

        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            message = message.Trim();
            // 货币系统
            if (message == "签到")
            {
                message = DailyAttendance(groupId, userId);
                //racehorse.dailyAttendance(group, user);
                results.Add(message);
                return true;
            }

            if (message == "个人信息")
            {
                string res = $"{getUserInfo(userId)}";
                results.Add(res);
                return true;
            }

            Regex zzs = new Regex("给(.+)转(\\d+)");
            var matchzzs = zzs.Match(message);
            if (matchzzs.Success)
            {
                try
                {
                    string target = matchzzs.Groups[1].ToString().Trim();
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
                        res = $"系统里找不到昵称 {target} ，转账失败。可以输入qq号码直接转";
                    }
                    else
                    {
                        long money = long.Parse(matchzzs.Groups[2].ToString());
                        res = TransMoney(userId, targetqq, money);

                    }
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        results.Add(res);
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }














        /// <summary>
        /// 每日签到，领取低保
        /// </summary>
        /// <param name="group"></param>
        /// <param name="userqq"></param>
        public string DailyAttendance(long group, long userqq)
        {
            var u = Config.Instance.GetPlayerInfo(userqq);
            if (u.LastSignTime < DateTime.Today)
            {
                int maxmoney = 114;
                int minmoney = 30;
                // success
                long money = MyRandom.Next(minmoney, maxmoney);
                u.Money += money;
                u.LastSignTime = DateTime.Now;
                u.SignTimes += 1;
                return $"您今日领取失业补助{money}枚{unitName}，现在账上一共{u.Money}枚";
            }
            else
            {
                return $"嗨嗨嗨，今天领过了";
            }
        }

        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="fromqq"></param>
        /// <param name="targetqq"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public string TransMoney(long fromqq, long targetqq, long money)
        {
            string res = "";

            try
            {
                var user1 = Config.Instance.GetPlayerInfo(fromqq);
                var user2 = Config.Instance.GetPlayerInfo(targetqq);
                if (money <= 0)
                {
                    return $"只允许正向转账";
                }
                if (user1.Money < money)
                {
                    return $"您的余额不足。当前余额{user1.Money}{unitName}";
                }

                res = $"您向{targetqq}发起转账{money}枚{unitName}，";
                long user1oldmoney = user1.Money;
                long user2oldmoney = user2.Money;
                bool succeed = false;
                try
                {

                    checked
                    {
                        user1.Money -= money;
                        user2.Money += money;
                    }
                    succeed = true;
                }
                catch (OverflowException)
                {
                    //Console.WriteLine("转账失败：超出 long 数据范围");
                    string errMsg = $"转账失败：{user1}或{user2}的{ModBank.unitName}溢出，所转数额{money}, 自己钱包有{user1.Money}，目标钱包已有{user2.Money}。";
                    res += errMsg;
                    Logger.Instance.Log(errMsg);
                    user1.Money = user1oldmoney;
                    user2.Money = user2oldmoney;
                    succeed = false;
                }
                if (succeed)
                {
                    //writeRecord(new BankRecord(fromqq, targetqq, realMoney, "转账", realMoney == money ? "成功。"));

                }

                res += $"余额{user1.Money}{unitName}";
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                res += $"银行被橄榄了，你钱没了！请带截图联系bot管理者{Config.Instance.App.Avatar.adminQQ}";
            }
            return res;
        }

        /// <summary>
        /// 富人榜
        /// </summary>
        /// <returns></returns>
        public string showRichest()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int maxnum = 10;
                var users = Config.Instance.players.Values.ToList();
                users.Sort((left, right) =>
                {
                    return -1 * left.Money.CompareTo(right.Money);
                });

                sb.Append("富 豪 榜 \r\n");
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{users[i].Name},{users[i].Money}枚\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return "";
            }
        }

        /// <summary>
        /// 穷人榜
        /// </summary>
        /// <returns></returns>
        public string showPoorest()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int maxnum = 10;
                var users = Config.Instance.players.Values.ToList();
                users.Sort((left, right) =>
                {
                    return left.Money.CompareTo(right.Money);
                });

                sb.Append("穷 人 榜 \r\n");
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{users[i].Name},{users[i].Money}枚\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return "";
            }
        }

        public string getUserInfo(long userqq)
        {
            var u = Config.Instance.GetPlayerInfo(userqq);
            return $"您的账上共有{u.Money}枚{unitName}。共领取失业补助{u.SignTimes}次，今日失业补助{(u.LastSignTime >= DateTime.Today ? "已领取" : "还未领取")}";
        }

    }


    class BankRecord
    {
        public long src;
        public long tar;
        public DateTime time;
        public long money;
        public string reason;
        public string result;

        public BankRecord()
        {
            src = -1;
            tar = -1;
            time = DateTime.Now;
            money = 0;
            reason = "";
            result = "";
        }

        public BankRecord(long _src, long _tar, long _money, string _reason, string _result)
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
                var items = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 6)
                {
                    src = long.Parse(items[0]);
                    tar = long.Parse(items[1]);
                    time = DateTime.ParseExact(items[2], "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    money = long.Parse(items[3]);
                    reason = items[4];
                    result = items[5];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        public override string ToString()
        {
            return $"{src}\t{tar}\t{time.ToString("yyyy-MM-dd HH:mm:ss")}\t{money}\t{reason}\t{result}";
        }
    }
}
