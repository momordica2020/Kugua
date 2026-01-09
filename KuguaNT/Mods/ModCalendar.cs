using Kugua.Mods.Base;
using Prophecy;
using Prophecy.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Kugua.Mods
{
    public class ModCalendar : Mod
    {
        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^今夕是何年", RegexOptions.Singleline), handleShowCalendar));
            ModCommands.Add(new ModCommand(new Regex(@"^(.*)公历(.*)", RegexOptions.Singleline), handleCheckGeroge));

            return true;
        }

        private string handleCheckGeroge(MessageContext context, string[] param)
        {
            StringBuilder res = new StringBuilder();

            JDateTime jdt = JDateTime.Now;
            var timezone = param[1];
            if (string.IsNullOrWhiteSpace(timezone)) timezone = "北京市";
            var area = LocationInfo.FindCoordinate(timezone.Trim());

            
            var p = param[2];
            
            if (!string.IsNullOrWhiteSpace(p))
            {
                jdt = JDateTime.Parse(p);
            }

            res.AppendLine($"公元{jdt.ToStringGeroge("yyyy年MM月dd日 星期W HH:mm:ss")}{jdt.MoonState.ToString()}");

            res.AppendLine($"回历：{jdt.ToStringIslamic("yyyy年MM月dd日")}");
            //res.AppendLine($"农历：{dt.ToStringLunar("yyyy年MM月dd日 h时m刻")}");
            res.AppendLine($"农历：{jdt.LunarFourPillars.Year.ToString()}{jdt.LunarShengxiao.ToString()}年" +
                $" {(jdt.IsLunarLeapMonth ? "闰" : "")}{jdt.LunarMonthName}月{(jdt.IsLunarBigMonth ? "大" : "小")}" +
                $" {jdt.LunarDayName}日 {jdt.LunarShiKe} ");
            res.AppendLine($"{(jdt.isTodayJieqi ? $"今天{jdt.Jieqi.ToString()}" : $"{jdt.Jieqi.ToString()}已过{Math.Floor(jdt.JieqiBegin)}天")} {jdt.FeastInfo}");
            res.AppendLine($"{ChaodaiInfo.getChaodaiDesc(jdt)}");
            var r = jdt.LunarFourPillars;
            res.AppendLine($"四柱：{r.Year} {r.Month} {r.Day} {r.Hour}");

            var c = jdt.FeastComing();
            //res.AppendLine($"");
            foreach (var cc in c)
            {
                res.AppendLine($"再过{(int)(cc.date.JulianDate0 - jdt.JulianDate + 1)}天：{cc.feast.ToString()}");
            }

            return res.ToString();
        }



        /// <summary>
        /// 今夕是何年？查查今天的黄历（
        /// 今夕是何年
        /// </summary>
        /// <param name="context"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string handleShowCalendar(MessageContext context, string[] param)
        {
            StringBuilder res = new StringBuilder();

            JDateTime dt = JDateTime.UtcNow;

            res.AppendLine($"儒略日：{dt.JulianDate}   {dt.JulianDateFrom2000}");
            res.AppendLine($"公历：{dt.ToStringGeroge("yyyy年MM月dd日 星期W hh:mm:ss")}");

            res.AppendLine($"回历：{dt.ToStringIslamic("yyyy年MM月dd日")}");
            //res.AppendLine($"农历：{dt.ToStringLunar("yyyy年MM月dd日 h时m刻")}");
            res.AppendLine($"农历{dt.LunarFourPillars.Year.ToString()}{dt.LunarShengxiao.ToString()}年 {(dt.IsLunarLeapMonth ? "闰" : "")}{dt.LunarMonthName}月{(dt.IsLunarBigMonth ? "大" : "小")} {dt.LunarDayName}日 {dt.LunarShiKe} {dt.Jieqi.ToString()}已过{dt.JieqiBegin}天{(dt.isTodayJieqi ? "★" : "")}");
            //res.AppendLine($"{ChaodaiInfo.getChaodaiDesc(dt.LunarYear, dt.LunarMonth)}");
            var r = dt.LunarFourPillars;
            res.AppendLine($"四柱（农历）：{r.Year} {r.Month} {r.Day} {r.Hour}");
            var r0 = dt.LunarFourPillars0;
            res.AppendLine($"四柱（节气）：{r0.Year} {r0.Month} {r0.Day} {r0.Hour}");
            return res.ToString();
        }
    }
}
