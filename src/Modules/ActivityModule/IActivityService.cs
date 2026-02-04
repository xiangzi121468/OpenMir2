using ActivityModule.Models;

namespace ActivityModule
{
    /// <summary>
    /// 活动服务接口
    /// </summary>
    public interface IActivityService
    {
        #region 活动配置管理

        /// <summary>
        /// 获取所有活动
        /// </summary>
        Task<List<GameActivity>> GetAllActivitiesAsync();

        /// <summary>
        /// 分页查询活动
        /// </summary>
        Task<(List<GameActivity> Items, int Total)> QueryActivitiesAsync(ActivityQueryRequest request);

        /// <summary>
        /// 获取单个活动
        /// </summary>
        Task<GameActivity?> GetActivityByIdAsync(int id);

        /// <summary>
        /// 保存活动(创建或更新)
        /// </summary>
        Task<bool> SaveActivityAsync(ActivitySaveRequest request, string? operatorName = null);

        /// <summary>
        /// 删除活动
        /// </summary>
        Task<bool> DeleteActivityAsync(int id);

        /// <summary>
        /// 启用/禁用活动
        /// </summary>
        Task<bool> SetActivityEnabledAsync(int id, bool enabled);

        #endregion

        #region 活动运行控制

        /// <summary>
        /// 手动开始活动
        /// </summary>
        Task<bool> StartActivityAsync(int id);

        /// <summary>
        /// 手动停止活动
        /// </summary>
        Task<bool> StopActivityAsync(int id);

        /// <summary>
        /// 获取当前运行中的活动
        /// </summary>
        List<ActivityRuntime> GetRunningActivities();

        /// <summary>
        /// 检查活动是否正在运行
        /// </summary>
        bool IsActivityRunning(int activityId);

        /// <summary>
        /// 获取活动状态概览
        /// </summary>
        Task<ActivityStatusResponse> GetActivityStatusAsync();

        #endregion

        #region 活动日志

        /// <summary>
        /// 记录活动日志
        /// </summary>
        Task LogActivityAsync(int activityId, string action, string status, string? message = null, int participants = 0, int rewards = 0);

        /// <summary>
        /// 查询活动日志
        /// </summary>
        Task<List<ActivityLog>> GetActivityLogsAsync(int? activityId = null, int limit = 100);

        #endregion

        #region 活动效果

        /// <summary>
        /// 获取当前经验倍率
        /// </summary>
        double GetCurrentExpRate();

        /// <summary>
        /// 获取当前掉落倍率
        /// </summary>
        double GetCurrentDropRate();

        /// <summary>
        /// 检查玩家是否可参与活动
        /// </summary>
        bool CanPlayerJoin(int activityId, int playerLevel, string? mapId, bool isVip);

        #endregion

        #region 活动奖励

        /// <summary>
        /// 获取活动奖励配置
        /// </summary>
        Task<List<ActivityReward>> GetActivityRewardsAsync(int activityId);

        /// <summary>
        /// 保存活动奖励
        /// </summary>
        Task<bool> SaveActivityRewardAsync(ActivityReward reward);

        /// <summary>
        /// 删除活动奖励
        /// </summary>
        Task<bool> DeleteActivityRewardAsync(int rewardId);

        #endregion
    }
}
