using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoticeModule.Models;

namespace NoticeModule
{
    /// <summary>
    /// 通知服务接口
    /// </summary>
    public interface INoticeService
    {
        #region 公告查询

        /// <summary>
        /// 获取所有启用的公告
        /// </summary>
        Task<List<GameNotice>> GetEnabledNoticesAsync();

        /// <summary>
        /// 获取指定类型的公告
        /// </summary>
        Task<List<GameNotice>> GetNoticesByTypeAsync(string noticeType);

        /// <summary>
        /// 获取登录公告
        /// </summary>
        Task<List<GameNotice>> GetLoginNoticesAsync();

        /// <summary>
        /// 获取滚动公告
        /// </summary>
        Task<List<GameNotice>> GetScrollNoticesAsync();

        /// <summary>
        /// 获取定时公告
        /// </summary>
        Task<List<GameNotice>> GetScheduleNoticesAsync();

        /// <summary>
        /// 获取公告详情
        /// </summary>
        Task<GameNotice> GetNoticeByIdAsync(int id);

        #endregion

        #region 公告管理

        /// <summary>
        /// 添加公告
        /// </summary>
        Task<int> AddNoticeAsync(GameNotice notice);

        /// <summary>
        /// 更新公告
        /// </summary>
        Task<bool> UpdateNoticeAsync(GameNotice notice);

        /// <summary>
        /// 删除公告
        /// </summary>
        Task<bool> DeleteNoticeAsync(int id);

        /// <summary>
        /// 启用/禁用公告
        /// </summary>
        Task<bool> SetNoticeEnabledAsync(int id, bool enabled);

        #endregion

        #region 公告发送

        /// <summary>
        /// 发送全服广播
        /// </summary>
        Task<int> SendBroadcastAsync(string content, string color = "yellow", int priority = 0);

        /// <summary>
        /// 发送地图公告
        /// </summary>
        Task<int> SendMapNoticeAsync(string mapName, string content, string color = "yellow");

        /// <summary>
        /// 记录发送日志
        /// </summary>
        Task LogSendAsync(int noticeId, string noticeType, int targetCount, string content);

        /// <summary>
        /// 更新公告发送状态
        /// </summary>
        Task UpdateNoticeSentAsync(int noticeId);

        #endregion

        #region 缓存管理

        /// <summary>
        /// 重新加载公告缓存
        /// </summary>
        Task ReloadCacheAsync();

        /// <summary>
        /// 获取缓存的登录公告内容
        /// </summary>
        List<string> GetCachedLoginNotices();

        /// <summary>
        /// 获取下一条滚动公告
        /// </summary>
        GameNotice GetNextScrollNotice();

        #endregion
    }
}
