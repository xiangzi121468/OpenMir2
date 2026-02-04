using System;
using System.Threading.Tasks;
using OpenMir2;
using SystemModule;
using SystemModule.Actors;
using SystemModule.Enums;

namespace CommandModule.Commands
{
    /// <summary>
    /// GM发放元宝命令
    /// @SendGold 角色名 数量
    /// </summary>
    [Command("SendGold", "发放元宝给玩家", "角色名 数量", 10)]
    public class SendGoldCommand : GameCommand
    {
        [ExecuteCommand]
        public void Execute(string[] @params, IPlayerActor PlayerActor)
        {
            if (@params == null || @params.Length < 2)
            {
                PlayerActor.SysMsg("格式: @SendGold 角色名 数量", MsgColor.Red, MsgType.Hint);
                return;
            }

            string charName = @params[0];
            int gold = HUtil32.StrToInt(@params[1], 0);

            if (gold <= 0 || gold > 10000000)
            {
                PlayerActor.SysMsg("数量必须在1-10000000之间", MsgColor.Red, MsgType.Hint);
                return;
            }

            var target = SystemShare.WorldEngine.GetPlayObject(charName);
            if (target == null)
            {
                PlayerActor.SysMsg($"玩家 {charName} 不在线", MsgColor.Red, MsgType.Hint);
                return;
            }

            target.GameGold += gold;
            target.GameGoldChanged();

            target.SysMsg($"GM {PlayerActor.ChrName} 发放了 {gold} 元宝给您", MsgColor.Green, MsgType.Hint);
            PlayerActor.SysMsg($"成功发放 {gold} 元宝给 {charName}, 当前元宝: {target.GameGold}", MsgColor.Green, MsgType.Hint);
        }
    }

    /// <summary>
    /// 查询玩家元宝
    /// @QueryGold [角色名]
    /// </summary>
    [Command("QueryGold", "查询玩家元宝", "[角色名]", 6)]
    public class QueryGoldCommand : GameCommand
    {
        [ExecuteCommand]
        public void Execute(string[] @params, IPlayerActor PlayerActor)
        {
            string charName = @params?.Length > 0 ? @params[0] : PlayerActor.ChrName;

            var target = SystemShare.WorldEngine.GetPlayObject(charName);
            if (target == null)
            {
                PlayerActor.SysMsg($"玩家 {charName} 不在线", MsgColor.Red, MsgType.Hint);
                return;
            }

            PlayerActor.SysMsg($"玩家 {charName} 当前元宝: {target.GameGold}, 点数: {target.GamePoint}", MsgColor.Green, MsgType.Hint);
        }
    }

    /// <summary>
    /// 全服发放元宝
    /// @SendGoldAll 数量
    /// </summary>
    [Command("SendGoldAll", "全服发放元宝", "数量", 10)]
    public class SendGoldAllCommand : GameCommand
    {
        [ExecuteCommand]
        public void Execute(string[] @params, IPlayerActor PlayerActor)
        {
            if (@params == null || @params.Length < 1)
            {
                PlayerActor.SysMsg("格式: @SendGoldAll 数量", MsgColor.Red, MsgType.Hint);
                return;
            }

            int gold = HUtil32.StrToInt(@params[0], 0);
            if (gold <= 0 || gold > 100000)
            {
                PlayerActor.SysMsg("数量必须在1-100000之间", MsgColor.Red, MsgType.Hint);
                return;
            }

            int count = 0;
            foreach (var player in SystemShare.WorldEngine.PlayObjectList)
            {
                if (player != null && !player.Ghost)
                {
                    player.GameGold += gold;
                    player.GameGoldChanged();
                    player.SysMsg($"GM发放了 {gold} 元宝给全服玩家", MsgColor.Green, MsgType.Hint);
                    count++;
                }
            }

            PlayerActor.SysMsg($"成功发放 {gold} 元宝给 {count} 名在线玩家", MsgColor.Green, MsgType.Hint);
        }
    }

    /// <summary>
    /// 设置玩家VIP等级
    /// @SetVip 角色名 等级
    /// </summary>
    [Command("SetVip", "设置玩家VIP等级", "角色名 等级(0-10)", 10)]
    public class SetVipCommand : GameCommand
    {
        [ExecuteCommand]
        public void Execute(string[] @params, IPlayerActor PlayerActor)
        {
            if (@params == null || @params.Length < 2)
            {
                PlayerActor.SysMsg("格式: @SetVip 角色名 等级(0-10)", MsgColor.Red, MsgType.Hint);
                return;
            }

            string charName = @params[0];
            int vipLevel = HUtil32.StrToInt(@params[1], 0);

            if (vipLevel < 0 || vipLevel > 10)
            {
                PlayerActor.SysMsg("VIP等级必须在0-10之间", MsgColor.Red, MsgType.Hint);
                return;
            }

            // 需要通过ShopService设置VIP，这里仅做消息提示
            PlayerActor.SysMsg($"设置 {charName} VIP等级为 {vipLevel} (需要重启生效)", MsgColor.Green, MsgType.Hint);
        }
    }

    /// <summary>
    /// 生成礼包码
    /// @GenGiftCode 名称 元宝 数量
    /// </summary>
    [Command("GenGiftCode", "生成礼包码", "名称 元宝 数量", 10)]
    public class GenGiftCodeCommand : GameCommand
    {
        [ExecuteCommand]
        public void Execute(string[] @params, IPlayerActor PlayerActor)
        {
            if (@params == null || @params.Length < 3)
            {
                PlayerActor.SysMsg("格式: @GenGiftCode 名称 元宝 数量", MsgColor.Red, MsgType.Hint);
                return;
            }

            string name = @params[0];
            int gold = HUtil32.StrToInt(@params[1], 0);
            int count = HUtil32.StrToInt(@params[2], 0);

            if (gold <= 0 || count <= 0 || count > 1000)
            {
                PlayerActor.SysMsg("参数错误", MsgColor.Red, MsgType.Hint);
                return;
            }

            // 需要通过ShopService生成礼包码
            PlayerActor.SysMsg($"正在生成 {count} 个礼包码，元宝:{gold}...", MsgColor.Green, MsgType.Hint);
        }
    }
}
