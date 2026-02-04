namespace ScriptSystem.Consts
{
    /// <summary>
    /// 商城相关脚本命令编码
    /// </summary>
    public enum ShopScriptCode : short
    {
        /// <summary>
        /// 打开商城界面
        /// OPENSHOP [分类ID]
        /// </summary>
        [ScriptDefName("OPENSHOP")]
        OpenShop = 1000,

        /// <summary>
        /// 购买商城商品
        /// BUYSHOPITEM 商品ID [数量]
        /// </summary>
        [ScriptDefName("BUYSHOPITEM")]
        BuyShopItem = 1001,

        /// <summary>
        /// 兑换礼包码
        /// REDEEMCODE 变量(存放礼包码)
        /// </summary>
        [ScriptDefName("REDEEMCODE")]
        RedeemCode = 1002,

        /// <summary>
        /// 检查VIP等级
        /// CHECKVIPLEVEL >= 等级
        /// </summary>
        [ScriptDefName("CHECKVIPLEVEL")]
        CheckVipLevel = 1003,

        /// <summary>
        /// 领取VIP每日礼包
        /// CLAIMVIPDAILY
        /// </summary>
        [ScriptDefName("CLAIMVIPDAILY")]
        ClaimVipDaily = 1004,

        /// <summary>
        /// 获取VIP折扣价格
        /// GETVIPPRICE 原价 结果变量
        /// </summary>
        [ScriptDefName("GETVIPPRICE")]
        GetVipPrice = 1005,

        /// <summary>
        /// 检查是否首充
        /// CHECKFIRSTRECHARGE
        /// </summary>
        [ScriptDefName("CHECKFIRSTRECHARGE")]
        CheckFirstRecharge = 1006,

        /// <summary>
        /// 打开充值界面
        /// OPENRECHARGE
        /// </summary>
        [ScriptDefName("OPENRECHARGE")]
        OpenRecharge = 1007,

        /// <summary>
        /// 获取用户累计充值
        /// GETTOTALRECHARGE 结果变量
        /// </summary>
        [ScriptDefName("GETTOTALRECHARGE")]
        GetTotalRecharge = 1008,

        /// <summary>
        /// 给予VIP经验加成
        /// GIVEVIPEXPBONUS
        /// </summary>
        [ScriptDefName("GIVEVIPEXPBONUS")]
        GiveVipExpBonus = 1009
    }

    /// <summary>
    /// 商城相关条件命令
    /// </summary>
    public enum ShopConditionCode : short
    {
        /// <summary>
        /// 检查元宝是否足够
        /// CHECKGAMEGOLD >= 数量
        /// </summary>
        [ScriptDefName("CHECKGAMEGOLD")]
        CheckGameGold = 2000,

        /// <summary>
        /// 检查VIP等级
        /// CHECKVIP >= 等级
        /// </summary>
        [ScriptDefName("CHECKVIP")]
        CheckVip = 2001,

        /// <summary>
        /// 检查是否首充
        /// ISFIRSTRECHARGE
        /// </summary>
        [ScriptDefName("ISFIRSTRECHARGE")]
        IsFirstRecharge = 2002,

        /// <summary>
        /// 检查礼包码是否可用
        /// CHECKGIFTCODE 礼包码
        /// </summary>
        [ScriptDefName("CHECKGIFTCODE")]
        CheckGiftCode = 2003,

        /// <summary>
        /// 检查今日是否领取VIP礼包
        /// CHECKVIPGIFTTODAY
        /// </summary>
        [ScriptDefName("CHECKVIPGIFTTODAY")]
        CheckVipGiftToday = 2004
    }
}
