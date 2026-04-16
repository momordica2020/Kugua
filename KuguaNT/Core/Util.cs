using Kugua.Core.Algorithms;
using Kugua.Core.Chinese;
using Microsoft.AspNetCore.Components.Forms;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Kugua.Core
{


    /// <summary>
    /// 全局功能
    /// </summary>
    public static class Util
    {

        public static string ComputeHash(string input)
        {
            try
            {
                byte[] hashBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

                string hashHex = BitConverter.ToString(hashBytes)
                                    .Replace("-", "")
                                    .ToLower(); // 小写格式
                return hashHex;
            }catch(Exception ex)
            {
                Logger.Log(ex);
            }
            return MyRandom.NextString(16);

        }



        #region DateTime时间与unix时间戳互转
        /// <summary>
        /// 将 Unix 时间戳转换为 DateTime
        /// </summary>
        /// <param name="timestamp">Unix 时间戳</param>
        /// <param name="isMilliseconds">是否为毫秒级时间戳，默认是 false（秒级）</param>
        /// <returns>转换后的 DateTime 对象（本地时间）</returns>
        public static DateTime ConvertTimestampToDateTime(long timestamp, bool isMilliseconds = false)
        {
            DateTime dateTime;

            if (isMilliseconds)
            {
                dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            }
            else
            {
                dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }

            // 转换为本地时间
            return dateTime.ToLocalTime();
        }

        /// <summary>
        /// 将 DateTime 转换为 Unix 时间戳
        /// </summary>
        /// <param name="dateTime">需要转换的 DateTime 对象</param>
        /// <param name="toMilliseconds">是否转换为毫秒级时间戳，默认是 false（秒级）</param>
        /// <returns>对应的 Unix 时间戳</returns>
        public static long ConvertDateTimeToTimestamp(DateTime dateTime, bool toMilliseconds = false)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);

            if (toMilliseconds)
            {
                return dateTimeOffset.ToUnixTimeMilliseconds();
            }
            else
            {
                return dateTimeOffset.ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// 打出和python打印bytes一样的字符串结构
        /// 在 Python 中，bytes 对象打印时，会以 b'...' 的格式显示，其中非 ASCII 字符会被转义为 \x 形式的十六进制值，而 ASCII 字符会直接显示为字符。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FormatBytes(byte[] data)
        {
            StringBuilder sb = new StringBuilder("b'");
            foreach (byte b in data)
            {
                if (b >= 32 && b <= 126) // ASCII 可打印字符
                {
                    sb.Append((char)b);
                }
                else // 非 ASCII 字符
                {
                    sb.AppendFormat("\\x{0:x2}", b);
                }
            }
            sb.Append("'");
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 计算基尼系数
        /// </summary>
        /// <param name="incomes"></param>
        /// <returns></returns>
        public static double CalculateGiniCoefficient(List<BigInteger> incomes)
        {
            var sortedIncomes = incomes.OrderBy(income => income).ToList();

            int count = sortedIncomes.Count;
            if (count == 0) return 0.0;

            BigInteger totalIncome = sortedIncomes.Aggregate(BigInteger.Zero, (acc, income) => acc + income);
            if (totalIncome == BigInteger.Zero) return 0.0;

            // Calculate cumulative proportions
            BigInteger cumulativeIncome = BigInteger.Zero;
            BigInteger cumulativeProportionSum = BigInteger.Zero;

            for (int i = 0; i < count; i++)
            {
                cumulativeIncome += sortedIncomes[i];
                cumulativeProportionSum += cumulativeIncome * 2;
            }

            // Gini coefficient formula
            BigInteger totalPopulation = new BigInteger(count);
            BigInteger numerator = totalPopulation * cumulativeProportionSum;
            BigInteger denominator = totalIncome * totalPopulation;

            BigInteger giniNumerator = denominator - numerator;
            double giniCoefficient = (double)giniNumerator / (double)denominator;

            return Math.Round(giniCoefficient, 2);
        }

        /// <summary>
        /// 获取本程序集的编译日期
        /// </summary>
        /// <param name="assembly">目标程序集</param>
        /// <returns>编译日期</returns>
        public static DateTime GetBuildDate()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            // 获取程序集文件的路径
            var filePath = assembly.Location;
            var fileInfo = new FileInfo(filePath);

            // 获取编译日期，文件的最后写入时间
            return fileInfo.LastWriteTime;
        }

        /// <summary>
        /// 获取本地所有代码行数
        /// </summary>
        /// <returns></returns>
        public static List<(string,int)> GetCodeLineNum()
        {
            var res = new List<(string,int)>();
            // string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            string projectDirectory = @"D:\Projects\momordica2020\Kugua\KuguaNT";
            
            string[] csFiles = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

            int totalLines = 0;

            foreach (var file in csFiles)
            {
                int linesInFile = File.ReadAllLines(file).Length;
                res.Add((file, linesInFile));
                totalLines += linesInFile;
            }
            res.Sort((x, y) => string.Compare(x.Item1, y.Item1));
            return res;
        }



        /// <summary>
        /// 根据中文描述时间的字符串，获得日期信息。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime GetDateFromHans(string str)
        {
            DateTime checkDate = DateTime.Now;
            try
            {

                if (str == "今天") checkDate = DateTime.Now;
                else if (str == "昨天") checkDate = DateTime.Now.AddDays(-1);
                else if (str == "前天") checkDate = DateTime.Now.AddDays(-2);
                else if (str == "明天") checkDate = DateTime.Now.AddDays(1);
                else if (str == "后天") checkDate = DateTime.Now.AddDays(2);
                else
                {
                    Match match = Regex.Match(str, @"^(大+)(前天)$");
                    if (match.Success)
                    {
                        string dPart = match.Groups[1].Value;
                        int dCount = dPart.Length;
                        checkDate = DateTime.Now.AddDays(-2 - dCount);
                    }


                    match = Regex.Match(str, @"^(大+)(后天)$");
                    if (match.Success)
                    {
                        string dPart = match.Groups[1].Value;
                        int dCount = dPart.Length;
                        checkDate = DateTime.Now.AddDays(2 + dCount);
                    }

                    match = Regex.Match(str, @"^(\d{1,2}|[一二三四五六七八九十]{1,2})月([一二三四五六七八九十]{1,3}|\d{1,2})(日)?$");
                    if (match.Success)
                    {
                        int m = ChineseNumber.ConvertToNumber(match.Groups[1].Value);
                        int d = ChineseNumber.ConvertToNumber(match.Groups[2].Value);
                        checkDate = new DateTime(DateTime.Today.Year, m, d);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }


            return checkDate;
        }
    }
}
