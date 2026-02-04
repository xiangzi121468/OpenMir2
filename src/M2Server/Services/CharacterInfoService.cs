using NLog;
using M2Server.Actor;
using SystemModule.Enums;
using System.Text.Json;

namespace M2Server.Services
{
    /// <summary>
    /// 角色信息服务
    /// 提供角色信息的查询、计算和同步功能
    /// </summary>
    public class CharacterInfoService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static CharacterInfoService _instance;
        public static CharacterInfoService Instance => _instance ??= new CharacterInfoService();

        #region 角色信息查询

        /// <summary>
        /// 获取角色基础信息（用于客户端左上角显示）
        /// </summary>
        public CharacterBasicInfo GetBasicInfo(PlayObject player)
        {
            if (player == null) return null;

            return new CharacterBasicInfo
            {
                // 基本信息
                ActorId = player.ActorId,
                ChrName = player.ChrName,
                Level = player.Abil.Level,
                Job = (int)player.Job,
                JobName = GetJobName(player.Job),
                Gender = (int)player.Gender,
                GenderName = player.Gender == PlayerGender.Man ? "男" : "女",
                
                // 生命魔法
                HP = player.WAbil.HP,
                MaxHP = player.WAbil.MaxHP,
                MP = player.WAbil.MP,
                MaxMP = player.WAbil.MaxMP,
                
                // 经验
                Exp = player.Abil.Exp,
                MaxExp = player.Abil.MaxExp,
                ExpPercent = player.Abil.MaxExp > 0 ? (double)player.Abil.Exp / player.Abil.MaxExp * 100 : 0,
                
                // 货币
                Gold = player.Gold,
                GameGold = player.GameGold,
                GamePoint = player.GamePoint,
                
                // 位置
                MapName = player.MapName,
                MapTitle = player.Envir?.MapDesc ?? player.MapName,
                CurrX = player.CurrX,
                CurrY = player.CurrY,
                
                // 行会
                GuildName = player.MyGuild?.GuildName,
                GuildRank = player.MyGuild != null ? GetGuildRankName(player.GuildRankNo) : null,
                
                // PK值
                PkPoint = player.PkPoint,
                PkLevel = GetPkLevel(player.PkPoint),
                
                // 在线时长
                OnlineSeconds = (int)(DateTime.Now - player.LogonTime).TotalSeconds
            };
        }

        /// <summary>
        /// 获取角色战斗属性
        /// </summary>
        public CharacterCombatInfo GetCombatInfo(PlayObject player)
        {
            if (player == null) return null;

            return new CharacterCombatInfo
            {
                // 攻击属性
                DC = player.WAbil.DC,        // 攻击力下限
                MaxDC = player.WAbil.MaxDC,  // 攻击力上限
                MC = player.WAbil.MC,        // 魔法力下限
                MaxMC = player.WAbil.MaxMC,  // 魔法力上限
                SC = player.WAbil.SC,        // 道术下限
                MaxSC = player.WAbil.MaxSC,  // 道术上限
                
                // 防御属性
                AC = player.WAbil.AC,        // 防御下限
                MaxAC = player.WAbil.MaxAC,  // 防御上限
                MAC = player.WAbil.MAC,      // 魔防下限
                MaxMAC = player.WAbil.MaxMAC,// 魔防上限
                
                // 其他属性
                HIT = player.HitPoint,       // 命中
                Speed = player.SpeedPoint,   // 敏捷
                AntiPoison = player.AntiPoison, // 抗毒
                PoisonRecover = player.PoisonRecover, // 毒素恢复
                HealthRecover = player.HealthRecover, // 体力恢复
                SpellRecover = player.SpellRecover,   // 魔法恢复
                AntiMagic = player.AntiMagic, // 魔法躲避
                
                // 附加属性
                Luck = player.Luck,          // 幸运
                UnLuck = player.UnLuck,      // 诅咒
                
                // 伤害加成
                AttackSpeed = player.AttackSpeed, // 攻击速度
                
                // 综合战斗力
                CombatPower = CalculateCombatPower(player)
            };
        }

