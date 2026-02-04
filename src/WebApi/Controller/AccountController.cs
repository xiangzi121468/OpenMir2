using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using NLog;

namespace WebApi.Controller
{
    /// <summary>
    /// 认证授权API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;
        private readonly string _jwtSecret;

        public AuthenticationController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
            _jwtSecret = configuration["Jwt:Secret"] ?? "OpenMir2_Default_Secret_Key_2024";
        }

        /// <summary>
        /// 登录获取Token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.UserName) || string.IsNullOrEmpty(request?.Password))
                {
                    return Ok(new { code = -1, msg = "用户名和密码不能为空" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 查询账号
                using var cmd = new MySqlCommand(
                    "SELECT Id, UserName, Password, Quiz1, Answer1 FROM account WHERE UserName = @UserName", conn);
                cmd.Parameters.AddWithValue("@UserName", request.UserName);

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return Ok(new { code = -1, msg = "账号不存在" });
                }

                string storedPassword = reader.GetString("Password");
                int accountId = reader.GetInt32("Id");

                // 验证密码 (MD5)
                string inputPasswordHash = ComputeMd5(request.Password);
                if (!string.Equals(storedPassword, inputPasswordHash, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(storedPassword, request.Password, StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { code = -1, msg = "密码错误" });
                }

                // 生成JWT Token
                var token = GenerateJwtToken(accountId, request.UserName);

                Logger.Info($"用户登录成功: {request.UserName}");
                return Ok(new
                {
                    code = 0,
                    msg = "登录成功",
                    data = new
                    {
                        token,
                        accountId,
                        userName = request.UserName,
                        expiresIn = 7200 // 2小时
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "登录失败");
                return Ok(new { code = -1, msg = "登录失败" });
            }
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.UserName) || string.IsNullOrEmpty(request?.Password))
                {
                    return Ok(new { code = -1, msg = "用户名和密码不能为空" });
                }

                if (request.UserName.Length < 4 || request.UserName.Length > 20)
                {
                    return Ok(new { code = -1, msg = "用户名长度需要4-20个字符" });
                }

                if (request.Password.Length < 6)
                {
                    return Ok(new { code = -1, msg = "密码长度不能少于6位" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 检查用户名是否存在
                using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM account WHERE UserName = @UserName", conn);
                checkCmd.Parameters.AddWithValue("@UserName", request.UserName);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                if (count > 0)
                {
                    return Ok(new { code = -1, msg = "用户名已存在" });
                }

                // 创建账号
                string passwordHash = ComputeMd5(request.Password);
                using var insertCmd = new MySqlCommand(@"
                    INSERT INTO account (UserName, Password, Quiz1, Answer1, Quiz2, Answer2, CreateDate, UpdateDate) 
                    VALUES (@UserName, @Password, @Quiz1, @Answer1, '', '', NOW(), NOW())", conn);
                insertCmd.Parameters.AddWithValue("@UserName", request.UserName);
                insertCmd.Parameters.AddWithValue("@Password", passwordHash);
                insertCmd.Parameters.AddWithValue("@Quiz1", request.Quiz ?? "");
                insertCmd.Parameters.AddWithValue("@Answer1", request.Answer ?? "");

                await insertCmd.ExecuteNonQueryAsync();

                Logger.Info($"新用户注册: {request.UserName}");
                return Ok(new { code = 0, msg = "注册成功" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "注册失败");
                return Ok(new { code = -1, msg = "注册失败" });
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.UserName) ||
                    string.IsNullOrEmpty(request?.OldPassword) ||
                    string.IsNullOrEmpty(request?.NewPassword))
                {
                    return Ok(new { code = -1, msg = "参数不完整" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 验证旧密码
                using var checkCmd = new MySqlCommand(
                    "SELECT Password FROM account WHERE UserName = @UserName", conn);
                checkCmd.Parameters.AddWithValue("@UserName", request.UserName);

                var storedPassword = await checkCmd.ExecuteScalarAsync();
                if (storedPassword == null)
                {
                    return Ok(new { code = -1, msg = "账号不存在" });
                }

                string oldPasswordHash = ComputeMd5(request.OldPassword);
                if (!string.Equals(storedPassword.ToString(), oldPasswordHash, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(storedPassword.ToString(), request.OldPassword, StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { code = -1, msg = "原密码错误" });
                }

                // 更新密码
                string newPasswordHash = ComputeMd5(request.NewPassword);
                using var updateCmd = new MySqlCommand(
                    "UPDATE account SET Password = @Password, UpdateDate = NOW() WHERE UserName = @UserName", conn);
                updateCmd.Parameters.AddWithValue("@Password", newPasswordHash);
                updateCmd.Parameters.AddWithValue("@UserName", request.UserName);
                await updateCmd.ExecuteNonQueryAsync();

                Logger.Info($"用户修改密码: {request.UserName}");
                return Ok(new { code = 0, msg = "密码修改成功" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "修改密码失败");
                return Ok(new { code = -1, msg = "修改密码失败" });
            }
        }

        /// <summary>
        /// 自助解封
        /// </summary>
        [HttpPost("unblock")]
        public async Task<IActionResult> Unblock([FromBody] UnblockRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.UserName) || string.IsNullOrEmpty(request?.Answer))
                {
                    return Ok(new { code = -1, msg = "参数不完整" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 验证密保问题
                using var checkCmd = new MySqlCommand(
                    "SELECT Answer1 FROM account WHERE UserName = @UserName", conn);
                checkCmd.Parameters.AddWithValue("@UserName", request.UserName);

                var storedAnswer = await checkCmd.ExecuteScalarAsync();
                if (storedAnswer == null)
                {
                    return Ok(new { code = -1, msg = "账号不存在" });
                }

                if (!string.Equals(storedAnswer.ToString(), request.Answer, StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { code = -1, msg = "密保答案错误" });
                }

                // 解除封禁 - 将IsActive设为0而非删除记录
                using var updateCmd = new MySqlCommand(@"
                    UPDATE ban_records SET IsActive = 0 
                    WHERE BanType = 'account' AND BanValue = @UserName 
                    AND IsActive = 1 AND (IsPermanent = 0 OR EndTime IS NULL OR EndTime > NOW())", conn);
                updateCmd.Parameters.AddWithValue("@UserName", request.UserName);
                int affected = await updateCmd.ExecuteNonQueryAsync();

                if (affected > 0)
                {
                    Logger.Info($"用户自助解封: {request.UserName}");
                    return Ok(new { code = 0, msg = "解封成功" });
                }
                else
                {
                    return Ok(new { code = 0, msg = "账号未被封禁" });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "自助解封失败");
                return Ok(new { code = -1, msg = "解封失败" });
            }
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        [HttpGet("characters")]
        public async Task<IActionResult> GetCharacters([FromQuery] string accountId)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                {
                    return Ok(new { code = -1, msg = "参数错误" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    SELECT Id, ChrName, Job, Sex, Level, Gold, MapName 
                    FROM characters WHERE LoginID = @AccountId AND Deleted = 0", conn);
                cmd.Parameters.AddWithValue("@AccountId", accountId);

                using var reader = await cmd.ExecuteReaderAsync();
                var characters = new System.Collections.Generic.List<object>();
                while (await reader.ReadAsync())
                {
                    characters.Add(new
                    {
                        id = reader.GetInt32("Id"),
                        name = reader.GetString("ChrName"),
                        job = reader.GetInt32("Job"),
                        sex = reader.GetInt32("Sex"),
                        level = reader.GetInt32("Level"),
                        gold = reader.GetInt64("Gold"),
                        map = reader.GetString("MapName")
                    });
                }

                return Ok(new { code = 0, data = characters });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取角色列表失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        #region 辅助方法

        private string GenerateJwtToken(int accountId, string userName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim("type", "user")
            };

            var token = new JwtSecurityToken(
                issuer: "OpenMir2",
                audience: "OpenMir2Client",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string ComputeMd5(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        #endregion
    }

    #region 请求模型

    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Quiz { get; set; }
        public string Answer { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string UserName { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UnblockRequest
    {
        public string UserName { get; set; }
        public string Answer { get; set; }
    }

    #endregion
}