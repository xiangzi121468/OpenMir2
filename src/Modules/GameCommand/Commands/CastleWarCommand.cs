using CastleModule;
using M2Server;
using M2Server.Actor;
using SystemModule.Enums;

namespace GameCommand.Commands
{
    /// <summary>
    /// 攻城战相关GM命令
    /// </summary>
    [Command("CastleWar", "攻城", "攻城战管理命令", 10)]
    public class CastleWarCommand : BaseCommand
    {
        private static ICastleWarService _castleWarService;

        public static void Initialize(ICastleWarService service)
        {
            _castleWarService = service;
        }

        [DefaultCommand]
        public async Task Execute(string[] @params, PlayObject playObject)
        {
            if (@params.Length == 0)
            {
                ShowHelp(playObject);
                return;
            }

            var action = @params[0].ToLower();

            switch (action)
            {
                case "start":
                    await StartWar(playObject, @params);
                    break;
                case "stop":
                case "end":
                    await EndWar(playObject, @params);
                    break;
                case "info":
                case "status":
                    await ShowStatus(playObject, @params);
                    break;
                case "list":
                    await ListCastles(playObject);
                    break;
                case "apply":
                    await ShowApplicants(playObject, @params);
                    break;
                case "setowner":
                    await SetOwner(playObject, @params);
                    break;
                case "damage":
                    await DamageGate(playObject, @params);
                    break;
                case "repair":
                    await RepairGate(playObject, @params);
                    break;
                default:
                    ShowHelp(playObject);
                    break;
            }
        }

