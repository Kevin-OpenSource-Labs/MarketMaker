using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MarketMaker.Types;

namespace MarketMaker.Exchange
{
    public class ExchangeMock : ExchangeBase
    {
        public override void UpdateMarketData(MarketData marketData)
        {
            marketData.DepthData.Ask[0] = 1000 + 5 * (new Random()).NextDouble();
            marketData.DepthData.AskVol[0] = 1;
            marketData.DepthData.Bid[0] = 1000 - 5 * (new Random()).NextDouble();
            marketData.DepthData.BidVol[0] = 1;
            marketData.DepthData.Mid = 1000;
        }

        public override void UpdateStockAccountInfo(StockAccount account)
        {
            //first time to initialized
            if (account.StockPositionMap.Count <= 0)
            {
                String symbol = MarketMakerMgr.GetInstance().GetFairValueMgr().MyMarketData.Symbol;
                StockPosition position = new StockPosition();
                position.BaseVolume = position.TotalVolume = position.AvailVolume = 100;
                account.StockPositionMap[symbol] = position;
            }
        }

        public override bool PlaceOrder(Order order)
        {
            //start mock trade thread
            if (m_mockTradeThread == null)
            {
                StartMockTradeThread();
            }

            order.OrderId = order.OrderTime = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");
            order.State = OrderState.Submitted;
            order.IsFinished = false;
            m_pendingOrders.Add(order);
            return true;
        }

        public override void UpdateOrder(Order order)
        {
            
        }

        public override void CancelOrder(Order order)
        {
            order.IsFinished = true;
            if (order.TradedVolume >= order.Volume)
            {
                order.State = OrderState.Filled;
            }
            else if (order.TradedVolume <= 0)
            {
                order.State = OrderState.Cancelled;
            }
            else
            {
                order.State = OrderState.Partially_Cancelled;
            }
        }

        public override void CancelOrders(List<Order> orders)
        {
            foreach (Order order in orders)
            {
                CancelOrder(order);
            }
        }

        private void StartMockTradeThread()
        {
            m_mockTradeThread = new Thread(() =>
            {
                while (!m_marketMakerMgr.Stop())
                {
                    foreach (Order order in m_pendingOrders)
                    {
                        if (!order.IsFinished)
                        {
                            //Mock two phase trades
                            TradeRecord record = new TradeRecord();
                            record.TradeTime = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");
                            record.OrderId = order.OrderId;
                            record.Symbol = order.Symbol;
                            record.Direction = order.Direction;
                            record.TradedPrice = order.Price;
                            record.TradedVolume = order.Volume / 2;
                            NotifyTrade(record);

                            if (order.TradedVolume <= 0)
                            {
                                order.TradedPrice = order.Price;
                                order.TradedVolume = order.Volume / 2;
                                order.State = OrderState.Partially_Filled;
                                order.IsFinished = false;
                            }
                            else
                            {
                                order.TradedPrice = order.Price;
                                order.TradedVolume = order.Volume;
                                order.State = OrderState.Filled;
                                order.IsFinished = true;
                            }
                        }
                        if (order.IsFinished)
                        {
                            m_pendingOrders.Remove(order);
                        }
                    }
                    Thread.Sleep(2000);
                }
            });
            m_mockTradeThread.Start();
        }

        private Thread m_mockTradeThread = null;
        private List<Order> m_pendingOrders = new List<Order>();
    }
}
