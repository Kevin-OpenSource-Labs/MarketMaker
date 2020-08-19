using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Types
{
    public class MarketDepthData
    {
        public double Mid { get; set; }
        public double[] Bid { get; set; }
        public double[] BidVol { get; set; }
        public double[] Ask { get; set; }
        public double[] AskVol { get; set; }
        public DateTime UpdateTime { get; set; }

        public MarketDepthData()
        {
            Bid = new double[5];
            BidVol = new double[5];
            Ask = new double[5];
            AskVol = new double[5];
            UpdateTime = default(DateTime);
        }

        public override string ToString()
        {
            return string.Format("{0}, Mid={1}", UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"), Mid);    
        }
    }
    public class MarketData
    {
        public bool IsNormal { get; set; }
        public String Symbol = null;
        public MarketDepthData DepthData = null;
        public double FairValue = 0;

        public MarketData(String symbol)
        {
            Symbol = symbol;
            DepthData = new MarketDepthData();
        }       
        public override String ToString()
        {
            return string.Format("Symbol={0}, IsNormal={1}, FairValue={2}, DepthData={3}", 
                             Symbol, IsNormal, FairValue, DepthData);
        }
    }
}