        /// <summary>
        /// 获取角色完整信息
        /// </summary>
        public CharacterFullInfo GetFullInfo(PlayObject player)
        {
            if (player == null) return null;

            return new CharacterFullInfo
            {
                Basic = GetBasicInfo(player),
                Combat = GetCombatInfo(player),
                Equipment = GetEquipmentInfo(player),
                Skills = GetSkillsInfo(player),
                Buffs = GetBuffsInfo(player)
            };
        }

        /// <summary>
        /// 获取装备信息
        /// </summary>
        public List<EquipmentSlotInfo> GetEquipmentInfo(PlayObject player)
        {
            if (player == null) return new List<EquipmentSlotInfo>();

            var result = new List<EquipmentSlotInfo>();
            var slotNames = new[]
            {
                "衣服", "武器", "照明", "项链", "头盔", "左手镯", "右手镯",
                "左戒指", "右戒指", "护身符", "腰带", "靴子", "宝石"
            };

            for (int i = 0; i < player.UseItems.Length && i < slotNames.Length; i++)
            {
                var item = player.UseItems[i];
                if (item != null && item.Index > 0)
                {
                    var stdItem = SystemShare.ItemSystem.GetStdItem(item.Index);
                    if (stdItem != null)
                    {
                        result.Add(new EquipmentSlotInfo
                        {
                            SlotIndex = i,
                            SlotName = slotNames[i],
                            ItemId = item.Index,
                            ItemName = stdItem.Name,
                            Durability = item.Dura,
                            MaxDurability = item.DuraMax,
                            UpgradeLevel = item.Desc[0], // 升级等级
                            IsEquipped = true
                        });
                    }
                }
                else
                {
                    result.Add(new EquipmentSlotInfo
                    {
                        SlotIndex = i,
                        SlotName = slotNames[i],
                        IsEquipped = false
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// 获取技能信息
        /// </summary>
        public List<SkillInfo> GetSkillsInfo(PlayObject player)
        {
            if (player == null) return new List<SkillInfo>();

            var result = new List<SkillInfo>();

            foreach (var magic in player.MagicList)
            {
                if (magic != null && magic.Magic != null)
                {
                    result.Add(new SkillInfo
                    {
                        MagicId = magic.Magic.MagicId,
                        Name = magic.Magic.MagicName,
                        Level = magic.Level,
                        MaxLevel = 3,
                        TrainLevel = magic.TranPoint,
                        MaxTrainLevel = GetMagicMaxTrainPoint(magic.Level),
                        Cooldown = magic.Magic.DefSpell,
                        ManaCost = (int)PlayObject.GetMagicSpell(magic),
                        KeyBind = magic.Key
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// 获取Buff信息
        /// </summary>
        public List<BuffInfo> GetBuffsInfo(PlayObject player)
        {
            if (player == null) return new List<BuffInfo>();

            var result = new List<BuffInfo>();

            // 检查各种状态效果
            if (player.StatusTimeArr != null)
            {
                var buffNames = new Dictionary<int, string>
                {
                    { 0, "防御术" },
                    { 1, "魔法盾" },
                    { 2, "隐身" },
                    { 3, "中毒" },
                    { 4, "麻痹" },
                    { 5, "攻击加成" },
                    { 6, "护体神盾" },
                    { 7, "幽灵盾" },
                    { 8, "神圣战甲术" }
                };

                for (int i = 0; i < player.StatusTimeArr.Length; i++)
                {
                    if (player.StatusTimeArr[i] > 0)
                    {
                        result.Add(new BuffInfo
                        {
                            BuffId = i,
                            Name = buffNames.GetValueOrDefault(i, $"状态{i}"),
                            RemainingSeconds = player.StatusTimeArr[i] / 1000,
                            IsPositive = i != 3 && i != 4 // 中毒和麻痹是负面效果
                        });
                    }
                }
            }

            return result;
        }

        #endregion

        #region 信息同步

        /// <summary>
        /// 发送角色基础信息更新到客户端
        /// </summary>
        public void SendBasicInfoUpdate(PlayObject player)
        {
            if (player == null || player.IsRobot) return;

            var info = GetBasicInfo(player);
            var json = JsonSerializer.Serialize(info);
            
            // TODO: 根据协议格式发送
            // player.SendDefMessage(Messages.SM_CHARINFO, ...);
        }

        /// <summary>
        /// 发送HP/MP更新（高频调用，精简数据）
        /// </summary>
        public void SendHealthUpdate(PlayObject player)
        {
            if (player == null || player.IsRobot) return;

            // 调用已有的方法
            player.HealthSpellChanged();
        }

        /// <summary>
        /// 发送经验更新
        /// </summary>
        public void SendExpUpdate(PlayObject player, long gainedExp)
        {
            if (player == null || player.IsRobot) return;

            // 发送经验变化
            var expPercent = player.Abil.MaxExp > 0 
                ? (int)(player.Abil.Exp * 100 / player.Abil.MaxExp) 
                : 0;

            // TODO: 根据协议发送
            // player.SendDefMessage(Messages.SM_EXPCHANGE, gainedExp, expPercent, ...);
        }

        /// <summary>
        /// 发送货币更新
        /// </summary>
        public void SendCurrencyUpdate(PlayObject player, string currencyType, long amount, long newTotal)
        {
            if (player == null || player.IsRobot) return;

            // 调用已有的金币变化通知
            if (currencyType == "gold")
            {
                player.GoldChanged();
            }
            else if (currencyType == "gamegold")
            {
                player.GameGoldChanged();
            }
        }

        #endregion

        #region 属性计算

        /// <summary>
        /// 计算综合战斗力
        /// </summary>
        public int CalculateCombatPower(PlayObject player)
        {
            if (player == null) return 0;

            int power = 0;

            // 等级贡献
            power += player.Abil.Level * 10;

            // 生命贡献
            power += player.WAbil.MaxHP / 10;

            // 攻击贡献（根据职业）
            switch (player.Job)
            {
                case PlayerJob.Warrior:
                    power += (player.WAbil.DC + player.WAbil.MaxDC) * 5;
                    power += (player.WAbil.AC + player.WAbil.MaxAC) * 3;
                    break;
                case PlayerJob.Wizard:
                    power += (player.WAbil.MC + player.WAbil.MaxMC) * 5;
                    power += player.WAbil.MaxMP / 5;
                    break;
                case PlayerJob.Taoist:
                    power += (player.WAbil.SC + player.WAbil.MaxSC) * 5;
                    power += (player.WAbil.MAC + player.WAbil.MaxMAC) * 3;
                    break;
            }

            // 防御贡献
            power += (player.WAbil.AC + player.WAbil.MaxAC) * 2;
            power += (player.WAbil.MAC + player.WAbil.MaxMAC) * 2;

            // 装备加成
            for (int i = 0; i < player.UseItems.Length; i++)
            {
                if (player.UseItems[i] != null && player.UseItems[i].Index > 0)
                {
                    power += 50; // 每件装备基础战力
                    power += player.UseItems[i].Desc[0] * 20; // 强化等级加成
                }
            }

            // 技能加成
            power += player.MagicList.Count * 30;

            return power;
        }

        /// <summary>
        /// 获取职业名称
        /// </summary>
        private string GetJobName(PlayerJob job)
        {
            return job switch
            {
                PlayerJob.Warrior => "战士",
                PlayerJob.Wizard => "法师",
                PlayerJob.Taoist => "道士",
                _ => "未知"
            };
        }

        /// <summary>
        /// 获取行会职位名称
        /// </summary>
        private string GetGuildRankName(int rankNo)
        {
            return rankNo switch
            {
                1 => "会长",
                2 => "副会长",
                3 => "长老",
                4 => "精英",
                _ => "成员"
            };
        }

        /// <summary>
        /// 获取PK等级
        /// </summary>
        private string GetPkLevel(int pkPoint)
        {
            if (pkPoint >= 300) return "红名";
            if (pkPoint >= 100) return "黄名";
            if (pkPoint > 0) return "灰名";
            return "白名";
        }

        /// <summary>
        /// 获取技能最大修炼点数
        /// </summary>
        private int GetMagicMaxTrainPoint(int level)
        {
            return level switch
            {
                0 => 50,
                1 => 100,
                2 => 200,
                3 => 300,
                _ => 100
            };
        }

        #endregion
    }

    #region 数据模型

    /// <summary>
    /// 角色基础信息（用于客户端显示）
    /// </summary>
    public class CharacterBasicInfo
    {
        public int ActorId { get; set; }
        public string ChrName { get; set; }
        public int Level { get; set; }
        public int Job { get; set; }
        public string JobName { get; set; }
        public int Gender { get; set; }
        public string GenderName { get; set; }
        
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int MP { get; set; }
        public int MaxMP { get; set; }
        
        public long Exp { get; set; }
        public long MaxExp { get; set; }
        public double ExpPercent { get; set; }
        
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public int GamePoint { get; set; }
        
        public string MapName { get; set; }
        public string MapTitle { get; set; }
        public int CurrX { get; set; }
        public int CurrY { get; set; }
        
        public string GuildName { get; set; }
        public string GuildRank { get; set; }
        
        public int PkPoint { get; set; }
        public string PkLevel { get; set; }
        
        public int OnlineSeconds { get; set; }
    }

    /// <summary>
    /// 角色战斗属性
    /// </summary>
    public class CharacterCombatInfo
    {
        // 攻击
        public int DC { get; set; }
        public int MaxDC { get; set; }
        public int MC { get; set; }
        public int MaxMC { get; set; }
        public int SC { get; set; }
        public int MaxSC { get; set; }
        
        // 防御
        public int AC { get; set; }
        public int MaxAC { get; set; }
        public int MAC { get; set; }
        public int MaxMAC { get; set; }
        
        // 其他
        public int HIT { get; set; }
        public int Speed { get; set; }
        public int AntiPoison { get; set; }
        public int PoisonRecover { get; set; }
        public int HealthRecover { get; set; }
        public int SpellRecover { get; set; }
        public int AntiMagic { get; set; }
        
        // 幸运
        public int Luck { get; set; }
        public int UnLuck { get; set; }
        
        public int AttackSpeed { get; set; }
        public int CombatPower { get; set; }
    }

    /// <summary>
    /// 装备槽位信息
    /// </summary>
    public class EquipmentSlotInfo
    {
        public int SlotIndex { get; set; }
        public string SlotName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Durability { get; set; }
        public int MaxDurability { get; set; }
        public int UpgradeLevel { get; set; }
        public bool IsEquipped { get; set; }
    }

    /// <summary>
    /// 技能信息
    /// </summary>
    public class SkillInfo
    {
        public int MagicId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public int TrainLevel { get; set; }
        public int MaxTrainLevel { get; set; }
        public int Cooldown { get; set; }
        public int ManaCost { get; set; }
        public byte KeyBind { get; set; }
    }

    /// <summary>
    /// Buff信息
    /// </summary>
    public class BuffInfo
    {
        public int BuffId { get; set; }
        public string Name { get; set; }
        public int RemainingSeconds { get; set; }
        public bool IsPositive { get; set; }
    }

    /// <summary>
    /// 角色完整信息
    /// </summary>
    public class CharacterFullInfo
    {
        public CharacterBasicInfo Basic { get; set; }
        public CharacterCombatInfo Combat { get; set; }
        public List<EquipmentSlotInfo> Equipment { get; set; }
        public List<SkillInfo> Skills { get; set; }
        public List<BuffInfo> Buffs { get; set; }
    }

    #endregion
}
