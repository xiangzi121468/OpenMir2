using MySqlConnector;
using NLog;
using System.Collections.Concurrent;
using System.Text.Json;

namespace M2Server.Services
{
    /// <summary>
    /// 玩家行为日志服务
    /// 异步批量写入，不影响游戏性能
    /// </summary>
    public class PlayerLogService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static PlayerLogService? _instance;
        public static PlayerLogService Instance => _instance ??= new PlayerLogService();

        private readonly string _connectionString;
        private readonly ConcurrentQueue<LogEntry> _logQueue;
        private readonly Timer _flushTimer;
        private readonly int _batchSize = 100;
        private readonly int _flushIntervalMs = 5000;
        private bool _isDisposed;

        private PlayerLogService()
        {
            _connectionString = SystemShare.Config.DbConnectionString;
            _logQueue = new ConcurrentQueue<LogEntry>();
            _flushTimer = new Timer(FlushLogs, null, _flushIntervalMs, _flushIntervalMs);
        }

        #region 登录日志

        public void LogLogin(string accountId, string charName, string clientIp, string deviceId, int level, string mapName, int posX, int posY)
        {
            Enqueue(new LoginLogEntry
            {
                AccountId = accountId,
                CharName = charName,
                LoginType = "login",
                ClientIp = clientIp,
                DeviceId = deviceId,
                Level = level,
                MapName = mapName,
                PosX = posX,
                PosY = posY
            });
        }

        public void LogLogout(string accountId, string charName, int onlineSeconds, int level, string mapName, int posX, int posY)
        {
            Enqueue(new LoginLogEntry
            {
                AccountId = accountId,
                CharName = charName,
                LoginType = "logout",
                OnlineSeconds = onlineSeconds,
                Level = level,
                MapName = mapName,
                PosX = posX,
                PosY = posY
            });
        }

        #endregion

        #region 物品日志

        public void LogItemChange(string accountId, string charName, int itemId, string itemName, int count,
            string actionType, string actionDetail = null, string targetName = null, int gold = 0, int gameGold = 0,
            string mapName = null, int posX = 0, int posY = 0)
        {
            Enqueue(new ItemLogEntry
            {
                AccountId = accountId,
                CharName = charName,
                ItemId = itemId,
                ItemName = itemName,
                ItemCount = count,
                ActionType = actionType,
                ActionDetail = actionDetail,
                TargetName = targetName,
                Gold = gold,
                GameGold = gameGold,
                MapName = mapName,
                PosX = posX,
                PosY = posY
            });
        }

        #endregion

        #region 货币日志

        public void LogCurrencyChange(string accountId, string charName, string currencyType,
            long amount, long beforeAmount, long afterAmount, string actionType, string actionDetail = null, string orderNo = null)
        {
            Enqueue(new CurrencyLogEntry
            {
                AccountId = accountId,
                CharName = charName,
                CurrencyType = currencyType,
                Amount = amount,
                BeforeAmount = beforeAmount,
                AfterAmount = afterAmount,
                ActionType = actionType,
                ActionDetail = actionDetail,
                OrderNo = orderNo
            });
        }

        #endregion

        #region 交易日志

        public void LogTrade(string tradeNo, string tradeType,
            string player1Account, string player1Name, List<TradeItem> player1Items, int player1Gold, int player1GameGold,
            string player2Account, string player2Name, List<TradeItem> player2Items, int player2Gold, int player2GameGold,
            string status = "success")
        {
            Enqueue(new TradeLogEntry
            {
                TradeNo = tradeNo,
                TradeType = tradeType,
                Player1Account = player1Account,
                Player1Name = player1Name,
                Player1Items = player1Items != null ? JsonSerializer.Serialize(player1Items) : null,
                Player1Gold = player1Gold,
                Player1GameGold = player1GameGold,
                Player2Account = player2Account,
                Player2Name = player2Name,
                Player2Items = player2Items != null ? JsonSerializer.Serialize(player2Items) : null,
                Player2Gold = player2Gold,
                Player2GameGold = player2GameGold,
                Status = status
            });
        }

        #endregion

        #region 聊天日志

        public void LogChat(string accountId, string charName, string chatType, string content, string targetName = null, string clientIp = null)
        {
            Enqueue(new ChatLogEntry
            {
                AccountId = accountId,
                CharName = charName,
                ChatType = chatType,
                Content = content,
                TargetName = targetName,
                ClientIp = clientIp
            });
        }

