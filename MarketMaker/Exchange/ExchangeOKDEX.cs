using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Secp256k1Net;

using MarketMaker.Common;
using MarketMaker.Types;
using System.Diagnostics;

namespace MarketMaker.Exchange
{
    public class ExchangeOKDEX : ExchangeBase
    {        
        public override void UpdateMarketData(MarketData marketData)
        {
            if (null != marketData && !string.IsNullOrEmpty(marketData.Symbol))
            {
                string url = string.Format("{0}/v1/order/depthbook?product={1}", m_urlPrefix, marketData.Symbol.Replace("-", "_"));
                string json = HttpGet(url);
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = JsonHelper.DeserializeAnonymousType(json, new { code = -1,
                          data = new { asks = new List<SortedDictionary<string, double>>(), bids = new List<SortedDictionary<string, double>>() } });
                    if(obj.code == 0)
                    {
                        for(int i=0; i<5; i++)
                        {
                            if(obj.data.asks.Count>i)
                            {
                                marketData.DepthData.Ask[i] = obj.data.asks[i]["price"];
                                marketData.DepthData.AskVol[i] = obj.data.asks[i]["quantity"];
                            }
                            else
                            {
                                marketData.DepthData.Ask[i] = 0;
                                marketData.DepthData.AskVol[i] = 0;
                            }
                            if (obj.data.bids.Count > i)
                            {
                                marketData.DepthData.Bid[i] = obj.data.bids[i]["price"];
                                marketData.DepthData.BidVol[i] = obj.data.bids[i]["quantity"];
                            }
                            else
                            {
                                marketData.DepthData.Bid[i] = 0;
                                marketData.DepthData.BidVol[i] = 0;
                            }
                        }
                    }
                }
            }
        }

