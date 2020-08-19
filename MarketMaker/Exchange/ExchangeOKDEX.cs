using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMaker.Types;

namespace MarketMaker.Exchange
{
    public class ExchangeOKDEX : ExchangeBase
    {
        public override void UpdateMarketData(MarketData marketData)
        {
            
        }

        public override void UpdateStockAccountInfo(StockAccount account)
        {

        }
        public override bool PlaceOrder(Order order)
        {
            return false;
        }
        public override void UpdateOrder(Order order)
        {
            
        }
        public override void CancelOrder(Order order)
        {
            
        }
        public override void CancelOrders(List<Order> orders)
        {
            
        }        
    }
}
