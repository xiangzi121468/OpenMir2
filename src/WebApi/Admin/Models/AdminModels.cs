using System;
using System.Collections.Generic;

namespace WebApi.Admin.Models
{
    /// <summary>
    /// 管理员
    /// </summary>
    public class AdminUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string Role { get; set; }
        public string Permissions { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public string LastLoginIp { get; set; }
        public int LoginCount { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 管理员角色
    /// </summary>
    public static class AdminRoles
    {
        public const string SuperAdmin = "superadmin";
        public const string Admin = "admin";
        public const string GM = "gm";
        public const string Viewer = "viewer";
    }

    /// <summary>
    /// 操作日志
    /// </summary>
    public class AdminLog
    {
        public long Id { get; set; }
        public int AdminId { get; set; }
        public string AdminName { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string Target { get; set; }
        public string Content { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public int Result { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 系统公告
    /// </summary>
    public class SystemNotice
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public string Target { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentTime { get; set; }
        public int? AdminId { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 封禁记录
    /// </summary>
    public class BanRecord
    {
        public long Id { get; set; }
        public string BanType { get; set; }
        public string BanValue { get; set; }
        public string CharName { get; set; }
        public string Reason { get; set; }
        public int Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int AdminId { get; set; }
        public string AdminName { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 邮件记录
    /// </summary>
    public class MailRecord
    {
        public long Id { get; set; }
        public string MailType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Attachments { get; set; }
        public int GameGold { get; set; }
        public string Recipients { get; set; }
        public int RecipientCount { get; set; }
        public int SentCount { get; set; }
        public int Status { get; set; }
        public int AdminId { get; set; }
        public string AdminName { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 统计快照
    /// </summary>
    public class StatsSnapshot
    {
        public long Id { get; set; }
        public DateTime SnapshotDate { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalCharacters { get; set; }
        public int NewAccounts { get; set; }
        public int NewCharacters { get; set; }
        public int ActiveAccounts { get; set; }
        public int OnlinePeak { get; set; }
        public decimal TotalRecharge { get; set; }
        public int RechargeCount { get; set; }
        public int ShopSales { get; set; }
        public int ShopOrders { get; set; }
    }

    #region 请求/响应模型

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Captcha { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public AdminUserInfo UserInfo { get; set; }
    }

    public class AdminUserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }
        public string Role { get; set; }
        public List<string> Permissions { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class CreateAdminRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public string Role { get; set; }
        public List<string> Permissions { get; set; }
    }

    public class BanPlayerRequest
    {
        public string BanType { get; set; }
        public string BanValue { get; set; }
        public string CharName { get; set; }
        public string Reason { get; set; }
        public int Duration { get; set; }
    }

    public class SendMailRequest
    {
        public string MailType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<MailAttachment> Attachments { get; set; }
        public int GameGold { get; set; }
        public List<string> Recipients { get; set; }
    }

    public class MailAttachment
    {
        public string ItemName { get; set; }
        public int Count { get; set; }
    }

    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Keyword { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class PagedResponse<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; }
    }

    public class ApiResult
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }

        public static ApiResult Success(object data = null, string msg = "success")
            => new ApiResult { Code = 0, Data = data, Msg = msg };

        public static ApiResult Fail(string msg, int code = -1)
            => new ApiResult { Code = code, Msg = msg };
    }

    #endregion
}
