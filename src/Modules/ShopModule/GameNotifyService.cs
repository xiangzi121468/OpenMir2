using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NLog;

namespace ShopModule
{
    /// <summary>
    /// 游戏服务器通知服务
    /// 用于在充值/发放元宝后实时通知游戏服务器更新玩家数据
    /// </summary>
    public class GameNotifyService : IGameNotifyService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly string _gameServerHost;
        private readonly int _gameServerPort;
        private readonly bool _enabled;

        public GameNotifyService(string gameServerHost = "127.0.0.1", int gameServerPort = 6800, bool enabled = true)
        {
            _gameServerHost = gameServerHost;
            _gameServerPort = gameServerPort;
            _enabled = enabled;
        }

        /// <summary>
        /// 通知游戏服务器刷新玩家元宝
        /// </summary>
        public async Task<bool> NotifyGameGoldChangeAsync(string accountId, string charName, int changeAmount, string reason)
        {
            if (!_enabled)
            {
                Logger.Debug($"游戏通知已禁用，跳过: {charName} +{changeAmount} ({reason})");
                return true;
            }

            try
            {
                var message = new GameNotifyMessage
                {
                    Type = "GAMEGOLD_CHANGE",
                    AccountId = accountId,
                    CharName = charName,
                    Data = new
                    {
                        Amount = changeAmount,
                        Reason = reason,
                        Timestamp = DateTime.Now
                    }
                };

                return await SendNotifyAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"通知游戏服务器失败: {charName} +{changeAmount}");
                return false;
            }
        }

        /// <summary>
        /// 通知游戏服务器发放物品
        /// </summary>
        public async Task<bool> NotifyItemGrantAsync(string accountId, string charName, List<ItemGrant> items, string reason)
        {
            if (!_enabled)
            {
                Logger.Debug($"游戏通知已禁用，跳过物品发放: {charName} ({reason})");
                return true;
            }

            try
            {
                var message = new GameNotifyMessage
                {
                    Type = "ITEM_GRANT",
                    AccountId = accountId,
                    CharName = charName,
                    Data = new
                    {
                        Items = items,
                        Reason = reason,
                        Timestamp = DateTime.Now
                    }
                };

                return await SendNotifyAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"通知游戏服务器发放物品失败: {charName}");
                return false;
            }
        }

        /// <summary>
        /// 通知游戏服务器玩家有新邮件
        /// </summary>
        public async Task<bool> NotifyNewMailAsync(string accountId, string charName)
        {
            if (!_enabled)
                return true;

            try
            {
                var message = new GameNotifyMessage
                {
                    Type = "NEW_MAIL",
                    AccountId = accountId,
                    CharName = charName,
                    Data = new { Timestamp = DateTime.Now }
                };

                return await SendNotifyAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"通知新邮件失败: {charName}");
                return false;
            }
        }

        /// <summary>
        /// 通知游戏服务器踢下线
        /// </summary>
        public async Task<bool> NotifyKickPlayerAsync(string accountId, string charName, string reason)
        {
            if (!_enabled)
                return true;

            try
            {
                var message = new GameNotifyMessage
                {
                    Type = "KICK_PLAYER",
                    AccountId = accountId,
                    CharName = charName,
                    Data = new { Reason = reason, Timestamp = DateTime.Now }
                };

                return await SendNotifyAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"通知踢下线失败: {charName}");
                return false;
            }
        }

        /// <summary>
        /// 发送通知到游戏服务器
        /// </summary>
        private async Task<bool> SendNotifyAsync(GameNotifyMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var data = Encoding.UTF8.GetBytes(json + "\n");

                using var client = new TcpClient();
                client.SendTimeout = 5000;
                client.ReceiveTimeout = 5000;

                await client.ConnectAsync(_gameServerHost, _gameServerPort);
                
                var stream = client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();

                // 等待响应
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Logger.Debug($"游戏服务器响应: {response}");

                return response.Contains("OK") || response.Contains("success");
            }
            catch (SocketException ex)
            {
                Logger.Warn($"无法连接游戏服务器 {_gameServerHost}:{_gameServerPort} - {ex.Message}");
                // 连接失败不影响业务，只记录日志
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "发送游戏通知失败");
                return false;
            }
        }
    }

    public interface IGameNotifyService
    {
        Task<bool> NotifyGameGoldChangeAsync(string accountId, string charName, int changeAmount, string reason);
        Task<bool> NotifyItemGrantAsync(string accountId, string charName, List<ItemGrant> items, string reason);
        Task<bool> NotifyNewMailAsync(string accountId, string charName);
        Task<bool> NotifyKickPlayerAsync(string accountId, string charName, string reason);
    }

    public class GameNotifyMessage
    {
        public string Type { get; set; }
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public object Data { get; set; }
    }

    public class ItemGrant
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Count { get; set; }
    }
}
