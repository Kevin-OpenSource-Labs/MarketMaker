using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMaker.Common;
using MarketMaker.Types;

namespace MarketMaker.Exchange
{
    public class ExchangeBinance : ExchangeBase
    {
        public override void UpdateMarketData(MarketData marketData)
        {
            if (marketData.Symbol != null && marketData.Symbol != "")
            {
                string symbol = ConvertFromDEXSymbol(marketData.Symbol).Replace("_", "").ToUpper();
                string binanceHttpUrl = String.Format("https://api.binance.com/api/v3/depth?symbol={0}&limit=5", symbol);                
                string binanceJson = HttpGet(binanceHttpUrl);
                if (binanceJson != null && binanceJson != "")
                {
                    //parse binance market data
                    var obj = JsonHelper.DeserializeAnonymousType(binanceJson, new { asks = new List<double[]>(), bids = new List<double[]>() });                   
                    MarketDepthData depthData = marketData.DepthData;
                    depthData.Ask[0] = obj.asks[0][0];
                    depthData.Bid[0] = obj.bids[0][0];
                    if (depthData.Ask[0] > 0 && depthData.Bid[0] > 0)
                    {
                        depthData.Mid = (depthData.Ask[0] + depthData.Bid[0]) / 2;
                    }
                    depthData.UpdateTime = DateTime.Now;
                }
            }
        }
    }
}
