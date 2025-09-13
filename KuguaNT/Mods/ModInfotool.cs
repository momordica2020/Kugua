using Kugua.Core;
using Kugua.Generators;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Kugua.Mods
{
    /// <summary>
    /// 一些额外功能
    /// </summary>
    public class ModInfotool : Mod
    {
        QQWry qqwry;
        //IpLocation ipLocation;


        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^开(.+)"), checkID));
            ModCommands.Add(new ModCommand(new Regex(@"^查IP(.+)"), checkIP));
            ModCommands.Add(new ModCommand(new Regex(@"^0[xX]([0-9A-Fa-f]+)$"), convertHex, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^([0-9]+)$"), convertToHex, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^([0-9]+)([bB])$"), convertToByteNum, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^(.+)=$"), calculate, _needAsk: false));

            try
            {
                qqwry = new QQWry(Config.Instance.FullPath("qqwry.dat"));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }


            return true;
        }
        

        /// <summary>
        /// 计算数学公式
        /// 1+1=
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string calculate(MessageContext context, string[] param)
        {
            try
            {
                string expr = param[1];
                if (string.IsNullOrWhiteSpace(expr)) return "";
                Calculator cal = new Calculator(expr);
                return $"{param[1]}={cal.Evaluate()}";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }


        // 字节数换算
        private string convertToByteNum(MessageContext context, string[] param)
        {
           // BigInteger.Parse("1099511627776")
            try
            {
                if (context.IsAskme) return "";
                BigInteger data = BigInteger.Parse(param[1]);
                string unit = param[2];
                string res = $"{data}{unit}";
                if (data <1024* 1024) res += $"= {(double)data / 1024:0.00}K{unit}";
                if (data >= 1024 * 1024) res += $"= {(double)data / (1024 * 1024):0.00}M{unit}";
                if (data >= 1024 * 1024 * 1024) res += $"= {(double)data / (1024 * 1024 * 1024):0.00}G{unit}";
                if (data >= BigInteger.Parse("1099511627776")) res += $"= {Util.BigDivToString(data, BigInteger.Parse("1099511627776"),2)}T{unit}";
                if (data >= BigInteger.Parse("1125899906842624")) res += $"= {Util.BigDivToString(data, BigInteger.Parse("1125899906842624"), 2)}P{unit}";
                return res;
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

        private string convertToHex(MessageContext context, string[] param)
        {
            try
            {
                if (context.IsGroup && context.Group.Is("测试")&& context.IsAdminUser && !context.IsAskme)
                {
                    string numString = param[1];
                    if (string.IsNullOrWhiteSpace(numString)) return "";
                    long decimalValue = Convert.ToInt64(numString, 10);
                    if (decimalValue != 0) return $"{param[0]} = 0x{Convert.ToString(decimalValue, 16)}";
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

        private string convertHex(MessageContext context, string[] param)
        {
            try
            {
                if(context.IsPrivate || (context.IsGroup &&context.Group.Is("测试")&&!context.IsAskme))
                {
                    string hexString = param[1];
                    if (string.IsNullOrWhiteSpace(hexString)) return "";
                    long decimalValue = Convert.ToInt64(hexString, 16);
                    if (decimalValue != 0) return $"{param[0]} = {decimalValue.ToString()}";
                }

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

        /// <summary>
        /// 输入ip地址查属地
        /// 查IP 192.168.1.1
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string checkIP(MessageContext context, string[] param)
        {
            var ipstr = param[1].Trim();
            try
            {
                string ipcheck = $"https://ip.dnomd343.top/info/{ipstr}";
                var dd = Network.Get(ipcheck);
                if (dd != null)
                {
                    var jo = JsonObject.Parse(dd.ToString());
                    string res = $"{jo["detail"]} ({jo["loc"]})";
                    return res;
                }

                else
                {
                    // failed. use local qqwry.day
                    var info = qqwry.find_info(ipstr);
                    return $"{info.Item1}\n{info.Item2}\n{info.Item3}";
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            //var res = ipLocation.Find(ipstr);
            //if (res != null && res.Length > 0)
            //{

            //    string str = string.Join(",", res);
            //    return str;
            //    //return null;
            //}
            return "";
        }



        /// <summary>
        /// 随即开合
        /// 开我/ 开287859992
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string checkID(MessageContext context, string[] param)
        {
            var userName = param[1].Trim();
            try
            {
                return IDGenerator.Get(userName);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

    }
}
