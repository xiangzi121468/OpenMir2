using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using ShopModule.Models;

namespace ShopModule.Repository
{
    /// <summary>
    /// MySQL商城数据仓储实现
    /// </summary>
    public class MySqlShopRepository : IShopRepository
    {
        private readonly string _connectionString;

        public MySqlShopRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        #region 分类

        public async Task<List<ShopCategory>> GetCategoriesAsync()
        {
            var list = new List<ShopCategory>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "SELECT * FROM shop_categories WHERE IsEnabled=1 ORDER BY SortOrder", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ShopCategory
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    SortOrder = reader.GetInt32("SortOrder"),
                    Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString("Icon"),
                    IsEnabled = reader.GetBoolean("IsEnabled")
                });
            }
            return list;
        }

        #endregion

        #region 商品

        public async Task<List<ShopItem>> GetItemsByCategoryAsync(int categoryId)
        {
            var list = new List<ShopItem>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "SELECT * FROM shop_items WHERE CategoryId=@CategoryId AND IsEnabled=1 ORDER BY SortOrder", conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(ReadShopItem(reader));
            }
            return list;
        }

        public async Task<List<ShopItem>> GetHotItemsAsync(int limit)
        {
            var list = new List<ShopItem>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                $"SELECT * FROM shop_items WHERE IsHot=1 AND IsEnabled=1 ORDER BY SortOrder LIMIT {limit}", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(ReadShopItem(reader));
            }
            return list;
        }

        public async Task<List<ShopItem>> GetNewItemsAsync(int limit)
        {
            var list = new List<ShopItem>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                $"SELECT * FROM shop_items WHERE IsNew=1 AND IsEnabled=1 ORDER BY CreateTime DESC LIMIT {limit}", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(ReadShopItem(reader));
            }
            return list;
        }

        public async Task<List<ShopItem>> GetRecommendItemsAsync(int limit)
        {
            var list = new List<ShopItem>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                $"SELECT * FROM shop_items WHERE IsRecommend=1 AND IsEnabled=1 ORDER BY SortOrder LIMIT {limit}", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(ReadShopItem(reader));
            }
            return list;
        }

        public async Task<List<ShopItem>> SearchItemsAsync(string keyword)
        {
            var list = new List<ShopItem>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "SELECT * FROM shop_items WHERE IsEnabled=1 AND (DisplayName LIKE @Keyword OR ItemName LIKE @Keyword) ORDER BY SortOrder", conn);
            cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(ReadShopItem(reader));
            }
            return list;
        }

        public async Task<ShopItem> GetItemAsync(int itemId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM shop_items WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", itemId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadShopItem(reader);
            }
            return null;
        }

        public async Task UpdateStockAsync(int itemId, int delta)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE shop_items SET Stock=Stock+@Delta WHERE Id=@Id AND Stock>0", conn);
            cmd.Parameters.AddWithValue("@Id", itemId);
            cmd.Parameters.AddWithValue("@Delta", delta);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateSoldCountAsync(int itemId, int delta)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE shop_items SET SoldCount=SoldCount+@Delta WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", itemId);
            cmd.Parameters.AddWithValue("@Delta", delta);
            await cmd.ExecuteNonQueryAsync();
        }

        private ShopItem ReadShopItem(MySqlDataReader reader)
        {
            return new ShopItem
            {
                Id = reader.GetInt32("Id"),
                CategoryId = reader.GetInt32("CategoryId"),
                ItemName = reader.GetString("ItemName"),
                ItemId = reader.GetInt32("ItemId"),
                DisplayName = reader.GetString("DisplayName"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString("Icon"),
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
                StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? (DateTime?)null : reader.GetDateTime("StartTime"),
                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? (DateTime?)null : reader.GetDateTime("EndTime"),
                IsEnabled = reader.GetBoolean("IsEnabled")
            };
        }

        #endregion

        #region 订单

        public async Task CreateOrderAsync(ShopOrder order)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                INSERT INTO shop_orders (OrderNo, AccountId, CharName, ShopItemId, ItemName, ItemCount, UnitPrice, TotalPrice, Status, CreateTime)
                VALUES (@OrderNo, @AccountId, @CharName, @ShopItemId, @ItemName, @ItemCount, @UnitPrice, @TotalPrice, @Status, @CreateTime)", conn);

            cmd.Parameters.AddWithValue("@OrderNo", order.OrderNo);
            cmd.Parameters.AddWithValue("@AccountId", order.AccountId);
            cmd.Parameters.AddWithValue("@CharName", order.CharName);
            cmd.Parameters.AddWithValue("@ShopItemId", order.ShopItemId);
            cmd.Parameters.AddWithValue("@ItemName", order.ItemName);
            cmd.Parameters.AddWithValue("@ItemCount", order.ItemCount);
            cmd.Parameters.AddWithValue("@UnitPrice", order.UnitPrice);
            cmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
            cmd.Parameters.AddWithValue("@Status", (int)order.Status);
            cmd.Parameters.AddWithValue("@CreateTime", order.CreateTime);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<ShopOrder> GetOrderByNoAsync(string orderNo)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM shop_orders WHERE OrderNo=@OrderNo", conn);
            cmd.Parameters.AddWithValue("@OrderNo", orderNo);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ShopOrder
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
                    Status = (OrderStatus)reader.GetByte("Status"),
                    DeliverTime = reader.IsDBNull(reader.GetOrdinal("DeliverTime")) ? (DateTime?)null : reader.GetDateTime("DeliverTime"),
                    CreateTime = reader.GetDateTime("CreateTime")
                };
            }
            return null;
        }

        public async Task UpdateOrderAsync(ShopOrder order)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE shop_orders SET Status=@Status, DeliverTime=@DeliverTime, Remark=@Remark WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", order.Id);
            cmd.Parameters.AddWithValue("@Status", (int)order.Status);
            cmd.Parameters.AddWithValue("@DeliverTime", order.DeliverTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Remark", order.Remark ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<ShopOrder>> GetUserOrdersAsync(string accountId, int page, int pageSize)
        {
            var list = new List<ShopOrder>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            int offset = (page - 1) * pageSize;
            using var cmd = new MySqlCommand(
                $"SELECT * FROM shop_orders WHERE AccountId=@AccountId ORDER BY CreateTime DESC LIMIT {offset},{pageSize}", conn);
            cmd.Parameters.AddWithValue("@AccountId", accountId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ShopOrder
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
                    Status = (OrderStatus)reader.GetByte("Status"),
                    CreateTime = reader.GetDateTime("CreateTime")
                });
            }
            return list;
        }

        public async Task<int> GetUserTodayBuyCountAsync(string accountId, int shopItemId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                SELECT COALESCE(SUM(ItemCount),0) FROM shop_orders 
                WHERE AccountId=@AccountId AND ShopItemId=@ShopItemId 
                AND DATE(CreateTime)=CURDATE() AND Status IN (0,1)", conn);
            cmd.Parameters.AddWithValue("@AccountId", accountId);
            cmd.Parameters.AddWithValue("@ShopItemId", shopItemId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> GetUserTotalBuyCountAsync(string accountId, int shopItemId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                SELECT COALESCE(SUM(ItemCount),0) FROM shop_orders 
                WHERE AccountId=@AccountId AND ShopItemId=@ShopItemId AND Status IN (0,1)", conn);
            cmd.Parameters.AddWithValue("@AccountId", accountId);
            cmd.Parameters.AddWithValue("@ShopItemId", shopItemId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        #endregion

        #region 充值

        public async Task<List<RechargePackage>> GetRechargePackagesAsync()
        {
            var list = new List<RechargePackage>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "SELECT * FROM recharge_packages WHERE IsEnabled=1 ORDER BY SortOrder", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new RechargePackage
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Amount = reader.GetDecimal("Amount"),
                    GameGold = reader.GetInt32("GameGold"),
                    BonusGold = reader.GetInt32("BonusGold"),
                    ExtraBonusGold = reader.GetInt32("ExtraBonusGold"),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                    SortOrder = reader.GetInt32("SortOrder"),
                    IsEnabled = reader.GetBoolean("IsEnabled"),
                    IsHot = reader.GetBoolean("IsHot")
                });
            }
            return list;
        }

        public async Task<RechargePackage> GetRechargePackageAsync(int packageId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM recharge_packages WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", packageId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RechargePackage
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Amount = reader.GetDecimal("Amount"),
                    GameGold = reader.GetInt32("GameGold"),
                    BonusGold = reader.GetInt32("BonusGold"),
                    ExtraBonusGold = reader.GetInt32("ExtraBonusGold"),
                    IsEnabled = reader.GetBoolean("IsEnabled")
                };
            }
            return null;
        }

        public async Task CreateRechargeRecordAsync(RechargeRecord record)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                INSERT INTO recharge_records (OrderNo, AccountId, CharName, Amount, GameGold, BonusGold, PayChannel, Status, ClientIp, CreateTime)
                VALUES (@OrderNo, @AccountId, @CharName, @Amount, @GameGold, @BonusGold, @PayChannel, @Status, @ClientIp, @CreateTime)", conn);

            cmd.Parameters.AddWithValue("@OrderNo", record.OrderNo);
            cmd.Parameters.AddWithValue("@AccountId", record.AccountId);
            cmd.Parameters.AddWithValue("@CharName", record.CharName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Amount", record.Amount);
            cmd.Parameters.AddWithValue("@GameGold", record.GameGold);
            cmd.Parameters.AddWithValue("@BonusGold", record.BonusGold);
            cmd.Parameters.AddWithValue("@PayChannel", record.PayChannel);
            cmd.Parameters.AddWithValue("@Status", (int)record.Status);
            cmd.Parameters.AddWithValue("@ClientIp", record.ClientIp ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreateTime", record.CreateTime);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<RechargeRecord> GetRechargeRecordByNoAsync(string orderNo)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM recharge_records WHERE OrderNo=@OrderNo", conn);
            cmd.Parameters.AddWithValue("@OrderNo", orderNo);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadRechargeRecord(reader);
            }
            return null;
        }

        public async Task UpdateRechargeRecordAsync(RechargeRecord record)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                UPDATE recharge_records SET PayOrderNo=@PayOrderNo, Status=@Status, PayTime=@PayTime, DeliverTime=@DeliverTime, Remark=@Remark 
                WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", record.Id);
            cmd.Parameters.AddWithValue("@PayOrderNo", record.PayOrderNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (int)record.Status);
            cmd.Parameters.AddWithValue("@PayTime", record.PayTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DeliverTime", record.DeliverTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Remark", record.Remark ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<RechargeRecord>> GetRechargeRecordsAsync(string accountId, int page, int pageSize)
        {
            var list = new List<RechargeRecord>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            int offset = (page - 1) * pageSize;
            using var cmd = new MySqlCommand(
                $"SELECT * FROM recharge_records WHERE AccountId=@AccountId ORDER BY CreateTime DESC LIMIT {offset},{pageSize}", conn);
            cmd.Parameters.AddWithValue("@AccountId", accountId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(ReadRechargeRecord(reader));
            }
            return list;
        }

        public async Task<bool> IsFirstRechargeAsync(string accountId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM recharge_records WHERE AccountId=@AccountId AND Status IN (1,2)", conn);
            cmd.Parameters.AddWithValue("@AccountId", accountId);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count == 0;
        }

        private RechargeRecord ReadRechargeRecord(MySqlDataReader reader)
        {
            return new RechargeRecord
            {
                Id = reader.GetInt64("Id"),
                OrderNo = reader.GetString("OrderNo"),
                AccountId = reader.GetString("AccountId"),
                CharName = reader.IsDBNull(reader.GetOrdinal("CharName")) ? null : reader.GetString("CharName"),
                Amount = reader.GetDecimal("Amount"),
                GameGold = reader.GetInt32("GameGold"),
                BonusGold = reader.GetInt32("BonusGold"),
                PayChannel = reader.GetString("PayChannel"),
                PayOrderNo = reader.IsDBNull(reader.GetOrdinal("PayOrderNo")) ? null : reader.GetString("PayOrderNo"),
                Status = (RechargeStatus)reader.GetByte("Status"),
                PayTime = reader.IsDBNull(reader.GetOrdinal("PayTime")) ? (DateTime?)null : reader.GetDateTime("PayTime"),
                DeliverTime = reader.IsDBNull(reader.GetOrdinal("DeliverTime")) ? (DateTime?)null : reader.GetDateTime("DeliverTime"),
                CreateTime = reader.GetDateTime("CreateTime")
            };
        }

        public async Task<bool> AddPlayerGameGoldAsync(string accountId, string charName, int gold)
        {
            try
            {
                using var conn = CreateConnection();
                await conn.OpenAsync();

                // 根据账号或角色名更新元宝
                MySqlCommand cmd;
                if (!string.IsNullOrEmpty(charName))
                {
                    // 优先按角色名更新
                    cmd = new MySqlCommand(
                        "UPDATE characters SET GameGold = GameGold + @Gold WHERE ChrName = @ChrName AND LoginID = @LoginID", conn);
                    cmd.Parameters.AddWithValue("@ChrName", charName);
                    cmd.Parameters.AddWithValue("@LoginID", accountId);
                }
                else
                {
                    // 按账号更新（更新该账号下的第一个角色）
                    cmd = new MySqlCommand(
                        "UPDATE characters SET GameGold = GameGold + @Gold WHERE LoginID = @LoginID AND Deleted = 0 ORDER BY Id LIMIT 1", conn);
                    cmd.Parameters.AddWithValue("@LoginID", accountId);
                }
                cmd.Parameters.AddWithValue("@Gold", gold);

                int affected = await cmd.ExecuteNonQueryAsync();
                return affected > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"增加玩家元宝失败: {accountId}, {charName}, {gold}");
                return false;
            }
        }

        public async Task<int> GetPlayerGameGoldAsync(string accountId, string charName)
        {
            try
            {
                using var conn = CreateConnection();
                await conn.OpenAsync();

                MySqlCommand cmd;
                if (!string.IsNullOrEmpty(charName))
                {
                    cmd = new MySqlCommand(
                        "SELECT Gold FROM characters WHERE ChrName = @ChrName AND LoginID = @LoginID", conn);
                    cmd.Parameters.AddWithValue("@ChrName", charName);
                    cmd.Parameters.AddWithValue("@LoginID", accountId);
                }
                else
                {
                    cmd = new MySqlCommand(
                        "SELECT Gold FROM characters WHERE LoginID = @LoginID AND Deleted = 0 ORDER BY Id LIMIT 1", conn);
                    cmd.Parameters.AddWithValue("@LoginID", accountId);
                }

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"查询玩家元宝失败: {accountId}, {charName}");
                return 0;
            }
        }

        #endregion

        #region 礼包码

        public async Task<GiftCode> GetGiftCodeAsync(string code)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM gift_codes WHERE Code=@Code", conn);
            cmd.Parameters.AddWithValue("@Code", code);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new GiftCode
                {
                    Id = reader.GetInt64("Id"),
                    Code = reader.GetString("Code"),
                    BatchNo = reader.GetString("BatchNo"),
                    Name = reader.GetString("Name"),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                    GameGold = reader.GetInt32("GameGold"),
                    GamePoint = reader.GetInt32("GamePoint"),
                    Items = reader.IsDBNull(reader.GetOrdinal("Items")) ? null : reader.GetString("Items"),
                    MaxUseCount = reader.GetInt32("MaxUseCount"),
                    UsedCount = reader.GetInt32("UsedCount"),
                    LimitOnePerAccount = reader.GetBoolean("LimitOnePerAccount"),
                    RequireLevel = reader.GetInt32("RequireLevel"),
                    StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? (DateTime?)null : reader.GetDateTime("StartTime"),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? (DateTime?)null : reader.GetDateTime("EndTime"),
                    IsEnabled = reader.GetBoolean("IsEnabled"),
                    CreateTime = reader.GetDateTime("CreateTime")
                };
            }
            return null;
        }

        public async Task CreateGiftCodeAsync(GiftCode giftCode)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                INSERT INTO gift_codes (Code, BatchNo, Name, Description, GameGold, GamePoint, Items, MaxUseCount, LimitOnePerAccount, RequireLevel, StartTime, EndTime, IsEnabled, CreateTime)
                VALUES (@Code, @BatchNo, @Name, @Description, @GameGold, @GamePoint, @Items, @MaxUseCount, @LimitOnePerAccount, @RequireLevel, @StartTime, @EndTime, @IsEnabled, @CreateTime)", conn);

            cmd.Parameters.AddWithValue("@Code", giftCode.Code);
            cmd.Parameters.AddWithValue("@BatchNo", giftCode.BatchNo);
            cmd.Parameters.AddWithValue("@Name", giftCode.Name);
            cmd.Parameters.AddWithValue("@Description", giftCode.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@GameGold", giftCode.GameGold);
            cmd.Parameters.AddWithValue("@GamePoint", giftCode.GamePoint);
            cmd.Parameters.AddWithValue("@Items", giftCode.Items ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxUseCount", giftCode.MaxUseCount);
            cmd.Parameters.AddWithValue("@LimitOnePerAccount", giftCode.LimitOnePerAccount);
            cmd.Parameters.AddWithValue("@RequireLevel", giftCode.RequireLevel);
            cmd.Parameters.AddWithValue("@StartTime", giftCode.StartTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@EndTime", giftCode.EndTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsEnabled", giftCode.IsEnabled);
            cmd.Parameters.AddWithValue("@CreateTime", giftCode.CreateTime);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task IncrementGiftCodeUsedCountAsync(long codeId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE gift_codes SET UsedCount=UsedCount+1 WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", codeId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> HasUsedGiftCodeAsync(string code, string accountId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM gift_code_records WHERE Code=@Code AND AccountId=@AccountId", conn);
            cmd.Parameters.AddWithValue("@Code", code);
            cmd.Parameters.AddWithValue("@AccountId", accountId);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task CreateGiftCodeRecordAsync(GiftCodeRecord record)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                INSERT INTO gift_code_records (CodeId, Code, AccountId, CharName, ClientIp, UseTime)
                VALUES (@CodeId, @Code, @AccountId, @CharName, @ClientIp, @UseTime)", conn);

            cmd.Parameters.AddWithValue("@CodeId", record.CodeId);
            cmd.Parameters.AddWithValue("@Code", record.Code);
            cmd.Parameters.AddWithValue("@AccountId", record.AccountId);
            cmd.Parameters.AddWithValue("@CharName", record.CharName);
            cmd.Parameters.AddWithValue("@ClientIp", record.ClientIp ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UseTime", record.UseTime);

            await cmd.ExecuteNonQueryAsync();
        }

        #endregion

        #region VIP

        public async Task<List<VipLevel>> GetVipLevelsAsync()
        {
            var list = new List<VipLevel>();
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM vip_levels ORDER BY Level", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new VipLevel
                {
                    Level = reader.GetInt32("Level"),
                    Name = reader.GetString("Name"),
                    RequireRecharge = reader.GetInt32("RequireRecharge"),
                    ExpBonus = reader.GetInt32("ExpBonus"),
                    DropBonus = reader.GetInt32("DropBonus"),
                    ShopDiscount = reader.GetInt32("ShopDiscount"),
                    DailyGift = reader.IsDBNull(reader.GetOrdinal("DailyGift")) ? null : reader.GetString("DailyGift"),
                    Privileges = reader.IsDBNull(reader.GetOrdinal("Privileges")) ? null : reader.GetString("Privileges"),
                    Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString("Icon")
                });
            }
            return list;
        }

        public async Task<UserVip> GetUserVipAsync(string accountId)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM user_vip WHERE AccountId=@AccountId", conn);
            cmd.Parameters.AddWithValue("@AccountId", accountId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserVip
                {
                    AccountId = reader.GetString("AccountId"),
                    VipLevel = reader.GetInt32("VipLevel"),
                    TotalRecharge = reader.GetInt32("TotalRecharge"),
                    LastDailyGiftTime = reader.IsDBNull(reader.GetOrdinal("LastDailyGiftTime")) ? (DateTime?)null : reader.GetDateTime("LastDailyGiftTime")
                };
            }
            return null;
        }

        public async Task SaveUserVipAsync(UserVip userVip)
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(@"
                INSERT INTO user_vip (AccountId, VipLevel, TotalRecharge, LastDailyGiftTime)
                VALUES (@AccountId, @VipLevel, @TotalRecharge, @LastDailyGiftTime)
                ON DUPLICATE KEY UPDATE VipLevel=@VipLevel, TotalRecharge=@TotalRecharge, LastDailyGiftTime=@LastDailyGiftTime", conn);

            cmd.Parameters.AddWithValue("@AccountId", userVip.AccountId);
            cmd.Parameters.AddWithValue("@VipLevel", userVip.VipLevel);
            cmd.Parameters.AddWithValue("@TotalRecharge", userVip.TotalRecharge);
            cmd.Parameters.AddWithValue("@LastDailyGiftTime", userVip.LastDailyGiftTime ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        #endregion
    }
}
