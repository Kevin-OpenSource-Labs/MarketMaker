using System;
using System.Collections.Generic;
using System.IO;
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

        public static string HttpGet(string url)
        {
            string result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json;charset=UTF-8";
            request.Method = "GET";
            request.Timeout = 10000;
            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd().ToLower();
            }
            return result;
        }
        public static string HttpPost(string url, string bodyJson)
        {
            string result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json;charset=UTF-8";
            request.Method = "POST";
            request.Timeout = 10000;
            if(!string.IsNullOrEmpty(bodyJson))
            {
                byte[] postData = Encoding.UTF8.GetBytes(bodyJson);
                Stream newStream = request.GetRequestStream();
                newStream.Write(postData, 0, postData.Length);
                newStream.Close();
            }
            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd().ToLower();
            }            
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
