using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;
using WealthLab.Rules;
using WealthLab.Rules.Candlesticks; 

namespace Strategies.ActiveTrader
{
    public class ATMJan2009_1 : WealthScript
    {
        protected override void Execute()
        {
            DataSeries dma3_3 = SMA.Series(Close, 3) >> 3;
            PlotSeries(PricePane, dma3_3, Color.Red, LineStyle.Solid, 1);

            bool maxu1 = false;
            bool maxo1 = false;
            bool maxu2 = false;
            int maxu1bar = -1;
            int maxobar = -1;
            int maxu2bar = -1;

            int bThrustingMarketAction = -1;

            SetBarColors(Color.Silver, Color.Silver);
            for (int bar = 30; bar < Bars.Count; bar++)
            {

                if (ThrustingMarketAction(bar))
                {
                    SetBackgroundColor(bar, Color.FromArgb(10, Color.Red));
                    bThrustingMarketAction = bar;
                }

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (CrossOver(bar, SMA.Series(Close, 13), SMA.Series(Close, 26)))
                        CoverAtMarket(bar + 1, p, "Cover @ XO");
                }
                else
                {
                    Position p = LastPosition;

                    if (!maxu1)
                    {
                        // 1st crossunder
                        if (maxu(bar, dma3_3))
                            // "thrusting market action"
                            if (bar < bThrustingMarketAction + 10)
                            {
                                maxu1 = true;
                                maxu1bar = bar;
                                AnnotateBar("1st XU", bar, false, Color.Orange);
                                SetBarColor(bar, Color.Orange);
                            }
                    }
                    else if (maxu1)
                    {
                        if (!maxo1)
                        {
                            // crossover in the middle
                            if (maxo(bar, dma3_3))
                            {
                                maxo1 = true;
                                maxobar = bar;
                                AnnotateBar("XO", bar, false, Color.Blue);
                                SetBarColor(bar, Color.Blue);
                            }
                        }
                        else if (maxo1)
                        {
                            // 2nd crossunder
                            if (!maxu2)
                            {
                                if (maxu(bar, dma3_3) == true)
                                {
                                    maxu2 = true;
                                    maxu2bar = bar;
                                    AnnotateBar("2nd XU", bar, false, Color.Red);
                                    SetBarColor(bar, Color.Red);
                                }
                            }
                            else if (maxu2)
                            {
                                maxu1 = false;
                                maxo1 = false;
                                maxu2 = false;

                                if (maxu2bar <= maxu1bar + 10)
                                    ShortAtStop(bar + 1, Low[maxu2bar], "Double RePo");
                            }
                        }
                    }
                }
            }
        }

        /* Thrusting Market Action (Eugene's take) */
        public bool ThrustingMarketAction(int bar)
        {
            bool setupValid = (High[bar] > (High[bar - 10] + 4 * ATR.Series(Bars, 10)[bar]));
            return setupValid;
        }

        /* MA Crossover */
        public bool maxo(int bar)
        {
            bool setupValid = CrossOver(bar, Close, SMA.Series(Close, 10));
            return setupValid;
        }

        /* MA Crossover overloaded */
        public bool maxo(int bar, DataSeries series)
        {
            bool setupValid = CrossOver(bar, Close, series);
            return setupValid;
        }

        /* MA Crossunder */
        public bool maxu(int bar)
        {
            bool setupValid = CrossUnder(bar, Close, SMA.Series(Close, 10));
            return setupValid;
        }

