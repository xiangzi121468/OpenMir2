namespace VipModule.Models
{
    /// <summary>
    /// VIP地图配置
    /// </summary>
    public class VipMap
    {
        public int Id { get; set; }
        public string MapId { get; set; } = string.Empty;
        public string MapName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MinVipLevel { get; set; } = 1;
        public int MinRecharge { get; set; } = 0;
        public int EntryFee { get; set; } = 0;
        public int DailyLimit { get; set; } = -1;
        public int LevelLimit { get; set; } = 1;
        public bool IsRandomEntry { get; set; } = true;
        public bool IsFixedEntry { get; set; } = true;
        public int RandomXMin { get; set; } = 100;
        public int RandomXMax { get; set; } = 200;
        public int RandomYMin { get; set; } = 100;
        public int RandomYMax { get; set; } = 200;
        public int FixedX { get; set; } = 150;
        public int FixedY { get; set; } = 150;
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 关联数据
        public List<VipMapMonster> Monsters { get; set; } = new();
    }

    /// <summary>
    /// VIP地图怪物配置
    /// </summary>
    public class VipMapMonster
    {
        public int Id { get; set; }
        public int VipMapId { get; set; }
        public string MonsterName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SpawnCount { get; set; } = 1;
        public int SpawnInterval { get; set; } = 30;
        public int SpawnX { get; set; } = 150;
        public int SpawnY { get; set; } = 150;
        public int SpawnRange { get; set; } = 10;
        public bool IsBoss { get; set; } = false;
        public string? DropDescription { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// VIP地图进入日志
    /// </summary>
    public class VipMapEntryLog
    {
        public long Id { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public string CharName { get; set; } = string.Empty;
        public string MapId { get; set; } = string.Empty;
        public string? MapName { get; set; }
        public string EntryType { get; set; } = "random";
        public int EntryFee { get; set; } = 0;
        public DateTime EntryTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// VIP等级配置
    /// </summary>
    public class VipLevelConfig
    {
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MinRecharge { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "#FFD700";
        public string Benefits { get; set; } = string.Empty;
    }

    #region API Models

    public class VipMapSaveRequest
    {
        public int? Id { get; set; }
        public string MapId { get; set; } = string.Empty;
        public string MapName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MinVipLevel { get; set; } = 1;
        public int MinRecharge { get; set; } = 0;
        public int EntryFee { get; set; } = 0;
        public int DailyLimit { get; set; } = -1;
        public int LevelLimit { get; set; } = 1;
        public bool IsRandomEntry { get; set; } = true;
        public bool IsFixedEntry { get; set; } = true;
        public int RandomXMin { get; set; } = 100;
        public int RandomXMax { get; set; } = 200;
        public int RandomYMin { get; set; } = 100;
        public int RandomYMax { get; set; } = 200;
        public int FixedX { get; set; } = 150;
        public int FixedY { get; set; } = 150;
        public bool IsEnabled { get; set; } = true;
    }

    public class VipMapMonsterSaveRequest
    {
        public int? Id { get; set; }
        public int VipMapId { get; set; }
        public string MonsterName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SpawnCount { get; set; } = 1;
        public int SpawnInterval { get; set; } = 30;
        public int SpawnX { get; set; } = 150;
        public int SpawnY { get; set; } = 150;
        public int SpawnRange { get; set; } = 10;
        public bool IsBoss { get; set; } = false;
        public string? DropDescription { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    #endregion
}
