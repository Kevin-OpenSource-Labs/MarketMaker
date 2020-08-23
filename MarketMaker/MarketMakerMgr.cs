using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MarketMaker.Common;
using MarketMaker.Types;
using MarketMaker.Quote;
using MarketMaker.FairValue;
using MarketMaker.Exchange;
using MarketMaker.RiskControl;

namespace MarketMaker
{
    public class MarketMakerMgr : SingleTon<MarketMakerMgr>
    {
        public void Start()
        {
            m_setting = Setting.GetInstance();
            if(m_setting.Exchange == null || string.IsNullOrEmpty(m_setting.Parameter.Symbol))
            {
                Console.WriteLine("Does not find setting.xml file. Type any key to exist ...");
                Console.Read();
                return; 
            }
            m_mockTrade = m_setting.IsMockTrade;
            m_exchange = m_setting.Exchange;
            m_account = new StockAccount();
            m_fairValueMgr = FairValueMgr.GetInstance();            
            m_quoteMgr = QuoteMgr.GetInstance();
            m_hedgeMgr = AutoHedgeMgr.GetInstance();

            m_quoteParameter = m_setting.Parameter;        
            
            //quote logic
            m_quoteThread = new Thread(() => m_quoteMgr.Start());
		    m_quoteThread.Start();
				
		    //Update fair value logic
		    m_marketDataThread = new Thread(()=> m_fairValueMgr.Start(m_setting.Parameter.Symbol));              
		    m_marketDataThread.Start();		
				
		    //Hedge logic
		    m_hedgeThread = new Thread(()=>m_hedgeMgr.Start());               
		    m_hedgeThread.Start();            
	    }

        public Setting GetSetting()
        {
            return m_setting;
        }
        public FairValueMgr GetFairValueMgr()
        {
            return m_fairValueMgr;
        }
        public QuoteMgr GetQuoteMgr()
        {
            return m_quoteMgr;
        }
        public AutoHedgeMgr GetAutoHedgeMgr()
        {
            return m_hedgeMgr;
        }
        public ExchangeBase GetExchange()
        {
            return m_exchange;
        }
        public StockAccount GetAccount()
        {
            return m_account;
        }
        public QuoteParameter GetQuoteParameter()
        {
            return m_quoteParameter;
        }
        public Thread GetQuoteThread()
        {
            return m_quoteThread;
        }
        public bool Stop()
        {
            return m_stop;
        }
        public bool MockTrade()
        {
            return m_mockTrade;
        }

        private Setting m_setting = null;
        private ExchangeBase m_exchange = null;
        private StockAccount m_account = null;
        private FairValueMgr m_fairValueMgr = null;
        private QuoteMgr m_quoteMgr = null;
        private AutoHedgeMgr m_hedgeMgr = null;        
        private Thread m_marketDataThread = null;
        private Thread m_quoteThread = null;
        private Thread m_hedgeThread = null;
        private QuoteParameter m_quoteParameter = null;
        private bool m_stop = false;
        private bool m_mockTrade = false;
    }
}
