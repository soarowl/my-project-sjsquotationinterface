using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.Binary
{
    public enum MsgType
    {
        /// <summary>
        /// 登录
        /// </summary>
        Logon=1,
        /// <summary>
        /// 注销
        /// </summary>
        Logout=2,
        /// <summary>
        /// 心跳
        /// </summary>
        Heartbeat=3,
        /// <summary>
        /// 频道心跳
        /// </summary>
        ChannelHeartbeat=390095,
        /// <summary>
        /// 证券实时状态
        /// </summary>
        RealtimeStatus=390013,
        /// <summary>
        /// 公告
        /// </summary>
        Bullet=390012,
        /// <summary>
        /// 集中竞价业务行情快照
        /// </summary>
        QuotationSnap=300111,
        /// <summary>
        /// 盘后定价大宗交易业务行情快照
        /// </summary>
        BlockTradingQuotationSnap = 300611,
        /// <summary>
        /// 指数行情
        /// </summary>
        IndexQuotationSnap = 309011,
        /// <summary>
        /// 成交量统计指标
        /// </summary>
        TradingVolumeStatisticalIndicator=309111,
        /// <summary>
        /// 逐笔委托
        /// </summary>
        Order=300192,
        /// <summary>
        /// 协议交易业务逐笔委托行情
        /// </summary>
        AgreementOrder=300592,
        /// <summary>
        /// 转融通证券出借业务逐笔委托行情
        /// </summary>
       RefinanceOrder=300792, 
        /// <summary>
        /// 逐笔成交
        /// </summary>
        Tick=300191

    }
}
