using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;
using ShopModule.Models;
using ShopModule.Repository;

namespace ShopModule
{
    /// <summary>
    /// 商城服务实现
    /// </summary>
    public class ShopService : IShopService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IShopRepository _repository;
        private List<VipLevel> _vipLevels;

        public ShopService(IShopRepository repository)
        {
            _repository = repository;
        }

        #region 商品管理

        public async Task<List<ShopCategory>> GetCategoriesAsync()
        {
            return await _repository.GetCategoriesAsync();
        }

        public async Task<List<ShopItem>> GetItemsByCategoryAsync(int categoryId)
        {
            var items = await _repository.GetItemsByCategoryAsync(categoryId);
            return items.Where(x => x.IsOnSale && x.HasStock).ToList();
        }

        public async Task<List<ShopItem>> GetHotItemsAsync(int limit = 10)
        {
            var items = await _repository.GetHotItemsAsync(limit);
            return items.Where(x => x.IsOnSale && x.HasStock).ToList();
        }

        public async Task<List<ShopItem>> GetNewItemsAsync(int limit = 10)
        {
            var items = await _repository.GetNewItemsAsync(limit);
            return items.Where(x => x.IsOnSale && x.HasStock).ToList();
        }

        public async Task<List<ShopItem>> GetRecommendItemsAsync(int limit = 10)
        {
            var items = await _repository.GetRecommendItemsAsync(limit);
            return items.Where(x => x.IsOnSale && x.HasStock).ToList();
        }

        public async Task<List<ShopItem>> SearchItemsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<ShopItem>();

