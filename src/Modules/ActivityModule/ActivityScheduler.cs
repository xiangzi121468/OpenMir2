using System.Timers;
using ActivityModule.Models;
using NLog;
using Timer = System.Timers.Timer;

namespace ActivityModule
{
    /// <summary>
    /// 活动调度器 - 负责定时检查和触发活动
    /// </summary>
    public class ActivityScheduler : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly IActivityService _activityService;
        private readonly Timer _checkTimer;
        private readonly Timer _cleanupTimer;
        private bool _isRunning = false;
        private DateTime _serverStartTime;

        // 活动处理器委托
        public delegate Task ActivityHandler(GameActivity activity, bool isStart);
        
        /// <summary>
        /// 活动开始事件
        /// </summary>
        public event ActivityHandler? OnActivityStart;
        
        /// <summary>
        /// 活动结束事件
        /// </summary>
        public event ActivityHandler? OnActivityEnd;

        /// <summary>
        /// BOSS刷新事件
        /// </summary>
        public event Func<string, string, int, int, int, Task>? OnBossSpawn;

        /// <summary>
        /// 全服广播事件
        /// </summary>
        public event Action<string, int>? OnBroadcast;

        /// <summary>
        /// 天降物品事件
        /// </summary>
        public event Func<string, string[], int, Task>? OnGiftRain;

        /// <summary>
        /// NPC刷新事件
        /// </summary>
        public event Func<string, string, int, int, Task>? OnNpcSpawn;

        public ActivityScheduler(IActivityService activityService)
        {
            _activityService = activityService;
            _serverStartTime = DateTime.Now;

            // 每30秒检查一次活动状态
            _checkTimer = new Timer(30000);
            _checkTimer.Elapsed += OnCheckTimerElapsed;

            // 每5分钟清理过期活动
            _cleanupTimer = new Timer(300000);
            _cleanupTimer.Elapsed += OnCleanupTimerElapsed;
        }

