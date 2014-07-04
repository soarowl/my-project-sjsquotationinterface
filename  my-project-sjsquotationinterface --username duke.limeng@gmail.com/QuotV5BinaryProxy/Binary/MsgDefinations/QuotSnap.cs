using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace QuotV5.Binary
{

    /// <summary>
    /// 快照行情通用信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct QuotSnapCommonInfo
    {
        /// <summary>
        /// 数据生成时间
        /// </summary>
        public Int64 OrigTime;

        /// <summary>
        /// 频道代码
        /// </summary>
        public UInt16 ChannelNo;

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
        /// 产品所处的交易阶段代码
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string TradingPhaseCode;

        /// <summary>
        /// 昨收价
        /// </summary>
        /// <remarks>
        /// Price,
        /// </remarks>
        public Int64 PrevClosePx;

        /// <summary>
        /// 成交笔数
        /// </summary>
        public Int64 NumTrades;

        /// <summary>
        ///成交总量
        /// </summary>
        /// <remarks>
        /// Qty
        /// </remarks>
        public Int64 TotalVolumeTrade;

        /// <summary>
        /// 成交总金额
        /// </summary>
        /// <remarks>
        /// Amt
        /// </remarks>
        public Int64 TotalValueTrade;

    }

    #region 行情快照扩展信息定义
    /// <summary>
    /// 行情快照扩展信息基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QuotSnapExtInfoBase<T>
        where T : QuotSnapExtInfoBase<T>, new()
    {

        private static T instance = null;
        /// <summary>
        /// 将byte数组转换为QuotSnapExtInfoBase对象
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected abstract T deserialize(byte[] bytes);
        public static T Deserialize(byte[] bytes)
        {
            EnsureInstanceCreated();
            return instance.deserialize(bytes);
        }
        protected static void EnsureInstanceCreated()
        {
            if (instance == null)
            {
                ObjectCreator<T> objCreator = ObjectCreator<T>.Instance;
                instance = objCreator.Create(Type.EmptyTypes);
            }
        }
    }

    /// <summary>
    /// 集中竞价业务行情快照扩展信息
    /// </summary>
    public class QuotSnapExtInfo300111 : QuotSnapExtInfoBase<QuotSnapExtInfo300111>
    {
        private static int mdEntrySize = Marshal.SizeOf(typeof(MDEntryWithoutOrders));
        private static int orderSize = Marshal.SizeOf(typeof(Order));

        #region 结构定义
        public UInt32 NoMDEntries { get; set; }

        public MDEntry[] MDEntries { get; set; }


        public class MDEntry
        {
            public MDEntryWithoutOrders Entry { get; set; }
            /// <summary>
            /// 委托
            /// </summary>
            public Order[] Orders { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MDEntryWithoutOrders
        {
            /// <summary>
            /// 行情条目类别
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
            public string MDEntryType;

            /// <summary>
            /// 价格
            /// </summary>
            public Int64 MDEntryPx;

            /// <summary>
            /// 数量
            /// </summary>
            public Int64 MDEntrySize;

            /// <summary>
            /// 买卖盘档位
            /// </summary>
            public UInt16 MDPriceLevel;

            /// <summary>
            /// 价位总委托笔数
            /// 为0表示不揭示
            /// </summary>
            public UInt64 NumberOfOrders;

            /// <summary>
            /// 价位揭示委托笔数
            /// 为0表示不揭示
            /// </summary>
            public UInt32 NoOrders;

        }



        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Order
        {
            /// <summary>
            /// 委托数量
            /// </summary>
            public Int64 OrderQty;
        }

        public class MDEntryType
        {
            public const string BuyPrice = "0";
            public const string SellPrice = "1";
            public const string KnockPrice = "2";
            public const string OpenPrice = "4";
            public const string SettlePrice = "6";
            public const string HighPrice = "7";
            public const string LowPrice = "8";
            public const string Diff1 = "x1";
            public const string Diff2 = "x2";
            public const string BuySummary = "x3";
            public const string SellSummary = "x4";
            public const string PriceEarningRatio1 = "x5";
            public const string PriceEarningRatio2 = "x6";
            public const string X7 = "x7";
            public const string IOPV = "x8";
            public const string X9 = "x9";
            public const string MaxOrderPrice = "xe";
            public const string MinOrderPrice = "xf";
        }
        #endregion

        protected override QuotSnapExtInfo300111 deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return null;
            QuotSnapExtInfo300111 rtn = new QuotSnapExtInfo300111();

            Array.Reverse(bytes, 0, 4);
            rtn.NoMDEntries = BitConverter.ToUInt32(bytes, 0);
            if (rtn.NoMDEntries > 0)
            {
                rtn.MDEntries = new MDEntry[rtn.NoMDEntries];
                int startIndex = 4;

                for (int entryIndex = 0; entryIndex < rtn.NoMDEntries; entryIndex++)
                {
                    if (bytes.Length - startIndex >= mdEntrySize)
                    {
                        var mdEntryBytes = bytes.Skip(startIndex).Take(mdEntrySize).ToArray();
                        MDEntry mdEntry = new MDEntry();
                        mdEntry.Entry = BigEndianStructHelper<MDEntryWithoutOrders>.BytesToStruct(mdEntryBytes, MsgConsts.MsgEncoding);
                        startIndex += mdEntrySize;
                        if (mdEntry.Entry.NoOrders > 0)
                        {
                            if (bytes.Length - startIndex >= orderSize * mdEntry.Entry.NoOrders)
                            {
                                mdEntry.Orders = new Order[mdEntry.Entry.NoOrders];
                                for (int orderIndex = 0; orderIndex < mdEntry.Entry.NoOrders; orderIndex++)
                                {
                                    var orderBytes = bytes.Skip(startIndex).Take(orderSize).ToArray();
                                    Order order = BigEndianStructHelper<Order>.BytesToStruct(orderBytes, MsgConsts.MsgEncoding);
                                    mdEntry.Orders[orderIndex] = order;
                                    startIndex += orderSize;
                                }
                            }
                            else
                                return null;
                        }

                        rtn.MDEntries[entryIndex] = mdEntry;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return rtn;
        }
    }


    /// <summary>
    /// 指数行情快照扩展信息
    /// </summary>
    public class QuotSnapExtInfo309011 : QuotSnapExtInfoBase<QuotSnapExtInfo309011>
    {
        private static int mdEntrySize = Marshal.SizeOf(typeof(MDEntry));


        #region 结构定义
        public UInt32 NoMDEntries { get; private set; }

        public MDEntry[] MDEntries { get; private set; }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MDEntry
        {
            /// <summary>
            /// 行情条目类别
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
            public string MDEntryType;

            /// <summary>
            /// 价格
            /// </summary>
            public Int64 MDEntryPx;
        }
        public class MDEntryType
        {
            public const string NewIndex = "3";
            public const string CloseIndex = "xa";
            public const string OpenIndex = "xb";
            public const string HighIndex = "xc";
            public const string LowIndex = "xd";
        }
        #endregion

        protected override QuotSnapExtInfo309011 deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return null;
            QuotSnapExtInfo309011 rtn = new QuotSnapExtInfo309011();

            Array.Reverse(bytes, 0, 4);
            rtn.NoMDEntries = BitConverter.ToUInt32(bytes, 0);
            if (rtn.NoMDEntries > 0)
            {
                rtn.MDEntries = new MDEntry[rtn.NoMDEntries];
                int startIndex = 4;

                for (int entryIndex = 0; entryIndex < rtn.NoMDEntries; entryIndex++)
                {
                    if (bytes.Length - startIndex >= mdEntrySize)
                    {
                        var mdEntryBytes = bytes.Skip(startIndex).Take(mdEntrySize).ToArray();
                        MDEntry mdEntry = BigEndianStructHelper<MDEntry>.BytesToStruct(mdEntryBytes, MsgConsts.MsgEncoding);
                        startIndex += mdEntrySize;
                        rtn.MDEntries[entryIndex] = mdEntry;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return rtn;
        }

    }

    /// <summary>
    /// 盘后定价大宗交易业务行情快照扩展信息
    /// </summary>
    public class QuotSnapExtInfo300611 : QuotSnapExtInfoBase<QuotSnapExtInfo300611>
    {
        private static int mdEntrySize = Marshal.SizeOf(typeof(MDEntry));


        #region 结构定义
        public UInt32 NoMDEntries { get; private set; }

        public MDEntry[] MDEntries { get; private set; }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MDEntry
        {
            /// <summary>
            /// 行情条目类别
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)]
            public string MDEntryType;

            /// <summary>
            /// 价格
            /// </summary>
            public Int64 MDEntryPx;

            /// <summary>
            /// 数量
            /// </summary>
            public Int64 MDEntrySize;
        }
        public class MDEntryType
        {
            public const string Buy = "0";
            public const string Sell = "1";

        }
        #endregion

        protected override QuotSnapExtInfo300611 deserialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return null;
            QuotSnapExtInfo300611 rtn = new QuotSnapExtInfo300611();

            Array.Reverse(bytes, 0, 4);
            rtn.NoMDEntries = BitConverter.ToUInt32(bytes, 0);
            if (rtn.NoMDEntries > 0)
            {
                rtn.MDEntries = new MDEntry[rtn.NoMDEntries];
                int startIndex = 4;

                for (int entryIndex = 0; entryIndex < rtn.NoMDEntries; entryIndex++)
                {
                    if (bytes.Length - startIndex >= mdEntrySize)
                    {
                        var mdEntryBytes = bytes.Skip(startIndex).Take(mdEntrySize).ToArray();
                        MDEntry mdEntry = BigEndianStructHelper<MDEntry>.BytesToStruct(mdEntryBytes, MsgConsts.MsgEncoding);
                        startIndex += mdEntrySize;
                        rtn.MDEntries[entryIndex] = mdEntry;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return rtn;
        }

    }


    #endregion

    #region 行情快照信息定义
    /// <summary>
    /// 行情快照信息基类
    /// </summary>
    /// <typeparam name="TExtInfo"></typeparam>
    /// <typeparam name="TQuotSnap"></typeparam>
    public abstract class QuotSnapBase<TExtInfo, TQuotSnap> : IMarketData
        where TExtInfo : QuotSnapExtInfoBase<TExtInfo>, new()
        where TQuotSnap : QuotSnapBase<TExtInfo, TQuotSnap>, new()
    {
        protected static int commonInfoSize = Marshal.SizeOf(typeof(QuotSnapCommonInfo));

        public QuotSnapCommonInfo CommonInfo { get; set; }

        public TExtInfo ExtInfo { get; set; }


        public static TQuotSnap Deserialize(byte[] bytes)
        {

            if (bytes == null || bytes.Length < commonInfoSize)
                return null;
            else
            {
                //for (int i = 0; i < bytes.Length; i++)
                //{
                //    Debug .WriteLine("bytes[{0}] =\t{1}",i,bytes[i]);
                //}

                ObjectCreator<TQuotSnap> objCreator = ObjectCreator<TQuotSnap>.Instance;
                var commonInfoBytes = bytes.Take(commonInfoSize).ToArray();
                TQuotSnap rtn = objCreator.Create(Type.EmptyTypes);
                rtn.CommonInfo = BigEndianStructHelper<QuotSnapCommonInfo>.BytesToStruct(commonInfoBytes, MsgConsts.MsgEncoding);
                var extInfoBytes = bytes.Skip(commonInfoSize).ToArray();
                rtn.ExtInfo = QuotSnapExtInfoBase<TExtInfo>.Deserialize(extInfoBytes);
                return rtn;
            }
        }
    }


    /// <summary>
    /// 集中竞价业务行情快照
    /// </summary>
    public class QuotSnap300111 : QuotSnapBase<QuotSnapExtInfo300111, QuotSnap300111>
    {

    }

    /// <summary>
    /// 指数行情快照
    /// </summary>
    public class QuotSnap309011 : QuotSnapBase<QuotSnapExtInfo309011, QuotSnap309011>
    {

    }

    /// <summary>
    /// 盘后定价大宗交易业务行情快照
    /// </summary>
    public class QuotSnap300611 : QuotSnapBase<QuotSnapExtInfo300611, QuotSnap300611>
    {

    }

    #endregion
}
