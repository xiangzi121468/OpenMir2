using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;
using WebApi.Admin.Models;
using WebApi.Admin.Services;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 玩家管理
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    public class PlayerController : AdminBaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AdminService _adminService;
        private readonly string _connectionString;

        public PlayerController(AdminService adminService, IConfiguration configuration)
        {
            _adminService = adminService;
            _connectionString = configuration.GetConnectionString("Default") 
                ?? "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";
        }

        /// <summary>
        /// 搜索玩家
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchPlayers([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = "WHERE 1=1";
                if (!string.IsNullOrEmpty(keyword))
                {
                    where += " AND (ChrName LIKE @Keyword OR LoginID LIKE @Keyword)";
                }

                // 总数
                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM characters {where}", conn);
                if (!string.IsNullOrEmpty(keyword))
                    countCmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                // 列表
                int offset = (page - 1) * pageSize;
                using var cmd = new MySqlCommand($@"
                    SELECT Id, LoginID, ChrName, Job, Level, Sex, Gold, GamePoint, MapName, CX, CY, 
                           PkPoint, Deleted, CREATEDATE, LASTUPDATE 
                    FROM characters {where} ORDER BY Id DESC LIMIT {offset},{pageSize}", conn);
                if (!string.IsNullOrEmpty(keyword))
                    cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt32("Id"),
                        LoginId = reader.GetString("LoginID"),
                        ChrName = reader.GetString("ChrName"),
                        Job = reader.GetInt32("Job"),
                        Level = reader.GetInt32("Level"),
                        Sex = reader.GetInt32("Sex"),
                        Gold = reader.GetInt32("Gold"),
                        GamePoint = reader.GetInt32("GamePoint"),
                        MapName = reader.GetString("MapName"),
                        X = reader.GetInt32("CX"),
                        Y = reader.GetInt32("CY"),
                        PkPoint = reader.GetInt32("PkPoint"),
                        Deleted = reader.GetBoolean("Deleted"),
                        CreateTime = reader.GetDateTime("CREATEDATE"),
                        LastUpdate = reader.GetDateTime("LASTUPDATE")
                    });
                }

                return Ok(ApiResult.Success(new { total, page, pageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "搜索玩家失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取玩家详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(int id)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT * FROM characters WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Ok(ApiResult.Fail("玩家不存在"));

                var player = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    player[name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }

                return Ok(ApiResult.Success(player));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取玩家详情失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 修改玩家元宝
        /// </summary>
        [HttpPost("{id}/gold")]
        public async Task<IActionResult> ModifyGold(int id, [FromBody] ModifyGoldRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 获取玩家信息
                using var getCmd = new MySqlCommand("SELECT ChrName, Gold FROM characters WHERE Id=@Id", conn);
                getCmd.Parameters.AddWithValue("@Id", id);
                using var reader = await getCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Ok(ApiResult.Fail("玩家不存在"));

                string chrName = reader.GetString("ChrName");
                int currentGold = reader.GetInt32("Gold");
                reader.Close();

                // 计算新值
                int newGold = request.Type switch
                {
                    "set" => request.Amount,
                    "add" => currentGold + request.Amount,
                    "sub" => Math.Max(0, currentGold - request.Amount),
                    _ => currentGold
                };

                // 更新
                using var updateCmd = new MySqlCommand("UPDATE characters SET Gold=@Gold WHERE Id=@Id", conn);
                updateCmd.Parameters.AddWithValue("@Id", id);
                updateCmd.Parameters.AddWithValue("@Gold", newGold);
                await updateCmd.ExecuteNonQueryAsync();

                // 记录日志
                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "player", "modify_gold",
                    chrName, $"修改元宝: {currentGold} -> {newGold} ({request.Type} {request.Amount})", ClientIp);

                return Ok(ApiResult.Success(new { oldGold = currentGold, newGold }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "修改元宝失败");
                return Ok(ApiResult.Fail("修改失败"));
            }
        }

        /// <summary>
        /// 修改玩家等级
        /// </summary>
        [HttpPost("{id}/level")]
        public async Task<IActionResult> ModifyLevel(int id, [FromBody] ModifyLevelRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 获取玩家信息
                using var getCmd = new MySqlCommand("SELECT ChrName, Level FROM characters WHERE Id=@Id", conn);
                getCmd.Parameters.AddWithValue("@Id", id);
                using var reader = await getCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Ok(ApiResult.Fail("玩家不存在"));

                string chrName = reader.GetString("ChrName");
                int currentLevel = reader.GetInt32("Level");
                reader.Close();

                if (request.Level < 1 || request.Level > 255)
                    return Ok(ApiResult.Fail("等级范围1-255"));

                // 更新
                using var updateCmd = new MySqlCommand("UPDATE characters SET Level=@Level WHERE Id=@Id", conn);
                updateCmd.Parameters.AddWithValue("@Id", id);
                updateCmd.Parameters.AddWithValue("@Level", request.Level);
                await updateCmd.ExecuteNonQueryAsync();

                // 记录日志
                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "player", "modify_level",
                    chrName, $"修改等级: {currentLevel} -> {request.Level}", ClientIp);

                return Ok(ApiResult.Success(new { oldLevel = currentLevel, newLevel = request.Level }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "修改等级失败");
                return Ok(ApiResult.Fail("修改失败"));
            }
        }

        /// <summary>
        /// 封禁玩家
        /// </summary>
        [HttpPost("{id}/ban")]
        public async Task<IActionResult> BanPlayer(int id, [FromBody] BanPlayerRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 获取玩家信息
                using var getCmd = new MySqlCommand("SELECT ChrName, LoginID FROM characters WHERE Id=@Id", conn);
                getCmd.Parameters.AddWithValue("@Id", id);
                using var reader = await getCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Ok(ApiResult.Fail("玩家不存在"));

                string chrName = reader.GetString("ChrName");
                string loginId = reader.GetString("LoginID");
                reader.Close();

                // 确定封禁值
                string banValue = request.BanType switch
                {
                    "account" => loginId,
                    _ => request.BanValue ?? loginId
                };

                DateTime? endTime = request.Duration > 0 ? DateTime.Now.AddMinutes(request.Duration) : null;

                // 插入封禁记录
                using var insertCmd = new MySqlCommand(@"
                    INSERT INTO ban_records (BanType, BanValue, CharName, Reason, Duration, StartTime, EndTime, AdminId, AdminName, Status)
                    VALUES (@BanType, @BanValue, @CharName, @Reason, @Duration, NOW(), @EndTime, @AdminId, @AdminName, 1)", conn);
                insertCmd.Parameters.AddWithValue("@BanType", request.BanType);
                insertCmd.Parameters.AddWithValue("@BanValue", banValue);
                insertCmd.Parameters.AddWithValue("@CharName", chrName);
                insertCmd.Parameters.AddWithValue("@Reason", request.Reason);
                insertCmd.Parameters.AddWithValue("@Duration", request.Duration);
                insertCmd.Parameters.AddWithValue("@EndTime", endTime ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@AdminId", CurrentAdminId);
                insertCmd.Parameters.AddWithValue("@AdminName", CurrentAdminName);
                await insertCmd.ExecuteNonQueryAsync();

                // 记录日志
                string durationStr = request.Duration > 0 ? $"{request.Duration}分钟" : "永久";
                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "player", "ban",
                    chrName, $"封禁玩家: {request.BanType}={banValue}, 时长:{durationStr}, 原因:{request.Reason}", ClientIp);

                return Ok(ApiResult.Success(null, "封禁成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "封禁玩家失败");
                return Ok(ApiResult.Fail("封禁失败"));
            }
        }

        /// <summary>
        /// 解封玩家
        /// </summary>
        [HttpPost("unban/{banId}")]
        public async Task<IActionResult> UnbanPlayer(long banId)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE ban_records SET Status=0, UnbanTime=NOW(), UnbanAdminId=@AdminId 
                    WHERE Id=@Id AND Status=1", conn);
                cmd.Parameters.AddWithValue("@Id", banId);
                cmd.Parameters.AddWithValue("@AdminId", CurrentAdminId);

                int affected = await cmd.ExecuteNonQueryAsync();
                if (affected == 0)
                    return Ok(ApiResult.Fail("记录不存在或已解封"));

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "player", "unban",
                    banId.ToString(), "解除封禁", ClientIp);

                return Ok(ApiResult.Success(null, "解封成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "解封失败");
                return Ok(ApiResult.Fail("解封失败"));
            }
        }

        /// <summary>
        /// 获取封禁列表
        /// </summary>
        [HttpGet("bans")]
        public async Task<IActionResult> GetBanList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? status = null)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = "WHERE 1=1";
                if (status.HasValue)
                    where += " AND Status=@Status";

                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM ban_records {where}", conn);
                if (status.HasValue)
                    countCmd.Parameters.AddWithValue("@Status", status.Value);
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                int offset = (page - 1) * pageSize;
                using var cmd = new MySqlCommand($"SELECT * FROM ban_records {where} ORDER BY Id DESC LIMIT {offset},{pageSize}", conn);
                if (status.HasValue)
                    cmd.Parameters.AddWithValue("@Status", status.Value);

                var list = new List<BanRecord>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new BanRecord
                    {
                        Id = reader.GetInt64("Id"),
                        BanType = reader.GetString("BanType"),
                        BanValue = reader.GetString("BanValue"),
                        CharName = reader.IsDBNull(reader.GetOrdinal("CharName")) ? null : reader.GetString("CharName"),
                        Reason = reader.GetString("Reason"),
                        Duration = reader.GetInt32("Duration"),
                        StartTime = reader.GetDateTime("StartTime"),
                        EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime("EndTime"),
                        AdminId = reader.GetInt32("AdminId"),
                        AdminName = reader.GetString("AdminName"),
                        Status = reader.GetInt32("Status"),
                        CreateTime = reader.GetDateTime("CreateTime")
                    });
                }

                return Ok(ApiResult.Success(new { total, page, pageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取封禁列表失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 传送玩家
        /// </summary>
        [HttpPost("{id}/teleport")]
        public async Task<IActionResult> TeleportPlayer(int id, [FromBody] TeleportRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(
                    "UPDATE characters SET MapName=@MapName, CX=@X, CY=@Y WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@MapName", request.MapName);
                cmd.Parameters.AddWithValue("@X", request.X);
                cmd.Parameters.AddWithValue("@Y", request.Y);
                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "player", "teleport",
                    id.ToString(), $"传送到 {request.MapName}({request.X},{request.Y})", ClientIp);

                return Ok(ApiResult.Success(null, "传送成功(下次登录生效)"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "传送失败");
                return Ok(ApiResult.Fail("传送失败"));
            }
        }
    }

    public class ModifyGoldRequest
    {
        public string Type { get; set; } // set, add, sub
        public int Amount { get; set; }
    }

    public class ModifyLevelRequest
    {
        public int Level { get; set; }
    }

    public class TeleportRequest
    {
        public string MapName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
