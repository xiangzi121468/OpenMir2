using MemoryPack;
using OpenMir2.Data;
using System.Collections.Generic;

namespace OpenMir2.Packets.ServerPackets
{
    [MemoryPackable]
    public partial struct MarketDataMessage
    {
        public IList<MarketItem> List { get; set; }
        public int TotalCount { get; set; }
    }

    [MemoryPackable]
    public partial struct MarkerUserLoadMessage
    {
        public int SellCount { get; set; }
        public int MarketNPC { get; set; }
        public byte IsBusy { get; set; }
    }

    [MemoryPackable]
    public partial class MarketSaveDataItem
    {
        [MemoryPackAllowSerialize]
        public MarketItem Item { get; set; }
        public byte ServerIndex { get; set; }
        public string ServerName { get; set; }
        public byte GroupId { get; set; }
    }

    [MemoryPackable]
    public partial struct MarketRegisterMessage
    {
        public byte ServerIndex { get; set; }
        public string ServerName { get; set; }
        public byte GroupId { get; set; }
        public string Token { get; set; }
    }

    [MemoryPackable]
    public partial struct MarketSearchMessage
    {
        public byte ServerIndex { get; set; }
        public byte GroupId { get; set; }
        public string UserName { get; set; }
        public string MarketName { get; set; }
        public string SearchWho { get; set; }
        public string SearchItem { get; set; }
        public short ItemType { get; set; }
        public byte ItemSet { get; set; }
        public int UserMode { get; set; }
        public int MarketNPC { get; set; }
    }

    /// <summary>
    /// 删除拍卖行物品消息
    /// </summary>
    [MemoryPackable]
    public partial struct MarketDeleteMessage
    {
        public int Index { get; set; }
        public string MarketName { get; set; }
        public string UserName { get; set; }
        public byte Reason { get; set; } // 0=取消, 1=过期, 2=购买
    }

    /// <summary>
    /// 物品归还消息(用于拍卖失败/取消/过期后归还物品给玩家)
    /// </summary>
    [MemoryPackable]
    public partial struct MarketItemReturnMessage
    {
        public string UserName { get; set; }
        public int ItemIndex { get; set; }
        public string ItemName { get; set; }
        public byte ReturnType { get; set; } // 0=直接归还背包, 1=邮件发送
    }
}