using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace Kugua
{
    /// <summary>
    /// 掷骰模块
    /// </summary>
    public class ModDice : Mod
    {

        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^r(\d*)?d(\d*)?(.*)?$",RegexOptions.Singleline), handleDice));

            return true;
        }


        /// <summary>
        /// 随机掷骰子，可加缘由，可调整骰子个数r和面数d
        /// rd/rd50猝死概率/r3d6扔三个骰
        /// </summary>
        private string handleDice(MessageContext context, string[] param)
        {
            int dicenum = 1;
            int facenum = 100;
            string desc = "";
            try
            {
                if (param.Length == 4)
                {
                    if (int.TryParse(param[1], out dicenum))
                    {
                        
                        
                    }
                    if (int.TryParse(param[2],out facenum))
                    {
                        
                    }
                    dicenum = Math.Min(dicenum, 100);
                    if (dicenum <= 0) dicenum = 1;
                    if (facenum <= 0) facenum = 100;
                    desc = param[3].Trim();
                }
            }
            catch { }
            string resdesc = "";
            long res = getRoll(facenum, dicenum, out resdesc);
            return ($"{desc} {dicenum}d{facenum} = {resdesc}");
        }

        public bool HandleText(long userId, long groupId, string message, List<string> results)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            Regex reg = new Regex(@"^r(\d*)?d(\d*)?(.*)?$");
            var result = reg.Match(message);
            if (result.Success)
            {
                
            }
            return false;
        }














        public long getRoll(int faceNum, int DiceNum, out string resdesc)
        {
            long res = 0;
            List<long> ress = new List<long>();
            for (int i = 0; i < DiceNum; i++)
            {
                ress.Add(faceNum > 1 ? MyRandom.Next(faceNum) + 1 : 1);
            }
            res = ress.Sum();
            if (DiceNum == 1) resdesc = $"{res}";
            else resdesc = $"{string.Join("+", ress)} = {res}";
            return res;
        }
    }


}
