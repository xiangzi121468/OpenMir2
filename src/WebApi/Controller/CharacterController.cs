using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;

namespace WebApi.Controller
{
    /// <summary>
    /// 角色信息API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public CharacterController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";
        }

        /// <summary>
        /// 获取角色基础信息
        /// </summary>
        [HttpGet("{charName}")]
        public async Task<IActionResult> GetCharacter(string charName)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT c.*, 
                           g.GuildName, g.GuildRank,
                           (SELECT COUNT(*) FROM characters WHERE LoginID = c.LoginID AND Deleted = 0) as CharCount
                    FROM characters c
                    LEFT JOIN guild_members g ON c.ChrName = g.CharName
                    WHERE c.ChrName = @Name AND c.Deleted = 0", conn);
                cmd.Parameters.AddWithValue("@Name", charName);

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return Ok(new { code = -1, msg = "角色不存在" });
                }

                var character = new
                {
                    // 基本信息
                    ChrName = reader.GetString("ChrName"),
                    Level = reader.GetInt32("Level"),
                    Job = reader.GetInt32("Job"),
                    JobName = GetJobName(reader.GetInt32("Job")),
                    Sex = reader.GetInt32("Sex"),
                    SexName = reader.GetInt32("Sex") == 0 ? "男" : "女",
                    Hair = reader.GetInt32("Hair"),

                    // 位置
                    MapName = reader.GetString("MapName"),
                    CX = reader.GetInt32("CX"),
                    CY = reader.GetInt32("CY"),

                    // 货币
                    Gold = reader.GetInt32("Gold"),
                    GameGold = reader.IsDBNull(reader.GetOrdinal("GameGold")) ? 0 : reader.GetInt32("GameGold"),

                    // 行会
                    GuildName = reader.IsDBNull(reader.GetOrdinal("GuildName")) ? null : reader.GetString("GuildName"),

                    // 账号信息
                    LoginID = reader.GetString("LoginID"),
                    CharCount = reader.GetInt32("CharCount")
                };

                return Ok(new { code = 0, data = character });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取角色信息失败");
                return Ok(new { code = -1, msg = "查询失败" });
            }
        }

        /// <summary>
        /// 搜索角色
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchCharacters([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 2)
            {
                return Ok(new { code = -1, msg = "关键词至少2个字符" });
            }

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 查询总数
                using var countCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM characters WHERE ChrName LIKE @Keyword AND Deleted = 0", conn);
                countCmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                // 查询数据
                using var cmd = new MySqlCommand($@"
                    SELECT ChrName, Level, Job, Sex, MapName, Gold, GameGold
                    FROM characters 
                    WHERE ChrName LIKE @Keyword AND Deleted = 0
                    ORDER BY Level DESC
                    LIMIT @Offset, @PageSize", conn);
                cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        ChrName = reader.GetString("ChrName"),
                        Level = reader.GetInt32("Level"),
                        Job = reader.GetInt32("Job"),
                        JobName = GetJobName(reader.GetInt32("Job")),
                        Sex = reader.GetInt32("Sex"),
                        MapName = reader.GetString("MapName")
                    });
                }

                return Ok(new { code = 0, data = new { total, page, pageSize, items = list } });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "搜索角色失败");
                return Ok(new { code = -1, msg = "搜索失败" });
            }
        }

        /// <summary>
        /// 获取等级排行榜
        /// </summary>
        [HttpGet("rank/level")]
        public async Task<IActionResult> GetLevelRank([FromQuery] int limit = 50)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand($@"
                    SELECT ChrName, Level, Job, Sex, 
                           (SELECT GuildName FROM guild_members WHERE CharName = c.ChrName LIMIT 1) as GuildName
                    FROM characters c
                    WHERE Deleted = 0
                    ORDER BY Level DESC, Exp DESC
                    LIMIT {limit}", conn);

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<object>();
                int rank = 1;
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Rank = rank++,
                        ChrName = reader.GetString("ChrName"),
                        Level = reader.GetInt32("Level"),
                        Job = reader.GetInt32("Job"),
                        JobName = GetJobName(reader.GetInt32("Job")),
                        GuildName = reader.IsDBNull(reader.GetOrdinal("GuildName")) ? null : reader.GetString("GuildName")
                    });
                }

                return Ok(new { code = 0, data = list });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取等级排行失败");
                return Ok(new { code = -1, msg = "查询失败" });
            }
        }

        /// <summary>
        /// 获取财富排行榜
        /// </summary>
        [HttpGet("rank/wealth")]
        public async Task<IActionResult> GetWealthRank([FromQuery] int limit = 50)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand($@"
                    SELECT ChrName, Level, Job, Gold, GameGold, (Gold + GameGold * 100) as TotalWealth
                    FROM characters
                    WHERE Deleted = 0
                    ORDER BY TotalWealth DESC
                    LIMIT {limit}", conn);

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<object>();
                int rank = 1;
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Rank = rank++,
                        ChrName = reader.GetString("ChrName"),
                        Level = reader.GetInt32("Level"),
                        JobName = GetJobName(reader.GetInt32("Job")),
                        Gold = reader.GetInt32("Gold"),
                        GameGold = reader.IsDBNull(reader.GetOrdinal("GameGold")) ? 0 : reader.GetInt32("GameGold")
                    });
                }

                return Ok(new { code = 0, data = list });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取财富排行失败");
                return Ok(new { code = -1, msg = "查询失败" });
            }
        }

        /// <summary>
        /// 获取账号下的所有角色
        /// </summary>
        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetAccountCharacters(string accountId)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT ChrName, Level, Job, Sex, MapName, Gold, GameGold, CreateDate
                    FROM characters
                    WHERE LoginID = @AccountId AND Deleted = 0
                    ORDER BY Level DESC", conn);
                cmd.Parameters.AddWithValue("@AccountId", accountId);

                using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<object>();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        ChrName = reader.GetString("ChrName"),
                        Level = reader.GetInt32("Level"),
                        Job = reader.GetInt32("Job"),
                        JobName = GetJobName(reader.GetInt32("Job")),
                        Sex = reader.GetInt32("Sex"),
                        MapName = reader.GetString("MapName"),
                        Gold = reader.GetInt32("Gold"),
                        GameGold = reader.IsDBNull(reader.GetOrdinal("GameGold")) ? 0 : reader.GetInt32("GameGold")
                    });
                }

                return Ok(new { code = 0, data = list });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取账号角色失败");
                return Ok(new { code = -1, msg = "查询失败" });
            }
        }

        /// <summary>
        /// 获取在线玩家列表
        /// </summary>
        [HttpGet("online")]
        public async Task<IActionResult> GetOnlinePlayers([FromQuery] string mapName = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            // 这个需要从游戏服务器获取实时数据
            // 暂时返回空列表，实际应该通过GameSrv的WorldEngine获取
            return Ok(new
            {
                code = 0,
                msg = "需要从游戏服务器获取实时数据",
                data = new
                {
                    total = 0,
                    page,
                    pageSize,
                    items = new List<object>()
                }
            });
        }

        private string GetJobName(int job)
        {
            return job switch
            {
                0 => "战士",
                1 => "法师",
                2 => "道士",
                _ => "未知"
            };
        }
    }
}
