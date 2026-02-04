using MySqlConnector;
using NLog;
using System.Text.Json;

namespace MailModule
{
    /// <summary>
    /// 邮件服务
    /// </summary>
    public class MailService : IMailService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public MailService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 邮件管理

        /// <summary>
        /// 获取邮件列表
        /// </summary>
        public async Task<(List<GameMail> Items, int Total)> GetMailsAsync(
            string? search = null, 
            string? mailType = null, 
            string? status = null,
            int page = 1, 
            int pageSize = 20)
        {
            var mails = new List<GameMail>();
            int total = 0;

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 构建查询条件
                var where = "WHERE 1=1";
                if (!string.IsNullOrEmpty(search))
                    where += " AND (Title LIKE @Search OR SenderName LIKE @Search)";
                if (!string.IsNullOrEmpty(mailType))
                    where += " AND MailType = @MailType";
                if (!string.IsNullOrEmpty(status))
                    where += " AND Status = @Status";

                // 查询总数
                var countSql = $"SELECT COUNT(*) FROM game_mails {where}";
                await using (var countCmd = new MySqlCommand(countSql, conn))
                {
                    if (!string.IsNullOrEmpty(search))
                        countCmd.Parameters.AddWithValue("@Search", $"%{search}%");
                    if (!string.IsNullOrEmpty(mailType))
                        countCmd.Parameters.AddWithValue("@MailType", mailType);
                    if (!string.IsNullOrEmpty(status))
                        countCmd.Parameters.AddWithValue("@Status", status);
                    total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                }

                // 查询数据
                var sql = $@"SELECT * FROM game_mails {where} 
                             ORDER BY CreateTime DESC 
                             LIMIT @Offset, @PageSize";
                await using var cmd = new MySqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                if (!string.IsNullOrEmpty(mailType))
                    cmd.Parameters.AddWithValue("@MailType", mailType);
                if (!string.IsNullOrEmpty(status))
                    cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    mails.Add(ReadMail(reader));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取邮件列表失败");
            }

