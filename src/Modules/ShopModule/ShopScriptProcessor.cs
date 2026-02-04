using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;
using OpenMir2.Packets.ClientPackets;
using ShopModule.Models;
using SystemModule;
using SystemModule.Actors;
using SystemModule.Enums;

namespace ShopModule
{
    /// <summary>
    /// 商城脚本处理器
    /// </summary>
    public class ShopScriptProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IShopService _shopService;

        public ShopScriptProcessor(IShopService shopService)
        {
            _shopService = shopService;
        }

        /// <summary>
        /// 给玩家发放物品
        /// </summary>
        /// <param name="player">玩家对象</param>
        /// <param name="itemName">物品名称</param>
        /// <param name="count">数量</param>
        /// <returns>成功发放的数量</returns>
        private int GiveItemToPlayer(IPlayerActor player, string itemName, int count)
        {
            int successCount = 0;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    UserItem userItem = new UserItem();
                    if (SystemShare.ItemSystem.CopyToUserItemFromName(itemName, ref userItem))
                    {
                        if (player.AddItemToBag(userItem))
                        {
                            player.SendAddItem(userItem);
                            successCount++;
                        }
                        else
                        {
                            // 背包已满
                            player.SysMsg("背包已满，部分物品无法发放", MsgColor.Red, MsgType.Hint);
                            break;
                        }
                    }
                    else
                    {
                        Logger.Warn($"物品不存在: {itemName}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"发放物品失败: {player.ChrName}, {itemName}, {count}");
            }
            return successCount;
        }

        /// <summary>
        /// 购买商城商品
        /// </summary>
        public async Task<bool> BuyShopItemAsync(IPlayerActor player, int shopItemId, int buyCount = 1)
        {
            try
            {
                var vip = await _shopService.GetUserVipAsync(player.AccountId);

                var request = new BuyRequest
                {
                    AccountId = player.AccountId,
                    CharName = player.ChrName,
                    ShopItemId = shopItemId,
                    BuyCount = buyCount
                };

                var result = await _shopService.BuyItemAsync(request, player.GameGold, vip?.VipLevel ?? 0);

                if (!result.Success)
                {
                    player.SysMsg(result.Message, MsgColor.Red, MsgType.Hint);
                    return false;
                }

                // 扣除元宝
                player.GameGold = result.RemainGold;
                player.GameGoldChanged();

                // 获取商品信息并发放物品
                var item = await _shopService.GetItemAsync(shopItemId);
                if (item != null)
                {
                    int totalCount = item.ItemCount * buyCount;
                    int givenCount = GiveItemToPlayer(player, item.ItemName, totalCount);

                    // 标记订单已发货
                    await _shopService.DeliverOrderAsync(result.OrderNo);

                    if (givenCount > 0)
                    {
                        player.SysMsg($"购买成功! 获得 {item.DisplayName} x{givenCount}", MsgColor.Green, MsgType.Hint);
                        Logger.Info($"商城购买: {player.ChrName} 购买 {item.DisplayName} x{givenCount}, 订单:{result.OrderNo}");
                    }
                    if (givenCount < totalCount)
                    {
                        Logger.Warn($"商城购买物品未全部发放: {player.ChrName}, {item.ItemName}, 应发:{totalCount}, 实发:{givenCount}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"购买商品失败: {player.ChrName} -> {shopItemId}");
                player.SysMsg("购买失败，请稍后重试", MsgColor.Red, MsgType.Hint);
                return false;
            }
        }

        /// <summary>
        /// 兑换礼包码
        /// </summary>
        public async Task<bool> RedeemGiftCodeAsync(IPlayerActor player, string code)
        {
            try
            {
                var result = await _shopService.RedeemGiftCodeAsync(
                    code,
                    player.AccountId,
                    player.ChrName,
                    player.Abil.Level,
                    "" // clientIp
                );

                if (!result.Success)
                {
                    player.SysMsg(result.Message, MsgColor.Red, MsgType.Hint);
                    return false;
                }

                // 发放元宝
                if (result.GameGold > 0)
                {
                    player.GameGold += result.GameGold;
                    player.GameGoldChanged();
                    player.SysMsg($"获得 {result.GameGold} 元宝", MsgColor.Green, MsgType.Hint);
                }

                // 发放点数
                if (result.GamePoint > 0)
                {
                    player.GamePoint += result.GamePoint;
                    player.SysMsg($"获得 {result.GamePoint} 点数", MsgColor.Green, MsgType.Hint);
                }

                // 发放物品
                if (result.Items != null && result.Items.Count > 0)
                {
                    foreach (var item in result.Items)
                    {
                        int givenCount = GiveItemToPlayer(player, item.Name, item.Count);
                        if (givenCount > 0)
                        {
                            player.SysMsg($"获得 {item.Name} x{givenCount}", MsgColor.Green, MsgType.Hint);
                        }
                    }
                }

                player.SysMsg("礼包兑换成功!", MsgColor.Green, MsgType.Hint);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"兑换礼包码失败: {player.ChrName} -> {code}");
                player.SysMsg("兑换失败，请稍后重试", MsgColor.Red, MsgType.Hint);
                return false;
            }
        }

        /// <summary>
        /// 领取VIP每日礼包
        /// </summary>
        public async Task<bool> ClaimVipDailyGiftAsync(IPlayerActor player)
        {
            try
            {
                var (success, message) = await _shopService.ClaimVipDailyGiftAsync(player.AccountId, player.ChrName);

                if (!success)
                {
                    player.SysMsg(message, MsgColor.Red, MsgType.Hint);
                    return false;
                }

                // message 包含礼包内容JSON，解析并发放
                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        var gifts = JsonSerializer.Deserialize<List<GiftCodeItem>>(message);
                        if (gifts != null)
                        {
                            foreach (var gift in gifts)
                            {
                                int givenCount = GiveItemToPlayer(player, gift.Name, gift.Count);
                                if (givenCount > 0)
                                {
                                    player.SysMsg($"获得 {gift.Name} x{givenCount}", MsgColor.Green, MsgType.Hint);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"解析VIP礼包内容失败: {message}");
                    }
                }

                player.SysMsg("VIP每日礼包领取成功!", MsgColor.Green, MsgType.Hint);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"领取VIP礼包失败: {player.ChrName}");
                player.SysMsg("领取失败，请稍后重试", MsgColor.Red, MsgType.Hint);
                return false;
            }
        }

        /// <summary>
        /// 获取VIP等级
        /// </summary>
        public async Task<int> GetVipLevelAsync(IPlayerActor player)
        {
            var vip = await _shopService.GetUserVipAsync(player.AccountId);
            return vip?.VipLevel ?? 0;
        }

        /// <summary>
        /// 获取VIP折扣价格
        /// </summary>
        public int GetVipPrice(int originalPrice, int vipLevel)
        {
            return _shopService.GetVipPrice(originalPrice, vipLevel);
        }

        /// <summary>
        /// 检查是否首充
        /// </summary>
        public async Task<bool> IsFirstRechargeAsync(IPlayerActor player)
        {
            return await _shopService.IsFirstRechargeAsync(player.AccountId);
        }

        /// <summary>
        /// 获取累计充值
        /// </summary>
        public async Task<int> GetTotalRechargeAsync(IPlayerActor player)
        {
            var vip = await _shopService.GetUserVipAsync(player.AccountId);
            return vip?.TotalRecharge ?? 0;
        }

        /// <summary>
        /// 获取VIP经验加成
        /// </summary>
        public async Task<int> GetVipExpBonusAsync(IPlayerActor player)
        {
            var vip = await _shopService.GetUserVipAsync(player.AccountId);
            if (vip == null || vip.VipLevel <= 0)
                return 0;

            var levels = await _shopService.GetVipLevelsAsync();
            var level = levels.Find(x => x.Level == vip.VipLevel);
            return level?.ExpBonus ?? 0;
        }

        /// <summary>
        /// 获取VIP掉落加成
        /// </summary>
        public async Task<int> GetVipDropBonusAsync(IPlayerActor player)
        {
            var vip = await _shopService.GetUserVipAsync(player.AccountId);
            if (vip == null || vip.VipLevel <= 0)
                return 0;

            var levels = await _shopService.GetVipLevelsAsync();
            var level = levels.Find(x => x.Level == vip.VipLevel);
            return level?.DropBonus ?? 0;
        }
    }
}
