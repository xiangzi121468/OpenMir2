using GameGate.Conf;
using System.Threading.Channels;

namespace GameGate.Services
{
    public class ServerManager
    {
        private static readonly ServerManager instance = new ServerManager();
        public static ServerManager Instance => instance;
        /// <summary>
        /// 配置文件
        /// </summary>
        private static ConfigManager ConfigManager => ConfigManager.Instance;
        /// <summary>
        /// 服务器列表
        /// </summary>
        private ServerService[] _serverServices;
        /// <summary>
        /// 消息消费者线程
        /// </summary>
        private ClientMessageWorkThread[] _messageWorkThreads;
        /// <summary>
        /// 运行消息消费线程数
        /// </summary>
        private int RunMessageThreadCount { get; set; }
        /// <summary>
        /// 接收封包（客户端-》网关）
        /// </summary>
        private readonly Channel<ClientPacketMessage> _messageQueue;

        private ServerManager()
        {
            _messageQueue = Channel.CreateUnbounded<ClientPacketMessage>();
        }

        public void Initialize()
        {
            _serverServices = new ServerService[ConfigManager.GateConfig.ServerWorkThread];
            for (int i = 0; i < _serverServices.Length; i++)
            {
                _serverServices[i] = new ServerService(ConfigManager.GateList[i]);
                _serverServices[i].Initialize();
            }
        }

        public void Start(CancellationToken stoppingToken)
        {
            for (int i = 0; i < _serverServices.Length; i++)
            {
                if (_serverServices[i] == null)
                {
                    continue;
                }
                _serverServices[i].Start(stoppingToken);
            }
        }

        public void Stop()
        {
            for (int i = 0; i < _serverServices.Length; i++)
            {
                if (_serverServices[i] == null)
                {
                    continue;
                }
                _serverServices[i].Stop();
            }
        }

        /// <summary>
        /// 添加到客户端消息队列
        /// </summary>
        public Task Send(SessionMessage sendPacket)
        {
            return _serverServices[sendPacket.ServiceId].Send(sendPacket);
        }

        /// <summary>
        /// 客户端消息添加到队列给服务端处理
        /// GameGate -> GameSrv
        /// </summary>
        public void SendMessageQueue(ClientPacketMessage messagePacket)
        {
            _messageQueue.Writer.TryWrite(messagePacket);
        }

        /// <summary>
        /// 消息处理线程数
        /// </summary>
        public static int MessageWorkThreads => ConfigManager.GateConfig.MessageWorkThread;

        /// <summary>
        /// 开启服务消息消费线程
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void StartServerThreadMessageWork(CancellationToken cancellationToken)
        {
            Task[] tasks = new Task[_serverServices.Length];
            for (int i = 0; i < _serverServices.Length; i++)
            {
                tasks[i] = _serverServices[i].ClientThread.StartMessageQueue(cancellationToken);
            }
            Task.WaitAll(tasks, cancellationToken);
        }

        /// <summary>
        /// 开启客户端消息消费线程
        /// </summary>
        public Task StartClientMessageWork(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(() =>
            {
                if (RunMessageThreadCount == ConfigManager.GateConfig.MessageWorkThread)
                {
                    return;
                }
                if (ConfigManager.GateConfig.MessageWorkThread > RunMessageThreadCount)
                {
                    Array.Resize(ref _messageWorkThreads, ConfigManager.GateConfig.MessageWorkThread);
                    for (int i = 0; i < ConfigManager.GateConfig.MessageWorkThread; i++)
                    {
                        if (_messageWorkThreads[i] == null)
                        {
                            _messageWorkThreads[i] = new ClientMessageWorkThread(stoppingToken, _messageQueue.Reader);
                        }
                        if (_messageWorkThreads[i].ThreadState == MessageThreadState.Stop)
                        {
                            _messageWorkThreads[i]?.Start();
                        }
                    }
                }
                else
                {
                    for (int i = _messageWorkThreads.Length - 1; i >= ConfigManager.GateConfig.MessageWorkThread; i--)
                    {
                        if (_messageWorkThreads[i] == null)
                        {
                            continue;
                        }
                        if (_messageWorkThreads[i].ThreadState == MessageThreadState.Runing)
                        {
                            _messageWorkThreads[i]?.Stop();
                            _messageWorkThreads[i] = null;
                        }
                    }
                }
                RunMessageThreadCount = ConfigManager.GateConfig.MessageWorkThread;
            }, stoppingToken);
        }

        public ServerService[] GetServerList()
        {
            return _serverServices;
        }

        /// <summary>
        /// 轮询计数器
        /// </summary>
        private int _roundRobinIndex = 0;

        /// <summary>
        /// 根据配置的分配模式获取客户端线程
        /// </summary>
        public ClientThread GetClientThread(byte serviceId, out int threadId)
        {
            threadId = -1;
            if (!_serverServices.Any())
            {
                return null;
            }

            ServerService[] availableList = _serverServices.Where(x => x.ClientThread.Running == RunningState.Runing).ToArray();
            if (availableList.Length == 0)
            {
                return null;
            }
            if (availableList.Length == 1)
            {
                threadId = 0;
                return availableList[0].ClientThread;
            }
            if (serviceId > 0 && serviceId < availableList.Length)
            {
                threadId = serviceId;
                return availableList[serviceId].ClientThread;
            }

            // 根据配置的分配模式选择网关
            var config = ConfigManager.Instance.GateConfig;
            int selectedIndex = config.AllocationMode switch
            {
                Conf.ClientAllocationMode.RoundRobin => GetRoundRobinIndex(availableList.Length),
                Conf.ClientAllocationMode.LeastConnections => GetLeastConnectionsIndex(availableList),
                Conf.ClientAllocationMode.FillFirst => GetFillFirstIndex(availableList, config.MaxPlayersPerGate),
                Conf.ClientAllocationMode.Weighted => GetWeightedIndex(availableList),
                _ => RandomNumber.GetInstance().Random(availableList.Length) // 默认随机
            };

            threadId = selectedIndex;
            return availableList[selectedIndex].ClientThread;
        }

