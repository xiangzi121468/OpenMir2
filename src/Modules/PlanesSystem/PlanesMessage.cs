using OpenMir2;
using SystemModule.Actors;

namespace PlanesSystem
{
    public class PlanesMessage
    {
        private readonly IPlayerActor PlayObject = null;

        public PlanesMessage()
        {

        }

        public void ProcessData(int ident, int serverNum, string body)
        {
            switch (ident)
            {
                case Messages.ISM_GROUPSERVERHEART:
                    ServerHeartMessage(serverNum, body);
                    break;
                case Messages.ISM_USERSERVERCHANGE:
                    MsgGetUserServerChange(serverNum, body);
                    break;
                case Messages.ISM_CHANGESERVERRECIEVEOK:
                    MsgGetUserChangeServerRecieveOk(serverNum, body);
                    break;
                case Messages.ISM_USERLOGON:
                    MsgGetUserLogon(serverNum, body);
                    break;
                case Messages.ISM_USERLOGOUT:
                    MsgGetUserLogout(serverNum, body);
                    break;
                case Messages.ISM_WHISPER:
                    MsgGetWhisper(serverNum, body);
                    break;
                case Messages.ISM_GMWHISPER:
                    MsgGetGMWhisper(serverNum, body);
                    break;
                case Messages.ISM_LM_WHISPER:
                    MsgGetLoverWhisper(serverNum, body);
                    break;
                case Messages.ISM_SYSOPMSG:
                    MsgGetSysopMsg(serverNum, body);
                    break;
                case Messages.ISM_ADDGUILD:
                    MsgGetAddGuild(serverNum, body);
                    break;
                case Messages.ISM_DELGUILD:
                    MsgGetDelGuild(serverNum, body);
                    break;
                case Messages.ISM_RELOADGUILD:
                    MsgGetReloadGuild(serverNum, body);
                    break;
                case Messages.ISM_GUILDMSG:
                    MsgGetGuildMsg(serverNum, body);
                    break;
                case Messages.ISM_GUILDWAR:
                    MsgGetGuildWarInfo(serverNum, body);
                    break;
                case Messages.ISM_CHATPROHIBITION:
                    MsgGetChatProhibition(serverNum, body);
                    break;
                case Messages.ISM_CHATPROHIBITIONCANCEL:
                    MsgGetChatProhibitionCancel(serverNum, body);
                    break;
                case Messages.ISM_CHANGECASTLEOWNER:
                    MsgGetChangeCastleOwner(serverNum, body);
                    break;
                case Messages.ISM_RELOADCASTLEINFO:
                    MsgGetReloadCastleAttackers(serverNum);
                    break;
                case Messages.ISM_RELOADADMIN:
                    MsgGetReloadAdmin();
                    break;
                case Messages.ISM_MARKETOPEN:
                    MsgGetMarketOpen(true);
                    break;
                case Messages.ISM_MARKETCLOSE:
                    MsgGetMarketOpen(false);
                    break;
                case Messages.ISM_RELOADCHATLOG:
                    MsgGetReloadChatLog();
                    break;
                case Messages.ISM_USER_INFO:
                case Messages.ISM_FRIEND_INFO:
                case Messages.ISM_FRIEND_DELETE:
                case Messages.ISM_FRIEND_OPEN:
                case Messages.ISM_FRIEND_CLOSE:
                case Messages.ISM_FRIEND_RESULT:
                case Messages.ISM_TAG_SEND:
                case Messages.ISM_TAG_RESULT:
                    MsgGetUserMgr(serverNum, body, ident);
                    break;
                case Messages.ISM_RELOADMAKEITEMLIST:
                    MsgGetReloadMakeItemList();
                    break;
                case Messages.ISM_GUILDMEMBER_RECALL:
                    MsgGetGuildMemberRecall(serverNum, body);
                    break;
                case Messages.ISM_RELOADGUILDAGIT:
                    MsgGetReloadGuildAgit(serverNum, body);
                    break;
                case Messages.ISM_LM_LOGIN:
                    MsgGetLoverLogin(serverNum, body);
                    break;
                case Messages.ISM_LM_LOGOUT:
                    MsgGetLoverLogout(serverNum, body);
                    break;
                case Messages.ISM_LM_LOGIN_REPLY:
                    MsgGetLoverLoginReply(serverNum, body);
                    break;
                case Messages.ISM_LM_KILLED_MSG:
                    MsgGetLoverKilledMsg(serverNum, body);
                    break;
                case Messages.ISM_RECALL:
                    MsgGetRecall(serverNum, body);
                    break;
                case Messages.ISM_REQUEST_RECALL:
                    MsgGetRequestRecall(serverNum, body);
                    break;
                case Messages.ISM_REQUEST_LOVERRECALL:
                    MsgGetRequestLoverRecall(serverNum, body);
                    break;
                case Messages.ISM_GRUOPMESSAGE:
                    //LogService.Info("跨服消息");
                    break;
            }
        }

