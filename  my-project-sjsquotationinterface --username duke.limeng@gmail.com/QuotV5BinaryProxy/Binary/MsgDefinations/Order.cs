using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace QuotV5.Binary
{
    /// <summary>
    /// 逐笔委托行情通用信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OrderCommonInfo
    {
        /// <summary>
        /// 频道代码
        /// </summary>
        public UInt16 ChannelNo;

        /// <summary>
        /// 消息记录号
        /// </summary>
        public UInt64 AppSeqNum;

        /// <summary>
        /// 行情类别
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
        public string MDStreamID;

        /// <summary>
        /// 证券代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string SecurityID;

        /// <summary>
        /// 证券代码源
        /// 102=深圳证券交易所
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string SecurityIDSource;

        /// <summary>
        /// 委托价格
        /// </summary>
        public UInt64 Price;

        /// <summary>
        /// 委托数量
        /// </summary>
        public UInt64 OrderQty;

        /// <summary>
        /// 买卖方向
        /// </summary>
        public char Side;

        /// <summary>
        /// 委托时间
        /// </summary>
        public UInt64 TranscTime;


    }


    #region 逐笔委托行情快照扩展信息定义
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OrderExtInfo300192
    {
        /// <summary>
        /// 订单类别
        /// </summary>
        /// <remarks>
        /// 1 市价
        /// 2 限价
        /// U 本方最优
        /// </remarks>
        public char OrderType;

        /// <summary>
        /// 0 当日有效
        /// 3 即时成交或撤销
        /// </summary>
        public char TimeInForce;

        /// <summary>
        /// 最多成交价位数
        /// </summary>
        public UInt16 MaxPriceLevels;

        /// <summary>
        /// 最低成交数量
        /// </summary>
        public Int64 MinQty;
    }

    #endregion

    #region 逐笔委托行情定义

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Order300192:IMarketData
    {
        public OrderCommonInfo CommonInfo;
        public OrderExtInfo300192 ExtInfo;

        public static Order300192 Deserialize(byte[] bytes)
        {
            return BigEndianStructHelper<Order300192>.BytesToStruct(bytes, MsgConsts.MsgEncoding);
        }
    }



    #endregion
}
