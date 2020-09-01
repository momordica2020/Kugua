using MMDK.Core;
using MMDK.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMDK.Plugins
{
    class BankPlugin : Plugin
    {
        MoneyManager btc;
        public BankPlugin(): base("Bank")
        {

        }
        protected override void InitSource()
        {
            btc = BOT.getMoneyManager();
        }


        public override bool HandleMessage(Message msg)
        {
            string cmd = BOT.getAskCmd(msg);
            if (string.IsNullOrWhiteSpace(cmd)) return false;

            // BTC货币系统
            if (msg.fromGroup>0 && cmd == "签到")
            {
                msg.str = btc.dailyAttendance(msg.fromGroup, msg.from);
                //racehorse.dailyAttendance(group, user);
                BOT.sendBack(msg, true);
                return true;
            }

            //if (cmd == "个人信息")
            //{
            //    string res = $"{btc.getUserInfo(msg.from)}\r\n{getRHInfo(msg.from)}";
            //    if (!string.IsNullOrWhiteSpace(res))
            //    {
            //        msg.str = res;
            //        BOT.sendBack(msg);
            //        return false;
            //    }

            //}

            Regex zzs = new Regex("给(.+)转(\\d+)");
            var matchzzs = zzs.Match(cmd);
            if (matchzzs.Success)
            {
                try
                {
                    string target = matchzzs.Groups[1].ToString().Trim();
                    long targetqq = -1;
                    if (!long.TryParse(target, out targetqq))
                    {
                        // nick name -> qq
                        targetqq = BOT.getID(target, msg.fromGroup);
                        // targetqq = getQQNumFromGroup(group, target.Trim());
                    }
                    string res = "";
                    if (targetqq <= 0)
                    {
                        res = $"群里好像没人叫 {target} ，转账失败。";
                    }
                    else
                    {
                        long money = long.Parse(matchzzs.Groups[2].ToString());
                        res = btc.transMoney(msg.from, targetqq, money);

                    }
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        msg.str = res;
                        BOT.sendBack(msg, true);
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }


        public override void Dispose()
        {
            
        }

    }
}
