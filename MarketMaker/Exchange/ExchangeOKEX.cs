using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMaker.Common;
using MarketMaker.Types;

namespace MarketMaker.Exchange
{
    public class ExchangeOKEX : ExchangeBase
    {
        public override void UpdateMarketData(MarketData marketData)
        {
            if (marketData.Symbol != null && marketData.Symbol != "")
            {
                string symbol = ConvertFromDEXSymbol(marketData.Symbol);
                string okexHttpUrl = string.Format("https://www.okex.com/api/spot/v3/instruments/{0}/book?size=5", symbol);                
                string okexJson = HttpGet(okexHttpUrl);
                if (okexJson != null && okexJson != "")
                {
                    //parse okex market data    
                    var obj = JsonHelper.DeserializeAnonymousType(okexJson, new { asks = new List<double[]>(), bids = new List<double[]>() });
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
