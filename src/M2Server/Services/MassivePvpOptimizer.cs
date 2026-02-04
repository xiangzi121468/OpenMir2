using NLog;
using System.Collections.Concurrent;

namespace M2Server.Services
{
    /// <summary>
    /// 大规模PVP性能优化器
    /// 用于攻城战等大量玩家聚集场景
    /// </summary>
    public class MassivePvpOptimizer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static MassivePvpOptimizer _instance;
        public static MassivePvpOptimizer Instance => _instance ??= new MassivePvpOptimizer();

        // AOI九宫格管理
        private readonly ConcurrentDictionary<string, AoiGrid> _mapGrids = new();
        
        // 消息合并缓冲
        private readonly ConcurrentDictionary<string, MessageBuffer> _messageBuffers = new();
        
        // 特效合并队列
        private readonly ConcurrentDictionary<string, EffectBatch> _effectBatches = new();

        // 配置
        private readonly PvpOptimizeConfig _config = new();

        #region AOI 视野管理

        /// <summary>
        /// 获取地图的AOI网格
        /// </summary>
        public AoiGrid GetOrCreateGrid(string mapName, int mapWidth, int mapHeight)
        {
            return _mapGrids.GetOrAdd(mapName, _ => new AoiGrid(mapName, mapWidth, mapHeight, _config.GridSize));
        }

        /// <summary>
        /// 更新玩家位置（进入新格子时触发视野更新）
        /// </summary>
        public (List<int> enterViewIds, List<int> leaveViewIds) UpdatePlayerPosition(
            string mapName, int playerId, int oldX, int oldY, int newX, int newY)
        {
            if (!_mapGrids.TryGetValue(mapName, out var grid))
                return (new List<int>(), new List<int>());

            var oldGridKey = grid.GetGridKey(oldX, oldY);
            var newGridKey = grid.GetGridKey(newX, newY);

            if (oldGridKey == newGridKey)
                return (new List<int>(), new List<int>()); // 同一格子，无需更新

            // 移除旧格子
            grid.RemoveEntity(oldGridKey, playerId);
            
            // 添加到新格子
            grid.AddEntity(newGridKey, playerId);

            // 计算视野变化
            var oldNearby = grid.GetNearbyGridKeys(oldX, oldY);
            var newNearby = grid.GetNearbyGridKeys(newX, newY);

            var enterGrids = newNearby.Except(oldNearby).ToList();
            var leaveGrids = oldNearby.Except(newNearby).ToList();

            var enterViewIds = new List<int>();
            var leaveViewIds = new List<int>();

            foreach (var gridKey in enterGrids)
            {
                enterViewIds.AddRange(grid.GetEntitiesInGrid(gridKey));
            }

            foreach (var gridKey in leaveGrids)
            {
                leaveViewIds.AddRange(grid.GetEntitiesInGrid(gridKey));
            }

            return (enterViewIds, leaveViewIds);
        }

        /// <summary>
        /// 获取视野内的玩家ID列表
        /// </summary>
        public List<int> GetPlayersInView(string mapName, int x, int y)
        {
            if (!_mapGrids.TryGetValue(mapName, out var grid))
                return new List<int>();

            return grid.GetNearbyEntities(x, y);
        }

        /// <summary>
        /// 获取视野内的玩家数量
        /// </summary>
        public int GetPlayerCountInView(string mapName, int x, int y)
        {
            if (!_mapGrids.TryGetValue(mapName, out var grid))
                return 0;

            return grid.GetNearbyEntityCount(x, y);
        }

        #endregion

        #region 消息广播优化

        /// <summary>
        /// 添加消息到合并缓冲区（批量发送）
        /// </summary>
        public void QueueBroadcastMessage(string mapName, int x, int y, byte[] message, MessageType msgType)
        {
            var bufferKey = $"{mapName}_{x / _config.GridSize}_{y / _config.GridSize}";
            var buffer = _messageBuffers.GetOrAdd(bufferKey, _ => new MessageBuffer());

            lock (buffer)
            {
                buffer.Messages.Add(new QueuedMessage
                {
                    Data = message,
                    Type = msgType,
                    X = x,
                    Y = y,
                    Timestamp = DateTime.Now
                });

                // 达到阈值或时间间隔，立即发送
                if (buffer.Messages.Count >= _config.MessageBatchSize)
                {
                    FlushMessageBuffer(mapName, buffer);
                }
            }
        }

        /// <summary>
        /// 刷新消息缓冲区（定时调用）
        /// </summary>
        public void FlushAllMessageBuffers()
        {
            foreach (var kvp in _messageBuffers)
            {
                var parts = kvp.Key.Split('_');
                if (parts.Length >= 1)
                {
                    var mapName = parts[0];
                    FlushMessageBuffer(mapName, kvp.Value);
                }
            }
        }

