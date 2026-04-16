using Kugua.Core.Chinese;
using Kugua.Mods.Base;
using Prophecy;
using Prophecy.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Kugua.Mods.ModTextFunctions
{
    public class ModCalendar : Mod
    {
        public override bool Init(string[] args)
        {
            ModCommands.Add(new ModCommand(new Regex(@"^今夕是何年", RegexOptions.Singleline), handleShowCalendar));
            ModCommands.Add(new ModCommand(new Regex(@"^(.*)公(历|元)(.*)$", RegexOptions.Singleline), handleCheckGeroge));

            ModCommands.Add(new ModCommand(new Regex(@"^蒸汽(.*?)年?$", RegexOptions.Singleline), handleCheckYunhua1));
            ModCommands.Add(new ModCommand(new Regex(@"^创世(.*?)年?$", RegexOptions.Singleline), handleCheckYunhua2));

            return true;
        }

        private string handleCheckGeroge(MessageContext context, string[] param)
        {
            StringBuilder res = new StringBuilder();

            JDateTime jdt = JDateTime.Now;
            var timezone = param[1];
            if (string.IsNullOrWhiteSpace(timezone)) timezone = "北京市";
            var area = LocationInfo.FindCoordinate(timezone.Trim());


            var p = param[3];

            if (!string.IsNullOrWhiteSpace(p))
            {
                if (p.StartsWith("后")) p = p.Substring(1);
                else if (p.StartsWith("前")) p = "-" + p.Substring(1);

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

        private string handleCheckYunhua1(MessageContext context, string[] param)
        {
            string res = "";
            string year = param[1];
            if(year.StartsWith("前")) year = "-" + year.Substring(1);
            if(year.StartsWith("后")) year = year.Substring(1);
            if (int.TryParse(year, out int y))
            {
                res = $"蒸汽{y}年是{YunCalendar.SE2YN(y)}";
            }
            return res;
        }
        private string handleCheckYunhua2(MessageContext context, string[] param)
        {
            string res = "";
            string year = param[1];
            if (year.StartsWith("前")) year = "-" + year.Substring(1);
            if (year.StartsWith("后")) year = year.Substring(1);
            if (int.TryParse(year, out int y))
            {
                res = $"创世{y}年是{YunCalendar.YY2YN(y)}";
            }
            return res;
        }
    }


    public class YunCalendar
    {
        private const int YearsPerTianYuan = 1140; // 天元
        private const int YearsPerDiYuan = 228; // 地元
        private const int YearsPerShi = 19;    // 一世

        // 基准点：S.E. 1 = 创世后 9267 年
        private const int BaseSE = 1;
        private const int BaseTotalYears = 9267;

        public static string SE2YN(int seYear)
        {
            // 计算目标年份距离 S.E. 1 的年数（注意没有 0 年）
            int diff = seYear - BaseSE;
            int targetTotalYears = BaseTotalYears + diff;

            // 计算天元
            int yuan = (targetTotalYears - 1) / YearsPerTianYuan + 1;
            int remainingYears = (targetTotalYears - 1) % YearsPerTianYuan;

            // 计算地元
            int diYuan = (remainingYears / YearsPerDiYuan) + 1;
            

            // 计算世
            int shi = (remainingYears / YearsPerShi) + 1;
            int yearInShi = (remainingYears % YearsPerShi) + 1;

            // 获取干支
            string ganzhi = ChineseCulture.GetGanZhi(shi);
            string yuanstr = ChineseNumber.GetChineseDigital(yuan);
            string diyuanstr = "初上中下末".Substring(diYuan-1,1);
            string diyuanShiStr = ChineseNumber.GetChineseDigital(shi);
            string shistr = ChineseNumber.GetChineseDigital(shi);
            string yearInShistr = ChineseNumber.GetChineseDigital(yearInShi);


            return $"芸历{yuanstr}天元，{diyuanstr}地元， {shistr}世，{yearInShistr}年 " +
                $"（{yuanstr}元{diyuanstr}甲{diyuanShiStr}世）\r\n" +
                $"{yuanstr}元{ganzhi}{yearInShistr}年\r\n" +
                $"创后{targetTotalYears}年";
        }
        public static string YY2YN(int yyYear)
        {
            // 计算云历创世迄今年数（没有 0 年）
            int targetTotalYears = yyYear;

            // 计算天元
            int yuan = (targetTotalYears - 1) / YearsPerTianYuan + 1;
            int remainingYears = (targetTotalYears - 1) % YearsPerTianYuan;

            // 计算地元
            int diYuan = (remainingYears / YearsPerDiYuan) + 1;


            // 计算世
            int shi = (remainingYears / YearsPerShi) + 1;
            int yearInShi = (remainingYears % YearsPerShi) + 1;

            // 获取干支
            string ganzhi = ChineseCulture.GetGanZhi(shi);
            string yuanstr = ChineseNumber.GetChineseDigital(yuan);
            string diyuanstr = "初上中下末".Substring(diYuan - 1, 1);
            string diyuanShiStr = ChineseNumber.GetChineseDigital(shi);
            string shistr = ChineseNumber.GetChineseDigital(shi);
            string yearInShistr = ChineseNumber.GetChineseDigital(yearInShi);


            return $"芸历{yuanstr}天元，{diyuanstr}地元， {shistr}世，{yearInShistr}年 " +
                $"（{yuanstr}元{diyuanstr}甲{diyuanShiStr}世）\r\n" +
                $"{yuanstr}元{ganzhi}{yearInShistr}年\r\n" +
                $"蒸汽{targetTotalYears - BaseTotalYears + 1}年";
        }
    }

}
