namespace AppraisalModule.Models
{
    /// <summary>
    /// 鉴定属性配置
    /// </summary>
    public class AppraisalAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string EffectType { get; set; } = string.Empty;
        public int EffectValue { get; set; }
        public int MaxLevel { get; set; } = 5;
        public int ScrollLevel { get; set; } = 1;
        public int Probability { get; set; } = 10;
        public bool IsRare { get; set; }
        public bool Broadcast { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 卷轴商品配置
    /// </summary>
    public class ScrollItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int ScrollLevel { get; set; } = 1;
        public int PriceSingle { get; set; } = 1000;
        public int PriceBatch { get; set; } = 10000;
        public int BatchCount { get; set; } = 10;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 鉴定记录日志
    /// </summary>
    public class AppraisalLog
    {
        public long Id { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public string CharName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? ScrollName { get; set; }
        public string? ResultAttribute { get; set; }
        public int ResultLevel { get; set; }
        public bool IsSuccess { get; set; } = true;
        public bool IsBroadcast { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 鉴定结果
    /// </summary>
    public class AppraisalResult
    {
        public bool Success { get; set; }
        public string? AttributeName { get; set; }
        public int AttributeLevel { get; set; }
        public string? EffectType { get; set; }
        public bool ShouldBroadcast { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// 属性效果类型
    /// </summary>
    public static class EffectTypes
    {
        public const string Rebirth = "rebirth";           // 重生
        public const string Block = "block";               // 格挡
        public const string Paralysis = "paralysis";       // 麻痹
        public const string MagicParalysis = "magic_paralysis"; // 魔法麻痹
        public const string TaoParalysis = "tao_paralysis";     // 道术麻痹
        public const string Detect = "detect";             // 探测
        public const string Teleport = "teleport";         // 传送
        public const string Lifesteal = "lifesteal";       // 吸血
        public const string Reflect = "reflect";           // 反弹
        public const string Lucky = "lucky";               // 幸运
        public const string MagicShield = "magic_shield";  // 魔法护盾
        public const string Speed = "speed";               // 速度
    }

    #region API Models

    public class ScrollItemSaveRequest
    {
        public int? Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int ScrollLevel { get; set; } = 1;
        public int PriceSingle { get; set; } = 1000;
        public int PriceBatch { get; set; } = 10000;
        public int BatchCount { get; set; } = 10;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class AppraisalAttributeSaveRequest
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string EffectType { get; set; } = string.Empty;
        public int MaxLevel { get; set; } = 5;
        public int ScrollLevel { get; set; } = 1;
        public int Probability { get; set; } = 10;
        public bool IsRare { get; set; }
        public bool Broadcast { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class AppraisalRequest
    {
        public string AccountId { get; set; } = string.Empty;
        public string CharName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ScrollName { get; set; } = string.Empty;
        public bool UseLuckyCharm { get; set; } = false;
    }

    #endregion
}