        private void FlushMessageBuffer(string mapName, MessageBuffer buffer)
        {
            List<QueuedMessage> messages;
            lock (buffer)
            {
                if (buffer.Messages.Count == 0) return;
                messages = new List<QueuedMessage>(buffer.Messages);
                buffer.Messages.Clear();
            }

            // 合并相同类型的消息
            var mergedMessages = MergeMessages(messages);

            // 发送合并后的消息
            foreach (var msg in mergedMessages)
            {
                BroadcastToNearbyPlayers(mapName, msg.X, msg.Y, msg.Data);
            }
        }

        private List<QueuedMessage> MergeMessages(List<QueuedMessage> messages)
        {
            // 按类型分组并合并
            var grouped = messages.GroupBy(m => m.Type);
            var result = new List<QueuedMessage>();

            foreach (var group in grouped)
            {
                switch (group.Key)
                {
                    case MessageType.Movement:
                        // 移动消息：只保留最新位置
                        var latestMovement = group.OrderByDescending(m => m.Timestamp).First();
                        result.Add(latestMovement);
                        break;

                    case MessageType.Effect:
                        // 特效消息：合并为批量特效
                        if (group.Count() > _config.EffectMergeThreshold)
                        {
                            result.Add(MergeEffectMessages(group.ToList()));
                        }
                        else
                        {
                            result.AddRange(group);
                        }
                        break;

                    default:
                        result.AddRange(group);
                        break;
                }
            }

            return result;
        }

        private QueuedMessage MergeEffectMessages(List<QueuedMessage> effects)
        {
            // 合并多个特效为一个批量特效消息
            // 实际实现需要根据协议格式来处理
            return new QueuedMessage
            {
                Data = CreateBatchEffectMessage(effects),
                Type = MessageType.BatchEffect,
                X = effects[0].X,
                Y = effects[0].Y,
                Timestamp = DateTime.Now
            };
        }

        private byte[] CreateBatchEffectMessage(List<QueuedMessage> effects)
        {
            // TODO: 根据实际协议格式实现批量特效消息
            // 这里返回简化的合并消息
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            
            bw.Write((byte)0xBE); // 批量特效标识
            bw.Write((ushort)effects.Count);
            
            foreach (var effect in effects)
            {
                bw.Write((ushort)effect.Data.Length);
                bw.Write(effect.Data);
            }
            
            return ms.ToArray();
        }

        private void BroadcastToNearbyPlayers(string mapName, int x, int y, byte[] data)
        {
            // 获取视野内玩家并发送
            var playerIds = GetPlayersInView(mapName, x, y);
            foreach (var playerId in playerIds)
            {
                // TODO: 通过WorldEngine发送消息
                // SystemShare.WorldEngine.SendToPlayer(playerId, data);
            }
        }

        #endregion

        #region 技能特效优化

        /// <summary>
        /// 添加技能特效到批量队列
        /// </summary>
        public void QueueSkillEffect(string mapName, int x, int y, int skillId, int casterId, int targetId)
        {
            var batchKey = $"{mapName}_{x / _config.GridSize}_{y / _config.GridSize}";
            var batch = _effectBatches.GetOrAdd(batchKey, _ => new EffectBatch());

            lock (batch)
            {
                batch.Effects.Add(new SkillEffect
                {
                    SkillId = skillId,
                    CasterId = casterId,
                    TargetId = targetId,
                    X = x,
                    Y = y,
                    Timestamp = DateTime.Now
                });

                // 达到阈值，合并发送
                if (batch.Effects.Count >= _config.EffectBatchSize)
                {
                    FlushEffectBatch(mapName, batch);
                }
            }
        }

        /// <summary>
        /// 刷新所有特效批次
        /// </summary>
        public void FlushAllEffectBatches()
        {
            foreach (var kvp in _effectBatches)
            {
                var parts = kvp.Key.Split('_');
                if (parts.Length >= 1)
                {
                    FlushEffectBatch(parts[0], kvp.Value);
                }
            }
        }