        public override void UpdateStockAccountInfo(StockAccount account)
        {
            if (m_marketMakerMgr.MockTrade() && account.StockPositionMap.Count>0)
            {
                return;
            }
            else
            {
                string url = string.Format("{0}/v1/accounts/{1}?show=all", m_urlPrefix, m_marketMakerMgr.GetSetting().ExchangeSettingMap["Address"]);
                string json = HttpGet(url);
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = JsonHelper.DeserializeAnonymousType(json, new
                    {
                        code = -1,
                        data = new { currencies = new List<SortedDictionary<string, string>>() }
                    });
                    if (obj.code == 0)
                    {
                        string[] items = m_marketMakerMgr.GetQuoteParameter().Symbol.Split("_".ToArray());
                        SortedSet<string> existingCoinSet = new SortedSet<string>();
                        foreach (SortedDictionary<string, string> coinDict in obj.data.currencies)
                        {
                            string coinName = coinDict["symbol"];
                            //Ignore other coins
                            if(!items.Contains(coinName))
                            {
                                continue;
                            }                            
                            double availVol = double.Parse(coinDict["available"]);
                            double lockedVol = double.Parse(coinDict["locked"]);
                            if (!account.StockPositionMap.ContainsKey(coinName))
                            {
                                account.StockPositionMap[coinName] = new StockPosition { CoinName = coinName };
                            }
                            account.StockPositionMap[coinName].AvailVolume = availVol;
                            account.StockPositionMap[coinName].TotalVolume = availVol + lockedVol;
                            if(items[0] == coinName)
                            {
                                account.StockPositionMap[coinName].BaseVolume = m_marketMakerMgr.GetQuoteParameter().BaseVolume;
                            }
                            existingCoinSet.Add(coinName);
                        }
                        //Remove non existing coins
                        List<string> coinNames = account.StockPositionMap.Keys.ToList();
                        foreach (string coinName in coinNames)
                        {
                            if (!existingCoinSet.Contains(coinName))
                            {
                                account.StockPositionMap.Remove(coinName);
                            }
                        }
                    }
                }
            }
        }
        public override bool PlaceOrder(Order order)
        {
            if (m_marketMakerMgr.MockTrade())
            {
                return m_mockExchange.PlaceOrder(order);
            }
            else
            {
                //Refer API https://documenter.getpostman.com/view/1112175/SzS5u6bE?version=latest#03709c4f-d620-4fef-a36a-f9cb97e909b8
                var msgObj = new []{ new { type = "okchain/order/MsgNew",
                    value = new {  order_items = new []{
                        new { price =  ((decimal)order.Price).ToString(),
                              product = order.Symbol.Replace("-", "_"),
                              quantity = ((decimal)order.Volume).ToString(),
                              side =  TradeDirectionUtil.IsBuy(order.Direction) ? "BUY" : "SELL" } } },
                              sender = m_marketMakerMgr.GetSetting().ExchangeSettingMap["Address"] } };

                string privateKey = m_marketMakerMgr.GetSetting().ExchangeSettingMap["PrivateKey"];
                string publicKey = GetPublicKey(privateKey);                
                var signatureObj = new[] { new { pub_key = new { type = "tendermint/PubKeySecp256k1", value = publicKey },
                    signature = GetSignature(JsonHelper.SerializeObject(msgObj), privateKey) } };
                var feeObj = new { amount = new[] { new { amount = "0.02000000", denom = "tokt" } }, gas = "200000" };
                var bodyObj = new { tx = new { msg = msgObj, signature = signatureObj, fee = feeObj, memo = "" }, mode = "block" };

                string url = string.Format("{0}/v1/txs?sync=true", m_urlPrefix);
                string json = HttpPost(url, JsonHelper.SerializeObject(bodyObj));
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = JsonHelper.DeserializeAnonymousType(json, new
                    {
                        logs = new List<SortedDictionary<string, string>>(),
                        events = new { attributes = new List<SortedDictionary<string, string>>() }
                    });
                }
            }
            return false;
        }
        public override void UpdateOrder(Order order)
        {
            if (m_marketMakerMgr.MockTrade())
            {
                m_mockExchange.UpdateOrder(order);
            }
            else
            {               
                string url = string.Format("{0}/v1/order/list/deals?address={1}&product={2}&side={3}&start={4}",
                m_urlPrefix, m_marketMakerMgr.GetSetting().ExchangeSettingMap["Address"], order.Symbol.Replace("-", "_"),
                TradeDirectionUtil.IsBuy(order.Direction) ? "BUY" : "SELL", TimeUtil.GetLocalUnixTimestamp(DateTime.Parse(order.OrderTime)));
                string json = HttpGet(url);
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = JsonHelper.DeserializeAnonymousType(json, new
                    {
                        
                    });                    
                }
            }
        }
        public override void CancelOrder(Order order)
        {
            if (m_marketMakerMgr.MockTrade())
            {
                m_mockExchange.CancelOrder(order);
            }
            else
            {
                //Refer API https://documenter.getpostman.com/view/1112175/SzS5u6bE?version=latest#80a454ec-276c-46b0-91f7-867cb1d5da06
                string url = string.Format("{0}/v1/txs?sync=true", m_urlPrefix);
                //to do, create data-raw... post
                string json = HttpGet(url);
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = JsonHelper.DeserializeAnonymousType(json, new
                    {

                    });                    
                }
            }
        }
        public override void CancelOrders(List<Order> orders)
        {
            if (m_marketMakerMgr.MockTrade())
            {
                m_mockExchange.CancelOrders(orders);
            }
            else
            {
                foreach (Order order in orders)
                {
                    CancelOrder(order);
                }
            }
        }

        public string GetSignature(string strToSign, string privateKey)
        {
            var secp256k1 = new Secp256k1();
            var messageBytes = Encoding.UTF8.GetBytes(strToSign);
            var messageHash = System.Security.Cryptography.SHA256.Create().ComputeHash(messageBytes);
            var signature = new byte[64];
            if(secp256k1.Sign(signature, messageHash, HexStringToByteArray(privateKey)))
            {
                return ByteArrayToHexString(signature);
            }
            else
            {
                return null;
            }
        }
        public string GetPublicKey(string privateKey)
        {
            var secp256k1 = new Secp256k1();
            var publicKey = new byte[64];
            if (secp256k1.PublicKeyCreate(publicKey, HexStringToByteArray(privateKey)))
            {
                return ByteArrayToHexString(publicKey);
            }
            else
            {
                return null;
            }
        }
        private byte[] HexStringToByteArray(string str)
        {
            str = str.Replace(" ", "");
            byte[] buffer = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }
            return buffer;
        }
        private string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            }            
            return sb.ToString();
        }

        private string m_urlPrefix = "https://www.okex.com/okchain";        
        private ExchangeBase m_mockExchange = new ExchangeMock();
    }
}
