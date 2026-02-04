using OpenMir2;
using SystemModule.Actors;

namespace M2Server.Monster.Monsters
{
    /// <summary>
    /// 吸血蝙蝠 - 吸血恢复型怪物
    /// 特点：攻击时吸取对方生命值恢复自身
    /// Race = 304
    /// </summary>
    public class VampireBat : AtMonster
    {
        private const int LifeStealPercent = 30; // 吸取伤害30%为生命

        public VampireBat() : base()
        {
            ViewRange = 7;
            RunTime = 180; // 移动较快
            SearchTime = M2Share.RandomNumber.Random(1200) + 1200;
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

                    int power = GetAttackPower(HUtil32.LoByte(WAbil.DC), (short)(HUtil32.HiByte(WAbil.DC) - HUtil32.LoByte(WAbil.DC)));
                    int damage = TargetCret.GetHitStruckDamage(this, power);

                    if (damage > 0)
                    {
                        TargetCret.StruckDamage(damage);
                        TargetCret.SendStruckDelayMsg(Messages.RM_STRUCK, damage, TargetCret.WAbil.HP, TargetCret.WAbil.MaxHP, ActorId, "", 200);

                        // 吸血恢复
                        int healAmount = damage * LifeStealPercent / 100;
                        if (healAmount > 0)
                        {
                            WAbil.HP = (ushort)Math.Min(WAbil.HP + healAmount, WAbil.MaxHP);
                            // 发送恢复特效
                            SendRefMsg(Messages.RM_MAGHEALING, 0, healAmount, 0, 0, "");
                        }
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
    }
}