        private void FlushEffectBatch(string mapName, EffectBatch batch)
        {
            List<SkillEffect> effects;
            lock (batch)
            {
                if (batch.Effects.Count == 0) return;
                effects = new List<SkillEffect>(batch.Effects);
                batch.Effects.Clear();
            }

            // 按技能类型分组
            var grouped = effects.GroupBy(e => e.SkillId);

            foreach (var group in grouped)
            {
                var skillEffects = group.ToList();
                
                // 同一技能超过阈值，简化显示
                if (skillEffects.Count > _config.SameSkillSimplifyThreshold)
                {
                    // 只显示部分特效，其他用简化表示
                    var displayCount = Math.Min(skillEffects.Count, _config.MaxDisplayEffects);
                    var displayed = skillEffects.Take(displayCount).ToList();
                    var remaining = skillEffects.Count - displayCount;

                    // 发送显示的特效
                    foreach (var effect in displayed)
                    {
                        SendSkillEffect(mapName, effect);
                    }

                    // 如果还有剩余，发送一个汇总提示
                    if (remaining > 0)
                    {
                        SendEffectSummary(mapName, group.Key, remaining, skillEffects[0].X, skillEffects[0].Y);
                    }
                }
                else
                {
                    // 正常发送
                    foreach (var effect in skillEffects)
                    {
                        SendSkillEffect(mapName, effect);
                    }
                }
            }
        }

        private void SendSkillEffect(string mapName, SkillEffect effect)
        {
            // TODO: 实际发送技能特效
            // 这里需要调用游戏引擎的特效发送方法
        }

        private void SendEffectSummary(string mapName, int skillId, int count, int x, int y)
        {
            // 发送简化的特效汇总（例如显示"火球术 x15"）
            Logger.Debug($"特效汇总: {mapName} 技能{skillId} x{count} 位置({x},{y})");
        }

        #endregion

        #region 大规模战斗优化

        /// <summary>
        /// 检查是否处于大规模战斗模式
        /// </summary>
        public bool IsMassiveBattleMode(string mapName, int x, int y)
        {
            var playerCount = GetPlayerCountInView(mapName, x, y);
            return playerCount >= _config.MassiveBattleThreshold;
        }

        /// <summary>
        /// 获取大规模战斗的优化参数
        /// </summary>
        public BattleOptimizeParams GetBattleOptimizeParams(string mapName, int x, int y)
        {
            var playerCount = GetPlayerCountInView(mapName, x, y);

            if (playerCount < 30)
            {
                // 正常模式
                return new BattleOptimizeParams
                {
                    Mode = BattleMode.Normal,
                    DamageTickRate = 100,
                    EffectLevel = EffectLevel.Full,
                    BroadcastRate = 1.0f,
                    AoiRange = _config.NormalAoiRange
                };
            }
            else if (playerCount < 100)
            {
                // 中等规模
                return new BattleOptimizeParams
                {
                    Mode = BattleMode.Medium,
                    DamageTickRate = 150,
                    EffectLevel = EffectLevel.Reduced,
                    BroadcastRate = 0.7f,
                    AoiRange = _config.MediumAoiRange
                };
            }
            else
            {
                // 大规模战斗
                return new BattleOptimizeParams
                {
                    Mode = BattleMode.Massive,
                    DamageTickRate = 200,
                    EffectLevel = EffectLevel.Minimal,
                    BroadcastRate = 0.5f,
                    AoiRange = _config.MassiveAoiRange
                };
            }
        }

        /// <summary>
        /// 伤害计算优化（批量处理）
        /// </summary>
        public void ProcessBatchDamage(List<DamageInfo> damages)
        {
            // 按目标分组，合并伤害
            var grouped = damages.GroupBy(d => d.TargetId);

            foreach (var group in grouped)
            {
                var targetId = group.Key;
                var totalDamage = group.Sum(d => d.Damage);
                var attackers = group.Select(d => d.AttackerId).Distinct().ToList();

                // 发送合并后的伤害
                ApplyMergedDamage(targetId, totalDamage, attackers);
            }
        }

        private void ApplyMergedDamage(int targetId, int totalDamage, List<int> attackers)
        {
            // TODO: 应用合并伤害
            // SystemShare.WorldEngine.ApplyDamage(targetId, totalDamage, attackers);
        }

        #endregion

        #region 定时处理

        /// <summary>
        /// 定时处理（每帧或每tick调用）
        /// </summary>
        public void ProcessTick()
        {
            FlushAllMessageBuffers();
            FlushAllEffectBatches();
        }

