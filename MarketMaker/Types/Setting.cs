using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using MarketMaker.Common;
using MarketMaker.Exchange;

namespace MarketMaker.Types
{
    public class Setting : SingleTon<Setting>
    {
        public bool IsMockTrade { get; set; }
        public ExchangeBase Exchange { get; set; }
        public SortedDictionary<string, string> ExchangeSettingMap { get; set; }
        public QuoteParameter Parameter { get; set; }

        public Setting()
        {
            ExchangeSettingMap = new SortedDictionary<string, string>();
            Parameter = new QuoteParameter();

            LoadSetting();
        }

        public void LoadSetting()
        {
            string fileName = "Setting.xml";
            if (File.Exists(fileName))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);
                //Exchange
                XmlNode exchangeNode = doc.DocumentElement.SelectSingleNode("Exchange");
                if (null != exchangeNode)
                {
                    //Only support okdex currently
                    if (exchangeNode.Attributes["Type"].Value == "ExchangeOKDEX")
                    {
                        Exchange = new ExchangeOKDEX();
                    }
                    if(exchangeNode.Attributes.GetNamedItem("MockTrade") != null && exchangeNode.Attributes["MockTrade"].Value == "1")
                    {
                        IsMockTrade = true;
                    }
                    foreach (XmlAttribute attribute in exchangeNode.Attributes)
                    {
                        ExchangeSettingMap[attribute.Name] = attribute.Value;
                    }
                }
                //Quote parameter
                XmlNode paraNode = doc.DocumentElement.SelectSingleNode("Quote");
                if (null != paraNode)
                {
                    foreach (XmlAttribute attribute in paraNode.Attributes)
                    {
                        switch (attribute.Name)
                        {
                            case "Symbol":
                                Parameter.Symbol = attribute.Value.Replace("-", "_");
                                break;
                            case "BaseVolume":
                                Parameter.BaseVolume = double.Parse(attribute.Value);
                                break;
                            case "QuoteVolume":
                                Parameter.QuoteVolume = double.Parse(attribute.Value);
                                break;
                            case "QuoteVolumeRatioThreshold":
                                Parameter.QuoteVolumeRatioThreshold = double.Parse(attribute.Value);
                                break;
                            case "QuotePriceSpreadRatio":
                                Parameter.QuotePriceSpreadRatio = double.Parse(attribute.Value);
                                break;
                            case "QuotePriceRatioThreshold":
                                Parameter.QuotePriceRatioThreshold = double.Parse(attribute.Value);
                                break;
                            case "AutoHedgeVolumeThreshold":
                                Parameter.AutoHedgeVolumeThreshold = double.Parse(attribute.Value);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
        public override string ToString()
        {
            return string.Format("{0},{1}", string.Join("/", ExchangeSettingMap.Keys), Parameter.Symbol);
        }
    }
}
