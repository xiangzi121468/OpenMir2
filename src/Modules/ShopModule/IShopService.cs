using System.Collections.Generic;
using System.Threading.Tasks;
using ShopModule.Models;

namespace ShopModule
{
    /// <summary>
    /// 商城服务接口
    /// </summary>
    public interface IShopService
    {
        #region 商品管理

        /// <summary>
        /// 获取所有分类
        /// </summary>
        Task<List<ShopCategory>> GetCategoriesAsync();

        /// <summary>
        /// 获取分类下的商品
        /// </summary>
        Task<List<ShopItem>> GetItemsByCategoryAsync(int categoryId);

        /// <summary>
        /// 获取热销商品
        /// </summary>
        Task<List<ShopItem>> GetHotItemsAsync(int limit = 10);

        /// <summary>
        /// 获取新品
        /// </summary>
        Task<List<ShopItem>> GetNewItemsAsync(int limit = 10);

        /// <summary>
        /// 获取推荐商品
        /// </summary>
        Task<List<ShopItem>> GetRecommendItemsAsync(int limit = 10);

        /// <summary>
        /// 搜索商品
        /// </summary>
        Task<List<ShopItem>> SearchItemsAsync(string keyword);

        /// <summary>
        /// 获取商品详情
        /// </summary>
        Task<ShopItem> GetItemAsync(int itemId);

        #endregion

        #region 购买

        /// <summary>
        /// 购买商品
        /// </summary>
        Task<BuyResult> BuyItemAsync(BuyRequest request, int currentGold, int vipLevel);

        /// <summary>
        /// 发货(给玩家物品)
        /// </summary>
        Task<bool> DeliverOrderAsync(string orderNo);

        /// <summary>
        /// 获取用户购买记录
        /// </summary>
        Task<List<ShopOrder>> GetUserOrdersAsync(string accountId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 获取用户今日购买某商品数量
        /// </summary>
        Task<int> GetUserTodayBuyCountAsync(string accountId, int shopItemId);

        /// <summary>
        /// 获取用户累计购买某商品数量
        /// </summary>
        Task<int> GetUserTotalBuyCountAsync(string accountId, int shopItemId);

        #endregion

        #region 充值

        /// <summary>
        /// 获取充值档位
        /// </summary>
        Task<List<RechargePackage>> GetRechargePackagesAsync();

        /// <summary>
        /// 创建充值订单
        /// </summary>
        Task<RechargeRecord> CreateRechargeOrderAsync(string accountId, string charName, int packageId, string payChannel, string clientIp);

        /// <summary>
        /// 支付回调处理
        /// </summary>
        Task<bool> HandlePayCallbackAsync(string orderNo, string payOrderNo, bool success);

        /// <summary>
        /// 发放充值元宝
        /// </summary>
        Task<bool> DeliverRechargeAsync(string orderNo);

        /// <summary>
        /// 获取充值记录
        /// </summary>
        Task<List<RechargeRecord>> GetRechargeRecordsAsync(string accountId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 检查是否首充
        /// </summary>
        Task<bool> IsFirstRechargeAsync(string accountId);

        /// <summary>
        /// 获取订单金额
        /// </summary>
        Task<decimal> GetOrderAmountAsync(string orderNo);

        /// <summary>
        /// 检查订单是否已支付
        /// </summary>
        Task<bool> IsOrderPaidAsync(string orderNo);

        #endregion

        #region 礼包码

        /// <summary>
        /// 兑换礼包码
        /// </summary>
        Task<RedeemResult> RedeemGiftCodeAsync(string code, string accountId, string charName, int playerLevel, string clientIp);

        /// <summary>
        /// 检查礼包码是否可用
        /// </summary>
        Task<(bool valid, string message)> CheckGiftCodeAsync(string code, string accountId);

        /// <summary>
        /// 批量生成礼包码
        /// </summary>
        Task<List<string>> GenerateGiftCodesAsync(string batchNo, string name, int gameGold, int gamePoint, string items, int count, DateTime? endTime);

        #endregion

        #region VIP

        /// <summary>
        /// 获取VIP等级配置
        /// </summary>
        Task<List<VipLevel>> GetVipLevelsAsync();

        /// <summary>
        /// 获取用户VIP信息
        /// </summary>
        Task<UserVip> GetUserVipAsync(string accountId);

        /// <summary>
        /// 更新用户VIP(充值后调用)
        /// </summary>
        Task<int> UpdateUserVipAsync(string accountId, int rechargeGold);

        /// <summary>
        /// 领取VIP每日礼包
        /// </summary>
        Task<(bool success, string message)> ClaimVipDailyGiftAsync(string accountId, string charName);

        /// <summary>
        /// 获取VIP折扣后价格
        /// </summary>
        int GetVipPrice(int originalPrice, int vipLevel);

        #endregion
    }
}
