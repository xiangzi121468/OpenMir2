using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using NoticeModule.Models;

namespace NoticeModule
{
    /// <summary>
    /// 公告调度器
    /// </summary>
    public class NoticeScheduler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly INoticeService _noticeService;

        // 滚动公告计时
        private DateTime _lastScrollTime = DateTime.MinValue;
        private int _scrollIntervalSeconds = 60; // 默认60秒滚动一次

        // 定时公告检查
        private DateTime _lastScheduleCheck = DateTime.MinValue;

        // 发送回调
        public Action<string, string, int> OnSendBroadcast { get; set; }
        public Action<string, string, string> OnSendMapNotice { get; set; }

        public NoticeScheduler(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        /// <summary>
        /// 初始化调度器
        /// </summary>
        public async Task InitializeAsync()
        {
            await _noticeService.ReloadCacheAsync();
            Logger.Info("公告调度器已初始化");
        }

        /// <summary>
        /// 定时处理（每秒调用一次）
        /// </summary>
        public async Task ProcessAsync()
        {
            try
            {
                await ProcessScrollNoticesAsync();
                await ProcessScheduleNoticesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "公告调度处理异常");
            }
        }

        /// <summary>
        /// 处理滚动公告
        /// </summary>
        private async Task ProcessScrollNoticesAsync()
        {
            var notice = _noticeService.GetNextScrollNotice();
            if (notice == null)
                return;

            // 检查间隔
            var elapsed = (DateTime.Now - _lastScrollTime).TotalSeconds;
            if (elapsed < _scrollIntervalSeconds)
                return;

            // 检查公告自身的间隔
            if (notice.LastSentTime.HasValue && notice.RepeatInterval > 0)
            {
                var noticeElapsed = (DateTime.Now - notice.LastSentTime.Value).TotalSeconds;
                if (noticeElapsed < notice.RepeatInterval)
                    return;
            }

            // 发送
            var (fColor, bColor) = NoticeColors.GetMsgColors(notice.Color);
            OnSendBroadcast?.Invoke(notice.Content, notice.Color, notice.Priority);

            // 更新状态
            await _noticeService.UpdateNoticeSentAsync(notice.Id);
            await _noticeService.LogSendAsync(notice.Id, NoticeTypes.Scroll, 0, notice.Content);

            _lastScrollTime = DateTime.Now;
            Logger.Debug("滚动公告已发送: {0}", notice.Title);
        }

        /// <summary>
        /// 处理定时公告（基于简单Cron表达式）
        /// </summary>
        private async Task ProcessScheduleNoticesAsync()
        {
            // 每分钟检查一次
            if ((DateTime.Now - _lastScheduleCheck).TotalSeconds < 60)
                return;

            _lastScheduleCheck = DateTime.Now;

            var notices = await _noticeService.GetScheduleNoticesAsync();
            foreach (var notice in notices)
            {
                if (string.IsNullOrEmpty(notice.ScheduleCron))
                    continue;

                if (ShouldSendNow(notice.ScheduleCron, notice.LastSentTime))
                {
                    // 发送
                    OnSendBroadcast?.Invoke(notice.Content, notice.Color, notice.Priority);

                    // 更新状态
                    await _noticeService.UpdateNoticeSentAsync(notice.Id);
                    await _noticeService.LogSendAsync(notice.Id, NoticeTypes.Schedule, 0, notice.Content);

                    Logger.Info("定时公告已发送: {0} (Cron: {1})", notice.Title, notice.ScheduleCron);
                }
            }
        }

        /// <summary>
        /// 检查是否应该发送（简单Cron解析）
        /// 格式: 分 时 日 月 周 (如: 0 12 * * * 每天12点)
        /// </summary>
        private bool ShouldSendNow(string cron, DateTime? lastSent)
        {
            try
            {
                var parts = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                    return false;

                var now = DateTime.Now;

                // 如果今天已经发过，跳过
                if (lastSent.HasValue && lastSent.Value.Date == now.Date)
                    return false;

                // 分钟
                if (!MatchCronPart(parts[0], now.Minute))
                    return false;

                // 小时
                if (!MatchCronPart(parts[1], now.Hour))
                    return false;

                // 日
                if (!MatchCronPart(parts[2], now.Day))
                    return false;

                // 月
                if (!MatchCronPart(parts[3], now.Month))
                    return false;

                // 周 (0=周日, 1-6=周一到周六)
                int dayOfWeek = (int)now.DayOfWeek;
                if (!MatchCronPart(parts[4], dayOfWeek))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Cron表达式解析失败: {0}", cron);
                return false;
            }
        }

        /// <summary>
        /// 匹配Cron部分
        /// </summary>
        private bool MatchCronPart(string part, int value)
        {
            if (part == "*")
                return true;

            // 支持逗号分隔的多个值
            if (part.Contains(","))
            {
                var values = part.Split(',');
                foreach (var v in values)
                {
                    if (int.TryParse(v.Trim(), out int val) && val == value)
                        return true;
                }
                return false;
            }

            // 支持范围 (如: 9-17)
            if (part.Contains("-"))
            {
                var range = part.Split('-');
                if (range.Length == 2 &&
                    int.TryParse(range[0], out int start) &&
                    int.TryParse(range[1], out int end))
                {
                    return value >= start && value <= end;
                }
                return false;
            }

            // 支持间隔 (如: */5)
            if (part.StartsWith("*/"))
            {
                if (int.TryParse(part.Substring(2), out int interval) && interval > 0)
                {
                    return value % interval == 0;
                }
                return false;
            }

            // 精确匹配
            return int.TryParse(part, out int exact) && exact == value;
        }

        /// <summary>
        /// 设置滚动间隔
        /// </summary>
        public void SetScrollInterval(int seconds)
        {
            _scrollIntervalSeconds = Math.Max(10, seconds);
        }

        /// <summary>
        /// 手动发送广播
        /// </summary>
        public async Task SendBroadcastNowAsync(string content, string color = "yellow", int priority = 0)
        {
            OnSendBroadcast?.Invoke(content, color, priority);
            await _noticeService.LogSendAsync(0, NoticeTypes.Broadcast, 0, content);
        }

        /// <summary>
        /// 刷新公告缓存
        /// </summary>
        public async Task RefreshCacheAsync()
        {
            await _noticeService.ReloadCacheAsync();
        }
    }
}
