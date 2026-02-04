using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using NLog;
using NoticeModule.Models;

namespace NoticeModule
{
    /// <summary>
    /// 通知服务实现
    /// </summary>
    public class NoticeService : INoticeService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        // 缓存
        private List<string> _loginNoticesCache = new List<string>();
        private List<GameNotice> _scrollNoticesCache = new List<GameNotice>();
        private List<GameNotice> _scheduleNoticesCache = new List<GameNotice>();
        private int _scrollIndex = 0;
        private DateTime _lastCacheTime = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

        public NoticeService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 公告查询

        public async Task<List<GameNotice>> GetEnabledNoticesAsync()
        {
            var list = new List<GameNotice>();
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT * FROM game_notices 
                    WHERE IsEnabled = 1 
                    AND (StartTime IS NULL OR StartTime <= NOW())
                    AND (EndTime IS NULL OR EndTime >= NOW())
                    ORDER BY Priority DESC, Id", conn);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(ReadNotice(reader));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取启用公告失败");
            }
            return list;
        }

        public async Task<List<GameNotice>> GetNoticesByTypeAsync(string noticeType)
        {
            var list = new List<GameNotice>();
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT * FROM game_notices 
                    WHERE NoticeType = @Type AND IsEnabled = 1
                    AND (StartTime IS NULL OR StartTime <= NOW())
                    AND (EndTime IS NULL OR EndTime >= NOW())
                    ORDER BY Priority DESC, Id", conn);
                cmd.Parameters.AddWithValue("@Type", noticeType);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(ReadNotice(reader));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取公告失败: {0}", noticeType);
            }
            return list;
        }

        public Task<List<GameNotice>> GetLoginNoticesAsync() => GetNoticesByTypeAsync(NoticeTypes.Login);
        public Task<List<GameNotice>> GetScrollNoticesAsync() => GetNoticesByTypeAsync(NoticeTypes.Scroll);
        public Task<List<GameNotice>> GetScheduleNoticesAsync() => GetNoticesByTypeAsync(NoticeTypes.Schedule);

        public async Task<GameNotice> GetNoticeByIdAsync(int id)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT * FROM game_notices WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return ReadNotice(reader);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取公告详情失败: {0}", id);
            }
            return null;
        }

        #endregion

        #region 公告管理

        public async Task<int> AddNoticeAsync(GameNotice notice)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO game_notices (Title, Content, NoticeType, Priority, Color, TargetMap, TargetLevel, TargetVip,
                        RepeatCount, RepeatInterval, StartTime, EndTime, ScheduleCron, IsEnabled, AdminId, AdminName, CreateTime)
                    VALUES (@Title, @Content, @NoticeType, @Priority, @Color, @TargetMap, @TargetLevel, @TargetVip,
                        @RepeatCount, @RepeatInterval, @StartTime, @EndTime, @ScheduleCron, @IsEnabled, @AdminId, @AdminName, NOW());
                    SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@Title", notice.Title);
                cmd.Parameters.AddWithValue("@Content", notice.Content);
                cmd.Parameters.AddWithValue("@NoticeType", notice.NoticeType);
                cmd.Parameters.AddWithValue("@Priority", notice.Priority);
                cmd.Parameters.AddWithValue("@Color", notice.Color ?? "yellow");
                cmd.Parameters.AddWithValue("@TargetMap", notice.TargetMap ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TargetLevel", notice.TargetLevel);
                cmd.Parameters.AddWithValue("@TargetVip", notice.TargetVip);
                cmd.Parameters.AddWithValue("@RepeatCount", notice.RepeatCount);
                cmd.Parameters.AddWithValue("@RepeatInterval", notice.RepeatInterval);
                cmd.Parameters.AddWithValue("@StartTime", notice.StartTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EndTime", notice.EndTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ScheduleCron", notice.ScheduleCron ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsEnabled", notice.IsEnabled);
                cmd.Parameters.AddWithValue("@AdminId", notice.AdminId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AdminName", notice.AdminName ?? (object)DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                int id = Convert.ToInt32(result);

                // 刷新缓存
                await ReloadCacheAsync();

                return id;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "添加公告失败");
                return 0;
            }
        }

        public async Task<bool> UpdateNoticeAsync(GameNotice notice)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE game_notices SET 
                        Title = @Title, Content = @Content, NoticeType = @NoticeType, Priority = @Priority, 
                        Color = @Color, TargetMap = @TargetMap, TargetLevel = @TargetLevel, TargetVip = @TargetVip,
                        RepeatCount = @RepeatCount, RepeatInterval = @RepeatInterval, 
                        StartTime = @StartTime, EndTime = @EndTime, ScheduleCron = @ScheduleCron, IsEnabled = @IsEnabled
                    WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", notice.Id);
                cmd.Parameters.AddWithValue("@Title", notice.Title);
                cmd.Parameters.AddWithValue("@Content", notice.Content);
                cmd.Parameters.AddWithValue("@NoticeType", notice.NoticeType);
                cmd.Parameters.AddWithValue("@Priority", notice.Priority);
                cmd.Parameters.AddWithValue("@Color", notice.Color ?? "yellow");
                cmd.Parameters.AddWithValue("@TargetMap", notice.TargetMap ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TargetLevel", notice.TargetLevel);
                cmd.Parameters.AddWithValue("@TargetVip", notice.TargetVip);
                cmd.Parameters.AddWithValue("@RepeatCount", notice.RepeatCount);
                cmd.Parameters.AddWithValue("@RepeatInterval", notice.RepeatInterval);
                cmd.Parameters.AddWithValue("@StartTime", notice.StartTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EndTime", notice.EndTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ScheduleCron", notice.ScheduleCron ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsEnabled", notice.IsEnabled);

                int affected = await cmd.ExecuteNonQueryAsync();

                // 刷新缓存
                await ReloadCacheAsync();

                return affected > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "更新公告失败: {0}", notice.Id);
                return false;
            }
        }

        public async Task<bool> DeleteNoticeAsync(int id)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("DELETE FROM game_notices WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                int affected = await cmd.ExecuteNonQueryAsync();

                // 刷新缓存
                await ReloadCacheAsync();

                return affected > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "删除公告失败: {0}", id);
                return false;
            }
        }

        public async Task<bool> SetNoticeEnabledAsync(int id, bool enabled)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("UPDATE game_notices SET IsEnabled = @Enabled WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Enabled", enabled);

                int affected = await cmd.ExecuteNonQueryAsync();

                // 刷新缓存
                await ReloadCacheAsync();

                return affected > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "设置公告状态失败: {0}", id);
                return false;
            }
        }

        #endregion

        #region 公告发送

        public async Task<int> SendBroadcastAsync(string content, string color = "yellow", int priority = 0)
        {
            // 这里只是记录，实际发送由游戏服务器WorldEngine处理
            await LogSendAsync(0, NoticeTypes.Broadcast, 0, content);
            return 0;
        }

        public async Task<int> SendMapNoticeAsync(string mapName, string content, string color = "yellow")
        {
            await LogSendAsync(0, NoticeTypes.Map, 0, $"[{mapName}] {content}");
            return 0;
        }

        public async Task LogSendAsync(int noticeId, string noticeType, int targetCount, string content)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO notice_send_logs (NoticeId, NoticeType, TargetCount, Content, SendTime)
                    VALUES (@NoticeId, @NoticeType, @TargetCount, @Content, NOW())", conn);

                cmd.Parameters.AddWithValue("@NoticeId", noticeId);
                cmd.Parameters.AddWithValue("@NoticeType", noticeType);
                cmd.Parameters.AddWithValue("@TargetCount", targetCount);
                cmd.Parameters.AddWithValue("@Content", content?.Length > 500 ? content.Substring(0, 500) : content);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "记录发送日志失败");
            }
        }

        public async Task UpdateNoticeSentAsync(int noticeId)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE game_notices SET LastSentTime = NOW(), SentCount = SentCount + 1 WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", noticeId);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "更新公告发送状态失败: {0}", noticeId);
            }
        }

        #endregion

        #region 缓存管理

        public async Task ReloadCacheAsync()
        {
            try
            {
                // 加载登录公告
                var loginNotices = await GetLoginNoticesAsync();
                _loginNoticesCache.Clear();
                foreach (var notice in loginNotices)
                {
                    // 按行拆分内容
                    var lines = notice.Content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _loginNoticesCache.Add(line.Trim());
                        }
                    }
                }

                // 加载滚动公告
                _scrollNoticesCache = await GetScrollNoticesAsync();
                _scrollIndex = 0;

                // 加载定时公告
                _scheduleNoticesCache = await GetScheduleNoticesAsync();

                _lastCacheTime = DateTime.Now;
                Logger.Info("公告缓存已刷新: 登录公告{0}条, 滚动公告{1}条, 定时公告{2}条",
                    _loginNoticesCache.Count, _scrollNoticesCache.Count, _scheduleNoticesCache.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "刷新公告缓存失败");
            }
        }

        public List<string> GetCachedLoginNotices()
        {
            // 检查缓存是否过期
            if (DateTime.Now - _lastCacheTime > _cacheExpiry)
            {
                _ = ReloadCacheAsync();
            }
            return _loginNoticesCache;
        }

        public GameNotice GetNextScrollNotice()
        {
            if (_scrollNoticesCache.Count == 0)
                return null;

            // 检查缓存是否过期
            if (DateTime.Now - _lastCacheTime > _cacheExpiry)
            {
                _ = ReloadCacheAsync();
            }

            var notice = _scrollNoticesCache[_scrollIndex];
            _scrollIndex = (_scrollIndex + 1) % _scrollNoticesCache.Count;

            // 检查是否可以发送（间隔和次数限制）
            if (notice.LastSentTime.HasValue && notice.RepeatInterval > 0)
            {
                var elapsed = (DateTime.Now - notice.LastSentTime.Value).TotalSeconds;
                if (elapsed < notice.RepeatInterval)
                    return null;
            }

            if (notice.RepeatCount > 0 && notice.SentCount >= notice.RepeatCount)
                return null;

            return notice;
        }

        #endregion

        #region 工具方法

        private GameNotice ReadNotice(MySqlDataReader reader)
        {
            return new GameNotice
            {
                Id = reader.GetInt32("Id"),
                Title = reader.GetString("Title"),
                Content = reader.GetString("Content"),
                NoticeType = reader.GetString("NoticeType"),
                Priority = reader.GetInt32("Priority"),
                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? "yellow" : reader.GetString("Color"),
                TargetMap = reader.IsDBNull(reader.GetOrdinal("TargetMap")) ? null : reader.GetString("TargetMap"),
                TargetLevel = reader.GetInt32("TargetLevel"),
                TargetVip = reader.GetInt32("TargetVip"),
                RepeatCount = reader.GetInt32("RepeatCount"),
                RepeatInterval = reader.GetInt32("RepeatInterval"),
                StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? null : reader.GetDateTime("StartTime"),
                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime("EndTime"),
                ScheduleCron = reader.IsDBNull(reader.GetOrdinal("ScheduleCron")) ? null : reader.GetString("ScheduleCron"),
                LastSentTime = reader.IsDBNull(reader.GetOrdinal("LastSentTime")) ? null : reader.GetDateTime("LastSentTime"),
                SentCount = reader.GetInt32("SentCount"),
                IsEnabled = reader.GetBoolean("IsEnabled"),
                AdminId = reader.IsDBNull(reader.GetOrdinal("AdminId")) ? null : reader.GetInt32("AdminId"),
                AdminName = reader.IsDBNull(reader.GetOrdinal("AdminName")) ? null : reader.GetString("AdminName"),
                CreateTime = reader.GetDateTime("CreateTime")
            };
        }

        #endregion
    }
}
