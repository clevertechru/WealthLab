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
            //bars.Add(new DateTime(2012, 04, 07), 2, 5, 1, 3, 5);
            //bars.Add(new DateTime(2010, 09, 06), 3, 6, 2, 2, 6);
            return bars;
        }
        public string GetCompanyName(string symbol)
        {
            return symbol.ToUpper() + "CompanyName";
        }
    }
}