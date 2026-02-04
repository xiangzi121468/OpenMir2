using OpenMir2;
using SystemModule;
using SystemModule.Actors;

namespace M2Server.Monster.Monsters
{
    /// <summary>
    /// 死灵召唤师 - BOSS级召唤型怪物
    /// 特点：周期性召唤骷髅，HP低于30%进入狂暴模式
    /// Race = 302
    /// </summary>
    public class NecroSummoner : MonsterObject
    {
        private int _summonTick;
        private int _summonedCount;
        private bool _enraged;
        private const int SummonInterval = 12000;     // 12秒召唤一次
        private const int MaxSummons = 6;             // 最多召唤6只
        private const string SummonMonster = "骷髅"; // 召唤的怪物名称 (数据库中的Name)

        public NecroSummoner() : base()
        {
            ViewRange = 10;
            RunTime = 300;
            SearchTime = M2Share.RandomNumber.Random(2000) + 2000;
            _summonTick = HUtil32.GetTickCount();
            _summonedCount = 0;
            _enraged = false;
        }

        private void SummonSkeletons()
        {
            if (_summonedCount >= MaxSummons)
            {
                return;
            }

            int summonCount = _enraged ? 3 : 2; // 狂暴时召唤更多

            for (int i = 0; i < summonCount && _summonedCount < MaxSummons; i++)
            {
                short summonX = (short)(CurrX + M2Share.RandomNumber.Random(5) - 2);
                short summonY = (short)(CurrY + M2Share.RandomNumber.Random(5) - 2);

                var skeleton = SystemShare.WorldEngine.RegenMonsterByName(Envir.MapName, summonX, summonY, SummonMonster);
                if (skeleton != null)
                {
                    _summonedCount++;
                    // 发送召唤特效
                    SendRefMsg(Messages.RM_MAGICFIRE, 0, skeleton.CurrX + (skeleton.CurrY << 16), skeleton.ActorId, 0, "");
                }
            }
        }

        private void CheckEnrage()
        {
            if (!_enraged && WAbil.HP < WAbil.MaxHP * 0.3)
            {
                _enraged = true;
                // 狂暴状态：攻击力提升50%
                WAbil.DC = (ushort)(WAbil.DC * 1.5);
                WAbil.MC = (ushort)(WAbil.MC * 1.5);
                // 改变名称颜色提示狂暴
                NameColor = 249; // 红色
                RefNameColor();
                // 发送狂暴特效
                SendRefMsg(Messages.RM_SPACEMOVE_SHOW2, 0, 0, 0, 0, "");
            }
        }

        protected override bool AttackTarget()
        {
            bool result = false;
            byte btDir = 0;
            if (TargetCret != null && !TargetCret.Death)
            {
                if (GetAttackDir(TargetCret, ref btDir))
                {
                    if ((HUtil32.GetTickCount() - AttackTick) > NextHitTime)
                    {
                        AttackTick = HUtil32.GetTickCount();
                        TargetFocusTick = HUtil32.GetTickCount();
                        Dir = btDir;

                        // 混合物理和魔法伤害
                        int phyPower = GetAttackPower(HUtil32.LoByte(WAbil.DC), (short)(HUtil32.HiByte(WAbil.DC) - HUtil32.LoByte(WAbil.DC)));
                        int magPower = GetAttackPower(HUtil32.LoByte(WAbil.MC), (short)(HUtil32.HiByte(WAbil.MC) - HUtil32.LoByte(WAbil.MC)));

                        int damage = TargetCret.GetHitStruckDamage(this, phyPower);
                        damage += TargetCret.GetMagStruckDamage(this, magPower / 2);

                        if (damage > 0)
                        {
                            TargetCret.StruckDamage(damage);
                            TargetCret.SendStruckDelayMsg(Messages.RM_STRUCK, damage, TargetCret.WAbil.HP, TargetCret.WAbil.MaxHP, ActorId, "", 300);
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
                CheckEnrage();

                // 周期性召唤骷髅
                int summonInterval = _enraged ? SummonInterval / 2 : SummonInterval;
                if ((HUtil32.GetTickCount() - _summonTick) > summonInterval && TargetCret != null)
                {
                    _summonTick = HUtil32.GetTickCount();
                    SummonSkeletons();
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
