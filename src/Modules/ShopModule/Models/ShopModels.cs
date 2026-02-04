using System;
using System.Collections.Generic;

namespace ShopModule.Models
{
    /// <summary>
    /// 商品分类
    /// </summary>
    public class ShopCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// 商城商品
    /// </summary>
    public class ShopItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int Price { get; set; }
        public int OriginalPrice { get; set; }
        public int ItemCount { get; set; }
        public int Stock { get; set; }
        public int SoldCount { get; set; }
        public int LimitPerUser { get; set; }
        public int LimitPerDay { get; set; }
        public int RequireLevel { get; set; }
        public int RequireVip { get; set; }
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }
        public bool IsRecommend { get; set; }
        public int SortOrder { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否在销售时间内
        /// </summary>
        public bool IsOnSale
        {
            get
            {
                if (!IsEnabled) return false;
                var now = DateTime.Now;
                if (StartTime.HasValue && now < StartTime.Value) return false;
                if (EndTime.HasValue && now > EndTime.Value) return false;
                return true;
            }
        }

        /// <summary>
        /// 是否有库存
        /// </summary>
        public bool HasStock => Stock == -1 || Stock > 0;

        /// <summary>
        /// 折扣率(0-100)
        /// </summary>
        public int DiscountRate => OriginalPrice > 0 ? (int)(Price * 100.0 / OriginalPrice) : 100;
    }

    /// <summary>
    /// 购买订单
    /// </summary>
    public class ShopOrder
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public int ShopItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemCount { get; set; }
        public int UnitPrice { get; set; }
        public int TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? DeliverTime { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 0,    // 待发货
        Delivered = 1,  // 已发货
        Cancelled = 2,  // 已取消
        Refunded = 3    // 已退款
    }

    /// <summary>
    /// 充值记录
    /// </summary>
    public class RechargeRecord
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public decimal Amount { get; set; }
        public int GameGold { get; set; }
        public int BonusGold { get; set; }
        public string PayChannel { get; set; }
        public string PayOrderNo { get; set; }
        public RechargeStatus Status { get; set; }
        public DateTime? PayTime { get; set; }
        public DateTime? DeliverTime { get; set; }
        public string ClientIp { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 获得总元宝
        /// </summary>
        public int TotalGold => GameGold + BonusGold;
    }

    public enum RechargeStatus
    {
        Pending = 0,    // 待支付
        Paid = 1,       // 已支付
        Delivered = 2,  // 已发放
        Failed = 3,     // 失败
        Refunded = 4    // 已退款
    }

    /// <summary>
    /// 充值档位
    /// </summary>
    public class RechargePackage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int GameGold { get; set; }
        public int BonusGold { get; set; }
        public int ExtraBonusGold { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsHot { get; set; }

        /// <summary>
        /// 首充总元宝
        /// </summary>
        public int FirstRechargeTotal => GameGold + BonusGold;

        /// <summary>
        /// 非首充总元宝
        /// </summary>
        public int NormalTotal => GameGold;
    }

    /// <summary>
    /// 礼包码
    /// </summary>
    public class GiftCode
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string BatchNo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int GameGold { get; set; }
        public int GamePoint { get; set; }
        public string Items { get; set; } // JSON
        public int MaxUseCount { get; set; }
        public int UsedCount { get; set; }
        public bool LimitOnePerAccount { get; set; }
        public int RequireLevel { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!IsEnabled) return false;
                if (UsedCount >= MaxUseCount) return false;
                var now = DateTime.Now;
                if (StartTime.HasValue && now < StartTime.Value) return false;
                if (EndTime.HasValue && now > EndTime.Value) return false;
                return true;
            }
        }
    }

    /// <summary>
    /// 礼包码物品
    /// </summary>
    public class GiftCodeItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// 礼包码使用记录
    /// </summary>
    public class GiftCodeRecord
    {
        public long Id { get; set; }
        public long CodeId { get; set; }
        public string Code { get; set; }
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public string ClientIp { get; set; }
        public DateTime UseTime { get; set; }
    }

    /// <summary>
    /// VIP等级配置
    /// </summary>
    public class VipLevel
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public int RequireRecharge { get; set; }
        public int ExpBonus { get; set; }
        public int DropBonus { get; set; }
        public int ShopDiscount { get; set; }
        public string DailyGift { get; set; }
        public string Privileges { get; set; }
        public string Icon { get; set; }
    }

    /// <summary>
    /// 用户VIP信息
    /// </summary>
    public class UserVip
    {
        public string AccountId { get; set; }
        public int VipLevel { get; set; }
        public int TotalRecharge { get; set; }
        public DateTime? LastDailyGiftTime { get; set; }
    }

    /// <summary>
    /// 购买请求
    /// </summary>
    public class BuyRequest
    {
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public int ShopItemId { get; set; }
        public int BuyCount { get; set; } = 1;
    }

    /// <summary>
    /// 购买结果
    /// </summary>
    public class BuyResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OrderNo { get; set; }
        public int CostGold { get; set; }
        public int RemainGold { get; set; }

        public static BuyResult Fail(string msg) => new BuyResult { Success = false, Message = msg };
        public static BuyResult Ok(string orderNo, int cost, int remain) => new BuyResult
        {
            Success = true,
            Message = "购买成功",
            OrderNo = orderNo,
            CostGold = cost,
            RemainGold = remain
        };
    }

    /// <summary>
    /// 礼包码兑换结果
    /// </summary>
    public class RedeemResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int GameGold { get; set; }
        public int GamePoint { get; set; }
        public List<GiftCodeItem> Items { get; set; }

        public static RedeemResult Fail(string msg) => new RedeemResult { Success = false, Message = msg };
        public static RedeemResult Ok(int gold, int point, List<GiftCodeItem> items) => new RedeemResult
        {
            Success = true,
            Message = "兑换成功",
            GameGold = gold,
            GamePoint = point,
            Items = items ?? new List<GiftCodeItem>()
        };
    }
}