        private void ShowHelp(PlayObject playObject)
        {
            playObject.SysMsg("攻城战命令用法:", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 start [城堡名] - 开始攻城战", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 stop [城堡名] - 结束攻城战", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 info [城堡名] - 查看攻城状态", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 list - 列出所有城堡", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 apply [城堡名] - 查看报名行会", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 setowner [城堡名] [行会名] - 设置城主", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 damage [城堡名] [伤害] - 对城门造成伤害", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg("@攻城 repair [城堡名] - 修复城门", MsgColor.Green, MsgType.Hint);
        }

        private async Task StartWar(PlayObject playObject, string[] @params)
        {
            var castleName = @params.Length > 1 ? @params[1] : "沙巴克";
            var castle = await _castleWarService.GetCastleByNameAsync(castleName);

            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            if (_castleWarService.IsWarActive(castle.Id))
            {
                playObject.SysMsg($"攻城战已在进行中: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            var success = await _castleWarService.StartWarAsync(castle.Id);
            if (success)
            {
                playObject.SysMsg($"攻城战已开始: {castleName}", MsgColor.Green, MsgType.Hint);
                // 全服广播
                SystemShare.WorldEngine.SendBroadCastMsg($"【GM公告】{castleName}攻城战已开始！", MsgType.System);
            }
            else
            {
                playObject.SysMsg($"开始攻城战失败: {castleName}", MsgColor.Red, MsgType.Hint);
            }
        }

        private async Task EndWar(PlayObject playObject, string[] @params)
        {
            var castleName = @params.Length > 1 ? @params[1] : "沙巴克";
            var castle = await _castleWarService.GetCastleByNameAsync(castleName);

            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            if (!_castleWarService.IsWarActive(castle.Id))
            {
                playObject.SysMsg($"当前没有攻城战: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            var success = await _castleWarService.EndWarAsync(castle.Id, true);
            if (success)
            {
                playObject.SysMsg($"攻城战已结束: {castleName}", MsgColor.Green, MsgType.Hint);
                SystemShare.WorldEngine.SendBroadCastMsg($"【GM公告】{castleName}攻城战已结束！", MsgType.System);
            }
            else
            {
                playObject.SysMsg($"结束攻城战失败: {castleName}", MsgColor.Red, MsgType.Hint);
            }
        }

        private async Task ShowStatus(PlayObject playObject, string[] @params)
        {
            var castleName = @params.Length > 1 ? @params[1] : "沙巴克";
            var castle = await _castleWarService.GetCastleByNameAsync(castleName);

            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            playObject.SysMsg($"=== {castle.CastleName} 状态 ===", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg($"城主行会: {castle.OwnerGuildName ?? "无"}", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg($"城主: {castle.OwnerName ?? "无"}", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg($"占领时间: {castle.OccupyTime?.ToString("yyyy-MM-dd HH:mm") ?? "无"}", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg($"税率: {castle.TaxRate}%", MsgColor.Green, MsgType.Hint);
            playObject.SysMsg($"城门HP: {castle.GateHP}/{castle.GateMaxHP}", MsgColor.Green, MsgType.Hint);

            var warState = _castleWarService.GetWarState(castle.Id);
            if (warState != null && warState.IsActive)
            {
                playObject.SysMsg($"--- 攻城战进行中 ---", MsgColor.Yellow, MsgType.Hint);
                playObject.SysMsg($"剩余时间: {warState.RemainingSeconds / 60}分{warState.RemainingSeconds % 60}秒", MsgColor.Yellow, MsgType.Hint);
                playObject.SysMsg($"城门HP: {warState.GateHP}/{warState.GateMaxHP}", MsgColor.Yellow, MsgType.Hint);
                playObject.SysMsg($"击杀数: {warState.TotalKills}", MsgColor.Yellow, MsgType.Hint);
                if (warState.CurrentOccupierId.HasValue)
                {
                    playObject.SysMsg($"当前占领: [{warState.CurrentOccupierName}] {warState.LastOccupier}", MsgColor.Yellow, MsgType.Hint);
                }
            }
            else
            {
                var nextWar = _castleWarService.GetNextWarDate();
                playObject.SysMsg($"下次攻城: {nextWar:yyyy-MM-dd HH:mm}", MsgColor.Green, MsgType.Hint);
            }
        }

        private async Task ListCastles(PlayObject playObject)
        {
            var castles = await _castleWarService.GetAllCastlesAsync();

            playObject.SysMsg("=== 城堡列表 ===", MsgColor.Green, MsgType.Hint);
            foreach (var castle in castles)
            {
                var status = _castleWarService.IsWarActive(castle.Id) ? "[攻城中]" : "";
                playObject.SysMsg($"{castle.Id}. {castle.CastleName} - 城主: {castle.OwnerGuildName ?? "无"} {status}", MsgColor.Green, MsgType.Hint);
            }
        }

        private async Task ShowApplicants(PlayObject playObject, string[] @params)
        {
            var castleName = @params.Length > 1 ? @params[1] : "沙巴克";
            var castle = await _castleWarService.GetCastleByNameAsync(castleName);

            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            var nextWar = _castleWarService.GetNextWarDate();
            var applicants = await _castleWarService.GetWarApplicantsAsync(castle.Id, nextWar);

            playObject.SysMsg($"=== {castle.CastleName} 报名行会 ({nextWar:yyyy-MM-dd}) ===", MsgColor.Green, MsgType.Hint);
            if (applicants.Count == 0)
            {
                playObject.SysMsg("暂无行会报名", MsgColor.Green, MsgType.Hint);
            }
            else
            {
                foreach (var app in applicants)
                {
                    playObject.SysMsg($"{app.GuildName} - 会长: {app.GuildMaster}, 人数: {app.MemberCount}", MsgColor.Green, MsgType.Hint);
                }
            }
        }

        private async Task SetOwner(PlayObject playObject, string[] @params)
        {
            if (@params.Length < 3)
            {
                playObject.SysMsg("用法: @攻城 setowner [城堡名] [行会名]", MsgColor.Red, MsgType.Hint);
                return;
            }

            var castleName = @params[1];
            var guildName = @params[2];

            var castle = await _castleWarService.GetCastleByNameAsync(castleName);
            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            // TODO: 从数据库查找行会信息
            // 这里简化处理
            playObject.SysMsg($"设置城主功能需要行会系统支持", MsgColor.Yellow, MsgType.Hint);
        }

        private async Task DamageGate(PlayObject playObject, string[] @params)
        {
            if (@params.Length < 3)
            {
                playObject.SysMsg("用法: @攻城 damage [城堡名] [伤害值]", MsgColor.Red, MsgType.Hint);
                return;
            }

            var castleName = @params[1];
            if (!int.TryParse(@params[2], out var damage))
            {
                playObject.SysMsg("伤害值必须是数字", MsgColor.Red, MsgType.Hint);
                return;
            }

            var castle = await _castleWarService.GetCastleByNameAsync(castleName);
            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            if (!_castleWarService.IsWarActive(castle.Id))
            {
                playObject.SysMsg("当前没有攻城战进行中", MsgColor.Red, MsgType.Hint);
                return;
            }

            var remainingHP = await _castleWarService.AttackGateAsync(castle.Id, playObject.ChrName, playObject.MyGuild?.GuildIndex ?? 0, damage);
            playObject.SysMsg($"对城门造成 {damage} 点伤害，剩余HP: {remainingHP}", MsgColor.Green, MsgType.Hint);
        }

        private async Task RepairGate(PlayObject playObject, string[] @params)
        {
            var castleName = @params.Length > 1 ? @params[1] : "沙巴克";
            var castle = await _castleWarService.GetCastleByNameAsync(castleName);

            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            // 直接更新数据库
            // TODO: 实现修复逻辑
            playObject.SysMsg($"城门已修复: {castle.CastleName}", MsgColor.Green, MsgType.Hint);
        }
    }

    /// <summary>
    /// 玩家报名攻城命令
    /// </summary>
    [Command("ApplyCastle", "报名攻城", "报名参加攻城战", 0)]
    public class ApplyCastleCommand : BaseCommand
    {
        private static ICastleWarService _castleWarService;

        public static void Initialize(ICastleWarService service)
        {
            _castleWarService = service;
        }

        [DefaultCommand]
        public async Task Execute(string[] @params, PlayObject playObject)
        {
            if (playObject.MyGuild == null)
            {
                playObject.SysMsg("您还没有加入行会", MsgColor.Red, MsgType.Hint);
                return;
            }

            if (playObject.MyGuild.GuildRank != 1) // 不是会长
            {
                playObject.SysMsg("只有会长才能报名攻城", MsgColor.Red, MsgType.Hint);
                return;
            }

            var castleName = @params.Length > 0 ? @params[0] : "沙巴克";
            var castle = await _castleWarService.GetCastleByNameAsync(castleName);

            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            var (success, message) = await _castleWarService.ApplyForWarAsync(
                castle.Id,
                playObject.MyGuild.GuildIndex,
                playObject.MyGuild.GuildName,
                playObject.ChrName,
                playObject.MyGuild.MemberList.Count,
                playObject.Gold
            );

            if (success)
            {
                playObject.SysMsg(message, MsgColor.Green, MsgType.Hint);
            }
            else
            {
                playObject.SysMsg(message, MsgColor.Red, MsgType.Hint);
            }
        }
    }

    /// <summary>
    /// 查看城堡信息命令
    /// </summary>
    [Command("Castle", "沙城", "查看沙城信息", 0)]
    public class CastleInfoCommand : BaseCommand
    {
        private static ICastleWarService _castleWarService;

        public static void Initialize(ICastleWarService service)
        {
            _castleWarService = service;
        }

        [DefaultCommand]
        public async Task Execute(string[] @params, PlayObject playObject)
        {
            var castleName = @params.Length > 0 ? @params[0] : "沙巴克";
            var castle = await _castleWarService.GetCastleByNameAsync(castleName);

            if (castle == null)
            {
                playObject.SysMsg($"城堡不存在: {castleName}", MsgColor.Red, MsgType.Hint);
                return;
            }

            playObject.SysMsg($"=== {castle.CastleName} ===", MsgColor.Green, MsgType.Hint);
            
            if (castle.OwnerGuildId.HasValue)
            {
                playObject.SysMsg($"城主行会: [{castle.OwnerGuildName}]", MsgColor.Green, MsgType.Hint);
                playObject.SysMsg($"城主: {castle.OwnerName}", MsgColor.Green, MsgType.Hint);
                playObject.SysMsg($"占领时间: {castle.OccupyTime:yyyy-MM-dd HH:mm}", MsgColor.Green, MsgType.Hint);
            }
            else
            {
                playObject.SysMsg("当前无城主", MsgColor.Yellow, MsgType.Hint);
            }

            var warState = _castleWarService.GetWarState(castle.Id);
            if (warState != null && warState.IsActive)
            {
                playObject.SysMsg($"【攻城战进行中】", MsgColor.Red, MsgType.Hint);
                playObject.SysMsg($"剩余时间: {warState.RemainingSeconds / 60} 分钟", MsgColor.Red, MsgType.Hint);
            }
            else
            {
                var nextWar = _castleWarService.GetNextWarDate();
                playObject.SysMsg($"下次攻城: {nextWar:yyyy-MM-dd HH:mm}", MsgColor.Green, MsgType.Hint);
                
                var applicants = await _castleWarService.GetWarApplicantsAsync(castle.Id, nextWar);
                playObject.SysMsg($"已报名行会: {applicants.Count} 个", MsgColor.Green, MsgType.Hint);
            }
        }
    }
}
