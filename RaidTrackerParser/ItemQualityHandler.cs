using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using RaidTrackerParser.Data;

namespace RaidTrackerParser
{
    public class ItemQualityHandler
    {
        //https://www.wowhead.com/item=21323&xml
        public static int GetQuality(ItemTO item)
        {
            using var client = new WebClient();
            var contents = client.DownloadString($"https://www.wowhead.com/item={item.WOWID}&xml");

            var xDocument = XDocument.Parse(contents);

            var result = xDocument.Element("wowhead").Descendants();

            var itemQuality = result.Elements().FirstOrDefault(item => string.Equals(item.Name.ToString(), "quality"));

            return Convert.ToInt32(itemQuality.Attribute("id")?.Value);
        }
    }
}