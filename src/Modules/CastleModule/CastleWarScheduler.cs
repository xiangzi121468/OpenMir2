using NLog;
using System.Timers;

namespace CastleModule
{
    /// <summary>
    /// 攻城战调度器
    /// 负责定时检查和自动开启/结束攻城战
    /// </summary>
    public class CastleWarScheduler : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly ICastleWarService _castleWarService;
        private readonly System.Timers.Timer _checkTimer;
        private readonly int _checkIntervalSeconds;
        private bool _isRunning;

        // 广播回调
        public Action<string> OnBroadcast { get; set; }
        public Action<string, string> OnMapBroadcast { get; set; } // mapName, message

        public CastleWarScheduler(ICastleWarService castleWarService, int checkIntervalSeconds = 60)
        {
            _castleWarService = castleWarService;
            _checkIntervalSeconds = checkIntervalSeconds;
            _checkTimer = new System.Timers.Timer(checkIntervalSeconds * 1000);
            _checkTimer.Elapsed += OnTimerElapsed;
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _checkTimer.Start();

            var nextWar = _castleWarService.GetNextWarDate();
            Logger.Info($"攻城战调度器已启动，下次攻城时间: {nextWar:yyyy-MM-dd HH:mm}");
        }

        public void Stop()
        {
            _isRunning = false;
            _checkTimer.Stop();
            Logger.Info("攻城战调度器已停止");
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await CheckAndProcessWarAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "攻城战调度检查失败");
            }
        }

        private async Task CheckAndProcessWarAsync()
        {
            var now = DateTime.Now;
            var castles = await _castleWarService.GetAllCastlesAsync();

            foreach (var castle in castles)
            {
                var warState = _castleWarService.GetWarState(castle.Id);

                if (warState != null && warState.IsActive)
                {
                    // 检查是否应该结束
                    if (now >= warState.EndTime)
                    {
                        BroadcastMessage($"【{castle.CastleName}】攻城战即将结束！");
                        await Task.Delay(5000);
                        await _castleWarService.EndWarAsync(castle.Id);
                        BroadcastWarResult(castle.Id);
                    }
                    // 倒计时提醒
                    else if (warState.RemainingSeconds <= 300 && warState.RemainingSeconds > 295)
                    {
                        BroadcastMessage($"【{castle.CastleName}】攻城战还剩 5 分钟！");
                    }
                    else if (warState.RemainingSeconds <= 60 && warState.RemainingSeconds > 55)
                    {
                        BroadcastMessage($"【{castle.CastleName}】攻城战还剩 1 分钟！");
                    }
                }
                else
                {
                    // 检查是否应该开始
                    var nextWarTime = _castleWarService.GetNextWarDate();
                    var timeUntilWar = (nextWarTime - now).TotalMinutes;

                    // 提前30分钟提醒
                    if (timeUntilWar <= 30 && timeUntilWar > 29)
                    {
                        BroadcastMessage($"【{castle.CastleName}】攻城战将在 30 分钟后开始！");
                    }
                    // 提前10分钟提醒
                    else if (timeUntilWar <= 10 && timeUntilWar > 9)
                    {
                        BroadcastMessage($"【{castle.CastleName}】攻城战将在 10 分钟后开始！请做好准备！");
                    }
                    // 提前5分钟提醒
                    else if (timeUntilWar <= 5 && timeUntilWar > 4)
                    {
                        BroadcastMessage($"【{castle.CastleName}】攻城战将在 5 分钟后开始！");
                    }
                    // 提前1分钟提醒
                    else if (timeUntilWar <= 1 && timeUntilWar > 0)
                    {
                        BroadcastMessage($"【{castle.CastleName}】攻城战即将开始！");
                    }
                    // 开始攻城
                    else if (timeUntilWar <= 0 && timeUntilWar > -1)
                    {
                        var applicants = await _castleWarService.GetWarApplicantsAsync(castle.Id, nextWarTime.Date);
                        if (applicants.Count > 0 || castle.OwnerGuildId == null)
                        {
                            await _castleWarService.StartWarAsync(castle.Id);
                            BroadcastWarStart(castle);
                        }
                        else
                        {
                            Logger.Info($"无行会报名攻城，跳过: {castle.CastleName}");
                        }
                    }
                }
            }
        }

        private void BroadcastWarStart(CastleInfo castle)
        {
            var message = castle.OwnerGuildId.HasValue
                ? $"【{castle.CastleName}】攻城战开始！守城方: [{castle.OwnerGuildName}]，城主: {castle.OwnerName}"
                : $"【{castle.CastleName}】攻城战开始！当前无城主，先占领皇宫者获胜！";

            BroadcastMessage(message);
        }

        private async void BroadcastWarResult(int castleId)
        {
            var castle = await _castleWarService.GetCastleAsync(castleId);
            if (castle == null) return;

            var history = await _castleWarService.GetWarHistoryAsync(castleId, 1);
            if (history.Count == 0) return;

            var lastWar = history[0];
            string message;

            if (lastWar.Result == "attacker_win")
            {
                message = $"【{castle.CastleName}】攻城战结束！[{lastWar.WinnerGuildName}] 成功夺取沙城！新城主: {lastWar.WinnerName}";
            }
            else if (lastWar.Result == "defender_win")
            {
                message = $"【{castle.CastleName}】攻城战结束！[{lastWar.DefenderGuildName}] 成功守城！";
            }
            else
            {
                message = $"【{castle.CastleName}】攻城战结束！";
            }

            BroadcastMessage(message);
        }

        private void BroadcastMessage(string message)
        {
            Logger.Info($"攻城战广播: {message}");
            OnBroadcast?.Invoke(message);
        }

        public void Dispose()
        {
            Stop();
            _checkTimer?.Dispose();
        }
    }
}
