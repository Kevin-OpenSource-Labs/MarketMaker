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
            ExchangeOKEX okexExchange = new ExchangeOKEX();
            ExchangeBinance binanceExchange = new ExchangeBinance();
            MyMarketData = new MarketData(symbol);
            while (!m_marketMaker.Stop())
            {
                ExchangeBase exchange = m_marketMaker.GetExchange();
                QuoteMgr quoteMgr = m_marketMaker.GetQuoteMgr();
                AutoResetEvent quoteEventObj = quoteMgr.GetQuoteEventObject();
                //mock trade
                if (exchange.GetType() == typeof(ExchangeMock))
                {
                    exchange.UpdateMarketData(MyMarketData);
                    MyMarketData.FairValue = MyMarketData.DepthData.Mid + 10 *  (new Random()).NextDouble();
                    MyMarketData.IsNormal = true;
                    MyMarketData.DepthData.UpdateTime = DateTime.Now;               
                    quoteEventObj.Set();                    
                }
                //real trade
                else
                {
                    MarketData okexMarketData = new MarketData(symbol);
                    okexExchange.UpdateMarketData(okexMarketData);
                    MarketData binanceMarketData = new MarketData(symbol);
                    binanceExchange.UpdateMarketData(binanceMarketData);
                    if (okexMarketData.DepthData.Mid > 0 && binanceMarketData.DepthData.Mid > 0)
                    {
                        //calculate fair value and market state
                        double avgMidPrice = (okexMarketData.DepthData.Mid + binanceMarketData.DepthData.Mid) / 2;
                        MyMarketData.FairValue = avgMidPrice;
                        if (Math.Abs(okexMarketData.DepthData.Mid - binanceMarketData.DepthData.Mid) / 2 >= 0.01)
                        {
                            MyMarketData.IsNormal = false;
                        }
                        else
                        {
                            MyMarketData.IsNormal = true;
                        }
                        MyMarketData.DepthData.UpdateTime = DateTime.Now;
                        quoteEventObj.Set();
                    }
                }                                
                Thread.Sleep(50);
            }
        }

        private MarketMakerMgr m_marketMaker = MarketMakerMgr.GetInstance();
    }
}
