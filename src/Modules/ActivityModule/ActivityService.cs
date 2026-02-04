using System.Collections.Concurrent;
using System.Text.Json;
using ActivityModule.Models;
using MySqlConnector;
using NLog;

namespace ActivityModule
{
    /// <summary>
    /// 活动服务实现
    /// </summary>
    public class ActivityService : IActivityService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        // 运行时状态
        private readonly ConcurrentDictionary<int, ActivityRuntime> _runningActivities = new();
        private readonly object _lockObj = new();

        // 当前倍率缓存
        private double _currentExpRate = 1.0;
        private double _currentDropRate = 1.0;

        public ActivityService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 活动配置管理

        public async Task<List<GameActivity>> GetAllActivitiesAsync()
        {
            var activities = new List<GameActivity>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT * FROM game_activities ORDER BY priority, id", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    activities.Add(MapActivity(reader));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取活动列表失败");
            }
            return activities;
        }

        public async Task<(List<GameActivity> Items, int Total)> QueryActivitiesAsync(ActivityQueryRequest request)
        {
            var activities = new List<GameActivity>();
            int total = 0;

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var whereClauses = new List<string>();
                var parameters = new List<MySqlParameter>();

                if (!string.IsNullOrEmpty(request.ActivityType))
                {
                    whereClauses.Add("activity_type = @type");
                    parameters.Add(new MySqlParameter("@type", request.ActivityType));
                }

                if (request.IsEnabled.HasValue)
                {
                    whereClauses.Add("is_enabled = @enabled");
                    parameters.Add(new MySqlParameter("@enabled", request.IsEnabled.Value));
                }

                if (request.IsRunning.HasValue)
                {
                    whereClauses.Add("is_running = @running");
                    parameters.Add(new MySqlParameter("@running", request.IsRunning.Value));
                }

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    whereClauses.Add("(name LIKE @keyword OR description LIKE @keyword)");
                    parameters.Add(new MySqlParameter("@keyword", $"%{request.Keyword}%"));
                }

                var whereStr = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

                // 获取总数
                var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM game_activities {whereStr}", conn);
                countCmd.Parameters.AddRange(parameters.ToArray());
                total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                // 分页查询
                var offset = (request.Page - 1) * request.PageSize;
                var sql = $"SELECT * FROM game_activities {whereStr} ORDER BY priority, id LIMIT @offset, @limit";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddRange(parameters.Select(p => p.Clone()).ToArray());
                cmd.Parameters.Add(new MySqlParameter("@offset", offset));
                cmd.Parameters.Add(new MySqlParameter("@limit", request.PageSize));

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    activities.Add(MapActivity(reader));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "查询活动失败");
            }

            return (activities, total);
        }

        public async Task<GameActivity?> GetActivityByIdAsync(int id)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT * FROM game_activities WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapActivity(reader);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"获取活动失败: {id}");
            }
            return null;
        }

        public async Task<bool> SaveActivityAsync(ActivitySaveRequest request, string? operatorName = null)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql;
                if (request.Id.HasValue && request.Id > 0)
                {
                    sql = @"UPDATE game_activities SET 
                        name = @name, activity_type = @type, description = @desc, icon = @icon, color = @color,
                        schedule_type = @scheduleType, schedule_rule = @scheduleRule, 
                        start_time = @startTime, end_time = @endTime, duration_minutes = @duration,
                        interval_minutes = @interval, week_days = @weekDays, params = @params,
                        is_enabled = @enabled, priority = @priority, min_level = @minLevel, max_level = @maxLevel,
                        map_id = @mapId, vip_only = @vipOnly, updated_at = NOW()
                        WHERE id = @id";
                }
                else
                {
                    sql = @"INSERT INTO game_activities 
                        (name, activity_type, description, icon, color, schedule_type, schedule_rule,
                         start_time, end_time, duration_minutes, interval_minutes, week_days, params,
                         is_enabled, priority, min_level, max_level, map_id, vip_only, created_by, created_at)
                        VALUES (@name, @type, @desc, @icon, @color, @scheduleType, @scheduleRule,
                         @startTime, @endTime, @duration, @interval, @weekDays, @params,
                         @enabled, @priority, @minLevel, @maxLevel, @mapId, @vipOnly, @createdBy, NOW())";
                }

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", request.Name);
                cmd.Parameters.AddWithValue("@type", request.ActivityType);
                cmd.Parameters.AddWithValue("@desc", request.Description ?? "");
                cmd.Parameters.AddWithValue("@icon", request.Icon ?? "default");
                cmd.Parameters.AddWithValue("@color", request.Color ?? "#FFD700");
                cmd.Parameters.AddWithValue("@scheduleType", request.ScheduleType);
                cmd.Parameters.AddWithValue("@scheduleRule", request.ScheduleRule ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@startTime", ParseTime(request.StartTime));
                cmd.Parameters.AddWithValue("@endTime", ParseTime(request.EndTime));
                cmd.Parameters.AddWithValue("@duration", request.DurationMinutes);
                cmd.Parameters.AddWithValue("@interval", request.IntervalMinutes);
                cmd.Parameters.AddWithValue("@weekDays", request.WeekDays ?? "1,2,3,4,5,6,7");
                cmd.Parameters.AddWithValue("@params", request.Params ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@enabled", request.IsEnabled);
                cmd.Parameters.AddWithValue("@priority", request.Priority);
                cmd.Parameters.AddWithValue("@minLevel", request.MinLevel);
                cmd.Parameters.AddWithValue("@maxLevel", request.MaxLevel);
                cmd.Parameters.AddWithValue("@mapId", request.MapId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@vipOnly", request.VipOnly);

                if (request.Id.HasValue && request.Id > 0)
                {
                    cmd.Parameters.AddWithValue("@id", request.Id.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@createdBy", operatorName ?? "system");
                }

                await cmd.ExecuteNonQueryAsync();
                Logger.Info($"活动保存成功: {request.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"保存活动失败: {request.Name}");
                return false;
            }
        }

        public async Task<bool> DeleteActivityAsync(int id)
        {
            try
            {
                // 先停止活动
                if (_runningActivities.ContainsKey(id))
                {
                    await StopActivityAsync(id);
                }

                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 删除奖励配置
                var deleteRewards = new MySqlCommand("DELETE FROM activity_rewards WHERE activity_id = @id", conn);
                deleteRewards.Parameters.AddWithValue("@id", id);
                await deleteRewards.ExecuteNonQueryAsync();

                // 删除活动
                var cmd = new MySqlCommand("DELETE FROM game_activities WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();

                Logger.Info($"活动已删除: {id}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"删除活动失败: {id}");
                return false;
            }
        }

        public async Task<bool> SetActivityEnabledAsync(int id, bool enabled)
        {
            try
            {
                if (!enabled && _runningActivities.ContainsKey(id))
                {
                    await StopActivityAsync(id);
                }

                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("UPDATE game_activities SET is_enabled = @enabled WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@enabled", enabled);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();

                Logger.Info($"活动状态已更新: {id}, enabled={enabled}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"更新活动状态失败: {id}");
                return false;
            }
        }

        #endregion

        #region 活动运行控制

        public async Task<bool> StartActivityAsync(int id)
        {
            try
            {
                var activity = await GetActivityByIdAsync(id);
                if (activity == null)
                {
                    Logger.Warn($"活动不存在: {id}");
                    return false;
                }

                if (_runningActivities.ContainsKey(id))
                {
                    Logger.Warn($"活动已在运行: {activity.Name}");
                    return false;
                }

                var runtime = new ActivityRuntime
                {
                    ActivityId = id,
                    Name = activity.Name,
                    ActivityType = activity.ActivityType,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(activity.DurationMinutes),
                    IsActive = true
                };

                _runningActivities[id] = runtime;

                // 更新数据库状态
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"UPDATE game_activities SET 
                    is_running = 1, last_run_time = NOW(), total_runs = total_runs + 1 
                    WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();

                // 应用活动效果
                ApplyActivityEffects(activity);

                // 记录日志
                await LogActivityAsync(id, "start", "success", $"活动开始: {activity.Name}");

                // 广播活动开始
                BroadcastActivityMessage(activity, true);

                Logger.Info($"活动已启动: {activity.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"启动活动失败: {id}");
                return false;
            }
        }

        public async Task<bool> StopActivityAsync(int id)
        {
            try
            {
                if (!_runningActivities.TryRemove(id, out var runtime))
                {
                    Logger.Warn($"活动未在运行: {id}");
                    return false;
                }

                var activity = await GetActivityByIdAsync(id);

                // 更新数据库状态
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("UPDATE game_activities SET is_running = 0 WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();

                // 移除活动效果
                RemoveActivityEffects(activity);

                // 记录日志
                await LogActivityAsync(id, "end", "success", 
                    $"活动结束: {runtime.Name}, 参与人数: {runtime.ParticipantCount}",
                    runtime.ParticipantCount);

                // 广播活动结束
                if (activity != null)
                {
                    BroadcastActivityMessage(activity, false);
                }

                Logger.Info($"活动已停止: {runtime.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"停止活动失败: {id}");
                return false;
            }
        }

        public List<ActivityRuntime> GetRunningActivities()
        {
            return _runningActivities.Values.ToList();
        }

        public bool IsActivityRunning(int activityId)
        {
            return _runningActivities.ContainsKey(activityId);
        }

        public async Task<ActivityStatusResponse> GetActivityStatusAsync()
        {
            var response = new ActivityStatusResponse
            {
                RunningActivities = GetRunningActivities()
            };

            try
            {
                var allActivities = await GetAllActivitiesAsync();
                
                // 获取即将开始的活动
                response.UpcomingActivities = allActivities
                    .Where(a => a.IsEnabled && !a.IsRunning && a.NextRunTime.HasValue)
                    .OrderBy(a => a.NextRunTime)
                    .Take(5)
                    .ToList();

                // 统计信息
                response.TodayStats = new ActivityStats
                {
                    TotalActivities = allActivities.Count,
                    EnabledActivities = allActivities.Count(a => a.IsEnabled),
                    RunningActivities = _runningActivities.Count
                };

                // 获取今日运行次数
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"SELECT COUNT(*) FROM activity_logs 
                    WHERE action = 'start' AND DATE(created_at) = CURDATE()", conn);
                response.TodayStats.TodayRuns = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取活动状态失败");
            }

            return response;
        }

        #endregion

        #region 活动日志

        public async Task LogActivityAsync(int activityId, string action, string status, 
            string? message = null, int participants = 0, int rewards = 0)
        {
            try
            {
                var activity = await GetActivityByIdAsync(activityId);

                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"INSERT INTO activity_logs 
                    (activity_id, activity_name, activity_type, action, status, message, participants, rewards_given)
                    VALUES (@activityId, @name, @type, @action, @status, @message, @participants, @rewards)", conn);

                cmd.Parameters.AddWithValue("@activityId", activityId);
                cmd.Parameters.AddWithValue("@name", activity?.Name ?? "Unknown");
                cmd.Parameters.AddWithValue("@type", activity?.ActivityType ?? "Unknown");
                cmd.Parameters.AddWithValue("@action", action);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@message", message ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@participants", participants);
                cmd.Parameters.AddWithValue("@rewards", rewards);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "记录活动日志失败");
            }
        }

        public async Task<List<ActivityLog>> GetActivityLogsAsync(int? activityId = null, int limit = 100)
        {
            var logs = new List<ActivityLog>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = activityId.HasValue
                    ? "SELECT * FROM activity_logs WHERE activity_id = @activityId ORDER BY created_at DESC LIMIT @limit"
                    : "SELECT * FROM activity_logs ORDER BY created_at DESC LIMIT @limit";

                var cmd = new MySqlCommand(sql, conn);
                if (activityId.HasValue)
                    cmd.Parameters.AddWithValue("@activityId", activityId.Value);
                cmd.Parameters.AddWithValue("@limit", limit);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logs.Add(new ActivityLog
                    {
                        Id = reader.GetInt64("id"),
                        ActivityId = reader.GetInt32("activity_id"),
                        ActivityName = reader.IsDBNull(reader.GetOrdinal("activity_name")) ? null : reader.GetString("activity_name"),
                        ActivityType = reader.IsDBNull(reader.GetOrdinal("activity_type")) ? null : reader.GetString("activity_type"),
                        Action = reader.GetString("action"),
                        Status = reader.GetString("status"),
                        Message = reader.IsDBNull(reader.GetOrdinal("message")) ? null : reader.GetString("message"),
                        Participants = reader.GetInt32("participants"),
                        RewardsGiven = reader.GetInt32("rewards_given"),
                        CreatedAt = reader.GetDateTime("created_at")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "查询活动日志失败");
            }
            return logs;
        }

        #endregion

        #region 活动效果

        public double GetCurrentExpRate()
        {
            return _currentExpRate;
        }

        public double GetCurrentDropRate()
        {
            return _currentDropRate;
        }

        public bool CanPlayerJoin(int activityId, int playerLevel, string? mapId, bool isVip)
        {
            if (!_runningActivities.TryGetValue(activityId, out var runtime))
                return false;

            var activity = GetActivityByIdAsync(activityId).Result;
            if (activity == null)
                return false;

            // 等级检查
            if (playerLevel < activity.MinLevel || playerLevel > activity.MaxLevel)
                return false;

            // VIP检查
            if (activity.VipOnly && !isVip)
                return false;

            // 地图检查
            if (!string.IsNullOrEmpty(activity.MapId) && activity.MapId != mapId)
                return false;

            return true;
        }

        private void ApplyActivityEffects(GameActivity? activity)
        {
            if (activity == null) return;

            lock (_lockObj)
            {
                var expRate = activity.GetParam("exp_rate", 1.0);
                var dropRate = activity.GetParam("drop_rate", 1.0);

                if (expRate > _currentExpRate)
                    _currentExpRate = expRate;

                if (dropRate > _currentDropRate)
                    _currentDropRate = dropRate;

                Logger.Info($"活动效果已应用 - 经验倍率: {_currentExpRate}, 掉落倍率: {_currentDropRate}");
            }
        }

        private void RemoveActivityEffects(GameActivity? activity)
        {
            if (activity == null) return;

            lock (_lockObj)
            {
                // 重新计算当前所有运行活动的倍率
                _currentExpRate = 1.0;
                _currentDropRate = 1.0;

                foreach (var runtime in _runningActivities.Values)
                {
                    var act = GetActivityByIdAsync(runtime.ActivityId).Result;
                    if (act != null)
                    {
                        var expRate = act.GetParam("exp_rate", 1.0);
                        var dropRate = act.GetParam("drop_rate", 1.0);

                        if (expRate > _currentExpRate)
                            _currentExpRate = expRate;
                        if (dropRate > _currentDropRate)
                            _currentDropRate = dropRate;
                    }
                }

                Logger.Info($"活动效果已移除 - 当前经验倍率: {_currentExpRate}, 掉落倍率: {_currentDropRate}");
            }
        }

        private void BroadcastActivityMessage(GameActivity activity, bool isStart)
        {
            var action = isStart ? "开启" : "结束";
            var message = $"【{activity.Name}】活动已{action}！{activity.Description}";

            // 调用游戏广播接口
            // SystemShare.WorldEngine?.SendBroadcastMessage(message, MsgColor.Green, MsgType.System);

            Logger.Info($"活动广播: {message}");
        }

        #endregion

        #region 活动奖励

        public async Task<List<ActivityReward>> GetActivityRewardsAsync(int activityId)
        {
            var rewards = new List<ActivityReward>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT * FROM activity_rewards WHERE activity_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", activityId);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rewards.Add(new ActivityReward
                    {
                        Id = reader.GetInt32("id"),
                        ActivityId = reader.GetInt32("activity_id"),
                        RewardType = reader.GetString("reward_type"),
                        RewardValue = reader.GetString("reward_value"),
                        Quantity = reader.GetInt32("quantity"),
                        Probability = reader.GetInt32("probability"),
                        IsBroadcast = reader.GetBoolean("is_broadcast")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"获取活动奖励失败: {activityId}");
            }
            return rewards;
        }

        public async Task<bool> SaveActivityRewardAsync(ActivityReward reward)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql;
                if (reward.Id > 0)
                {
                    sql = @"UPDATE activity_rewards SET 
                        reward_type = @type, reward_value = @value, quantity = @qty, 
                        probability = @prob, is_broadcast = @broadcast
                        WHERE id = @id";
                }
                else
                {
                    sql = @"INSERT INTO activity_rewards 
                        (activity_id, reward_type, reward_value, quantity, probability, is_broadcast)
                        VALUES (@activityId, @type, @value, @qty, @prob, @broadcast)";
                }

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@type", reward.RewardType);
                cmd.Parameters.AddWithValue("@value", reward.RewardValue);
                cmd.Parameters.AddWithValue("@qty", reward.Quantity);
                cmd.Parameters.AddWithValue("@prob", reward.Probability);
                cmd.Parameters.AddWithValue("@broadcast", reward.IsBroadcast);

                if (reward.Id > 0)
                    cmd.Parameters.AddWithValue("@id", reward.Id);
                else
                    cmd.Parameters.AddWithValue("@activityId", reward.ActivityId);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存活动奖励失败");
                return false;
            }
        }

        public async Task<bool> DeleteActivityRewardAsync(int rewardId)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("DELETE FROM activity_rewards WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", rewardId);
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"删除活动奖励失败: {rewardId}");
                return false;
            }
        }

        #endregion

        #region 辅助方法

        private static GameActivity MapActivity(MySqlDataReader reader)
        {
            return new GameActivity
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                ActivityType = reader.GetString("activity_type"),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                Icon = reader.IsDBNull(reader.GetOrdinal("icon")) ? "default" : reader.GetString("icon"),
                Color = reader.IsDBNull(reader.GetOrdinal("color")) ? "#FFD700" : reader.GetString("color"),
                ScheduleType = reader.GetString("schedule_type"),
                ScheduleRule = reader.IsDBNull(reader.GetOrdinal("schedule_rule")) ? null : reader.GetString("schedule_rule"),
                StartTime = reader.IsDBNull(reader.GetOrdinal("start_time")) ? null : reader.GetTimeSpan("start_time"),
                EndTime = reader.IsDBNull(reader.GetOrdinal("end_time")) ? null : reader.GetTimeSpan("end_time"),
                DurationMinutes = reader.GetInt32("duration_minutes"),
                IntervalMinutes = reader.GetInt32("interval_minutes"),
                WeekDays = reader.IsDBNull(reader.GetOrdinal("week_days")) ? "1,2,3,4,5,6,7" : reader.GetString("week_days"),
                Params = reader.IsDBNull(reader.GetOrdinal("params")) ? null : reader.GetString("params"),
                IsEnabled = reader.GetBoolean("is_enabled"),
                IsRunning = reader.GetBoolean("is_running"),
                Priority = reader.GetInt32("priority"),
                MinLevel = reader.GetInt32("min_level"),
                MaxLevel = reader.GetInt32("max_level"),
                MapId = reader.IsDBNull(reader.GetOrdinal("map_id")) ? null : reader.GetString("map_id"),
                VipOnly = reader.GetBoolean("vip_only"),
                TotalRuns = reader.GetInt32("total_runs"),
                LastRunTime = reader.IsDBNull(reader.GetOrdinal("last_run_time")) ? null : reader.GetDateTime("last_run_time"),
                NextRunTime = reader.IsDBNull(reader.GetOrdinal("next_run_time")) ? null : reader.GetDateTime("next_run_time"),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetString("created_by"),
                CreatedAt = reader.GetDateTime("created_at"),
                UpdatedAt = reader.GetDateTime("updated_at")
            };
        }

        private static object ParseTime(string? timeStr)
        {
            if (string.IsNullOrEmpty(timeStr))
                return DBNull.Value;

            if (TimeSpan.TryParse(timeStr, out var time))
                return time;

            return DBNull.Value;
        }

        #endregion
    }
}
