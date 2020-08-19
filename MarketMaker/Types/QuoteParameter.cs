using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Types
{
    public class QuoteParameter
    {
        public double QuoteVolume { get; set; }
        public double QuoteVolumeRatioThreshold { get; set; }

        public double QuotePriceSpreadRatio { get; set; }
        public double QuotePriceRatioThreshold { get; set; }
        public double AutoHedgeVolumeThreshold { get; set; }
    }
}
