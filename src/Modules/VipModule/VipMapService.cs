using MySqlConnector;
using NLog;
using VipModule.Models;

namespace VipModule
{
    /// <summary>
    /// VIP地图服务
    /// </summary>
    public class VipMapService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        // VIP等级配置
        public static readonly List<VipLevelConfig> VipLevels = new()
        {
            new VipLevelConfig { Level = 0, Name = "普通玩家", MinRecharge = 0, Icon = "👤", Color = "#FFFFFF" },
            new VipLevelConfig { Level = 1, Name = "贵族VIP", MinRecharge = 30, Icon = "⭐", Color = "#FFD700", Benefits = "贵族大厅进入权限" },
            new VipLevelConfig { Level = 2, Name = "黄金VIP", MinRecharge = 100, Icon = "🌟", Color = "#FFA500", Benefits = "黄金殿堂进入权限、每日礼包" },
            new VipLevelConfig { Level = 3, Name = "钻石VIP", MinRecharge = 500, Icon = "💎", Color = "#00BFFF", Benefits = "钻石圣殿进入权限、专属称号、每日超级礼包" },
            new VipLevelConfig { Level = 4, Name = "至尊VIP", MinRecharge = 1000, Icon = "👑", Color = "#FF1493", Benefits = "所有VIP权限、至尊称号、专属BOSS召唤" },
            new VipLevelConfig { Level = 5, Name = "神级VIP", MinRecharge = 5000, Icon = "🔱", Color = "#9400D3", Benefits = "神级特权、免费传送、专属神装" }
        };

        public VipMapService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region VIP地图管理