            return (mails, total);
        }

        /// <summary>
        /// 创建邮件
        /// </summary>
        public async Task<int> CreateMailAsync(CreateMailRequest request, string createBy)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"INSERT INTO game_mails 
                    (SenderId, SenderName, ReceiverType, ReceiverId, ReceiverName, ReceiverCondition,
                     Title, Content, Attachments, Gold, GameGold, ExpireHours, MailType, Priority, Status, CreateBy)
                    VALUES 
                    (@SenderId, @SenderName, @ReceiverType, @ReceiverId, @ReceiverName, @ReceiverCondition,
                     @Title, @Content, @Attachments, @Gold, @GameGold, @ExpireHours, @MailType, @Priority, @Status, @CreateBy);
                    SELECT LAST_INSERT_ID();";

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@SenderId", request.SenderId ?? "SYSTEM");
                cmd.Parameters.AddWithValue("@SenderName", request.SenderName ?? "系统");
                cmd.Parameters.AddWithValue("@ReceiverType", request.ReceiverType);
                cmd.Parameters.AddWithValue("@ReceiverId", request.ReceiverId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ReceiverName", request.ReceiverName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ReceiverCondition", request.ReceiverCondition ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", request.Title);
                cmd.Parameters.AddWithValue("@Content", request.Content);
                cmd.Parameters.AddWithValue("@Attachments", request.Attachments != null ? JsonSerializer.Serialize(request.Attachments) : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Gold", request.Gold);
                cmd.Parameters.AddWithValue("@GameGold", request.GameGold);
                cmd.Parameters.AddWithValue("@ExpireHours", request.ExpireHours);
                cmd.Parameters.AddWithValue("@MailType", request.MailType);
                cmd.Parameters.AddWithValue("@Priority", request.Priority);
                cmd.Parameters.AddWithValue("@Status", request.SendNow ? "sent" : "pending");
                cmd.Parameters.AddWithValue("@CreateBy", createBy);

                var mailId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                // 如果立即发送，创建玩家邮件记录
                if (request.SendNow)
                {
                    await SendMailToPlayersAsync(conn, mailId, request);
                }

                Logger.Info($"创建邮件成功: ID={mailId}, Title={request.Title}, By={createBy}");
                return mailId;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "创建邮件失败");
                return 0;
            }
        }

        /// <summary>
        /// 发送邮件给玩家
        /// </summary>
        private async Task SendMailToPlayersAsync(MySqlConnection conn, int mailId, CreateMailRequest request)
        {
            var expireTime = DateTime.Now.AddHours(request.ExpireHours);
            var totalReceivers = 0;

            switch (request.ReceiverType)
            {
                case "single":
                    // 单人发送
                    if (!string.IsNullOrEmpty(request.ReceiverName))
                    {
                        var accountId = await GetAccountIdByCharNameAsync(conn, request.ReceiverName);
                        if (!string.IsNullOrEmpty(accountId))
                        {
                            await InsertPlayerMailAsync(conn, mailId, accountId, request.ReceiverName, expireTime);
                            totalReceivers = 1;
                        }
                    }
                    break;

                case "all":
                    // 全服发送
                    var allPlayers = await GetAllPlayersAsync(conn);
                    foreach (var (accountId, charName) in allPlayers)
                    {
                        await InsertPlayerMailAsync(conn, mailId, accountId, charName, expireTime);
                        totalReceivers++;
                    }
                    break;

                case "level":
                    // 按等级发送
                    var levelCondition = JsonSerializer.Deserialize<LevelCondition>(request.ReceiverCondition ?? "{}");
                    var levelPlayers = await GetPlayersByLevelAsync(conn, levelCondition?.MinLevel ?? 0, levelCondition?.MaxLevel ?? 999);
                    foreach (var (accountId, charName) in levelPlayers)
                    {
                        await InsertPlayerMailAsync(conn, mailId, accountId, charName, expireTime);
                        totalReceivers++;
                    }
                    break;

                case "vip":
                    // 按VIP发送
                    var vipCondition = JsonSerializer.Deserialize<VipCondition>(request.ReceiverCondition ?? "{}");
                    var vipPlayers = await GetPlayersByVipAsync(conn, vipCondition?.MinVip ?? 0);
                    foreach (var (accountId, charName) in vipPlayers)
                    {
                        await InsertPlayerMailAsync(conn, mailId, accountId, charName, expireTime);
                        totalReceivers++;
                    }
                    break;
            }

            // 更新发送统计
            await using var updateCmd = new MySqlCommand(
                "UPDATE game_mails SET SendTime = NOW(), TotalReceivers = @Total WHERE Id = @Id", conn);
            updateCmd.Parameters.AddWithValue("@Total", totalReceivers);
            updateCmd.Parameters.AddWithValue("@Id", mailId);
            await updateCmd.ExecuteNonQueryAsync();
        }

        private async Task InsertPlayerMailAsync(MySqlConnection conn, int mailId, string accountId, string charName, DateTime expireTime)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO player_mails (MailId, AccountId, CharName, ExpireTime)
                VALUES (@MailId, @AccountId, @CharName, @ExpireTime)", conn);
            cmd.Parameters.AddWithValue("@MailId", mailId);
            cmd.Parameters.AddWithValue("@AccountId", accountId);
            cmd.Parameters.AddWithValue("@CharName", charName);
            cmd.Parameters.AddWithValue("@ExpireTime", expireTime);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public async Task<bool> SendMailAsync(int mailId)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 获取邮件信息
                await using var getCmd = new MySqlCommand("SELECT * FROM game_mails WHERE Id = @Id AND Status = 'pending'", conn);
                getCmd.Parameters.AddWithValue("@Id", mailId);
                await using var reader = await getCmd.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                    return false;

                var mail = ReadMail(reader);
                reader.Close();

                // 创建请求对象
                var request = new CreateMailRequest
                {
                    ReceiverType = mail.ReceiverType,
                    ReceiverName = mail.ReceiverName,
                    ReceiverCondition = mail.ReceiverCondition,
                    ExpireHours = mail.ExpireHours
                };

                // 发送给玩家
                await SendMailToPlayersAsync(conn, mailId, request);

                // 更新状态
                await using var updateCmd = new MySqlCommand(
                    "UPDATE game_mails SET Status = 'sent', SendTime = NOW() WHERE Id = @Id", conn);
                updateCmd.Parameters.AddWithValue("@Id", mailId);
                await updateCmd.ExecuteNonQueryAsync();

                Logger.Info($"发送邮件成功: ID={mailId}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"发送邮件失败: ID={mailId}");
                return false;
            }
        }

        /// <summary>
        /// 删除邮件
        /// </summary>
        public async Task<bool> DeleteMailAsync(int mailId)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new MySqlCommand("DELETE FROM game_mails WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", mailId);
                await cmd.ExecuteNonQueryAsync();

                Logger.Info($"删除邮件: ID={mailId}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"删除邮件失败: ID={mailId}");
                return false;
            }
        }

        #endregion

        #region 玩家邮件

        /// <summary>
        /// 获取玩家邮件列表
        /// </summary>
        public async Task<List<PlayerMailInfo>> GetPlayerMailsAsync(string charName, bool includeRead = true)
        {
            var mails = new List<PlayerMailInfo>();

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = includeRead ? "" : "AND pm.IsRead = 0";
                var sql = $@"SELECT pm.*, gm.Title, gm.Content, gm.SenderName, gm.Attachments, 
                                    gm.Gold, gm.GameGold, gm.MailType, gm.Priority
                             FROM player_mails pm
                             JOIN game_mails gm ON pm.MailId = gm.Id
                             WHERE pm.CharName = @CharName AND pm.IsDeleted = 0 
                                   AND pm.ExpireTime > NOW() {where}
                             ORDER BY pm.CreateTime DESC";

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CharName", charName);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    mails.Add(new PlayerMailInfo
                    {
                        Id = reader.GetInt64("Id"),
                        MailId = reader.GetInt32("MailId"),
                        Title = reader.GetString("Title"),
                        Content = reader.GetString("Content"),
                        SenderName = reader.GetString("SenderName"),
                        Attachments = reader.IsDBNull(reader.GetOrdinal("Attachments")) ? null : reader.GetString("Attachments"),
                        Gold = reader.GetInt32("Gold"),
                        GameGold = reader.GetInt32("GameGold"),
                        MailType = reader.GetString("MailType"),
                        Priority = reader.GetInt32("Priority"),
                        IsRead = reader.GetBoolean("IsRead"),
                        IsClaimed = reader.GetBoolean("IsClaimed"),
                        ExpireTime = reader.GetDateTime("ExpireTime"),
                        CreateTime = reader.GetDateTime("CreateTime")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"获取玩家邮件失败: {charName}");
            }

            return mails;
        }

        /// <summary>
        /// 阅读邮件
        /// </summary>
        public async Task<bool> ReadMailAsync(long playerMailId, string charName)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new MySqlCommand(@"
                    UPDATE player_mails SET IsRead = 1, ReadTime = NOW() 
                    WHERE Id = @Id AND CharName = @CharName", conn);
                cmd.Parameters.AddWithValue("@Id", playerMailId);
                cmd.Parameters.AddWithValue("@CharName", charName);
                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"阅读邮件失败: ID={playerMailId}");
                return false;
            }
        }

        /// <summary>
        /// 领取邮件附件
        /// </summary>
        public async Task<ClaimResult> ClaimMailAsync(long playerMailId, string charName)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 获取邮件信息
                var sql = @"SELECT pm.*, gm.Attachments, gm.Gold, gm.GameGold
                            FROM player_mails pm
                            JOIN game_mails gm ON pm.MailId = gm.Id
                            WHERE pm.Id = @Id AND pm.CharName = @CharName AND pm.IsClaimed = 0";
                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", playerMailId);
                cmd.Parameters.AddWithValue("@CharName", charName);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return new ClaimResult { Success = false, Message = "邮件不存在或已领取" };
                }

                var attachments = reader.IsDBNull(reader.GetOrdinal("Attachments")) ? null : reader.GetString("Attachments");
                var gold = reader.GetInt32("Gold");
                var gameGold = reader.GetInt32("GameGold");
                var mailId = reader.GetInt32("MailId");
                reader.Close();

                // 更新领取状态
                await using var updateCmd = new MySqlCommand(@"
                    UPDATE player_mails SET IsClaimed = 1, ClaimTime = NOW() 
                    WHERE Id = @Id", conn);
                updateCmd.Parameters.AddWithValue("@Id", playerMailId);
                await updateCmd.ExecuteNonQueryAsync();

                // 更新领取统计
                await using var statsCmd = new MySqlCommand(
                    "UPDATE game_mails SET ClaimedCount = ClaimedCount + 1 WHERE Id = @Id", conn);
                statsCmd.Parameters.AddWithValue("@Id", mailId);
                await statsCmd.ExecuteNonQueryAsync();

                // 发放奖励（金币和元宝直接发放，物品需要游戏服务器处理）
                if (gameGold > 0)
                {
                    await using var goldCmd = new MySqlCommand(
                        "UPDATE characters SET GameGold = GameGold + @Gold WHERE ChrName = @CharName", conn);
                    goldCmd.Parameters.AddWithValue("@Gold", gameGold);
                    goldCmd.Parameters.AddWithValue("@CharName", charName);
                    await goldCmd.ExecuteNonQueryAsync();
                }

                if (gold > 0)
                {
                    await using var coinCmd = new MySqlCommand(
                        "UPDATE characters SET Gold = Gold + @Gold WHERE ChrName = @CharName", conn);
                    coinCmd.Parameters.AddWithValue("@Gold", gold);
                    coinCmd.Parameters.AddWithValue("@CharName", charName);
                    await coinCmd.ExecuteNonQueryAsync();
                }

                Logger.Info($"领取邮件附件: CharName={charName}, MailId={playerMailId}, Gold={gold}, GameGold={gameGold}");

                return new ClaimResult
                {
                    Success = true,
                    Gold = gold,
                    GameGold = gameGold,
                    Items = attachments != null ? JsonSerializer.Deserialize<List<MailAttachment>>(attachments) : null
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"领取邮件附件失败: ID={playerMailId}");
                return new ClaimResult { Success = false, Message = "领取失败" };
            }
        }

        /// <summary>
        /// 删除玩家邮件
        /// </summary>
        public async Task<bool> DeletePlayerMailAsync(long playerMailId, string charName)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new MySqlCommand(@"
                    UPDATE player_mails SET IsDeleted = 1 
                    WHERE Id = @Id AND CharName = @CharName", conn);
                cmd.Parameters.AddWithValue("@Id", playerMailId);
                cmd.Parameters.AddWithValue("@CharName", charName);
                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"删除玩家邮件失败: ID={playerMailId}");
                return false;
            }
        }

        /// <summary>
        /// 获取未读邮件数
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string charName)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM player_mails 
                    WHERE CharName = @CharName AND IsRead = 0 AND IsDeleted = 0 AND ExpireTime > NOW()", conn);
                cmd.Parameters.AddWithValue("@CharName", charName);

                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region 模板管理

        public async Task<List<MailTemplate>> GetTemplatesAsync()
        {
            var templates = new List<MailTemplate>();

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new MySqlCommand("SELECT * FROM mail_templates WHERE IsEnabled = 1", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    templates.Add(new MailTemplate
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Title = reader.GetString("Title"),
                        Content = reader.GetString("Content"),
                        Attachments = reader.IsDBNull(reader.GetOrdinal("Attachments")) ? null : reader.GetString("Attachments"),
                        Gold = reader.GetInt32("Gold"),
                        GameGold = reader.GetInt32("GameGold"),
                        MailType = reader.GetString("MailType")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取邮件模板失败");
            }

            return templates;
        }

        #endregion

        #region 辅助方法

        private GameMail ReadMail(MySqlDataReader reader)
        {
            return new GameMail
            {
                Id = reader.GetInt32("Id"),
                SenderId = reader.GetString("SenderId"),
                SenderName = reader.GetString("SenderName"),
                ReceiverType = reader.GetString("ReceiverType"),
                ReceiverName = reader.IsDBNull(reader.GetOrdinal("ReceiverName")) ? null : reader.GetString("ReceiverName"),
                ReceiverCondition = reader.IsDBNull(reader.GetOrdinal("ReceiverCondition")) ? null : reader.GetString("ReceiverCondition"),
                Title = reader.GetString("Title"),
                Content = reader.GetString("Content"),
                Attachments = reader.IsDBNull(reader.GetOrdinal("Attachments")) ? null : reader.GetString("Attachments"),
                Gold = reader.GetInt32("Gold"),
                GameGold = reader.GetInt32("GameGold"),
                ExpireHours = reader.GetInt32("ExpireHours"),
                MailType = reader.GetString("MailType"),
                Priority = reader.GetInt32("Priority"),
                Status = reader.GetString("Status"),
                SendTime = reader.IsDBNull(reader.GetOrdinal("SendTime")) ? null : reader.GetDateTime("SendTime"),
                CreateTime = reader.GetDateTime("CreateTime"),
                CreateBy = reader.IsDBNull(reader.GetOrdinal("CreateBy")) ? null : reader.GetString("CreateBy"),
                TotalReceivers = reader.GetInt32("TotalReceivers"),
                ClaimedCount = reader.GetInt32("ClaimedCount")
            };
        }

        private async Task<string?> GetAccountIdByCharNameAsync(MySqlConnection conn, string charName)
        {
            await using var cmd = new MySqlCommand("SELECT LoginID FROM characters WHERE ChrName = @Name LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@Name", charName);
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        private async Task<List<(string AccountId, string CharName)>> GetAllPlayersAsync(MySqlConnection conn)
        {
            var players = new List<(string, string)>();
            await using var cmd = new MySqlCommand("SELECT LoginID, ChrName FROM characters WHERE Deleted = 0", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                players.Add((reader.GetString("LoginID"), reader.GetString("ChrName")));
            }
            return players;
        }

        private async Task<List<(string AccountId, string CharName)>> GetPlayersByLevelAsync(MySqlConnection conn, int minLevel, int maxLevel)
        {
            var players = new List<(string, string)>();
            await using var cmd = new MySqlCommand(
                "SELECT LoginID, ChrName FROM characters WHERE Deleted = 0 AND Level >= @Min AND Level <= @Max", conn);
            cmd.Parameters.AddWithValue("@Min", minLevel);
            cmd.Parameters.AddWithValue("@Max", maxLevel);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                players.Add((reader.GetString("LoginID"), reader.GetString("ChrName")));
            }
            return players;
        }

        private async Task<List<(string AccountId, string CharName)>> GetPlayersByVipAsync(MySqlConnection conn, int minVip)
        {
            var players = new List<(string, string)>();
            await using var cmd = new MySqlCommand(@"
                SELECT c.LoginID, c.ChrName FROM characters c
                JOIN user_vip v ON c.LoginID = v.AccountId
                WHERE c.Deleted = 0 AND v.VipLevel >= @MinVip", conn);
            cmd.Parameters.AddWithValue("@MinVip", minVip);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                players.Add((reader.GetString("LoginID"), reader.GetString("ChrName")));
            }
            return players;
        }

        #endregion
    }

    #region 模型定义

    public interface IMailService
    {
        Task<(List<GameMail> Items, int Total)> GetMailsAsync(string? search, string? mailType, string? status, int page, int pageSize);
        Task<int> CreateMailAsync(CreateMailRequest request, string createBy);
        Task<bool> SendMailAsync(int mailId);
        Task<bool> DeleteMailAsync(int mailId);
        Task<List<PlayerMailInfo>> GetPlayerMailsAsync(string charName, bool includeRead = true);
        Task<bool> ReadMailAsync(long playerMailId, string charName);
        Task<ClaimResult> ClaimMailAsync(long playerMailId, string charName);
        Task<bool> DeletePlayerMailAsync(long playerMailId, string charName);
        Task<int> GetUnreadCountAsync(string charName);
        Task<List<MailTemplate>> GetTemplatesAsync();
    }

    public class GameMail
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverType { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverCondition { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? Attachments { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public int ExpireHours { get; set; }
        public string MailType { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; }
        public DateTime? SendTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string? CreateBy { get; set; }
        public int TotalReceivers { get; set; }
        public int ClaimedCount { get; set; }
    }

    public class CreateMailRequest
    {
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string ReceiverType { get; set; } = "single";
        public string? ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverCondition { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<MailAttachment>? Attachments { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public int ExpireHours { get; set; } = 168;
        public string MailType { get; set; } = "system";
        public int Priority { get; set; }
        public bool SendNow { get; set; } = true;
    }

    public class MailAttachment
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Count { get; set; }
    }

    public class PlayerMailInfo
    {
        public long Id { get; set; }
        public int MailId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SenderName { get; set; }
        public string? Attachments { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public string MailType { get; set; }
        public int Priority { get; set; }
        public bool IsRead { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime ExpireTime { get; set; }
        public DateTime CreateTime { get; set; }
        public bool HasAttachment => Gold > 0 || GameGold > 0 || !string.IsNullOrEmpty(Attachments);
    }

    public class ClaimResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public List<MailAttachment>? Items { get; set; }
    }

    public class MailTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? Attachments { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public string MailType { get; set; }
    }

    public class LevelCondition
    {
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
    }

    public class VipCondition
    {
        public int MinVip { get; set; }
    }

    #endregion
}
