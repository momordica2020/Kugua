using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDK.Util
{
    /// <summary>
    /// 全局功能
    /// </summary>
    class UtilHelper
    {
        #region 转换时间为unix时间戳
        /// <summary>
        /// 转换时间为unix时间戳
        /// </summary>
        /// <param name="date">需要传递UTC时间,避免时区误差,例:DataTime.UTCNow</param>
        /// <returns></returns>
        public static double toTimestamp(DateTime date)
        {
            try
            {
                DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                TimeSpan diff = date - dateTimeStart;
                return Math.Floor(diff.TotalSeconds);
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
            return 0;
        }
        #endregion

        #region 时间戳转换为时间

        public static DateTime toDateTime(string timeStamp)
        {
            try
            {
                DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(timeStamp + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dateTimeStart.Add(toNow);
            }
            catch (Exception ex)
            {
                FileHelper.Log(ex);
            }
            return DateTime.Now;
        }

        #endregion

    }
}
