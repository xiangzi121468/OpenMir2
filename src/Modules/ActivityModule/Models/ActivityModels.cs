using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ActivityModule.Models
{
    /// <summary>
    /// 活动类型枚举
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// 经验加成
        /// </summary>
        ExpBoost,
        /// <summary>
        /// 掉落加成
        /// </summary>
        DropBoost,
        /// <summary>
        /// BOSS刷新
        /// </summary>
        BossSpawn,
        /// <summary>
        /// 天降礼物
        /// </summary>
        GiftRain,
        /// <summary>
        /// 特殊商人
        /// </summary>
        Merchant,
        /// <summary>
        /// 派对活动
        /// </summary>
        Party,
        /// <summary>
        /// 在线奖励
        /// </summary>
        OnlineReward,
        /// <summary>
        /// 首杀奖励
        /// </summary>
        FirstKill,
        /// <summary>
        /// 自定义脚本
        /// </summary>
        CustomScript
    }

    /// <summary>
    /// 调度类型枚举
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// 一次性
        /// </summary>
        Once,
        /// <summary>
        /// 每日
        /// </summary>
        Daily,
        /// <summary>
        /// 每周
        /// </summary>
        Weekly,
        /// <summary>
        /// 间隔(分钟)
        /// </summary>
        Interval,
        /// <summary>
        /// Cron表达式
        /// </summary>
        Cron
    }

    /// <summary>
    /// 活动状态
    /// </summary>
    public enum ActivityStatus
    {
        Disabled = 0,
        Enabled = 1,
        Running = 2,
        Paused = 3
    }

    /// <summary>
    /// 游戏活动配置
    /// </summary>
    public class GameActivity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ActivityType { get; set; } = "exp_boost";
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "default";
        public string Color { get; set; } = "#FFD700";

        // 时间配置
        public string ScheduleType { get; set; } = "daily";
        public string? ScheduleRule { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int DurationMinutes { get; set; } = 30;
        public int IntervalMinutes { get; set; } = 0;
        public string WeekDays { get; set; } = "1,2,3,4,5,6,7";

        // 活动参数
        public string? Params { get; set; }

        // 状态
        public bool IsEnabled { get; set; } = true;
        public bool IsRunning { get; set; } = false;
        public int Priority { get; set; } = 100;

        // 限制条件
        public int MinLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 999;
        public string? MapId { get; set; }
        public bool VipOnly { get; set; } = false;

        // 统计
        public int TotalRuns { get; set; } = 0;
        public DateTime? LastRunTime { get; set; }
        public DateTime? NextRunTime { get; set; }

        // 审计
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 解析活动参数
        /// </summary>
        public Dictionary<string, object> GetParams()
        {
            if (string.IsNullOrEmpty(Params))
                return new Dictionary<string, object>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(Params)
                    ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        public T GetParam<T>(string key, T defaultValue)
        {
            var dict = GetParams();
            if (dict.TryGetValue(key, out var value))
            {
                try
                {
                    if (value is JsonElement jsonElement)
                    {
                        return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
                    }
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 检查是否在有效时间范围内
        /// </summary>
        public bool IsInTimeRange()
        {
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            // 检查星期
            if (ScheduleType.Equals("weekly", StringComparison.OrdinalIgnoreCase))
            {
                var dayOfWeek = (int)now.DayOfWeek;
                if (dayOfWeek == 0) dayOfWeek = 7; // 周日转为7
                var validDays = WeekDays.Split(',');
                if (!validDays.Contains(dayOfWeek.ToString()))
                    return false;
            }

            // 检查时间段
            if (StartTime.HasValue)
            {
                if (EndTime.HasValue)
                {
                    return currentTime >= StartTime.Value && currentTime <= EndTime.Value;
                }
                else
                {
                    var endTime = StartTime.Value.Add(TimeSpan.FromMinutes(DurationMinutes));
                    return currentTime >= StartTime.Value && currentTime <= endTime;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// 活动运行日志
    /// </summary>
    public class ActivityLog
    {
        public long Id { get; set; }
        public int ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public string? ActivityType { get; set; }
        public string Action { get; set; } = "start";
        public string Status { get; set; } = "success";
        public string? Message { get; set; }
        public int Participants { get; set; } = 0;
        public int RewardsGiven { get; set; } = 0;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 活动奖励配置
    /// </summary>
    public class ActivityReward
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string RewardType { get; set; } = "item";
        public string RewardValue { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public int Probability { get; set; } = 100;
        public bool IsBroadcast { get; set; } = false;
    }

    /// <summary>
    /// 活动运行时状态
    /// </summary>
    public class ActivityRuntime
    {
        public int ActivityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public int ParticipantCount { get; set; } = 0;
        public Dictionary<string, object> RuntimeData { get; set; } = new();
    }

    #region API Models

    /// <summary>
    /// 活动列表查询请求
    /// </summary>
    public class ActivityQueryRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? ActivityType { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsRunning { get; set; }
        public string? Keyword { get; set; }
    }

    /// <summary>
    /// 活动创建/更新请求
    /// </summary>
    public class ActivitySaveRequest
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ActivityType { get; set; } = "exp_boost";
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string ScheduleType { get; set; } = "daily";
        public string? ScheduleRule { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int DurationMinutes { get; set; } = 30;
        public int IntervalMinutes { get; set; } = 0;
        public string? WeekDays { get; set; }
        public string? Params { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 100;
        public int MinLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 999;
        public string? MapId { get; set; }
        public bool VipOnly { get; set; } = false;
    }

    /// <summary>
    /// 活动状态响应
    /// </summary>
    public class ActivityStatusResponse
    {
        public List<ActivityRuntime> RunningActivities { get; set; } = new();
        public List<GameActivity> UpcomingActivities { get; set; } = new();
        public ActivityStats TodayStats { get; set; } = new();
    }

    /// <summary>
    /// 活动统计
    /// </summary>
    public class ActivityStats
    {
        public int TotalActivities { get; set; }
        public int EnabledActivities { get; set; }
        public int RunningActivities { get; set; }
        public int TodayRuns { get; set; }
        public int TotalParticipants { get; set; }
    }

    #endregion
}
