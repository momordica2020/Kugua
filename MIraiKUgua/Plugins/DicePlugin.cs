using MMDK.Core;
using MMDK.Struct;
using MMDK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XmlConfiguration;

namespace MMDK.Plugins
{
    class DicePlugin : Plugin
    {
        Random rand = new Random();
        public DicePlugin() : base("Dice")
        {

        }

        public override void InitSource()
        {
            try
            {
                
            }
            catch(Exception ex)
            {
                FileHelper.Log(ex);
            }
        }

        public override void HandleMessage(Message msg)
        {
            //msg.toGroup = 735545947;// msg.fromGroup;
            string cmd = BOT.getAskCmd(msg);
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                string res = getRollString(msg.str.Substring(2));
                if (!string.IsNullOrWhiteSpace(res))
                {
                    msg.str = res;
                    msg.toGroup = msg.fromGroup;
                    msg.to = msg.from;
                    msg.imgs.Clear();
                    msg.ats.Clear();
                    if (msg.fromGroup > 0)
                    {
                        msg.ats.Add(new MessageAt(msg.from, $"@{msg.fromName}"));
                        //msg.str = " " + msg.str;
                    }
                    BOT.send(msg);
                }

            }
        }


        public string getRollString(string cmd)
        {
            Regex reg = new Regex(@"^r(\d*)?d(\d*)?(.*)?$");
            var result = reg.Match(cmd);
            if (result.Success)
            {
                int dicenum = 1;
                int facenum = 100;
                string desc = "";
                try
                {
                    if (result.Groups.Count == 4)
                    {
                        try
                        {
                            dicenum = int.Parse(result.Groups[1].ToString());
                            if (dicenum > 100) dicenum = 100;
                        }
                        catch { }
                        try
                        {
                            facenum = int.Parse(result.Groups[2].ToString());
                        }
                        catch { }
                        try
                        {
                            desc = result.Groups[3].ToString();
                        }
                        catch { }
                    }
                }
                catch { }
                string resdesc = "";
                long res = getRoll(facenum, dicenum, out resdesc);
                return $"{desc} {dicenum}d{facenum} = {resdesc}";
            }
            return "";
        }

        public long getRoll(int faceNum, int DiceNum, out string resdesc)
        {
            long res = 0;
            List<long> ress = new List<long>();
            for (int i = 0; i < DiceNum; i++)
            {
                ress.Add(faceNum > 1 ? rand.Next(faceNum) + 1 : 1);
            }
            res = ress.Sum();
            if (DiceNum == 1) resdesc = $"{res}";
            else resdesc = $"{string.Join("+", ress)} = {res}";
            return res;
        }
    }
}
