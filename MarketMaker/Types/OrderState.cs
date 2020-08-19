using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketMaker.Types
{
    public enum OrderState
    {
        Submitted,
        Partially_Filled,
        Cancelling,
        Fail,
        Filled,
        Partially_Cancelled,
        Cancelled
    }
}
