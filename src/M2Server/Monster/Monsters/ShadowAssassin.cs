using M2Server.Magic;
using OpenMir2;
using OpenMir2.Consts;
using SystemModule.Actors;

namespace M2Server.Monster.Monsters
{
    /// <summary>
    /// 暗影刺客 - 隐身突袭怪物
    /// 特点：平时隐身，接近目标时显形并造成暴击伤害
    /// Race = 301
    /// </summary>
    public class ShadowAssassin : MonsterObject
    {
        private bool _isInStealth;
        private int _stealthCooldown;
        private const int CriticalMultiplier = 3;  // 暴击3倍伤害
        private const int StealthRecovery = 8000;  // 8秒后重新隐身

        public ShadowAssassin() : base()
        {
            ViewRange = 8;
            RunTime = 150;  // 移动速度快
            SearchTime = M2Share.RandomNumber.Random(1000) + 1500;
            _isInStealth = true;
            _stealthCooldown = HUtil32.GetTickCount();
            EnterStealth();
        }

        private void EnterStealth()
        {
            _isInStealth = true;
            // 使用隐身状态
            MagicManager.MagMakePrivateTransparent(this, 600);
            HideMode = true;
        }

        private void ExitStealth()
        {
            _isInStealth = false;
            _stealthCooldown = HUtil32.GetTickCount();
            StatusTimeArr[PoisonState.STATETRANSPARENT] = 0;
            HideMode = false;
            // 发送显形特效
            SendRefMsg(Messages.RM_SPACEMOVE_SHOW, 0, 0, 0, 0, "");
        }

        protected override bool AttackTarget()
        {
            if (TargetCret == null || TargetCret.Death)
            {
                return false;
            }

            byte btDir = 0;
            if (GetAttackDir(TargetCret, ref btDir))
            {
                if ((HUtil32.GetTickCount() - AttackTick) > NextHitTime)
                {
                    AttackTick = HUtil32.GetTickCount();
                    TargetFocusTick = HUtil32.GetTickCount();
                    Dir = btDir;

                    // 从隐身状态攻击造成暴击
                    int basePower = GetAttackPower(HUtil32.LoByte(WAbil.DC), (short)(HUtil32.HiByte(WAbil.DC) - HUtil32.LoByte(WAbil.DC)));
                    int power = _isInStealth ? basePower * CriticalMultiplier : basePower;

                    if (_isInStealth)
                    {
                        ExitStealth();
                        // 暴击特效
                        SendRefMsg(Messages.RM_STRUCK, power, 0, 0, 0, "");
                    }

                    int damage = TargetCret.GetHitStruckDamage(this, power);
                    if (damage > 0)
                    {
                        TargetCret.StruckDamage(damage);
                        TargetCret.SendStruckDelayMsg(Messages.RM_STRUCK, damage, TargetCret.WAbil.HP, TargetCret.WAbil.MaxHP, ActorId, "", 200);
                    }
                }
                return true;
            }
            else
            {
                if (TargetCret.Envir == Envir)
                {
                    SetTargetXy(TargetCret.CurrX, TargetCret.CurrY);
                }
                else
                {
                    DelTargetCreat();
                }
            }
            return false;
        }

        public override void Run()
        {
            if (!Death && !Ghost && CanMove())
            {
                // 脱战后重新进入隐身
                if (!_isInStealth && TargetCret == null &&
                    (HUtil32.GetTickCount() - _stealthCooldown) > StealthRecovery)
                {
                    EnterStealth();
                }

                // 搜索目标
                if ((HUtil32.GetTickCount() - SearchEnemyTick) > 6000 ||
                    (HUtil32.GetTickCount() - SearchEnemyTick) > 800 && TargetCret == null)
                {
                    SearchEnemyTick = HUtil32.GetTickCount();
                    SearchTarget();
                }
            }
            base.Run();
        }
    }
}
