using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;
using ShopModule;
using WebApi.Admin.Models;
using WebApi.Admin.Services;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 商城管理
    /// </summary>
    [ApiController]
    [Route("api/admin/shop")]
    public class ShopManageController : AdminBaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AdminService _adminService;
        private readonly IShopService _shopService;
        private readonly string _connectionString;

        public ShopManageController(AdminService adminService, IShopService shopService, IConfiguration configuration)
        {
            _adminService = adminService;
            _shopService = shopService;
            _connectionString = configuration.GetConnectionString("Default") 
                ?? "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";
        }

        #region 商品管理

        /// <summary>
        /// 获取商品列表
        /// </summary>
        [HttpGet("items")]
        public async Task<IActionResult> GetItems([FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = "WHERE 1=1";
                if (categoryId.HasValue)
                    where += " AND CategoryId=@CategoryId";

                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM shop_items {where}", conn);
                if (categoryId.HasValue)
                    countCmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                int offset = (page - 1) * pageSize;
                using var cmd = new MySqlCommand($"SELECT * FROM shop_items {where} ORDER BY SortOrder, Id LIMIT {offset},{pageSize}", conn);
                if (categoryId.HasValue)
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(ReadShopItem(reader));
                }

                return Ok(ApiResult.Success(new { total, page, pageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取商品列表失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 添加商品
        /// </summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddShopItemRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO shop_items (CategoryId, ItemName, DisplayName, Description, Price, OriginalPrice, ItemCount, 
                        Stock, LimitPerUser, LimitPerDay, RequireLevel, RequireVip, IsHot, IsNew, IsRecommend, SortOrder, IsEnabled, CreateTime)
                    VALUES (@CategoryId, @ItemName, @DisplayName, @Description, @Price, @OriginalPrice, @ItemCount,
                        @Stock, @LimitPerUser, @LimitPerDay, @RequireLevel, @RequireVip, @IsHot, @IsNew, @IsRecommend, @SortOrder, @IsEnabled, NOW())", conn);

                cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);
                cmd.Parameters.AddWithValue("@ItemName", request.ItemName);
                cmd.Parameters.AddWithValue("@DisplayName", request.DisplayName);
                cmd.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", request.Price);
                cmd.Parameters.AddWithValue("@OriginalPrice", request.OriginalPrice);
                cmd.Parameters.AddWithValue("@ItemCount", request.ItemCount);
                cmd.Parameters.AddWithValue("@Stock", request.Stock);
                cmd.Parameters.AddWithValue("@LimitPerUser", request.LimitPerUser);
                cmd.Parameters.AddWithValue("@LimitPerDay", request.LimitPerDay);
                cmd.Parameters.AddWithValue("@RequireLevel", request.RequireLevel);
                cmd.Parameters.AddWithValue("@RequireVip", request.RequireVip);
                cmd.Parameters.AddWithValue("@IsHot", request.IsHot);
                cmd.Parameters.AddWithValue("@IsNew", request.IsNew);
                cmd.Parameters.AddWithValue("@IsRecommend", request.IsRecommend);
                cmd.Parameters.AddWithValue("@SortOrder", request.SortOrder);
                cmd.Parameters.AddWithValue("@IsEnabled", request.IsEnabled);

                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "shop", "add_item",
                    request.ItemName, $"添加商品: {request.DisplayName}, 价格:{request.Price}", ClientIp);

                return Ok(ApiResult.Success(null, "添加成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "添加商品失败");
                return Ok(ApiResult.Fail("添加失败"));
            }
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] AddShopItemRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE shop_items SET CategoryId=@CategoryId, ItemName=@ItemName, DisplayName=@DisplayName, 
                        Description=@Description, Price=@Price, OriginalPrice=@OriginalPrice, ItemCount=@ItemCount,
                        Stock=@Stock, LimitPerUser=@LimitPerUser, LimitPerDay=@LimitPerDay, RequireLevel=@RequireLevel,
                        RequireVip=@RequireVip, IsHot=@IsHot, IsNew=@IsNew, IsRecommend=@IsRecommend, 
                        SortOrder=@SortOrder, IsEnabled=@IsEnabled
                    WHERE Id=@Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);
                cmd.Parameters.AddWithValue("@ItemName", request.ItemName);
                cmd.Parameters.AddWithValue("@DisplayName", request.DisplayName);
                cmd.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", request.Price);
                cmd.Parameters.AddWithValue("@OriginalPrice", request.OriginalPrice);
                cmd.Parameters.AddWithValue("@ItemCount", request.ItemCount);
                cmd.Parameters.AddWithValue("@Stock", request.Stock);
                cmd.Parameters.AddWithValue("@LimitPerUser", request.LimitPerUser);
                cmd.Parameters.AddWithValue("@LimitPerDay", request.LimitPerDay);
                cmd.Parameters.AddWithValue("@RequireLevel", request.RequireLevel);
                cmd.Parameters.AddWithValue("@RequireVip", request.RequireVip);
                cmd.Parameters.AddWithValue("@IsHot", request.IsHot);
                cmd.Parameters.AddWithValue("@IsNew", request.IsNew);
                cmd.Parameters.AddWithValue("@IsRecommend", request.IsRecommend);
                cmd.Parameters.AddWithValue("@SortOrder", request.SortOrder);
                cmd.Parameters.AddWithValue("@IsEnabled", request.IsEnabled);

                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "shop", "update_item",
                    id.ToString(), $"更新商品: {request.DisplayName}", ClientIp);

                return Ok(ApiResult.Success(null, "更新成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "更新商品失败");
                return Ok(ApiResult.Fail("更新失败"));
            }
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (!IsSuperAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("DELETE FROM shop_items WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "shop", "delete_item",
                    id.ToString(), "删除商品", ClientIp);

                return Ok(ApiResult.Success(null, "删除成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "删除商品失败");
                return Ok(ApiResult.Fail("删除失败"));
            }
        }

        #endregion

        #region 订单管理

        /// <summary>
        /// 获取商城订单
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] PagedRequest request)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = "WHERE 1=1";
                if (!string.IsNullOrEmpty(request.Keyword))
                    where += " AND (CharName LIKE @Keyword OR OrderNo LIKE @Keyword)";
                if (request.StartTime.HasValue)
                    where += " AND CreateTime >= @StartTime";
                if (request.EndTime.HasValue)
                    where += " AND CreateTime <= @EndTime";

                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM shop_orders {where}", conn);
                AddPagedParams(countCmd, request);
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                int offset = (request.Page - 1) * request.PageSize;
                using var cmd = new MySqlCommand($"SELECT * FROM shop_orders {where} ORDER BY Id DESC LIMIT {offset},{request.PageSize}", conn);
                AddPagedParams(cmd, request);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt64("Id"),
                        OrderNo = reader.GetString("OrderNo"),
                        AccountId = reader.GetString("AccountId"),
                        CharName = reader.GetString("CharName"),
                        ShopItemId = reader.GetInt32("ShopItemId"),
                        ItemName = reader.GetString("ItemName"),
                        ItemCount = reader.GetInt32("ItemCount"),
                        UnitPrice = reader.GetInt32("UnitPrice"),
                        TotalPrice = reader.GetInt32("TotalPrice"),
                        Status = reader.GetInt32("Status"),
                        CreateTime = reader.GetDateTime("CreateTime")
                    });
                }

                return Ok(ApiResult.Success(new { total, request.Page, request.PageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取订单失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        #endregion

        #region 充值记录

        /// <summary>
        /// 获取充值记录
        /// </summary>
        [HttpGet("recharges")]
        public async Task<IActionResult> GetRecharges([FromQuery] PagedRequest request)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = "WHERE 1=1";
                if (!string.IsNullOrEmpty(request.Keyword))
                    where += " AND (AccountId LIKE @Keyword OR OrderNo LIKE @Keyword)";
                if (request.StartTime.HasValue)
                    where += " AND CreateTime >= @StartTime";
                if (request.EndTime.HasValue)
                    where += " AND CreateTime <= @EndTime";

                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM recharge_records {where}", conn);
                AddPagedParams(countCmd, request);
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                int offset = (request.Page - 1) * request.PageSize;
                using var cmd = new MySqlCommand($"SELECT * FROM recharge_records {where} ORDER BY Id DESC LIMIT {offset},{request.PageSize}", conn);
                AddPagedParams(cmd, request);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt64("Id"),
                        OrderNo = reader.GetString("OrderNo"),
                        AccountId = reader.GetString("AccountId"),
                        CharName = reader.IsDBNull(reader.GetOrdinal("CharName")) ? null : reader.GetString("CharName"),
                        Amount = reader.GetDecimal("Amount"),
                        GameGold = reader.GetInt32("GameGold"),
                        BonusGold = reader.GetInt32("BonusGold"),
                        PayChannel = reader.GetString("PayChannel"),
                        Status = reader.GetInt32("Status"),
                        PayTime = reader.IsDBNull(reader.GetOrdinal("PayTime")) ? null : (DateTime?)reader.GetDateTime("PayTime"),
                        CreateTime = reader.GetDateTime("CreateTime")
                    });
                }

                return Ok(ApiResult.Success(new { total, request.Page, request.PageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取充值记录失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 充值统计
        /// </summary>
        [HttpGet("recharges/stats")]
        public async Task<IActionResult> GetRechargeStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                startDate ??= DateTime.Today.AddDays(-30);
                endDate ??= DateTime.Today;

                // 总充值
                using var totalCmd = new MySqlCommand(@"
                    SELECT COUNT(*) as Count, COALESCE(SUM(Amount),0) as Amount, COALESCE(SUM(GameGold+BonusGold),0) as Gold
                    FROM recharge_records WHERE Status IN (1,2) AND CreateTime BETWEEN @Start AND @End", conn);
                totalCmd.Parameters.AddWithValue("@Start", startDate);
                totalCmd.Parameters.AddWithValue("@End", endDate.Value.AddDays(1));

                using var totalReader = await totalCmd.ExecuteReaderAsync();
                await totalReader.ReadAsync();
                var total = new
                {
                    Count = totalReader.GetInt32(0),
                    Amount = totalReader.GetDecimal(1),
                    Gold = totalReader.GetInt64(2)
                };
                totalReader.Close();

                // 按日统计
                using var dailyCmd = new MySqlCommand(@"
                    SELECT DATE(CreateTime) as Date, COUNT(*) as Count, SUM(Amount) as Amount
                    FROM recharge_records WHERE Status IN (1,2) AND CreateTime BETWEEN @Start AND @End
                    GROUP BY DATE(CreateTime) ORDER BY Date", conn);
                dailyCmd.Parameters.AddWithValue("@Start", startDate);
                dailyCmd.Parameters.AddWithValue("@End", endDate.Value.AddDays(1));

                var daily = new List<object>();
                using var dailyReader = await dailyCmd.ExecuteReaderAsync();
                while (await dailyReader.ReadAsync())
                {
                    daily.Add(new
                    {
                        Date = dailyReader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        Count = dailyReader.GetInt32(1),
                        Amount = dailyReader.GetDecimal(2)
                    });
                }

                return Ok(ApiResult.Success(new { total, daily }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取充值统计失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        #endregion

        #region 礼包码管理

        /// <summary>
        /// 获取礼包码列表
        /// </summary>
        [HttpGet("giftcodes")]
        public async Task<IActionResult> GetGiftCodes([FromQuery] string batchNo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var where = "WHERE 1=1";
                if (!string.IsNullOrEmpty(batchNo))
                    where += " AND BatchNo=@BatchNo";

                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM gift_codes {where}", conn);
                if (!string.IsNullOrEmpty(batchNo))
                    countCmd.Parameters.AddWithValue("@BatchNo", batchNo);
                int total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                int offset = (page - 1) * pageSize;
                using var cmd = new MySqlCommand($"SELECT * FROM gift_codes {where} ORDER BY Id DESC LIMIT {offset},{pageSize}", conn);
                if (!string.IsNullOrEmpty(batchNo))
                    cmd.Parameters.AddWithValue("@BatchNo", batchNo);

                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt64("Id"),
                        Code = reader.GetString("Code"),
                        BatchNo = reader.GetString("BatchNo"),
                        Name = reader.GetString("Name"),
                        GameGold = reader.GetInt32("GameGold"),
                        GamePoint = reader.GetInt32("GamePoint"),
                        MaxUseCount = reader.GetInt32("MaxUseCount"),
                        UsedCount = reader.GetInt32("UsedCount"),
                        EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : (DateTime?)reader.GetDateTime("EndTime"),
                        IsEnabled = reader.GetBoolean("IsEnabled"),
                        CreateTime = reader.GetDateTime("CreateTime")
                    });
                }

                return Ok(ApiResult.Success(new { total, page, pageSize, items = list }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取礼包码失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 批量生成礼包码
        /// </summary>
        [HttpPost("giftcodes/generate")]
        public async Task<IActionResult> GenerateGiftCodes([FromBody] GenerateGiftCodeRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            if (request.Count <= 0 || request.Count > 10000)
                return Ok(ApiResult.Fail("生成数量1-10000"));

            try
            {
                string items = request.Items != null ? System.Text.Json.JsonSerializer.Serialize(request.Items) : null;
                var codes = await _shopService.GenerateGiftCodesAsync(
                    request.BatchNo ?? $"BATCH{DateTime.Now:yyyyMMddHHmmss}",
                    request.Name,
                    request.GameGold,
                    request.GamePoint,
                    items,
                    request.Count,
                    request.EndTime
                );

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "shop", "generate_giftcode",
                    request.BatchNo, $"生成礼包码{request.Count}个, 元宝:{request.GameGold}", ClientIp);

                return Ok(ApiResult.Success(new { count = codes.Count, codes }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "生成礼包码失败");
                return Ok(ApiResult.Fail("生成失败"));
            }
        }

        #endregion

        #region 充值档位管理

        /// <summary>
        /// 获取充值档位列表
        /// </summary>
        [HttpGet("recharge/packages")]
        public async Task<IActionResult> GetRechargePackages()
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT * FROM recharge_packages ORDER BY Amount", conn);
                var list = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Amount = reader.GetDecimal("Amount"),
                        GameGold = reader.GetInt32("GameGold"),
                        BonusGold = reader.GetInt32("BonusGold"),
                        FirstBonusGold = reader.IsDBNull(reader.GetOrdinal("FirstBonusGold")) ? 0 : reader.GetInt32("FirstBonusGold"),
                        BonusItems = reader.IsDBNull(reader.GetOrdinal("BonusItems")) ? null : reader.GetString("BonusItems"),
                        IsHot = reader.GetBoolean("IsHot"),
                        IsEnabled = reader.GetBoolean("IsEnabled"),
                        SortOrder = reader.GetInt32("SortOrder"),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description")
                    });
                }

                return Ok(ApiResult.Success(list));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取充值档位失败");
                return Ok(ApiResult.Fail("查询失败"));
            }
        }

        /// <summary>
        /// 添加充值档位
        /// </summary>
        [HttpPost("recharge/packages")]
        public async Task<IActionResult> AddRechargePackage([FromBody] RechargePackageRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    INSERT INTO recharge_packages (Name, Amount, GameGold, BonusGold, FirstBonusGold, BonusItems, IsHot, IsEnabled, SortOrder, Description, CreateTime)
                    VALUES (@Name, @Amount, @GameGold, @BonusGold, @FirstBonusGold, @BonusItems, @IsHot, @IsEnabled, @SortOrder, @Description, NOW())", conn);

                cmd.Parameters.AddWithValue("@Name", request.Name);
                cmd.Parameters.AddWithValue("@Amount", request.Amount);
                cmd.Parameters.AddWithValue("@GameGold", request.GameGold);
                cmd.Parameters.AddWithValue("@BonusGold", request.BonusGold);
                cmd.Parameters.AddWithValue("@FirstBonusGold", request.FirstBonusGold);
                cmd.Parameters.AddWithValue("@BonusItems", request.BonusItems ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsHot", request.IsHot);
                cmd.Parameters.AddWithValue("@IsEnabled", request.IsEnabled);
                cmd.Parameters.AddWithValue("@SortOrder", request.SortOrder);
                cmd.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "shop", "add_package",
                    request.Name, $"添加充值档位: {request.Amount}元={request.GameGold}元宝", ClientIp);

                return Ok(ApiResult.Success("添加成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "添加充值档位失败");
                return Ok(ApiResult.Fail("添加失败"));
            }
        }

        /// <summary>
        /// 更新充值档位
        /// </summary>
        [HttpPut("recharge/packages/{id}")]
        public async Task<IActionResult> UpdateRechargePackage(int id, [FromBody] RechargePackageRequest request)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(@"
                    UPDATE recharge_packages SET 
                        Name=@Name, Amount=@Amount, GameGold=@GameGold, BonusGold=@BonusGold, 
                        FirstBonusGold=@FirstBonusGold, BonusItems=@BonusItems,
                        IsHot=@IsHot, IsEnabled=@IsEnabled, SortOrder=@SortOrder, Description=@Description,
                        UpdateTime=NOW()
                    WHERE Id=@Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", request.Name);
                cmd.Parameters.AddWithValue("@Amount", request.Amount);
                cmd.Parameters.AddWithValue("@GameGold", request.GameGold);
                cmd.Parameters.AddWithValue("@BonusGold", request.BonusGold);
                cmd.Parameters.AddWithValue("@FirstBonusGold", request.FirstBonusGold);
                cmd.Parameters.AddWithValue("@BonusItems", request.BonusItems ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsHot", request.IsHot);
                cmd.Parameters.AddWithValue("@IsEnabled", request.IsEnabled);
                cmd.Parameters.AddWithValue("@SortOrder", request.SortOrder);
                cmd.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "shop", "update_package",
                    id.ToString(), $"更新充值档位: {request.Amount}元", ClientIp);

                return Ok(ApiResult.Success("更新成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "更新充值档位失败");
                return Ok(ApiResult.Fail("更新失败"));
            }
        }

        /// <summary>
        /// 删除充值档位
        /// </summary>
        [HttpDelete("recharge/packages/{id}")]
        public async Task<IActionResult> DeleteRechargePackage(int id)
        {
            if (!IsSuperAdmin)
                return Ok(ApiResult.Fail("需要超级管理员权限"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("DELETE FROM recharge_packages WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();

                await _adminService.LogActionAsync(CurrentAdminId, CurrentAdminName, "shop", "delete_package",
                    id.ToString(), "删除充值档位", ClientIp);

                return Ok(ApiResult.Success("删除成功"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "删除充值档位失败");
                return Ok(ApiResult.Fail("删除失败"));
            }
        }

        /// <summary>
        /// 启用/禁用充值档位
        /// </summary>
        [HttpPost("recharge/packages/{id}/toggle")]
        public async Task<IActionResult> ToggleRechargePackage(int id, [FromQuery] bool enabled)
        {
            if (!IsAdmin)
                return Ok(ApiResult.Fail("权限不足"));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("UPDATE recharge_packages SET IsEnabled=@Enabled, UpdateTime=NOW() WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Enabled", enabled);
                await cmd.ExecuteNonQueryAsync();

                return Ok(ApiResult.Success(enabled ? "已启用" : "已禁用"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "切换充值档位状态失败");
                return Ok(ApiResult.Fail("操作失败"));
            }
        }

        #endregion

        #region 工具方法

        private void AddPagedParams(MySqlCommand cmd, PagedRequest request)
        {
            if (!string.IsNullOrEmpty(request.Keyword))
                cmd.Parameters.AddWithValue("@Keyword", $"%{request.Keyword}%");
            if (request.StartTime.HasValue)
                cmd.Parameters.AddWithValue("@StartTime", request.StartTime.Value);
            if (request.EndTime.HasValue)
                cmd.Parameters.AddWithValue("@EndTime", request.EndTime.Value);
        }

        private object ReadShopItem(MySqlDataReader reader)
        {
            return new
            {
                Id = reader.GetInt32("Id"),
                CategoryId = reader.GetInt32("CategoryId"),
                ItemName = reader.GetString("ItemName"),
                DisplayName = reader.GetString("DisplayName"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Price = reader.GetInt32("Price"),
                OriginalPrice = reader.GetInt32("OriginalPrice"),
                ItemCount = reader.GetInt32("ItemCount"),
                Stock = reader.GetInt32("Stock"),
                SoldCount = reader.GetInt32("SoldCount"),
                LimitPerUser = reader.GetInt32("LimitPerUser"),
                LimitPerDay = reader.GetInt32("LimitPerDay"),
                RequireLevel = reader.GetInt32("RequireLevel"),
                RequireVip = reader.GetInt32("RequireVip"),
                IsHot = reader.GetBoolean("IsHot"),
                IsNew = reader.GetBoolean("IsNew"),
                IsRecommend = reader.GetBoolean("IsRecommend"),
                SortOrder = reader.GetInt32("SortOrder"),
                IsEnabled = reader.GetBoolean("IsEnabled"),
                CreateTime = reader.GetDateTime("CreateTime")
            };
        }

        #endregion
    }

    public class AddShopItemRequest
    {
        public int CategoryId { get; set; }
        public string ItemName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int OriginalPrice { get; set; }
        public int ItemCount { get; set; } = 1;
        public int Stock { get; set; } = -1;
        public int LimitPerUser { get; set; }
        public int LimitPerDay { get; set; }
        public int RequireLevel { get; set; }
        public int RequireVip { get; set; }
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }
        public bool IsRecommend { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class GenerateGiftCodeRequest
    {
        public string BatchNo { get; set; }
        public string Name { get; set; }
        public int GameGold { get; set; }
        public int GamePoint { get; set; }
        public List<ShopModule.Models.GiftCodeItem> Items { get; set; }
        public int Count { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class RechargePackageRequest
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int GameGold { get; set; }
        public int BonusGold { get; set; }
        public int FirstBonusGold { get; set; }
        public string? BonusItems { get; set; }
        public bool IsHot { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int SortOrder { get; set; }
        public string? Description { get; set; }
    }
}
