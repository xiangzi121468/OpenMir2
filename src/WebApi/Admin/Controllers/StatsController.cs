using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;
using WebApi.Admin.Models;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 数据统计API
    /// </summary>
    [ApiController]
    [Route("api/admin/stats")]
    public class StatsController : AdminBaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public StatsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default") 
                ?? "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";
        }

        /// <summary>
        /// 获取总览数据
        /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var result = new OverviewStats();

                // 总注册用户数
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM accounts", conn))
                    result.TotalAccounts = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 总角色数
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM characters WHERE Deleted=0", conn))
                    result.TotalCharacters = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 今日新增账号
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM accounts WHERE DATE(CreateDate)=CURDATE()", conn))
                    result.TodayNewAccounts = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 今日活跃（有登录记录）
                using (var cmd = new MySqlCommand("SELECT COUNT(DISTINCT AccountId) FROM player_login_logs WHERE DATE(CreateTime)=CURDATE() AND LoginType='login'", conn))
                    result.TodayActiveAccounts = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 今日充值金额
                using (var cmd = new MySqlCommand("SELECT COALESCE(SUM(Amount),0) FROM recharge_records WHERE DATE(CreateTime)=CURDATE() AND Status IN (1,2)", conn))
                    result.TodayRechargeAmount = Convert.ToDecimal(await cmd.ExecuteScalarAsync());

                // 今日充值笔数
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM recharge_records WHERE DATE(CreateTime)=CURDATE() AND Status IN (1,2)", conn))
                    result.TodayRechargeCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 总充值金额
                using (var cmd = new MySqlCommand("SELECT COALESCE(SUM(Amount),0) FROM recharge_records WHERE Status IN (1,2)", conn))
                    result.TotalRechargeAmount = Convert.ToDecimal(await cmd.ExecuteScalarAsync());

                // 付费用户数
                using (var cmd = new MySqlCommand("SELECT COUNT(DISTINCT AccountId) FROM recharge_records WHERE Status IN (1,2)", conn))
                    result.PayingUsers = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                return Ok(ApiResult.Success(result));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取总览数据失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取趋势数据
        /// </summary>
        [HttpGet("trend")]
        public async Task<IActionResult> GetTrend([FromQuery] int days = 7)
        {
            if (days > 90) days = 90;

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var trendData = new List<DailyStats>();

                for (int i = days - 1; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var dateStr = date.ToString("yyyy-MM-dd");

                    var stats = new DailyStats { Date = dateStr };

                    // 新增账号
                    using (var cmd = new MySqlCommand($"SELECT COUNT(*) FROM accounts WHERE DATE(CreateDate)='{dateStr}'", conn))
                        stats.NewAccounts = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    // 活跃账号
                    using (var cmd = new MySqlCommand($"SELECT COUNT(DISTINCT AccountId) FROM player_login_logs WHERE DATE(CreateTime)='{dateStr}' AND LoginType='login'", conn))
                        stats.ActiveAccounts = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    // 充值金额
                    using (var cmd = new MySqlCommand($"SELECT COALESCE(SUM(Amount),0) FROM recharge_records WHERE DATE(CreateTime)='{dateStr}' AND Status IN (1,2)", conn))
                        stats.RechargeAmount = Convert.ToDecimal(await cmd.ExecuteScalarAsync());

                    // 充值笔数
                    using (var cmd = new MySqlCommand($"SELECT COUNT(*) FROM recharge_records WHERE DATE(CreateTime)='{dateStr}' AND Status IN (1,2)", conn))
                        stats.RechargeCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    // 新增付费用户
                    using (var cmd = new MySqlCommand($@"
                        SELECT COUNT(DISTINCT AccountId) FROM recharge_records 
                        WHERE DATE(CreateTime)='{dateStr}' AND Status IN (1,2)
                        AND AccountId NOT IN (
                            SELECT DISTINCT AccountId FROM recharge_records 
                            WHERE CreateTime < '{dateStr}' AND Status IN (1,2)
                        )", conn))
                        stats.NewPayingUsers = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    trendData.Add(stats);
                }

                return Ok(ApiResult.Success(trendData));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取趋势数据失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取充值排行
        /// </summary>
        [HttpGet("recharge/rank")]
        public async Task<IActionResult> GetRechargeRank([FromQuery] int limit = 20, [FromQuery] string period = "all")
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = period switch
                {
                    "today" => "AND DATE(CreateTime)=CURDATE()",
                    "week" => "AND CreateTime >= DATE_SUB(CURDATE(), INTERVAL 7 DAY)",
                    "month" => "AND CreateTime >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)",
                    _ => ""
                };

                var sql = $@"
                    SELECT AccountId, CharName, SUM(Amount) as TotalAmount, COUNT(*) as RechargeCount
                    FROM recharge_records 
                    WHERE Status IN (1,2) {where}
                    GROUP BY AccountId, CharName
                    ORDER BY TotalAmount DESC
                    LIMIT {limit}";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<object>();
                int rank = 1;
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Rank = rank++,
                        AccountId = reader.GetString("AccountId"),
                        CharName = reader.IsDBNull(reader.GetOrdinal("CharName")) ? "" : reader.GetString("CharName"),
                        TotalAmount = reader.GetDecimal("TotalAmount"),
                        RechargeCount = reader.GetInt32("RechargeCount")
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取充值排行失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取等级分布
        /// </summary>
        [HttpGet("level/distribution")]
        public async Task<IActionResult> GetLevelDistribution()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        CASE 
                            WHEN Level < 10 THEN '1-9'
                            WHEN Level < 20 THEN '10-19'
                            WHEN Level < 30 THEN '20-29'
                            WHEN Level < 40 THEN '30-39'
                            WHEN Level < 50 THEN '40-49'
                            WHEN Level < 60 THEN '50-59'
                            ELSE '60+'
                        END as LevelRange,
                        COUNT(*) as Count
                    FROM characters 
                    WHERE Deleted=0
                    GROUP BY LevelRange
                    ORDER BY MIN(Level)";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        LevelRange = reader.GetString("LevelRange"),
                        Count = reader.GetInt32("Count")
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取等级分布失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取职业分布
        /// </summary>
        [HttpGet("job/distribution")]
        public async Task<IActionResult> GetJobDistribution()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        CASE Job
                            WHEN 0 THEN '战士'
                            WHEN 1 THEN '法师'
                            WHEN 2 THEN '道士'
                            ELSE '未知'
                        END as JobName,
                        Job,
                        COUNT(*) as Count
                    FROM characters 
                    WHERE Deleted=0
                    GROUP BY Job";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        JobName = reader.GetString("JobName"),
                        Job = reader.GetInt32("Job"),
                        Count = reader.GetInt32("Count")
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取职业分布失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取留存率
        /// </summary>
        [HttpGet("retention")]
        public async Task<IActionResult> GetRetention([FromQuery] int days = 7)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var retentionData = new List<RetentionStats>();

                for (int i = days; i >= 1; i--)
                {
                    var registerDate = DateTime.Today.AddDays(-i);
                    var dateStr = registerDate.ToString("yyyy-MM-dd");

                    var stats = new RetentionStats { RegisterDate = dateStr };

                    // 当日注册数
                    using (var cmd = new MySqlCommand($"SELECT COUNT(*) FROM accounts WHERE DATE(CreateDate)='{dateStr}'", conn))
                        stats.RegisterCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    if (stats.RegisterCount == 0)
                    {
                        retentionData.Add(stats);
                        continue;
                    }

                    // 次日留存
                    if (i >= 1)
                    {
                        var nextDay = registerDate.AddDays(1).ToString("yyyy-MM-dd");
                        using var cmd = new MySqlCommand($@"
                            SELECT COUNT(DISTINCT l.AccountId) FROM player_login_logs l
                            JOIN accounts a ON l.AccountId = a.UserID
                            WHERE DATE(a.CreateDate)='{dateStr}' AND DATE(l.CreateTime)='{nextDay}' AND l.LoginType='login'", conn);
                        stats.Day1Retention = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }

                    // 7日留存
                    if (i >= 7)
                    {
                        var day7 = registerDate.AddDays(7).ToString("yyyy-MM-dd");
                        using var cmd = new MySqlCommand($@"
                            SELECT COUNT(DISTINCT l.AccountId) FROM player_login_logs l
                            JOIN accounts a ON l.AccountId = a.UserID
                            WHERE DATE(a.CreateDate)='{dateStr}' AND DATE(l.CreateTime)='{day7}' AND l.LoginType='login'", conn);
                        stats.Day7Retention = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }

                    retentionData.Add(stats);
                }

                return Ok(ApiResult.Success(retentionData));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取留存率失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取在线时长分布
        /// </summary>
        [HttpGet("online/duration")]
        public async Task<IActionResult> GetOnlineDuration([FromQuery] string date = null)
        {
            var targetDate = string.IsNullOrEmpty(date) ? DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd") : date;

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = $@"
                    SELECT 
                        CASE 
                            WHEN OnlineSeconds < 300 THEN '<5分钟'
                            WHEN OnlineSeconds < 1800 THEN '5-30分钟'
                            WHEN OnlineSeconds < 3600 THEN '30-60分钟'
                            WHEN OnlineSeconds < 7200 THEN '1-2小时'
                            WHEN OnlineSeconds < 14400 THEN '2-4小时'
                            ELSE '4小时+'
                        END as DurationRange,
                        COUNT(*) as Count
                    FROM player_login_logs
                    WHERE DATE(CreateTime)='{targetDate}' AND LoginType='logout'
                    GROUP BY DurationRange
                    ORDER BY MIN(OnlineSeconds)";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        DurationRange = reader.GetString("DurationRange"),
                        Count = reader.GetInt32("Count")
                    });
                }

                return Ok(ApiResult.Success(new { date = targetDate, data = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取在线时长分布失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取实时在线数据
        /// </summary>
        [HttpGet("online/realtime")]
        public async Task<IActionResult> GetRealtimeOnline()
        {
            try
            {
                // 这里应该从游戏服务器获取实时数据
                // 暂时返回模拟数据
                return Ok(ApiResult.Success(new
                {
                    CurrentOnline = 0,
                    PeakToday = 0,
                    PeakTime = "00:00",
                    LastUpdate = DateTime.Now
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取实时在线失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }
    }

    #region 统计模型

    public class OverviewStats
    {
        public int TotalAccounts { get; set; }
        public int TotalCharacters { get; set; }
        public int TodayNewAccounts { get; set; }
        public int TodayActiveAccounts { get; set; }
        public decimal TodayRechargeAmount { get; set; }
        public int TodayRechargeCount { get; set; }
        public decimal TotalRechargeAmount { get; set; }
        public int PayingUsers { get; set; }
        public decimal ARPU => TotalAccounts > 0 ? TotalRechargeAmount / TotalAccounts : 0;
        public decimal ARPPU => PayingUsers > 0 ? TotalRechargeAmount / PayingUsers : 0;
        public decimal PayRate => TotalAccounts > 0 ? (decimal)PayingUsers / TotalAccounts * 100 : 0;
    }

    public class DailyStats
    {
        public string Date { get; set; }
        public int NewAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public decimal RechargeAmount { get; set; }
        public int RechargeCount { get; set; }
        public int NewPayingUsers { get; set; }
    }

    public class RetentionStats
    {
        public string RegisterDate { get; set; }
        public int RegisterCount { get; set; }
        public int Day1Retention { get; set; }
        public int Day7Retention { get; set; }
        public decimal Day1Rate => RegisterCount > 0 ? (decimal)Day1Retention / RegisterCount * 100 : 0;
        public decimal Day7Rate => RegisterCount > 0 ? (decimal)Day7Retention / RegisterCount * 100 : 0;
    }

    #endregion
}
