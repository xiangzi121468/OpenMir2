using M2Server.Magic;
using OpenMir2;
using OpenMir2.Consts;
using SystemModule;
using SystemModule.Actors;

namespace M2Server.Monster.Monsters
{
    /// <summary>
    /// 冰霜领主 - 高级BOSS
    /// 特点：远程冰霜攻击，可冻结目标，周期性AOE暴风雪
    /// Race = 300
    /// </summary>
    public class FrostLord : MonsterObject
    {
        private int _freezeSkillTick;
        private int _blizzardTick;
        private const int FreezeChance = 25;        // 25%冻结概率
        private const int FreezeDuration = 3000;    // 冻结3秒
        private const int BlizzardInterval = 15000; // 15秒释放一次暴风雪

        public FrostLord() : base()
        {
            ViewRange = 10;
            RunTime = 200;
            SearchTime = M2Share.RandomNumber.Random(1000) + 1000;
            _freezeSkillTick = HUtil32.GetTickCount();
            _blizzardTick = HUtil32.GetTickCount();
        }

        protected override bool AttackTarget()
        {
            if (TargetCret == null || TargetCret.Death)
            {
                return false;
            }

            int distance = Math.Abs(CurrX - TargetCret.CurrX) + Math.Abs(CurrY - TargetCret.CurrY);

            // 远程攻击范围内
            if (distance <= ViewRange && (HUtil32.GetTickCount() - AttackTick) > NextHitTime)
            {
                AttackTick = HUtil32.GetTickCount();
                TargetFocusTick = HUtil32.GetTickCount();

                // 计算冰霜伤害
                int power = GetAttackPower(HUtil32.LoByte(WAbil.DC), (short)(HUtil32.HiByte(WAbil.DC) - HUtil32.LoByte(WAbil.DC)));
                int damage = TargetCret.GetMagStruckDamage(this, power);

                if (damage > 0)
                {
                    TargetCret.StruckDamage(damage);
                    TargetCret.SendStruckDelayMsg(Messages.RM_MAGSTRUCK_MINE, damage, TargetCret.WAbil.HP, TargetCret.WAbil.MaxHP, ActorId, "", 300);

                    // 冻结效果
                    if (M2Share.RandomNumber.Random(100) < FreezeChance)
                    {
                        ApplyFreezeEffect(TargetCret);
                    }
                }
                return true;
            }

            // 移动接近目标
            if (TargetCret.Envir == Envir)
            {
                SetTargetXy(TargetCret.CurrX, TargetCret.CurrY);
            }
            return false;
        }

        private void ApplyFreezeEffect(IActor target)
        {
            // 施加冰冻状态（麻痹效果）
            target.StatusTimeArr[PoisonState.STONEMODE] = FreezeDuration / 1000;
            target.CharStatusEx = 1;
            target.CharStatus = SystemShare.GetCharStatus(target);
            target.SendRefMsg(Messages.RM_CHARSTATUSCHANGED, target.HitSpeed, target.CharStatus, 0, 0, "");
        }

        /// <summary>
        /// AOE暴风雪 - 对周围5格内所有敌人造成伤害
        /// </summary>
        private void CastBlizzard()
        {
            int power = GetAttackPower(HUtil32.LoByte(WAbil.MC), (short)(HUtil32.HiByte(WAbil.MC) - HUtil32.LoByte(WAbil.MC)));
            power = (int)(power * 1.5); // 暴风雪伤害增加50%

            for (int i = 0; i < VisibleActors.Count; i++)
            {
                IActor target = VisibleActors[i].BaseObject;
                if (target.Death || !IsProperTarget(target))
                {
                    continue;
                }

                if (Math.Abs(CurrX - target.CurrX) <= 5 && Math.Abs(CurrY - target.CurrY) <= 5)
                {
                    int damage = target.GetMagStruckDamage(this, power);
                    if (damage > 0)
                    {
                        target.StruckDamage(damage);
                        target.SendStruckDelayMsg(Messages.RM_MAGSTRUCK_MINE, damage, target.WAbil.HP, target.WAbil.MaxHP, ActorId, "", 500);
                    }

                    // 暴风雪有50%概率冻结
                    if (M2Share.RandomNumber.Random(100) < 50)
                    {
                        ApplyFreezeEffect(target);
                    }
                }
            }

            // 发送技能特效
            SendRefMsg(Messages.RM_MAGICFIRE, 0, MagicConst.INTURNSTORM, CurrX, CurrY, "");
        }

        public override void Run()
        {
            if (!Death && !Ghost && CanMove())
            {
                // 周期性释放暴风雪
                if ((HUtil32.GetTickCount() - _blizzardTick) > BlizzardInterval)
                {
                    _blizzardTick = HUtil32.GetTickCount();
                    if (TargetCret != null)
                    {
                        CastBlizzard();
                    }
                }

                if ((HUtil32.GetTickCount() - SearchEnemyTick) > 5000 ||
                    (HUtil32.GetTickCount() - SearchEnemyTick) > 1000 && TargetCret == null)
                {
                    SearchEnemyTick = HUtil32.GetTickCount();
                    SearchTarget();
                }
            }
            base.Run();
        }
    }
}
