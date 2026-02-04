using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NLog;

namespace GameSrv.Services
{
    /// <summary>
    /// 通知接收服务
    /// 接收来自WebApi的实时通知（元宝变化、物品发放等）
    /// </summary>
    public class NotifyReceiver : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private bool _isRunning;

        public NotifyReceiver(int port = 6800)
        {
            _port = port;
        }

        /// <summary>
        /// 启动通知接收服务
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;

                Task.Run(() => AcceptClientsAsync(_cts.Token));

                Logger.Info($"通知接收服务已启动，监听端口: {_port}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"启动通知接收服务失败，端口: {_port}");
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();
            _listener?.Stop();
            Logger.Info("通知接收服务已停止");
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _isRunning)
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync(token);
                    _ = ProcessClientAsync(client, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "接受客户端连接失败");
                }
            }
        }

        private async Task ProcessClientAsync(TcpClient client, CancellationToken token)
        {
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    var buffer = new byte[4096];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    
                    if (bytesRead > 0)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        var result = await ProcessNotifyAsync(json);
                        
                        var response = Encoding.UTF8.GetBytes(result ? "OK\n" : "FAIL\n");
                        await stream.WriteAsync(response, 0, response.Length, token);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "处理通知失败");
            }
        }

        private async Task<bool> ProcessNotifyAsync(string json)
        {
            try
            {
                var message = JsonSerializer.Deserialize<NotifyMessage>(json);
                if (message == null) return false;

                Logger.Debug($"收到通知: Type={message.Type}, CharName={message.CharName}");

                switch (message.Type)
                {
                    case "GAMEGOLD_CHANGE":
                        return await HandleGameGoldChangeAsync(message);

                    case "ITEM_GRANT":
                        return await HandleItemGrantAsync(message);

                    case "NEW_MAIL":
                        return await HandleNewMailAsync(message);

                    case "KICK_PLAYER":
                        return await HandleKickPlayerAsync(message);

                    default:
                        Logger.Warn($"未知通知类型: {message.Type}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"解析通知失败: {json}");
                return false;
            }
        }

        /// <summary>
        /// 处理元宝变化通知
        /// </summary>
        private async Task<bool> HandleGameGoldChangeAsync(NotifyMessage message)
        {
            try
            {
                // 查找在线玩家
                var player = SystemShare.WorldEngine.GetPlayObject(message.CharName);
                if (player != null)
                {
                    // 从数据库重新加载元宝
                    // 或者直接增加元宝（需要解析Data中的Amount）
                    var dataJson = message.Data?.ToString();
                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        var data = JsonSerializer.Deserialize<GameGoldChangeData>(dataJson);
                        if (data != null)
                        {
                            player.GameGold += data.Amount;
                            player.GameGoldChanged();
                            player.SysMsg($"您获得了 {data.Amount} 元宝", MsgColor.Green, MsgType.Hint);
                            Logger.Info($"玩家 {message.CharName} 元宝增加 {data.Amount}，原因: {data.Reason}");
                        }
                    }
                }
                else
                {
                    Logger.Debug($"玩家 {message.CharName} 不在线，元宝变化将在下次登录时生效");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"处理元宝变化失败: {message.CharName}");
                return false;
            }
        }

        /// <summary>
        /// 处理物品发放通知
        /// </summary>
        private async Task<bool> HandleItemGrantAsync(NotifyMessage message)
        {
            try
            {
                var player = SystemShare.WorldEngine.GetPlayObject(message.CharName);
                if (player != null)
                {
                    // TODO: 解析物品列表并发放
                    player.SysMsg("您收到了新物品，请查看背包", MsgColor.Green, MsgType.Hint);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"处理物品发放失败: {message.CharName}");
                return false;
            }
        }

        /// <summary>
        /// 处理新邮件通知
        /// </summary>
        private async Task<bool> HandleNewMailAsync(NotifyMessage message)
        {
            try
            {
                var player = SystemShare.WorldEngine.GetPlayObject(message.CharName);
                if (player != null)
                {
                    player.SysMsg("您有新邮件，请查收", MsgColor.Yellow, MsgType.Hint);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"处理新邮件通知失败: {message.CharName}");
                return false;
            }
        }

        /// <summary>
        /// 处理踢下线通知
        /// </summary>
        private async Task<bool> HandleKickPlayerAsync(NotifyMessage message)
        {
            try
            {
                var player = SystemShare.WorldEngine.GetPlayObject(message.CharName);
                if (player != null)
                {
                    var dataJson = message.Data?.ToString();
                    var reason = "管理员操作";
                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        var data = JsonSerializer.Deserialize<KickData>(dataJson);
                        reason = data?.Reason ?? reason;
                    }

                    player.SysMsg($"您已被踢下线: {reason}", MsgColor.Red, MsgType.Hint);
                    player.BoKickFlag = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"处理踢下线失败: {message.CharName}");
                return false;
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }

    public class NotifyMessage
    {
        public string Type { get; set; }
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public object? Data { get; set; }
    }

    public class GameGoldChangeData
    {
        public int Amount { get; set; }
        public string Reason { get; set; }
    }

    public class KickData
    {
        public string Reason { get; set; }
    }
}
