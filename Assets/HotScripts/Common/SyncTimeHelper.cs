using System;
using UnityEngine;

namespace Hot
{
    public class SyncTimeHelper
    {
        public const int OneMinSec = 60;// 一分钟的秒数
        public const int OneHourSec = 60 * OneMinSec;// 一小时的秒数
        public const int OneDaySec = 24 * OneHourSec;// 一天的秒数
        public const int OneWeekSec = 7 * OneDaySec;// 一周的秒数
        public static long LocalUnixTime => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        private const int HOUR_ZONE = 8;//UTC+8服务器时区
        private static double ServerTime = 0; //服务器现在的时间戳
        private static double ValidStartGameTime = 0; //游戏启动的时间
        private readonly static DateTime Date1970 = new DateTime(1970, 1, 1, HOUR_ZONE, 0, 0, DateTimeKind.Utc);
        //同步服务器时间
        public static void Sync(long time, bool isLocal = false)
        {
            ValidStartGameTime = Time.realtimeSinceStartup;
            ServerTime = time;
#if UNITY_EDITOR
            Debug.LogWarning($"{(isLocal ? "临时设置服务器时间" : "设置服务器时间")}：{time} 》 {GetSystemTime()}");
#endif
        }

        //当前服务器时间 UTC8,所以不再需要ToLocalTime
        public static DateTime GetSystemTime()
        {
            return Date1970.AddSeconds(GetSystemTimeSeconds());
        }
        public static long GetSystemTimeSeconds()
        {
            if (ServerTime == 0)
            {
                Sync(LocalUnixTime, true);
            }
            return (long)(ServerTime + (double)(Time.realtimeSinceStartup - ValidStartGameTime));
        }

        /// <summary>
        /// 获取服务器今天开始时间
        /// </summary>
        /// <returns></returns>
        public static long GetTodayBase()
        {
            DateTime now = GetSystemTime();
            //DateTime baseTime = new DateTime(now.Year, now.Month, now.Day, HOUR_ZONE, 0, 0, DateTimeKind.Utc)//服务器使用UTC8
            //long timestamp = (long)(baseTime - Date1970).TotalSeconds - HOUR_ZONE* OneHourSec;//这里相减差值和UTC0一致，所以要转为UTC8的时间，需要额外减去8小时才是UTC8的凌晨
            //简化为
            DateTime baseTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);//服务器使用UTC0 = UTC8的 自带8小时偏移
            long timestamp = (long)(baseTime - Date1970).TotalSeconds;
            return timestamp;
        }


        /// <summary>
        /// 本周开始时间
        /// </summary>
        /// <returns></returns>
        public static long GetWeekBase()
        {
            DateTime now = GetSystemTime();
            //DateTime nowBase = new DateTime(now.Year, now.Month, now.Day, HOUR_ZONE, 0, 0, DateTimeKind.Utc).Sub(8h);
            //简化为
            DateTime nowBase = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);//服务器使用UTC0 = UTC8的 自带8小时偏移

            int dayOfWeek = (int)nowBase.DayOfWeek;
            if (dayOfWeek == 0)
            {
                dayOfWeek = 7;
            }
            DateTime startOfWeek = nowBase.AddDays(-dayOfWeek + 1);
            long timestamp = (long)(startOfWeek - Date1970).TotalSeconds;
            return timestamp;
        }
        /// <summary>
        /// 本月开始时间
        /// </summary>
        /// <returns></returns>
        public static long GetNextMonthBase()
        {
            DateTime now = GetSystemTime();
            //DateTime startOfMonth = new DateTime(now.Year, now.Month, 1, HOUR_ZONE, 0, 0, DateTimeKind.Utc).Sub(8h);
            //简化为
            DateTime startOfMonth = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);//服务器使用UTC0 = UTC8的 自带8小时偏移


            startOfMonth = startOfMonth.AddMonths(1);
            long timestamp = (long)(startOfMonth - Date1970).TotalSeconds;
            return timestamp;
        }
        /// <summary>
        /// 待完成本地化
        /// </summary>
        public static string ConvertToDHMS(int seconds)
        {
            int d = seconds / 3600 / 24;
            int h = (seconds / 3600) % 24;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;
            if (d > 0)
                return string.Format("{0}Days{1}Hours", d.ToString(), h.ToString());
            else if (d <= 0 && h >= 1)
                return string.Format("{0}Hours{1}Minutes", h.ToString(), m.ToString());
            else if (h < 1)
                return string.Format("{0}Minutes{1}Seconds", m.ToString(), s.ToString());
            return string.Empty;
        }
        public static string ConvertToShortDHMS(int seconds, bool showHour = true)
        {
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;
            if (h > 0 || showHour)
                return $"{h:00}:{m:00}:{s:00}";
            else
                return $"{m:00}:{s:00}";
        }
    }
}