            var items = await _repository.SearchItemsAsync(keyword);
            return items.Where(x => x.IsOnSale && x.HasStock).ToList();
        }

        public async Task<ShopItem> GetItemAsync(int itemId)
        {
            return await _repository.GetItemAsync(itemId);
        }

        #endregion

        #region 购买

        public async Task<BuyResult> BuyItemAsync(BuyRequest request, int currentGold, int vipLevel)
        {
            try
            {
                // 获取商品信息
                var item = await _repository.GetItemAsync(request.ShopItemId);
                if (item == null)
                    return BuyResult.Fail("商品不存在");

                if (!item.IsOnSale)
                    return BuyResult.Fail("商品已下架");

                if (!item.HasStock)
                    return BuyResult.Fail("商品库存不足");

                // 检查等级限制
                // 这里需要从外部传入玩家等级，暂时跳过

                // 检查VIP限制
                if (item.RequireVip > vipLevel)
                    return BuyResult.Fail($"需要VIP{item.RequireVip}才能购买");

                // 检查限购
                if (item.LimitPerUser > 0)
                {
                    var totalCount = await GetUserTotalBuyCountAsync(request.AccountId, item.Id);
                    if (totalCount + request.BuyCount > item.LimitPerUser)
                        return BuyResult.Fail($"该商品每人限购{item.LimitPerUser}件");
                }

                if (item.LimitPerDay > 0)
                {
                    var todayCount = await GetUserTodayBuyCountAsync(request.AccountId, item.Id);
                    if (todayCount + request.BuyCount > item.LimitPerDay)
                        return BuyResult.Fail($"该商品每日限购{item.LimitPerDay}件");
                }

                // 计算价格(VIP折扣)
                int unitPrice = GetVipPrice(item.Price, vipLevel);
                int totalPrice = unitPrice * request.BuyCount;

                // 检查元宝
                if (currentGold < totalPrice)
                    return BuyResult.Fail("元宝不足");

                // 生成订单号
                string orderNo = GenerateOrderNo("SH");

                // 创建订单
                var order = new ShopOrder
                {
                    OrderNo = orderNo,
                    AccountId = request.AccountId,
                    CharName = request.CharName,
                    ShopItemId = item.Id,
                    ItemName = item.ItemName,
                    ItemCount = item.ItemCount * request.BuyCount,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    Status = OrderStatus.Pending,
                    CreateTime = DateTime.Now
                };

                await _repository.CreateOrderAsync(order);

                // 更新库存
                if (item.Stock > 0)
                {
                    await _repository.UpdateStockAsync(item.Id, -request.BuyCount);
                }

                // 更新销量
                await _repository.UpdateSoldCountAsync(item.Id, request.BuyCount);

                Logger.Info($"商城购买成功: {request.CharName} 购买 {item.DisplayName} x{request.BuyCount}, 花费{totalPrice}元宝, 订单号:{orderNo}");

                return BuyResult.Ok(orderNo, totalPrice, currentGold - totalPrice);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"商城购买失败: {request.CharName} -> {request.ShopItemId}");
                return BuyResult.Fail("购买失败，请稍后重试");
            }
        }

        public async Task<bool> DeliverOrderAsync(string orderNo)
        {
            try
            {
                var order = await _repository.GetOrderByNoAsync(orderNo);
                if (order == null || order.Status != OrderStatus.Pending)
                    return false;

                // 更新订单状态
                order.Status = OrderStatus.Delivered;
                order.DeliverTime = DateTime.Now;
                await _repository.UpdateOrderAsync(order);

                Logger.Info($"订单发货成功: {orderNo}, 物品:{order.ItemName} x{order.ItemCount}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"订单发货失败: {orderNo}");
                return false;
            }
        }

        public async Task<List<ShopOrder>> GetUserOrdersAsync(string accountId, int page = 1, int pageSize = 20)
        {
            return await _repository.GetUserOrdersAsync(accountId, page, pageSize);
        }

        public async Task<int> GetUserTodayBuyCountAsync(string accountId, int shopItemId)
        {
            return await _repository.GetUserTodayBuyCountAsync(accountId, shopItemId);
        }

        public async Task<int> GetUserTotalBuyCountAsync(string accountId, int shopItemId)
        {
            return await _repository.GetUserTotalBuyCountAsync(accountId, shopItemId);
        }

        #endregion

        #region 充值

        public async Task<List<RechargePackage>> GetRechargePackagesAsync()
        {
            return await _repository.GetRechargePackagesAsync();
        }

        public async Task<RechargeRecord> CreateRechargeOrderAsync(string accountId, string charName, int packageId, string payChannel, string clientIp)
        {
            try
            {
                var package = await _repository.GetRechargePackageAsync(packageId);
                if (package == null || !package.IsEnabled)
                    return null;

                bool isFirst = await IsFirstRechargeAsync(accountId);
                int bonusGold = isFirst ? package.BonusGold : 0;

                string orderNo = GenerateOrderNo("RC");

                var record = new RechargeRecord
                {
                    OrderNo = orderNo,
                    AccountId = accountId,
                    CharName = charName,
                    Amount = package.Amount,
                    GameGold = package.GameGold,
                    BonusGold = bonusGold + package.ExtraBonusGold,
                    PayChannel = payChannel,
                    Status = RechargeStatus.Pending,
                    ClientIp = clientIp,
                    CreateTime = DateTime.Now
                };

                await _repository.CreateRechargeRecordAsync(record);

                Logger.Info($"创建充值订单: {accountId}, 金额:{package.Amount}, 订单号:{orderNo}");
                return record;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"创建充值订单失败: {accountId}");
                return null;
            }
        }

        public async Task<bool> HandlePayCallbackAsync(string orderNo, string payOrderNo, bool success)
        {
            try
            {
                var record = await _repository.GetRechargeRecordByNoAsync(orderNo);
                if (record == null)
                {
                    Logger.Warn($"充值回调订单不存在: {orderNo}");
                    return false;
                }

                if (record.Status != RechargeStatus.Pending)
                {
                    Logger.Warn($"充值订单状态异常: {orderNo}, 状态:{record.Status}");
                    return false;
                }

                record.PayOrderNo = payOrderNo;
                record.PayTime = DateTime.Now;

                if (success)
                {
                    record.Status = RechargeStatus.Paid;
                    Logger.Info($"充值支付成功: {orderNo}, 第三方订单:{payOrderNo}");
                }
                else
                {
                    record.Status = RechargeStatus.Failed;
                    Logger.Info($"充值支付失败: {orderNo}");
                }

                await _repository.UpdateRechargeRecordAsync(record);
                return success;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"处理充值回调失败: {orderNo}");
                return false;
            }
        }

        public async Task<bool> DeliverRechargeAsync(string orderNo)
        {
            try
            {
                var record = await _repository.GetRechargeRecordByNoAsync(orderNo);
                if (record == null || record.Status != RechargeStatus.Paid)
                    return false;

                // 发放元宝到玩家账户
                bool goldAdded = await _repository.AddPlayerGameGoldAsync(
                    record.AccountId, 
                    record.CharName, 
                    record.TotalGold
                );

                if (!goldAdded)
                {
                    Logger.Warn($"充值元宝发放失败(角色可能不存在): {orderNo}, 账号:{record.AccountId}, 角色:{record.CharName}");
                    record.Remark = "元宝发放失败:角色不存在";
                }
                else
                {
                    Logger.Info($"充值元宝已发放: {orderNo}, 账号:{record.AccountId}, 角色:{record.CharName}, 元宝:{record.TotalGold}");
                }

                // 更新VIP
                await UpdateUserVipAsync(record.AccountId, record.TotalGold);

                // 更新状态
                record.Status = goldAdded ? RechargeStatus.Delivered : RechargeStatus.Failed;
                record.DeliverTime = DateTime.Now;
                await _repository.UpdateRechargeRecordAsync(record);

                Logger.Info($"充值发放完成: {orderNo}, 元宝:{record.TotalGold}, 状态:{record.Status}");
                return goldAdded;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"充值发放失败: {orderNo}");
                return false;
            }
        }

        public async Task<List<RechargeRecord>> GetRechargeRecordsAsync(string accountId, int page = 1, int pageSize = 20)
        {
            return await _repository.GetRechargeRecordsAsync(accountId, page, pageSize);
        }

        public async Task<bool> IsFirstRechargeAsync(string accountId)
        {
            return await _repository.IsFirstRechargeAsync(accountId);
        }

        public async Task<decimal> GetOrderAmountAsync(string orderNo)
        {
            return await _repository.GetOrderAmountAsync(orderNo);
        }

        public async Task<bool> IsOrderPaidAsync(string orderNo)
        {
            return await _repository.IsOrderPaidAsync(orderNo);
        }

        #endregion

        #region 礼包码

        public async Task<RedeemResult> RedeemGiftCodeAsync(string code, string accountId, string charName, int playerLevel, string clientIp)
        {
            try
            {
                code = code?.Trim().ToUpper();
                if (string.IsNullOrEmpty(code))
                    return RedeemResult.Fail("请输入礼包码");

                // 获取礼包码信息
                var giftCode = await _repository.GetGiftCodeAsync(code);
                if (giftCode == null)
                    return RedeemResult.Fail("礼包码不存在");

                if (!giftCode.IsValid)
                    return RedeemResult.Fail("礼包码已失效或已用完");

                // 检查等级限制
                if (giftCode.RequireLevel > playerLevel)
                    return RedeemResult.Fail($"需要达到{giftCode.RequireLevel}级才能使用");

                // 检查是否已使用
                if (giftCode.LimitOnePerAccount)
                {
                    bool used = await _repository.HasUsedGiftCodeAsync(code, accountId);
                    if (used)
                        return RedeemResult.Fail("您已使用过该礼包码");
                }

                // 解析物品
                List<GiftCodeItem> items = null;
                if (!string.IsNullOrEmpty(giftCode.Items))
                {
                    try
                    {
                        items = JsonSerializer.Deserialize<List<GiftCodeItem>>(giftCode.Items);
                    }
                    catch { }
                }

                // 记录使用
                var record = new GiftCodeRecord
                {
                    CodeId = giftCode.Id,
                    Code = code,
                    AccountId = accountId,
                    CharName = charName,
                    ClientIp = clientIp,
                    UseTime = DateTime.Now
                };
                await _repository.CreateGiftCodeRecordAsync(record);

                // 更新使用次数
                await _repository.IncrementGiftCodeUsedCountAsync(giftCode.Id);

                Logger.Info($"礼包码兑换成功: {charName} 使用 {code}, 获得元宝:{giftCode.GameGold}, 点数:{giftCode.GamePoint}");

                return RedeemResult.Ok(giftCode.GameGold, giftCode.GamePoint, items);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"礼包码兑换失败: {code}, {accountId}");
                return RedeemResult.Fail("兑换失败，请稍后重试");
            }
        }

        public async Task<(bool valid, string message)> CheckGiftCodeAsync(string code, string accountId)
        {
            code = code?.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
                return (false, "请输入礼包码");

            var giftCode = await _repository.GetGiftCodeAsync(code);
            if (giftCode == null)
                return (false, "礼包码不存在");

            if (!giftCode.IsValid)
                return (false, "礼包码已失效");

            if (giftCode.LimitOnePerAccount)
            {
                bool used = await _repository.HasUsedGiftCodeAsync(code, accountId);
                if (used)
                    return (false, "您已使用过该礼包码");
            }

            return (true, giftCode.Name);
        }

        public async Task<List<string>> GenerateGiftCodesAsync(string batchNo, string name, int gameGold, int gamePoint, string items, int count, DateTime? endTime)
        {
            var codes = new List<string>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                string code = GenerateRandomCode(12);

                var giftCode = new GiftCode
                {
                    Code = code,
                    BatchNo = batchNo,
                    Name = name,
                    GameGold = gameGold,
                    GamePoint = gamePoint,
                    Items = items,
                    MaxUseCount = 1,
                    LimitOnePerAccount = true,
                    EndTime = endTime,
                    IsEnabled = true,
                    CreateTime = DateTime.Now
                };

                await _repository.CreateGiftCodeAsync(giftCode);
                codes.Add(code);
            }

            Logger.Info($"生成礼包码: 批次{batchNo}, 数量{count}");
            return codes;
        }

        #endregion

        #region VIP

        public async Task<List<VipLevel>> GetVipLevelsAsync()
        {
            if (_vipLevels == null)
            {
                _vipLevels = await _repository.GetVipLevelsAsync();
            }
            return _vipLevels;
        }

        public async Task<UserVip> GetUserVipAsync(string accountId)
        {
            var vip = await _repository.GetUserVipAsync(accountId);
            if (vip == null)
            {
                vip = new UserVip
                {
                    AccountId = accountId,
                    VipLevel = 0,
                    TotalRecharge = 0
                };
            }
            return vip;
        }

        public async Task<int> UpdateUserVipAsync(string accountId, int rechargeGold)
        {
            var userVip = await GetUserVipAsync(accountId);
            userVip.TotalRecharge += rechargeGold;

            // 计算新VIP等级
            var levels = await GetVipLevelsAsync();
            int newLevel = 0;
            foreach (var level in levels.OrderByDescending(x => x.Level))
            {
                if (userVip.TotalRecharge >= level.RequireRecharge)
                {
                    newLevel = level.Level;
                    break;
                }
            }

            userVip.VipLevel = newLevel;
            await _repository.SaveUserVipAsync(userVip);

            Logger.Info($"更新VIP: {accountId}, 累计充值:{userVip.TotalRecharge}, VIP等级:{newLevel}");
            return newLevel;
        }

        public async Task<(bool success, string message)> ClaimVipDailyGiftAsync(string accountId, string charName)
        {
            var userVip = await GetUserVipAsync(accountId);
            if (userVip.VipLevel <= 0)
                return (false, "您还不是VIP会员");

            var today = DateTime.Today;
            if (userVip.LastDailyGiftTime.HasValue && userVip.LastDailyGiftTime.Value.Date == today)
                return (false, "今日已领取过VIP礼包");

            var levels = await GetVipLevelsAsync();
            var level = levels.FirstOrDefault(x => x.Level == userVip.VipLevel);
            if (level == null || string.IsNullOrEmpty(level.DailyGift))
                return (false, "当前VIP等级没有每日礼包");

            userVip.LastDailyGiftTime = today;
            await _repository.SaveUserVipAsync(userVip);

            Logger.Info($"领取VIP礼包: {charName}, VIP{userVip.VipLevel}");
            return (true, level.DailyGift);
        }

        public int GetVipPrice(int originalPrice, int vipLevel)
        {
            if (vipLevel <= 0)
                return originalPrice;

            var levels = _vipLevels ?? GetVipLevelsAsync().Result;
            var level = levels.FirstOrDefault(x => x.Level == vipLevel);
            if (level == null || level.ShopDiscount >= 100)
                return originalPrice;

            return (int)Math.Ceiling(originalPrice * level.ShopDiscount / 100.0);
        }

        #endregion

        #region 工具方法

        private string GenerateOrderNo(string prefix)
        {
            return $"{prefix}{DateTime.Now:yyyyMMddHHmmssfff}{new Random().Next(1000, 9999)}";
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion
    }
}
