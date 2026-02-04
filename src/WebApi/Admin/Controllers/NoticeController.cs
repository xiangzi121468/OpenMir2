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
    /// 公告管理
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    public class NoticeController : AdminBaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AdminService _adminService;
        private readonly string _connectionString;

        public NoticeController(AdminService adminService, IConfiguration configuration)
        {
            _adminService = adminService;
            _connectionString = configuration.GetConnectionString("Default")
                ?? "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";
        }

        /// <summary>
        /// 获取公告列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetNotices([FromQuery] string type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = "WHERE 1=1";
                if (!string.IsNullOrEmpty(type))
                    where += " AND NoticeType = @Type";

                // 总数
                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM game_notices {where}", conn);
                if (!string.IsNullOrEmpty(type))
                    countCmd.Parameters.AddWithValue("@Type", type);
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                // 列表
                int offset = (page - 1) * pageSize;
                using var cmd = new MySqlCommand($@"
                    SELECT * FROM game_notices {where} 
                    ORDER BY Priority DESC, Id DESC 
                    LIMIT {offset},{pageSize}", conn);
                if (!string.IsNullOrEmpty(type))
                    cmd.Parameters.AddWithValue("@Type", type);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        id = reader.GetInt32("Id"),
                        title = reader.GetString("Title"),
                        content = reader.GetString("Content"),
                        noticeType = reader.GetString("NoticeType"),
                        priority = reader.GetInt32("Priority"),
                        color = reader.IsDBNull(reader.GetOrdinal("Color")) ? "yellow" : reader.GetString("Color"),
                        targetMap = reader.IsDBNull(reader.GetOrdinal("TargetMap")) ? null : reader.GetString("TargetMap"),
                        targetLevel = reader.GetInt32("TargetLevel"),
                        targetVip = reader.GetInt32("TargetVip"),
                        repeatCount = reader.GetInt32("RepeatCount"),
                        repeatInterval = reader.GetInt32("RepeatInterval"),
                        startTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? null : (DateTime?)reader.GetDateTime("StartTime"),
                        endTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : (DateTime?)reader.GetDateTime("EndTime"),
                        scheduleCron = reader.IsDBNull(reader.GetOrdinal("ScheduleCron")) ? null : reader.GetString("ScheduleCron"),
                        lastSentTime = reader.IsDBNull(reader.GetOrdinal("LastSentTime")) ? null : (DateTime?)reader.GetDateTime("LastSentTime"),
                        sentCount = reader.GetInt32("SentCount"),
                        isEnabled = reader.GetBoolean("IsEnabled"),
                        adminName = reader.IsDBNull(reader.GetOrdinal("AdminName")) ? null : reader.GetString("AdminName"),
                        createTime = reader.GetDateTime("CreateTime")
                    });
                }

                return Ok(ApiResult.Success(new { total, page, pageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取公告列表失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 获取公告详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotice(int id)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT * FROM game_notices WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Ok(ApiResult.Fail("公告不存在"));

                var notice = new
                {
                    id = reader.GetInt32("Id"),
                    title = reader.GetString("Title"),
                    content = reader.GetString("Content"),
                    noticeType = reader.GetString("NoticeType"),
                    priority = reader.GetInt32("Priority"),
                    color = reader.IsDBNull(reader.GetOrdinal("Color")) ? "yellow" : reader.GetString("Color"),
                    targetMap = reader.IsDBNull(reader.GetOrdinal("TargetMap")) ? null : reader.GetString("TargetMap"),
                    targetLevel = reader.GetInt32("TargetLevel"),
                    targetVip = reader.GetInt32("TargetVip"),
                    repeatCount = reader.GetInt32("RepeatCount"),
                    repeatInterval = reader.GetInt32("RepeatInterval"),
                    startTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? null : (DateTime?)reader.GetDateTime("StartTime"),
                    endTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : (DateTime?)reader.GetDateTime("EndTime"),
                    scheduleCron = reader.IsDBNull(reader.GetOrdinal("ScheduleCron")) ? null : reader.GetString("ScheduleCron"),
                    isEnabled = reader.GetBoolean("IsEnabled"),
                    createTime = reader.GetDateTime("CreateTime")
                };

                return Ok(ApiResult.Success(notice));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取公告详情失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 添加公告
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddNotice([FromBody] AddNoticeRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            if (string.IsNullOrEmpty(request?.Title) || string.IsNullOrEmpty(request.Content))
                return Ok(ApiResult.Fail("标题和内容不能为空"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO game_notices (Title, Content, NoticeType, Priority, Color, TargetMap, TargetLevel, TargetVip,
                        RepeatCount, RepeatInterval, StartTime, EndTime, ScheduleCron, IsEnabled, AdminId, AdminName, CreateTime)
                    VALUES (@Title, @Content, @NoticeType, @Priority, @Color, @TargetMap, @TargetLevel, @TargetVip,
                        @RepeatCount, @RepeatInterval, @StartTime, @EndTime, @ScheduleCron, @IsEnabled, @AdminId, @AdminName, NOW())", conn);

                cmd.Parameters.AddWithValue("@Title", request.Title);
                cmd.Parameters.AddWithValue("@Content", request.Content);
                cmd.Parameters.AddWithValue("@NoticeType", request.NoticeType ?? "broadcast");
                cmd.Parameters.AddWithValue("@Priority", request.Priority);
                cmd.Parameters.AddWithValue("@Color", request.Color ?? "yellow");
                cmd.Parameters.AddWithValue("@TargetMap", request.TargetMap ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TargetLevel", request.TargetLevel);
                cmd.Parameters.AddWithValue("@TargetVip", request.TargetVip);
                cmd.Parameters.AddWithValue("@RepeatCount", request.RepeatCount);
                cmd.Parameters.AddWithValue("@RepeatInterval", request.RepeatInterval);
                cmd.Parameters.AddWithValue("@StartTime", request.StartTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EndTime", request.EndTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ScheduleCron", request.ScheduleCron ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsEnabled", request.IsEnabled);
                cmd.Parameters.AddWithValue("@AdminId", CurrentAdminId);
                cmd.Parameters.AddWithValue("@AdminName", CurrentAdminName);

                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "notice", "add",
                    request.Title, $"添加公告: {request.NoticeType}", ClientIp);

                return Ok(ApiResult.Success(null, "添加成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "添加公告失败");
                return Ok(ApiResult.Fail("添加失败"));
            }
        }

        /// <summary>
        /// 更新公告
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotice(int id, [FromBody] AddNoticeRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

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

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Title", request.Title);
                cmd.Parameters.AddWithValue("@Content", request.Content);
                cmd.Parameters.AddWithValue("@NoticeType", request.NoticeType ?? "broadcast");
                cmd.Parameters.AddWithValue("@Priority", request.Priority);
                cmd.Parameters.AddWithValue("@Color", request.Color ?? "yellow");
                cmd.Parameters.AddWithValue("@TargetMap", request.TargetMap ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TargetLevel", request.TargetLevel);
                cmd.Parameters.AddWithValue("@TargetVip", request.TargetVip);
                cmd.Parameters.AddWithValue("@RepeatCount", request.RepeatCount);
                cmd.Parameters.AddWithValue("@RepeatInterval", request.RepeatInterval);
                cmd.Parameters.AddWithValue("@StartTime", request.StartTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EndTime", request.EndTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ScheduleCron", request.ScheduleCron ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsEnabled", request.IsEnabled);

                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "notice", "update",
                    id.ToString(), $"更新公告: {request.Title}", ClientIp);

                return Ok(ApiResult.Success(null, "更新成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "更新公告失败");
                return Ok(ApiResult.Fail("更新失败"));
            }
        }

        /// <summary>
        /// 删除公告
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotice(int id)
        {
            if (!IsSuperAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("DELETE FROM game_notices WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "notice", "delete",
                    id.ToString(), "删除公告", ClientIp);

                return Ok(ApiResult.Success(null, "删除成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "删除公告失败");
                return Ok(ApiResult.Fail("删除失败"));
            }
        }

        /// <summary>
        /// 启用/禁用公告
        /// </summary>
        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleNotice(int id, [FromBody] ToggleRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("UPDATE game_notices SET IsEnabled = @Enabled WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Enabled", request.Enabled);
                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "notice", "toggle",
                    id.ToString(), request.Enabled ? "启用公告" : "禁用公告", ClientIp);

                return Ok(ApiResult.Success(null, request.Enabled ? "已启用" : "已禁用"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "切换公告状态失败");
                return Ok(ApiResult.Fail("操作失败"));
            }
        }

        /// <summary>
        /// 立即发送公告
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotice([FromBody] SendNoticeRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            if (string.IsNullOrEmpty(request?.Content))
                return Ok(ApiResult.Fail("内容不能为空"));

            try
            {
                // 记录发送日志
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO notice_send_logs (NoticeId, NoticeType, TargetCount, Content, SendTime)
                    VALUES (0, @Type, 0, @Content, NOW())", conn);
                cmd.Parameters.AddWithValue("@Type", request.NoticeType ?? "broadcast");
                cmd.Parameters.AddWithValue("@Content", request.Content);
                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "notice", "send",
                    null, $"发送公告: {request.Content.Substring(0, Math.Min(50, request.Content.Length))}", ClientIp);

                // 实际发送需要通过游戏服务器
                // 这里返回成功，由游戏服务器轮询或推送接口获取待发送公告

                return Ok(ApiResult.Success(null, "发送指令已提交"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "发送公告失败");
                return Ok(ApiResult.Fail("发送失败"));
            }
        }

        /// <summary>
        /// 获取发送记录
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetSendLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var countCmd = new MySqlCommand("SELECT COUNT(*) FROM notice_send_logs", conn);
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                int offset = (page - 1) * pageSize;
                using var cmd = new MySqlCommand($@"
                    SELECT * FROM notice_send_logs ORDER BY Id DESC LIMIT {offset},{pageSize}", conn);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        id = reader.GetInt64("Id"),
                        noticeId = reader.GetInt32("NoticeId"),
                        noticeType = reader.GetString("NoticeType"),
                        targetCount = reader.GetInt32("TargetCount"),
                        content = reader.IsDBNull(reader.GetOrdinal("Content")) ? null : reader.GetString("Content"),
                        sendTime = reader.GetDateTime("SendTime")
                    });
                }

                return Ok(ApiResult.Success(new { total, page, pageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取发送记录失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }
    }

    public class AddNoticeRequest
    {
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
        public bool IsEnabled { get; set; } = true;
    }

    public class ToggleRequest
    {
        public bool Enabled { get; set; }
    }

    public class SendNoticeRequest
    {
        public string Content { get; set; }
        public string NoticeType { get; set; }
        public string Color { get; set; }
        public string TargetMap { get; set; }
    }
}