        private static void ServerHeartMessage(int sNum, string Body)
        {
            // 回复心跳消息，保持连接
            if (SystemShare.Config.nServerNumber > 0)
            {
                string heartbeatReply = $"{Messages.ISM_GROUPSERVERHEART}/{SystemShare.ServerIndex}/{HUtil32.GetTickCount()}";
                PlanesClient.Instance.SendSocket(heartbeatReply);
                LogService.Debug($"收到位面服务器心跳[{sNum}]，已回复");
            }
        }

        private static void MsgGetUserServerChange(int sNum, string Body)
        {
            int shifttime = HUtil32.GetTickCount();
            string ufilename = Body;
            if (SystemShare.ServerIndex == sNum)
            {
                try
                {
                    // 添加跨服切换数据
                    LogService.Info($"收到玩家跨服切换请求: {ufilename}");
                    // 通知源服务器已准备接收
                    string replyMsg = $"{Messages.ISM_CHANGESERVERRECIEVEOK}/{SystemShare.ServerIndex}/{ufilename}";
                    PlanesClient.Instance.SendSocket(replyMsg);
                }
                catch (Exception ex)
                {
                    LogService.Error($"处理跨服切换消息失败: {ex.Message}");
                }
            }
        }

        private static void MsgGetUserChangeServerRecieveOk(int sNum, string Body)
        {
            string ufilename = Body;
            // 目标服务器已确认接收，可以断开玩家连接
            LogService.Info($"跨服切换确认: 玩家数据文件 {ufilename} 已被服务器 {sNum} 接收");
            // SystemShare.WorldEngine.GetIsmChangeServerReceive(ufilename);
        }

        private static void MsgGetUserLogon(int sNum, string Body)
        {
            string uname = Body;
            // 记录其他服务器的玩家登录信息，用于跨服查询
            LogService.Debug($"跨服玩家登录通知: {uname} 登录到服务器 {sNum}");
            // 可用于好友在线状态、情侣在线状态等功能
        }

        private static void MsgGetUserLogout(int sNum, string Body)
        {
            string uname = Body;
            // 记录其他服务器的玩家登出信息
            LogService.Debug($"跨服玩家登出通知: {uname} 从服务器 {sNum} 登出");
        }

        private static void MsgGetWhisper(int sNum, string Body)
        {
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                if (!string.IsNullOrEmpty(uname))
                {
                    IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                    if (hum != null && hum.HearWhisper)
                    {
                        // 发送跨服私聊消息
                        hum.SendMsg(hum, Messages.RM_WHISPER, 0, 0, 0, 0, Str);
                        LogService.Debug($"跨服私聊: 发送给 {uname}");
                    }
                }
            }
        }

