using GameCommand.Attributes;
using M2Server.Player;
using SystemModule;
using SystemModule.Enums;
using AppraisalModule;
using AppraisalModule.Models;

namespace GameCommand.Commands
{
    /// <summary>
    /// 装备鉴定命令
    /// 用法: @鉴定 [卷轴等级]
    /// 对当前装备栏中的武器进行鉴定
    /// </summary>
    [Command("Appraisal", "鉴定", "装备鉴定，为武器附加特殊属性", 10)]
    public class AppraisalCommand : BaseCommand
    {
        private static AppraisalService? _service;
        
        [DefaultCommand]
        public async Task Execute(string[] @params, PlayObject playObject)
        {
            if (playObject == null) return;
            
            // 获取卷轴等级参数，默认为1
            int scrollLevel = 1;
            if (@params.Length > 0 && int.TryParse(@params[0], out int level))
            {
                scrollLevel = Math.Clamp(level, 1, 3);
            }
            
            // 检查是否装备了武器
            var weapon = playObject.UseItems[0]; // 武器槽位
            if (weapon == null || string.IsNullOrEmpty(weapon.Index.ToString()) || weapon.Index == 0)
            {
                playObject.SysMsg("请先装备需要鉴定的武器", MsgColor.Red, MsgType.Hint);
                return;
            }
            
            // 初始化鉴定服务
            if (_service == null && !string.IsNullOrEmpty(SystemShare.Config.DBConnection))
            {
                _service = new AppraisalService(SystemShare.Config.DBConnection);
            }
            
            if (_service == null)
            {
                playObject.SysMsg("鉴定系统暂不可用", MsgColor.Red, MsgType.Hint);
                return;
            }
            
            try
            {
                // 检查是否有幸运符（提高成功率）
                bool hasLuckyCharm = false;
                for (int i = 0; i < playObject.ItemList.Count; i++)
                {
                    var item = playObject.ItemList[i];
                    if (item?.UserItem?.Index > 0)
                    {
                        var stdItem = SystemShare.ItemSystem.GetStdItem(item.UserItem.Index);
                        if (stdItem != null && stdItem.Name.Contains("幸运符"))
                        {
                            hasLuckyCharm = true;
                            break;
                        }
                    }
                }
                
                // 执行鉴定
                var result = await _service.AppraiseItemAsync(
                    playObject.ChrName,
                    weapon.Index,
                    weapon.MakeIndex,
                    scrollLevel,
                    hasLuckyCharm);
                
                if (result.Success)
                {
                    // 鉴定成功
                    playObject.SysMsg($"鉴定成功！获得属性: {result.AttributeName} Lv.{result.AttributeLevel}", MsgColor.Green, MsgType.Hint);
                    
                    // 保存鉴定属性到装备的扩展字段
                    // weapon.Desc[0] = (byte)result.AttributeId;
                    // weapon.Desc[1] = (byte)result.AttributeLevel;
                    
                    // 如果是稀有属性，全服广播
                    if (result.IsRare)
                    {
                        var msg = $"【全服公告】玩家 {playObject.ChrName} 鉴定出稀有属性【{result.AttributeName}】！";
                        SystemShare.WorldEngine.SendBroadCastMsg(msg, MsgType.System);
                    }
                    
                    // 更新装备显示
                    playObject.RecalcAbilitys();
                    playObject.SendMsg(Messages.RM_ABILITY, 0, 0, 0, 0);
                }
                else
                {
                    // 鉴定失败
                    playObject.SysMsg($"鉴定失败: {result.Message}", MsgColor.Red, MsgType.Hint);
                }
            }
            catch (Exception ex)
            {
                playObject.SysMsg("鉴定过程出现错误", MsgColor.Red, MsgType.Hint);
                LogService.Error($"鉴定命令执行失败: {ex.Message}");
            }
        }
    }
}
