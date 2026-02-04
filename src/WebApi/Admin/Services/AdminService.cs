using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using NLog;
using WebApi.Admin.Models;

namespace WebApi.Admin.Services
{
    /// <summary>
    /// 管理员服务
    /// </summary>
    public class AdminService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;
        private const string JwtSecret = "OpenMir2_Admin_Secret_Key_2024_Very_Long_String";
        private const int TokenExpireHours = 24;

        public AdminService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 认证

        /// <summary>
        /// 登录
        /// </summary>
        public async Task<(bool success, string message, LoginResponse response)> LoginAsync(string username, string password, string ip)
        {
            try
            {
                var admin = await GetAdminByUsernameAsync(username);
                if (admin == null)
                    return (false, "用户不存在", null);

                if (admin.Status != 1)
                    return (false, "账号已被禁用", null);

                // 验证密码
                string hashedPassword = HashPassword(password, admin.Salt);
                if (admin.Password != hashedPassword)
                    return (false, "密码错误", null);

                // 更新登录信息
                await UpdateLoginInfoAsync(admin.Id, ip);

                // 生成Token
                string token = GenerateJwtToken(admin);

                // 记录日志
                await LogActionAsync(admin.Id, admin.Username, "auth", "login", username, $"登录成功", ip);

                var response = new LoginResponse
                {
                    Token = token,
                    UserInfo = new AdminUserInfo
                    {
                        Id = admin.Id,
                        Username = admin.Username,
                        Nickname = admin.Nickname ?? admin.Username,
                        Avatar = admin.Avatar,
                        Role = admin.Role,
                        Permissions = ParsePermissions(admin.Permissions)
                    }
                };

                return (true, "登录成功", response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "登录失败");
                return (false, "登录失败", null);
            }
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(JwtSecret);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 生成JWT Token
        /// </summary>
        private string GenerateJwtToken(AdminUser admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                    new Claim(ClaimTypes.Name, admin.Username),
                    new Claim(ClaimTypes.Role, admin.Role),
                    new Claim("nickname", admin.Nickname ?? admin.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(TokenExpireHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task<(bool success, string message)> ChangePasswordAsync(int adminId, string oldPassword, string newPassword)
        {
            var admin = await GetAdminByIdAsync(adminId);
            if (admin == null)
                return (false, "用户不存在");

            string hashedOld = HashPassword(oldPassword, admin.Salt);
            if (admin.Password != hashedOld)
                return (false, "原密码错误");

            string newSalt = GenerateSalt();
            string hashedNew = HashPassword(newPassword, newSalt);

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE admin_users SET Password=@Password, Salt=@Salt WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", adminId);
            cmd.Parameters.AddWithValue("@Password", hashedNew);
            cmd.Parameters.AddWithValue("@Salt", newSalt);
            await cmd.ExecuteNonQueryAsync();

            return (true, "密码修改成功");
        }

        #endregion

        #region 管理员管理

        public async Task<AdminUser> GetAdminByIdAsync(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM admin_users WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadAdmin(reader);
            }
            return null;
        }

        public async Task<AdminUser> GetAdminByUsernameAsync(string username)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM admin_users WHERE Username=@Username", conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadAdmin(reader);
            }
            return null;
        }

        public async Task<List<AdminUser>> GetAdminListAsync(int page, int pageSize)
        {
            var list = new List<AdminUser>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            int offset = (page - 1) * pageSize;
            using var cmd = new MySqlCommand(
                $"SELECT * FROM admin_users ORDER BY Id LIMIT {offset},{pageSize}", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var admin = ReadAdmin(reader);
                admin.Password = null; // 不返回密码
                admin.Salt = null;
                list.Add(admin);
            }
            return list;
        }

        public async Task<bool> CreateAdminAsync(CreateAdminRequest request, int creatorId)
        {
            string salt = GenerateSalt();
            string hashedPassword = HashPassword(request.Password, salt);
            string permissions = request.Permissions != null ? 
                System.Text.Json.JsonSerializer.Serialize(request.Permissions) : null;

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                INSERT INTO admin_users (Username, Password, Salt, Nickname, Role, Permissions, Status, CreateTime)
                VALUES (@Username, @Password, @Salt, @Nickname, @Role, @Permissions, 1, NOW())", conn);

            cmd.Parameters.AddWithValue("@Username", request.Username);
            cmd.Parameters.AddWithValue("@Password", hashedPassword);
            cmd.Parameters.AddWithValue("@Salt", salt);
            cmd.Parameters.AddWithValue("@Nickname", request.Nickname ?? request.Username);
            cmd.Parameters.AddWithValue("@Role", request.Role);
            cmd.Parameters.AddWithValue("@Permissions", permissions ?? (object)DBNull.Value);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return false; // 用户名重复
            }
        }

