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
    /// 攻城战管理API
    /// </summary>
    [ApiController]
    [Route("api/admin/castle")]
    public class CastleController : AdminBaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public CastleController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";
        }

        #region 城堡管理

        /// <summary>
        /// 获取城堡列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetCastles()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT * FROM castles ORDER BY Id", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt32("Id"),
                        CastleName = reader.GetString("CastleName"),
                        MapName = reader.GetString("MapName"),
                        OwnerGuildId = reader.IsDBNull(reader.GetOrdinal("OwnerGuildId")) ? null : (int?)reader.GetInt32("OwnerGuildId"),
                        OwnerGuildName = reader.IsDBNull(reader.GetOrdinal("OwnerGuildName")) ? null : reader.GetString("OwnerGuildName"),
                        OwnerName = reader.IsDBNull(reader.GetOrdinal("OwnerName")) ? null : reader.GetString("OwnerName"),
                        OccupyTime = reader.IsDBNull(reader.GetOrdinal("OccupyTime")) ? null : (DateTime?)reader.GetDateTime("OccupyTime"),
                        TaxRate = reader.GetInt32("TaxRate"),
                        TotalTax = reader.GetInt64("TotalTax"),
                        GateHP = reader.GetInt32("GateHP"),
                        GateMaxHP = reader.GetInt32("GateMaxHP"),
                        IsUnderAttack = reader.GetBoolean("IsUnderAttack"),
                        LastWarTime = reader.IsDBNull(reader.GetOrdinal("LastWarTime")) ? null : (DateTime?)reader.GetDateTime("LastWarTime")
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取城堡列表失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取城堡详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCastle(int id)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT * FROM castles WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return Ok(ApiResult.Fail("城堡不存在"));

                var castle = new
                {
                    Id = reader.GetInt32("Id"),
                    CastleName = reader.GetString("CastleName"),
                    MapName = reader.GetString("MapName"),
                    PalaceMapName = reader.IsDBNull(reader.GetOrdinal("PalaceMapName")) ? null : reader.GetString("PalaceMapName"),
                    CenterX = reader.GetInt32("CenterX"),
                    CenterY = reader.GetInt32("CenterY"),
                    OwnerGuildId = reader.IsDBNull(reader.GetOrdinal("OwnerGuildId")) ? null : (int?)reader.GetInt32("OwnerGuildId"),
                    OwnerGuildName = reader.IsDBNull(reader.GetOrdinal("OwnerGuildName")) ? null : reader.GetString("OwnerGuildName"),
                    OwnerName = reader.IsDBNull(reader.GetOrdinal("OwnerName")) ? null : reader.GetString("OwnerName"),
                    OccupyTime = reader.IsDBNull(reader.GetOrdinal("OccupyTime")) ? null : (DateTime?)reader.GetDateTime("OccupyTime"),
                    TaxRate = reader.GetInt32("TaxRate"),
                    TotalTax = reader.GetInt64("TotalTax"),
                    DefenseLevel = reader.GetInt32("DefenseLevel"),
                    GateHP = reader.GetInt32("GateHP"),
                    GateMaxHP = reader.GetInt32("GateMaxHP"),
                    LeftTowerHP = reader.GetInt32("LeftTowerHP"),
                    RightTowerHP = reader.GetInt32("RightTowerHP"),
                    TowerMaxHP = reader.GetInt32("TowerMaxHP"),
                    GuardCount = reader.GetInt32("GuardCount"),
                    IsUnderAttack = reader.GetBoolean("IsUnderAttack"),
                    WarStartTime = reader.IsDBNull(reader.GetOrdinal("WarStartTime")) ? null : (DateTime?)reader.GetDateTime("WarStartTime"),
                    WarEndTime = reader.IsDBNull(reader.GetOrdinal("WarEndTime")) ? null : (DateTime?)reader.GetDateTime("WarEndTime"),
                    LastWarTime = reader.IsDBNull(reader.GetOrdinal("LastWarTime")) ? null : (DateTime?)reader.GetDateTime("LastWarTime")
                };

                return Ok(ApiResult.Success(castle));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取城堡详情失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 更新城堡设置
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCastle(int id, [FromBody] UpdateCastleRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE castles SET 
                        TaxRate = @TaxRate,
                        DefenseLevel = @DefenseLevel,
                        GuardCount = @GuardCount,
                        GateMaxHP = @GateMaxHP,
                        TowerMaxHP = @TowerMaxHP
                    WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@TaxRate", Math.Min(50, Math.Max(0, request.TaxRate)));
                cmd.Parameters.AddWithValue("@DefenseLevel", request.DefenseLevel);
                cmd.Parameters.AddWithValue("@GuardCount", request.GuardCount);
                cmd.Parameters.AddWithValue("@GateMaxHP", request.GateMaxHP);
                cmd.Parameters.AddWithValue("@TowerMaxHP", request.TowerMaxHP);

                await cmd.ExecuteNonQueryAsync();

                await LogAction("castle", "update", $"更新城堡设置: ID={id}");
                return Ok(ApiResult.Success("更新成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "更新城堡失败");
                return Ok(ApiResult.Fail("更新失败"));
            }
        }

        /// <summary>
        /// 设置城主
        /// </summary>
        [HttpPost("{id}/owner")]
        public async Task<IActionResult> SetOwner(int id, [FromBody] SetOwnerRequest request)
        {
            if (!IsSuperAdmin)
                return Ok(ApiResult.Fail("需要超级管理员权限"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE castles SET 
                        OwnerGuildId = @GuildId,
                        OwnerGuildName = @GuildName,
                        OwnerName = @OwnerName,
                        OccupyTime = @OccupyTime
                    WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@GuildId", request.GuildId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@GuildName", request.GuildName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@OwnerName", request.OwnerName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@OccupyTime", request.GuildId.HasValue ? DateTime.Now : (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();

                await LogAction("castle", "set_owner", $"设置城主: 城堡ID={id}, 行会={request.GuildName}");
                return Ok(ApiResult.Success("设置成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "设置城主失败");
                return Ok(ApiResult.Fail("设置失败"));
            }
        }

        /// <summary>
        /// 修复城门
        /// </summary>
        [HttpPost("{id}/repair")]
        public async Task<IActionResult> RepairGate(int id)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE castles SET 
                        GateHP = GateMaxHP,
                        LeftTowerHP = TowerMaxHP,
                        RightTowerHP = TowerMaxHP
                    WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();

                await LogAction("castle", "repair", $"修复城门: 城堡ID={id}");
                return Ok(ApiResult.Success("修复成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "修复城门失败");
                return Ok(ApiResult.Fail("修复失败"));
            }
        }

        #endregion

        #region 攻城战记录

        /// <summary>
        /// 获取攻城战历史记录
        /// </summary>
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetWarHistory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 查询总数
                using var countCmd = new MySqlCommand("SELECT COUNT(*) FROM castle_war_records WHERE CastleId = @CastleId", conn);
                countCmd.Parameters.AddWithValue("@CastleId", id);
                var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                // 查询数据
                using var cmd = new MySqlCommand($@"
                    SELECT * FROM castle_war_records 
                    WHERE CastleId = @CastleId 
                    ORDER BY StartTime DESC 
                    LIMIT @Offset, @PageSize", conn);
                cmd.Parameters.AddWithValue("@CastleId", id);
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt32("Id"),
                        WarDate = reader.GetDateTime("WarDate"),
                        StartTime = reader.GetDateTime("StartTime"),
                        EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : (DateTime?)reader.GetDateTime("EndTime"),
                        Duration = reader.GetInt32("Duration"),
                        DefenderGuildName = reader.IsDBNull(reader.GetOrdinal("DefenderGuildName")) ? null : reader.GetString("DefenderGuildName"),
                        WinnerGuildName = reader.IsDBNull(reader.GetOrdinal("WinnerGuildName")) ? null : reader.GetString("WinnerGuildName"),
                        WinnerName = reader.IsDBNull(reader.GetOrdinal("WinnerName")) ? null : reader.GetString("WinnerName"),
                        AttackerCount = reader.GetInt32("AttackerCount"),
                        TotalKills = reader.GetInt32("TotalKills"),
                        GateDestroyed = reader.GetBoolean("GateDestroyed"),
                        Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString("Result")
                    });
                }

                return Ok(ApiResult.Success(new { total, page, pageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取攻城战历史失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取攻城战击杀排行
        /// </summary>
        [HttpGet("war/{warId}/kills")]
        public async Task<IActionResult> GetWarKills(int warId, [FromQuery] int limit = 50)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand($@"
                    SELECT KillerName, KillerGuildName, COUNT(*) as Kills
                    FROM castle_war_kills 
                    WHERE WarRecordId = @WarId
                    GROUP BY KillerName, KillerGuildName
                    ORDER BY Kills DESC
                    LIMIT {limit}", conn);
                cmd.Parameters.AddWithValue("@WarId", warId);

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<object>();
                int rank = 1;
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Rank = rank++,
                        KillerName = reader.GetString("KillerName"),
                        GuildName = reader.IsDBNull(reader.GetOrdinal("KillerGuildName")) ? null : reader.GetString("KillerGuildName"),
                        Kills = reader.GetInt32("Kills")
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取击杀排行失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        #endregion

        #region 报名管理

        /// <summary>
        /// 获取报名列表
        /// </summary>
        [HttpGet("{id}/applications")]
        public async Task<IActionResult> GetApplications(int id, [FromQuery] string date = null)
        {
            var warDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT * FROM castle_war_applications 
                    WHERE CastleId = @CastleId AND WarDate >= @WarDate
                    ORDER BY WarDate, ApplyTime", conn);
                cmd.Parameters.AddWithValue("@CastleId", id);
                cmd.Parameters.AddWithValue("@WarDate", warDate.Date);

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt32("Id"),
                        WarDate = reader.GetDateTime("WarDate"),
                        GuildId = reader.GetInt32("GuildId"),
                        GuildName = reader.GetString("GuildName"),
                        GuildMaster = reader.GetString("GuildMaster"),
                        ApplyTime = reader.GetDateTime("ApplyTime"),
                        ApplyFee = reader.GetInt64("ApplyFee"),
                        MemberCount = reader.GetInt32("MemberCount"),
                        Status = reader.GetString("Status")
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取报名列表失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 取消报名
        /// </summary>
        [HttpPost("application/{appId}/cancel")]
        public async Task<IActionResult> CancelApplication(int appId)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(
                    "UPDATE castle_war_applications SET Status = 'cancelled' WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", appId);
                await cmd.ExecuteNonQueryAsync();

                await LogAction("castle", "cancel_application", $"取消攻城报名: ID={appId}");
                return Ok(ApiResult.Success("取消成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "取消报名失败");
                return Ok(ApiResult.Fail("取消失败"));
            }
        }

        #endregion

        #region 配置管理

        /// <summary>
        /// 获取攻城战配置
        /// </summary>
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT ConfigKey, ConfigValue, Description FROM castle_configs", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var config = new Dictionary<string, object>();
                while (await reader.ReadAsync())
                {
                    config[reader.GetString("ConfigKey")] = new
                    {
                        Value = reader.GetString("ConfigValue"),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description")
                    };
                }

                return Ok(ApiResult.Success(config));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取配置失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 更新攻城战配置
        /// </summary>
        [HttpPut("config")]
        public async Task<IActionResult> UpdateConfig([FromBody] Dictionary<string, string> configs)
        {
            if (!IsSuperAdmin)
                return Ok(ApiResult.Fail("需要超级管理员权限"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                foreach (var kv in configs)
                {
                    using var cmd = new MySqlCommand(
                        "UPDATE castle_configs SET ConfigValue = @Value WHERE ConfigKey = @Key", conn);
                    cmd.Parameters.AddWithValue("@Key", kv.Key);
                    cmd.Parameters.AddWithValue("@Value", kv.Value);
                    await cmd.ExecuteNonQueryAsync();
                }

                await LogAction("castle", "update_config", $"更新攻城战配置: {configs.Count}项");
                return Ok(ApiResult.Success("更新成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "更新配置失败");
                return Ok(ApiResult.Fail("更新失败"));
            }
        }

        #endregion
    }

    #region 请求模型

    public class UpdateCastleRequest
    {
        public int TaxRate { get; set; }
        public int DefenseLevel { get; set; }
        public int GuardCount { get; set; }
        public int GateMaxHP { get; set; }
        public int TowerMaxHP { get; set; }
    }

    public class SetOwnerRequest
    {
        public int? GuildId { get; set; }
        public string GuildName { get; set; }
        public string OwnerName { get; set; }
    }

    #endregion
}
