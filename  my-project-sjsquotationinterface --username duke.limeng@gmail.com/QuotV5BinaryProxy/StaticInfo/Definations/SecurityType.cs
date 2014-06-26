using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    public enum SecurityType
    {
        主板A股 = 1,
        中小板股票 = 2,
        创业板股票 = 3,
        主板B股 = 4,
        国债 = 5,
        企业债 = 6,
        公司债 = 7,
        可转债 = 8,
        中小企业私募债 = 9,
        中小企业可交换私募债 = 10,
        证券公司次级债 = 11,
        质押式回购 = 12,
        资产支持证券 = 13,
        本市场股票ETF = 14,
        跨市场股票ETF = 15,
        跨境ETF = 16,
        本市场事务债券ETF = 17,
        现金债券ETF = 18,
        黄金ETF = 19,
        货币ETF = 20,
        标准LOF = 23,
        分级子基金 = 24,
        封闭式基金 = 25,
        仅申赎基金 = 26,
        仅实时申赎货币基金 = 27,
        权证 = 28,
        股票期权 = 29,
        ETF期权 = 30,
        资管产品 = 31,
        报价回购 = 32
    }
}
