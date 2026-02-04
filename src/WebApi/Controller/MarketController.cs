using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NLog;

namespace WebApi.Controller
{
    /// <summary>
    /// 寄售行API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MarketController : ControllerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;

        public MarketController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        /// <summary>
        /// 获取寄售行列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetMarketList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? itemType = null,
            [FromQuery] string sortBy = "create_time",
            [FromQuery] bool sortDesc = true)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var whereClause = "WHERE status = 0"; // 只查询在售商品
                if (itemType.HasValue)
                {
                    whereClause += $" AND item_type = {itemType.Value}";
                }

                var orderClause = sortDesc ? $"ORDER BY {sortBy} DESC" : $"ORDER BY {sortBy} ASC";
                int offset = (page - 1) * pageSize;

                // 查询总数
                using var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM market_items {whereClause}", conn);
                var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                // 查询列表
                var sql = $@"SELECT id, seller_account, seller_name, item_name, item_type, item_idx, 
                            item_data, price, create_time, expire_time 
                            FROM market_items {whereClause} {orderClause} LIMIT {offset}, {pageSize}";
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var items = new List<object>();
                while (await reader.ReadAsync())
                {
                    items.Add(new
                    {
                        id = reader.GetInt64("id"),
                        sellerName = reader.GetString("seller_name"),
                        itemName = reader.GetString("item_name"),
                        itemType = reader.GetInt32("item_type"),
                        price = reader.GetInt64("price"),
                        createTime = reader.GetDateTime("create_time"),
                        expireTime = reader.IsDBNull(reader.GetOrdinal("expire_time")) ? null : (DateTime?)reader.GetDateTime("expire_time")
                    });
                }

                return Ok(new { code = 0, data = new { list = items, total, page, pageSize } });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取寄售行列表失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 获取物品类型列表
        /// </summary>
        [HttpGet("types")]
        public IActionResult GetItemTypes()
        {
            var types = new[]
            {
                new { id = 0, name = "全部" },
                new { id = 1, name = "武器" },
                new { id = 2, name = "衣服" },
                new { id = 3, name = "首饰" },
                new { id = 4, name = "头盔" },
                new { id = 5, name = "药品" },
                new { id = 6, name = "材料" },
                new { id = 7, name = "其他" }
            };
            return Ok(new { code = 0, data = types });
        }

        /// <summary>
        /// 搜索物品
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchItems([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Ok(new { code = -1, msg = "请输入搜索关键词" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                int offset = (page - 1) * pageSize;
                var sql = $@"SELECT id, seller_name, item_name, item_type, price, create_time 
                            FROM market_items 
                            WHERE status = 0 AND item_name LIKE @Keyword 
                            ORDER BY create_time DESC LIMIT {offset}, {pageSize}";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                using var reader = await cmd.ExecuteReaderAsync();

                var items = new List<object>();
                while (await reader.ReadAsync())
                {
                    items.Add(new
                    {
                        id = reader.GetInt64("id"),
                        sellerName = reader.GetString("seller_name"),
                        itemName = reader.GetString("item_name"),
                        itemType = reader.GetInt32("item_type"),
                        price = reader.GetInt64("price"),
                        createTime = reader.GetDateTime("create_time")
                    });
                }

                return Ok(new { code = 0, data = items });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "搜索寄售物品失败");
                return Ok(new { code = -1, msg = "搜索失败" });
            }
        }

        /// <summary>
        /// 购买物品
        /// </summary>
        [HttpPost("buy")]
        public async Task<IActionResult> BuyItem([FromBody] BuyMarketItemRequest request)
        {
            try
            {
                if (request == null || request.ItemId <= 0 || string.IsNullOrEmpty(request.BuyerAccount))
                {
                    return Ok(new { code = -1, msg = "参数错误" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                using var transaction = await conn.BeginTransactionAsync();

                try
                {
                    // 查询物品信息
                    using var selectCmd = new MySqlCommand(
                        "SELECT * FROM market_items WHERE id = @Id AND status = 0 FOR UPDATE", conn, transaction);
                    selectCmd.Parameters.AddWithValue("@Id", request.ItemId);
                    using var reader = await selectCmd.ExecuteReaderAsync();

                    if (!await reader.ReadAsync())
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { code = -1, msg = "物品不存在或已售出" });
                    }

                    long price = reader.GetInt64("price");
                    string sellerAccount = reader.GetString("seller_account");
                    string itemName = reader.GetString("item_name");
                    await reader.CloseAsync();

                    // 检查买家元宝
                    using var checkGoldCmd = new MySqlCommand(
                        "SELECT Gold FROM characters WHERE LoginID = @Account AND Deleted = 0 ORDER BY Id LIMIT 1", conn, transaction);
                    checkGoldCmd.Parameters.AddWithValue("@Account", request.BuyerAccount);
                    var buyerGold = await checkGoldCmd.ExecuteScalarAsync();

                    if (buyerGold == null || Convert.ToInt64(buyerGold) < price)
                    {
                        await transaction.RollbackAsync();
                        return Ok(new { code = -2, msg = "元宝不足" });
                    }

                    // 扣除买家元宝
                    using var deductCmd = new MySqlCommand(
                        "UPDATE characters SET Gold = Gold - @Price WHERE LoginID = @Account AND Deleted = 0 ORDER BY Id LIMIT 1", conn, transaction);
                    deductCmd.Parameters.AddWithValue("@Price", price);
                    deductCmd.Parameters.AddWithValue("@Account", request.BuyerAccount);
                    await deductCmd.ExecuteNonQueryAsync();

                    // 增加卖家元宝
                    using var addCmd = new MySqlCommand(
                        "UPDATE characters SET Gold = Gold + @Price WHERE LoginID = @Account AND Deleted = 0 ORDER BY Id LIMIT 1", conn, transaction);
                    addCmd.Parameters.AddWithValue("@Price", price);
                    addCmd.Parameters.AddWithValue("@Account", sellerAccount);
                    await addCmd.ExecuteNonQueryAsync();

                    // 更新物品状态
                    using var updateCmd = new MySqlCommand(
                        "UPDATE market_items SET status = 1, buyer_account = @Buyer, buy_time = NOW() WHERE id = @Id", conn, transaction);
                    updateCmd.Parameters.AddWithValue("@Id", request.ItemId);
                    updateCmd.Parameters.AddWithValue("@Buyer", request.BuyerAccount);
                    await updateCmd.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                    Logger.Info($"寄售行购买成功: {request.BuyerAccount} 购买 {itemName}, 价格: {price}");
                    return Ok(new { code = 0, msg = "购买成功", data = new { itemName, price } });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "购买寄售物品失败");
                return Ok(new { code = -1, msg = "购买失败" });
            }
        }

        /// <summary>
        /// 上架物品(需要游戏内操作，这里只提供查询接口)
        /// </summary>
        [HttpGet("myselling")]
        public async Task<IActionResult> GetMySelling([FromQuery] string accountId)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                {
                    return Ok(new { code = -1, msg = "参数错误" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"SELECT id, item_name, item_type, price, create_time, status 
                           FROM market_items WHERE seller_account = @Account ORDER BY create_time DESC";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Account", accountId);
                using var reader = await cmd.ExecuteReaderAsync();

                var items = new List<object>();
                while (await reader.ReadAsync())
                {
                    items.Add(new
                    {
                        id = reader.GetInt64("id"),
                        itemName = reader.GetString("item_name"),
                        itemType = reader.GetInt32("item_type"),
                        price = reader.GetInt64("price"),
                        createTime = reader.GetDateTime("create_time"),
                        status = reader.GetInt32("status") // 0=在售 1=已售 2=已取消
                    });
                }

                return Ok(new { code = 0, data = items });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "查询我的寄售失败");
                return Ok(new { code = -1, msg = "查询失败" });
            }
        }

        /// <summary>
        /// 取消寄售
        /// </summary>
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelSelling([FromBody] CancelMarketItemRequest request)
        {
            try
            {
                if (request == null || request.ItemId <= 0 || string.IsNullOrEmpty(request.AccountId))
                {
                    return Ok(new { code = -1, msg = "参数错误" });
                }

                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // 只能取消自己的在售物品
                using var cmd = new MySqlCommand(
                    "UPDATE market_items SET status = 2, cancel_time = NOW() WHERE id = @Id AND seller_account = @Account AND status = 0", conn);
                cmd.Parameters.AddWithValue("@Id", request.ItemId);
                cmd.Parameters.AddWithValue("@Account", request.AccountId);

                int affected = await cmd.ExecuteNonQueryAsync();
                if (affected > 0)
                {
                    Logger.Info($"取消寄售: {request.AccountId} 取消物品 {request.ItemId}");
                    return Ok(new { code = 0, msg = "取消成功" });
                }
                else
                {
                    return Ok(new { code = -1, msg = "物品不存在或无法取消" });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "取消寄售失败");
                return Ok(new { code = -1, msg = "取消失败" });
            }
        }
    }

    #region 请求模型

    public class BuyMarketItemRequest
    {
        public long ItemId { get; set; }
        public string BuyerAccount { get; set; }
    }

    public class CancelMarketItemRequest
    {
        public long ItemId { get; set; }
        public string AccountId { get; set; }
    }

    #endregion
}