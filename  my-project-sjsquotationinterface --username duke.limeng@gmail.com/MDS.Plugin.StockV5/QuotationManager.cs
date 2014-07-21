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
        private QuotV5.Binary.RealTimeQuotConnection[] quotConnections;
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
            QuotV5.Binary.RealTimeQuotConnection[] quotConnections,
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
            this.quotConnections = quotConnections;
            this.quotRepository = quotRepository;
            this.quotPublisher = quotPublisher;
            this.securityInfoProvider = securityInfoProvider;
            this.indexInfoProvider = indexInfoProvider;
            this.cashAuctionParamsProvider = cashAuctionParamsProvider;
            this.derivativeAuctionParamsProvider = derivativeAuctionParamsProvider;
            this.negotiationParamsProvider = negotiationParamsProvider;
            this.securityCloseMDProvider = securityCloseMDProvider;
            this.logHelper = logHelper;

            foreach (var conn in this.quotConnections)
                conn.OnMarketDataReceived += new Action<QuotV5.Binary.MarketDataEx>(QuotConn_OnMarketDataReceived);
            this.securityInfoProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.SecurityInfoBase>>(SecurityInfoProvider_OnStaticInfoRead);
            this.indexInfoProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.IndexInfo>>(IndexInfoProvider_OnStaticInfoRead);
            this.cashAuctionParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.CashAuctionParams>>(CashAuctionParamsProvider_OnStaticInfoRead);
            this.derivativeAuctionParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.DerivativeAuctionParams>>(DerivativeAuctionParamsProvider_OnStaticInfoRead);
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

                foreach (var conn in this.quotConnections)
                    conn.Start();
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
                foreach (var conn in this.quotConnections)
                    conn.Stop();
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

                var allQuotInfos = this.quotRepository.GetAllStockQuotation();
                ProcessedDataSnap.StockQuotation.Clear();
                if (allQuotInfos != null)
                {
                    foreach (var qi in allQuotInfos)
                    {
                        ProcessedDataSnap.StockQuotation[qi.stkId] = qi;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "从Redis中读取StockQuotation快照异常");
            }

            try
            {

                var allQuotInfos = this.quotRepository.GetAllFutureQuotation();
                ProcessedDataSnap.FutureQuotation.Clear();
                if (allQuotInfos != null)
                {
                    foreach (var qi in allQuotInfos)
                    {
                        ProcessedDataSnap.FutureQuotation[qi.stkId] = qi;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "从Redis中读取FutureQuotation快照异常");
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

            if (securityInfo is QuotV5.StaticInfo.OptionSecuirtyInfo)
            {
                QuotV5.StaticInfo.OptionSecuirtyInfo optionSecurityInfo = securityInfo as QuotV5.StaticInfo.OptionSecuirtyInfo;
                FutureQuotation preFutureQuotation = null;
                FutureQuotation newFutureQuotation = null;
                if (ProcessedDataSnap.FutureQuotation.TryGetValue(securityInfo.CommonInfo.SecurityID, out preFutureQuotation))
                {
                    newFutureQuotation = preFutureQuotation.Clone();
                    SetFutureQuotationProperty(newFutureQuotation, optionSecurityInfo);
                }
                else
                {
                    newFutureQuotation = new FutureQuotation();
                    SetFutureQuotationProperty(newFutureQuotation, optionSecurityInfo);
                }

                if (newFutureQuotation != null)
                {
                    ProcessedDataSnap.FutureQuotation[newFutureQuotation.stkId] = newFutureQuotation;
                    quotRepository.UpdateFutureQuotation(newFutureQuotation);

                }
            }
            else
            {

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
            FutureQuotation preQuotInfo = null;
            FutureQuotation newQuotInfo = null;

            if (ProcessedDataSnap.FutureQuotation.TryGetValue(securityInfo.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                SetFutureQuotationProperty(newQuotInfo, securityInfo);
            }
            else
            {
                newQuotInfo = new FutureQuotation();
                SetFutureQuotationProperty(newQuotInfo, securityInfo);
            }

            if (newQuotInfo != null)
            {
                ProcessedDataSnap.FutureQuotation[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateFutureQuotation(newQuotInfo);

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
            stockInfo.marketMakerFlag = cashAuctionParams.MarketMakerFlag;
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
        private void QuotConn_OnMarketDataReceived(QuotV5.Binary.MarketDataEx marketData)
        {
            if (marketData.MarketData is QuotV5.Binary.QuotSnap300111)
            {
                ProcessMarketData(marketData.MarketData as QuotV5.Binary.QuotSnap300111, marketData.ReceiveTime);
            }
            else if (marketData.MarketData is QuotV5.Binary.QuotSnap300611)
            {
                ProcessMarketData(marketData.MarketData as QuotV5.Binary.QuotSnap300611, marketData.ReceiveTime);
            }
            else if (marketData.MarketData is QuotV5.Binary.QuotSnap309011)
            {
                ProcessMarketData(marketData.MarketData as QuotV5.Binary.QuotSnap309011, marketData.ReceiveTime);
            }
            else if (marketData.MarketData is QuotV5.Binary.RealtimeStatus)
            {
                ProcessMarketData(marketData.MarketData as QuotV5.Binary.RealtimeStatus, marketData.ReceiveTime);
            }
        }

        private void ProcessMarketData(QuotV5.Binary.QuotSnap300111 quotSnap, DateTime receiveTime)
        {
            RawQuotationInfoSnap.QuotSnap300111[quotSnap.CommonInfo.SecurityID] = quotSnap;
            if (quotSnap.CommonInfo.MDStreamID == QuotV5.Binary.QuotSnap300111.MDStreamID.Option)
            {
                FutureQuotation preQuotInfo = null;
                FutureQuotation newQuotInfo = null;
                if (ProcessedDataSnap.FutureQuotation.TryGetValue(quotSnap.CommonInfo.SecurityID, out preQuotInfo))
                {
                    newQuotInfo = preQuotInfo.Clone();
                    QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                    if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                        SetFutureQuotationProperty(newQuotInfo, securityInfo as QuotV5.StaticInfo.OptionSecuirtyInfo);

                    SetFutureQuotationProperty(newQuotInfo, quotSnap);
                }
                else
                {
                    newQuotInfo = new FutureQuotation();

                    QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                    if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                        SetFutureQuotationProperty(newQuotInfo, securityInfo as QuotV5.StaticInfo.OptionSecuirtyInfo);

                    QuotV5.Binary.RealtimeStatus status = null;
                    if (RawQuotationInfoSnap.RealtimeStatus.TryGetValue(quotSnap.CommonInfo.SecurityID, out status))
                        SetFutureQuotationProperty(newQuotInfo, status);

                    SetFutureQuotationProperty(newQuotInfo, quotSnap);
                }
                if (newQuotInfo != null)
                {
                   
                    ProcessedDataSnap.FutureQuotation[newQuotInfo.stkId] = newQuotInfo;
                    quotRepository.UpdateFutureQuotation(newQuotInfo);
                    if (quotPublisher != null)
                        quotPublisher.Enqueue(newQuotInfo, true);
                }

            }
            else
            {
                StockQuotation preQuotInfo = null;
                StockQuotation newQuotInfo = null;
                if (ProcessedDataSnap.StockQuotation.TryGetValue(quotSnap.CommonInfo.SecurityID, out preQuotInfo))
                {
                    newQuotInfo = preQuotInfo.Clone();
                    QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                    if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                        SetStockQuotationProperty(newQuotInfo, securityInfo);

                    SetStockQuotationProperty(newQuotInfo, quotSnap);
                }
                else
                {
                    newQuotInfo = new StockQuotation();

                    QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                    if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                        SetStockQuotationProperty(newQuotInfo, securityInfo);

                    QuotV5.Binary.RealtimeStatus status = null;
                    if (RawQuotationInfoSnap.RealtimeStatus.TryGetValue(quotSnap.CommonInfo.SecurityID, out status))
                        SetStockQuotationProperty(newQuotInfo, status);

                    SetStockQuotationProperty(newQuotInfo, quotSnap);
                }
                if (newQuotInfo != null)
                {
                    newQuotInfo.receiveTime = DatetimeToLong(receiveTime);
                    ProcessedDataSnap.StockQuotation[newQuotInfo.stkId] = newQuotInfo;
                    quotRepository.UpdateStockQuotation(newQuotInfo);
                    if (quotPublisher != null)
                        quotPublisher.Enqueue(newQuotInfo, true);
                }
            }
        }

        private void ProcessMarketData(QuotV5.Binary.QuotSnap300611 quotSnap, DateTime receiveTime)
        {
            RawQuotationInfoSnap.QuotSnap300611[quotSnap.CommonInfo.SecurityID] = quotSnap;
            StockQuotation preQuotInfo = null;
            StockQuotation newQuotInfo = null;
            if (ProcessedDataSnap.StockQuotation.TryGetValue(quotSnap.CommonInfo.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                    SetStockQuotationProperty(newQuotInfo, securityInfo);
                SetStockQuotationProperty(newQuotInfo, quotSnap);
            }
            else
            {
                newQuotInfo = new StockQuotation();

                QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                if (RawStaticInfoSnap.SecurityInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                    SetStockQuotationProperty(newQuotInfo, securityInfo);

                QuotV5.Binary.RealtimeStatus status = null;
                if (RawQuotationInfoSnap.RealtimeStatus.TryGetValue(quotSnap.CommonInfo.SecurityID, out status))
                    SetStockQuotationProperty(newQuotInfo, status);

                SetStockQuotationProperty(newQuotInfo, quotSnap);



            }
            if (newQuotInfo != null)
            {
                newQuotInfo.receiveTime = DatetimeToLong(receiveTime);
                ProcessedDataSnap.StockQuotation[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateStockQuotation(newQuotInfo);
                if (quotPublisher != null)
                    quotPublisher.Enqueue(newQuotInfo, true);
            }
        }

        private void ProcessMarketData(QuotV5.Binary.QuotSnap309011 quotSnap, DateTime receiveTime)
        {
            RawQuotationInfoSnap.QuotSnap309011[quotSnap.CommonInfo.SecurityID] = quotSnap;
            StockQuotation preQuotInfo = null;
            StockQuotation newQuotInfo = null;
            if (ProcessedDataSnap.StockQuotation.TryGetValue(quotSnap.CommonInfo.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                QuotV5.StaticInfo.IndexInfo securityInfo = null;
                if (RawStaticInfoSnap.IndexInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                    SetStockQuotationProperty(newQuotInfo, securityInfo);
                SetStockQuotationProperty(newQuotInfo, quotSnap);
            }
            else
            {
                newQuotInfo = new StockQuotation();

                QuotV5.StaticInfo.IndexInfo securityInfo = null;
                if (RawStaticInfoSnap.IndexInfo.TryGetValue(quotSnap.CommonInfo.SecurityID, out securityInfo))
                    SetStockQuotationProperty(newQuotInfo, securityInfo);

                QuotV5.Binary.RealtimeStatus status = null;
                if (RawQuotationInfoSnap.RealtimeStatus.TryGetValue(quotSnap.CommonInfo.SecurityID, out status))
                    SetStockQuotationProperty(newQuotInfo, status);

                SetStockQuotationProperty(newQuotInfo, quotSnap);
            }
            if (newQuotInfo != null)
            {
                newQuotInfo.receiveTime = DatetimeToLong(receiveTime);
                ProcessedDataSnap.StockQuotation[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateStockQuotation(newQuotInfo);
                if (quotPublisher != null)
                    quotPublisher.Enqueue(newQuotInfo, true);
            }
        }

        private void ProcessMarketData(QuotV5.Binary.RealtimeStatus status, DateTime receiveTime)
        {
            RawQuotationInfoSnap.RealtimeStatus[status.Status.SecurityID] = status;
            StockQuotation preQuotInfo = null;
            StockQuotation newQuotInfo = null;
            if (ProcessedDataSnap.StockQuotation.TryGetValue(status.Status.SecurityID, out preQuotInfo))
            {
                newQuotInfo = preQuotInfo.Clone();
                SetStockQuotationProperty(newQuotInfo, status);
            }
            else
            {
                newQuotInfo = new StockQuotation();

                QuotV5.StaticInfo.SecurityInfoBase securityInfo = null;
                if (RawStaticInfoSnap.SecurityInfo.TryGetValue(status.Status.SecurityID, out securityInfo))
                    SetStockQuotationProperty(newQuotInfo, securityInfo);

                SetStockQuotationProperty(newQuotInfo, status);

                QuotV5.Binary.QuotSnap300111 quotSnap = null;
                if (RawQuotationInfoSnap.QuotSnap300111.TryGetValue(status.Status.SecurityID, out quotSnap))
                    SetStockQuotationProperty(newQuotInfo, quotSnap);

                QuotV5.Binary.QuotSnap300611 quotSnap2 = null;
                if (RawQuotationInfoSnap.QuotSnap300611.TryGetValue(status.Status.SecurityID, out quotSnap2))
                    SetStockQuotationProperty(newQuotInfo, quotSnap2);

            }
            if (newQuotInfo != null)
            {
                newQuotInfo.receiveTime = DatetimeToLong(receiveTime);
                ProcessedDataSnap.StockQuotation[newQuotInfo.stkId] = newQuotInfo;
                quotRepository.UpdateStockQuotation(newQuotInfo);
                if (quotPublisher != null)
                    quotPublisher.Enqueue(newQuotInfo, true);
            }
        }
        #endregion

        #region 为StockQuotation属性赋值
        private void SetStockQuotationProperty(StockQuotation quotInfo, QuotV5.StaticInfo.IndexInfo securityInfo)
        {
            quotInfo.stkId = securityInfo.SecurityID;
            quotInfo.stkName = securityInfo.Symbol;
        }

        private void SetStockQuotationProperty(StockQuotation quotInfo, QuotV5.StaticInfo.SecurityInfoBase securityInfo)
        {
            quotInfo.stkId = securityInfo.CommonInfo.SecurityID;
            quotInfo.stkName = securityInfo.CommonInfo.Symbol;
        }

        private void SetStockQuotationProperty(StockQuotation quotInfo, QuotV5.Binary.RealtimeStatus status)
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

        private void SetStockQuotationProperty(StockQuotation quotInfo, QuotV5.Binary.QuotSnap309011 quotSnap)
        {
            SetStockQuotationProperty(quotInfo, quotSnap.CommonInfo);
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

        private void SetStockQuotationProperty(StockQuotation quotInfo, QuotV5.Binary.QuotSnap300611 quotSnap)
        {
            SetStockQuotationProperty(quotInfo, quotSnap.CommonInfo);


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

        private void SetStockQuotationProperty(StockQuotation quotInfo, QuotV5.Binary.QuotSnap300111 quotSnap)
        {
            SetStockQuotationProperty(quotInfo, quotSnap.CommonInfo);
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
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.BuySummary)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SellSummary)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SettlePrice)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio1)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio2)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff1)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff2)
                    //{

                    //}
                }
            }

            quotInfo.change = quotInfo.newPrice - quotInfo.closePrice;
            if (quotInfo.closePrice != 0.0m)
                quotInfo.changePercent = quotInfo.change / quotInfo.closePrice;
        }

        private void SetStockQuotationProperty(StockQuotation quotInfo, QuotV5.Binary.QuotSnapCommonInfo quotSnapCommonInfo)
        {
            quotInfo.stkId = quotSnapCommonInfo.SecurityID;
            quotInfo.knockQty = quotSnapCommonInfo.TotalVolumeTrade / 100;
            quotInfo.knockMoney = quotSnapCommonInfo.TotalValueTrade / 10000;
            quotInfo.tradeTimeFlag = quotSnapCommonInfo.TradingPhaseCode;
            quotInfo.closePrice = (decimal)quotSnapCommonInfo.PrevClosePx / 10000;
            quotInfo.changeTime = quotSnapCommonInfo.OrigTime;
        }
        #endregion

        #region 为FutureQuotation属性赋值

        private void SetFutureQuotationProperty(FutureQuotation quotInfo, QuotV5.StaticInfo.OptionSecuirtyInfo securityInfo)
        {

            quotInfo.stkId = securityInfo.CommonInfo.SecurityID;
            quotInfo.stkName = securityInfo.CommonInfo.Symbol;
            quotInfo.preClosePrice = securityInfo.CommonInfo.PrevClosePx;
            quotInfo.basicExchId = null;
            quotInfo.basicStkId = securityInfo.CommonInfo.UnderlyingSecurityID;
            quotInfo.contractTimes = (int)securityInfo.Params.ContractUnit;
            Date lastTradeDay = Date.FromYMMDD(securityInfo.Params.LastTradeDay);
            DateTime lastTradeDate = lastTradeDay.ToDateTime();
            quotInfo.deliveryYear = lastTradeDay.Year;
            quotInfo.deliveryMonth = lastTradeDay.Month;
            quotInfo.listDate = securityInfo.CommonInfo.ListDate;
            quotInfo.firstTrdDate = quotInfo.listDate;
            quotInfo.lastTrdDate = securityInfo.Params.LastTradeDay;
            quotInfo.matureDate = securityInfo.Params.LastTradeDay;
            quotInfo.lastSettleDate = securityInfo.Params.LastTradeDay;

            quotInfo.preSettlementPrice = securityInfo.Params.PrevClearingPrice;
            quotInfo.preClosePrice = securityInfo.CommonInfo.PrevClosePx;
            quotInfo.strikePrice = securityInfo.Params.ExercisePrice;
            quotInfo.optionType = securityInfo.Params.CallOrPut;
            quotInfo.exerciseDate = securityInfo.Params.ExerciseBeginDate;
            quotInfo.adjustedFlag = securityInfo.Params.Adjusted;
            quotInfo.adjustNum = securityInfo.Params.AdjuestTimes;

            quotInfo.isinCode = securityInfo.CommonInfo.ISIN;

            if (securityInfo.CommonInfo.SecurityType == QuotV5.StaticInfo.SecurityType.股票期权)
            {
                quotInfo.F_ProductClass = "SO";
            }
            else
            {
                quotInfo.F_ProductClass = "EO";
            }
        }

        private void SetFutureQuotationProperty(FutureQuotation quotInfo, QuotV5.StaticInfo.DerivativeAuctionParams derivativeAuctionParams)
        {
            quotInfo.stkId = derivativeAuctionParams.SecurityID;
            quotInfo.buyMaxLimitOrderQty = (int)derivativeAuctionParams.BuyQtyUpperLimit;
            quotInfo.sellMaxLimitOrderQty = (int)derivativeAuctionParams.SellQtyUpperLimit;
            quotInfo.buyMaxMarketOrderQty = quotInfo.buyMaxLimitOrderQty;
            quotInfo.sellMaxMarketOrderQty = quotInfo.sellMaxMarketOrderQty;

            quotInfo.orderPriceUnit = derivativeAuctionParams.PriceTick;
            quotInfo.maxOrderPrice = derivativeAuctionParams.RisePrice;
            quotInfo.minOrderPrice = derivativeAuctionParams.FallPrice;
            quotInfo.marketMakerFlag = derivativeAuctionParams.MarketMakerFlag;
            quotInfo.currMargin = derivativeAuctionParams.SellMargin;
            quotInfo.minBuyQtyTimes = (int)derivativeAuctionParams.BuyQtyUnit;
            quotInfo.minSellQtyTimes = (int)derivativeAuctionParams.SellQtyUnit;
            quotInfo.preCurrMargin = derivativeAuctionParams.LastSellMargin;
            quotInfo.marketMakerFlag = derivativeAuctionParams.MarketMakerFlag;
        }


        private void SetFutureQuotationProperty(FutureQuotation quotInfo, QuotV5.StaticInfo.SecurityCloseMD securityCloseMD)
        {
            quotInfo.stkId = securityCloseMD.SecurityID;
            quotInfo.exchTotalKnockAmt = securityCloseMD.TotalValueTrade;
            quotInfo.exchTotalKnockQty = (Int64)securityCloseMD.TotalVolumeTrade;
            quotInfo.closePrice = securityCloseMD.ClosePx;
        }

        private void SetFutureQuotationProperty(FutureQuotation quotInfo, QuotV5.Binary.QuotSnap300111 quotSnap)
        {
            SetFutureQuotationProperty(quotInfo, quotSnap.CommonInfo);


            if (quotSnap.ExtInfo != null && quotSnap.ExtInfo.MDEntries != null && quotSnap.ExtInfo.MDEntries.Length > 0)
            {
                decimal d = 1000000;
                foreach (var mdEntry in quotSnap.ExtInfo.MDEntries)
                {
                    if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.BuyPrice)
                    {
                        if (mdEntry.Entry.MDPriceLevel == 1)
                        {
                            quotInfo.buy1 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyAmt1 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 2)
                        {
                            quotInfo.buy2 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyAmt2 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 3)
                        {
                            quotInfo.buy3 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyAmt3 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 4)
                        {
                            quotInfo.buy4 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyAmt4 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 5)
                        {
                            quotInfo.buy5 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.buyAmt5 = mdEntry.Entry.MDEntrySize;
                        }

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SellPrice)
                    {
                        if (mdEntry.Entry.MDPriceLevel == 1)
                        {
                            quotInfo.sell1 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellAmt1 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 2)
                        {
                            quotInfo.sell2 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellAmt2 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 3)
                        {
                            quotInfo.sell3 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellAmt3 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 4)
                        {
                            quotInfo.sell4 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellAmt4 = mdEntry.Entry.MDEntrySize;
                        }
                        else if (mdEntry.Entry.MDPriceLevel == 5)
                        {
                            quotInfo.sell5 = (decimal)mdEntry.Entry.MDEntryPx / d;
                            quotInfo.sellAmt5 = mdEntry.Entry.MDEntrySize;
                        }
                    }

                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.HighPrice)
                    {
                        quotInfo.highestPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.KnockPrice)
                    {
                        quotInfo.newPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.LowPrice)
                    {
                        quotInfo.lowestPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
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
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SettlePrice)
                    {
                        quotInfo.settlementPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.OpenPosition)
                    {
                        quotInfo.openPosition = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.IOPV)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.BuySummary)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SellSummary)
                    //{

                    //}
                  
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio1)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio2)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff1)
                    //{

                    //}
                    //else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff2)
                    //{

                    //}
                   
                }
            }

        }

        private void SetFutureQuotationProperty(FutureQuotation quotInfo, QuotV5.Binary.QuotSnapCommonInfo quotSnapCommonInfo)
        {
            quotInfo.stkId = quotSnapCommonInfo.SecurityID;

            quotInfo.stkStatus = null;
            if (!string.IsNullOrEmpty(quotSnapCommonInfo.TradingPhaseCode) && quotSnapCommonInfo.TradingPhaseCode.Length > 1)
            {
                string flag = quotSnapCommonInfo.TradingPhaseCode.Substring(1, 1);
                if (flag == "0")
                    quotInfo.stkStatus = "LIST";
                else if (flag == "1")
                    quotInfo.stkStatus = "PAUSE";
            }

            quotInfo.exchTotalKnockAmt = quotSnapCommonInfo.TotalValueTrade / 10000;
            quotInfo.exchTotalKnockQty = quotSnapCommonInfo.TotalVolumeTrade / 100;
           // quotInfo.preClosePrice = quotSnapCommonInfo.PrevClosePx;
            quotInfo.lastModifyTime = quotSnapCommonInfo.OrigTime;
        }

        private void SetFutureQuotationProperty(FutureQuotation quotInfo, QuotV5.Binary.RealtimeStatus status)
        {
            quotInfo.stkId = status.Status.SecurityID;
        }

        #endregion


        private Int64 DatetimeToLong(DateTime time)
        {
            return (Int64)(time.Year * 1000000000000000) + (Int64)(time.Month * 100000000000) + (Int64)(time.Day * 1000000000) + (Int64)(time.Hour * 10000000) + (Int64)(time.Minute * 100000) + (Int64)(time.Second * 1000) + (Int64)time.Millisecond;
        }

        enum Status
        {
            Stopped,
            Stopping,
            Started,
            Startting
        }

       
    }


}
