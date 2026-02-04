using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 管理后台控制器基类
    /// </summary>
    [Authorize]
    public abstract class AdminBaseController : ControllerBase
    {
        /// <summary>
        /// 当前管理员ID
        /// </summary>
        protected int CurrentAdminId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null ? int.Parse(claim.Value) : 0;
            }
        }

        /// <summary>
        /// 当前管理员用户名
        /// </summary>
        protected string CurrentAdminName
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.Name);
                return claim?.Value ?? "";
            }
        }

        /// <summary>
        /// 当前管理员角色
        /// </summary>
        protected string CurrentAdminRole
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.Role);
                return claim?.Value ?? "";
            }
        }

        /// <summary>
        /// 客户端IP
        /// </summary>
        protected string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        protected bool IsSuperAdmin => CurrentAdminRole == "superadmin";

        /// <summary>
        /// 是否管理员(含超管)
        /// </summary>
        protected bool IsAdmin => CurrentAdminRole == "superadmin" || CurrentAdminRole == "admin";
    }
}