        #endregion

        #region 战斗日志

        public void LogBattle(string accountId, string charName, string battleType, string targetType, string targetName,
            string mapName, int posX, int posY, int exp = 0, List<TradeItem> dropItems = null)
        {
            Enqueue(new BattleLogEntry
            {
                AccountId = accountId,
                CharName = charName,
                BattleType = battleType,
                TargetType = targetType,
                TargetName = targetName,
                MapName = mapName,
                PosX = posX,
                PosY = posY,
                Exp = exp,
                DropItems = dropItems != null ? JsonSerializer.Serialize(dropItems) : null
            });
        }

        #endregion

        #region GM日志

        public void LogGmAction(string gmAccount, string gmName, string command, string @params, string targetName, string result, string clientIp)
        {
            Enqueue(new GmLogEntry
            {
                GmAccount = gmAccount,
                GmName = gmName,
                Command = command,
                Params = @params,
                TargetName = targetName,
                Result = result,
                ClientIp = clientIp
            });
        }

        #endregion

        #region 内部方法

        private void Enqueue(LogEntry entry)
        {
            _logQueue.Enqueue(entry);

            // 队列过大时立即刷新
            if (_logQueue.Count >= _batchSize * 2)
            {
                Task.Run(() => FlushLogs(null));
            }
        }

        private void FlushLogs(object state)
        {
            if (_logQueue.IsEmpty) return;

            var batch = new List<LogEntry>();
            while (batch.Count < _batchSize && _logQueue.TryDequeue(out var entry))
            {
                batch.Add(entry);
            }

            if (batch.Count > 0)
            {
                Task.Run(() => WriteBatchAsync(batch));
            }
        }

        private async Task WriteBatchAsync(List<LogEntry> batch)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                foreach (var entry in batch)
                {
                    try
                    {
                        await WriteEntryAsync(conn, entry);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"写入日志失败: {entry.GetType().Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "批量写入日志失败");
            }
        }

        private async Task WriteEntryAsync(MySqlConnection conn, LogEntry entry)
        {
            switch (entry)
            {
                case LoginLogEntry login:
                    await WriteLoginLogAsync(conn, login);
                    break;
                case ItemLogEntry item:
                    await WriteItemLogAsync(conn, item);
                    break;
                case CurrencyLogEntry currency:
                    await WriteCurrencyLogAsync(conn, currency);
                    break;
                case TradeLogEntry trade:
                    await WriteTradeLogAsync(conn, trade);
                    break;
                case ChatLogEntry chat:
                    await WriteChatLogAsync(conn, chat);
                    break;
                case BattleLogEntry battle:
                    await WriteBattleLogAsync(conn, battle);
                    break;
                case GmLogEntry gm:
                    await WriteGmLogAsync(conn, gm);
                    break;
            }
        }

