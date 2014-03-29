using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kanae.Web.Infrastracture.Extension
{
    public static class TimeZoneExtension
    {
        /// <summary>
        /// UTCなDateTimeを設定で指定されたタイムゾーンに調整して返します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToTimeZoneAdjusted(this DateTime value)
        {
            var timeZoneId = System.Configuration.ConfigurationManager.AppSettings["Kanae:DefaultTimeZone"];
            if (String.IsNullOrWhiteSpace(timeZoneId))
            {
                return value;
            }

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(value, TimeZoneInfo.Utc.Id, timeZoneId);
        }
    }
}