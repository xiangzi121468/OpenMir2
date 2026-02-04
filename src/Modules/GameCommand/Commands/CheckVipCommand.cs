using GameCommand.Attributes;
using M2Server.Player;
using SystemModule;
using SystemModule.Enums;
using VipModule;

namespace GameCommand.Commands
{
    /// <summary>
    /// 检查VIP状态命令
    /// 用法: @VIP
    /// 显示当前VIP等级和权益
    /// </summary>
    [Command("Vip", "VIP", "查看VIP等级和权益", 0)]
    public class CheckVipCommand : BaseCommand
    {
        [DefaultCommand]
        public async Task Execute(string[] @params, PlayObject playObject)
        {
            if (playObject == null) return;
            
            try
            {
                // 从数据库获取充值信息
                if (string.IsNullOrEmpty(SystemShare.Config.DBConnection))
                {
                    playObject.SysMsg("VIP系统暂不可用", MsgColor.Red, MsgType.Hint);
                    return;
                }
                
                var vipService = new VipMapService(SystemShare.Config.DBConnection);
                
                // 获取累计充值金额（从充值记录表统计）
                int totalRecharge = await GetTotalRechargeAsync(playObject.Account);
                int vipLevel = vipService.GetVipLevelByRecharge(totalRecharge);
                
                string vipName = vipLevel switch
                {
                    0 => "普通玩家",
                    1 => "VIP1 (充值30+)",
                    2 => "VIP2 (充值100+)", 
                    3 => "VIP3 (充值500+)",
                    4 => "VIP4 (充值1000+)",
                    5 => "VIP5 (充值5000+)",
                    _ => $"VIP{vipLevel}"
                };
                
                playObject.SysMsg($"═══════════════════", MsgColor.Green, MsgType.Hint);
                playObject.SysMsg($"【VIP信息】", MsgColor.Yellow, MsgType.Hint);
                playObject.SysMsg($"账号: {playObject.Account}", MsgColor.White, MsgType.Hint);
                playObject.SysMsg($"累计充值: {totalRecharge} 元", MsgColor.White, MsgType.Hint);
                playObject.SysMsg($"VIP等级: {vipName}", MsgColor.Yellow, MsgType.Hint);
                playObject.SysMsg($"═══════════════════", MsgColor.Green, MsgType.Hint);
                
                // 显示VIP权益
                if (vipLevel >= 1)
                {
                    playObject.SysMsg("【VIP权益】", MsgColor.Yellow, MsgType.Hint);
                    playObject.SysMsg("- 进入VIP专属地图", MsgColor.White, MsgType.Hint);
                    playObject.SysMsg("- 商城物品折扣", MsgColor.White, MsgType.Hint);
                }
                if (vipLevel >= 2)
                {
                    playObject.SysMsg("- 经验加成10%", MsgColor.White, MsgType.Hint);
                }
                if (vipLevel >= 3)
                {
                    playObject.SysMsg("- 掉落加成10%", MsgColor.White, MsgType.Hint);
                    playObject.SysMsg("- 专属称号", MsgColor.White, MsgType.Hint);
                }
            }
            catch (Exception ex)
            {
                playObject.SysMsg("查询VIP信息失败", MsgColor.Red, MsgType.Hint);
                LogService.Error($"VIP命令执行失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取累计充值金额
        /// </summary>
        private async Task<int> GetTotalRechargeAsync(string accountId)
        {
            try
            {
                using var conn = new MySqlConnector.MySqlConnection(SystemShare.Config.DBConnection);
                await conn.OpenAsync();
                
                using var cmd = new MySqlConnector.MySqlCommand(
                    "SELECT COALESCE(SUM(Amount), 0) FROM recharge_records WHERE AccountId = @AccountId AND Status = 'success'", 
                    conn);
                cmd.Parameters.AddWithValue("@AccountId", accountId);
                
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                return 0;
            }
        }
    }
}
