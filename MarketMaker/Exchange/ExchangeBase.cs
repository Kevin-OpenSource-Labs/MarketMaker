﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketMaker.Quote;
using MarketMaker.Types;

namespace MarketMaker.Exchange
{
    public abstract class ExchangeBase
    {        
        public virtual void UpdateMarketData(MarketData marketData)
        {
            throw new Exception("Not implement method");
        }
        public virtual void UpdateStockAccountInfo(StockAccount account)
        {
            throw new Exception("Not implement method");
        }
        public virtual bool PlaceOrder(Order order)
        {
            throw new Exception("Not implement method");
        }
        public virtual void CancelOrder(Order order)
        {
            throw new Exception("Not implement method");
        }
        public virtual void UpdateOrder(Order order)
        {
            throw new Exception("Not implement method");
        }
        public virtual void CancelOrders(List<Order> orders)
        {
            throw new Exception("Not implement method");
        }

        public static string HttpGet(String httpUrl)
        {            
            String result = null;
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/json");
            result = webClient.DownloadString(httpUrl);
            return result;
        }
        protected void NotifyTrade(TradeRecord record)
        {
            //update account symbol volume
            if (record.Direction == TradeDirection.Buy)
            {
                m_marketMakerMgr.GetAccount().adjustStockVolume(record.Symbol, record.TradedVolume);
            }
            else
            {
                m_marketMakerMgr.GetAccount().adjustStockVolume(record.Symbol, -record.TradedVolume);
            }

            //notify quote manager to adjust pending orders if necessary
            AutoResetEvent quoteEventObj = m_marketMakerMgr.GetQuoteMgr().GetQuoteEventObject();            
            quoteEventObj.Set();
        }
        protected string ConvertFromDEXSymbol(string symbol)
        {
            return symbol.ToLower().Replace("tbtc", "btc").Replace("tusdk", "usdt");
        }
        protected static MarketMakerMgr m_marketMakerMgr = MarketMakerMgr.GetInstance();        
    }
}