        /* MA Crossunder overloaded */
        public bool maxu(int bar, DataSeries series)
        {
            bool setupValid = CrossUnder(bar, Close, series);
            return setupValid;
        }
    }

    public class ATMJan2009_1Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 11, 22); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the January, 2009 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 1 strategy rules (straight Double RePo):</b></p><br>" +

                "<p><b>1. Setup:</b><br>" +
                "a) Detect thrusting market action when today's high is more than four times the 10-day ATR above the high 10 days ago.<br>" +
                "b) Within 10 days of this thrust, identify when price closes below a three-day displaced SMA.<br>" +
                "c) Wait for the closing price to cross back above the three-day displaced SMA.<br></p>" +

                "<p><b>2. Entry (futures):</b> When price closes below the three-period displaced SMA a second time, enter short tomorrow on a stop at today's low.<br>" +
                "<b>Note:</b> The entire setup and trade signal must occur within a 10-day span.<br>" +
                "<b>3. Exit:</b> Cover short at the market tomorrow when the 13-day SMA crosses above the 26-day SMA.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("289437f9-dec8-488e-881d-2637c2d5df66");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 11, 22); }
        }

        public override string Name
        {
            get { { return "ATM 2009-01 | Double RePo (futures)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJan2009_1); }
        }
    }

    public class ATMJan2009_2 : WealthScript
    {
        protected override void Execute()
        {
            DataSeries dma3_3 = SMA.Series(Close, 3) >> 3;
            PlotSeries(PricePane, dma3_3, Color.Red, LineStyle.Solid, 1);

            bool maxu1 = false;
            bool maxo1 = false;
            bool maxu2 = false;
            int maxu1bar = -1;
            int maxobar = -1;
            int maxu2bar = -1;

            int bThrustingMarketAction = -1;

            SetBarColors(Color.Silver, Color.Silver);
            for (int bar = 30; bar < Bars.Count; bar++)
            {

                if (ThrustingMarketAction(bar))
                {
                    SetBackgroundColor(bar, Color.FromArgb(10, Color.Red));
                    bThrustingMarketAction = bar;
                }

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (CrossUnder(bar, SMA.Series(Close, 13), SMA.Series(Close, 26)))
                        SellAtMarket(bar + 1, p, "Crossover");
                }
                else
                {
                    Position p = LastPosition;

                    if (!maxu1)
                    {
                        // 1st crossunder
                        if (maxu(bar, dma3_3))
                            // "thrusting market action"
                            if (bar < bThrustingMarketAction + 5)
                            {
                                maxu1 = true;
                                maxu1bar = bar;
                                AnnotateBar("1st XU", bar, false, Color.Orange);
                                SetBarColor(bar, Color.Orange);
                            }
                    }
                    else if (maxu1)
                    {
                        if (!maxo1)
                        {
                            // crossover in the middle
                            if (maxo(bar, dma3_3))
                            {
                                maxo1 = true;
                                maxobar = bar;
                                AnnotateBar("XO", bar, false, Color.Blue);
                                SetBarColor(bar, Color.Blue);
                            }
                        }
                        else if (maxo1)
                        {
                            // 2nd crossunder
                            if (!maxu2)
                            {
                                if (maxu(bar, dma3_3) == true)
                                {
                                    maxu2 = true;
                                    maxu2bar = bar;
                                    AnnotateBar("2nd XU", bar, false, Color.Red);
                                    SetBarColor(bar, Color.Red);
                                }
                            }
                            else if (maxu2)
                            {
                                maxu1 = false;
                                maxo1 = false;
                                maxu2 = false;

                                if (maxu2bar <= maxu1bar + 10)
                                    if (BuyAtStop(bar + 1, High[maxu2bar]) != null)
                                        LastPosition.Priority = -RSI.Series(Close, 7)[bar];
                            }
                        }
                    }
                }
            }
        }

        /* Thrusting Market Action (my version) */
        public bool ThrustingMarketAction(int bar)
        {
            bool setupValid = (High[bar] > (High[bar - 10] + 4 * ATR.Series(Bars, 10)[bar]));
            return setupValid;
        }

        /* MA Crossover */
        public bool maxo(int bar)
        {
            bool setupValid = CrossOver(bar, Close, SMA.Series(Close, 10));
            return setupValid;
        }

        /* MA Crossover overloaded */
        public bool maxo(int bar, DataSeries series)
        {
            bool setupValid = CrossOver(bar, Close, series);
            return setupValid;
        }

        /* MA Crossunder */
        public bool maxu(int bar)
        {
            bool setupValid = CrossUnder(bar, Close, SMA.Series(Close, 10));
            return setupValid;
        }

        /* MA Crossunder overloaded */
        public bool maxu(int bar, DataSeries series)
        {
            bool setupValid = CrossUnder(bar, Close, series);
            return setupValid;
        }
    }

    public class ATMJan2009_2Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 11, 22); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the January, 2009 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 2 strategy rules (fading the Double RePo):</b></p><br>" +

                "<p><b>1. Setup:</b><br>" +
                "a) Detect thrusting market action when today's high is more than four times the 10-day ATR above the high 10 days ago.<br>" +
                "b) Within 10 days of this thrust, identify when price closes below a three-day displaced SMA.<br>" +
                "c) Wait for the closing price to cross back above the three-day displaced SMA.<br></p>" +

                "<p><b>2. Long Entry (stocks):</b> When price closes below the three-period displaced SMA a second time, enter long tomorrow on a stop at today's high.<br>" +
                "<b>Note:</b> The entire setup and trade signal must occur within a 10-day span.<br>" +
                "<b>3. Long Exit:</b> Sell at the market tomorrow when the 13-day SMA crosses below the 26-day SMA.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("63168b78-eaa6-408c-ab7e-1c10adaad22c");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 11, 22); }
        }

        public override string Name
        {
            get { { return "ATM 2009-01 | Double RePo Failure (stocks)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJan2009_2); }
        }
    }

    public class ATMFeb2009 : WealthScript
    {
        protected override void Execute()
        {
            BBandUpper bbU = BBandUpper.Series(Close, 20, 2);
            BBandLower bbL = BBandLower.Series(Close, 20, 2);
            KeltnerUpper kU = KeltnerUpper.Series(Bars, 20, 20);
            KeltnerLower kL = KeltnerLower.Series(Bars, 20, 20);

            PlotSeriesDualFillBand(PricePane, BBandUpper.Series(Close, 20, 2),
                BBandLower.Series(Close, 20, 2), Color.Transparent, Color.Blue,
                Color.Blue, LineStyle.Solid, 1);
            PlotSeriesDualFillBand(PricePane, KeltnerUpper.Series(Bars, 20, 20),
                KeltnerLower.Series(Bars, 20, 20), Color.Transparent, Color.Black,
                Color.Black, LineStyle.Solid, 2);

            Momentum mi = Momentum.Series(Close, 12);
            DataSeries smoothedMomentum = EMA.Series(mi, 5, EMACalculation.Modern);
            smoothedMomentum.Description = "Smoothed Momentum Oscillator";
            ChartPane miPane = CreatePane(30, false, true);
            PlotSeries(miPane, smoothedMomentum, Color.DarkRed, LineStyle.Histogram, 2);
            DrawHorzLine(miPane, 0, Color.Red, LineStyle.Dashed, 1);

            for (int bar = 20; bar < Bars.Count; bar++)
            {
                if (smoothedMomentum[bar] >= 0)
                    SetSeriesBarColor(bar, smoothedMomentum, Color.DarkGreen);
                else
                    SetSeriesBarColor(bar, smoothedMomentum, Color.DarkRed);
            }

            HideVolume();
            bool sq = false; int sqBar = -1;

            for (int bar = 20; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;

                    if (p.PositionType == PositionType.Long)
                    {
                        if (CumDown.Series(smoothedMomentum, 1)[bar] >= 1)
                            SellAtMarket(bar + 1, p, "MomExit LX");
                    }
                    else
                    {
                        if (CumUp.Series(smoothedMomentum, 1)[bar] >= 1)
                            CoverAtMarket(bar + 1, p, "MomExit SX");
                    }
                }
                else
                {
                    /* Squeeze */
                    if (sq == false)
                    {
                        if ((bbU[bar] < kU[bar]) && (bbL[bar] > kL[bar]))
                        {
                            sq = true;
                            sqBar = bar;
                        }
                    }

                    if (sq)
                    {
                        // Color the bars when Squeeze holds true
                        SetBarColor(bar, Color.Blue);

                        // Squeeze release
                        if ((bbU[bar] > kU[bar]) && (bbL[bar] < kL[bar]))
                        {
                            // The momentum condition
                            if (smoothedMomentum[bar] > 0)
                            {
                                if (BuyAtMarket(bar + 1,
                                    "Span = " + (bar - sqBar + 1).ToString()) != null)
                                {
                                    sq = false;
                                    LastActivePosition.Priority = smoothedMomentum[bar];
                                }
                            }
                            else
                                if (smoothedMomentum[bar] <= 0)
                                {
                                    if (ShortAtMarket(bar + 1,
                                        "Span = " + (bar - sqBar + 1).ToString()) != null)
                                    {
                                        sq = false;
                                        LastActivePosition.Priority = -smoothedMomentum[bar];
                                    }
                                }
                        }
                    }
                }
            }

        }

    }
    
    public class ATMFeb2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 01, 12); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the February, 2009 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Strategy rules (The double-band squeeze system):</b></p><br>" +

                "<p><b>Setup:</b> When the Bollinger Bands have moved inside the Keltner channels, prepare for a trade.</p>" +

                "<p><b>1. Long entry:</b> When the momentum oscillator decreases (i.e., fails to create a higher value), close the position at the next bar's open.<br>" +
                "<b>2. Long exit:</b> Cover short at the market tomorrow when the 13-day SMA crosses above the 26-day SMA.<br>" +
                "<b>3. Short entry:</b> When the Bollinger Bands move back out of the Keltner channels and the momentum oscillator is negative (below zero), enter short at the next bar's open.<br>" +
                "<b>4. Short exit:</b> When the momentum oscillator increases (i.e., fails to create a lower value), close the position at the next bar's open.</p>";

            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("7a58444e-5402-4f0f-8b09-ec4a457ae8f3");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 01, 12); }
        }

        public override string Name
        {
            get { { return "ATM 2009-02 |The double-band squeeze system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMFeb2009); }
        }
    }

    public class MACDEx_Histogram : DataSeries
    {
        private Bars bars;
        private DataSeries ds;
        private int period1;
        private int period2;

        public MACDEx_Histogram(Bars bars, DataSeries ds, int period1, int period2, string description)
            : base(bars, description)
        {
            this.bars = bars;
            this.ds = ds;
            this.period1 = period1;
            this.period2 = period2;
            base.FirstValidValue = Math.Max(period1, period2) * 3;
            MACDEx macdex = new MACDEx(bars, ds, period1, period2, description);
            MACDEx_Signal sigLine = new MACDEx_Signal(bars, ds, period1, period2, description);
            DataSeries macdHist = macdex - sigLine;

            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                base[bar] = macdHist[bar];
            }
        }

        public static MACDEx_Histogram Series(Bars bars, DataSeries ds, int period1, int period2)
        {
            string description = string.Concat(new object[] { "MACDEx Histogram(", bars.Symbol, ",", ds.Description, ",", period1, ",", period2, ")" });
            //string description = string.Concat(new object[] { "MACDEx Histogram(", bars.ToString(), ",", ds.Description, ",", period1, ",", period2, ")" });

            if (bars.Cache.ContainsKey(description))
            {
                return (MACDEx_Histogram)bars.Cache[description];
            }

            MACDEx_Histogram _MACDEx_Histogram = new MACDEx_Histogram(bars, ds, period1, period2, description);
            bars.Cache[description] = _MACDEx_Histogram;
            return _MACDEx_Histogram;
        }
    }

    public class MACDEx : DataSeries
    {
        private Bars bars;
        private DataSeries ds;
        private int period1;
        private int period2;

        public MACDEx(Bars bars, DataSeries ds, int period1, int period2, string description)
            : base(bars, description)
        {
            this.bars = bars;
            this.ds = ds;
            this.period1 = period1;
            this.period2 = period2;
            base.FirstValidValue = Math.Max(period1, period2) * 3;
            EMACalculation m = EMACalculation.Modern;
            EMA ema1 = EMA.Series(ds, period1, m);
            EMA ema2 = EMA.Series(ds, period2, m);

            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                base[bar] = ema1[bar] - ema2[bar];
            }
        }

        public static MACDEx Series(Bars bars, DataSeries ds, int period1, int period2)
        {
            string description = string.Concat(new object[] { "MACDEx(", bars.Symbol, ",", ds.Description, ",", period1, ",", period2, ")" });
            //string description = string.Concat(new object[] { "MACDEx(", bars.ToString(), ",", ds.Description, ",", period1, ",", period2, ")" });

            if (bars.Cache.ContainsKey(description))
            {
                return (MACDEx)bars.Cache[description];
            }

            MACDEx _MACDEx = new MACDEx(bars, ds, period1, period2, description);
            bars.Cache[description] = _MACDEx;
            return _MACDEx;
        }
    }

    public class MACDEx_Signal : DataSeries
    {
        private Bars bars;
        private DataSeries ds;
        private int period1;
        private int period2;

        public MACDEx_Signal(Bars bars, DataSeries ds, int period1, int period2, string description)
            : base(bars, description)
        {
            this.bars = bars;
            this.ds = ds;
            this.period1 = period1;
            this.period2 = period2;
            base.FirstValidValue = Math.Max(9, Math.Max(period1, period2)) * 3;
            EMACalculation m = EMACalculation.Modern;
            MACDEx macdex = new MACDEx(bars, ds, period1, period2, description);
            EMA ema = EMA.Series(macdex, 9, m);

            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                base[bar] = ema[bar];
            }
        }

        public static MACDEx_Signal Series(Bars bars, DataSeries ds, int period1, int period2)
        {
            string description = string.Concat(new object[] { "MACDEx Signal(", bars.Symbol, ",", ds.Description, ",", period1, ",", period2, ")" });
            //string description = string.Concat(new object[] { "MACDEx Signal(", bars.ToString(), ",", ds.Description, ",", period1, ",", period2, ")" });

            if (bars.Cache.ContainsKey(description))
            {
                return (MACDEx_Signal)bars.Cache[description];
            }

            MACDEx_Signal _MACDEx_Signal = new MACDEx_Signal(bars, ds, period1, period2, description);
            bars.Cache[description] = _MACDEx_Signal;
            return _MACDEx_Signal;
        }
    }

    public class ATMApr2009_1 : WealthScript
    {
        private StrategyParameter paramEMA;
        private StrategyParameter paramSwitch;

        public ATMApr2009_1()
        {
            paramEMA = CreateParameter("EMA", 13, 6, 18, 1);
            paramSwitch = CreateParameter("Filter Off/On", 1, 0, 1, 1);
        }

        protected override void Execute()
        {
            EMACalculation modern = EMACalculation.Modern;
            bool ImpulseBuy, ImpulseShort;
            int perEma = paramEMA.ValueInt;
            int FilterOn = paramSwitch.ValueInt;

            DataSeries maFast = SMA.Series(Close, 20);
            DataSeries maSlow = SMA.Series(Close, 60);
            DataSeries ema = EMA.Series(Close, perEma, modern); ema.Description = "EMA";
            //DataSeries macd = MACD.Series( Close ); macd.Description = "MACD";
            DataSeries macd = MACDEx_Histogram.Series(Bars, Close, 12, 26); macd.Description = "MACD-H";

            // Plotting
            PlotSeries(PricePane, maFast, Color.Red, LineStyle.Solid, 2);
            PlotSeries(PricePane, maSlow, Color.Green, LineStyle.Solid, 2);
            PlotSeries(PricePane, ema, Color.DarkBlue, LineStyle.Solid, 2);
            ChartPane PaneMACD = CreatePane(30, false, true);
            PlotSeries(PaneMACD, macd, Color.Purple, LineStyle.Histogram, 2);
            SetBarColors(Color.Silver, Color.Silver);
            HideVolume();

            for (int bar = maSlow.FirstValidValue + 1; bar < Bars.Count; bar++)
            {
                // Reset Impulse on each bar
                ImpulseShort = false;
                ImpulseBuy = false;

                // Define Impulse condition
                ImpulseBuy = (ema[bar] > ema[bar - 1]) & (macd[bar] > macd[bar - 1]);
                ImpulseShort = (ema[bar] < ema[bar - 1]) & (macd[bar] < macd[bar - 1]);

                if (ImpulseBuy) SetBarColor(bar, Color.Green);
                if (ImpulseShort) SetBarColor(bar, Color.Red);

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (LastPosition.PositionType == PositionType.Long)	// Long exit
                    {
                        if (FilterOn == 1)
                        {
                            if (CrossUnder(bar, maFast, maSlow) & !ImpulseBuy)
                                SellAtMarket(bar + 1, p);
                        }
                        else
                            if (CrossUnder(bar, maFast, maSlow))
                                SellAtMarket(bar + 1, p);
                    }
                    else 													// Short exit
                    {
                        if (FilterOn == 1)
                        {
                            if (CrossOver(bar, maFast, maSlow) & !ImpulseShort)
                                CoverAtMarket(bar + 1, p);
                        }
                        else
                            if (CrossOver(bar, maFast, maSlow))
                                CoverAtMarket(bar + 1, p);
                    }
                }
                else
                {
                    if (CrossOver(bar, maFast, maSlow))						// Long entry
                    {
                        if (FilterOn == 1) 	// Impulse filter enabled
                        {
                            if (ImpulseBuy)
                                if (BuyAtMarket(bar + 1) != null)
                                    LastPosition.Priority = RSI.Series(Close, 7)[bar];
                        }
                        else 			// Impulse filter disabled
                        {
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = RSI.Series(Close, 7)[bar];
                        }
                    }
                    else if (CrossUnder(bar, maFast, maSlow))				// Short entry
                    {
                        if (FilterOn == 1)	// Impulse filter enabled
                        {
                            if (ImpulseShort)
                                if (ShortAtMarket(bar + 1) != null)
                                    LastPosition.Priority = -RSI.Series(Close, 7)[bar];
                        }
                        else 			// Impulse filter disabled
                        {
                            if (ShortAtMarket(bar + 1) != null)
                                LastPosition.Priority = -RSI.Series(Close, 7)[bar];
                        }
                    }
                }
            }
        }
    }

    public class ATMApr2009_1Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 03, 08); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the April 2009 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 2 strategy rules (crossover system filtered by Impulse system on daily data):</b></p><br>" +
                "<p>The Impulse system filter states are green (long) and red (short):</p><br>" +
                "<p>Green: The 13-day EMA and 12-26-9 MACD histogram are both higher than yesterday's values.<br>" +
                "Red: The 13-day EMA and 12-26-9 MACD histogram are both lower than yesterday's values.</p>" +

                "<p>1. Buy next day at open when the 20-day SMA crosses above the 60-day SMA and the Impulse filter is green.<br>" +
                "2. Sell short next day at open when the 20-day SMA crosses below the 60-day SMA and the Impulse filter is red.<br>" +
                "3. Exit long next day at open when the 20-day SMA crosses below the 60-day SMA and the Impulse filter is not green.<br>" +
                "4. Cover short next day at open when the 20-day SMA crosses above the 60-day SMA and the Impulse filter is not red.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("74ec0c12-86b7-47f8-9ffd-77f794462d18");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 03, 08); }
        }

        public override string Name
        {
            get { { return "ATM 2009-04 | Impulse Filter (Daily)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMApr2009_1); }
        }
    }

    public class ATMApr2009_2 : WealthScript
    {
        private StrategyParameter paramWeekly;
        private StrategyParameter paramSwitch;

        public ATMApr2009_2()
        {
            paramWeekly = CreateParameter("Weekly EMA", 26, 12, 36, 2);
            paramSwitch = CreateParameter("Impulse Off/On", 1, 0, 1, 1);
        }

        protected override void Execute()
        {
            EMACalculation modern = EMACalculation.Modern;
            bool ImpulseBuy, ImpulseShort;
            int per = paramWeekly.ValueInt;
            int FilterOn = paramSwitch.ValueInt;

            // For simplicity, allow operation on minute and daily scales only
            if (!(Bars.Scale == BarScale.Daily) & !(Bars.Scale == BarScale.Minute)) return;

            // The GetWeeklyBar workaround
            SetScaleWeekly();
            DataSeries CompBar = Close * 0;
            CompBar.Description = "Compressed Bar Number";
            for (int bar = 0; bar < Bars.Count; bar++)	// load the bar numbers
                CompBar[bar] = bar;

            DataSeries wEMA = EMA.Series(Bars.Close, per, modern); wEMA.Description = "Weekly EMA";
            //DataSeries wMACD = MACD.Series( Close ); wMACD.Description = "Weekly MACD";
            DataSeries wMACD = MACDEx_Histogram.Series(Bars, Close, 12, 26); wMACD.Description = "MACD-H";
            RestoreScale();

            // Synch the bar numbers with the base time frame (here's your "function")
            DataSeries w_EMA = Synchronize(wEMA);
            DataSeries w_MACD = Synchronize(wMACD);
            DataSeries GetWeeklyBar = Synchronize(CompBar);

            DataSeries maFast = SMA.Series(Close, 20);
            DataSeries maSlow = SMA.Series(Close, 60);

            // Plotting
            PlotSeries(PricePane, w_EMA, Color.Blue, LineStyle.Solid, 2);
            PlotSeries(PricePane, maFast, Color.Red, LineStyle.Solid, 1);
            PlotSeries(PricePane, maSlow, Color.Green, LineStyle.Solid, 1);
            ChartPane PaneMACD = CreatePane(30, false, true);
            PlotSeries(PaneMACD, w_MACD, Color.Blue, LineStyle.Histogram, 2);
            HideVolume();

            for (int bar = maSlow.FirstValidValue + 1; bar < Bars.Count; bar++)
            {
                int wBar = (int)GetWeeklyBar[bar];

                // Reset Impulse on each bar
                ImpulseShort = false;
                ImpulseBuy = false;

                // Define Impulse condition
                ImpulseBuy = (wEMA[wBar] > wEMA[wBar - 1]) & (wMACD[wBar] > wMACD[wBar - 1]);
                ImpulseShort = (wEMA[wBar] < wEMA[wBar - 1]) & (wMACD[wBar] < wMACD[wBar - 1]);

                if (ImpulseBuy) SetBarColor(bar, Color.LightGreen);
                if (ImpulseShort) SetBarColor(bar, Color.Red);

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (LastPosition.PositionType == PositionType.Long)	// Long exit
                    {
                        if (FilterOn == 1)
                        {
                            if (CrossUnder(bar, maFast, maSlow) & !ImpulseBuy)
                                SellAtMarket(bar + 1, p);
                        }
                        else
                            if (CrossUnder(bar, maFast, maSlow))
                                SellAtMarket(bar + 1, p);
                    }
                    else 													// Short exit
                    {
                        if (FilterOn == 1)
                        {
                            if (CrossOver(bar, maFast, maSlow) & !ImpulseShort)
                                CoverAtMarket(bar + 1, p);
                        }
                        else
                            if (CrossOver(bar, maFast, maSlow))
                                CoverAtMarket(bar + 1, p);
                    }
                }
                else
                {
                    if (CrossOver(bar, maFast, maSlow))						// Long entry
                    {
                        if (FilterOn == 1) 							// Impulse filter engaged
                        {
                            if (ImpulseBuy)
                                if (BuyAtMarket(bar + 1) != null)
                                    LastPosition.Priority = RSI.Series(Close, 7)[bar];
                        }
                        else 									// Impulse filter disabled
                        {
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = RSI.Series(Close, 7)[bar];
                        }
                    }
                    else if (CrossUnder(bar, maFast, maSlow))				// Short entry
                    {
                        if (FilterOn == 1)							 // Impulse filter engaged
                        {
                            if (ImpulseShort)
                                if (ShortAtMarket(bar + 1) != null)
                                    LastPosition.Priority = -RSI.Series(Close, 7)[bar];
                        }
                        else 									// Impulse filter disabled
                        {
                            if (ShortAtMarket(bar + 1) != null)
                                LastPosition.Priority = -RSI.Series(Close, 7)[bar];
                        }
                    }
                }
            }
        }
    }

    public class ATMApr2009_2Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 03, 08); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the April 2009 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 3 strategy rules (crossover system filtered by Impulse system on weekly data):</b></p><br>" +
                "<p>Impulse filter states:</p><br>" +
                "<p>Green: The 13-day EMA and 12-26-9 MACD histogram are both higher than last week's values.<br>" +
                "Red: The 13-day EMA and 12-26-9 MACD histogram are both lower than last week's values.</p>" +

                "<p>1. Buy at open next day when the 20-day SMA crosses above the 60-day SMA and the weekly Impulse filter is green.<br>" +
                "2. Sell short at open next day when the 20-day SMA crosses below the 60-day SMA and the weekly Impulse filter is red.<br>" +
                "3. Exit long at open next day when the 20-day SMA crosses below the 60-day SMA and the Impulse filter is not green.<br>" +
                "4. Cover short position at open next day when the 20-day SMA crosses above the 60-day SMA and the Impulse filter is not red.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("471b277e-e6d1-4fa3-ac33-9e5211506eaf");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 03, 08); }
        }

        public override string Name
        {
            get { { return "ATM 2009-04 | Impulse Filter (Weekly)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMApr2009_2); }
        }
    }

    public class AdaptiveLookback : DataSeries
    {
        private Bars bars;

        public AdaptiveLookback(Bars bars, int howManySwings, bool UseAll, string description)
            : base(bars, description)
        {
            this.bars = bars;

            bool SwingLo, SwingHi;
            double lastSL = bars.Low[1];
            double lastSH = bars.High[1];
            int firstSwingBarOnChart = 0;
            int lastSwingInCalc = 0;
            int swingCount = 0;
            DataSeries so = new DataSeries(bars.Close, "swing_oscillator");
            System.Collections.ArrayList SwingBarArray = new System.Collections.ArrayList();

            for (int bar = 5; bar < bars.Count; bar++)
            {
                SwingLo = (CumDown.Series(bars.Low, 1)[bar - 2] >= 2) &&
                    (CumUp.Series(bars.High, 1)[bar] == 2);
                SwingHi = (CumUp.Series(bars.High, 1)[bar - 2] >= 2) &&
                    (CumDown.Series(bars.Low, 1)[bar] == 2);

                if (SwingLo)
                    so[bar] = -1;
                else
                    if (SwingHi)
                        so[bar] = 1;
                    else
                        so[bar] = 0;

                if ((so[bar] != 0) & (swingCount == 0))
                {
                    firstSwingBarOnChart = bar;
                    swingCount++;
                    SwingBarArray.Add(bar);
                }
                else
                    if (swingCount > 0)
                    {
                        if (so[bar] != 0.0)
                        {
                            swingCount++;
                            SwingBarArray.Add(bar);
                        }

                        // 20090127 Added
                        if (swingCount == howManySwings)
                            base.FirstValidValue = bar;
                    }

                lastSwingInCalc = (SwingBarArray.Count - howManySwings);

                if (lastSwingInCalc >= 0)
                {
                    base[bar] = UseAll ? (int)(bars.Count / SwingBarArray.Count) :
                        (bar - (int)SwingBarArray[lastSwingInCalc]) / howManySwings;
                }
            }
        }

        public static AdaptiveLookback Series(Bars bars, int howManySwings, bool UseAll)
        {
            string description = string.Concat(new object[] { "Adaptive Lookback(", bars.Symbol, ",", howManySwings.ToString(), ",", UseAll.ToString(), ")" });
            //string description = string.Concat(new object[] { "Adaptive Lookback(", bars.ToString(), ",", howManySwings.ToString(), ",", UseAll.ToString(), ")" });

            if (bars.Cache.ContainsKey(description))
            {
                return (AdaptiveLookback)bars.Cache[description];
            }

            AdaptiveLookback _AdaptiveLookback = new AdaptiveLookback(bars, howManySwings, UseAll, description);
            bars.Cache[description] = _AdaptiveLookback;
            return _AdaptiveLookback;
        }
    }

    public class ATMMay2009 : WealthScript
    {
        private StrategyParameter paramSwings;
        private StrategyParameter paramMode;

        public ATMMay2009()
        {
            paramSwings = CreateParameter("Nr. of swings", 6, 1, 20, 1);
            paramMode = CreateParameter("Use all", 0, 0, 1, 1);
        }

        protected override void Execute()
        {
            // mode: "Use all" includes ALL swing points in calculation - strongly not advised
            bool mode = false; if (paramMode.ValueInt > 0) mode = true;
            LineStyle solid = LineStyle.Solid;
            HideVolume();

            // Create an instance of the AdaptiveLookback indicator class
            AdaptiveLookback ap = AdaptiveLookback.Series(Bars, paramSwings.ValueInt, mode);
            DataSeries adaptiveMFI = new DataSeries(Bars, "Adaptive MFI");
            MFI mfi = MFI.Series(Bars, 14);

            // Fill the adaptive MFI series
            for (int bar = ap.FirstValidValue; bar < Bars.Count; bar++)
                adaptiveMFI[bar] = MFI.Series(Bars, Math.Max(1, (int)ap[bar]))[bar];

            ChartPane amPane = CreatePane(20, true, true);
            PlotSeries(amPane, adaptiveMFI, Color.Blue, solid, 2);
            PlotSeries(amPane, mfi, Color.DarkBlue, solid, 1);
            DrawHorzLine(amPane, 75.0, Color.Red, LineStyle.Dashed, 1);
            DrawHorzLine(amPane, 25.0, Color.Blue, LineStyle.Dashed, 1);

            for (int bar = 50; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (CrossOver(bar, adaptiveMFI, 75))
                        SellAtMarket(bar + 1, p);
                    else
                        SellAtStop(bar + 1, p, p.EntryPrice * 0.70);
                }
                else
                {
                    // Indicator reading validity check
                    if (ap[bar] > 0)
                        if (CrossUnder(bar, adaptiveMFI, 25))
                        {
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = Close[bar];
                        }
                }
            }
        }
    }

    public class ATMMay2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 3, 30); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the May 2009 issue of <b>Active Trader magazine.</b><br>" +

                    "<p><b>System Concept:</b> Introducing a simple and universal adaptive lookback period finder, that can " +
                    "be applied to many existing indicators to make them market-driven, and more responsive to changes in volatility.<br>" +

                    "<p><b>Trade rules:</b></p>" +

                    "<p>1. Buy at tomorrow's open next day when the adaptive MFI crosses below 25.<br>" +
                    "2. Sell at tomorrow's open next day when the adaptive MFI crosses above 75.<br>" +
                    "3. Stop-loss: Sell at a loss of 30 percent of the entry price.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("31336f70-4fab-4046-9ed5-fb47662b55ea");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 3, 30); }
        }

        public override string Name
        {
            get { { return "ATM 2009-05 | Adaptive MFI Strategy"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMMay2009); }
        }
    }

    public class ATMJune2009 : WealthScript
    {
        private StrategyParameter paramRatio;

        public ATMJune2009()
        {
            paramRatio = CreateParameter("Ratio", 150, 100, 300, 25);
        }

        protected override void Execute()
        {
            int Ratio = paramRatio.ValueInt;
            DataSeries X = new DataSeries(Bars, "X");
            DataSeries UpperChannel = new DataSeries(Bars, "Upper Channel");
            DataSeries LowerChannel = new DataSeries(Bars, "Lower Channel");
            UpperChannel.Description = "Upper Channel";
            LowerChannel.Description = "Lower Channel";

            for (int bar = 50; bar < Bars.Count; bar++)
            {
                X[bar] = Math.Max(Math.Truncate(Ratio / ADX.Series(Bars, 14)[bar]), 1);
                UpperChannel[bar] = Highest.Series(High, (int)X[bar])[bar];
                LowerChannel[bar] = Lowest.Series(Low, (int)X[bar])[bar];
            }

            ChartPane x_bar = CreatePane(40, true, true); HideVolume();
            PlotSeries(x_bar, X, Color.DarkGreen, LineStyle.Dashed, 1);
            PlotSeries(PricePane, UpperChannel, Color.Blue, LineStyle.Dotted, 2);
            PlotSeries(PricePane, LowerChannel, Color.Red, LineStyle.Dotted, 2);

            for (int bar = 50; bar < Bars.Count; bar++)
            {
                string x = X[bar].ToString();

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (p.PositionType == PositionType.Long)
                    {
                        SellAtStop(bar + 1, p, LowerChannel[bar] - Bars.SymbolInfo.Tick, x);
                        ShortAtStop(bar + 1, LowerChannel[bar] - Bars.SymbolInfo.Tick, x);
                    }
                    else
                    {
                        CoverAtStop(bar + 1, p, UpperChannel[bar] + Bars.SymbolInfo.Tick, x);
                        BuyAtStop(bar + 1, UpperChannel[bar] + Bars.SymbolInfo.Tick, x);
                    }
                }
                else
                {
                    BuyAtStop(bar + 1, UpperChannel[bar] + Bars.SymbolInfo.Tick, x);
                    ShortAtStop(bar + 1, LowerChannel[bar] - Bars.SymbolInfo.Tick, x);
                }
            }
        }
    }

    public class ATMJune2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 4, 29); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the June 2009 issue of <b>Active Trader magazine.</b><br>" +

                    "<p><b>System Concept:</b> This approach, originally derived from Chuck LeBeau's trading forum, " +
                    "sets the length of the channel (the look-back period) as a function of the trend strength, " +
                    "as measured by the average directional movement index (ADX).</p>" +

                    "<p><b>Trade rules:</b></p>" +

                    "<p>1. <b>Enter long and cover short</b> next day when the high of the day penetrates the highest high price of X bars back, plus one tick.<br>" +
                    "2. <b>Enter short and exit long</b> next day when the low of the day penetrates the lowest low price of X bars back, minus one tick.</p>" +

                    "<p>The formula to arrive at the variable channel period is: <br>" +
                    "X (channel length) = 300/14-bar ADX<br>" +
                    "where: X is rounded to the nearest whole number.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("6a0dffc5-caf0-4b04-a838-0fa4c6e07b04");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 4, 29); }
        }

        public override string Name
        {
            get { { return "ATM 2009-06 | Adaptive Price Channels"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJune2009); }
        }
    }

    public class AdaptiveLookback072009 : DataSeries
    {
        private Bars bars;

        public AdaptiveLookback072009(Bars bars, int howManySwings, string description)
            : base(bars, description)
        {
            this.bars = bars;

            bool SwingLo, SwingHi;
            double lastSL = bars.Low[1];
            double lastSH = bars.High[1];
            int firstSwingBarOnChart = 0;
            int lastSwingInCalc = 0;
            int swingCount = 0;
            double so = 0;
            List<int> SwingBarArray = new List<int>();

            for (int bar = 5; bar < bars.Count; bar++)
            {
                SwingLo = (CumDown.Series(bars.Low, 1)[bar - 2] >= 2) && (CumUp.Series(bars.High, 1)[bar] == 2);
                SwingHi = (CumUp.Series(bars.High, 1)[bar - 2] >= 2) && (CumDown.Series(bars.Low, 1)[bar] == 2);

                so = SwingLo ? -1 : SwingHi ? 1 : 0;

                if ((so != 0) & (swingCount == 0))
                {
                    firstSwingBarOnChart = bar;
                    swingCount++;
                    SwingBarArray.Add(bar);
                }
                else
                    if (swingCount > 0)
                    {
                        if (so != 0.0)
                        {
                            swingCount++;
                            SwingBarArray.Add(bar);
                        }

                        if (swingCount == howManySwings)
                            base.FirstValidValue = bar;
                    }

                lastSwingInCalc = (SwingBarArray.Count - howManySwings);

                if (lastSwingInCalc >= 0)
                    base[bar] = (bar - (int)SwingBarArray[lastSwingInCalc]) / howManySwings;
            }
        }

        public static AdaptiveLookback072009 Series(Bars bars, int howManySwings)
        {
            string description = string.Concat(new object[] { "Adaptive Lookback(", howManySwings.ToString(), ")" });

            if (bars.Cache.ContainsKey(description))
            {
                return (AdaptiveLookback072009)bars.Cache[description];
            }

            AdaptiveLookback072009 _AdaptiveLookback = new AdaptiveLookback072009(bars, howManySwings, description);
            bars.Cache[description] = _AdaptiveLookback;
            return _AdaptiveLookback;
        }
    }

    public class GannSwingOscillator : DataSeries
    {
        private Bars bars;

        public GannSwingOscillator(Bars bars, bool zero, string description)
            : base(bars, description)
        {
            this.bars = bars;

            bool SwingLo, SwingHi;
            base.FirstValidValue = 5;

            for (int bar = 5; bar < bars.Count; bar++)
            {
                /* Regular swing point calculation */
                /*SwingLo = (CumDown.Series(bars.Low, 1)[bar - 2] >= 2) && (CumUp.Series(bars.Low, 1)[bar] == 2);
                SwingHi = (CumUp.Series(bars.High, 1)[bar - 2] >= 2) && (CumDown.Series(bars.High, 1)[bar] == 2);*/

                /* Our experimental swing point calculation */
                SwingLo = (CumDown.Series(bars.Low, 1)[bar - 2] >= 2) && (CumUp.Series(bars.High, 1)[bar] == 2);
                SwingHi = (CumUp.Series(bars.High, 1)[bar - 2] >= 2) && (CumDown.Series(bars.Low, 1)[bar] == 2);

                if (SwingLo)
                    base[bar] = -1;
                else
                    if (SwingHi)
                        base[bar] = 1;
                    else
                        /* Behavior choice */
                        if (!zero)
                            base[bar] = this[bar - 1];
                        else
                            base[bar] = 0;
            }

        }

        public static GannSwingOscillator Series(Bars bars, bool zero)
        {
            string description = string.Concat(new object[] { "GannSwingOscillator(", zero, ")" });

            if (bars.Cache.ContainsKey(description))
            {
                return (GannSwingOscillator)bars.Cache[description];
            }

            GannSwingOscillator _GannSwingOscillator = new GannSwingOscillator(bars, zero, description);
            bars.Cache[description] = _GannSwingOscillator;
            return _GannSwingOscillator;
        }
    }

    public class ATMJuly2009 : WealthScript
    {
        private StrategyParameter paramMode;
        private StrategyParameter paramSwings;
        private StrategyParameter paramDays;

        public ATMJuly2009()
        {
            paramMode = CreateParameter("Adaptive", 1, 0, 1, 1);
            paramSwings = CreateParameter("Nr. of swings", 5, 1, 20, 1);
            paramDays = CreateParameter("Exit after", 15, 3, 50, 1);
        }

        protected override void Execute()
        {
            bool mode = (paramMode.ValueInt == 0) ? false : true;
            int days = paramDays.ValueInt;
            LineStyle solid = LineStyle.Solid;
            HideVolume();

            // Create an instance of the AdaptiveLookback indicator class
            AdaptiveLookback072009 ap = AdaptiveLookback072009.Series(Bars, paramSwings.ValueInt);
            DataSeries adaptiveRSI = new DataSeries(Bars, "Adaptive RSI");
            RSI rsi = RSI.Series(Close, 14);
            SMA sma = SMA.Series(Close, 150);
            DataSeries bbU = new DataSeries(Bars, "bbU");
            DataSeries bbL = new DataSeries(Bars, "bbL");

            // Fill the adaptive RSI series
            for (int bar = ap.FirstValidValue; bar < Bars.Count; bar++)
                adaptiveRSI[bar] = RSI.Series(Close, Math.Max(1, (int)ap[bar]))[bar];

            if (!mode)
            {
                bbU = BBandUpper.Series(rsi, 100, 2.0);
                bbL = BBandLower.Series(rsi, 100, 2.0);
            }
            else
            {
                bbU = BBandUpper.Series(adaptiveRSI, 100, 2.0);
                bbL = BBandLower.Series(adaptiveRSI, 100, 2.0);
            }

            bbU.Description = "Dynamic Upper Band";
            bbL.Description = "Dynamic Lower Band";
            ap.Description = "Adaptive Lookback (5)";
            GannSwingOscillator.Series(Bars, true).Description = "Swing Points";

            ChartPane swingPane = CreatePane(20, true, true);
            PlotSeries(swingPane, GannSwingOscillator.Series(Bars, true), Color.DarkBlue, solid, 1);
            ChartPane alPane = CreatePane(20, true, true);
            PlotSeries(alPane, ap, Color.Purple, solid, 2);

            ChartPane arPane = CreatePane(20, true, true);
            PlotSeries(arPane, adaptiveRSI, Color.Blue, solid, 2);
            PlotSeries(arPane, rsi, Color.DarkBlue, solid, 1);
            PlotSeries(arPane, bbU, Color.Red, LineStyle.Dashed, 1);
            PlotSeries(arPane, bbL, Color.Blue, LineStyle.Dashed, 1);
            PlotSeries(PricePane, sma, Color.DarkRed, solid, 1);

            bool uptrend = false; bool downtrend = false;

            for (int bar = 150; bar < Bars.Count; bar++)
            {
                if (!mode)
                {
                    uptrend = ((Close[bar] > sma[bar]) & (rsi[bar] > 0));
                    downtrend = ((Close[bar] < sma[bar]) & (rsi[bar] > 0));
                }
                else
                {
                    uptrend = ((Close[bar] > sma[bar]) & (adaptiveRSI[bar] > 0));
                    downtrend = ((Close[bar] < sma[bar]) & (adaptiveRSI[bar] > 0));
                }

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (bar + 1 - p.EntryBar >= days)
                        ExitAtMarket(bar + 1, p, "Timed");
                }
                else
                {
                    if (!mode)
                    {
                        if (uptrend & CrossOver(bar, rsi, bbL))
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = rsi[bar];
                        if (downtrend & CrossUnder(bar, rsi, bbU))
                            if (ShortAtMarket(bar + 1) != null)
                                LastPosition.Priority = rsi[bar];
                    }
                    else
                    {
                        if (uptrend & CrossOver(bar, adaptiveRSI, bbL))
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = adaptiveRSI[bar];
                        if (downtrend & CrossUnder(bar, adaptiveRSI, bbU))
                            if (ShortAtMarket(bar + 1) != null)
                                LastPosition.Priority = adaptiveRSI[bar];
                    }
                }
            }
        }
    }

    public class ATMJuly2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 6, 8); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the July 2009 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b> Introducing a simple and universal adaptive lookback period finder, that can " +
                "be applied to many existing indicators to make them market-driven, and more responsive to changes in volatility.<br>" +

                "<p><b>Experimental swing point calculation:</b></p>" +

                "<p>Define <b>Swing high</b> as two consecutive higher highs followed by two consecutive lower low" +
                " and <b>Swing low</b> as two consecutive lower lows followed by two consecutive higher highs.<br>" +
                "Determine the initial number of swing points to use in the calculation (five by default).<br>" +
                "Count the number of price bars it takes for the <i>n</i> swing points to form.<br>" +
                "Divide step 2 by step 1 and round the result.</p>" +

                "<p><b>Trade rules:</b></p>" +

                "<p>1. <b>Buy</b> at tomorrow's open when the closing price is above the 150-day SMA and the adaptive RSI crosses above its dynamic oversold threshold.<br>" +
                "2. <b>Short</b> at tomorrow's open when the closing price is below the 150-day SMA and the adaptive RSI crosses below its dynamic overbought threshold.<br>" +
                "3. Exit at the market after 15 days.</p>" +

                "<p><b>Overbought-oversold level calculation:</b></p>" +

                "<p>The adaptive RSI's dynamic thresholds defined by Bollinger Bands with upper and lower boundaries placed two standard deviations " +
                "above and below a 100-day SMA.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("f26dc7f4-4257-4ea4-bad8-19bedea35ec6");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 6, 8); }
        }

        public override string Name
        {
            get { { return "ATM 2009-07 | Adaptive RSI 2.0"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJuly2009); }
        }
    }

    public class ATMAug2009 : WealthScript
    {
        private StrategyParameter paramExit;

        public ATMAug2009()
        {
            paramExit = CreateParameter("Exit after", 20, 5, 50, 5);
        }

        protected override void Execute()
        {
            bool[] BullsShakeout;
            CandlePattern.ReversalGravestoneDoji(this, "BullsShakeout", true, out BullsShakeout);
            bool[] BearsShakeout;
            CandlePattern.ReversalDragonflyDoji(this, "BearsShakeout", true, out BearsShakeout);

            SMA sma1 = SMA.Series(Close, 20);
            SMA sma2 = SMA.Series(Close, 50);
            //VHF vhf = VHF.Series( Close,28 );
            DataSeries vhf = SMA.Series(VHF.Series(Close, 18), 6);
            vhf.Description = String.Concat("Smoothed VHF(", Close.Description, ",", "18)");
            ADX adx = ADX.Series(Bars, 28);

            int exitBars = paramExit.ValueInt;

            for (int bar = 50; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (bar + 1 - p.EntryBar >= exitBars)
                        ExitAtMarket(bar + 1, p, "Timed");
                }
                else
                {
                    if (vhf[bar] < 0.5)
                    {
                        if (sma1[bar] < sma2[bar])
                            if (BearsShakeout[bar])
                                if (BuyAtMarket(bar + 1) != null)
                                    LastPosition.Priority = vhf[bar];

                        if (sma1[bar] > sma2[bar])
                            if (BullsShakeout[bar])
                                if (ShortAtMarket(bar + 1) != null)
                                    LastPosition.Priority = vhf[bar];
                    }
                }
            }
        }
    }

    public class ATMAug2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 7, 1); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the August 2009 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b> This counter-trend strategy uses the Shakeout candle patterns to signal short term reversals. " +
                "The system includes the vertical horizontal filter (VHF) to avoid entries during strong trends.<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p><b>Go long</b> on the next bar at the open when:<br>" +
                "1. The open, close, and high are the same or nearly the same.<br>" +
                "2. The low is significantly lower than the open, close, and high.<br>" +
                "3. The 10-bar simple moving average (SMA) is below the 30-bar SMA.<br>" +
                "4. The six-bar SMA of the VHF indicator is less than 0.5.</p>" +

                "<p><b>Go short</b> on the next bar at the open when:<br>" +
                "1. The open, close, and low are the same or nearly the same.<br>" +
                "2. The high is significantly higher than the open, close, and low.<br>" +
                "3. The 10-bar SMA is above the 30-bar SMA.<br>" +
                "4. The six-bar SMA of the 18-bar VHF indicator is less than 0.5.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("ed64ba90-e034-457d-9fb4-745697ed49a5");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 7, 1); }
        }

        public override string Name
        {
            get { { return "ATM 2009-08 | The Shakeout system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMAug2009); }
        }
    }

    //Highest2 - the highest of two series
    public class Highest2 : DataSeries
    {
        public Highest2(DataSeries ds1, DataSeries ds2, int period)
            : base(ds2, "Highest Of Two")
        {
            DataSeries tmp = ds1 - ds1;
            for (int bar = tmp.FirstValidValue; bar < ds2.Count; bar++)
            {
                tmp[bar] = Math.Max(ds1[bar], ds2[bar]);
            }

            for (int bar = tmp.FirstValidValue; bar < ds2.Count; bar++)
            {
                this[bar] = Highest.Series(tmp, period)[bar];
            }
        }

        public static Highest2 Series(DataSeries ds1, DataSeries ds2, int period)
        {
            /*Highest2 _Highest2 = new Highest2( ds1, ds2, period );
            return _Highest2;*/

            string description = string.Concat(new object[] { "_Highest2(", ds1.Description, ",", ds2.Description, ",", period, ")" });
            if (ds1.Cache.ContainsKey(description))
            {
                return (Highest2)ds1.Cache[description];
            }

            Highest2 _Highest2 = new Highest2(ds1, ds2, period);
            ds1.Cache[description] = _Highest2;
            return _Highest2;

        }
    }

    //Lowest2 - the lowest of two series
    public class Lowest2 : DataSeries
    {
        public Lowest2(DataSeries ds1, DataSeries ds2, int period)
            : base(ds2, "Lowest Of Two")
        {
            DataSeries tmp = ds1 - ds1;
            for (int bar = tmp.FirstValidValue; bar < ds2.Count; bar++)
            {
                tmp[bar] = Math.Min(ds1[bar], ds2[bar]);
            }

            for (int bar = tmp.FirstValidValue; bar < ds2.Count; bar++)
            {
                this[bar] = Lowest.Series(tmp, period)[bar];
            }
        }

        public static Lowest2 Series(DataSeries ds1, DataSeries ds2, int period)
        {
            /*Lowest2 _Lowest2 = new Lowest2( ds1, ds2, period );
            return _Lowest2;*/

            string description = string.Concat(new object[] { "Lowest2(", ds1.Description, ",", ds2.Description, ",", period, ")" });
            if (ds1.Cache.ContainsKey(description))
            {
                return (Lowest2)ds1.Cache[description];
            }

            Lowest2 _Lowest2 = new Lowest2(ds1, ds2, period);
            ds1.Cache[description] = _Lowest2;
            return _Lowest2;
        }
    }

    public class Choppiness : DataSeries
    {
        private Bars bars;
        private int period;

        public Choppiness(Bars bars, int period, string description)
            : base(bars, description)
        {
            this.bars = bars;
            this.period = period;
            base.FirstValidValue = period;

            double true_high, true_low, true_rng, sum;
            DataSeries n_high = bars.High - bars.High;
            DataSeries n_low = bars.Low - bars.Low;
            for (int bar = bars.FirstActualBar + period; bar < bars.Count; bar++)
            {
                true_high = Math.Max(bars.High[bar], bars.Close[bar - 1]);
                true_low = Math.Min(bars.Low[bar], bars.Close[bar - 1]);
                true_rng = TrueRange.Series(bars)[bar];
                n_high[bar] = true_high;
                n_low[bar] = true_low;
            }

            DataSeries trueHigh = Highest2.Series(bars.High, bars.Close >> 1, period);
            DataSeries trueLow = Lowest2.Series(bars.Low, bars.Close >> 1, period);

            double nHigh, nLow, nRange, ratio, log_ratio, log_n;
            for (int bar = bars.FirstActualBar + period; bar < bars.Count; bar++)
            {
                // OLD:
                /* nHigh = Highest.Series( n_high, period )[bar];
                nLow = Lowest.Series( n_low, period )[bar]; */

                // NEW:
                nHigh = trueHigh[bar];
                nLow = trueLow[bar];

                nRange = nHigh - nLow;
                sum = Sum.Series(TrueRange.Series(bars), period)[bar];
                ratio = sum / nRange;
                log_ratio = Math.Log(ratio);
                log_n = Math.Log(period);

                if (bar <= period)
                    base[bar] = 50;
                else
                    base[bar] = 100 * log_ratio / log_n;
            }
        }

        public static Choppiness Series(Bars bars, int period)
        {
            string description = string.Concat(new object[] { "Choppiness(", period, ")" });

            if (bars.Cache.ContainsKey(description))
            {
                return (Choppiness)bars.Cache[description];
            }

            Choppiness _Choppiness = new Choppiness(bars, period, description);
            bars.Cache[description] = _Choppiness;
            return _Choppiness;
        }
    }

    public class ATMSep2009 : WealthScript
    {
        private StrategyParameter paramMode;
        private StrategyParameter up;
        private StrategyParameter down;

        public ATMSep2009()
        {
            paramMode = CreateParameter("Filter off/on (0/1)", 0, 0, 1, 1);
            down = CreateParameter("Down Days", 3, 2, 10, 1);
            up = CreateParameter("Up Days", 2, 2, 10, 1);
        }

        protected override void Execute()
        {
            VHF vhf = VHF.Series(Close, 14);
            Choppiness chop = Choppiness.Series(Bars, 14);

            ChartPane chopPane = CreatePane(30, true, true);
            ChartPane vhfPane = CreatePane(30, true, true);
            PlotSeriesOscillator(chopPane, chop, 61.8, 38.2, Color.FromArgb(75, Color.Yellow),
                Color.FromArgb(75, Color.Red), Color.CadetBlue, LineStyle.Solid, 2);
            PlotSeries(vhfPane, vhf, Color.Black, LineStyle.Solid, 2);

            bool mode = (paramMode.ValueInt == 1) ? true : false;
            int upDays = up.ValueInt;
            int downDays = down.ValueInt;

            for (int bar = 20; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    if (CumUp.Value(bar, Close, 1) >= upDays)
                        SellAtMarket(bar + 1, LastPosition);
                }
                else
                {
                    if (CumDown.Value(bar, Close, 1) >= downDays)
                        if (mode)
                        {
                            if (chop[bar] < 50)
                                if (BuyAtMarket(bar + 1) != null)
                                    LastPosition.Priority = -chop[bar];
                        }
                        else
                        {
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = -chop[bar];
                        }
                }
            }
        }
    }

    public class ATMSep2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 8, 5); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the September 2009 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b> This system uses a specific indicator, Dreiss Choppiness Index, to filter signals from a basic swing-trading system "
                + "to see if it helps eliminate unfavorable trades in non-trending environments.<br>" +

                "<p><b>Strategy rules (filtered):</b></p>" +

                "<p>1. <b>Buy</b> at tomorrow's open after three consecutive days of lower prices if the 14-day CI is below 50.<br>" +
                "2. <b>Sell</b> at open next bar after two consecutive days of higher prices.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("c13460f7-13a4-4cf9-bfd0-a8dce80e05e0");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 8, 5); }
        }

        public override string Name
        {
            get { { return "ATM 2009-09 | Choppiness Index filter"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMSep2009); }
        }
    }

    public class ATMOct2009_1 : WealthScript
    {
        protected override void Execute()
        {
            StochD sd = StochD.Series(Bars, 14, 3);
            double sdu = 75; double sdl = 25;

            ChartPane sp = CreatePane(75, true, true);
            PlotSeries(sp, sd, Color.Black, LineStyle.Dotted, 2);
            DrawHorzLine(sp, sdu, Color.Red, LineStyle.Solid, 1);
            DrawHorzLine(sp, sdl, Color.Green, LineStyle.Solid, 1);

            // Enough data to create a 14-period 15-minute unstable indicator?
            int barsRequired = 15 * (14 * 3 + 1) / Bars.BarInterval;

            for (int bar = barsRequired; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;

                    if (Bars.IsLastBarOfDay(bar))
                        ExitAtClose(bar, LastPosition, "At Close");
                    else
                    {
                        if (p.PositionType == PositionType.Long & CrossUnder(bar, sd, sdu))
                            if (SellAtMarket(bar + 1, p, "XU OB"))
                                AnnotateBar("Sell", bar, false, Color.DarkGreen);
                        //	else
                        if (p.PositionType == PositionType.Short & CrossOver(bar, sd, sdl))
                            if (CoverAtMarket(bar + 1, p, "XO OS"))
                                AnnotateBar("Cover", bar, true, Color.DarkMagenta);
                    }
                }
                else
                {
                    if (bar < Bars.Count - 1)
                        if (!Bars.IsLastBarOfDay(bar + 1))
                        {
                            if (CrossUnder(bar, sd, sdl))
                                if (ShortAtMarket(bar + 1, "Short SP") != null)
                                {
                                    LastPosition.Priority = -sd[bar];
                                    AnnotateBar("Short", bar, true, Color.Red);
                                }

                            if (CrossOver(bar, sd, sdu))
                                if (BuyAtMarket(bar + 1, "Long SP") != null)
                                {
                                    LastPosition.Priority = +sd[bar];
                                    AnnotateBar("Buy", bar, false, Color.Blue);
                                }
                        }
                }
            }
        }
    }

    public class ATMOct2009_1Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 9, 2); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the October 2009 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b> A highly active intraday tecnhqie that aims to fade the majority of traders who think the market is incapable of moving any higher when Stochastic reaches an oversold level.<br>" +

                "<p><b>Strategy rules (original):</b></p>" +

                "<p>1. <b>Buy</b> next bar at the open when the 14-period slow stochastic crosses above 75.<br>" +
                "2. <b>Sell short</b> next bar at the open when the 14-period slow stochastic crosses below 25.<br>" +
                "3. <b>Sell (close long)</b> next bar at the open when the 14-period slow stochastic crosses back below 75.<br>" +
                "4. <b>Cover short</b> next bar at the open when the 14-period slow stochastic crosses back above 25.<br>" +
                "5. <b>Exit</b> open positions at the close (don't hold overnight).</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("a40a80fb-e7ed-4d14-92be-8d32f0cb7c9a");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 9, 2); }
        }

        public override string Name
        {
            get { { return "ATM 2009-10 | Intraday stochastic pop (original)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMOct2009_1); }
        }
    }

    public class ATMOct2009_2 : WealthScript
    {
        protected override void Execute()
        {
            StochD sd = StochD.Series(Bars, 14, 3);
            double sdu = 75; double sdl = 25;

            ChartPane sp = CreatePane(75, true, true);
            PlotSeries(sp, sd, Color.Black, LineStyle.Dotted, 2);
            DrawHorzLine(sp, sdu, Color.Red, LineStyle.Solid, 1);
            DrawHorzLine(sp, sdl, Color.Green, LineStyle.Solid, 1);

            // Enough data to create a 14-period 60-minute unstable indicator?
            int barsRequired = 60 * (14 * 3 + 1) / Bars.BarInterval;

            for (int bar = barsRequired; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;

                    if (p.PositionType == PositionType.Long & CrossUnder(bar, sd, sdu))
                        SellAtMarket(bar + 1, p, "XU OB");
                    else
                        if (p.PositionType == PositionType.Short & CrossOver(bar, sd, sdl))
                            CoverAtMarket(bar + 1, p, "XO OS");
                }
                else
                {
                    if (bar < Bars.Count - 1)
                        if (!Bars.IsLastBarOfDay(bar + 1))
                        {
                            if (CrossUnder(bar, sd, sdl))
                                if (ShortAtMarket(bar + 1, "Short SP") != null)
                                    LastPosition.Priority = -sd[bar];

                            if (CrossOver(bar, sd, sdu))
                                if (BuyAtMarket(bar + 1, "Long SP") != null)
                                    LastPosition.Priority = sd[bar];
                        }
                }
            }
        }
    }

    public class ATMOct2009_2Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 9, 2); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the October 2009 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b> A highly active intraday tecnhqie that aims to fade the majority of traders who think the market is incapable of moving any higher when Stochastic reaches an oversold level.<br>" +

                "<p><b>Strategy rules (filtered):</b></p>" +

                "<p>1. <b>Buy</b> next bar at the open when the 14-period slow stochastic crosses above 75.<br>" +

                "2. <b>Sell short</b> next bar at the open when the 14-period slow stochastic crosses below 25.<br>" +
                "3. <b>Sell (close long)</b> next bar at the open when the 14-period slow stochastic crosses back below 75.<br>" +
                "4. <b>Cover short</b> next bar at the open when the 14-period slow stochastic crosses back above 25.<br></p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("38f30263-021e-43ae-995d-063f94264248");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 9, 2); }
        }

        public override string Name
        {
            get { { return "ATM 2009-10 | Intraday stochastic pop (modified)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMOct2009_2); }
        }
    }

    public class ATMNov2009 : WealthScript
    {
        private StrategyParameter paramRectLength;
        private StrategyParameter L2;
        private StrategyParameter L3;
        private StrategyParameter paramRectRatio;
        private StrategyParameter paramProfit;
        private StrategyParameter paramStop;
        private StrategyParameter paramATRBreakout;
        private StrategyParameter L8;

        public ATMNov2009()
        {
            paramRectLength = CreateParameter("Rectangle Length", 4, 3, 12, 1);
            L2 = CreateParameter("Range Length", 12, 10, 20, 1);
            L8 = CreateParameter("Range Factor", 1, 1, 3, 0.5);
            L3 = CreateParameter("ATR Length", 30, 10, 40, 10);
            paramRectRatio = CreateParameter("Rectangle Ratio", 0.3, 0.2, 0.5, 0.1);
            paramProfit = CreateParameter("ATR Profit", 2, 1, 4, 1);
            paramStop = CreateParameter("ATR Stop", 1, 1, 4, 1);
            paramATRBreakout = CreateParameter("ATR Breakout", 0.25, 0.25, 1, 0.25);
        }

        protected override void Execute()
        {
            HideVolume();

            int rectLength = paramRectLength.ValueInt;
            int rangeLength = L2.ValueInt;
            double l8 = L8.Value;
            int l3 = L3.ValueInt;
            double rectRatio = paramRectRatio.Value;
            double profit = paramProfit.Value;
            double stop = paramStop.Value;
            double breakout = paramATRBreakout.Value;

            // Fill Series Data
            DataSeries RectangleHeight = Highest.Series(High, rectLength) - Lowest.Series(Low, rectLength);
            DataSeries RangeHeight = (Highest.Series(High, rangeLength) >> rectLength) - (Lowest.Series(Low, rangeLength) >> rectLength);
            ATR atr = ATR.Series(Bars, l3);

            for (int bar = rangeLength * 3; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    double ep = p.EntryPrice;

                    if (LastPosition.PositionType == PositionType.Long)
                    {
                        if (!SellAtStop(bar + 1, p, ep - atr[bar] * stop, "Long Stop " + stop.ToString() + " ATRs"))
                            SellAtLimit(bar + 1, p, ep + atr[bar] * profit, "Long Target " + profit.ToString() + " ATRs");

                    }
                    else
                    {
                        if (!CoverAtStop(bar + 1, p, ep + atr[bar] * stop, "Short Stop " + stop.ToString() + " ATRs"))
                            CoverAtLimit(bar + 1, p, ep - atr[bar] * profit, "Short Target " + stop.ToString() + " ATRs");
                    }
                }
                else
                {
                    if (RectangleHeight[bar] > 0 && RangeHeight[bar] > 0)
                        if (((RectangleHeight[bar] / RangeHeight[bar]) < rectRatio) &
                        (RectangleHeight[bar] < l8 * ATR.Series(Bars, l3)[bar]))
                        //if( RectangleHeight[bar] < l8*ATR.Series( Bars, l3 )[bar] )
                        {
                            /*RectangleHeight = Highest.Series( High,rectLength) - Lowest.Series( Low,rectLength );
                            RangeHeight = ( Highest.Series( High,rangeLength)>>rectLength ) - ( Lowest.Series( Low,rangeLength )>>rectLength );*/

                            double[] rectangle = { bar, Highest.Series(High,rectLength)[bar], bar, Lowest.Series(Low,rectLength)[bar], bar-rectLength, 
								Lowest.Series(Low,rectLength)[bar], bar-rectLength, Highest.Series(High,rectLength)[bar] }; // counter-clockwise
                            double[] range = { bar-rectLength, Highest.Series(High,rangeLength)[bar-rectLength], bar-rectLength, Lowest.Series(Low,rangeLength)[bar-rectLength], bar-rangeLength-rectLength, 
								Lowest.Series(Low,rangeLength)[bar-rectLength], bar-rangeLength-rectLength, Highest.Series(High,rangeLength)[bar-rectLength] }; // counter-clockwise
                            DrawPolygon(PricePane, Color.Blue, Color.LightSteelBlue, LineStyle.Solid, 1, true, rectangle);
                            DrawPolygon(PricePane, Color.Blue, Color.Transparent, LineStyle.Dashed, 1, true, range);

                            if (BuyAtStop(bar + 1, Highest.Series(High, rectLength)[bar] + atr[bar] * breakout, "LE") != null)
                                LastPosition.Priority = -Close[bar];
                            else
                                if (ShortAtStop(bar + 1, Lowest.Series(Low, rectLength)[bar] - atr[bar] * breakout, "SE") != null)
                                    LastPosition.Priority = Close[bar];
                        }
                }
            }
        }
    }

    public class ATMNov2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 9, 25); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the November 2009 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b> The system identifies tradable rectangle patterns by comparing the size of a consolidation to the size " +
                "of a preceding trending range, looking for the consolidation range rather tight than volatile.<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p><b>Setup</b>:<br>" +
                "a) The consolidation's high-low range is less than or equal to 30 percent of the reference range.<br>" +
                "b) The rectangle's height is less than one unit of the 30-period ATR.</p>" +

                "<p>1. <b>Buy</b> the next bar with a stop order at the four-day highest high plus 25 percent of the 30-day ATR.<br>" +
                "2. <b>Sell short</b> the next bar with a stop order at the four-day lowest low minus 25 percent of the 30-day ATR.<br>" +
                "3. <b>Exit long</b> at the next bar with a stop order when price falls to the entry price minus the 30-day ATR.<br>" +
                "4. <b>Exit long</b> at the next bar with a limit order when price rises two times the 30-day ATR above the entry price.<br>" +
                "5. <b>Cover short</b> at the next bar with a stop order when price rises to the entry price plus the 30-day ATR.<br>" +
                "5. <b>Cover short</b> at the next bar with a limit order when price falls two times the 30-day ATR below the entry price.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("012681c6-46a0-470f-acec-7e85b1e957a2");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 9, 25); }
        }

        public override string Name
        {
            get { { return "ATM 2009-11 | Acme-R Rectangle trading system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMNov2009); }
        }
    }

    public class ATMDec2009 : WealthScript
    {
        private StrategyParameter paramBrk;
        private StrategyParameter paramExit;
        private StrategyParameter paramTimeout;

        public ATMDec2009()
        {
            paramBrk = CreateParameter("Low/high period", 20, 10, 40, 5);
            paramExit = CreateParameter("Exit after", 5, 5, 30, 5);
            paramTimeout = CreateParameter("Timeout", 30, 10, 40, 10);
        }

        protected override void Execute()
        {
            double ticks = 5 * Bars.SymbolInfo.Tick;
            int waitBars = 4; // original
            int timeout = paramTimeout.ValueInt;
            int exitBars = paramExit.ValueInt;
            int BreakoutPeriod = paramBrk.ValueInt;

            DataSeries l2 = Lowest.Series(Close, BreakoutPeriod) >> 1;
            l2.Description = String.Concat("Lowest Close of ", BreakoutPeriod.ToString(), " bars");
            PlotSeries(PricePane, l2, Color.Red, LineStyle.Dotted, 1);
            SetBarColors(Color.Silver, Color.Silver);

            bool support = false; int lowBar = 0;

            for (int bar = Math.Max(BreakoutPeriod, exitBars); bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;

                    if (bar + 1 - p.EntryBar >= exitBars)
                        ExitAtMarket(bar + 1, p, "Timed");
                }
                else
                {
                    if (!support)
                    {
                        if (Close[bar] <= l2[bar])
                        {
                            support = true;
                            lowBar = bar;
                            SetBarColor(lowBar, Color.Orange);
                            AnnotateBar(BreakoutPeriod.ToString() + "-day low", bar, false, Color.Orange);
                        }
                    }

                    // reset if setup has timed out
                    if (bar + 1 - lowBar > timeout)
                        support = false;

                    if (support)
                    {
                        if ((bar + 1 - lowBar > waitBars) & 	// 4 days have passed since 1st breakdown
                            (Close[bar] <= l2[bar]) & 		// 2nd breakdown
                            (Close[bar] <= Close[lowBar]) &	// price at the 2nd breakdown is lower than at the 1st
                            (bar + 1 - lowBar <= timeout))	// pattern emerged within [timeout] days
                        {
                            SetBarColor(bar, Color.Red);
                            AnnotateBar("New " + BreakoutPeriod.ToString() + "-day low", bar, false, Color.Red);

                            if (BuyAtMarket(bar + 1) != null)
                            {
                                LastPosition.Priority = Close[bar];
                                lowBar = 0;
                                support = false;
                            }
                        }
                    }
                }
            }
        }
    }

    public class ATMDec2009Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 10, 14); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the December 2009 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b>The modified Turtle Soup system attempts to profit from the more common false moves that often occur at " +
                "obvious breakout and breakdown thresholds.<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p><b>Setup</b>:<br>" +
                "Price makes a new 20-day low close that is equal to or below another 20-day low close established between four and 30 days ago.</p>" +

                "<p>1. <b>Buy</b> next day at the open.<br>" +
                "2. <b>Exit long</b> at the market after five days in the trade.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("33f08ce3-a008-47c3-98f2-9c7d3ad05d17");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 10, 14); }
        }

        public override string Name
        {
            get { { return "ATM 2009-12 | Modified Turtle Soup"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMDec2009); }
        }
    }
}
