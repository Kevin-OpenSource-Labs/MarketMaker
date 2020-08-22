using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketMaker.Common;
using MarketMaker.Exchange;
using MarketMaker.Types;

namespace MarketMaker.Quote
{
    public class QuoteMgr : SingleTon<QuoteMgr>
    {
        public void Start()
        {
            m_marketMaker = MarketMakerMgr.GetInstance();
            m_quoteParameter = m_marketMaker.GetQuoteParameter();
            m_exchange = m_marketMaker.GetExchange();
            m_account = m_marketMaker.GetAccount();

            while (!m_marketMaker.Stop())
            {
                m_quoteEventObj.WaitOne();
                //update pending order containers
                List<Order> orders = (from a in m_pendingOrders where a.IsFinished || a.State == OrderState.Cancelling orderby a.OrderId select a).ToList();
                foreach (Order order in orders)
                {                    
                    m_buyPendingOrderMap.Remove(order.OrderId);
                    m_sellPendingOrderMap.Remove(order.OrderId);
                    m_pendingOrders.Remove(order);                    
                }
                //Quote strategy
                MarketData marketData = m_marketMaker.GetFairValueMgr().MyMarketData;
                if (marketData.IsNormal)
                {
                    double buyPrice = marketData.FairValue * (1 - m_quoteParameter.QuotePriceSpreadRatio);
                    double sellPrice = marketData.FairValue * (1 + m_quoteParameter.QuotePriceSpreadRatio);

                    //Anti arbitrate adjust
                    if (buyPrice >= marketData.DepthData.Ask[0])
                    {
                        buyPrice = marketData.DepthData.Ask[0];
                    }
                    if (sellPrice <= marketData.DepthData.Bid[0])
                    {
                        sellPrice = marketData.DepthData.Bid[0];
                    }
                    //To do... Other checks...

                    //quote					
                    double exposeVolume = m_account.GetExpose(marketData.Symbol);
                    double buyVolume = m_quoteParameter.QuoteVolume - exposeVolume;
                    double sellVolume = m_quoteParameter.QuoteVolume + exposeVolume;

                    //Cancel order too far from target price
                    if (m_buyPendingOrderMap.Count > 0)
                    {
                        foreach (Order order in m_buyPendingOrderMap.Values)
                        {
                            if (Math.Abs(order.Price - buyPrice) / buyPrice > m_quoteParameter.QuotePriceRatioThreshold)
                            {
                                m_exchange.CancelOrder(order);
                            }
                        }
                    }
                    if (m_sellPendingOrderMap.Count > 0)
                    {
                        foreach (Order order in m_sellPendingOrderMap.Values)
                        {
                            if (Math.Abs(order.Price - sellPrice) / sellPrice > m_quoteParameter.QuotePriceRatioThreshold)
                            {
                                m_exchange.CancelOrder(order);
                            }
                        }
                    }
                    double minVolume = m_quoteParameter.QuoteVolume * m_quoteParameter.QuoteVolumeRatioThreshold;
                    //buy order
                    if (buyVolume > minVolume)
                    {
                        Order buyOrder = new Order { StrategyId = "Maker1" };
                        buyOrder.Symbol = marketData.Symbol;
                        buyOrder.Price = buyPrice;
                        buyOrder.Volume = buyVolume;
                        if (m_exchange.PlaceOrder(buyOrder))
                        {
                            m_buyPendingOrderMap.Add(buyOrder.OrderId, buyOrder);
                            m_pendingOrders.Add(buyOrder);
                        }
                    }
                    //sell order
                    if (sellVolume > minVolume)
                    {
                        Order sellOrder = new Order { StrategyId = "Maker1" };
                        sellOrder.Symbol = marketData.Symbol;
                        sellOrder.Price = sellPrice;
                        sellOrder.Volume = sellVolume;
                        if (m_exchange.PlaceOrder(sellOrder))
                        {
                            m_sellPendingOrderMap.Add(sellOrder.OrderId, sellOrder);
                            m_pendingOrders.Add(sellOrder);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("cancel all pending orders due to abnormal state ...");
                    m_marketMaker.GetExchange().CancelOrders(m_pendingOrders);
                }
            }
        }

        public AutoResetEvent GetQuoteEventObject()
        {
            return m_quoteEventObj;
        }
        private SortedDictionary<String, Order> m_buyPendingOrderMap = new SortedDictionary<String, Order>();
        private SortedDictionary<String, Order> m_sellPendingOrderMap = new SortedDictionary<String, Order>();
        private List<Order> m_pendingOrders = new List<Order>();

        private MarketMakerMgr m_marketMaker = null;
        private ExchangeBase m_exchange = null;
        private StockAccount m_account = null;
        private QuoteParameter m_quoteParameter = null;
        private AutoResetEvent m_quoteEventObj = new AutoResetEvent(false);
    }

}
