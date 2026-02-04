using System;

namespace NoticeModule.Models
{
    /// <summary>
    /// 游戏公告
    /// </summary>
    public class GameNotice
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string NoticeType { get; set; }
        public int Priority { get; set; }
        public string Color { get; set; }
        public string TargetMap { get; set; }
        public int TargetLevel { get; set; }
        public int TargetVip { get; set; }
        public int RepeatCount { get; set; }
        public int RepeatInterval { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ScheduleCron { get; set; }
        public DateTime? LastSentTime { get; set; }
        public int SentCount { get; set; }
        public bool IsEnabled { get; set; }
        public int? AdminId { get; set; }
        public string AdminName { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 公告类型
    /// </summary>
    public static class NoticeTypes
    {
        /// <summary>
        /// 登录公告 - 玩家登录时显示
        /// </summary>
        public const string Login = "login";
        
        /// <summary>
        /// 全服广播 - 立即发送给所有在线玩家
        /// </summary>
        public const string Broadcast = "broadcast";
        
        /// <summary>
        /// 滚动公告 - 定时循环发送
        /// </summary>
        public const string Scroll = "scroll";
        
        /// <summary>
        /// 地图公告 - 发送给指定地图玩家
        /// </summary>
        public const string Map = "map";
        
        /// <summary>
        /// 定时公告 - 按Cron表达式定时发送
        /// </summary>
        public const string Schedule = "schedule";
    }

    /// <summary>
    /// 公告优先级
    /// </summary>
    public static class NoticePriority
    {
        public const int Normal = 0;
        public const int Important = 1;
        public const int Urgent = 2;
    }

    /// <summary>
    /// 公告颜色
    /// </summary>
    public static class NoticeColors
    {
        public const string Red = "red";
        public const string Green = "green";
        public const string Blue = "blue";
        public const string Yellow = "yellow";
        public const string White = "white";

        public static (byte fColor, byte bColor) GetMsgColors(string color)
        {
            return color?.ToLower() switch
            {
                "red" => (249, 0),
                "green" => (1, 0),
                "blue" => (6, 0),
                "yellow" => (252, 0),
                "white" => (255, 0),
                _ => (252, 0)
            };
        }
    }

    /// <summary>
    /// 发送记录
    /// </summary>
    public class NoticeSendLog
    {
        public long Id { get; set; }
        public int NoticeId { get; set; }
        public string NoticeType { get; set; }
        public int TargetCount { get; set; }
        public string Content { get; set; }
        public DateTime SendTime { get; set; }
    }
}
