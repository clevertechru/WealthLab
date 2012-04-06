using System;
using System.Collections.Generic;
using System.Text;

using WealthLab;

namespace OpenWealth.WLProvider
{
    public class Zaglushka
    {
        public Bars RequestData(DataSource ds, string symbol, DateTime startDate, DateTime endDate, int maxBars, bool includePartialBar)
        {
            Bars bars = new Bars(symbol, ds.Scale, ds.BarInterval);
            bars.Add(new DateTime(2012, 04, 06, 08, 45, 00), 292.00, 292.00, 291.00, 291.80, 34);
            return bars;
        }
        public string GetCompanyName(string symbol)
        {
            return symbol.ToUpper() + "CompanyName";
        }
    }
}
