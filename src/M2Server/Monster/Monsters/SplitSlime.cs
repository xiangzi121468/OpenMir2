using OpenMir2;
using SystemModule;

namespace M2Server.Monster.Monsters
{
    /// <summary>
    /// 分裂史莱姆 - 死亡分裂机制
    /// 特点：死亡时分裂成多个小史莱姆
    /// Race = 303
    /// </summary>
    public class SplitSlime : AtMonster
    {
        private int _splitLevel; // 分裂等级 0=大 1=中 2=小(不再分裂)
        private const string SmallSlimeName = "小史莱姆";
        private const string TinySlimeName = "迷你史莱姆";

        public SplitSlime() : base()
        {
            ViewRange = 6;
            _splitLevel = 0;
        }

        /// <summary>
        /// 设置分裂等级，用于分裂后的小史莱姆
        /// </summary>
        public void SetSplitLevel(int level)
        {
            _splitLevel = level;
            // 根据等级调整属性
            if (_splitLevel > 0)
            {
                WAbil.MaxHP = (ushort)(WAbil.MaxHP / (1 + _splitLevel));
                WAbil.HP = WAbil.MaxHP;
                WAbil.DC = (ushort)(WAbil.DC * 0.7);
            }
        }

        public override void Die()
        {
            // 死亡前执行分裂
            if (_splitLevel < 2)
            {
                int splitCount = M2Share.RandomNumber.Random(2) + 2; // 分裂2-3只
                string spawnName = _splitLevel == 0 ? SmallSlimeName : TinySlimeName;

                for (int i = 0; i < splitCount; i++)
                {
                    short spawnX = (short)(CurrX + M2Share.RandomNumber.Random(3) - 1);
                    short spawnY = (short)(CurrY + M2Share.RandomNumber.Random(3) - 1);

                    var newSlime = SystemShare.WorldEngine.RegenMonsterByName(Envir.MapName, spawnX, spawnY, spawnName);
                    if (newSlime is SplitSlime splitSlime)
                    {
                        splitSlime.SetSplitLevel(_splitLevel + 1);
                    }
                }

                // 分裂特效
                SendRefMsg(Messages.RM_MAGICFIRE, 0, CurrX + (CurrY << 16), 0, 0, "");
            }

            base.Die();
        }
    }
}
