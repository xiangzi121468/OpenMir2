using OpenMir2;
using OpenMir2.Consts;
using SystemModule;
using SystemModule.Actors;
using SystemModule.MagicEvent.Events;

namespace M2Server.Monster.Monsters
{
    /// <summary>
    /// 烈焰魔龙 - 顶级BOSS
    /// 特点：火焰吐息，释放火墙，免疫火焰
    /// Race = 305
    /// </summary>
    public class InfernoWyrm : MonsterObject
    {
        private int _breathTick;
        private int _firewallTick;
        private const int BreathInterval = 8000;    // 8秒吐息一次
        private const int FirewallInterval = 20000; // 20秒释放火墙
        private const int FirewallDuration = 30000; // 火墙持续30秒
        private const int FirewallDamage = 50;

        public InfernoWyrm() : base()
        {
            ViewRange = 12;
            RunTime = 350;
            SearchTime = M2Share.RandomNumber.Random(1500) + 1500;
            BoFearFire = false; // 不怕火
            _breathTick = HUtil32.GetTickCount();
            _firewallTick = HUtil32.GetTickCount();
        }

        /// <summary>
        /// 火焰吐息 - 直线范围攻击
        /// </summary>
        private void FireBreath()
        {
            if (TargetCret == null) return;

            int power = GetAttackPower(HUtil32.LoByte(WAbil.MC), (short)(HUtil32.HiByte(WAbil.MC) - HUtil32.LoByte(WAbil.MC)));
            power = (int)(power * 2); // 吐息伤害翻倍

            // 计算吐息方向
            int dx = TargetCret.CurrX - CurrX;
            int dy = TargetCret.CurrY - CurrY;
            int stepX = dx == 0 ? 0 : dx / Math.Abs(dx);
            int stepY = dy == 0 ? 0 : dy / Math.Abs(dy);

            // 直线5格范围攻击
            for (int i = 1; i <= 5; i++)
            {
                short targetX = (short)(CurrX + stepX * i);
                short targetY = (short)(CurrY + stepY * i);

                // 对该位置的敌人造成伤害
                for (int j = 0; j < VisibleActors.Count; j++)
                {
                    IActor target = VisibleActors[j].BaseObject;
                    if (target.Death || !IsProperTarget(target)) continue;

                    if (target.CurrX == targetX && target.CurrY == targetY)
                    {
                        int damage = target.GetMagStruckDamage(this, power);
                        if (damage > 0)
                        {
                            target.StruckDamage(damage);
                            target.SendStruckDelayMsg(Messages.RM_MAGSTRUCK_MINE, damage, target.WAbil.HP, target.WAbil.MaxHP, ActorId, "", 300);
                        }
                    }
                }
            }

            // 发送吐息特效 (类似地狱火)
            SendRefMsg(Messages.RM_MAGICFIRE, 0, MagicConst.HELLFIRE, TargetCret.CurrX, TargetCret.CurrY, "");
        }

        /// <summary>
        /// 释放环形火墙
        /// </summary>
        private void ReleaseFirewall()
        {
            // 在自身周围8个方向释放火墙
            int[,] offsets = { { 0, -2 }, { 2, -2 }, { 2, 0 }, { 2, 2 }, { 0, 2 }, { -2, 2 }, { -2, 0 }, { -2, -2 } };

            for (int i = 0; i < 8; i++)
            {
                short fireX = (short)(CurrX + offsets[i, 0]);
                short fireY = (short)(CurrY + offsets[i, 1]);

                if (Envir.GetEvent(fireX, fireY) == null)
                {
                    var fireEvent = new FireBurnEvent(this, fireX, fireY, Grobal2.ET_FIRE, FirewallDuration, FirewallDamage);
                    SystemShare.EventMgr.AddEvent(fireEvent);
                }
            }

            // 发送火墙特效
            SendRefMsg(Messages.RM_MAGICFIRE, 0, MagicConst.FIREWALL, CurrX, CurrY, "");
        }

        protected override bool AttackTarget()
        {
            bool result = false;
            byte btDir = 0;

            if (TargetCret != null && !TargetCret.Death)
            {
                int distance = Math.Abs(CurrX - TargetCret.CurrX) + Math.Abs(CurrY - TargetCret.CurrY);

                // 远程吐息攻击
                if (distance <= 6 && (HUtil32.GetTickCount() - _breathTick) > BreathInterval)
                {
                    _breathTick = HUtil32.GetTickCount();
                    FireBreath();
                    return true;
                }

                // 近战攻击
                if (GetAttackDir(TargetCret, ref btDir))
                {
                    if ((HUtil32.GetTickCount() - AttackTick) > NextHitTime)
                    {
                        AttackTick = HUtil32.GetTickCount();
                        TargetFocusTick = HUtil32.GetTickCount();
                        Dir = btDir;

                        int power = GetAttackPower(HUtil32.LoByte(WAbil.DC), (short)(HUtil32.HiByte(WAbil.DC) - HUtil32.LoByte(WAbil.DC)));
                        power = (int)(power * 1.5); // 近战加成

                        int damage = TargetCret.GetHitStruckDamage(this, power);
                        if (damage > 0)
                        {
                            TargetCret.StruckDamage(damage);
                            TargetCret.SendStruckDelayMsg(Messages.RM_STRUCK, damage, TargetCret.WAbil.HP, TargetCret.WAbil.MaxHP, ActorId, "", 200);
                        }
                    }
                    result = true;
                }
                else
                {
                    if (TargetCret.Envir == Envir)
                    {
                        SetTargetXy(TargetCret.CurrX, TargetCret.CurrY);
                    }
                }
            }
            return result;
        }

        public override void Run()
        {
            if (!Death && !Ghost && CanMove())
            {
                // 周期性释放火墙
                if ((HUtil32.GetTickCount() - _firewallTick) > FirewallInterval && TargetCret != null)
                {
                    _firewallTick = HUtil32.GetTickCount();
                    ReleaseFirewall();
                }

                if ((HUtil32.GetTickCount() - SearchEnemyTick) > 5000 || TargetCret == null)
                {
                    SearchEnemyTick = HUtil32.GetTickCount();
                    SearchTarget();
                }
            }
            base.Run();
        }
    }
}
