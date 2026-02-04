using System.Collections.Generic;
using System.Threading.Tasks;
using ShopModule.Models;

namespace ShopModule.Repository
{
    /// <summary>
    /// 商城数据仓储接口
    /// </summary>
    public interface IShopRepository
    {
        #region 分类

        Task<List<ShopCategory>> GetCategoriesAsync();

        #endregion

        #region 商品

        Task<List<ShopItem>> GetItemsByCategoryAsync(int categoryId);
        Task<List<ShopItem>> GetHotItemsAsync(int limit);
        Task<List<ShopItem>> GetNewItemsAsync(int limit);
        Task<List<ShopItem>> GetRecommendItemsAsync(int limit);
        Task<List<ShopItem>> SearchItemsAsync(string keyword);
        Task<ShopItem> GetItemAsync(int itemId);
        Task UpdateStockAsync(int itemId, int delta);
        Task UpdateSoldCountAsync(int itemId, int delta);

        #endregion

        #region 订单

        Task CreateOrderAsync(ShopOrder order);
        Task<ShopOrder> GetOrderByNoAsync(string orderNo);
        Task UpdateOrderAsync(ShopOrder order);
        Task<List<ShopOrder>> GetUserOrdersAsync(string accountId, int page, int pageSize);
        Task<int> GetUserTodayBuyCountAsync(string accountId, int shopItemId);
        Task<int> GetUserTotalBuyCountAsync(string accountId, int shopItemId);

        #endregion

        #region 充值

        Task<List<RechargePackage>> GetRechargePackagesAsync();
        Task<RechargePackage> GetRechargePackageAsync(int packageId);
        Task CreateRechargeRecordAsync(RechargeRecord record);
        Task<RechargeRecord> GetRechargeRecordByNoAsync(string orderNo);
        Task UpdateRechargeRecordAsync(RechargeRecord record);
        Task<List<RechargeRecord>> GetRechargeRecordsAsync(string accountId, int page, int pageSize);
        Task<bool> IsFirstRechargeAsync(string accountId);

        /// <summary>
        /// 获取订单金额
        /// </summary>
        Task<decimal> GetOrderAmountAsync(string orderNo);

        /// <summary>
        /// 检查订单是否已支付
        /// </summary>
        Task<bool> IsOrderPaidAsync(string orderNo);

        /// <summary>
        /// 增加玩家元宝（直接更新数据库）
        /// </summary>
        Task<bool> AddPlayerGameGoldAsync(string accountId, string charName, int gold);

        /// <summary>
        /// 获取玩家当前元宝
        /// </summary>
        Task<int> GetPlayerGameGoldAsync(string accountId, string charName);

        #endregion

        #region 礼包码

        Task<GiftCode> GetGiftCodeAsync(string code);
        Task CreateGiftCodeAsync(GiftCode giftCode);
        Task IncrementGiftCodeUsedCountAsync(long codeId);
        Task<bool> HasUsedGiftCodeAsync(string code, string accountId);
        Task CreateGiftCodeRecordAsync(GiftCodeRecord record);

        #endregion

        #region VIP

        Task<List<VipLevel>> GetVipLevelsAsync();
        Task<UserVip> GetUserVipAsync(string accountId);
        Task SaveUserVipAsync(UserVip userVip);

        #endregion
    }
}
