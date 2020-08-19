using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMaker.Common;
using MarketMaker.Types;

namespace MarketMaker.Exchange
{
    public class ExchangeOKDEX : ExchangeBase
    {
        public override void UpdateMarketData(MarketData marketData)
        {
            if (null != marketData && !string.IsNullOrEmpty(marketData.Symbol))
            {
                string url = string.Format("{0}/v1/order/depthbook?product={1}", m_urlPrefix, marketData.Symbol.Replace("-", "_"));
                string json = HttpGet(url);
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = JsonHelper.DeserializeAnonymousType(json, new { code = -1,
                          data = new { asks = new List<SortedDictionary<string, double>>(), bids = new List<SortedDictionary<string, double>>() } });
                    if(obj.code == 0)
                    {
                        for(int i=0; i<=5; i++)
                        {
                            if(obj.data.asks.Count>i)
                            {
                                marketData.DepthData.Ask[i] = obj.data.asks[i]["price"];
                                marketData.DepthData.AskVol[i] = obj.data.asks[i]["quantity"];
                            }
                            else
                            {
                                marketData.DepthData.Ask[i] = 0;
                                marketData.DepthData.AskVol[i] = 0;
                            }
                            if (obj.data.bids.Count > i)
                            {
                                marketData.DepthData.Bid[i] = obj.data.bids[i]["price"];
                                marketData.DepthData.BidVol[i] = obj.data.bids[i]["quantity"];
                            }
                            else
                            {
                                marketData.DepthData.Bid[i] = 0;
                                marketData.DepthData.BidVol[i] = 0;
                            }
                        }
                    }
                }
            }
        }

        public override void UpdateStockAccountInfo(StockAccount account)
        {
            string url = string.Format("{0}/v1/accounts/{1}?show=all", m_urlPrefix, m_adress);
            string json = HttpGet(url);
            if (!string.IsNullOrEmpty(json))
            {
                var obj = JsonHelper.DeserializeAnonymousType(json, new
                {
                    code = -1,
                    data = new { currencies = new List<SortedDictionary<string, string>>() }
                });
                if (obj.code == 0)
                {
                    SortedSet<string> existingCoinSet = new SortedSet<string>();
                    foreach(SortedDictionary<string, string> coinDict in obj.data.currencies)
                    {
                        string coinName = coinDict["symbol"];
                        double availVol = double.Parse(coinDict["available"]);
                        double lockedVol = double.Parse(coinDict["locked"]);
                        if (!account.StockPositionMap.ContainsKey(coinName))
                        {
                            account.StockPositionMap[coinName] = new StockPosition { CoinName = coinName };
                        }
                        account.StockPositionMap[coinName].AvailVolume = availVol;
                        account.StockPositionMap[coinName].TotalVolume = availVol + lockedVol;
                        existingCoinSet.Add(coinName);
                    }
                    //Remove non existing coins
                    List<string> coinNames = account.StockPositionMap.Keys.ToList();
                    foreach (string coinName in coinNames)
                    {
                        if (!existingCoinSet.Contains(coinName))
                        {
                            account.StockPositionMap.Remove(coinName);
                        }                        
                    }
                }
            }
        }
        public override bool PlaceOrder(Order order)
        {
            //Refer API https://documenter.getpostman.com/view/1112175/SzS5u6bE?version=latest#03709c4f-d620-4fef-a36a-f9cb97e909b8
            string url = string.Format("{0}/v1/txs?sync=true", m_urlPrefix);
            //to do, create data-raw... post 
            string json = HttpGet(url);
            if (!string.IsNullOrEmpty(json))
            {
                var obj = JsonHelper.DeserializeAnonymousType(json, new
                {
                    code = -1,
                    data = new { currencies = new List<SortedDictionary<string, string>>() }
                });
                if (obj.code == 0)
                {

                }
            }
            return false;
        }
        public override void UpdateOrder(Order order)
        {
            string url = string.Format("{0}/v1/order/list/deals?address={1}&product={2}&side={3}&start={4}", 
                m_urlPrefix, m_adress, TradeDirectionUtil.IsBuy(order.Direction)? "BUY" : "SELL", TimeUtil.GetLocalUnixTimestamp(DateTime.Parse(order.OrderTime)));
            string json = HttpGet(url);
            if (!string.IsNullOrEmpty(json))
            {
                var obj = JsonHelper.DeserializeAnonymousType(json, new
                {
                    code = -1,
                    data = new { currencies = new List<SortedDictionary<string, string>>() }
                });
                if (obj.code == 0)
                {
                    
                }
            }                                            
        }
        public override void CancelOrder(Order order)
        {
            //Refer API https://documenter.getpostman.com/view/1112175/SzS5u6bE?version=latest#80a454ec-276c-46b0-91f7-867cb1d5da06
            string url = string.Format("{0}/v1/txs?sync=true",m_urlPrefix);
            //to do, create data-raw... post
            string json = HttpGet(url);
            if (!string.IsNullOrEmpty(json))
            {
                var obj = JsonHelper.DeserializeAnonymousType(json, new
                {
                    code = -1,
                    data = new { currencies = new List<SortedDictionary<string, string>>() }
                });
                if (obj.code == 0)
                {

                }
            }
        }
        public override void CancelOrders(List<Order> orders)
        {
            foreach(Order order in orders)
            {
                CancelOrder(order);
            }
        }

        private string m_urlPrefix = "https://www.okex.com/okchain";
        private string m_adress = "okchain1tcuvjeu9q2x84hnrjfqpkwh0r55egqye3daa4f";
    }
}
