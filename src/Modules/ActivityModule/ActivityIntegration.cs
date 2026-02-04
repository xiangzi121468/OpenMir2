using ActivityModule.Models;
using NLog;
using SystemModule;
using SystemModule.Enums;

namespace ActivityModule
{
    /// <summary>
    /// 活动系统与游戏服务器集成
    /// 支持静态初始化和实例化两种模式
    /// </summary>
    public class ActivityIntegration
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        // 静态成员用于全局访问
        private static ActivityIntegration _instance;
        private static IActivityService _staticService;
        private static ActivityScheduler _staticScheduler;
        private static bool _initialized = false;
        
        // 回调委托
        private static Action<string> _onBroadcast;
        private static Action<string, string, int, int> _onBossSpawn;
        private static Action<string[], string> _onGiftRain;
        private static Action<string, string, int, int> _onNpcSpawn;
        
        private readonly IActivityService _activityService;
        private readonly ActivityScheduler _scheduler;

        public ActivityIntegration(IActivityService activityService, ActivityScheduler scheduler)
        {
            _activityService = activityService;
            _scheduler = scheduler;

            // 注册事件处理器
            _scheduler.OnBroadcast += HandleBroadcast;
            _scheduler.OnBossSpawn += HandleBossSpawn;
            _scheduler.OnGiftRain += HandleGiftRain;
            _scheduler.OnNpcSpawn += HandleNpcSpawn;
            _scheduler.OnActivityStart += HandleActivityStart;
            _scheduler.OnActivityEnd += HandleActivityEnd;
        }
        
        #region 静态方法 - 用于M2Server集成
        
