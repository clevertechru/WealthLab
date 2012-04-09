using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;

namespace Strategies.ActiveTrader
{
    public class VK_WL_Band : DataSeries
    {
        private DataSeries _ds;
        private int _IntervalSize;
        private int _IntervalCount;

        public VK_WL_Band(DataSeries ds, int IntervalSize, int IntervalCount, string description)
            : base(ds, description)
        {
            this._ds = ds;
            this._IntervalSize = IntervalSize;
            this._IntervalCount = IntervalCount;

            //base.FirstValidValue = period;

            double val = 0;
            for (int bar = IntervalSize * IntervalCount; bar < ds.Count; bar++)
            {
                val = 0;
                for (int sequenz = 0; sequenz < IntervalCount; sequenz++)
                {
                    val += (IntervalCount - 1 - sequenz + 1) * Lowest.Series(ds, IntervalSize)[bar - sequenz * IntervalSize];
                }
                val /= ((IntervalCount + 1) * (IntervalCount / 2));
                base[bar] = val;
            }
        }

        public static VK_WL_Band Series(DataSeries ds, int IntervalSize, int IntervalCount)
        {
            string description = string.Concat(new object[] { "VK WL Band(", ds.Description, ",", IntervalSize, ",", IntervalCount, ")" });

            if (ds.Cache.ContainsKey(description))
            {
                return (VK_WL_Band)ds.Cache[description];
            }

            VK_WL_Band _VK_WL_Band = new VK_WL_Band(ds, IntervalSize, IntervalCount, description);
            ds.Cache[description] = _VK_WL_Band;
            return _VK_WL_Band;
        }
    }

    public class VK_WH_Band : DataSeries
    {
        private DataSeries _ds;
        private int _IntervalSize;
        private int _IntervalCount;

        public VK_WH_Band(DataSeries ds, int IntervalSize, int IntervalCount, string description)
            : base(ds, description)
        {
            this._ds = ds;
            this._IntervalSize = IntervalSize;
            this._IntervalCount = IntervalCount;

            //base.FirstValidValue = period;

            double val = 0;
            for (int bar = IntervalSize * IntervalCount; bar < ds.Count; bar++)
            {
                val = 0;
                for (int sequenz = 0; sequenz < IntervalCount; sequenz++)
                {
                    val += (IntervalCount - 1 - sequenz + 1) * Highest.Series(ds, IntervalSize)[bar - sequenz * IntervalSize];
                }
                val /= ((IntervalCount + 1) * (IntervalCount / 2));
                base[bar] = val;
            }
        }

        public static VK_WH_Band Series(DataSeries ds, int IntervalSize, int IntervalCount)
        {
            string description = string.Concat(new object[] { "VK WH Band(", ds.Description, ",", IntervalSize, ",", IntervalCount, ")" });

            if (ds.Cache.ContainsKey(description))
            {
                return (VK_WH_Band)ds.Cache[description];
            }

            VK_WH_Band _VK_WH_Band = new VK_WH_Band(ds, IntervalSize, IntervalCount, description);
            ds.Cache[description] = _VK_WH_Band;
            return _VK_WH_Band;
        }
    }

    public class VK_SL_Band : DataSeries
    {
        private DataSeries _ds;
        private int _IntervalSize;
        private int _IntervalCount;

        public VK_SL_Band(DataSeries ds, int IntervalSize, int IntervalCount, string description)
            : base(ds, description)
        {
            this._ds = ds;
            this._IntervalSize = IntervalSize;
            this._IntervalCount = IntervalCount;

            //base.FirstValidValue = period;

            double val = 0;
            for (int bar = IntervalSize * IntervalCount; bar < ds.Count; bar++)
            {
                val = 0;
                for (int sequenz = 0; sequenz < IntervalCount; sequenz++)
                {
                    val += Lowest.Series(ds, IntervalSize)[bar - sequenz * IntervalSize] / IntervalCount;
                }
                base[bar] = val;
            }
        }

        public static VK_SL_Band Series(DataSeries ds, int IntervalSize, int IntervalCount)
        {
            string description = string.Concat(new object[] { "VK SL Band(", ds.Description, ",", IntervalSize, ",", IntervalCount, ")" });

            if (ds.Cache.ContainsKey(description))
            {
                return (VK_SL_Band)ds.Cache[description];
            }

            VK_SL_Band _VK_SL_Band = new VK_SL_Band(ds, IntervalSize, IntervalCount, description);
            ds.Cache[description] = _VK_SL_Band;
            return _VK_SL_Band;
        }
    }

    public class VK_SH_Band : DataSeries
    {
        private DataSeries _ds;
        private int _IntervalSize;
        private int _IntervalCount;

        public VK_SH_Band(DataSeries ds, int IntervalSize, int IntervalCount, string description)
            : base(ds, description)
        {
            this._ds = ds;
            this._IntervalSize = IntervalSize;
            this._IntervalCount = IntervalCount;

            //base.FirstValidValue = period;

            double val = 0;
            for (int bar = IntervalSize * IntervalCount; bar < ds.Count; bar++)
            {
                val = 0;
                for (int sequenz = 0; sequenz < IntervalCount; sequenz++)
                {
                    val += Highest.Series(ds, IntervalSize)[bar - sequenz * IntervalSize] / IntervalCount;
                }
                base[bar] = val;
            }
        }

        public static VK_SH_Band Series(DataSeries ds, int IntervalSize, int IntervalCount)
        {
            string description = string.Concat(new object[] { "VK SH Band(", ds.Description, ",", IntervalSize, ",", IntervalCount, ")" });

            if (ds.Cache.ContainsKey(description))
            {
                return (VK_SH_Band)ds.Cache[description];
            }

            VK_SH_Band _VK_SH_Band = new VK_SH_Band(ds, IntervalSize, IntervalCount, description);
            ds.Cache[description] = _VK_SH_Band;
            return _VK_SH_Band;
        }
    }


    public class ATMMar2005 : WealthScript
    {
        protected override void Execute()
        {
            VK_WH_Band VK_WH1 = VK_WH_Band.Series(High, 10, 10);
            PlotSeries(PricePane, VK_WH1, Color.DarkBlue, LineStyle.Solid, 2);
            VK_WL_Band VK_WL1 = VK_WL_Band.Series(Low, 10, 10);
            PlotSeries(PricePane, VK_WL1, Color.DarkBlue, LineStyle.Solid, 2);
            VK_SH_Band VK_SH1 = VK_SH_Band.Series(High, 10, 10);
            PlotSeries(PricePane, VK_SH1, Color.LightBlue, LineStyle.Solid, 2);

            for (int bar = 20; bar < Bars.Count; bar++)
            {
                if (!IsLastPositionActive)
                {
                    if (Close[bar] < VK_WL1[bar])
                        BuyAtMarket(bar + 1);
                }
                else
                {
                    if (Close[bar] > VK_WH1[bar])
                        SellAtMarket(bar + 1, LastPosition);
                }
            }
        }

    }
    public class ATMMar2005Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Volker Knapp"; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 01, 25); }
        }

        public override string Description
        {
            get
            {
                return
                "The VKW Bands System from the Trading System Lab article in the March 2005 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Strategy rules:</b></p><br>" +

                "<p><b>Enter long</b> on the next open when the market closes below the lower band.<br>" +
                "<b>Exit</b> on the next open when the market closes above the upper band.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("f896b1bb-1f35-4adf-8aba-5eaeec236be5");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 01, 25); }
        }

        public override string Name
        {
            get { { return "ATM 2005-03 | The VKW Bands system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMMar2005); }
        }
    }
}
