using NLog;
using System.Collections.Concurrent;
using CastleModule;

namespace M2Server.Services
{
    /// <summary>
    /// 攻城战战斗管理器
    /// 专门处理大规模PVP场景的战斗逻辑
    /// </summary>
    public class CastleWarBattleManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static CastleWarBattleManager _instance;
        public static CastleWarBattleManager Instance => _instance ??= new CastleWarBattleManager();

        // 战斗区域
        private readonly ConcurrentDictionary<int, BattleZone> _battleZones = new();
        
        // 玩家战斗数据
        private readonly ConcurrentDictionary<int, PlayerBattleData> _playerData = new();
        
        // 伤害队列（用于批量处理）
        private readonly ConcurrentQueue<DamageEvent> _damageQueue = new();
        
        // 击杀队列
        private readonly ConcurrentQueue<KillEvent> _killQueue = new();

        // 优化器引用
        private readonly MassivePvpOptimizer _optimizer = MassivePvpOptimizer.Instance;

        // 配置
        private readonly CastleWarBattleConfig _config = new();

        #region 战斗区域管理

        /// <summary>
        /// 创建战斗区域（攻城战开始时调用）
        /// </summary>
        public void CreateBattleZone(int castleId, string mapName, int centerX, int centerY, int radius)
        {
            var zone = new BattleZone
            {
                CastleId = castleId,
                MapName = mapName,
                CenterX = centerX,
                CenterY = centerY,
                Radius = radius,
                StartTime = DateTime.Now,
                IsActive = true
            };

            _battleZones[castleId] = zone;
            Logger.Info($"创建战斗区域: CastleId={castleId}, Map={mapName}, Center=({centerX},{centerY})");
        }

        /// <summary>
        /// 移除战斗区域（攻城战结束时调用）
        /// </summary>
        public void RemoveBattleZone(int castleId)
        {
            if (_battleZones.TryRemove(castleId, out var zone))
            {
                Logger.Info($"移除战斗区域: CastleId={castleId}");
                
                // 清理该区域的玩家数据
                var keysToRemove = _playerData.Where(kv => kv.Value.CastleId == castleId)
                    .Select(kv => kv.Key).ToList();
                foreach (var key in keysToRemove)
                {
                    _playerData.TryRemove(key, out _);
                }
            }
        }

