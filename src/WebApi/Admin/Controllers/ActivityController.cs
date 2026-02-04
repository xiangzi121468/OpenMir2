using ActivityModule;
using ActivityModule.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 活动管理控制器
    /// </summary>
    [ApiController]
    [Route("api/admin/activity")]
    [Authorize]
    public class ActivityController : AdminBaseController
    {
        private readonly IActivityService _activityService;
        private readonly ActivityScheduler _scheduler;
        private readonly ILogger<ActivityController> _logger;

        public ActivityController(
            IActivityService activityService,
            ActivityScheduler scheduler,
            ILogger<ActivityController> logger)
        {
            _activityService = activityService;
            _scheduler = scheduler;
            _logger = logger;
        }

        #region 活动配置管理

        /// <summary>
        /// 获取活动列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetActivityList([FromQuery] ActivityQueryRequest request)
        {
            try
            {
                var (items, total) = await _activityService.QueryActivitiesAsync(request);
                return Ok(new
                {
                    success = true,
                    data = items,
                    total,
                    page = request.Page,
                    pageSize = request.PageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活动列表失败");
                return Ok(new { success = false, message = "获取活动列表失败" });
            }
        }

        /// <summary>
        /// 获取活动详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetActivity(int id)
        {
            try
            {
                var activity = await _activityService.GetActivityByIdAsync(id);
                if (activity == null)
                    return Ok(new { success = false, message = "活动不存在" });

                var rewards = await _activityService.GetActivityRewardsAsync(id);
                return Ok(new { success = true, data = activity, rewards });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取活动详情失败: {id}");
                return Ok(new { success = false, message = "获取活动详情失败" });
            }
        }

        /// <summary>
        /// 创建/更新活动
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveActivity([FromBody] ActivitySaveRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                    return Ok(new { success = false, message = "活动名称不能为空" });

                var operatorName = GetCurrentUserName();
                var result = await _activityService.SaveActivityAsync(request, operatorName);

                if (result)
                {
                    _logger.LogInformation($"活动保存成功: {request.Name}, 操作人: {operatorName}");
                    return Ok(new { success = true, message = "保存成功" });
                }

                return Ok(new { success = false, message = "保存失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存活动失败");
                return Ok(new { success = false, message = "保存活动失败" });
            }
        }

        /// <summary>
        /// 删除活动
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                var result = await _activityService.DeleteActivityAsync(id);
                if (result)
                {
                    _logger.LogInformation($"活动已删除: {id}, 操作人: {GetCurrentUserName()}");
                    return Ok(new { success = true, message = "删除成功" });
                }
                return Ok(new { success = false, message = "删除失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除活动失败: {id}");
                return Ok(new { success = false, message = "删除活动失败" });
            }
        }

        /// <summary>
        /// 启用/禁用活动
        /// </summary>
        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleActivity(int id, [FromQuery] bool enabled)
        {
            try
            {
                var result = await _activityService.SetActivityEnabledAsync(id, enabled);
                if (result)
                {
                    var action = enabled ? "启用" : "禁用";
                    _logger.LogInformation($"活动已{action}: {id}, 操作人: {GetCurrentUserName()}");
                    return Ok(new { success = true, message = $"{action}成功" });
                }
                return Ok(new { success = false, message = "操作失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"切换活动状态失败: {id}");
                return Ok(new { success = false, message = "操作失败" });
            }
        }

        #endregion

        #region 活动运行控制

        /// <summary>
        /// 手动开始活动
        /// </summary>
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartActivity(int id)
        {
            try
            {
                var result = await _activityService.StartActivityAsync(id);
                if (result)
                {
                    _logger.LogInformation($"活动手动启动: {id}, 操作人: {GetCurrentUserName()}");
                    return Ok(new { success = true, message = "活动已启动" });
                }
                return Ok(new { success = false, message = "启动失败，活动可能已在运行中" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"启动活动失败: {id}");
                return Ok(new { success = false, message = "启动活动失败" });
            }
        }

        /// <summary>
        /// 手动停止活动
        /// </summary>
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopActivity(int id)
        {
            try
            {
                var result = await _activityService.StopActivityAsync(id);
                if (result)
                {
                    _logger.LogInformation($"活动手动停止: {id}, 操作人: {GetCurrentUserName()}");
                    return Ok(new { success = true, message = "活动已停止" });
                }
                return Ok(new { success = false, message = "停止失败，活动可能未在运行" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"停止活动失败: {id}");
                return Ok(new { success = false, message = "停止活动失败" });
            }
        }

        /// <summary>
        /// 获取运行中的活动
        /// </summary>
        [HttpGet("running")]
        public IActionResult GetRunningActivities()
        {
            try
            {
                var activities = _activityService.GetRunningActivities();
                return Ok(new { success = true, data = activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取运行中活动失败");
                return Ok(new { success = false, message = "获取失败" });
            }
        }

        /// <summary>
        /// 获取活动状态概览
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetActivityStatus()
        {
            try
            {
                var status = await _activityService.GetActivityStatusAsync();
                status.TodayStats.TotalParticipants = 0; // 需要从游戏服务器获取

                return Ok(new
                {
                    success = true,
                    data = status,
                    currentExpRate = _activityService.GetCurrentExpRate(),
                    currentDropRate = _activityService.GetCurrentDropRate()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活动状态失败");
                return Ok(new { success = false, message = "获取状态失败" });
            }
        }

        #endregion

        #region 活动日志

        /// <summary>
        /// 获取活动日志
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetActivityLogs([FromQuery] int? activityId, [FromQuery] int limit = 100)
        {
            try
            {
                var logs = await _activityService.GetActivityLogsAsync(activityId, limit);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取活动日志失败");
                return Ok(new { success = false, message = "获取日志失败" });
            }
        }

        #endregion

        #region 活动奖励管理

        /// <summary>
        /// 获取活动奖励
        /// </summary>
        [HttpGet("{id}/rewards")]
        public async Task<IActionResult> GetActivityRewards(int id)
        {
            try
            {
                var rewards = await _activityService.GetActivityRewardsAsync(id);
                return Ok(new { success = true, data = rewards });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取活动奖励失败: {id}");
                return Ok(new { success = false, message = "获取奖励失败" });
            }
        }

        /// <summary>
        /// 保存活动奖励
        /// </summary>
        [HttpPost("reward")]
        public async Task<IActionResult> SaveActivityReward([FromBody] ActivityReward reward)
        {
            try
            {
                var result = await _activityService.SaveActivityRewardAsync(reward);
                if (result)
                    return Ok(new { success = true, message = "保存成功" });
                return Ok(new { success = false, message = "保存失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存活动奖励失败");
                return Ok(new { success = false, message = "保存奖励失败" });
            }
        }

        /// <summary>
        /// 删除活动奖励
        /// </summary>
        [HttpDelete("reward/{id}")]
        public async Task<IActionResult> DeleteActivityReward(int id)
        {
            try
            {
                var result = await _activityService.DeleteActivityRewardAsync(id);
                if (result)
                    return Ok(new { success = true, message = "删除成功" });
                return Ok(new { success = false, message = "删除失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除活动奖励失败: {id}");
                return Ok(new { success = false, message = "删除奖励失败" });
            }
        }

        #endregion

        #region 活动类型

        /// <summary>
        /// 获取活动类型列表
        /// </summary>
        [HttpGet("types")]
        public IActionResult GetActivityTypes()
        {
            var types = new[]
            {
                new { value = "exp_boost", label = "经验加成", icon = "⭐", color = "#00FF00" },
                new { value = "drop_boost", label = "掉落加成", icon = "💎", color = "#00BFFF" },
                new { value = "boss_spawn", label = "BOSS刷新", icon = "👹", color = "#FF4500" },
                new { value = "gift_rain", label = "天降豪礼", icon = "🎁", color = "#FF69B4" },
                new { value = "merchant", label = "特殊商人", icon = "🛒", color = "#8B4513" },
                new { value = "party", label = "派对活动", icon = "🎉", color = "#FF1493" },
                new { value = "online_reward", label = "在线奖励", icon = "⏰", color = "#FFD700" },
                new { value = "first_kill", label = "首杀奖励", icon = "🏆", color = "#9400D3" },
                new { value = "custom_script", label = "自定义脚本", icon = "📜", color = "#808080" }
            };

            return Ok(new { success = true, data = types });
        }

        /// <summary>
        /// 获取调度类型列表
        /// </summary>
        [HttpGet("schedule-types")]
        public IActionResult GetScheduleTypes()
        {
            var types = new[]
            {
                new { value = "once", label = "一次性", description = "只运行一次" },
                new { value = "daily", label = "每日", description = "每天固定时间运行" },
                new { value = "weekly", label = "每周", description = "每周指定日期运行" },
                new { value = "interval", label = "间隔", description = "按间隔时间循环运行" },
                new { value = "cron", label = "Cron", description = "使用Cron表达式定义" }
            };

            return Ok(new { success = true, data = types });
        }

        #endregion

        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "admin";
        }
    }
}
