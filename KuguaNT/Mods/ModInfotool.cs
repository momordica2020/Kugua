using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Kugua
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

            ModCommands.Add(new ModCommand(new Regex(@"^查IP(.+)"), checkIP));


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


    }
}
