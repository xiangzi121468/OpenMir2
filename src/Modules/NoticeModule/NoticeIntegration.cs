using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using NoticeModule.Models;

namespace NoticeModule
{
    /// <summary>
    /// 通知系统集成帮助类
    /// 用于与游戏服务器集成
    /// </summary>
    public static class NoticeIntegration
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static INoticeService _noticeService;
        private static NoticeScheduler _scheduler;
        private static bool _initialized = false;

        /// <summary>
        /// 初始化通知系统
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="onBroadcast">广播回调(content, color, priority)</param>
        /// <param name="onMapNotice">地图公告回调(map, content, color)</param>
        public static async Task InitializeAsync(
            string connectionString,
            Action<string, string, int> onBroadcast,
            Action<string, string, string> onMapNotice = null)
        {
            if (_initialized)
                return;

            try
            {
                _noticeService = new NoticeService(connectionString);
                _scheduler = new NoticeScheduler(_noticeService);
                _scheduler.OnSendBroadcast = onBroadcast;
                _scheduler.OnSendMapNotice = onMapNotice;

                await _scheduler.InitializeAsync();
                _initialized = true;

                Logger.Info("通知系统初始化成功");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "通知系统初始化失败");
            }
        }

        /// <summary>
        /// 定时处理（建议每秒调用一次）
        /// </summary>
        public static async Task ProcessAsync()
        {
            if (!_initialized || _scheduler == null)
                return;

            await _scheduler.ProcessAsync();
        }

        /// <summary>
        /// 获取登录公告列表
        /// </summary>
        public static List<string> GetLoginNotices()
        {
            if (!_initialized || _noticeService == null)
                return new List<string>();

            return _noticeService.GetCachedLoginNotices();
        }

        /// <summary>
        /// 发送登录公告给玩家
        /// </summary>
        /// <param name="sendMessage">发送消息的委托(message)</param>
        public static void SendLoginNoticesToPlayer(Action<string> sendMessage)
        {
            if (sendMessage == null)
                return;

            var notices = GetLoginNotices();
            foreach (var notice in notices)
            {
                sendMessage(notice);
            }
        }

        /// <summary>
        /// 手动发送全服广播
        /// </summary>
        public static async Task SendBroadcastAsync(string content, string color = "yellow", int priority = 0)
        {
            if (!_initialized || _scheduler == null)
                return;

            await _scheduler.SendBroadcastNowAsync(content, color, priority);
        }

        /// <summary>
        /// 刷新公告缓存
        /// </summary>
        public static async Task RefreshCacheAsync()
        {
            if (!_initialized || _scheduler == null)
                return;

            await _scheduler.RefreshCacheAsync();
        }

        /// <summary>
        /// 设置滚动公告间隔
        /// </summary>
        public static void SetScrollInterval(int seconds)
        {
            _scheduler?.SetScrollInterval(seconds);
        }

        /// <summary>
        /// 获取通知服务实例
        /// </summary>
        public static INoticeService GetService() => _noticeService;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized => _initialized;
    }
}