        /// <summary>
        /// 轮询分配
        /// </summary>
        private int GetRoundRobinIndex(int count)
        {
            int index = Interlocked.Increment(ref _roundRobinIndex) % count;
            if (_roundRobinIndex > 100000)
            {
                Interlocked.Exchange(ref _roundRobinIndex, 0);
            }
            return index;
        }

        /// <summary>
        /// 最小负载分配(分配到在线人数最少的网关)
        /// </summary>
        private int GetLeastConnectionsIndex(ServerService[] services)
        {
            int minIndex = 0;
            int minCount = int.MaxValue;
            for (int i = 0; i < services.Length; i++)
            {
                int count = services[i].ClientThread.SessionCount;
                if (count < minCount)
                {
                    minCount = count;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// 填满优先(先填满一个网关再分配下一个)
        /// </summary>
        private int GetFillFirstIndex(ServerService[] services, int maxPerGate)
        {
            for (int i = 0; i < services.Length; i++)
            {
                if (services[i].ClientThread.SessionCount < maxPerGate)
                {
                    return i;
                }
            }
            // 所有网关都满了，使用最小负载策略
            return GetLeastConnectionsIndex(services);
        }

        /// <summary>
        /// 按权重分配(权重基于剩余容量)
        /// </summary>
        private int GetWeightedIndex(ServerService[] services)
        {
            int maxPlayers = ConfigManager.Instance.GateConfig.MaxPlayersPerGate;
            int totalWeight = 0;
            int[] weights = new int[services.Length];

            for (int i = 0; i < services.Length; i++)
            {
                int remaining = Math.Max(0, maxPlayers - services[i].ClientThread.SessionCount);
                weights[i] = remaining;
                totalWeight += remaining;
            }

            if (totalWeight <= 0)
            {
                return RandomNumber.GetInstance().Random(services.Length);
            }

            int randomWeight = RandomNumber.GetInstance().Random(totalWeight);
            int cumulative = 0;
            for (int i = 0; i < services.Length; i++)
            {
                cumulative += weights[i];
                if (randomWeight < cumulative)
                {
                    return i;
                }
            }

            return services.Length - 1;
        }

        /// <summary>
        /// 客户端消息处理工作线程
        /// </summary>
        private class ClientMessageWorkThread : IDisposable
        {
            private readonly ManualResetEvent _resetEvent;
            private string _threadId;
            private readonly CancellationTokenSource _cts;
            /// <summary>
            /// 接收封包（客户端-》网关）
            /// </summary>
            private readonly ChannelReader<ClientPacketMessage> _messageQueue;
            public MessageThreadState ThreadState;
            private static SessionContainer SessionContainer => SessionContainer.Instance;

            public ClientMessageWorkThread(CancellationToken stoppingToken, ChannelReader<ClientPacketMessage> channel)
            {
                _messageQueue = channel;
                _resetEvent = new ManualResetEvent(true);
                ThreadState = MessageThreadState.Stop;
                _cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                _cts.Token.Register(() =>
                    LogService.Debug($"消息消费线程[{_threadId}]已停止处理.")
                );
            }

            public void Start()
            {
                Task.Factory.StartNew(async () =>
                {
                    _threadId = Guid.NewGuid().ToString("N");
                    ThreadState = MessageThreadState.Runing;
                    LogService.Debug($"消息消费线程[{_threadId}]已启动.");
                    while (await _messageQueue.WaitToReadAsync(_cts.Token))
                    {
                        _resetEvent.WaitOne();
                        if (_messageQueue.TryRead(out ClientPacketMessage message))
                        {
                            ClientSession clientSession = SessionContainer.GetSession(message.ServiceId, message.SessionId);
                            if (clientSession == null)
                            {
                                LogService.Debug($"ServiceId:[{message.ServiceId}] SocketId:[{message.SessionId}] Session会话不存在");
                                return;
                            }
                            if (clientSession.Session == null)
                            {
                                LogService.Debug($"ServiceId:[{message.ServiceId}] SocketId:[{message.SessionId}] Session会话已经失效");
                                return;
                            }
                            try
                            {
                                clientSession.ProcessSessionPacket(message);
                            }
                            catch (Exception e)
                            {
                                LogService.Error(e.Message);
                            }
                        }
                    }
                }, _cts.Token);
            }

            public void Stop()
            {
                ThreadState = MessageThreadState.Stop;
                _resetEvent.Reset();//暂停
                if (_messageQueue.Count > 0)
                {
                    _cts.CancelAfter(_messageQueue.Count * 100);//延时取消消费，防止消息丢失
                }
                else
                {
                    _cts.Cancel();
                }
            }

            public void Dispose()
            {
                _resetEvent.Dispose();
                _cts.Dispose();
            }
        }
    }
}