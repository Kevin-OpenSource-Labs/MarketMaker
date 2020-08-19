using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Types
{
    public enum TradeDirection
    {
        Buy,
        Sell
    }
    public class TradeDirectionUtil
    {
        public static bool IsBuy(TradeDirection dir)
        {
            return dir == TradeDirection.Buy;            
        }
    }
}
