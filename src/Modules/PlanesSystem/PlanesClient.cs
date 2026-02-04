using OpenMir2;
using System.Net;
using SystemModule;
using TouchSocket.Core;
using TouchSocket.Sockets;
using TcpClient = TouchSocket.Sockets.TcpClient;

namespace PlanesSystem
{
    /// <summary>
    /// 位面服务器
    /// </summary>
    public class PlanesClient
    {

        private string sRecvMsg = string.Empty;
        private readonly TcpClient _tcpClient;
        private readonly PlanesMessage _groupMessageHandle;

        private static PlanesClient instance;

        public static PlanesClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlanesClient();
                }
                return instance;
            }
        }

        private PlanesClient()
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connected += MsgClientConnect; //成功连接到服务器
            _tcpClient.Disconnected += MsgClientDisconnected; //从服务器断开连接，当连接不成功时不会触发。
            _tcpClient.Received += MsgClientRead;
            _groupMessageHandle = new PlanesMessage();
        }

        public void Initialize()
        {
            _tcpClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost(IPAddress.Parse(SystemShare.Config.MasterSrvAddr), SystemShare.Config.MasterSrvPort))
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger(); //添加一个日志注入
                }));
        }

        public async Task Start()
        {
            try
            {
                if (_tcpClient.Online)
                {
                    return;
                }
                await _tcpClient.ConnectAsync();
            }
            catch (TimeoutException)
            {
                LogService.Error($"链接主分区服务器[{SystemShare.Config.MasterSrvAddr}:{SystemShare.Config.MasterSrvPort}]超时.");
            }
            catch (Exception)
            {
                LogService.Error($"链接主分区服务器[{SystemShare.Config.MasterSrvAddr}:{SystemShare.Config.MasterSrvPort}]失败.");
            }
        }

        public void Run()
        {
            if (_tcpClient.Online)
            {
                DecodeSocStr();
            }
        }

        public void CheckConnected()
        {
            if (SystemShare.Config.nServerNumber > 0)
            {
                if (_tcpClient.Online)
                {
                    return;
                }
                _ = Start();
            }
        }

        private bool IsConnect()
        {
            return _tcpClient.Online;
        }

        private void DecodeSocStr()
        {
            string Str = string.Empty;
            string sNumStr = string.Empty;
            string Head = string.Empty;
            int Ident;
            int sNum;
            const string sExceptionMsg = "[Exception] TFrmSrvMsg::DecodeSocStr";
            if (sRecvMsg.IndexOf(')') <= 0)
            {
                return;
            }
            try
            {
                string BufStr = sRecvMsg;
                sRecvMsg = string.Empty;
                while (BufStr.IndexOf(')') > -1)
                {
                    BufStr = HUtil32.ArrestStringEx(BufStr, "(", ")", ref Str);
                    if (!string.IsNullOrEmpty(Str))
                    {
                        string Body = HUtil32.GetValidStr3(Str, ref Head, HUtil32.Backslash);
                        Body = HUtil32.GetValidStr3(Body, ref sNumStr, HUtil32.Backslash);
                        Ident = HUtil32.StrToInt(Head, 0);
                        sNum = HUtil32.StrToInt(sNumStr, -1);
                        _groupMessageHandle.ProcessData(Ident, sNum, Body);
                    }
                    else
                    {
                        break;
                    }
                }
                sRecvMsg = BufStr + sRecvMsg;
            }
            catch (Exception ex)
            {
                LogService.Error(sExceptionMsg);
                LogService.Error(ex.StackTrace);
            }
        }

        public void SendSocket(string sMsg)
        {
            if (IsConnect())
            {
                byte[] buff = HUtil32.GetBytes("(" + sMsg + ")");
                _tcpClient.Send(buff);
            }
        }

        private Task MsgClientConnect(ITcpClient client, ConnectedEventArgs e)
        {
            LogService.Info("连接主服务器(" + client.RemoteIPHost + ")成功...");
            // 连接成功后发送服务器注册消息
            SendServerRegister();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送服务器注册消息到主位面服务器
        /// </summary>
        private void SendServerRegister()
        {
            try
            {
                // 格式: ISM_SERVERREGISTER/服务器索引/服务器名称/IP/端口/在线人数
                string registerMsg = string.Format("{0}/{1}/{2}/{3}/{4}/{5}",
                    Messages.ISM_SERVERREGISTER,
                    SystemShare.ServerIndex,
                    SystemShare.Config.ServerName,
                    SystemShare.Config.GateAddr,
                    SystemShare.Config.GatePort,
                    SystemShare.WorldEngine.OnlinePlayObject);
                SendSocket(registerMsg);
                LogService.Info($"已发送服务器注册消息: {SystemShare.Config.ServerName}[{SystemShare.ServerIndex}]");
            }
            catch (Exception ex)
            {
                LogService.Error($"发送服务器注册消息失败: {ex.Message}");
            }
        }

        private Task MsgClientDisconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            LogService.Error("节点服务器(" + client.GetIPPort() + ")断开连接...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 接收主机发送过来的消息
        /// </summary>
        private Task MsgClientRead(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            sRecvMsg += HUtil32.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
            return Task.CompletedTask;
        }
    }
}