        /// <summary>
        /// 检查玩家是否在战斗区域内
        /// </summary>
        public bool IsInBattleZone(string mapName, int x, int y)
        {
            foreach (var zone in _battleZones.Values)
            {
                if (zone.MapName == mapName && zone.IsActive)
                {
                    var distance = Math.Sqrt(Math.Pow(x - zone.CenterX, 2) + Math.Pow(y - zone.CenterY, 2));
                    if (distance <= zone.Radius)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取战斗区域
        /// </summary>
        public BattleZone GetBattleZone(string mapName, int x, int y)
        {
            foreach (var zone in _battleZones.Values)
            {
                if (zone.MapName == mapName && zone.IsActive)
                {
                    var distance = Math.Sqrt(Math.Pow(x - zone.CenterX, 2) + Math.Pow(y - zone.CenterY, 2));
                    if (distance <= zone.Radius)
                        return zone;
                }
            }
            return null;
        }

        #endregion

        #region 玩家进入/离开战斗

        /// <summary>
        /// 玩家进入战斗区域
        /// </summary>
        public void OnPlayerEnterBattleZone(int playerId, int castleId, int guildId, string guildName, bool isDefender)
        {
            var data = _playerData.GetOrAdd(playerId, _ => new PlayerBattleData
            {
                PlayerId = playerId,
                CastleId = castleId,
                GuildId = guildId,
                GuildName = guildName,
                IsDefender = isDefender,
                EnterTime = DateTime.Now
            });

            data.IsInZone = true;

            // 更新区域玩家计数
            if (_battleZones.TryGetValue(castleId, out var zone))
            {
                lock (zone)
                {
                    if (isDefender)
                        zone.DefenderCount++;
                    else
                        zone.AttackerCount++;
                }
            }

            Logger.Debug($"玩家进入战斗区域: PlayerId={playerId}, Castle={castleId}, Defender={isDefender}");
        }

        /// <summary>
        /// 玩家离开战斗区域
        /// </summary>
        public void OnPlayerLeaveBattleZone(int playerId)
        {
            if (_playerData.TryGetValue(playerId, out var data))
            {
                data.IsInZone = false;
                data.LeaveTime = DateTime.Now;

                // 更新区域玩家计数
                if (_battleZones.TryGetValue(data.CastleId, out var zone))
                {
                    lock (zone)
                    {
                        if (data.IsDefender)
                            zone.DefenderCount = Math.Max(0, zone.DefenderCount - 1);
                        else
                            zone.AttackerCount = Math.Max(0, zone.AttackerCount - 1);
                    }
                }
            }
        }

        #endregion

        #region 伤害处理

        /// <summary>
        /// 处理攻城战伤害（加入队列批量处理）
        /// </summary>
        public void EnqueueDamage(int attackerId, int targetId, int damage, int skillId, string mapName, int x, int y)
        {
            _damageQueue.Enqueue(new DamageEvent
            {
                AttackerId = attackerId,
                TargetId = targetId,
                Damage = damage,
                SkillId = skillId,
                MapName = mapName,
                X = x,
                Y = y,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 批量处理伤害队列
        /// </summary>
        public void ProcessDamageQueue()
        {
            var batch = new List<DamageEvent>();
            var processLimit = _config.DamageBatchSize;

            while (batch.Count < processLimit && _damageQueue.TryDequeue(out var damage))
            {
                batch.Add(damage);
            }

            if (batch.Count == 0) return;

            // 按目标分组处理
            var grouped = batch.GroupBy(d => d.TargetId);

            foreach (var group in grouped)
            {
                var targetId = group.Key;
                var totalDamage = 0;
                var attackers = new HashSet<int>();
                int lastSkillId = 0;
                string mapName = "";
                int x = 0, y = 0;

                foreach (var dmg in group)
                {
                    totalDamage += dmg.Damage;
                    attackers.Add(dmg.AttackerId);
                    lastSkillId = dmg.SkillId;
                    mapName = dmg.MapName;
                    x = dmg.X;
                    y = dmg.Y;

                    // 更新攻击者数据
                    UpdateAttackerStats(dmg.AttackerId, dmg.Damage);
                }

                // 应用合并伤害
                ApplyDamageToTarget(targetId, totalDamage, attackers.ToList(), mapName, x, y);
            }
        }

        private void UpdateAttackerStats(int attackerId, int damage)
        {
            if (_playerData.TryGetValue(attackerId, out var data))
            {
                data.DamageDealt += damage;
            }
        }

        private void ApplyDamageToTarget(int targetId, int totalDamage, List<int> attackers, string mapName, int x, int y)
        {
            // 更新受击者数据
            if (_playerData.TryGetValue(targetId, out var data))
            {
                data.DamageTaken += totalDamage;
            }

            // TODO: 调用游戏引擎应用伤害
            // var target = SystemShare.WorldEngine.GetPlayObject(targetId);
            // if (target != null)
            // {
            //     target.ReceiveDamage(totalDamage, attackers[0]);
            // }

            Logger.Debug($"应用合并伤害: Target={targetId}, Damage={totalDamage}, Attackers={attackers.Count}");
        }

        #endregion

        #region 击杀处理

        /// <summary>
        /// 记录击杀
        /// </summary>
        public void RecordKill(int killerId, string killerName, int? killerGuildId, string killerGuildName,
            int victimId, string victimName, int? victimGuildId, string victimGuildName,
            int castleId, string mapName, int x, int y)
        {
            _killQueue.Enqueue(new KillEvent
            {
                KillerId = killerId,
                KillerName = killerName,
                KillerGuildId = killerGuildId,
                KillerGuildName = killerGuildName,
                VictimId = victimId,
                VictimName = victimName,
                VictimGuildId = victimGuildId,
                VictimGuildName = victimGuildName,
                CastleId = castleId,
                MapName = mapName,
                X = x,
                Y = y,
                Timestamp = DateTime.Now
            });

            // 更新玩家数据
            if (_playerData.TryGetValue(killerId, out var killerData))
            {
                killerData.Kills++;
            }
            if (_playerData.TryGetValue(victimId, out var victimData))
            {
                victimData.Deaths++;
            }

            // 更新区域击杀数
            if (_battleZones.TryGetValue(castleId, out var zone))
            {
                Interlocked.Increment(ref zone.TotalKills);
            }
        }

        /// <summary>
        /// 获取击杀排行
        /// </summary>
        public List<PlayerBattleData> GetKillRanking(int castleId, int top = 10)
        {
            return _playerData.Values
                .Where(p => p.CastleId == castleId)
                .OrderByDescending(p => p.Kills)
                .Take(top)
                .ToList();
        }

        /// <summary>
        /// 获取伤害排行
        /// </summary>
        public List<PlayerBattleData> GetDamageRanking(int castleId, int top = 10)
        {
            return _playerData.Values
                .Where(p => p.CastleId == castleId)
                .OrderByDescending(p => p.DamageDealt)
                .Take(top)
                .ToList();
        }

        #endregion

        #region 复活控制

        /// <summary>
        /// 获取复活延迟时间
        /// </summary>
        public int GetRespawnDelay(int playerId)
        {
            if (_playerData.TryGetValue(playerId, out var data))
            {
                // 根据死亡次数增加复活时间
                var baseDelay = _config.BaseRespawnDelay;
                var extraDelay = Math.Min(data.Deaths * _config.RespawnDelayPerDeath, _config.MaxRespawnDelay - baseDelay);
                return baseDelay + extraDelay;
            }
            return _config.BaseRespawnDelay;
        }

        /// <summary>
        /// 获取复活点
        /// </summary>
        public (string mapName, int x, int y) GetRespawnPoint(int playerId, int castleId)
        {
            if (_playerData.TryGetValue(playerId, out var data))
            {
                if (data.IsDefender)
                {
                    // 守城方在城内复活
                    return (_config.DefenderRespawnMap, _config.DefenderRespawnX, _config.DefenderRespawnY);
                }
                else
                {
                    // 攻城方在城外复活
                    return (_config.AttackerRespawnMap, _config.AttackerRespawnX, _config.AttackerRespawnY);
                }
            }
            return (_config.DefaultRespawnMap, _config.DefaultRespawnX, _config.DefaultRespawnY);
        }

        #endregion

        #region 战斗统计

        /// <summary>
        /// 获取战斗统计
        /// </summary>
        public BattleStatistics GetBattleStatistics(int castleId)
        {
            var stats = new BattleStatistics { CastleId = castleId };

            if (_battleZones.TryGetValue(castleId, out var zone))
            {
                stats.MapName = zone.MapName;
                stats.StartTime = zone.StartTime;
                stats.Duration = (int)(DateTime.Now - zone.StartTime).TotalSeconds;
                stats.AttackerCount = zone.AttackerCount;
                stats.DefenderCount = zone.DefenderCount;
                stats.TotalKills = zone.TotalKills;
            }

            var players = _playerData.Values.Where(p => p.CastleId == castleId).ToList();
            stats.TotalDamageDealt = players.Sum(p => p.DamageDealt);
            stats.TotalDamageTaken = players.Sum(p => p.DamageTaken);
            stats.TopKiller = players.OrderByDescending(p => p.Kills).FirstOrDefault();
            stats.TopDamager = players.OrderByDescending(p => p.DamageDealt).FirstOrDefault();

            return stats;
        }

        #endregion

        #region 定时处理

        /// <summary>
        /// 定时处理（每帧调用）
        /// </summary>
        public void ProcessTick()
        {
            ProcessDamageQueue();
            
            // 处理击杀队列（可以异步写入数据库）
            ProcessKillQueue();
        }

        private void ProcessKillQueue()
        {
            var batch = new List<KillEvent>();
            while (batch.Count < 100 && _killQueue.TryDequeue(out var kill))
            {
                batch.Add(kill);
            }

            if (batch.Count > 0)
            {
                // TODO: 批量写入数据库
                // Task.Run(() => SaveKillsToDatabase(batch));
            }
        }

        #endregion
    }

    #region 数据模型

    public class BattleZone
    {
        public int CastleId { get; set; }
        public string MapName { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int Radius { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsActive { get; set; }
        public int AttackerCount { get; set; }
        public int DefenderCount { get; set; }
        public int TotalKills;
    }

    public class PlayerBattleData
    {
        public int PlayerId { get; set; }
        public int CastleId { get; set; }
        public int GuildId { get; set; }
        public string GuildName { get; set; }
        public bool IsDefender { get; set; }
        public bool IsInZone { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime? LeaveTime { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public long DamageDealt { get; set; }
        public long DamageTaken { get; set; }
        public long HealingDone { get; set; }
        public long GateDamage { get; set; }

        public int Score => Kills * 10 - Deaths * 3 + (int)(DamageDealt / 1000) + (int)(GateDamage / 500);
    }

    public class DamageEvent
    {
        public int AttackerId { get; set; }
        public int TargetId { get; set; }
        public int Damage { get; set; }
        public int SkillId { get; set; }
        public string MapName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class KillEvent
    {
        public int KillerId { get; set; }
        public string KillerName { get; set; }
        public int? KillerGuildId { get; set; }
        public string KillerGuildName { get; set; }
        public int VictimId { get; set; }
        public string VictimName { get; set; }
        public int? VictimGuildId { get; set; }
        public string VictimGuildName { get; set; }
        public int CastleId { get; set; }
        public string MapName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class BattleStatistics
    {
        public int CastleId { get; set; }
        public string MapName { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int AttackerCount { get; set; }
        public int DefenderCount { get; set; }
        public int TotalKills { get; set; }
        public long TotalDamageDealt { get; set; }
        public long TotalDamageTaken { get; set; }
        public PlayerBattleData TopKiller { get; set; }
        public PlayerBattleData TopDamager { get; set; }
    }

    public class CastleWarBattleConfig
    {
        public int DamageBatchSize { get; set; } = 50;
        public int BaseRespawnDelay { get; set; } = 10; // 基础复活延迟（秒）
        public int RespawnDelayPerDeath { get; set; } = 5; // 每次死亡增加的延迟
        public int MaxRespawnDelay { get; set; } = 60; // 最大复活延迟
        
        // 复活点配置
        public string DefenderRespawnMap { get; set; } = "D716"; // 守城方复活地图
        public int DefenderRespawnX { get; set; } = 15;
        public int DefenderRespawnY { get; set; } = 15;
        public string AttackerRespawnMap { get; set; } = "D715"; // 攻城方复活地图
        public int AttackerRespawnX { get; set; } = 100;
        public int AttackerRespawnY { get; set; } = 100;
        public string DefaultRespawnMap { get; set; } = "0";
        public int DefaultRespawnX { get; set; } = 330;
        public int DefaultRespawnY { get; set; } = 330;
    }

    #endregion
}
