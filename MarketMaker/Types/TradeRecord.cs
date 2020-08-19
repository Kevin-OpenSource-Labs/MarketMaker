using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Types
{
    public class TradeRecord
    {
        public String TradeTime { get; set; }
        public String OrderId { get; set; }
        public String Symbol { get; set; }
        public TradeDirection Direction { get; set; }
        public double TradedVolume { get; set; }
        public double TradedPrice { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5}", TradeTime, OrderId, Symbol, Direction, TradedVolume, TradedPrice);
        }
    }
}
