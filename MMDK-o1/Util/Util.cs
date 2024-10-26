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
    class Util
    {


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

        #endregion

    }
}