        private async Task WriteLoginLogAsync(MySqlConnection conn, LoginLogEntry log)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO player_login_logs (AccountId, CharName, LoginType, ClientIp, DeviceId, OnlineSeconds, MapName, PosX, PosY, Level)
                VALUES (@AccountId, @CharName, @LoginType, @ClientIp, @DeviceId, @OnlineSeconds, @MapName, @PosX, @PosY, @Level)", conn);
            cmd.Parameters.AddWithValue("@AccountId", log.AccountId);
            cmd.Parameters.AddWithValue("@CharName", log.CharName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LoginType", log.LoginType);
            cmd.Parameters.AddWithValue("@ClientIp", log.ClientIp ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DeviceId", log.DeviceId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@OnlineSeconds", log.OnlineSeconds);
            cmd.Parameters.AddWithValue("@MapName", log.MapName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PosX", log.PosX);
            cmd.Parameters.AddWithValue("@PosY", log.PosY);
            cmd.Parameters.AddWithValue("@Level", log.Level);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task WriteItemLogAsync(MySqlConnection conn, ItemLogEntry log)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO player_item_logs (AccountId, CharName, ItemId, ItemName, ItemCount, ActionType, ActionDetail, TargetName, Gold, GameGold, MapName, PosX, PosY)
                VALUES (@AccountId, @CharName, @ItemId, @ItemName, @ItemCount, @ActionType, @ActionDetail, @TargetName, @Gold, @GameGold, @MapName, @PosX, @PosY)", conn);
            cmd.Parameters.AddWithValue("@AccountId", log.AccountId);
            cmd.Parameters.AddWithValue("@CharName", log.CharName);
            cmd.Parameters.AddWithValue("@ItemId", log.ItemId);
            cmd.Parameters.AddWithValue("@ItemName", log.ItemName);
            cmd.Parameters.AddWithValue("@ItemCount", log.ItemCount);
            cmd.Parameters.AddWithValue("@ActionType", log.ActionType);
            cmd.Parameters.AddWithValue("@ActionDetail", log.ActionDetail ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TargetName", log.TargetName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Gold", log.Gold);
            cmd.Parameters.AddWithValue("@GameGold", log.GameGold);
            cmd.Parameters.AddWithValue("@MapName", log.MapName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PosX", log.PosX);
            cmd.Parameters.AddWithValue("@PosY", log.PosY);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task WriteCurrencyLogAsync(MySqlConnection conn, CurrencyLogEntry log)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO player_currency_logs (AccountId, CharName, CurrencyType, Amount, BeforeAmount, AfterAmount, ActionType, ActionDetail, OrderNo)
                VALUES (@AccountId, @CharName, @CurrencyType, @Amount, @BeforeAmount, @AfterAmount, @ActionType, @ActionDetail, @OrderNo)", conn);
            cmd.Parameters.AddWithValue("@AccountId", log.AccountId);
            cmd.Parameters.AddWithValue("@CharName", log.CharName);
            cmd.Parameters.AddWithValue("@CurrencyType", log.CurrencyType);
            cmd.Parameters.AddWithValue("@Amount", log.Amount);
            cmd.Parameters.AddWithValue("@BeforeAmount", log.BeforeAmount);
            cmd.Parameters.AddWithValue("@AfterAmount", log.AfterAmount);
            cmd.Parameters.AddWithValue("@ActionType", log.ActionType);
            cmd.Parameters.AddWithValue("@ActionDetail", log.ActionDetail ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@OrderNo", log.OrderNo ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task WriteTradeLogAsync(MySqlConnection conn, TradeLogEntry log)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO player_trade_logs (TradeNo, TradeType, Player1Account, Player1Name, Player1Items, Player1Gold, Player1GameGold, Player2Account, Player2Name, Player2Items, Player2Gold, Player2GameGold, Status)
                VALUES (@TradeNo, @TradeType, @Player1Account, @Player1Name, @Player1Items, @Player1Gold, @Player1GameGold, @Player2Account, @Player2Name, @Player2Items, @Player2Gold, @Player2GameGold, @Status)", conn);
            cmd.Parameters.AddWithValue("@TradeNo", log.TradeNo);
            cmd.Parameters.AddWithValue("@TradeType", log.TradeType);
            cmd.Parameters.AddWithValue("@Player1Account", log.Player1Account);
            cmd.Parameters.AddWithValue("@Player1Name", log.Player1Name);
            cmd.Parameters.AddWithValue("@Player1Items", log.Player1Items ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Player1Gold", log.Player1Gold);
            cmd.Parameters.AddWithValue("@Player1GameGold", log.Player1GameGold);
            cmd.Parameters.AddWithValue("@Player2Account", log.Player2Account ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Player2Name", log.Player2Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Player2Items", log.Player2Items ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Player2Gold", log.Player2Gold);
            cmd.Parameters.AddWithValue("@Player2GameGold", log.Player2GameGold);
            cmd.Parameters.AddWithValue("@Status", log.Status);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task WriteChatLogAsync(MySqlConnection conn, ChatLogEntry log)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO player_chat_logs (AccountId, CharName, ChatType, TargetName, Content, ClientIp)
                VALUES (@AccountId, @CharName, @ChatType, @TargetName, @Content, @ClientIp)", conn);
            cmd.Parameters.AddWithValue("@AccountId", log.AccountId);
            cmd.Parameters.AddWithValue("@CharName", log.CharName);
            cmd.Parameters.AddWithValue("@ChatType", log.ChatType);
            cmd.Parameters.AddWithValue("@TargetName", log.TargetName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Content", log.Content);
            cmd.Parameters.AddWithValue("@ClientIp", log.ClientIp ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task WriteBattleLogAsync(MySqlConnection conn, BattleLogEntry log)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO player_battle_logs (AccountId, CharName, BattleType, TargetType, TargetName, MapName, PosX, PosY, Exp, DropItems)
                VALUES (@AccountId, @CharName, @BattleType, @TargetType, @TargetName, @MapName, @PosX, @PosY, @Exp, @DropItems)", conn);
            cmd.Parameters.AddWithValue("@AccountId", log.AccountId);
            cmd.Parameters.AddWithValue("@CharName", log.CharName);
            cmd.Parameters.AddWithValue("@BattleType", log.BattleType);
            cmd.Parameters.AddWithValue("@TargetType", log.TargetType);
            cmd.Parameters.AddWithValue("@TargetName", log.TargetName);
            cmd.Parameters.AddWithValue("@MapName", log.MapName);
            cmd.Parameters.AddWithValue("@PosX", log.PosX);
            cmd.Parameters.AddWithValue("@PosY", log.PosY);
            cmd.Parameters.AddWithValue("@Exp", log.Exp);
            cmd.Parameters.AddWithValue("@DropItems", log.DropItems ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task WriteGmLogAsync(MySqlConnection conn, GmLogEntry log)
        {
            await using var cmd = new MySqlCommand(@"
                INSERT INTO gm_action_logs (GmAccount, GmName, Command, Params, TargetName, Result, ClientIp)
                VALUES (@GmAccount, @GmName, @Command, @Params, @TargetName, @Result, @ClientIp)", conn);
            cmd.Parameters.AddWithValue("@GmAccount", log.GmAccount);
            cmd.Parameters.AddWithValue("@GmName", log.GmName);
            cmd.Parameters.AddWithValue("@Command", log.Command);
            cmd.Parameters.AddWithValue("@Params", log.Params ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TargetName", log.TargetName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Result", log.Result ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientIp", log.ClientIp ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _flushTimer?.Dispose();

            // 刷新剩余日志
            while (!_logQueue.IsEmpty)
            {
                FlushLogs(null);
            }
        }

        #endregion
    }

    #region 日志实体

    public abstract class LogEntry
    {
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    public class LoginLogEntry : LogEntry
    {
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public string LoginType { get; set; }
        public string ClientIp { get; set; }
        public string DeviceId { get; set; }
        public int OnlineSeconds { get; set; }
        public string MapName { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Level { get; set; }
    }

    public class ItemLogEntry : LogEntry
    {
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemCount { get; set; }
        public string ActionType { get; set; }
        public string ActionDetail { get; set; }
        public string TargetName { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public string MapName { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

    public class CurrencyLogEntry : LogEntry
    {
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public string CurrencyType { get; set; }
        public long Amount { get; set; }
        public long BeforeAmount { get; set; }
        public long AfterAmount { get; set; }
        public string ActionType { get; set; }
        public string ActionDetail { get; set; }
        public string OrderNo { get; set; }
    }

    public class TradeLogEntry : LogEntry
    {
        public string TradeNo { get; set; }
        public string TradeType { get; set; }
        public string Player1Account { get; set; }
        public string Player1Name { get; set; }
        public string Player1Items { get; set; }
        public int Player1Gold { get; set; }
        public int Player1GameGold { get; set; }
        public string Player2Account { get; set; }
        public string Player2Name { get; set; }
        public string Player2Items { get; set; }
        public int Player2Gold { get; set; }
        public int Player2GameGold { get; set; }
        public string Status { get; set; }
    }

    public class ChatLogEntry : LogEntry
    {
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public string ChatType { get; set; }
        public string TargetName { get; set; }
        public string Content { get; set; }
        public string ClientIp { get; set; }
    }

    public class BattleLogEntry : LogEntry
    {
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public string BattleType { get; set; }
        public string TargetType { get; set; }
        public string TargetName { get; set; }
        public string MapName { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Exp { get; set; }
        public string DropItems { get; set; }
    }

    public class GmLogEntry : LogEntry
    {
        public string GmAccount { get; set; }
        public string GmName { get; set; }
        public string Command { get; set; }
        public string Params { get; set; }
        public string TargetName { get; set; }
        public string Result { get; set; }
        public string ClientIp { get; set; }
    }

    public class TradeItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Count { get; set; }
    }

    #endregion
}
