using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMaker.Exchange;

namespace MarketMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            MarketMakerMgr marketMakerMgr = MarketMakerMgr.GetInstance();
            marketMakerMgr.Start(new ExchangeMock(), "tbtc_tusdk");
        }
    }
}
