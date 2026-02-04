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
    /// 数据统计
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
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
        /// 仪表盘概览
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 总账号数
                using var accountCmd = new MySqlCommand("SELECT COUNT(*) FROM account", conn);
                int totalAccounts = Convert.ToInt32(await accountCmd.ExecuteScalarAsync());

                // 总角色数
                using var charCmd = new MySqlCommand("SELECT COUNT(*) FROM characters WHERE Deleted=0", conn);
                int totalCharacters = Convert.ToInt32(await charCmd.ExecuteScalarAsync());

                // 今日新增账号
                using var newAccountCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM account WHERE DATE(CREATEDATE)=CURDATE()", conn);
                int newAccounts = Convert.ToInt32(await newAccountCmd.ExecuteScalarAsync());

                // 今日新增角色
                using var newCharCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM characters WHERE DATE(CREATEDATE)=CURDATE()", conn);
                int newCharacters = Convert.ToInt32(await newCharCmd.ExecuteScalarAsync());

                // 今日充值
                using var rechargeCmd = new MySqlCommand(@"
                    SELECT COALESCE(COUNT(*),0) as Count, COALESCE(SUM(Amount),0) as Amount 
                    FROM recharge_records WHERE STATUS IN (1,2) AND DATE(CreateTime)=CURDATE()", conn);
                using var rechargeReader = await rechargeCmd.ExecuteReaderAsync();
                await rechargeReader.ReadAsync();
                int todayRechargeCount = rechargeReader.GetInt32(0);
                decimal todayRechargeAmount = rechargeReader.GetDecimal(1);
                rechargeReader.Close();

                // 今日商城销售
                using var shopCmd = new MySqlCommand(@"
                    SELECT COALESCE(COUNT(*),0) as Count, COALESCE(SUM(TotalPrice),0) as Amount 
                    FROM shop_orders WHERE Status=1 AND DATE(CreateTime)=CURDATE()", conn);
                using var shopReader = await shopCmd.ExecuteReaderAsync();
                await shopReader.ReadAsync();
                int todayShopOrders = shopReader.GetInt32(0);
                int todayShopSales = shopReader.GetInt32(1);
                shopReader.Close();

                return Ok(ApiResult.Success(new
                {
                    totalAccounts,
                    totalCharacters,
                    newAccounts,
                    newCharacters,
                    todayRecharge = new { count = todayRechargeCount, amount = todayRechargeAmount },
                    todayShop = new { orders = todayShopOrders, sales = todayShopSales },
                    onlinePlayers = 0 // 需要从游戏服务器获取
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取仪表盘数据失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 玩家等级分布
        /// </summary>
        [HttpGet("level-distribution")]
        public async Task<IActionResult> GetLevelDistribution()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT 
                        CASE 
                            WHEN Level BETWEEN 1 AND 20 THEN '1-20'
                            WHEN Level BETWEEN 21 AND 40 THEN '21-40'
                            WHEN Level BETWEEN 41 AND 60 THEN '41-60'
                            WHEN Level BETWEEN 61 AND 80 THEN '61-80'
                            WHEN Level BETWEEN 81 AND 100 THEN '81-100'
                            ELSE '100+'
                        END as LevelRange,
                        COUNT(*) as Count
                    FROM characters WHERE Deleted=0
                    GROUP BY LevelRange
                    ORDER BY MIN(Level)", conn);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        range = reader.GetString(0),
                        count = reader.GetInt32(1)
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
        /// 职业分布
        /// </summary>
        [HttpGet("job-distribution")]
        public async Task<IActionResult> GetJobDistribution()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT Job, COUNT(*) as Count 
                    FROM characters WHERE Deleted=0 
                    GROUP BY Job", conn);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int job = reader.GetInt32(0);
                    string jobName = job switch
                    {
                        0 => "战士",
                        1 => "法师",
                        2 => "道士",
                        _ => $"未知({job})"
                    };
                    list.Add(new
                    {
                        job,
                        name = jobName,
                        count = reader.GetInt32(1)
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
        /// 新增趋势(近30天)
        /// </summary>
        [HttpGet("trend")]
        public async Task<IActionResult> GetTrend([FromQuery] int days = 30)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var list = new List<object>();

                // 生成日期范围
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);

                    // 新增账号
                    using var accountCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM account WHERE DATE(CREATEDATE)=@Date", conn);
                    accountCmd.Parameters.AddWithValue("@Date", date);
                    int newAccounts = Convert.ToInt32(await accountCmd.ExecuteScalarAsync());

                    // 新增角色
                    using var charCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM characters WHERE DATE(CREATEDATE)=@Date", conn);
                    charCmd.Parameters.AddWithValue("@Date", date);
                    int newCharacters = Convert.ToInt32(await charCmd.ExecuteScalarAsync());

                    // 充值金额
                    using var rechargeCmd = new MySqlCommand(
                        "SELECT COALESCE(SUM(Amount),0) FROM recharge_records WHERE Status IN (1,2) AND DATE(CreateTime)=@Date", conn);
                    rechargeCmd.Parameters.AddWithValue("@Date", date);
                    decimal rechargeAmount = Convert.ToDecimal(await rechargeCmd.ExecuteScalarAsync());

                    list.Add(new
                    {
                        date = date.ToString("MM-dd"),
                        newAccounts,
                        newCharacters,
                        rechargeAmount
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取趋势数据失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// VIP分布
        /// </summary>
        [HttpGet("vip-distribution")]
        public async Task<IActionResult> GetVipDistribution()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT VipLevel, COUNT(*) as Count 
                    FROM user_vip 
                    GROUP BY VipLevel 
                    ORDER BY VipLevel", conn);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        level = reader.GetInt32(0),
                        count = reader.GetInt32(1)
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取VIP分布失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 充值排行榜
        /// </summary>
        [HttpGet("recharge-ranking")]
        public async Task<IActionResult> GetRechargeRanking([FromQuery] int limit = 20)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand($@"
                    SELECT AccountId, SUM(Amount) as TotalAmount, SUM(GameGold+BonusGold) as TotalGold, COUNT(*) as Count
                    FROM recharge_records WHERE Status IN (1,2)
                    GROUP BY AccountId
                    ORDER BY TotalAmount DESC
                    LIMIT {limit}", conn);

                var list = new List<object>();
                int rank = 1;
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        rank = rank++,
                        accountId = reader.GetString(0),
                        totalAmount = reader.GetDecimal(1),
                        totalGold = reader.GetInt64(2),
                        count = reader.GetInt32(3)
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
    }
}
