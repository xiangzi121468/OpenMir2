using MySqlConnector;
using NLog;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CastleModule
{
    /// <summary>
    /// 沙城争夺/攻城战核心服务
    /// </summary>
    public class CastleWarService : ICastleWarService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;
        
        // 运行时状态
        private readonly ConcurrentDictionary<int, CastleWarState> _activeWars = new();
        private readonly ConcurrentDictionary<int, CastleInfo> _castleCache = new();
        private CastleWarConfig _config;
        private bool _initialized;

        public CastleWarService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 初始化

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                await LoadConfigAsync();
                await LoadCastlesAsync();
                _initialized = true;
                Logger.Info("攻城战系统初始化完成");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "攻城战系统初始化失败");
            }
        }

        private async Task LoadConfigAsync()
        {
            _config = new CastleWarConfig();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT ConfigKey, ConfigValue FROM castle_configs", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var key = reader.GetString("ConfigKey");
                var value = reader.GetString("ConfigValue");

                switch (key)
                {
                    case "WAR_DAY": _config.WarDay = int.Parse(value); break;
                    case "WAR_START_HOUR": _config.WarStartHour = int.Parse(value); break;
                    case "WAR_START_MINUTE": _config.WarStartMinute = int.Parse(value); break;
                    case "WAR_DURATION_MINUTES": _config.WarDurationMinutes = int.Parse(value); break;
                    case "APPLY_FEE": _config.ApplyFee = long.Parse(value); break;
                    case "APPLY_DEADLINE_HOURS": _config.ApplyDeadlineHours = int.Parse(value); break;
                    case "MIN_GUILD_LEVEL": _config.MinGuildLevel = int.Parse(value); break;
                    case "MIN_GUILD_MEMBERS": _config.MinGuildMembers = int.Parse(value); break;
                    case "RESPAWN_DELAY_SECONDS": _config.RespawnDelaySeconds = int.Parse(value); break;
                    case "MAX_TAX_RATE": _config.MaxTaxRate = int.Parse(value); break;
                }
            }
        }

        private async Task LoadCastlesAsync()
        {
            _castleCache.Clear();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT * FROM castles", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var castle = ReadCastle(reader);
                _castleCache[castle.Id] = castle;
            }

            Logger.Info($"已加载 {_castleCache.Count} 个城堡配置");
        }

        #endregion

        #region 城堡信息

        public async Task<List<CastleInfo>> GetAllCastlesAsync()
        {
            return _castleCache.Values.ToList();
        }

        public async Task<CastleInfo> GetCastleAsync(int castleId)
        {
            if (_castleCache.TryGetValue(castleId, out var castle))
                return castle;

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT * FROM castles WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", castleId);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                castle = ReadCastle(reader);
                _castleCache[castle.Id] = castle;
                return castle;
            }

            return null;
        }

        public async Task<CastleInfo> GetCastleByNameAsync(string castleName)
        {
            var castle = _castleCache.Values.FirstOrDefault(c => c.CastleName == castleName);
            if (castle != null) return castle;

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT * FROM castles WHERE CastleName = @Name", conn);
            cmd.Parameters.AddWithValue("@Name", castleName);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                castle = ReadCastle(reader);
                _castleCache[castle.Id] = castle;
                return castle;
            }

            return null;
        }

        #endregion

        #region 攻城战流程

        /// <summary>
        /// 开始攻城战
        /// </summary>
        public async Task<bool> StartWarAsync(int castleId)
        {
            var castle = await GetCastleAsync(castleId);
            if (castle == null)
            {
                Logger.Warn($"城堡不存在: {castleId}");
                return false;
            }

            if (_activeWars.ContainsKey(castleId))
            {
                Logger.Warn($"攻城战已在进行中: {castle.CastleName}");
                return false;
            }

            try
            {
                // 获取报名的行会
                var applicants = await GetWarApplicantsAsync(castleId, DateTime.Today);

                // 创建战争状态
                var warState = new CastleWarState
                {
                    CastleId = castleId,
                    CastleName = castle.CastleName,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(_config.WarDurationMinutes),
                    DefenderGuildId = castle.OwnerGuildId,
                    DefenderGuildName = castle.OwnerGuildName,
                    AttackingGuilds = applicants.Select(a => a.GuildId).ToList(),
                    GateHP = castle.GateHP,
                    GateMaxHP = castle.GateMaxHP,
                    LeftTowerHP = castle.LeftTowerHP,
                    RightTowerHP = castle.RightTowerHP,
                    IsActive = true
                };

                _activeWars[castleId] = warState;

                // 更新数据库
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new MySqlCommand(@"
                    UPDATE castles SET IsUnderAttack = 1, WarStartTime = @StartTime, WarEndTime = @EndTime 
                    WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", castleId);
                cmd.Parameters.AddWithValue("@StartTime", warState.StartTime);
                cmd.Parameters.AddWithValue("@EndTime", warState.EndTime);
                await cmd.ExecuteNonQueryAsync();

                // 创建战争记录
                await using var recordCmd = new MySqlCommand(@"
                    INSERT INTO castle_war_records (CastleId, CastleName, WarDate, StartTime, DefenderGuildId, DefenderGuildName, AttackerCount)
                    VALUES (@CastleId, @CastleName, @WarDate, @StartTime, @DefenderId, @DefenderName, @AttackerCount);
                    SELECT LAST_INSERT_ID();", conn);
                recordCmd.Parameters.AddWithValue("@CastleId", castleId);
                recordCmd.Parameters.AddWithValue("@CastleName", castle.CastleName);
                recordCmd.Parameters.AddWithValue("@WarDate", DateTime.Today);
                recordCmd.Parameters.AddWithValue("@StartTime", warState.StartTime);
                recordCmd.Parameters.AddWithValue("@DefenderId", castle.OwnerGuildId ?? (object)DBNull.Value);
                recordCmd.Parameters.AddWithValue("@DefenderName", castle.OwnerGuildName ?? (object)DBNull.Value);
                recordCmd.Parameters.AddWithValue("@AttackerCount", applicants.Count);
                warState.WarRecordId = Convert.ToInt32(await recordCmd.ExecuteScalarAsync());

                // 更新缓存
                castle.IsUnderAttack = true;
                castle.WarStartTime = warState.StartTime;
                castle.WarEndTime = warState.EndTime;

                Logger.Info($"攻城战开始: {castle.CastleName}, 守城方: {castle.OwnerGuildName ?? "无"}, 攻城行会: {applicants.Count}个");

                // 触发开始事件
                OnWarStarted?.Invoke(this, new CastleWarEventArgs { CastleId = castleId, CastleName = castle.CastleName });

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"开始攻城战失败: {castle.CastleName}");
                return false;
            }
        }

        /// <summary>
        /// 结束攻城战
        /// </summary>
        public async Task<bool> EndWarAsync(int castleId, bool forceEnd = false)
        {
            if (!_activeWars.TryRemove(castleId, out var warState))
            {
                Logger.Warn($"攻城战不存在: {castleId}");
                return false;
            }

            try
            {
                warState.IsActive = false;
                var endTime = DateTime.Now;
                var duration = (int)(endTime - warState.StartTime).TotalSeconds;

                // 确定胜利者
                string result;
                int? winnerGuildId;
                string winnerGuildName;
                string winnerName = warState.LastOccupier;

                if (warState.GateHP <= 0 && warState.CurrentOccupierId.HasValue)
                {
                    // 攻城方胜利
                    result = "attacker_win";
                    winnerGuildId = warState.CurrentOccupierId;
                    winnerGuildName = warState.CurrentOccupierName;
                }
                else
                {
                    // 守城方胜利
                    result = "defender_win";
                    winnerGuildId = warState.DefenderGuildId;
                    winnerGuildName = warState.DefenderGuildName;
                }

                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 更新城堡信息
                if (result == "attacker_win" && winnerGuildId.HasValue)
                {
                    await using var updateCmd = new MySqlCommand(@"
                        UPDATE castles SET 
                            IsUnderAttack = 0, 
                            OwnerGuildId = @GuildId, 
                            OwnerGuildName = @GuildName,
                            OwnerName = @OwnerName,
                            OccupyTime = @OccupyTime,
                            LastWarTime = @LastWarTime,
                            GateHP = @GateHP
                        WHERE Id = @Id", conn);
                    updateCmd.Parameters.AddWithValue("@Id", castleId);
                    updateCmd.Parameters.AddWithValue("@GuildId", winnerGuildId);
                    updateCmd.Parameters.AddWithValue("@GuildName", winnerGuildName);
                    updateCmd.Parameters.AddWithValue("@OwnerName", winnerName ?? (object)DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@OccupyTime", endTime);
                    updateCmd.Parameters.AddWithValue("@LastWarTime", endTime);
                    updateCmd.Parameters.AddWithValue("@GateHP", warState.GateMaxHP); // 重置城门
                    await updateCmd.ExecuteNonQueryAsync();

                    // 更新缓存
                    if (_castleCache.TryGetValue(castleId, out var castle))
                    {
                        castle.OwnerGuildId = winnerGuildId;
                        castle.OwnerGuildName = winnerGuildName;
                        castle.OwnerName = winnerName;
                        castle.OccupyTime = endTime;
                        castle.IsUnderAttack = false;
                        castle.GateHP = castle.GateMaxHP;
                    }
                }
                else
                {
                    await using var updateCmd = new MySqlCommand(@"
                        UPDATE castles SET IsUnderAttack = 0, LastWarTime = @LastWarTime, GateHP = @GateHP
                        WHERE Id = @Id", conn);
                    updateCmd.Parameters.AddWithValue("@Id", castleId);
                    updateCmd.Parameters.AddWithValue("@LastWarTime", endTime);
                    updateCmd.Parameters.AddWithValue("@GateHP", warState.GateMaxHP);
                    await updateCmd.ExecuteNonQueryAsync();

                    if (_castleCache.TryGetValue(castleId, out var castle))
                    {
                        castle.IsUnderAttack = false;
                        castle.GateHP = castle.GateMaxHP;
                    }
                }

                // 更新战争记录
                await using var recordCmd = new MySqlCommand(@"
                    UPDATE castle_war_records SET 
                        EndTime = @EndTime, 
                        Duration = @Duration,
                        WinnerGuildId = @WinnerId,
                        WinnerGuildName = @WinnerName,
                        WinnerName = @WinnerChar,
                        TotalKills = @TotalKills,
                        GateDestroyed = @GateDestroyed,
                        Result = @Result
                    WHERE Id = @Id", conn);
                recordCmd.Parameters.AddWithValue("@Id", warState.WarRecordId);
                recordCmd.Parameters.AddWithValue("@EndTime", endTime);
                recordCmd.Parameters.AddWithValue("@Duration", duration);
                recordCmd.Parameters.AddWithValue("@WinnerId", winnerGuildId ?? (object)DBNull.Value);
                recordCmd.Parameters.AddWithValue("@WinnerName", winnerGuildName ?? (object)DBNull.Value);
                recordCmd.Parameters.AddWithValue("@WinnerChar", winnerName ?? (object)DBNull.Value);
                recordCmd.Parameters.AddWithValue("@TotalKills", warState.TotalKills);
                recordCmd.Parameters.AddWithValue("@GateDestroyed", warState.GateHP <= 0 ? 1 : 0);
                recordCmd.Parameters.AddWithValue("@Result", result);
                await recordCmd.ExecuteNonQueryAsync();

                Logger.Info($"攻城战结束: {warState.CastleName}, 结果: {result}, 胜利方: {winnerGuildName}");

                // 触发结束事件
                OnWarEnded?.Invoke(this, new CastleWarEventArgs
                {
                    CastleId = castleId,
                    CastleName = warState.CastleName,
                    WinnerGuildId = winnerGuildId,
                    WinnerGuildName = winnerGuildName,
                    Result = result
                });

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"结束攻城战失败: {castleId}");
                return false;
            }
        }

        /// <summary>
        /// 攻击城门
        /// </summary>
        public async Task<int> AttackGateAsync(int castleId, string attackerName, int attackerGuildId, int damage)
        {
            if (!_activeWars.TryGetValue(castleId, out var warState))
                return -1;

            if (!warState.IsActive)
                return -1;

            // 守城方不能攻击城门
            if (attackerGuildId == warState.DefenderGuildId)
                return warState.GateHP;

            lock (warState)
            {
                warState.GateHP = Math.Max(0, warState.GateHP - damage);
                warState.GateDamageDealt[attackerName] = warState.GateDamageDealt.GetValueOrDefault(attackerName) + damage;
            }

            // 城门被破坏
            if (warState.GateHP <= 0)
            {
                Logger.Info($"城门被破坏: {warState.CastleName}, 攻击者: {attackerName}");
                OnGateDestroyed?.Invoke(this, new CastleWarEventArgs { CastleId = castleId, CastleName = warState.CastleName });
            }

            return warState.GateHP;
        }

        /// <summary>
        /// 占领皇宫（夺旗）
        /// </summary>
        public async Task<bool> OccupyPalaceAsync(int castleId, string playerName, int guildId, string guildName)
        {
            if (!_activeWars.TryGetValue(castleId, out var warState))
                return false;

            if (!warState.IsActive)
                return false;

            // 城门未破不能占领
            if (warState.GateHP > 0)
                return false;

            // 守城方不能重复占领
            if (guildId == warState.DefenderGuildId && warState.CurrentOccupierId == guildId)
                return false;

            lock (warState)
            {
                warState.CurrentOccupierId = guildId;
                warState.CurrentOccupierName = guildName;
                warState.LastOccupier = playerName;
                warState.OccupyTime = DateTime.Now;
            }

            Logger.Info($"皇宫被占领: {warState.CastleName}, 占领者: {playerName}({guildName})");

            OnPalaceOccupied?.Invoke(this, new CastleWarEventArgs
            {
                CastleId = castleId,
                CastleName = warState.CastleName,
                WinnerGuildId = guildId,
                WinnerGuildName = guildName,
                PlayerName = playerName
            });

            return true;
        }

        /// <summary>
        /// 记录击杀
        /// </summary>
        public async Task RecordKillAsync(int castleId, string killerName, int? killerGuildId, string killerGuildName,
            string victimName, int? victimGuildId, string victimGuildName, string mapName, int posX, int posY)
        {
            if (!_activeWars.TryGetValue(castleId, out var warState))
                return;

            lock (warState)
            {
                warState.TotalKills++;
                warState.PlayerKills[killerName] = warState.PlayerKills.GetValueOrDefault(killerName) + 1;
                warState.PlayerDeaths[victimName] = warState.PlayerDeaths.GetValueOrDefault(victimName) + 1;
            }

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                await using var cmd = new MySqlCommand(@"
                    INSERT INTO castle_war_kills 
                    (WarRecordId, CastleId, KillerAccount, KillerName, KillerGuildId, KillerGuildName, 
                     VictimAccount, VictimName, VictimGuildId, VictimGuildName, MapName, PosX, PosY)
                    VALUES (@WarId, @CastleId, @KillerAccount, @KillerName, @KillerGuildId, @KillerGuildName,
                            @VictimAccount, @VictimName, @VictimGuildId, @VictimGuildName, @MapName, @PosX, @PosY)", conn);
                cmd.Parameters.AddWithValue("@WarId", warState.WarRecordId);
                cmd.Parameters.AddWithValue("@CastleId", castleId);
                cmd.Parameters.AddWithValue("@KillerAccount", killerName); // TODO: 获取账号
                cmd.Parameters.AddWithValue("@KillerName", killerName);
                cmd.Parameters.AddWithValue("@KillerGuildId", killerGuildId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@KillerGuildName", killerGuildName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@VictimAccount", victimName);
                cmd.Parameters.AddWithValue("@VictimName", victimName);
                cmd.Parameters.AddWithValue("@VictimGuildId", victimGuildId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@VictimGuildName", victimGuildName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MapName", mapName);
                cmd.Parameters.AddWithValue("@PosX", posX);
                cmd.Parameters.AddWithValue("@PosY", posY);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "记录攻城战击杀失败");
            }
        }

        #endregion

        #region 报名

        /// <summary>
        /// 行会报名攻城
        /// </summary>
        public async Task<(bool success, string message)> ApplyForWarAsync(int castleId, int guildId, string guildName, string guildMaster, int memberCount, long playerGold)
        {
            var castle = await GetCastleAsync(castleId);
            if (castle == null)
                return (false, "城堡不存在");

            // 检查是否为城主行会
            if (castle.OwnerGuildId == guildId)
                return (false, "您的行会已是城主，无需报名");

            // 检查行会等级和人数
            if (memberCount < _config.MinGuildMembers)
                return (false, $"行会人数不足{_config.MinGuildMembers}人");

            // 检查报名费
            if (playerGold < _config.ApplyFee)
                return (false, $"报名费不足，需要{_config.ApplyFee}金币");

            // 检查报名截止时间
            var nextWarDate = GetNextWarDate();
            var deadline = nextWarDate.AddHours(-_config.ApplyDeadlineHours);
            if (DateTime.Now > deadline)
                return (false, "报名已截止");

            // 检查是否已报名
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var checkCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM castle_war_applications WHERE CastleId = @CastleId AND WarDate = @WarDate AND GuildId = @GuildId",
                conn);
            checkCmd.Parameters.AddWithValue("@CastleId", castleId);
            checkCmd.Parameters.AddWithValue("@WarDate", nextWarDate.Date);
            checkCmd.Parameters.AddWithValue("@GuildId", guildId);
            var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
            if (count > 0)
                return (false, "您的行会已报名");

            // 插入报名记录
            await using var cmd = new MySqlCommand(@"
                INSERT INTO castle_war_applications (CastleId, WarDate, GuildId, GuildName, GuildMaster, ApplyFee, MemberCount)
                VALUES (@CastleId, @WarDate, @GuildId, @GuildName, @GuildMaster, @ApplyFee, @MemberCount)", conn);
            cmd.Parameters.AddWithValue("@CastleId", castleId);
            cmd.Parameters.AddWithValue("@WarDate", nextWarDate.Date);
            cmd.Parameters.AddWithValue("@GuildId", guildId);
            cmd.Parameters.AddWithValue("@GuildName", guildName);
            cmd.Parameters.AddWithValue("@GuildMaster", guildMaster);
            cmd.Parameters.AddWithValue("@ApplyFee", _config.ApplyFee);
            cmd.Parameters.AddWithValue("@MemberCount", memberCount);
            await cmd.ExecuteNonQueryAsync();

            Logger.Info($"行会报名攻城: {guildName} -> {castle.CastleName}, 下次攻城: {nextWarDate:yyyy-MM-dd}");

            return (true, $"报名成功！攻城时间: {nextWarDate:yyyy-MM-dd HH:mm}");
        }

        /// <summary>
        /// 获取报名的行会列表
        /// </summary>
        public async Task<List<WarApplicant>> GetWarApplicantsAsync(int castleId, DateTime warDate)
        {
            var list = new List<WarApplicant>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(@"
                SELECT * FROM castle_war_applications 
                WHERE CastleId = @CastleId AND WarDate = @WarDate AND Status = 'approved'
                ORDER BY ApplyTime", conn);
            cmd.Parameters.AddWithValue("@CastleId", castleId);
            cmd.Parameters.AddWithValue("@WarDate", warDate.Date);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new WarApplicant
                {
                    Id = reader.GetInt32("Id"),
                    GuildId = reader.GetInt32("GuildId"),
                    GuildName = reader.GetString("GuildName"),
                    GuildMaster = reader.GetString("GuildMaster"),
                    ApplyTime = reader.GetDateTime("ApplyTime"),
                    MemberCount = reader.GetInt32("MemberCount")
                });
            }

            return list;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取攻城战状态
        /// </summary>
        public CastleWarState GetWarState(int castleId)
        {
            _activeWars.TryGetValue(castleId, out var state);
            return state;
        }

        /// <summary>
        /// 是否正在攻城
        /// </summary>
        public bool IsWarActive(int castleId)
        {
            return _activeWars.ContainsKey(castleId);
        }

        /// <summary>
        /// 获取下次攻城时间
        /// </summary>
        public DateTime GetNextWarDate()
        {
            var now = DateTime.Now;
            var daysUntilWar = (_config.WarDay - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilWar == 0 && now.Hour >= _config.WarStartHour)
                daysUntilWar = 7;

            var warDate = now.Date.AddDays(daysUntilWar)
                .AddHours(_config.WarStartHour)
                .AddMinutes(_config.WarStartMinute);

            return warDate;
        }

        /// <summary>
        /// 获取战争历史记录
        /// </summary>
        public async Task<List<WarRecord>> GetWarHistoryAsync(int castleId, int limit = 10)
        {
            var list = new List<WarRecord>();

            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand($@"
                SELECT * FROM castle_war_records 
                WHERE CastleId = @CastleId 
                ORDER BY StartTime DESC 
                LIMIT {limit}", conn);
            cmd.Parameters.AddWithValue("@CastleId", castleId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new WarRecord
                {
                    Id = reader.GetInt32("Id"),
                    CastleId = reader.GetInt32("CastleId"),
                    CastleName = reader.GetString("CastleName"),
                    WarDate = reader.GetDateTime("WarDate"),
                    StartTime = reader.GetDateTime("StartTime"),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime("EndTime"),
                    Duration = reader.GetInt32("Duration"),
                    DefenderGuildName = reader.IsDBNull(reader.GetOrdinal("DefenderGuildName")) ? null : reader.GetString("DefenderGuildName"),
                    WinnerGuildName = reader.IsDBNull(reader.GetOrdinal("WinnerGuildName")) ? null : reader.GetString("WinnerGuildName"),
                    WinnerName = reader.IsDBNull(reader.GetOrdinal("WinnerName")) ? null : reader.GetString("WinnerName"),
                    TotalKills = reader.GetInt32("TotalKills"),
                    Result = reader.IsDBNull(reader.GetOrdinal("Result")) ? null : reader.GetString("Result")
                });
            }

            return list;
        }

        #endregion

        #region 事件

        public event EventHandler<CastleWarEventArgs> OnWarStarted;
        public event EventHandler<CastleWarEventArgs> OnWarEnded;
        public event EventHandler<CastleWarEventArgs> OnGateDestroyed;
        public event EventHandler<CastleWarEventArgs> OnPalaceOccupied;

        #endregion

        #region 辅助方法

        private CastleInfo ReadCastle(MySqlDataReader reader)
        {
            return new CastleInfo
            {
                Id = reader.GetInt32("Id"),
                CastleName = reader.GetString("CastleName"),
                MapName = reader.GetString("MapName"),
                PalaceMapName = reader.IsDBNull(reader.GetOrdinal("PalaceMapName")) ? null : reader.GetString("PalaceMapName"),
                CenterX = reader.GetInt32("CenterX"),
                CenterY = reader.GetInt32("CenterY"),
                OwnerGuildId = reader.IsDBNull(reader.GetOrdinal("OwnerGuildId")) ? null : reader.GetInt32("OwnerGuildId"),
                OwnerGuildName = reader.IsDBNull(reader.GetOrdinal("OwnerGuildName")) ? null : reader.GetString("OwnerGuildName"),
                OwnerName = reader.IsDBNull(reader.GetOrdinal("OwnerName")) ? null : reader.GetString("OwnerName"),
                OccupyTime = reader.IsDBNull(reader.GetOrdinal("OccupyTime")) ? null : reader.GetDateTime("OccupyTime"),
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
                WarStartTime = reader.IsDBNull(reader.GetOrdinal("WarStartTime")) ? null : reader.GetDateTime("WarStartTime"),
                WarEndTime = reader.IsDBNull(reader.GetOrdinal("WarEndTime")) ? null : reader.GetDateTime("WarEndTime")
            };
        }

        #endregion
    }

    #region 接口

    public interface ICastleWarService
    {
        Task InitializeAsync();
        Task<List<CastleInfo>> GetAllCastlesAsync();
        Task<CastleInfo> GetCastleAsync(int castleId);
        Task<CastleInfo> GetCastleByNameAsync(string castleName);
        Task<bool> StartWarAsync(int castleId);
        Task<bool> EndWarAsync(int castleId, bool forceEnd = false);
        Task<int> AttackGateAsync(int castleId, string attackerName, int attackerGuildId, int damage);
        Task<bool> OccupyPalaceAsync(int castleId, string playerName, int guildId, string guildName);
        Task RecordKillAsync(int castleId, string killerName, int? killerGuildId, string killerGuildName, string victimName, int? victimGuildId, string victimGuildName, string mapName, int posX, int posY);
        Task<(bool success, string message)> ApplyForWarAsync(int castleId, int guildId, string guildName, string guildMaster, int memberCount, long playerGold);
        Task<List<WarApplicant>> GetWarApplicantsAsync(int castleId, DateTime warDate);
        CastleWarState GetWarState(int castleId);
        bool IsWarActive(int castleId);
        DateTime GetNextWarDate();
        Task<List<WarRecord>> GetWarHistoryAsync(int castleId, int limit = 10);

        event EventHandler<CastleWarEventArgs> OnWarStarted;
        event EventHandler<CastleWarEventArgs> OnWarEnded;
        event EventHandler<CastleWarEventArgs> OnGateDestroyed;
        event EventHandler<CastleWarEventArgs> OnPalaceOccupied;
    }

    #endregion

    #region 模型

    public class CastleInfo
    {
        public int Id { get; set; }
        public string CastleName { get; set; }
        public string MapName { get; set; }
        public string PalaceMapName { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int? OwnerGuildId { get; set; }
        public string OwnerGuildName { get; set; }
        public string OwnerName { get; set; }
        public DateTime? OccupyTime { get; set; }
        public int TaxRate { get; set; }
        public long TotalTax { get; set; }
        public int DefenseLevel { get; set; }
        public int GateHP { get; set; }
        public int GateMaxHP { get; set; }
        public int LeftTowerHP { get; set; }
        public int RightTowerHP { get; set; }
        public int TowerMaxHP { get; set; }
        public int GuardCount { get; set; }
        public bool IsUnderAttack { get; set; }
        public DateTime? WarStartTime { get; set; }
        public DateTime? WarEndTime { get; set; }
    }

    public class CastleWarState
    {
        public int CastleId { get; set; }
        public string CastleName { get; set; }
        public int WarRecordId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }

        public int? DefenderGuildId { get; set; }
        public string DefenderGuildName { get; set; }
        public List<int> AttackingGuilds { get; set; } = new();

        public int? CurrentOccupierId { get; set; }
        public string CurrentOccupierName { get; set; }
        public string LastOccupier { get; set; }
        public DateTime? OccupyTime { get; set; }

        public int GateHP { get; set; }
        public int GateMaxHP { get; set; }
        public int LeftTowerHP { get; set; }
        public int RightTowerHP { get; set; }

        public int TotalKills { get; set; }
        public Dictionary<string, int> PlayerKills { get; set; } = new();
        public Dictionary<string, int> PlayerDeaths { get; set; } = new();
        public Dictionary<string, long> GateDamageDealt { get; set; } = new();

        public int RemainingSeconds => (int)(EndTime - DateTime.Now).TotalSeconds;
    }

    public class CastleWarConfig
    {
        public int WarDay { get; set; } = 6; // 周六
        public int WarStartHour { get; set; } = 20;
        public int WarStartMinute { get; set; } = 0;
        public int WarDurationMinutes { get; set; } = 120;
        public long ApplyFee { get; set; } = 1000000;
        public int ApplyDeadlineHours { get; set; } = 24;
        public int MinGuildLevel { get; set; } = 3;
        public int MinGuildMembers { get; set; } = 10;
        public int RespawnDelaySeconds { get; set; } = 30;
        public int MaxTaxRate { get; set; } = 50;
    }

    public class WarApplicant
    {
        public int Id { get; set; }
        public int GuildId { get; set; }
        public string GuildName { get; set; }
        public string GuildMaster { get; set; }
        public DateTime ApplyTime { get; set; }
        public int MemberCount { get; set; }
    }

    public class WarRecord
    {
        public int Id { get; set; }
        public int CastleId { get; set; }
        public string CastleName { get; set; }
        public DateTime WarDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Duration { get; set; }
        public string DefenderGuildName { get; set; }
        public string WinnerGuildName { get; set; }
        public string WinnerName { get; set; }
        public int TotalKills { get; set; }
        public string Result { get; set; }
    }

    public class CastleWarEventArgs : EventArgs
    {
        public int CastleId { get; set; }
        public string CastleName { get; set; }
        public int? WinnerGuildId { get; set; }
        public string WinnerGuildName { get; set; }
        public string PlayerName { get; set; }
        public string Result { get; set; }
    }

    #endregion
}
