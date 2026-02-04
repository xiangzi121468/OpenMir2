using M2Server;
using OpenMir2;
using OpenMir2.Data;
using SystemModule.Actors;

namespace RobotSystem
{
    public class RobotProcessor : TimerScheduledService
    {

        /// <summary>
        /// 假人列表
        /// </summary>
        public readonly Queue<RoBotLogon> RobotLogonQueue;
        public int RobotLogonQueueCount => RobotLogonQueue.Count;
        public int ProcessedMonsters { get; private set; }
        /// <summary>
        /// 处理假人间隔
        /// </summary>
        public long RobotLogonTick { get; set; }
        private int ProcBotHubIdx { get; set; }
        protected readonly IList<IRobotPlayer> BotPlayObjectList;
        public int RobotPlayerCount => BotPlayObjectList.Count;

        public RobotProcessor() : base(TimeSpan.FromMilliseconds(100), "RobotProcessor")
        {
            RobotLogonQueue = new Queue<RoBotLogon>();
            BotPlayObjectList = new List<IRobotPlayer>();
        }

        public override void Initialize(CancellationToken cancellationToken)
        {
            LogService.Debug("初始化Robot(机器人)处理插件...");
        }

        protected override void Startup(CancellationToken stoppingToken)
        {
            LogService.Info("机器人管理线程初始化完成...");
        }

        protected override void Stopping(CancellationToken stoppingToken)
        {
            LogService.Info("机器人管理线程停止ֹ...");
        }

        protected override Task ExecuteInternal(CancellationToken stoppingToken)
        {
            try
            {
                ProcessedMonsters = 0;
                ProcessRobotPlayData();
                /*foreach (var map in Kernel.MapMgr.GameMaps.Values)
                    ProcessedMonsters += await map.OnTimerAsync();
                await Kernel.RoleManager.OnRoleTimerAsync();*/
            }
            catch (Exception ex)
            {
                LogService.Error("[Exception] RobotProcessor::ExecuteInternal");
                LogService.Error(ex);
            }
            return Task.CompletedTask;
        }

        public void AddRobotLogon(RoBotLogon ai)
        {
            RobotLogonQueue.Enqueue(ai);
        }

        private void RegenRobotPlayer(RoBotLogon ai)
        {
            IRobotPlayer playObject = CreateRobotPlayObject(ai);
            if (playObject != null)
            {
                short homeX = 0;
                short homeY = 0;
                //playObject.HomeMap = GetHomeInfo(playObject.Job, ref homeX, ref homeY);
                playObject.HomeX = homeX;
                playObject.HomeY = homeY;
                playObject.MapFileName = playObject.HomeMap;
                playObject.UserAccount = "假人" + ai.sChrName;
                //playObject.Start(FindPathType.Dynamic);
                BotPlayObjectList.Add(playObject);
            }
        }

        public void ProcessRobotPlayData()
        {
            const string sExceptionMsg = "[Exception] WorldServer::ProcessRobotPlayData";
            //人工智障开始登陆
            if (RobotLogonQueue.Count > 0)
            {
                if (HUtil32.GetTickCount() - RobotLogonTick > 1000)
                {
                    RobotLogonTick = HUtil32.GetTickCount();
                    if (RobotLogonQueue.Count > 0)
                    {
                        RegenRobotPlayer(RobotLogonQueue.Dequeue());
                    }
                }
            }
            try
            {
                int dwCurTick = HUtil32.GetTickCount();
                int nIdx = ProcBotHubIdx;
                bool boCheckTimeLimit = false;
                int dwCheckTime = HUtil32.GetTickCount();
                while (true)
                {
                    if (BotPlayObjectList.Count <= nIdx)
                    {
                        break;
                    }

                    IRobotPlayer robotPlayer = BotPlayObjectList[nIdx];
                    if (dwCurTick - robotPlayer.RunTick > robotPlayer.RunTime)
                    {
                        robotPlayer.RunTick = dwCurTick;
                        if (!robotPlayer.Ghost)
                        {
                            if (!robotPlayer.LoginNoticeOk)
                            {
                                robotPlayer.RunNotice();
                            }
                            else
                            {
                                if (!robotPlayer.BoReadyRun)
                                {
                                    robotPlayer.BoReadyRun = true;
                                    robotPlayer.UserLogon();
                                }
                                else
                                {
                                    if ((HUtil32.GetTickCount() - robotPlayer.SearchTick) > robotPlayer.SearchTime)
                                    {
                                        robotPlayer.SearchTick = HUtil32.GetTickCount();
                                        robotPlayer.SearchViewRange();
                                        robotPlayer.GameTimeChanged();
                                    }
                                    robotPlayer.Run();
                                }
                            }
                        }
                        else
                        {
                            BotPlayObjectList.Remove(robotPlayer);
                            robotPlayer.Disappear();
                            //AddToHumanFreeList(robotPlayer);
                            //robotPlayer.DealCancelA();
                            //SaveHumanRcd(robotPlayer);
                            // GameShare.SocketMgr.CloseUser(robotPlayer.GateIdx, robotPlayer.SocketId);
                            // SendServerGroupMsg(Messages.SS_202, M2Share.ServerIndex, robotPlayer.ChrName);
                            continue;
                        }
                    }
                    nIdx++;
                    if ((HUtil32.GetTickCount() - dwCheckTime) > M2Share.HumLimit)
                    {
                        boCheckTimeLimit = true;
                        ProcBotHubIdx = nIdx;
                        break;
                    }
                }
                if (!boCheckTimeLimit)
                {
                    ProcBotHubIdx = 0;
                }
            }
            catch (Exception ex)
            {
                LogService.Error(sExceptionMsg);
                LogService.Error(ex.StackTrace);
            }
        }