        /// <summary>
        /// 静态初始化活动系统
        /// </summary>
        public static async Task InitializeAsync(
            string connectionString,
            Action<string> onBroadcast,
            Action<string, string, int, int> onBossSpawn = null,
            Action<string[], string> onGiftRain = null,
            Action<string, string, int, int> onNpcSpawn = null)
        {
            if (_initialized)
                return;
                
            try
            {
                _onBroadcast = onBroadcast;
                _onBossSpawn = onBossSpawn;
                _onGiftRain = onGiftRain;
                _onNpcSpawn = onNpcSpawn;
                
                _staticService = new ActivityService(connectionString);
                _staticScheduler = new ActivityScheduler(_staticService);
                
                // 注册事件
                _staticScheduler.OnBroadcast += (msg, color) => _onBroadcast?.Invoke(msg);
                _staticScheduler.OnBossSpawn += async (boss, map, x, y, count) => 
                {
                    _onBossSpawn?.Invoke(boss, map, x, y);
                    await Task.CompletedTask;
                };
                _staticScheduler.OnGiftRain += async (map, items, count) =>
                {
                    _onGiftRain?.Invoke(items, map);
                    await Task.CompletedTask;
                };
                _staticScheduler.OnNpcSpawn += async (npc, map, x, y) =>
                {
                    _onNpcSpawn?.Invoke(npc, map, x, y);
                    await Task.CompletedTask;
                };
                
                await _staticScheduler.InitializeAsync();
                _staticScheduler.Start();
                _initialized = true;
                
                Logger.Info("活动系统静态初始化成功");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "活动系统初始化失败");
            }
        }
        
        /// <summary>
        /// 定时处理活动（建议每秒调用）
        /// </summary>
        public static async Task ProcessAsync()
        {
            if (!_initialized || _staticScheduler == null)
                return;
                
            await _staticScheduler.ProcessAsync();
        }
        
        /// <summary>
        /// 获取当前经验倍率（静态方法）
        /// </summary>
        public static double GetCurrentExpRate()
        {
            return _staticService?.GetCurrentExpRate() ?? 1.0;
        }
        
        /// <summary>
        /// 获取当前掉落倍率（静态方法）
        /// </summary>
        public static double GetCurrentDropRate()
        {
            return _staticService?.GetCurrentDropRate() ?? 1.0;
        }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => _initialized;
        
        #endregion

        /// <summary>
        /// 启动活动系统
        /// </summary>
        public void Start()
        {
            _scheduler.Start();
            Logger.Info("活动系统已启动并与游戏服务器集成");
        }

        /// <summary>
        /// 停止活动系统
        /// </summary>
        public void Stop()
        {
            _scheduler.Stop();
            Logger.Info("活动系统已停止");
        }

        /// <summary>
        /// 获取当前经验倍率 - 供游戏逻辑调用
        /// </summary>
        public double GetExpRate()
        {
            return _activityService.GetCurrentExpRate();
        }

        /// <summary>
        /// 获取当前掉落倍率 - 供游戏逻辑调用
        /// </summary>
        public double GetDropRate()
        {
            return _activityService.GetCurrentDropRate();
        }

        /// <summary>
        /// 检查玩家是否可以参与活动
        /// </summary>
        public bool CanPlayerJoinActivity(int activityId, int playerLevel, string? mapId, bool isVip)
        {
            return _activityService.CanPlayerJoin(activityId, playerLevel, mapId, isVip);
        }

        /// <summary>
        /// 获取运行中的活动列表
        /// </summary>
        public List<ActivityRuntime> GetRunningActivities()
        {
            return _activityService.GetRunningActivities();
        }

        #region 事件处理器

        private void HandleBroadcast(string message, int colorType)
        {
            try
            {
                // 调用游戏引擎广播接口
                var color = colorType switch
                {
                    0 => MsgColor.Green,
                    1 => MsgColor.Red,
                    2 => MsgColor.Yellow,
                    _ => MsgColor.White
                };

                // 通过SystemShare发送全服广播
                // SystemShare.WorldEngine?.SendBroadCastMsg(message, color, MsgType.System);
                
                Logger.Info($"[活动广播] {message}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "活动广播发送失败");
            }
        }

        private async Task HandleBossSpawn(string bossName, string mapId, int x, int y, int count)
        {
            try
            {
                Logger.Info($"[BOSS刷新] {bossName} x{count} 在地图 {mapId}({x},{y})");

                // 调用游戏引擎刷怪接口
                // var map = SystemShare.MapMgr.FindMap(mapId);
                // if (map != null)
                // {
                //     for (int i = 0; i < count; i++)
                //     {
                //         SystemShare.WorldEngine.CreateMonster(map, x, y, bossName);
                //     }
                // }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"BOSS刷新失败: {bossName}");
            }
        }

        private async Task HandleGiftRain(string mapId, string[] items, int countPerItem)
        {
            try
            {
                Logger.Info($"[天降礼物] 地图 {mapId}, 物品: {string.Join(",", items)}, 每种 x{countPerItem}");

                // 调用游戏引擎掉落物品接口
                // var map = SystemShare.MapMgr.FindMap(mapId);
                // if (map != null)
                // {
                //     foreach (var itemName in items)
                //     {
                //         for (int i = 0; i < countPerItem; i++)
                //         {
                //             var randomX = RandomNumber.Random(map.Width);
                //             var randomY = RandomNumber.Random(map.Height);
                //             SystemShare.ItemSystem.DropItemToMap(map, randomX, randomY, itemName);
                //         }
                //     }
                // }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "天降礼物失败");
            }
        }

        private async Task HandleNpcSpawn(string npcName, string mapId, int x, int y)
        {
            try
            {
                Logger.Info($"[NPC刷新] {npcName} 在地图 {mapId}({x},{y})");

                // 调用游戏引擎创建NPC接口
                // var map = SystemShare.MapMgr.FindMap(mapId);
                // if (map != null)
                // {
                //     SystemShare.WorldEngine.CreateNpc(map, x, y, npcName);
                // }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"NPC刷新失败: {npcName}");
            }
        }

        private async Task HandleActivityStart(GameActivity activity, bool isStart)
        {
            try
            {
                Logger.Info($"[活动开始] {activity.Name} ({activity.ActivityType})");

                // 可以在这里执行活动开始的额外逻辑
                // 例如: 修改全局经验/掉落倍率
                // SystemShare.Config.KillMonExpRateTime = (int)activity.DurationMinutes;
                // SystemShare.Config.KillMonExpRate = activity.GetParam("exp_rate", 1);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"活动开始处理失败: {activity.Name}");
            }
        }

        private async Task HandleActivityEnd(GameActivity activity, bool isEnd)
        {
            try
            {
                Logger.Info($"[活动结束] {activity.Name} ({activity.ActivityType})");

                // 可以在这里执行活动结束的额外逻辑
                // 例如: 恢复全局经验/掉落倍率

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"活动结束处理失败: {activity.Name}");
            }
        }

        #endregion
    }
}
