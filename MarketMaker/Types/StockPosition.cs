using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Types
{
    public class StockPosition
    {
        public string CoinName { get; set; }
        public double AvailVolume { get; set; }
        public double TotalVolume { get; set; }
        public double BaseVolume { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, Avail={1}, Total={2}, Base={3}", CoinName, AvailVolume, TotalVolume, BaseVolume);
        }
    }
}