        private static IRobotPlayer CreateRobotPlayObject(RoBotLogon ai)
        {
            try
            {
                var envirnoment = SystemShare.MapMgr.FindMap(ai.sMapName);
                if (envirnoment == null)
                {
                    LogService.Warn($"机器人创建失败: 地图 {ai.sMapName} 不存在");
                    return null;
                }

                var robot = new Services.RobotPlayer();
                robot.Envir = envirnoment;
                robot.MapName = ai.sMapName;
                robot.CurrX = ai.nX;
                robot.CurrY = ai.nY;
                robot.Dir = (byte)SystemShare.RandomNumber.Random(8);
                robot.ChrName = ai.sChrName;
                robot.WAbil = robot.Abil;
                
                if (SystemShare.RandomNumber.Random(100) < robot.CoolEyeCode)
                {
                    robot.CoolEye = true;
                }

                robot.ConfigFileName = ai.sConfigFileName;
                robot.FilePath = ai.sFilePath;
                robot.ConfigListFileName = ai.sConfigListFileName;
                robot.HeroConfigListFileName = ai.sHeroConfigListFileName;
                robot.Initialize();
                robot.RecalcLevelAbilitys();
                robot.RecalcAbilitys();
                robot.Abil.HP = robot.Abil.MaxHP;
                robot.Abil.MP = robot.Abil.MaxMP;

                if (robot.AddtoMapSuccess)
                {
                    bool mapSuccess = false;
                    int stepSize = envirnoment.Width < 50 ? 2 : 3;
                    int margin = envirnoment.Height < 250 ? (envirnoment.Height < 30 ? 2 : 20) : 50;
                    
                    int tryCount = 0;
                    while (tryCount < 31)
                    {
                        if (!envirnoment.CanWalk(robot.CurrX, robot.CurrY, false))
                        {
                            // 尝试找一个可以行走的位置
                            if ((envirnoment.Width - margin - 1) > robot.CurrX)
                            {
                                robot.CurrX += (short)stepSize;
                            }
                            else
                            {
                                robot.CurrX = (short)(SystemShare.RandomNumber.Random(envirnoment.Width / 2) + margin);
                                if (envirnoment.Height - margin - 1 > robot.CurrY)
                                {
                                    robot.CurrY += (short)stepSize;
                                }
                                else
                                {
                                    robot.CurrY = (short)(SystemShare.RandomNumber.Random(envirnoment.Height / 2) + margin);
                                }
                            }
                        }
                        else
                        {
                            mapSuccess = envirnoment.AddMapObject(robot.CurrX, robot.CurrY, robot.CellType, robot.ActorId, robot);
                            break;
                        }
                        tryCount++;
                    }

                    if (!mapSuccess)
                    {
                        LogService.Warn($"机器人 {ai.sChrName} 无法添加到地图 {ai.sMapName}");
                        return null;
                    }
                }

                LogService.Info($"机器人 {ai.sChrName} 创建成功, 地图: {ai.sMapName}({ai.nX},{ai.nY})");
                return robot;
            }
            catch (Exception ex)
            {
                LogService.Error($"创建机器人失败: {ex.Message}");
                return null;
            }
        }
    }
}