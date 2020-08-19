using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Common
{
    public class TimeUtil
    {
        public static DateTime UnitTimestampToDateTime(double timeStampMilliseconds)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long ticks = (long)timeStampMilliseconds * 10000;
            TimeSpan toNow = new TimeSpan(ticks);
            return dtStart.Add(toNow);
        }
        public static long GetLocalUnixTimestamp()
        {
            DateTime dtStart = new DateTime(1970, 1, 1);
            return Convert.ToInt64((DateTime.Now - dtStart).TotalSeconds * 1000);
        }
        public static long GetLocalUnixTimestamp(DateTime time)
        {
            DateTime dtStart = new DateTime(1970, 1, 1);
            return Convert.ToInt64((time - dtStart).TotalSeconds * 1000);
        }
    }
}