        private static void MsgGetGMWhisper(int sNum, string Body)
        {
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                if (!string.IsNullOrEmpty(uname))
                {
                    IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                    if (hum != null && hum.HearWhisper)
                    {
                        // 发送GM跨服私聊消息
                        hum.SendMsg(hum, Messages.RM_WHISPER, 0, 0, 0, 0, "[GM]" + Str);
                        LogService.Debug($"GM跨服私聊: 发送给 {uname}");
                    }
                }
            }
        }

        private static void MsgGetLoverWhisper(int sNum, string Body)
        {
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                if (!string.IsNullOrEmpty(uname))
                {
                    IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                    if (hum != null && hum.HearWhisper)
                    {
                        // 发送情侣跨服私聊消息
                        hum.SendMsg(hum, Messages.RM_WHISPER, 0, 0, 0, 0, "[情侣]" + Str);
                        LogService.Debug($"情侣跨服私聊: 发送给 {uname}");
                    }
                }
            }
        }

        private static void MsgGetSysopMsg(int sNum, string Body)
        {
            // 跨服系统广播消息
            if (!string.IsNullOrEmpty(Body))
            {
                SystemShare.WorldEngine.SendBroadCastMsg(Body, SystemModule.Enums.MsgType.System);
                LogService.Info($"收到跨服系统广播: {Body}");
            }
        }

        private static void MsgGetAddGuild(int sNum, string Body)
        {
            string gname = string.Empty;
            string mname = HUtil32.GetValidStr3(Body, ref gname, '/');
            if (!string.IsNullOrEmpty(gname) && !string.IsNullOrEmpty(mname))
            {
                // 跨服同步：其他服务器创建了新行会
                SystemShare.GuildMgr.AddGuild(gname, mname);
                LogService.Info($"跨服行会同步: 添加行会 {gname}, 会长 {mname}");
            }
        }

        private static void MsgGetDelGuild(int sNum, string Body)
        {
            string gname = Body;
            if (!string.IsNullOrEmpty(gname))
            {
                // 跨服同步：其他服务器删除了行会
                SystemShare.GuildMgr.DelGuild(gname);
                LogService.Info($"跨服行会同步: 删除行会 {gname}");
            }
        }

        private static void MsgGetReloadGuild(int sNum, string Body)
        {
            string gname = Body;
            if (!string.IsNullOrEmpty(gname))
            {
                var guild = SystemShare.GuildMgr.FindGuild(gname);
                if (guild != null)
                {
                    // 重新加载行会数据
                    guild.LoadGuild();
                    LogService.Info($"跨服行会同步: 重载行会 {gname}");
                }
            }
        }

        private static void MsgGetGuildMsg(int sNum, string Body)
        {
            string gname = string.Empty;
            string Str = Body;
            Str = HUtil32.GetValidStr3(Str, ref gname, '/');
            if (!string.IsNullOrEmpty(gname))
            {
                var guild = SystemShare.GuildMgr.FindGuild(gname);
                if (guild != null)
                {
                    // 发送行会跨服消息
                    guild.SendGuildMsg(Str);
                    LogService.Debug($"跨服行会消息: {gname} -> {Str}");
                }
            }
        }

        private static void MsgGetGuildWarInfo(int sNum, string Body)
        {
            string gname = string.Empty;
            string warguildname = string.Empty;
            string startTimeStr = string.Empty;
            string remainTimeStr = string.Empty;
            
            if (sNum == 0)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref gname, '/');
                Str = HUtil32.GetValidStr3(Str, ref warguildname, '/');
                Str = HUtil32.GetValidStr3(Str, ref startTimeStr, '/');
                remainTimeStr = Str;
                
                if (!string.IsNullOrEmpty(gname) && !string.IsNullOrEmpty(warguildname))
                {
                    var guild = SystemShare.GuildMgr.FindGuild(gname);
                    var warGuild = SystemShare.GuildMgr.FindGuild(warguildname);
                    
                    if (guild != null && warGuild != null)
                    {
                        int startTime = HUtil32.StrToInt(startTimeStr, 0);
                        int remainTime = HUtil32.StrToInt(remainTimeStr, 0);
                        
                        // 同步行会战信息
                        guild.StartGuildWar(warGuild, remainTime);
                        LogService.Info($"[行会战同步] {gname} <-> {warguildname}, 开始时间: {startTime}, 持续: {remainTime}分钟");
                    }
                }
            }
        }

        private void MsgGetChatProhibition(int sNum, string Body)
        {
            string whostr = string.Empty;
            string minstr = string.Empty;
            string Str = Body;
            Str = HUtil32.GetValidStr3(Str, ref whostr, '/');
            Str = HUtil32.GetValidStr3(Str, ref minstr, '/');
            if (!string.IsNullOrEmpty(whostr))
            {
                // 跨服禁言同步
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(whostr);
                if (hum != null)
                {
                    int minutes = HUtil32.StrToInt(minstr, 5);
                    hum.ShutupTime = HUtil32.GetTickCount() + minutes * 60 * 1000;
                    hum.SysMsg($"你已被禁言 {minutes} 分钟", SystemModule.Enums.MsgColor.Red, SystemModule.Enums.MsgType.Hint);
                    LogService.Info($"跨服禁言: {whostr} 被禁言 {minutes} 分钟");
                }
            }
        }

        private static void MsgGetChatProhibitionCancel(int sNum, string Body)
        {
            string whostr = Body;
            if (!string.IsNullOrEmpty(whostr))
            {
                // 跨服取消禁言
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(whostr);
                if (hum != null)
                {
                    hum.ShutupTime = 0;
                    hum.SysMsg("禁言已解除", SystemModule.Enums.MsgColor.Green, SystemModule.Enums.MsgType.Hint);
                    LogService.Info($"跨服解除禁言: {whostr}");
                }
            }
        }

        /// <summary>
        /// 处理城堡所有权变更消息
        /// </summary>
        /// <param name="sNum">服务器编号</param>
        /// <param name="Body">消息内容: 城堡名称/新行会名称</param>
        private static void MsgGetChangeCastleOwner(int sNum, string Body)
        {
            if (string.IsNullOrEmpty(Body))
            {
                return;
            }

            try
            {
                string castleName = string.Empty;
                string guildName = HUtil32.GetValidStr3(Body, ref castleName, '/');

                if (string.IsNullOrEmpty(castleName))
                {
                    return;
                }

                // 查找城堡
                var castle = SystemShare.CastleMgr.Find(castleName);
                if (castle == null)
                {
                    LogService.Warn($"收到城堡变更消息，但城堡未找到: {castleName}");
                    return;
                }

                // 查找新的行会
                if (!string.IsNullOrEmpty(guildName))
                {
                    var guild = SystemShare.GuildMgr.FindGuild(guildName);
                    if (guild != null)
                    {
                        castle.MasterGuild = guild;
                        castle.OwnGuild = guildName;
                        castle.Save();
                        LogService.Info($"城堡 {castleName} 所有权已变更为行会 {guildName} (来自服务器 {sNum})");
                    }
                    else
                    {
                        LogService.Warn($"收到城堡变更消息，但行会未找到: {guildName}");
                    }
                }
                else
                {
                    // 清除城堡所有权
                    castle.MasterGuild = null;
                    castle.OwnGuild = string.Empty;
                    castle.Save();
                    LogService.Info($"城堡 {castleName} 所有权已清除 (来自服务器 {sNum})");
                }
            }
            catch (Exception ex)
            {
                LogService.Error($"处理城堡所有权变更消息失败: {ex.Message}");
            }
        }

        private static void MsgGetReloadCastleAttackers(int sNum)
        {
            // 重新加载城堡攻城者列表
            SystemShare.CastleMgr.Initialize();
            LogService.Info("跨服通知: 重新加载城堡攻城信息");
        }

        private static void MsgGetReloadAdmin()
        {
            // 重新加载管理员列表
            // LocalDb.LoadAdminList();
            LogService.Info("跨服通知: 重新加载管理员列表");
        }

        private static void MsgGetReloadChatLog()
        {
            // 重新加载聊天日志配置
            LogService.Info("跨服通知: 重新加载聊天日志");
        }

        private static void MsgGetUserMgr(int sNum, string Body, int Ident_)
        {
            string UserName = string.Empty;
            string Str = Body;
            string msgbody = HUtil32.GetValidStr3(Str, ref UserName, '/');
            
            // 处理用户管理相关的跨服消息(好友、标签等)
            switch (Ident_)
            {
                case Messages.ISM_FRIEND_INFO:
                    LogService.Debug($"跨服好友信息: {UserName}");
                    break;
                case Messages.ISM_FRIEND_DELETE:
                    LogService.Debug($"跨服好友删除: {UserName}");
                    break;
                case Messages.ISM_FRIEND_OPEN:
                case Messages.ISM_FRIEND_CLOSE:
                    LogService.Debug($"跨服好友状态变更: {UserName}");
                    break;
                default:
                    LogService.Debug($"跨服用户管理消息: {Ident_} -> {UserName}");
                    break;
            }
        }

        private static void MsgGetReloadMakeItemList()
        {
            // 重新加载物品合成列表
            // GameShare.LocalDb.LoadMakeItem();
            LogService.Info("跨服通知: 重新加载物品合成列表");
        }

        private static void MsgGetGuildMemberRecall(int sNum, string Body)
        {
            string dxstr = string.Empty;
            string dystr = string.Empty;
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                string mapName = string.Empty;
                Str = HUtil32.GetValidStr3(Str, ref mapName, '/');
                Str = HUtil32.GetValidStr3(Str, ref dxstr, '/');
                dystr = Str;
                short dx = HUtil32.StrToInt16(dxstr, 0);
                short dy = HUtil32.StrToInt16(dystr, 0);
                
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                if (hum != null)
                {
                    if (hum.AllowGuildReCall)
                    {
                        hum.SendRefMsg(Messages.RM_SPACEMOVE_FIRE, 0, 0, 0, 0, "");
                        hum.SpaceMove(mapName, dx, dy, 0);
                        LogService.Info($"行会成员跨服召回: {uname} -> {mapName}({dx},{dy})");
                    }
                    else
                    {
                        hum.SysMsg("你未开启行会召回功能", SystemModule.Enums.MsgColor.Red, SystemModule.Enums.MsgType.Hint);
                    }
                }
            }
        }

        private static void MsgGetReloadGuildAgit(int sNum, string Body)
        {
            // 重新加载行会领地数据
            LogService.Info("跨服通知: 重新加载行会领地数据");
        }

        private static void MsgGetLoverLogin(int sNum, string Body)
        {
            string uname = string.Empty;
            string lovername = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                lovername = Str;
                
                // 查找情侣是否在本服务器
                IPlayerActor humlover = SystemShare.WorldEngine.GetPlayObject(lovername);
                if (humlover != null)
                {
                    // 回复情侣登录信息
                    string replyMsg = $"{Messages.ISM_LM_LOGIN_REPLY}/{SystemShare.ServerIndex}/{lovername}/{uname}/{humlover.Envir.MapDesc}";
                    PlanesClient.Instance.SendSocket(replyMsg);
                    LogService.Debug($"情侣登录通知: {uname} 的情侣 {lovername} 在本服务器");
                }
            }
        }

        private static void MsgGetLoverLogout(int sNum, string Body)
        {
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                string lovername = Str;
                
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(lovername);
                if (hum != null)
                {
                    hum.SysMsg($"你的情侣 {uname} 已离线", SystemModule.Enums.MsgColor.Pink, SystemModule.Enums.MsgType.Hint);
                    LogService.Debug($"情侣离线通知: {lovername} 的情侣 {uname} 已离线");
                }
            }
        }

        private static void MsgGetLoverLoginReply(int sNum, string Body)
        {
            string uname = string.Empty;
            string lovername = string.Empty;
            string mapDesc = string.Empty;
            
            string Str = Body;
            Str = HUtil32.GetValidStr3(Str, ref uname, '/');
            Str = HUtil32.GetValidStr3(Str, ref lovername, '/');
            mapDesc = Str;
            
            if (sNum == SystemShare.ServerIndex && !string.IsNullOrEmpty(uname))
            {
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                if (hum != null)
                {
                    // 通知玩家其情侣已在其他服务器上线
                    hum.SysMsg($"你的情侣 {lovername} 已在 {mapDesc} 上线", SystemModule.Enums.MsgColor.Pink, SystemModule.Enums.MsgType.Hint);
                    LogService.Debug($"情侣上线通知: {uname} 的情侣 {lovername} 上线");
                }
            }
        }

        private static void MsgGetLoverKilledMsg(int sNum, string Body)
        {
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                string killedMsg = Str;
                
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                if (hum != null)
                {
                    // 通知玩家其情侣被杀
                    hum.SysMsg(killedMsg, SystemModule.Enums.MsgColor.Red, SystemModule.Enums.MsgType.Hint);
                    LogService.Debug($"情侣被杀通知: {uname} -> {killedMsg}");
                }
            }
        }

        private static void MsgGetRecall(int sNum, string Body)
        {
            string dxstr = string.Empty;
            string dystr = string.Empty;
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                string mapName = string.Empty;
                Str = HUtil32.GetValidStr3(Str, ref mapName, '/');
                Str = HUtil32.GetValidStr3(Str, ref dxstr, '/');
                dystr = Str;
                short dx = HUtil32.StrToInt16(dxstr, 0);
                short dy = HUtil32.StrToInt16(dystr, 0);
                
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                if (hum != null)
                {
                    // 跨服传送玩家
                    hum.SendRefMsg(Messages.RM_SPACEMOVE_FIRE, 0, 0, 0, 0, "");
                    hum.SpaceMove(mapName, dx, dy, 0);
                    LogService.Info($"跨服召回: {uname} -> {mapName}({dx},{dy})");
                }
            }
        }

        private static void MsgGetRequestRecall(int sNum, string Body)
        {
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                string targetName = Str;
                
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                if (hum != null)
                {
                    // 处理跨服召回请求
                    hum.RecallHuman(targetName);
                    LogService.Info($"跨服召回请求: {uname} 召回 {targetName}");
                }
            }
        }

        private static void MsgGetRequestLoverRecall(int sNum, string Body)
        {
            string uname = string.Empty;
            if (sNum == SystemShare.ServerIndex)
            {
                string Str = Body;
                Str = HUtil32.GetValidStr3(Str, ref uname, '/');
                string loverName = Str;
                
                IPlayerActor hum = SystemShare.WorldEngine.GetPlayObject(uname);
                if (hum != null)
                {
                    // 情侣召回，需检查地图是否允许
                    if (!hum.Envir.Flag.NoReCall)
                    {
                        hum.RecallHuman(loverName);
                        LogService.Info($"情侣跨服召回: {uname} 召回情侣 {loverName}");
                    }
                    else
                    {
                        hum.SysMsg("当前地图不允许召回", SystemModule.Enums.MsgColor.Red, SystemModule.Enums.MsgType.Hint);
                    }
                }
            }
        }

        private static void MsgGetMarketOpen(bool WantOpen)
        {
            // 跨服控制拍卖行开关
            SystemShare.Config.EnableMarket = WantOpen;
            LogService.Info($"跨服通知: 拍卖行状态变更为 {(WantOpen ? "开启" : "关闭")}");
            
            // 广播给所有在线玩家
            string msg = WantOpen ? "拍卖行已开放" : "拍卖行已关闭维护";
            SystemShare.WorldEngine.SendBroadCastMsg(msg, SystemModule.Enums.MsgType.System);
        }
    }
}