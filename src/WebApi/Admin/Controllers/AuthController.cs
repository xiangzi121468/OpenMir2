using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Admin.Models;
using WebApi.Admin.Services;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 管理员认证
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    public class AuthController : AdminBaseController
    {
        private readonly AdminService _adminService;

        public AuthController(AdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// 登录
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request?.Username) || string.IsNullOrEmpty(request.Password))
                return Ok(ApiResult.Fail("用户名和密码不能为空"));

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (success, message, response) = await _adminService.LoginAsync(request.Username, request.Password, ip);

            if (!success)
                return Ok(ApiResult.Fail(message));

            return Ok(ApiResult.Success(response));
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        [HttpGet("info")]
        public async Task<IActionResult> GetInfo()
        {
            var admin = await _adminService.GetAdminByIdAsync(CurrentAdminId);
            if (admin == null)
                return Ok(ApiResult.Fail("用户不存在"));

            var info = new AdminUserInfo
            {
                Id = admin.Id,
                Username = admin.Username,
                Nickname = admin.Nickname ?? admin.Username,
                Avatar = admin.Avatar,
                Role = admin.Role
            };

            return Ok(ApiResult.Success(info));
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [HttpPost("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request?.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
                return Ok(ApiResult.Fail("参数错误"));

            if (request.NewPassword.Length < 6)
                return Ok(ApiResult.Fail("新密码长度不能少于6位"));

            var (success, message) = await _adminService.ChangePasswordAsync(CurrentAdminId, request.OldPassword, request.NewPassword);

            if (success)
            {
                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "auth", "change_password",
                    null, "修改密码", ClientIp);
            }

            return Ok(success ? ApiResult.Success(null, message) : ApiResult.Fail(message));
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "auth", "logout",
                null, "退出登录", ClientIp);
            return Ok(ApiResult.Success());
        }

        /// <summary>
        /// 获取操作日志
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] PagedRequest request)
        {
            request ??= new PagedRequest();
            var result = await _adminService.GetLogsAsync(request);
            return Ok(ApiResult.Success(result));
        }
    }
}
