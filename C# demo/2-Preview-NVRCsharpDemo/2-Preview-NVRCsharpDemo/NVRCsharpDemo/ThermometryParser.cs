using System;
using System.Linq;
using System.Xml.Linq;

namespace NVRCsharpDemo
{
    public class ThermometryData
    {
        public string Time { get; set; }
        public string Channel { get; set; }
        public string Temperature { get; set; }
        public string RawXml { get; set; }
    }

    public static class ThermometryParser
    {
        public static ThermometryData Parse(string xml)
        {
            ThermometryData data = new ThermometryData();
            data.RawXml = xml;
            data.Time = "";
            data.Channel = "";
            data.Temperature = "";

            try
            {
                XDocument doc = XDocument.Parse(xml);

                XNamespace ns1 = "http://www.isapi.org/ver20/XMLSchema";
                XNamespace ns2 = "http://www.hikvision.com/ver20/XMLSchema";

                data.Time =
                    doc.Descendants(ns1 + "time").FirstOrDefault() != null ? doc.Descendants(ns1 + "time").FirstOrDefault().Value :
                    doc.Descendants(ns2 + "time").FirstOrDefault() != null ? doc.Descendants(ns2 + "time").FirstOrDefault().Value :
                    "";

                data.Channel =
                    doc.Descendants(ns1 + "channel").FirstOrDefault() != null ? doc.Descendants(ns1 + "channel").FirstOrDefault().Value :
                    doc.Descendants(ns2 + "channel").FirstOrDefault() != null ? doc.Descendants(ns2 + "channel").FirstOrDefault().Value :
                    "";

                var tempProp1 = doc.Descendants(ns1 + "Property")
                    .FirstOrDefault(x =>
                        x.Element(ns1 + "description") != null &&
                        x.Element(ns1 + "description").Value == "tempValue");

                var tempProp2 = doc.Descendants(ns2 + "Property")
                    .FirstOrDefault(x =>
                        x.Element(ns2 + "description") != null &&
                        x.Element(ns2 + "description").Value == "tempValue");

                if (tempProp1 != null && tempProp1.Element(ns1 + "value") != null)
                {
                    data.Temperature = tempProp1.Element(ns1 + "value").Value;
                }
                else if (tempProp2 != null && tempProp2.Element(ns2 + "value") != null)
                {
                    data.Temperature = tempProp2.Element(ns2 + "value").Value;
                }
            }
            catch
            {
            }

            return data;
        }
    }
}