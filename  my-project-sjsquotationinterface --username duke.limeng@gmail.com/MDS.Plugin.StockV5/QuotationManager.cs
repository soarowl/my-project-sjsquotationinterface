using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace MDS.Plugin.SZQuotV5
{
    public class QuotationManager
    {
        private Status currentStatus = Status.Stopped;
        private Log4cb.ILog4cbHelper logHelper;
        private QuotV5.Binary.RealTimeQuotConnection quotConn;
        private QuotationRepository quotRepository;
        private QuotationMQPublisher quotPublisher;

        private SecurityInfoProvider securityInfoProvider;
        private IndexInfoProvider indexInfoProvider;
        private CashAuctionParamsProvider cashAuctionParamsProvider;
        private DerivativeAuctionParamsProvider derivativeAuctionParamsProvider;
        private NegotiationParamsProvider negotiationParamsProvider;
        private SecurityCloseMDProvider securityCloseMDProvider;

        private object syncUpdateStaticInfoSnap = new object();
        private object syncRunningStatus = new object();
        public QuotationManager(
            QuotV5.Binary.RealTimeQuotConnection quotConn,
            QuotationRepository quotRepository,
            QuotationMQPublisher quotPublisher,
            SecurityInfoProvider securityInfoProvider,
            IndexInfoProvider indexInfoProvider,
            CashAuctionParamsProvider cashAuctionParamsProvider,
           DerivativeAuctionParamsProvider derivativeAuctionParamsProvider,
            NegotiationParamsProvider negotiationParamsProvider,
            SecurityCloseMDProvider securityCloseMDProvider,
            Log4cb.ILog4cbHelper logHelper
            )
        {
            this.quotConn = quotConn;
            this.quotRepository = quotRepository;
            this.quotPublisher = quotPublisher;
            this.securityInfoProvider = securityInfoProvider;
            this.indexInfoProvider = indexInfoProvider;
            this.cashAuctionParamsProvider = cashAuctionParamsProvider;
            this.derivativeAuctionParamsProvider = derivativeAuctionParamsProvider;
            this.negotiationParamsProvider = negotiationParamsProvider;
            this.securityCloseMDProvider = securityCloseMDProvider;
            this.logHelper = logHelper;

            this.quotConn.OnMarketDataReceived += new Action<QuotV5.Binary.IMarketData>(QuotConn_OnMarketDataReceived);
            this.securityInfoProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.SecurityInfoBase>>(SecurityInfoProvider_OnStaticInfoRead);
            this.indexInfoProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.IndexInfo>>(IndexInfoProvider_OnStaticInfoRead);
            this.cashAuctionParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.CashAuctionParams>>(CashAuctionParamsProvider_OnStaticInfoRead);
            //this.derivativeAuctionParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.DerivativeAuctionParams>>(DerivativeAuctionParamsProvider_OnStaticInfoRead);
            this.negotiationParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.NegotiationParams>>(NegotiationParamsProvider_OnStaticInfoRead);
            //this.securityCloseMDProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.SecurityCloseMD>>(SecurityCloseMDProvider_OnStaticInfoRead);
        }

        public void Start()
        {
            lock (syncRunningStatus)
            {
                if (this.currentStatus == Status.Started)
                    return;
                this.currentStatus = Status.Startting;
                RawQuotationInfoSnap.ClearAll();
                RawStaticInfoSnap.ClearAll();
                TryLoadStoredDataSnap();

                this.quotConn.Start();
                this.securityInfoProvider.Start();
                this.indexInfoProvider.Start();
                this.cashAuctionParamsProvider.Start();
                this.derivativeAuctionParamsProvider.Start();
                this.negotiationParamsProvider.Start();
                this.securityCloseMDProvider.Start();
                this.currentStatus = Status.Started;
            }
        }

        public void Stop()
        {
            lock (syncRunningStatus)
            {
                if (this.currentStatus == Status.Stopped)
                    return;
                this.currentStatus = Status.Stopping;
                this.quotConn.Stop();
                this.securityInfoProvider.Stop();
                this.indexInfoProvider.Stop();
                this.cashAuctionParamsProvider.Stop();
                this.derivativeAuctionParamsProvider.Stop();
                this.negotiationParamsProvider.Stop();
                this.securityCloseMDProvider.Stop();
                this.currentStatus = Status.Stopped;
            }
        }


        private void TryLoadStoredDataSnap()
        {
            try
            {
                var allStockInfos = this.quotRepository.GetAllStockInfo();
                ProcessedDataSnap.StockInfo.Clear();

                if (allStockInfos != null)
                {
                    foreach (var bi in allStockInfos)
                    {
                        if (!string.IsNullOrEmpty(bi.stkId))
                            ProcessedDataSnap.StockInfo[bi.stkId] = bi;
                        else
                        {
                            this.logHelper.LogErrMsg("bi.stkId==null");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "从Redis中读取StockInfo快照异常");
            }
            try
            {

                var allQuotInfos = this.quotRepository.GetAllQuotationInfo();
                ProcessedDataSnap.QuotationInfo.Clear();
                if (allQuotInfos != null)
                {
                    foreach (var qi in allQuotInfos)
                    {
                        ProcessedDataSnap.QuotationInfo[qi.stkId] = qi;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "从Redis中读取QuotationInfo快照异常");
            }
        }


        #region 静态信息更新
        private void SecurityInfoProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.SecurityInfoBase> securityInfos)
        {
            if (securityInfos == null)
                return;
            lock (syncUpdateStaticInfoSnap)
            {
                foreach (var securityInfo in securityInfos)
                {
                    ProcessStaticInfo(securityInfo);
                }
            }
        }
        private void ProcessStaticInfo(QuotV5.StaticInfo.SecurityInfoBase securityInfo)
        {
            RawStaticInfoSnap.SecurityInfo[securityInfo.CommonInfo.SecurityID] = securityInfo;

            StockInfo preStockInfo = null;
            StockInfo newStockInfo = null;

            if (ProcessedDataSnap.StockInfo.TryGetValue(securityInfo.CommonInfo.SecurityID, out preStockInfo))
            {
                newStockInfo = preStockInfo.Clone();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }
            else
            {
                newStockInfo = new StockInfo();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }

            if (newStockInfo != null)
            {
                ProcessedDataSnap.StockInfo[newStockInfo.stkId] = newStockInfo;
                quotRepository.UpdateBasicInfo(newStockInfo);

            }
        }

        private void IndexInfoProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.IndexInfo> securityInfos)
        {
            if (securityInfos == null)
                return;
            lock (syncUpdateStaticInfoSnap)
            {
                foreach (var securityInfo in securityInfos)
                {
                    ProcessStaticInfo(securityInfo);
                }
            }
        }
        private void ProcessStaticInfo(QuotV5.StaticInfo.IndexInfo securityInfo)
        {
            RawStaticInfoSnap.IndexInfo[securityInfo.SecurityID] = securityInfo;
            StockInfo preStockInfo = null;
            StockInfo newStockInfo = null;

            if (ProcessedDataSnap.StockInfo.TryGetValue(securityInfo.SecurityID, out preStockInfo))
            {
                newStockInfo = preStockInfo.Clone();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }
            else
            {
                newStockInfo = new StockInfo();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }

            if (newStockInfo != null)
            {
                ProcessedDataSnap.StockInfo[newStockInfo.stkId] = newStockInfo;
                quotRepository.UpdateBasicInfo(newStockInfo);

            }
        }

        private void CashAuctionParamsProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.CashAuctionParams> securityInfos)
        {
            if (securityInfos == null)
                return;
            lock (syncUpdateStaticInfoSnap)
            {
                foreach (var securityInfo in securityInfos)
                {
                    ProcessStaticInfo(securityInfo);
                }
            }
        }
        private void ProcessStaticInfo(QuotV5.StaticInfo.CashAuctionParams securityInfo)
        {
            RawStaticInfoSnap.CashAuctionParams[securityInfo.SecurityID] = securityInfo;
            StockInfo preStockInfo = null;
            StockInfo newStockInfo = null;

            if (ProcessedDataSnap.StockInfo.TryGetValue(securityInfo.SecurityID, out preStockInfo))
            {
                newStockInfo = preStockInfo.Clone();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }
            else
            {
                newStockInfo = new StockInfo();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }

            if (newStockInfo != null)
            {
                ProcessedDataSnap.StockInfo[newStockInfo.stkId] = newStockInfo;
                quotRepository.UpdateBasicInfo(newStockInfo);

            }
        }

        private void DerivativeAuctionParamsProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.DerivativeAuctionParams> securityInfos)
        {
            if (securityInfos == null)
                return;
            lock (syncUpdateStaticInfoSnap)
            {
                foreach (var securityInfo in securityInfos)
                {
                    ProcessStaticInfo(securityInfo);
                }
            }
        }
        private void ProcessStaticInfo(QuotV5.StaticInfo.DerivativeAuctionParams securityInfo)
        {
            RawStaticInfoSnap.DerivativeAuctionParams[securityInfo.SecurityID] = securityInfo;
            StockInfo preStockInfo = null;
            StockInfo newStockInfo = null;

            if (ProcessedDataSnap.StockInfo.TryGetValue(securityInfo.SecurityID, out preStockInfo))
            {
                newStockInfo = preStockInfo.Clone();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }
            else
            {
                newStockInfo = new StockInfo();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }

            if (newStockInfo != null)
            {
                ProcessedDataSnap.StockInfo[newStockInfo.stkId] = newStockInfo;
                quotRepository.UpdateBasicInfo(newStockInfo);

            }
        }


        private void NegotiationParamsProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.NegotiationParams> securityInfos)
        {
            if (securityInfos == null)
                return;
            lock (syncUpdateStaticInfoSnap)
            {
                foreach (var securityInfo in securityInfos)
                {
                    ProcessStaticInfo(securityInfo);
                }
            }
        }
        private void ProcessStaticInfo(QuotV5.StaticInfo.NegotiationParams securityInfo)
        {
            RawStaticInfoSnap.NegotiationParams[securityInfo.SecurityID] = securityInfo;
            StockInfo preStockInfo = null;
            StockInfo newStockInfo = null;

            if (ProcessedDataSnap.StockInfo.TryGetValue(securityInfo.SecurityID, out preStockInfo))
            {
                newStockInfo = preStockInfo.Clone();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }
            else
            {
                newStockInfo = new StockInfo();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }

            if (newStockInfo != null)
            {
                ProcessedDataSnap.StockInfo[newStockInfo.stkId] = newStockInfo;
                quotRepository.UpdateBasicInfo(newStockInfo);

            }
        }

        private void SecurityCloseMDProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.SecurityCloseMD> securityInfos)
        {
            if (securityInfos == null)
                return;
            lock (syncUpdateStaticInfoSnap)
            {
                foreach (var securityInfo in securityInfos)
                {
                    ProcessStaticInfo(securityInfo);
                }
            }
        }
        private void ProcessStaticInfo(QuotV5.StaticInfo.SecurityCloseMD securityInfo)
        {
            RawStaticInfoSnap.SecurityCloseMD[securityInfo.SecurityID] = securityInfo;
            StockInfo preStockInfo = null;
            StockInfo newStockInfo = null;

            if (ProcessedDataSnap.StockInfo.TryGetValue(securityInfo.SecurityID, out preStockInfo))
            {
                newStockInfo = preStockInfo.Clone();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }
            else
            {
                newStockInfo = new StockInfo();
                SetStockInfoProperty(newStockInfo, securityInfo);
            }

            if (newStockInfo != null)
            {
                ProcessedDataSnap.StockInfo[newStockInfo.stkId] = newStockInfo;
                quotRepository.UpdateBasicInfo(newStockInfo);

            }
        }
        #endregion

        #region 为StockInfo属性赋值

        private void SetStockInfoProperty(StockInfo stockInfo, QuotV5.StaticInfo.SecurityInfoBase securityInfo)
        {
            stockInfo.stkId = securityInfo.CommonInfo.SecurityID;
            stockInfo.stkName = securityInfo.CommonInfo.Symbol;
            stockInfo.stkEnglishtAbbr = securityInfo.CommonInfo.EnglishName;
            stockInfo.stkParValue = securityInfo.CommonInfo.ParValue;
            stockInfo.totalCurrentStkQty = (Int64)securityInfo.CommonInfo.PublicFloatShareQuantity;
            stockInfo.listedDate = securityInfo.CommonInfo.ListDate;
            stockInfo.standardConvertRate = securityInfo.CommonInfo.ContractMultiplier;
            stockInfo.closePrice = securityInfo.CommonInfo.PrevClosePx;
            stockInfo.basicStkId = securityInfo.CommonInfo.UnderlyingSecurityID;
            stockInfo.ISINCode = securityInfo.CommonInfo.ISIN;
            stockInfo.CMOStandardRate = securityInfo.CommonInfo.GageRatio;
            stockInfo.totalStkQty = (Int64)securityInfo.CommonInfo.OutstandingShare;

            stockInfo.isCreditCashStk = securityInfo.CommonInfo.CrdBuyUnderlying;
            stockInfo.isCreditShareStk = securityInfo.CommonInfo.CrdSellUnderlying;

            if (securityInfo is QuotV5.StaticInfo.StockSecurityInfo)
            {
                QuotV5.StaticInfo.StockParams para = (securityInfo as QuotV5.StaticInfo.StockSecurityInfo).Params;
                stockInfo.stkIndustryType = para.IndustryClassification;
                stockInfo.lastYearProfit = para.PreviousYearProfitPerShare;
                stockInfo.thisYearProfit = para.CurrentYearProfitPerShare;
            }
            else if (securityInfo is QuotV5.StaticInfo.BondSecurityInfo)
            {
                QuotV5.StaticInfo.BondParams para = (securityInfo as QuotV5.StaticInfo.BondSecurityInfo).Params;
                stockInfo.endingDate = para.MaturityDate;
                stockInfo.accuredInterest = para.Interest;
                stockInfo.beginInterestDate = para.InterestAccrualDate;
            }
            else if (securityInfo is QuotV5.StaticInfo.FundSecurityInfo)
            {
                QuotV5.StaticInfo.FundParams para = (securityInfo as QuotV5.StaticInfo.FundSecurityInfo).Params;
                stockInfo.NAV = para.NAV;
            }
        }

        private void SetStockInfoProperty(StockInfo stockInfo, QuotV5.StaticInfo.IndexInfo securityInfo)
        {

            stockInfo.stkId = securityInfo.SecurityID;
            stockInfo.stkName = securityInfo.Symbol;
            stockInfo.stkEnglishtAbbr = securityInfo.EnglishName;
            stockInfo.closePrice = securityInfo.PreCloseIdx;
        }

        private void SetStockInfoProperty(StockInfo stockInfo, QuotV5.StaticInfo.CashAuctionParams cashAuctionParams)
        {
            stockInfo.stkId = cashAuctionParams.SecurityID;
            stockInfo.buyQtyUpperLimit = (int)cashAuctionParams.BuyQtyUpperLimit;
            stockInfo.sellQtyUpperLimit = (int)cashAuctionParams.SellQtyUpperLimit;
            stockInfo.orderPriceUnit = cashAuctionParams.PriceTick;
            stockInfo.marketMarkerFlag = cashAuctionParams.MarketMakerFlag;
        }

        private void SetStockInfoProperty(StockInfo stockInfo, QuotV5.StaticInfo.DerivativeAuctionParams derivativeAuctionParams)
        {
            stockInfo.stkId = derivativeAuctionParams.SecurityID;
            stockInfo.buyQtyUpperLimit = (int)derivativeAuctionParams.BuyQtyUpperLimit;
            stockInfo.sellQtyUpperLimit = (int)derivativeAuctionParams.SellQtyUpperLimit;
            stockInfo.orderPriceUnit = derivativeAuctionParams.PriceTick;
            stockInfo.marketMarkerFlag = derivativeAuctionParams.MarketMakerFlag;
        }

        private void SetStockInfoProperty(StockInfo stockInfo, QuotV5.StaticInfo.NegotiationParams negotiationParams)
        {
            stockInfo.stkId = negotiationParams.SecurityID;
        }

        private void SetStockInfoProperty(StockInfo stockInfo, QuotV5.StaticInfo.SecurityCloseMD securityCloseMD)
        {
            stockInfo.stkId = securityCloseMD.SecurityID;
            stockInfo.exchTotalKnockAmt = securityCloseMD.TotalValueTrade;
            stockInfo.exchTotalKnockQty = (Int64)securityCloseMD.TotalVolumeTrade;
        }
        #endregion

        #region 处理收到的动态行情

        /// <summary>
        /// 收到行情数据
        /// </summary>
        /// <param name="marketData"></param>
        private void QuotConn_OnMarketDataReceived(QuotV5.Binary.IMarketData marketData)
        {
            if (marketData is QuotV5.Binary.QuotSnap300111)
            {
                ProcessMarketData(marketData as QuotV5.Binary.QuotSnap300111);
            }
            else if (marketData is QuotV5.Binary.QuotSnap300611)
            {
                ProcessMarketData(marketData as QuotV5.Binary.QuotSnap300611);
            }
            else if (marketData is QuotV5.Binary.QuotSnap309011)
            {
                ProcessMarketData(marketData as QuotV5.Binary.QuotSnap309011);
            }
            else if (marketData is QuotV5.Binary.RealtimeStatus)
            {
                ProcessMarketData(marketData as QuotV5.Binary.RealtimeStatus);
            }
        }

        private void ProcessMarketData(QuotV5.Binary.QuotSnap300111 quotSnap)
        {
            RawQuotationInfoSnap.QuotSnap300111[quotSnap.CommonInfo.SecurityID] = quotSnap;
            QuotationInfo preQuotInfo = null;
            QuotationInfo newQuotInfo = null;
            if (ProcessedDataSnap.QuotationInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                SetQuotInfoProperty(newQuotInfo, quotSnap);
            }
            else
            {
                newQuotInfo = new QuotationInfo();

                QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                    SetQuotInfoProperty(newQuotInfo, securityInfo);

                QuotV5.Binary.RealtimeStatus status = null;
                if (RawQuotationInfoSnap.RealtimeStatus.TryGetValue(quotSnap.CommonInfo.SecurityID, out status))
                    SetQuotInfoProperty(newQuotInfo, status);

                SetQuotInfoProperty(newQuotInfo, quotSnap);
            }
            if (newQuotInfo != null)
            {
                ProcessedDataSnap.QuotationInfo[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateQuotInfo(newQuotInfo);
                if (quotPublisher != null)
                    quotPublisher.Enqueue(newQuotInfo, true);
            }
        }

        private void ProcessMarketData(QuotV5.Binary.QuotSnap300611 quotSnap)
        {
            RawQuotationInfoSnap.QuotSnap300611[quotSnap.CommonInfo.SecurityID] = quotSnap;
            QuotationInfo preQuotInfo = null;
            QuotationInfo newQuotInfo = null;
            if (ProcessedDataSnap.QuotationInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                SetQuotInfoProperty(newQuotInfo, quotSnap);
            }
            else
            {
                newQuotInfo = new QuotationInfo();

                QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                    SetQuotInfoProperty(newQuotInfo, securityInfo);

                QuotV5.Binary.RealtimeStatus status = null;
                if (RawQuotationInfoSnap.RealtimeStatus.TryGetValue(quotSnap.CommonInfo.SecurityID, out status))
                    SetQuotInfoProperty(newQuotInfo, status);

                SetQuotInfoProperty(newQuotInfo, quotSnap);



            }
            if (newQuotInfo != null)
            {
                ProcessedDataSnap.QuotationInfo[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateQuotInfo(newQuotInfo);
                if (quotPublisher != null)
                    quotPublisher.Enqueue(newQuotInfo, true);
            }
        }

        private void ProcessMarketData(QuotV5.Binary.QuotSnap309011 quotSnap)
        {
            RawQuotationInfoSnap.QuotSnap309011[quotSnap.CommonInfo.SecurityID] = quotSnap;
            QuotationInfo preQuotInfo = null;
            QuotationInfo newQuotInfo = null;
            if (ProcessedDataSnap.QuotationInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                SetQuotInfoProperty(newQuotInfo, quotSnap);
            }
            else
            {
                newQuotInfo = new QuotationInfo();

                QuotV5.StaticInfo.IndexInfo securityInfo = null;
                if (RawStaticInfoSnap.IndexInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                    SetQuotInfoProperty(newQuotInfo, securityInfo);

                QuotV5.Binary.RealtimeStatus status = null;
                if (RawQuotationInfoSnap.RealtimeStatus.TryGetValue(quotSnap.CommonInfo.SecurityID, out status))
                    SetQuotInfoProperty(newQuotInfo, status);

                SetQuotInfoProperty(newQuotInfo, quotSnap);
            }
            if (newQuotInfo != null)
            {
                ProcessedDataSnap.QuotationInfo[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateQuotInfo(newQuotInfo);
                if (quotPublisher != null)
                    quotPublisher.Enqueue(newQuotInfo, true);
            }
        }

        private void ProcessMarketData(QuotV5.Binary.RealtimeStatus status)
        {
            RawQuotationInfoSnap.RealtimeStatus[status.Status.SecurityID] = status;
            QuotationInfo preQuotInfo = null;
            QuotationInfo newQuotInfo = null;
            if (ProcessedDataSnap.QuotationInfo.TryGetValue(status.Status.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                SetQuotInfoProperty(newQuotInfo, status);
            }
            else
            {
                newQuotInfo = new QuotationInfo();

                QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                if (RawStaticInfoSnap.SecurityInfo.TryGetValue(status.Status.SecurityID, out securityInfo))
                    SetQuotInfoProperty(newQuotInfo, securityInfo);

                SetQuotInfoProperty(newQuotInfo, status);

                QuotV5.Binary.QuotSnap300111 quotSnap = null;
                if (RawQuotationInfoSnap.QuotSnap300111.TryGetValue(status.Status.SecurityID, out quotSnap))
                    SetQuotInfoProperty(newQuotInfo, quotSnap);

                QuotV5.Binary.QuotSnap300611 quotSnap2 = null;
                if (RawQuotationInfoSnap.QuotSnap300611.TryGetValue(status.Status.SecurityID, out quotSnap2))
                    SetQuotInfoProperty(newQuotInfo, quotSnap2);

            }
            if (newQuotInfo != null)
            {
                ProcessedDataSnap.QuotationInfo[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateQuotInfo(newQuotInfo);
                if (quotPublisher != null)
                    quotPublisher.Enqueue(newQuotInfo, true);
            }
        }
        #endregion

        #region 为QuotationInfo属性赋值
        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.StaticInfo.IndexInfo securityInfo)
        {
            quotInfo.stkId = securityInfo.SecurityID;
            quotInfo.stkName = securityInfo.Symbol;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.StaticInfo.SecurityInfoBase securityInfo)
        {
            quotInfo.stkId = securityInfo.CommonInfo.SecurityID;
            quotInfo.stkName = securityInfo.CommonInfo.Symbol;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.RealtimeStatus status)
        {
            quotInfo.stkId = status.Status.SecurityID;
            quotInfo.stkIdPrefix = status.Status.SecurityPreName;
            if (!string.IsNullOrEmpty(status.Status.SecurityPreName))
            {
                quotInfo.closeFlag = status.Status.SecurityPreName.Substring(0, 1);

                if (status.Status.SecurityPreName.Length > 1)
                {
                    quotInfo.passCreditCashStk = status.Status.SecurityPreName.Substring(1, 1);
                    quotInfo.passCreditShareStk = quotInfo.passCreditCashStk;
                }
            }
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.QuotSnap309011 quotSnap)
        {
            SetQuotInfoProperty(quotInfo, quotSnap.CommonInfo);
            if (quotSnap.ExtInfo != null && quotSnap.ExtInfo.MDEntries != null && quotSnap.ExtInfo.MDEntries.Length > 0)
            {
                decimal d = 1000000;
                foreach (var mdEntry in quotSnap.ExtInfo.MDEntries)
                {
                    if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.NewIndex)
                    {
                        quotInfo.newIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.OpenIndex)
                    {
                        quotInfo.openIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.CloseIndex)
                    {
                        quotInfo.closeIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.HighIndex)
                    {
                        quotInfo.highIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.LowIndex)
                    {
                        quotInfo.lowIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                }
            }
            quotInfo.change = quotInfo.newIndex - quotInfo.closeIndex;
            if (quotInfo.closeIndex != 0.0m)
                quotInfo.changePercent = quotInfo.change / quotInfo.closeIndex;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.QuotSnap300611 quotSnap)
        {
            SetQuotInfoProperty(quotInfo, quotSnap.CommonInfo);


            quotInfo.buyQty1 = 0;
            quotInfo.buyQty2 = 0;
            quotInfo.buyQty3 = 0;
            quotInfo.buyQty4 = 0;
            quotInfo.buyQty5 = 0;

            quotInfo.sellQty1 = 0;
            quotInfo.sellQty2 = 0;
            quotInfo.sellQty3 = 0;
            quotInfo.sellQty4 = 0;
            quotInfo.sellQty5 = 0;

            quotInfo.buyPrice1 = 0;
            quotInfo.buyPrice2 = 0;
            quotInfo.buyPrice3 = 0;
            quotInfo.buyPrice4 = 0;
            quotInfo.buyPrice5 = 0;

            quotInfo.sellPrice1 = 0;
            quotInfo.sellPrice2 = 0;
            quotInfo.sellPrice3 = 0;
            quotInfo.sellPrice4 = 0;
            quotInfo.sellPrice5 = 0;

            if (quotSnap.ExtInfo != null && quotSnap.ExtInfo.MDEntries != null && quotSnap.ExtInfo.MDEntries.Length > 0)
            {
                decimal d = 1000000;
                foreach (var mdEntry in quotSnap.ExtInfo.MDEntries)
                {
                    if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300611.MDEntryType.Buy)
                    {
                        quotInfo.buyPrice1 = (decimal)mdEntry.MDEntryPx / d;
                        quotInfo.buyQty1 = mdEntry.MDEntrySize;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300611.MDEntryType.Sell)
                    {
                        quotInfo.sellPrice1 = (decimal)mdEntry.MDEntryPx / d;
                        quotInfo.sellQty1 = mdEntry.MDEntrySize;
                    }
                }
            }
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.QuotSnap300111 quotSnap)
        {
            SetQuotInfoProperty(quotInfo, quotSnap.CommonInfo);
            if (quotSnap.ExtInfo != null && quotSnap.ExtInfo.MDEntries != null && quotSnap.ExtInfo.MDEntries.Length > 0)
            {
                decimal d = 1000000;
                foreach (var mdEntry in quotSnap.ExtInfo.MDEntries)
                {
                    if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.BuyPrice)
                    {
                        if (mdEntry.Entry.MDPriceLevel == 1)
                        {
                            quotInfo.buyPrice1 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyQty1 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 2)
                        {
                            quotInfo.buyPrice2 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyQty2 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 3)
                        {
                            quotInfo.buyPrice3 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyQty3 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 4)
                        {
                            quotInfo.buyPrice4 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyQty4 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 5)
                        {
                            quotInfo.buyPrice5 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyQty5 = mdEntry.Entry.MDEntrySize;
                        }

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SellPrice)
                    {
                        if (mdEntry.Entry.MDPriceLevel == 1)
                        {
                            quotInfo.sellPrice1 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellQty1 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 2)
                        {
                            quotInfo.sellPrice2 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellQty2 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 3)
                        {
                            quotInfo.sellPrice3 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellQty3 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 4)
                        {
                            quotInfo.sellPrice4 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellQty4 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 5)
                        {
                            quotInfo.sellPrice5 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellQty5 = mdEntry.Entry.MDEntrySize;
                        }
                    }

                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.HighPrice)
                    {
                        quotInfo.highPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.KnockPrice)
                    {
                        quotInfo.newPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.LowPrice)
                    {
                        quotInfo.lowPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.OpenPrice)
                    {
                        quotInfo.openPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.MaxOrderPrice)
                    {
                        quotInfo.maxOrderPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.MinOrderPrice)
                    {
                        quotInfo.minOrderPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.IOPV)
                    {
                        quotInfo.IOPV = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.BuySummary)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SellSummary)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SettlePrice)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio1)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio2)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff1)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff2)
                    {

                    }
                }
            }

            quotInfo.change = quotInfo.newPrice - quotInfo.closePrice;
            if (quotInfo.closePrice != 0.0m)
                quotInfo.changePercent = quotInfo.change / quotInfo.closePrice;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.QuotSnapCommonInfo quotSnapCommonInfo)
        {
            quotInfo.stkId = quotSnapCommonInfo.SecurityID;
            quotInfo.knockQty = quotSnapCommonInfo.TotalVolumeTrade;
            quotInfo.knockMoney = quotSnapCommonInfo.TotalValueTrade;
            quotInfo.tradeTimeFlag = quotSnapCommonInfo.TradingPhaseCode;
            quotInfo.closePrice = (decimal)quotSnapCommonInfo.PrevClosePx / 10000;
            quotInfo.changeTime = quotSnapCommonInfo.OrigTime;
        }
        #endregion



        enum Status
        {
            Stopped,
            Stopping,
            Started,
            Startting
        }
    }


}
