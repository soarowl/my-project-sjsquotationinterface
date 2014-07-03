using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.StockV5
{
    public struct QuotationInfo
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public string exchId;

        /// <summary>
        /// 证券代码
        /// </summary>
        public string stkId;

        /// <summary>
        /// 证券简称前缀
        /// </summary>
        public string stkIdPrefix;

        /// <summary>
        /// 证券名称
        /// </summary>
        public string stkName;

        /// <summary>
        /// 行情标志
        /// </summary>
        public string levelFlag;

        /// <summary>
        /// 停牌标志
        /// </summary>
        public char closeFlag;

        /// <summary>
        /// 昨收价
        /// </summary>
        public decimal closePrice;

        /// <summary>
        /// 今开价
        /// </summary>
        public decimal openPrice;

        /// <summary>
        /// 当前价
        /// </summary>
        public decimal newPrice;

        /// <summary>
        /// 最高价
        /// </summary>
        public decimal highPrice;

        /// <summary>
        /// 最低价
        /// </summary>
        public decimal lowPrice;

        /// <summary>
        /// 成交量
        /// </summary>
        public Int64 knockQty;

        /// <summary>
        /// 成交金额
        /// </summary>
        public decimal knockMoney;

        public decimal buyPrice1;
        public decimal buyPrice2;
        public decimal buyPrice3;
        public decimal buyPrice4;
        public decimal buyPrice5;

        public decimal sellPrice1;
        public decimal sellPrice2;
        public decimal sellPrice3;
        public decimal sellPrice4;
        public decimal sellPrice5;

        public Int64 buyQty1;
        public Int64 buyQty2;
        public Int64 buyQty3;
        public Int64 buyQty4;
        public Int64 buyQty5;

        public Int64 sellQty1;
        public Int64 sellQty2;
        public Int64 sellQty3;
        public Int64 sellQty4;
        public Int64 sellQty5;

        /// <summary>
        /// 基金净值
        /// </summary>
        public decimal IOPV;

        /// <summary>
        /// 涨停价格
        /// </summary>
        public decimal maxOrderPrice;

        /// <summary>
        /// 跌停价格
        /// </summary>
        public decimal minOrderPrice;

        /// <summary>
        /// 产品交易阶段
        /// </summary>
        public string tradeTimeFlag;

        /// <summary>
        /// 暂停交易标志
        /// </summary>
        public bool pauseTradeStatus;

        /// <summary>
        /// 融资交易状态
        /// </summary>
        public char passCreditCashStk;

        /// <summary>
        /// 融券交易状态
        /// </summary>
        public char passCreditShareStk;

        /// <summary>
        /// 其它业务状态
        /// </summary>
        public string otherBusinessMark;

        /// <summary>
        /// 价格变动
        /// </summary>
        public decimal change;

        /// <summary>
        /// 价格变动率
        /// </summary>
        public decimal changePercent;

        /// <summary>
        /// 昨收指数
        /// </summary>
        public decimal closeIndex;

        /// <summary>
        /// 今开指数
        /// </summary>
        public decimal openIndex;

        /// <summary>
        /// 最高指数
        /// </summary>
        public decimal highIndex;

        /// <summary>
        /// 最低指数
        /// </summary>
        public decimal lowIndex;

        /// <summary>
        /// 最新指数
        /// </summary>
        public decimal newIndex;

        /// <summary>
        /// 行情时间（YYYYMMDDHHMMSSsss）
        /// </summary>
        public Int64 changeTime;
        
        /// <summary>
        /// MDS 收到行情的时间
        /// </summary>
        public Int64 ReceiveTime;

        /// <summary>
        /// MDS将行情推到MQ的时间
        /// </summary>
        public Int64 SendTime;
    
    }

}
