using System;
using System.Collections.Generic;
using System.Text;
using WealthLab.Extensions.Attribute;

// To satisfy for the Extension Manager
[assembly: ExtensionInfo(
    ExtensionType.Strategy,
    "Strategies.ActiveTrader",
    "ActiveTrader Strategy Pack",
    "A collection of Strategies published in ActiveTrader and Futures & Options Trader.",
    "2010.06",
    "MS123",
    "Strategies.ActiveTrader.Resources.AT.png",
    ExtensionLicence.Freeware,
    new string[] { "Strategies.ActiveTrader.dll" },
    MinProVersion = "5.6",
    MinDeveloperVersion = "5.6",
    PublisherUrl = "http://www2.wealth-lab.com/WL5WIKI/ActiveTraderMain.ashx")
    ]

namespace Strategies.ActiveTrader.Resources
{
    class EM
    {
    }
}
