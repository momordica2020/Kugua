using Kugua.Core;
using Kugua.Integrations.Generators;
using Kugua.Integrations.NTBot;
using Kugua.Mods.Base;
using System.Numerics;
using System.Text;
using System.Text.Json;
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
            ModCommands.Add(new ModCommand(new Regex(@"^开[∶|:|：|\s](.+)"), checkID));
            ModCommands.Add(new ModCommand(new Regex(@"^查IP(.+)"), checkIP));
            ModCommands.Add(new ModCommand(new Regex(@"^0[xX]([0-9A-Fa-f]+)$"), convertHex, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^([0-9]+)$"), convertToHex, _needAsk: true));
            ModCommands.Add(new ModCommand(new Regex(@"^([0-9]+)([bB])$"), convertToByteNum, _needAsk: true));
            ModCommands.Add(new ModCommand(new Regex(@"^(.+)=$"), calculate, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^url=(.+)$"), UrlDecode, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^tourl=(.+)$"), UrlEncode, _needAsk: false));
            ModCommands.Add(new ModCommand(new Regex(@"^加群[∶|:|：|\s]\s*(\S+)$"), AddGroupUrl));
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
        /// 随即开合（私聊用）
        /// 开 龚诗峰/ 开 287859992
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string checkID(MessageContext context, string[] param)
        {
            if (!context.IsAdminGroup && context.IsGroup) return "";
            var userName = param[1].Trim();
            if (string.IsNullOrWhiteSpace(userName) || Util.ContainsSymbol(userName)) return "";
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









        /// <summary>
        /// URL编码转正常字符
        /// url=%2Findex.html%3FfromNormal
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string UrlDecode(MessageContext context, string[] param)
        {
            try
            {
                string data = param[1];
                if (!string.IsNullOrWhiteSpace(data))
                {
                    return System.Web.HttpUtility.UrlDecode(data);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

        /// <summary>
        /// 转URL编码
        /// tourl=?index哈哈
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string UrlEncode(MessageContext context, string[] param)
        {
            try
            {
                string data = param[1];
                if (!string.IsNullOrWhiteSpace(data))
                {
                    return System.Web.HttpUtility.UrlEncode(data);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }


        /// <summary>
        /// 根据群号获取直连链接
        /// 加群：123456789
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string AddGroupUrl(MessageContext context, string[] param)
        {
            try
            {
                string groupid = param[1];
                if (!string.IsNullOrWhiteSpace(groupid))
                {
                    string url = GenerateQQUniversalShareLink(groupid);
                    string imgbase64 = QRCodeHelper.GenerateQRCodeBase64(url);
                    _=context.SendBack([new ImageSend( $"base64://{imgbase64}")]);
                    return url;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return "";
        }

        private const string FixedAuthKey = "9B7z1dmkREwBtmRDFDe29EK6XI7ToY%2Bigk1BindaGSVRw2qznkhY/BFItbkJek31";//"QkVDJY2vPGGNcu8VVXj6EBwCUC%2FlkOHxFqUT%2FnxBUYS5gLACzwxAC7hJX96EDRtm";
        private const string FixedData = "JJi_QZzvlFpVf7iNtTH53TDNWXL8tjp-KSuCZC74fyYVbz0WD2NjU_qCOWe4H1l0McW6xDj6yufY3cmhenn4Rpms5ZvoMIOvMDqjFfG8_Bk";//"5myO448r2NFS0ttBRy6BfK6Mg2ImkXGH6tUf5poRiqqntiZFt4x6lrOTWMegI4U4Rx_g0nmiaBbUyRtHIr-sVA";
        private const string BusiToken = "6L31oYBW4jUAI1zGW1Dm2TVHB2nlNw/Qzg5DIbb3gPiBke3/J1l/mzkImIyhKWfj";
        /// <summary>
        /// 根据群号生成 QQ 群万能直加链接（关闭搜索也能点开加入）
        /// </summary>
        /// <param name="groupCode">QQ群号，支持 string 或 long</param>
        /// <returns>完整的可直接点开的分享链接</returns>
        public static string GenerateQQUniversalShareLink(string groupCode)
        {
            if (string.IsNullOrWhiteSpace(groupCode))
                throw new ArgumentException("群号不能为空");

            groupCode = groupCode.Trim();

            // busi 原始 JSON 结构（只改 groupCode 部分
            var busiJsonObj = new
            {
                groupCode = groupCode,
                token = BusiToken,//"RXZU7+26lmMyRK+Ob/FY/q7Cgn0g0geUMXbPxa2ypWM2txalTWLVS Ea+br1pRcap1",
                uin = "1627126029"//"287859992"
            };
            string busiJson = JsonSerializer.Serialize(busiJsonObj, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            string busiDataBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(busiJson));
            string busiDataEncoded = Uri.EscapeDataString(busiDataBase64);

            // 拼接完整链接
            string url = $"https://qun.qq.com/universal-share/share?" +
                         //$"ac=1" +
                         //$"&authKey={FixedAuthKey}" +
                         $"busi_data={busiDataEncoded}" +
                         //$"&data={FixedData}" +
                         //$"&svctype=5" +
                         $"&tempid=h5_group_info";

            return url;
        }


    }
}
