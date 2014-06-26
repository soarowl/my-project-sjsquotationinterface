using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 证券信息基类
    /// </summary>
    public abstract class SecurityInfoBase
    {
        public CommonSecurityInfo CommonInfo { get; set; }
    }
    
    /// <summary>
    /// 证券信息
    /// </summary>
    /// <typeparam name="TParams"></typeparam>
    public abstract class SecurityInfoBase<TParams> : SecurityInfoBase
        where TParams : SecurityParamsBase
    {
        /// <summary>
        /// 特有字段
        /// </summary>
        public TParams Params { get; set; }
    }

    /// <summary>
    /// 股票信息
    /// </summary>
    public class StockSecurityInfo : SecurityInfoBase<StockParams> { }

    /// <summary>
    /// 基金信息
    /// </summary>
    public class FundSecurityInfo : SecurityInfoBase<FundParams> { }

    /// <summary>
    /// 债券信息
    /// </summary>
    public class BondSecurityInfo : SecurityInfoBase<BondParams> { }
    /// <summary>
    /// 权证信息
    /// </summary>
    public class WarrantSecurityInfo : SecurityInfoBase<WarrantParams> { }
    /// <summary>
    /// 回购信息
    /// </summary>
    public class RepoSecurityInfo : SecurityInfoBase<RepoParams> { }
    /// <summary>
    /// 期权信息
    /// </summary>
    public class OptionSecuirtyInfo : SecurityInfoBase<OptionParams> { }

    /// <summary>
    /// 其它证券信息
    /// </summary>
    public class OtherSecurityInfo : SecurityInfoBase
    { 
    
    }
}
