using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MarketMaker.Common;
using MarketMaker.Types;
using MarketMaker.Exchange;
using MarketMaker.Quote;

namespace MarketMaker.FairValue
{
    public class FairValueMgr : SingleTon<FairValueMgr>
    {
        public MarketData MyMarketData = null;
       
        public void Start(string symbol)
        {
            MyMarketData = new MarketData(symbol);
            ExchangeOKEX okexExchange = new ExchangeOKEX();
            ExchangeBinance binanceExchange = new ExchangeBinance();            
            while (!m_marketMaker.Stop())
            {
                ExchangeBase exchange = m_marketMaker.GetExchange();
                QuoteMgr quoteMgr = m_marketMaker.GetQuoteMgr();
                AutoResetEvent quoteEventObj = quoteMgr.GetQuoteEventObject();
                //Get reference price
                MarketData okexMarketData = new MarketData(symbol);
                okexExchange.UpdateMarketData(okexMarketData);
                MarketData binanceMarketData = new MarketData(symbol);
                binanceExchange.UpdateMarketData(binanceMarketData);                
                if (okexMarketData.DepthData.Mid > 0 && binanceMarketData.DepthData.Mid > 0)
                {
                    //calculate fair value and market state
                    double avgMidPrice = (okexMarketData.DepthData.Mid + binanceMarketData.DepthData.Mid) / 2;
                    MyMarketData.FairValue = avgMidPrice;
                    if (Math.Abs(okexMarketData.DepthData.Mid / binanceMarketData.DepthData.Mid - 1) >= 0.01)
                    {
                        MyMarketData.IsNormal = false;
                    }
                    else
                    {
                        MyMarketData.IsNormal = true;
                    }
                    //Get current price
                    exchange.UpdateMarketData(MyMarketData);
                    //If not mock trade. Execute anti-arbitrage check
                    if (exchange.GetType() != typeof(ExchangeMock))
                    {

                    }
                    MyMarketData.DepthData.UpdateTime = DateTime.Now;
                    quoteEventObj.Set();
                }                                              
                Thread.Sleep(50);
            }
        }

        private MarketMakerMgr m_marketMaker = MarketMakerMgr.GetInstance();
    }
}