        #endregion
    }

    #region 辅助类

    /// <summary>
    /// AOI网格
    /// </summary>
    public class AoiGrid
    {
        private readonly string _mapName;
        private readonly int _gridSize;
        private readonly int _gridCountX;
        private readonly int _gridCountY;
        private readonly ConcurrentDictionary<string, HashSet<int>> _grids = new();
        private readonly object _lock = new();

        public AoiGrid(string mapName, int mapWidth, int mapHeight, int gridSize)
        {
            _mapName = mapName;
            _gridSize = gridSize;
            _gridCountX = (mapWidth / gridSize) + 1;
            _gridCountY = (mapHeight / gridSize) + 1;
        }

        public string GetGridKey(int x, int y)
        {
            return $"{x / _gridSize}_{y / _gridSize}";
        }

        public void AddEntity(string gridKey, int entityId)
        {
            var grid = _grids.GetOrAdd(gridKey, _ => new HashSet<int>());
            lock (grid)
            {
                grid.Add(entityId);
            }
        }

        public void RemoveEntity(string gridKey, int entityId)
        {
            if (_grids.TryGetValue(gridKey, out var grid))
            {
                lock (grid)
                {
                    grid.Remove(entityId);
                }
            }
        }

        public List<int> GetEntitiesInGrid(string gridKey)
        {
            if (_grids.TryGetValue(gridKey, out var grid))
            {
                lock (grid)
                {
                    return grid.ToList();
                }
            }
            return new List<int>();
        }

        public List<string> GetNearbyGridKeys(int x, int y)
        {
            var keys = new List<string>();
            var gx = x / _gridSize;
            var gy = y / _gridSize;

            // 九宫格
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var nx = gx + dx;
                    var ny = gy + dy;
                    if (nx >= 0 && ny >= 0 && nx < _gridCountX && ny < _gridCountY)
                    {
                        keys.Add($"{nx}_{ny}");
                    }
                }
            }

            return keys;
        }

        public List<int> GetNearbyEntities(int x, int y)
        {
            var result = new List<int>();
            var nearbyKeys = GetNearbyGridKeys(x, y);

            foreach (var key in nearbyKeys)
            {
                result.AddRange(GetEntitiesInGrid(key));
            }

            return result;
        }

        public int GetNearbyEntityCount(int x, int y)
        {
            var count = 0;
            var nearbyKeys = GetNearbyGridKeys(x, y);

            foreach (var key in nearbyKeys)
            {
                if (_grids.TryGetValue(key, out var grid))
                {
                    lock (grid)
                    {
                        count += grid.Count;
                    }
                }
            }

            return count;
        }
    }

    /// <summary>
    /// 消息缓冲区
    /// </summary>
    public class MessageBuffer
    {
        public List<QueuedMessage> Messages { get; } = new();
        public DateTime LastFlush { get; set; } = DateTime.Now;
    }

    public class QueuedMessage
    {
        public byte[] Data { get; set; }
        public MessageType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum MessageType
    {
        Movement,
        Attack,
        Effect,
        BatchEffect,
        Chat,
        Other
    }

    /// <summary>
    /// 特效批次
    /// </summary>
    public class EffectBatch
    {
        public List<SkillEffect> Effects { get; } = new();
    }

    public class SkillEffect
    {
        public int SkillId { get; set; }
        public int CasterId { get; set; }
        public int TargetId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 伤害信息
    /// </summary>
    public class DamageInfo
    {
        public int AttackerId { get; set; }
        public int TargetId { get; set; }
        public int Damage { get; set; }
        public int SkillId { get; set; }
    }

    /// <summary>
    /// 战斗优化参数
    /// </summary>
    public class BattleOptimizeParams
    {
        public BattleMode Mode { get; set; }
        public int DamageTickRate { get; set; } // 伤害计算间隔(ms)
        public EffectLevel EffectLevel { get; set; }
        public float BroadcastRate { get; set; } // 广播比例
        public int AoiRange { get; set; } // 视野范围
    }

    public enum BattleMode
    {
        Normal,  // 正常模式 (<30人)
        Medium,  // 中等规模 (30-100人)
        Massive  // 大规模战斗 (>100人)
    }

    public enum EffectLevel
    {
        Full,    // 完整特效
        Reduced, // 减少特效
        Minimal  // 最小特效
    }

    /// <summary>
    /// PVP优化配置
    /// </summary>
    public class PvpOptimizeConfig
    {
        public int GridSize { get; set; } = 20; // 格子大小
        public int MessageBatchSize { get; set; } = 10; // 消息批量大小
        public int EffectBatchSize { get; set; } = 20; // 特效批量大小
        public int EffectMergeThreshold { get; set; } = 5; // 特效合并阈值
        public int SameSkillSimplifyThreshold { get; set; } = 10; // 同技能简化阈值
        public int MaxDisplayEffects { get; set; } = 5; // 最大显示特效数
        public int MassiveBattleThreshold { get; set; } = 50; // 大规模战斗阈值
        public int NormalAoiRange { get; set; } = 20; // 正常视野范围
        public int MediumAoiRange { get; set; } = 15; // 中等规模视野
        public int MassiveAoiRange { get; set; } = 10; // 大规模视野
    }

    #endregion
}
