using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Types
{
    public class StockAccount
    {
        public SortedDictionary<string, StockPosition> StockPositionMap { get; set; }    
        
        public StockAccount()
        {
            StockPositionMap = new SortedDictionary<string, StockPosition>();
        }
        public double GetExpose(string coinName)
        {
            double exposeVolume = 0;
            if (StockPositionMap.ContainsKey(coinName))
            {
                StockPosition position = StockPositionMap[coinName];
                return position.TotalVolume - position.BaseVolume;
            }
            return exposeVolume;
        }
        public void adjustStockVolume(String coinName, double volume)
        {
            if (!StockPositionMap.ContainsKey(coinName))
            {
                StockPositionMap[coinName] = new StockPosition { CoinName = coinName };
            }
            StockPosition position = StockPositionMap[coinName];
            position.TotalVolume += volume;
            position.AvailVolume += volume;
        }

        public override string ToString()
        {
            return string.Join("\n", StockPositionMap.Values);
        }
    }
}