        public async Task<bool> UpdateAdminStatusAsync(int adminId, int status)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE admin_users SET Status=@Status WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", adminId);
            cmd.Parameters.AddWithValue("@Status", status);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private async Task UpdateLoginInfoAsync(int adminId, string ip)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                UPDATE admin_users SET LastLoginTime=NOW(), LastLoginIp=@Ip, LoginCount=LoginCount+1 
                WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", adminId);
            cmd.Parameters.AddWithValue("@Ip", ip);
            await cmd.ExecuteNonQueryAsync();
        }

        private AdminUser ReadAdmin(MySqlDataReader reader)
        {
            return new AdminUser
            {
                Id = reader.GetInt32("Id"),
                Username = reader.GetString("Username"),
                Password = reader.GetString("Password"),
                Salt = reader.GetString("Salt"),
                Nickname = reader.IsDBNull(reader.GetOrdinal("Nickname")) ? null : reader.GetString("Nickname"),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                Role = reader.GetString("Role"),
                Permissions = reader.IsDBNull(reader.GetOrdinal("Permissions")) ? null : reader.GetString("Permissions"),
                LastLoginTime = reader.IsDBNull(reader.GetOrdinal("LastLoginTime")) ? null : reader.GetDateTime("LastLoginTime"),
                LastLoginIp = reader.IsDBNull(reader.GetOrdinal("LastLoginIp")) ? null : reader.GetString("LastLoginIp"),
                LoginCount = reader.GetInt32("LoginCount"),
                Status = reader.GetInt32("Status"),
                CreateTime = reader.GetDateTime("CreateTime")
            };
        }

        #endregion

        #region 操作日志

        public async Task LogActionAsync(int adminId, string adminName, string module, string action, 
            string target, string content, string ip, string userAgent = null, int result = 1)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO admin_logs (AdminId, AdminName, Module, Action, Target, Content, IpAddress, UserAgent, Result, CreateTime)
                    VALUES (@AdminId, @AdminName, @Module, @Action, @Target, @Content, @IpAddress, @UserAgent, @Result, NOW())", conn);

                cmd.Parameters.AddWithValue("@AdminId", adminId);
                cmd.Parameters.AddWithValue("@AdminName", adminName);
                cmd.Parameters.AddWithValue("@Module", module);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Target", target ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Content", content ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IpAddress", ip ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UserAgent", userAgent ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Result", result);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "记录操作日志失败");
            }
        }

        public async Task<PagedResponse<AdminLog>> GetLogsAsync(PagedRequest request)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var where = new StringBuilder("WHERE 1=1");
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                where.Append(" AND (AdminName LIKE @Keyword OR Module LIKE @Keyword OR Action LIKE @Keyword)");
            }
            if (request.StartTime.HasValue)
            {
                where.Append(" AND CreateTime >= @StartTime");
            }
            if (request.EndTime.HasValue)
            {
                where.Append(" AND CreateTime <= @EndTime");
            }

            // 查询总数
            using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM admin_logs {where}", conn);
            if (!string.IsNullOrEmpty(request.Keyword))
                countCmd.Parameters.AddWithValue("@Keyword", $"%{request.Keyword}%");
            if (request.StartTime.HasValue)
                countCmd.Parameters.AddWithValue("@StartTime", request.StartTime.Value);
            if (request.EndTime.HasValue)
                countCmd.Parameters.AddWithValue("@EndTime", request.EndTime.Value);

            int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            // 查询列表
            int offset = (request.Page - 1) * request.PageSize;
            using var cmd = new MySqlCommand(
                $"SELECT * FROM admin_logs {where} ORDER BY Id DESC LIMIT {offset},{request.PageSize}", conn);
            if (!string.IsNullOrEmpty(request.Keyword))
                cmd.Parameters.AddWithValue("@Keyword", $"%{request.Keyword}%");
            if (request.StartTime.HasValue)
                cmd.Parameters.AddWithValue("@StartTime", request.StartTime.Value);
            if (request.EndTime.HasValue)
                cmd.Parameters.AddWithValue("@EndTime", request.EndTime.Value);

            var list = new List<AdminLog>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new AdminLog
                {
                    Id = reader.GetInt64("Id"),
                    AdminId = reader.GetInt32("AdminId"),
                    AdminName = reader.GetString("AdminName"),
                    Module = reader.GetString("Module"),
                    Action = reader.GetString("Action"),
                    Target = reader.IsDBNull(reader.GetOrdinal("Target")) ? null : reader.GetString("Target"),
                    Content = reader.IsDBNull(reader.GetOrdinal("Content")) ? null : reader.GetString("Content"),
                    IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString("IpAddress"),
                    Result = reader.GetInt32("Result"),
                    CreateTime = reader.GetDateTime("CreateTime")
                });
            }

            return new PagedResponse<AdminLog>
            {
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = list
            };
        }

        #endregion

        #region 工具方法

        private string HashPassword(string password, string salt)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private List<string> ParsePermissions(string permissionsJson)
        {
            if (string.IsNullOrEmpty(permissionsJson))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(permissionsJson);
            }
            catch
            {
                return new List<string>();
            }
        }

        #endregion
    }
}