        /// <summary>
        /// 异步初始化调度器
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // 刷新活动缓存
                await _activityService.RefreshCacheAsync();
                Logger.Info("活动调度器初始化完成");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "活动调度器初始化失败");
            }
        }
        
        /// <summary>
        /// 手动处理一次活动检查（供外部定时调用）
        /// </summary>
        public async Task ProcessAsync()
        {
            if (!_isRunning)
                return;
                
            await CheckAndTriggerActivitiesAsync();
        }

        /// <summary>
        /// 启动调度器
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _serverStartTime = DateTime.Now;
            _checkTimer.Start();
            _cleanupTimer.Start();

            Logger.Info("活动调度器已启动");

            // 立即执行一次检查
            Task.Run(CheckAndTriggerActivitiesAsync);
        }

        /// <summary>
        /// 停止调度器
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _checkTimer.Stop();
            _cleanupTimer.Stop();

            // 停止所有运行中的活动
            var runningActivities = _activityService.GetRunningActivities();
            foreach (var activity in runningActivities)
            {
                _activityService.StopActivityAsync(activity.ActivityId).Wait();
            }

            Logger.Info("活动调度器已停止");
        }

        private async void OnCheckTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_isRunning)
                return;

            try
            {
                await CheckAndTriggerActivitiesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "活动检查异常");
            }
        }

        private async void OnCleanupTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                await CleanupExpiredActivitiesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "活动清理异常");
            }
        }

        /// <summary>
        /// 检查并触发活动
        /// </summary>
        private async Task CheckAndTriggerActivitiesAsync()
        {
            var now = DateTime.Now;
            var activities = await _activityService.GetAllActivitiesAsync();

            foreach (var activity in activities.Where(a => a.IsEnabled))
            {
                try
                {
                    // 检查是否需要停止
                    if (activity.IsRunning || _activityService.IsActivityRunning(activity.Id))
                    {
                        var runtime = _activityService.GetRunningActivities()
                            .FirstOrDefault(r => r.ActivityId == activity.Id);

                        if (runtime != null && now >= runtime.EndTime)
                        {
                            await StopActivityWithHandler(activity);
                        }
                        continue;
                    }

                    // 检查是否需要启动
                    if (ShouldStartActivity(activity, now))
                    {
                        await StartActivityWithHandler(activity);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"处理活动异常: {activity.Name}");
                }
            }
        }

        /// <summary>
        /// 判断是否应该启动活动
        /// </summary>
        private bool ShouldStartActivity(GameActivity activity, DateTime now)
        {
            var currentTime = now.TimeOfDay;
            var minutesSinceStart = (now - _serverStartTime).TotalMinutes;

            switch (activity.ScheduleType.ToLower())
            {
                case "once":
                    // 一次性活动，检查是否已运行过
                    return activity.TotalRuns == 0 && activity.IsInTimeRange();

                case "daily":
                    // 每日活动，检查时间段
                    if (!activity.StartTime.HasValue)
                        return false;

                    var startTime = activity.StartTime.Value;
                    var endTime = startTime.Add(TimeSpan.FromMinutes(activity.DurationMinutes));

                    // 检查是否在时间范围内且今天未运行
                    if (currentTime >= startTime && currentTime <= endTime)
                    {
                        // 检查今天是否已经运行过
                        if (activity.LastRunTime.HasValue && activity.LastRunTime.Value.Date == now.Date)
                            return false;
                        return true;
                    }
                    return false;

                case "weekly":
                    // 每周活动，检查星期和时间
                    var dayOfWeek = (int)now.DayOfWeek;
                    if (dayOfWeek == 0) dayOfWeek = 7;

                    var validDays = activity.WeekDays.Split(',');
                    if (!validDays.Contains(dayOfWeek.ToString()))
                        return false;

                    return ShouldStartActivity(new GameActivity
                    {
                        ScheduleType = "daily",
                        StartTime = activity.StartTime,
                        DurationMinutes = activity.DurationMinutes,
                        TotalRuns = activity.TotalRuns,
                        LastRunTime = activity.LastRunTime
                    }, now);

                case "interval":
                    // 间隔活动，检查距离上次运行或开服的时间
                    if (!int.TryParse(activity.ScheduleRule, out var intervalMinutes))
                        return false;

                    if (activity.LastRunTime.HasValue)
                    {
                        var minutesSinceLastRun = (now - activity.LastRunTime.Value).TotalMinutes;
                        return minutesSinceLastRun >= intervalMinutes;
                    }
                    else
                    {
                        // 从开服开始计算
                        return minutesSinceStart >= intervalMinutes;
                    }

                case "cron":
                    // Cron表达式 (简化实现，支持 分 时 日 月 周)
                    return MatchCronExpression(activity.ScheduleRule, now) && 
                           (activity.LastRunTime == null || 
                            (now - activity.LastRunTime.Value).TotalMinutes >= 1);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 简化的Cron表达式匹配
        /// </summary>
        private bool MatchCronExpression(string? cronExpr, DateTime now)
        {
            if (string.IsNullOrEmpty(cronExpr))
                return false;

            try
            {
                var parts = cronExpr.Split(' ');
                if (parts.Length < 5)
                    return false;

                // 分 时 日 月 周
                return MatchCronPart(parts[0], now.Minute) &&
                       MatchCronPart(parts[1], now.Hour) &&
                       MatchCronPart(parts[2], now.Day) &&
                       MatchCronPart(parts[3], now.Month) &&
                       MatchCronPart(parts[4], (int)now.DayOfWeek == 0 ? 7 : (int)now.DayOfWeek);
            }
            catch
            {
                return false;
            }
        }

        private bool MatchCronPart(string pattern, int value)
        {
            if (pattern == "*")
                return true;

            if (pattern.Contains(","))
            {
                return pattern.Split(',').Select(int.Parse).Contains(value);
            }

            if (pattern.Contains("-"))
            {
                var range = pattern.Split('-');
                var start = int.Parse(range[0]);
                var end = int.Parse(range[1]);
                return value >= start && value <= end;
            }

            if (pattern.Contains("/"))
            {
                var step = pattern.Split('/');
                var interval = int.Parse(step[1]);
                return value % interval == 0;
            }

            return int.TryParse(pattern, out var exact) && exact == value;
        }

        /// <summary>
        /// 启动活动并触发处理器
        /// </summary>
        private async Task StartActivityWithHandler(GameActivity activity)
        {
            var success = await _activityService.StartActivityAsync(activity.Id);
            if (!success)
                return;

            Logger.Info($"活动自动启动: {activity.Name} ({activity.ActivityType})");

            // 触发事件
            try
            {
                OnActivityStart?.Invoke(activity, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "活动启动事件处理异常");
            }

            // 执行活动特定逻辑
            await ExecuteActivityLogic(activity, true);

            // 发送广播
            var message = $"【{activity.Name}】活动已开启！{activity.Description}";
            OnBroadcast?.Invoke(message, 0); // 0=Green
        }

        /// <summary>
        /// 停止活动并触发处理器
        /// </summary>
        private async Task StopActivityWithHandler(GameActivity activity)
        {
            await _activityService.StopActivityAsync(activity.Id);

            Logger.Info($"活动自动结束: {activity.Name}");

            // 触发事件
            try
            {
                OnActivityEnd?.Invoke(activity, false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "活动结束事件处理异常");
            }

            // 发送广播
            var message = $"【{activity.Name}】活动已结束，感谢参与！";
            OnBroadcast?.Invoke(message, 2); // 2=Yellow
        }

        /// <summary>
        /// 执行活动特定逻辑
        /// </summary>
        private async Task ExecuteActivityLogic(GameActivity activity, bool isStart)
        {
            if (!isStart)
                return;

            try
            {
                switch (activity.ActivityType.ToLower())
                {
                    case "boss_spawn":
                        await HandleBossSpawnActivity(activity);
                        break;

                    case "gift_rain":
                        await HandleGiftRainActivity(activity);
                        break;

                    case "merchant":
                        await HandleMerchantActivity(activity);
                        break;

                    case "exp_boost":
                    case "drop_boost":
                    case "party":
                        // 这些活动效果由ActivityService自动处理倍率
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"执行活动逻辑异常: {activity.Name}");
            }
        }

        /// <summary>
        /// 处理BOSS刷新活动
        /// </summary>
        private async Task HandleBossSpawnActivity(GameActivity activity)
        {
            var bossName = activity.GetParam("boss_name", "教主");
            var mapId = activity.GetParam("spawn_map", "3");
            var x = activity.GetParam("spawn_x", 330);
            var y = activity.GetParam("spawn_y", 330);
            var count = activity.GetParam("spawn_count", 1);

            if (OnBossSpawn != null)
            {
                await OnBossSpawn.Invoke(bossName, mapId, x, y, count);
                Logger.Info($"BOSS刷新: {bossName} 在 {mapId}({x},{y}) x{count}");
            }

            // 广播
            OnBroadcast?.Invoke($"【世界BOSS】{bossName}已出现！快去挑战吧！", 1);
        }

        /// <summary>
        /// 处理天降礼物活动
        /// </summary>
        private async Task HandleGiftRainActivity(GameActivity activity)
        {
            var items = activity.GetParam("items", new List<string> { "金条" });
            var count = activity.GetParam("count_per_item", 20);
            var maps = activity.GetParam("maps", new List<string> { "0", "3" });

            if (OnGiftRain != null)
            {
                foreach (var map in maps)
                {
                    var itemsArray = items is List<object> list 
                        ? list.Select(i => i.ToString() ?? "").ToArray()
                        : new[] { items.ToString() ?? "金条" };
                    
                    await OnGiftRain.Invoke(map, itemsArray, count);
                }
                Logger.Info($"天降礼物: {string.Join(",", items)} x{count} 在 {string.Join(",", maps)}");
            }
        }

        /// <summary>
        /// 处理商人活动
        /// </summary>
        private async Task HandleMerchantActivity(GameActivity activity)
        {
            var npcName = activity.GetParam("npc_name", "神秘商人");
            var mapId = activity.GetParam("spawn_map", "3");
            var x = activity.GetParam("spawn_x", 338);
            var y = activity.GetParam("spawn_y", 338);

            if (OnNpcSpawn != null)
            {
                await OnNpcSpawn.Invoke(npcName, mapId, x, y);
                Logger.Info($"商人刷新: {npcName} 在 {mapId}({x},{y})");
            }

            OnBroadcast?.Invoke($"【{npcName}】已来到比奇城，限时高价收购装备！", 1);
        }

        /// <summary>
        /// 清理过期活动数据
        /// </summary>
        private async Task CleanupExpiredActivitiesAsync()
        {
            var runningActivities = _activityService.GetRunningActivities();
            var now = DateTime.Now;

            foreach (var runtime in runningActivities)
            {
                if (now >= runtime.EndTime)
                {
                    var activity = await _activityService.GetActivityByIdAsync(runtime.ActivityId);
                    if (activity != null)
                    {
                        await StopActivityWithHandler(activity);
                    }
                    else
                    {
                        await _activityService.StopActivityAsync(runtime.ActivityId);
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _checkTimer.Dispose();
            _cleanupTimer.Dispose();
        }
    }
}
