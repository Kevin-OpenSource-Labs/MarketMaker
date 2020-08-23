using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MarketMaker.Common;
using MarketMaker.Types;
using MarketMaker.Exchange;
using MarketMaker.FairValue;

namespace MarketMaker.RiskControl
{
    public class AutoHedgeMgr : SingleTon<AutoHedgeMgr>
    {
        public void Start()
        {
            m_marketMakerMgr = MarketMakerMgr.GetInstance();
            m_exchange = m_marketMakerMgr.GetExchange();
            m_account = m_marketMakerMgr.GetAccount();
            m_fairValueMgr = m_marketMakerMgr.GetFairValueMgr();
            m_quoteParameter = m_marketMakerMgr.GetQuoteParameter();

            long lastUpdateAccountTime = DateTime.Now.Ticks;
            while (!m_marketMakerMgr.Stop())
            {
                long now = DateTime.Now.Ticks;
                if (now - lastUpdateAccountTime >= 1000 * 10)
                {
                    SyncAccountInfo();
                    lastUpdateAccountTime = now;
                }
                AutoHedgeIfNeed();
                Thread.Sleep(1000);               
            }
        }
        public void SyncAccountInfo()
        {
            m_exchange.UpdateStockAccountInfo(m_account);
        }

        public void AutoHedgeIfNeed()
        {
            if(null == m_fairValueMgr.MyMarketData)
            {
                return;
            }
            String symbol = m_fairValueMgr.MyMarketData.Symbol;
            if (m_account.StockPositionMap.ContainsKey(symbol))
            {
                //cancel hedge orders and remove finished orders
                List<Order> orders = m_hedgeOrders.ToList();
                foreach (Order order in orders)
                {
                    m_exchange.UpdateOrder(order);
                    if (order.IsFinished)
                    {
                        m_hedgeOrders.Remove(order);
                    }
                    else
                    {
                        m_exchange.CancelOrder(order);
                    }
                }

                //to do hedge
                double exposeVolume = m_account.GetExpose(symbol);
                exposeVolume = Math.Round(exposeVolume, 8);
                Console.Title = string.Format("[ MarketMaker ] - MockTrade: {0}, Symbol: {1}, Expose: {2}, Pending Orders Count: {3}",
                    m_marketMakerMgr.MockTrade() ? 1 : 0, m_marketMakerMgr.GetQuoteParameter().Symbol, exposeVolume, m_marketMakerMgr.GetQuoteMgr().GetPendingOrders().Count);
                if (Math.Abs(exposeVolume) > m_quoteParameter.AutoHedgeVolumeThreshold)
                {
                    m_logMgr.Warn(string.Format("Need Hedge, Expose={0}, Threshold={1}", exposeVolume, m_quoteParameter.AutoHedgeVolumeThreshold));
                    if (exposeVolume > 0)
                    {
                        Order sellOrder = new Order();
                        sellOrder.Direction = TradeDirection.Sell;
                        sellOrder.Symbol = symbol;
                        sellOrder.Price = m_marketMakerMgr.GetFairValueMgr().MyMarketData.DepthData.Bid[0] * 0.99;
                        sellOrder.Volume = Math.Round(exposeVolume / 2, 8);
                        if (m_exchange.PlaceOrder(sellOrder))
                        {
                            m_hedgeOrders.Add(sellOrder);
                        }
                    }
                    else
                    {
                        Order buyOrder = new Order();
                        buyOrder.Direction = TradeDirection.Buy;
                        buyOrder.Symbol = symbol;
                        buyOrder.Price = m_marketMakerMgr.GetFairValueMgr().MyMarketData.DepthData.Ask[0] * 1.01;
                        buyOrder.Volume = Math.Round(exposeVolume / 2, 8);
                        if (m_exchange.PlaceOrder(buyOrder))
                        {
                            m_hedgeOrders.Add(buyOrder);
                        }
                    }
                }
            }
        }

        private LogMgr m_logMgr = LogMgr.GetInstance();
        private MarketMakerMgr m_marketMakerMgr = null;
        private FairValueMgr m_fairValueMgr = null;
        private ExchangeBase m_exchange = null;
        private StockAccount m_account = null;
        private QuoteParameter m_quoteParameter = null;
        private List<Order> m_hedgeOrders = new List<Order>();
    }
}