        public async Task<List<VipMap>> GetAllVipMapsAsync()
        {
            var maps = new List<VipMap>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT * FROM vip_maps ORDER BY min_vip_level, id", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    maps.Add(MapVipMap(reader));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取VIP地图列表失败");
            }
            return maps;
        }

        public async Task<VipMap?> GetVipMapByIdAsync(int id)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT * FROM vip_maps WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var map = MapVipMap(reader);
                    reader.Close();

                    // 加载怪物配置
                    map.Monsters = await GetVipMapMonstersAsync(conn, id);
                    return map;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"获取VIP地图失败: {id}");
            }
            return null;
        }

        public async Task<bool> SaveVipMapAsync(VipMapSaveRequest request)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql;
                if (request.Id.HasValue && request.Id > 0)
                {
                    sql = @"UPDATE vip_maps SET 
                        map_id = @mapId, map_name = @mapName, description = @desc,
                        min_vip_level = @vipLevel, min_recharge = @recharge, entry_fee = @fee,
                        daily_limit = @dailyLimit, level_limit = @levelLimit,
                        is_random_entry = @randomEntry, is_fixed_entry = @fixedEntry,
                        random_x_min = @rxMin, random_x_max = @rxMax, random_y_min = @ryMin, random_y_max = @ryMax,
                        fixed_x = @fx, fixed_y = @fy, is_enabled = @enabled
                        WHERE id = @id";
                }
                else
                {
                    sql = @"INSERT INTO vip_maps 
                        (map_id, map_name, description, min_vip_level, min_recharge, entry_fee,
                         daily_limit, level_limit, is_random_entry, is_fixed_entry,
                         random_x_min, random_x_max, random_y_min, random_y_max, fixed_x, fixed_y, is_enabled)
                        VALUES (@mapId, @mapName, @desc, @vipLevel, @recharge, @fee,
                         @dailyLimit, @levelLimit, @randomEntry, @fixedEntry,
                         @rxMin, @rxMax, @ryMin, @ryMax, @fx, @fy, @enabled)";
                }

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@mapId", request.MapId);
                cmd.Parameters.AddWithValue("@mapName", request.MapName);
                cmd.Parameters.AddWithValue("@desc", request.Description ?? "");
                cmd.Parameters.AddWithValue("@vipLevel", request.MinVipLevel);
                cmd.Parameters.AddWithValue("@recharge", request.MinRecharge);
                cmd.Parameters.AddWithValue("@fee", request.EntryFee);
                cmd.Parameters.AddWithValue("@dailyLimit", request.DailyLimit);
                cmd.Parameters.AddWithValue("@levelLimit", request.LevelLimit);
                cmd.Parameters.AddWithValue("@randomEntry", request.IsRandomEntry);
                cmd.Parameters.AddWithValue("@fixedEntry", request.IsFixedEntry);
                cmd.Parameters.AddWithValue("@rxMin", request.RandomXMin);
                cmd.Parameters.AddWithValue("@rxMax", request.RandomXMax);
                cmd.Parameters.AddWithValue("@ryMin", request.RandomYMin);
                cmd.Parameters.AddWithValue("@ryMax", request.RandomYMax);
                cmd.Parameters.AddWithValue("@fx", request.FixedX);
                cmd.Parameters.AddWithValue("@fy", request.FixedY);
                cmd.Parameters.AddWithValue("@enabled", request.IsEnabled);

                if (request.Id.HasValue && request.Id > 0)
                    cmd.Parameters.AddWithValue("@id", request.Id.Value);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存VIP地图失败");
                return false;
            }
        }

        public async Task<bool> DeleteVipMapAsync(int id)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 删除怪物配置
                var delMonsters = new MySqlCommand("DELETE FROM vip_map_monsters WHERE vip_map_id = @id", conn);
                delMonsters.Parameters.AddWithValue("@id", id);
                await delMonsters.ExecuteNonQueryAsync();

                // 删除地图
                var cmd = new MySqlCommand("DELETE FROM vip_maps WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"删除VIP地图失败: {id}");
                return false;
            }
        }

        #endregion

        #region VIP地图怪物管理

        private async Task<List<VipMapMonster>> GetVipMapMonstersAsync(MySqlConnection conn, int vipMapId)
        {
            var monsters = new List<VipMapMonster>();
            var cmd = new MySqlCommand("SELECT * FROM vip_map_monsters WHERE vip_map_id = @id ORDER BY is_boss DESC, id", conn);
            cmd.Parameters.AddWithValue("@id", vipMapId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                monsters.Add(new VipMapMonster
                {
                    Id = reader.GetInt32("id"),
                    VipMapId = reader.GetInt32("vip_map_id"),
                    MonsterName = reader.GetString("monster_name"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                    SpawnCount = reader.GetInt32("spawn_count"),
                    SpawnInterval = reader.GetInt32("spawn_interval"),
                    SpawnX = reader.GetInt32("spawn_x"),
                    SpawnY = reader.GetInt32("spawn_y"),
                    SpawnRange = reader.GetInt32("spawn_range"),
                    IsBoss = reader.GetBoolean("is_boss"),
                    DropDescription = reader.IsDBNull(reader.GetOrdinal("drop_description")) ? null : reader.GetString("drop_description"),
                    IsEnabled = reader.GetBoolean("is_enabled")
                });
            }
            return monsters;
        }

        public async Task<bool> SaveVipMapMonsterAsync(VipMapMonsterSaveRequest request)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql;
                if (request.Id.HasValue && request.Id > 0)
                {
                    sql = @"UPDATE vip_map_monsters SET 
                        monster_name = @name, description = @desc, spawn_count = @count,
                        spawn_interval = @interval, spawn_x = @x, spawn_y = @y, spawn_range = @range,
                        is_boss = @boss, drop_description = @drop, is_enabled = @enabled
                        WHERE id = @id";
                }
                else
                {
                    sql = @"INSERT INTO vip_map_monsters 
                        (vip_map_id, monster_name, description, spawn_count, spawn_interval, 
                         spawn_x, spawn_y, spawn_range, is_boss, drop_description, is_enabled)
                        VALUES (@mapId, @name, @desc, @count, @interval, @x, @y, @range, @boss, @drop, @enabled)";
                }

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", request.MonsterName);
                cmd.Parameters.AddWithValue("@desc", request.Description ?? "");
                cmd.Parameters.AddWithValue("@count", request.SpawnCount);
                cmd.Parameters.AddWithValue("@interval", request.SpawnInterval);
                cmd.Parameters.AddWithValue("@x", request.SpawnX);
                cmd.Parameters.AddWithValue("@y", request.SpawnY);
                cmd.Parameters.AddWithValue("@range", request.SpawnRange);
                cmd.Parameters.AddWithValue("@boss", request.IsBoss);
                cmd.Parameters.AddWithValue("@drop", request.DropDescription ?? "");
                cmd.Parameters.AddWithValue("@enabled", request.IsEnabled);

                if (request.Id.HasValue && request.Id > 0)
                    cmd.Parameters.AddWithValue("@id", request.Id.Value);
                else
                    cmd.Parameters.AddWithValue("@mapId", request.VipMapId);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存VIP地图怪物失败");
                return false;
            }
        }

        public async Task<bool> DeleteVipMapMonsterAsync(int id)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("DELETE FROM vip_map_monsters WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"删除VIP地图怪物失败: {id}");
                return false;
            }
        }

        #endregion

        #region VIP等级检查

        /// <summary>
        /// 根据累计充值金额获取VIP等级
        /// </summary>
        public int GetVipLevelByRecharge(int totalRecharge)
        {
            for (int i = VipLevels.Count - 1; i >= 0; i--)
            {
                if (totalRecharge >= VipLevels[i].MinRecharge)
                    return VipLevels[i].Level;
            }
            return 0;
        }

        /// <summary>
        /// 检查玩家是否可以进入VIP地图
        /// </summary>
        public async Task<(bool CanEnter, string Message)> CanEnterVipMapAsync(
            string accountId, string charName, int mapId, int playerLevel, int vipLevel, int totalRecharge)
        {
            var map = await GetVipMapByIdAsync(mapId);
            if (map == null)
                return (false, "VIP地图不存在");

            if (!map.IsEnabled)
                return (false, "该VIP地图暂未开放");

            // 等级检查
            if (playerLevel < map.LevelLimit)
                return (false, $"需要{map.LevelLimit}级以上才能进入");

            // VIP等级检查
            if (vipLevel < map.MinVipLevel)
            {
                var requiredLevel = VipLevels.FirstOrDefault(v => v.Level == map.MinVipLevel);
                return (false, $"需要{requiredLevel?.Name ?? $"VIP{map.MinVipLevel}"}才能进入");
            }

            // 充值金额检查
            if (totalRecharge < map.MinRecharge)
                return (false, $"需要累计充值{map.MinRecharge}元才能进入");

            // 每日次数检查
            if (map.DailyLimit > 0)
            {
                var todayCount = await GetTodayEntryCountAsync(accountId, map.MapId);
                if (todayCount >= map.DailyLimit)
                    return (false, $"今日进入次数已用完({todayCount}/{map.DailyLimit})");
            }

            return (true, "可以进入");
        }

        /// <summary>
        /// 获取今日进入次数
        /// </summary>
        private async Task<int> GetTodayEntryCountAsync(string accountId, string mapId)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"SELECT COUNT(*) FROM vip_map_entry_logs 
                    WHERE account_id = @account AND map_id = @map AND DATE(entry_time) = CURDATE()", conn);
                cmd.Parameters.AddWithValue("@account", accountId);
                cmd.Parameters.AddWithValue("@map", mapId);

                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 记录进入日志
        /// </summary>
        public async Task LogEntryAsync(string accountId, string charName, string mapId, string mapName, string entryType, int fee)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"INSERT INTO vip_map_entry_logs 
                    (account_id, char_name, map_id, map_name, entry_type, entry_fee)
                    VALUES (@account, @char, @mapId, @mapName, @type, @fee)", conn);

                cmd.Parameters.AddWithValue("@account", accountId);
                cmd.Parameters.AddWithValue("@char", charName);
                cmd.Parameters.AddWithValue("@mapId", mapId);
                cmd.Parameters.AddWithValue("@mapName", mapName);
                cmd.Parameters.AddWithValue("@type", entryType);
                cmd.Parameters.AddWithValue("@fee", fee);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "记录VIP地图进入日志失败");
            }
        }

        #endregion

        #region 生成NPC脚本

        /// <summary>
        /// 自动生成VIP地图NPC脚本
        /// </summary>
        public async Task<string> GenerateNpcScriptAsync(int mapId)
        {
            var map = await GetVipMapByIdAsync(mapId);
            if (map == null) return "";

            var vipLevelName = VipLevels.FirstOrDefault(v => v.Level == map.MinVipLevel)?.Name ?? $"VIP{map.MinVipLevel}";
            var monstersInfo = string.Join("、", map.Monsters.Where(m => m.IsBoss).Select(m => m.MonsterName));
            var dropsInfo = string.Join("、", map.Monsters.Where(m => m.IsBoss).Select(m => m.DropDescription ?? "高级装备"));

            var script = $@"; ============================================
; {map.MapName} NPC脚本 (自动生成)
; ============================================

[@main]
#IF
#ACT
OPENMERCHANTBIGDLG  550 450 0 1,1,1,1
BIGHINTSHOW  <Title/FCOLOR=249>{map.MapName}</Title>
BIGHINTSHOW
BIGHINTSHOW  <怪物：><FCOLOR=250>{monstersInfo}</FCOLOR>
BIGHINTSHOW  <刷新：>{string.Join("、", map.Monsters.Select(m => $"{m.MonsterName}{m.SpawnInterval}分钟"))}
BIGHINTSHOW  <爆出：><FCOLOR=250>{dropsInfo}</FCOLOR>
BIGHINTSHOW  <条件：><FCOLOR=249>{vipLevelName}(充值{map.MinRecharge}元)及以上进入</FCOLOR>";

            if (map.IsRandomEntry && map.IsFixedEntry)
            {
                script += @"
BIGHINTSHOW
BIGHINTSHOW  <随机进入/@随机进入>         <定点进入/@定点进入>         <不去.../@exit>";
            }
            else if (map.IsRandomEntry)
            {
                script += @"
BIGHINTSHOW
BIGHINTSHOW  <随机进入/@随机进入>                    <不去.../@exit>";
            }
            else
            {
                script += @"
BIGHINTSHOW
BIGHINTSHOW  <进入/@定点进入>                    <不去.../@exit>";
            }

            // 随机进入
            script += $@"

[@随机进入]
#IF
CHECKMEMBERLEVEL >= {map.MinVipLevel}
CHECKLEVEL >= {map.LevelLimit}
#ACT
MOV P0 <$RANDOM({map.RandomXMax - map.RandomXMin})>
MOV P1 <$RANDOM({map.RandomYMax - map.RandomYMin})>
INC P0 {map.RandomXMin}
INC P1 {map.RandomYMin}
MAPMOVE {map.MapId} <$STR(P0)> <$STR(P1)>
SENDMSG 5 【{map.MapName}】欢迎 <$USERNAME> 进入！祝您爆装愉快！
#ELSEACT
MESSAGEBOX 需要{vipLevelName}(充值{map.MinRecharge}元)且{map.LevelLimit}级以上才能进入！";

            // 定点进入
            if (map.IsFixedEntry)
            {
                script += $@"

[@定点进入]
#IF
CHECKMEMBERLEVEL >= {map.MinVipLevel}
CHECKLEVEL >= {map.LevelLimit}
#ACT
MAPMOVE {map.MapId} {map.FixedX} {map.FixedY}
SENDMSG 5 【{map.MapName}】欢迎 <$USERNAME> 进入！祝您爆装愉快！
#ELSEACT
MESSAGEBOX 需要{vipLevelName}(充值{map.MinRecharge}元)且{map.LevelLimit}级以上才能进入！";
            }

            return script;
        }

        #endregion

        #region 辅助方法

        private static VipMap MapVipMap(MySqlDataReader reader)
        {
            return new VipMap
            {
                Id = reader.GetInt32("id"),
                MapId = reader.GetString("map_id"),
                MapName = reader.GetString("map_name"),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                MinVipLevel = reader.GetInt32("min_vip_level"),
                MinRecharge = reader.GetInt32("min_recharge"),
                EntryFee = reader.GetInt32("entry_fee"),
                DailyLimit = reader.GetInt32("daily_limit"),
                LevelLimit = reader.GetInt32("level_limit"),
                IsRandomEntry = reader.GetBoolean("is_random_entry"),
                IsFixedEntry = reader.GetBoolean("is_fixed_entry"),
                RandomXMin = reader.GetInt32("random_x_min"),
                RandomXMax = reader.GetInt32("random_x_max"),
                RandomYMin = reader.GetInt32("random_y_min"),
                RandomYMax = reader.GetInt32("random_y_max"),
                FixedX = reader.GetInt32("fixed_x"),
                FixedY = reader.GetInt32("fixed_y"),
                IsEnabled = reader.GetBoolean("is_enabled"),
                CreatedAt = reader.GetDateTime("created_at")
            };
        }

        #endregion
    }
}
