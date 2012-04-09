using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;
using WealthLab.Rules;

namespace Strategies.ActiveTrader
{
    public class FOTJan2008 : WealthScript
    {
        //Create parameters
        private StrategyParameter bbPeriod;
        private StrategyParameter bbStdDev;
        private StrategyParameter bbSqueezeLookback;

        public FOTJan2008()
        {
            bbPeriod = CreateParameter("Bands Period", 20, 5, 50, 5);
            bbStdDev = CreateParameter("Std Dev", 2, 1, 5, 0.25);
            bbSqueezeLookback = CreateParameter("Squeeze lookback", 100, 5, 150, 5);
        }

        protected override void Execute()
        {
            HideVolume();

            int BBAvg = bbPeriod.ValueInt;
            double BBStdev = bbStdDev.Value;
            int SqueezeLen = bbSqueezeLookback.ValueInt;
            double level = 0;

            //Obtain parameters values
            int bbPer = bbPeriod.ValueInt;
            double bbSD = bbStdDev.Value;

            BBandLower bbL = BBandLower.Series(Close, bbPer, bbSD);
            BBandUpper bbU = BBandUpper.Series(Close, bbPer, bbSD);
            PlotSeriesFillBand(PricePane, bbU, bbL, Color.Silver, Color.Empty, LineStyle.Solid, 2);

            DataSeries PctBBW = ((bbU - bbL) / SMA.Series(Close, BBAvg)) * 100;
            PctBBW.Description = "% BB Width";
            ChartPane PctBBWPane = CreatePane(30, true, true);
            PlotSeries(PctBBWPane, PctBBW, Color.Silver, WealthLab.LineStyle.Solid, 2);
            PlotSeries(PctBBWPane, Lowest.Series(PctBBW, SqueezeLen), Color.Red, WealthLab.LineStyle.Dots, 4);

            for (int bar = Math.Max(SqueezeLen, BBAvg); bar < Bars.Count; bar++)
            {
                if (PctBBW[bar] <= Lowest.Series(PctBBW, SqueezeLen)[bar - 1])
                    if ((bbU[bar] - bbL[bar]) < 2.5 * ATR.Series(Bars, 10)[bar])
                        SetBackgroundColor(bar, Color.FromArgb(231, 255, 231));

                if (IsLastPositionActive)
                {
                    // Exit trade after 10 bars
                    if (bar + 1 - LastPosition.EntryBar >= 10)
                        ExitAtMarket(bar + 1, LastPosition, "Time-Based");					// or exit at 5-day channel stop
                    level = (LastPosition.PositionType == PositionType.Long) ? Lowest.Series(Low, 5)[bar] : Highest.Series(High, 5)[bar];
                    ExitAtStop(bar + 1, LastPosition, level);
                }
                else
                {
                    if (PctBBW[bar] <= Lowest.Series(PctBBW, SqueezeLen)[bar - 1])
                        if ((bbU[bar] - bbL[bar]) < 2.5 * ATR.Series(Bars, 10)[bar])
                            if (BuyAtStop(bar + 1, High[bar] + Bars.SymbolInfo.Tick) == null)
                            {
                                ShortAtStop(bar + 1, Low[bar] - Bars.SymbolInfo.Tick);
                            }
                }
            }
        }
    }

    public class FOTJan2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return
                "<p>This strategy was featured in the January 2008 issue of <b>Futures & Options Trader</b>.<br></p>" +

                "<p><b>System concept:</b>There is a tendency for Bollinger Bands bands to alternate between expansion and contraction, expanding as volatility increases and contracting as it decreases. The system attempts to capitalize on this idea: when the bands tighten significantly, sharp volatility expansions - and trends - are possible.<br></p>" +

                "<p><b>Strategy Rules:</b><br>" +
                "<p>If the Bollinger Band width is the lowest in 100 days and the absolute distance between the upper and lower Bollinger Bands is less than 2.5 times the 10-day ATR:<br></p>" +
                "<p>1. <b>Go long</b> tomorrow with a stop loss order at today's high plus one tick, or <b>go short</b> tomorrow with a stop-loss order at today’s low minus one tick, whichever comes first.<br>" +
                "2. After 10 bars in a position, <b>exit</b> tomorrow at market.<br>" +
                "3. Alternately, exit on a penetration of the 5-day low (for a long position) or the 5-day high (for a short position).</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("112bf7b7-51b9-4d05-8602-dbbfa18eaa99");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "FOT 2008-01 | Bollinger Band breakout-anticipation system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(FOTJan2008); }
        }
    }

    public class ATMFeb2008 : WealthScript
    {
        //Create parameters
        private StrategyParameter ma1Period;
        private StrategyParameter ma2Period;
        private StrategyParameter ma3Period;
        private StrategyParameter paramConv;
        private StrategyParameter paramChannel;

        public ATMFeb2008()
        {
            ma1Period = CreateParameter("SMA1 period", 10, 10, 30, 2);
            ma2Period = CreateParameter("EMA2 period", 30, 20, 60, 2);
            ma3Period = CreateParameter("EMA3 period", 50, 30, 100, 2);

            paramConv = CreateParameter("Convergence %", 2.5, 0.5, 3, 0.25);
            paramChannel = CreateParameter("Channel length", 30, 2, 30, 2);
        }

        protected override void Execute()
        {
            int ma1period = ma1Period.ValueInt;
            int ma2period = ma2Period.ValueInt;
            int ma3period = ma3Period.ValueInt; int channel = paramChannel.ValueInt;
            double ConvLimit = paramConv.Value;

            DataSeries ma1 = SMA.Series(Close, ma1period);
            DataSeries ma2 = SMA.Series(Close, ma2period);
            DataSeries ma3 = SMA.Series(Close, ma3period);
            ma1.Description = "SMA(" + ma1period + ")";
            ma2.Description = "SMA(" + ma2period + ")";
            ma3.Description = "SMA(" + ma3period + ")";
            PlotSeries(PricePane, ma1, Color.Blue, WealthLab.LineStyle.Solid, 2);
            PlotSeries(PricePane, ma2, Color.Red, WealthLab.LineStyle.Dashed, 2);
            PlotSeries(PricePane, ma3, Color.BlueViolet, WealthLab.LineStyle.Dashed, 2);
            DataSeries conv = (ma1 - ma3) / Bars.Close * 100; conv.Description = "Convergence";
            ChartPane convPane = CreatePane(25, true, true);
            PlotSeries(convPane, conv, Color.Silver, WealthLab.LineStyle.Histogram, 2);
            DrawHorzLine(convPane, ConvLimit, Color.Blue, WealthLab.LineStyle.Dashed, 2);
            DrawHorzLine(convPane, -ConvLimit, Color.Red, WealthLab.LineStyle.Dashed, 2);
            HideVolume();

            for (int bar = ma3.FirstValidValue; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    if (LastPosition.PositionType == PositionType.Long)
                    {
                        if (CrossUnder(bar, Close, ma3))
                            SellAtMarket(bar + 1, LastPosition, "MA XO");
                    }
                    else
                    {
                        if (CrossOver(bar, Close, ma3))
                            CoverAtMarket(bar + 1, LastPosition, "MA XO");
                    }
                }
                else
                {
                    // Extra condition to avoid buying in ranges: the MAs should point up!
                    if (ma1[bar] > ma1[bar - 1])

                        // Proper uptrend order of the moving averages
                        if ((ma1[bar] > ma2[bar]) & (ma2[bar] > ma3[bar]) &
                            // The MAs should converge
                            (Math.Abs(conv[bar - 2]) < ConvLimit) &
                            (Math.Abs(conv[bar - 1]) < ConvLimit) &
                            (Math.Abs(conv[bar]) < ConvLimit))
                        {
                            if (BuyAtStop(bar + 1, Bars.High[bar] + 1 * Bars.SymbolInfo.Tick) != null)
                                LastActivePosition.Priority = -Close[bar];
                        }
                }
            }
        }
    }

    public class ATMFeb2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return
                "<p>This strategy was featured in the February 2008 issue of <b>Active Trader magazine</b>.<br></p>" +
                "<p><b>System concept:</b> The bow-tie pattern is a setup (originally described by Dave Landry) " +
                "that uses three moving averages (e.g. 10-, 20- and 30-day) to capture a price thrust in the direction of a trend. " +
                "The basic setup develops when the moving averages tighten and then separate, resembling a bow tie.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("55027746-de53-4b29-9300-c419a19443ea");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2008-02 | Bow tie variation"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMFeb2008); }
        }
    }

    public class FOTFeb2008 : WealthScript
    {
        private StrategyParameter lookbackPeriod;

        public FOTFeb2008()
        {
            lookbackPeriod = CreateParameter("Lookback Period", 100, 50, 200, 25);
        }

        private string MatchingSymbol()
        {
            /* Routine for CLC-to-COT symbol matching */
            string sym = Bars.Symbol;
            string newSym = sym;
            newSym = sym.Replace("_RAD", "_ALL.COT");			// General			
            switch (sym)										// Exceptions:
            {
                case "C__RAD": newSym = "C_ALL.COT"; break; 	// Corn
                case "BN_RAD": newSym = "BP_ALL.COT"; break;	// British Pound
                case "FN_RAD": newSym = "FX_ALL.COT"; break;	// Euro
                case "JN_RAD": newSym = "JY_ALL.COT"; break;	// Japanese Yen
                case "SN_RAD": newSym = "SF_ALL.COT"; break;	// Swiss Franc
            }

            return newSym;
        }

        protected override void Execute()
        {
            WealthLab.LineStyle solid = WealthLab.LineStyle.Solid;
            WealthLab.LineStyle dashed = WealthLab.LineStyle.Dashed;
            WealthLab.Indicators.EMACalculation modern = WealthLab.Indicators.EMACalculation.Modern;
            int lookback = lookbackPeriod.ValueInt;

            /* To be used with Pinnacle data only. BarScale.Daily only. This data contains following fields:			
            OPEN 		Commercial Long Contacts
            HIGH 		Commercial Short Contracts
            LOW 		Non Commercial Long Contracts
            CLOSE 		Non Commercial Short Contracts
            VOLUME 		Small Trader Long Contracts
            OPEN INT. 	Small Trader Short Contracts
            */
            if (Bars.Scale != BarScale.Daily)
                return;

            Bars cotSym = GetExternalSymbol(MatchingSymbol(), true);// COT Data
            DataSeries NetCommercials = (cotSym.Open - cotSym.High);
            NetCommercials.Description = "Net Commercials"; Font font = new Font("Impact", 12, FontStyle.Regular);
            ChartPane cotPane = CreatePane(30, false, false);
            DrawHorzLine(cotPane, 0, Color.Brown, solid, 2);
            PlotSeries(cotPane, NetCommercials, Color.Blue, solid, 2);
            HideVolume(); PlotStops();

            DataSeries _high = Highest.Series(NetCommercials, lookback);
            DataSeries _low = Lowest.Series(NetCommercials, lookback);
            _high.Description = lookback + "-day high"; _low.Description = lookback + "-day low";
            PlotSeriesFillBand(cotPane, _high, _low, Color.FromArgb(0, 128, 128), Color.Empty, dashed, 2);

            DataSeries ema13 = EMA.Series(Close, 13, modern); ema13.Description = "EMA(13)";
            DataSeries ema39 = EMA.Series(Close, 39, modern); ema39.Description = "EMA(39)";
            PlotSeries(PricePane, ema13, Color.Red, dashed, 2);
            PlotSeries(PricePane, ema39, Color.Blue, dashed, 2);

            for (int bar = lookback; bar < Bars.Count; bar++)
            {
                double adx = ADX.Series(Bars, 14)[bar];
                if (NetCommercials[bar] > 0)
                    SetBackgroundColor(bar, Color.FromArgb(255, 227, 231));
                else 	// Reddish
                    SetBackgroundColor(bar, Color.FromArgb(231, 255, 231));		// Greenish					
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    double stop = (p.PositionType == PositionType.Long) ? p.EntryPrice * 0.95 : p.EntryPrice * 1.05;

                    if (p.PositionType == PositionType.Long)
                    {
                        if (NetCommercials[bar] <= _low[bar])
                        {
                            if (SellAtMarket(bar + 1, p, "Extreme"))
                                AnnotateBar("Cover", bar, true, Color.Red, Color.Empty, font);
                        }
                        else
                            if (SellAtStop(bar + 1, p, stop, "stop loss"))
                                AnnotateBar("Cover", bar, true, Color.Red, Color.Empty, font);
                    }
                    else
                    {
                        if (NetCommercials[bar] >= _high[bar])
                        {
                            if (CoverAtMarket(bar + 1, p, "Extreme"))
                                AnnotateBar("Cover", bar, true, Color.DarkGreen, Color.Empty, font);
                        }
                        else
                            if (CoverAtStop(bar + 1, p, stop, "stop loss"))
                                AnnotateBar("Cover", bar, true, Color.DarkGreen, Color.Empty, font);
                    }
                }
                else
                {
                    if (NetCommercials[bar] < 0)
                    {
                        if ((CrossOver(bar, ema13, ema39)) & adx >= 15)
                            if (BuyAtMarket(bar + 1) != null)
                                AnnotateBar("Buy", bar, false, Color.Blue, Color.Empty, font);
                    }
                    else if (NetCommercials[bar] > 0)
                    {
                        if ((CrossUnder(bar, ema13, ema39)) & adx >= 15)
                            if (ShortAtMarket(bar + 1) != null)
                                AnnotateBar("Short", bar, false, Color.Brown, Color.Empty, font);
                    }
                }
            }
        }
    }

    public class FOTFeb2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return
                "<p>This strategy was featured in the February 2008 issue of <b>Futures & Options Trader</b>.<br></p>" +

                "<p><b>System concept:</b>This system is based on the idea that an extreme long or short commercial position indicates a market reversal. To determine what qualifies as 'extreme', the current COT reading is compared to a certain number of past readings.<br></p>" +

                "<p><b>Strategy Rules:</b><br>" +
                "Buy tomorrow at the market when:<br>" +
                "1. Today's net commercial position is below zero.<br>" +
                "2. The 13-day EMA of closing prices crosses above the 39-day EMA of closing prices.<br>" +
                "3. The 14-day ADX is above 15.<br></p>" +

                "<p>Short tomorrow at the market when:<br>" +
                "1. Today's net commercial position is above zero.<br>" +
                "2. The 13-day EMA of closing prices crosses below the 39-day EMA of closing prices.<br>" +
                "3. The 14-day ADX is above 15.<br>" +

                "<p><font color=red>Note: </font>The code is hardlinked to the Pinnacle Data's symbol names created in ASCII datasource. You have to really know what you're doing and have all those CLC & COT datasources in both MetaStock and ASCII formats on your disk.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("4c190fcb-e433-4d2b-83e6-a63781fe91a9");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "FOT 2008-02 | COT extreme-position system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(FOTFeb2008); }
        }
    }

    public class ATMMar2008 : WealthScript
    {
        //Create parameters
        private StrategyParameter bbPeriod;
        private StrategyParameter bbStdDev;
        private StrategyParameter bbSqueezeLookback;

        public ATMMar2008()
        {
            bbPeriod = CreateParameter("Bollinger Period", 20, 5, 50, 5);
            bbStdDev = CreateParameter("Std Dev", 2, 1, 5, 0.25);
            bbSqueezeLookback = CreateParameter("Squeeze lookback", 100, 5, 150, 1);
        }

        protected override void Execute()
        {
            int BBAvg = bbPeriod.ValueInt;
            double BBStdev = bbStdDev.Value;
            int SqueezeLen = bbSqueezeLookback.ValueInt;
            double PctBBWMin = new double();

            //Obtain parameter values
            int bbPer = bbPeriod.ValueInt;
            double bbSD = bbStdDev.Value;

            BBandLower bbL = BBandLower.Series(Close, bbPer, bbSD);
            BBandUpper bbU = BBandUpper.Series(Close, bbPer, bbSD);
            DataSeries PctBBW = ((bbU - bbL) / SMA.Series(Close, BBAvg)) * 100;
            PctBBW.Description = "% BB Width";
            DataSeries sma = SMA.Series(Close, BBAvg);
            ChartPane PctBBWPane = CreatePane(20, true, true);
            PlotSeries(PctBBWPane, PctBBW, Color.Blue, WealthLab.LineStyle.Solid, 2);
            PlotSeries(PctBBWPane, Lowest.Series(PctBBW, SqueezeLen), Color.Gray, WealthLab.LineStyle.Dashed, 2);
            PlotSeries(PricePane, sma, Color.Silver, WealthLab.LineStyle.Solid, 2);
            Color fillColor = Color.FromArgb(16, 0, 0, 255);
            PlotSeriesFillBand(PricePane, bbU, bbL, Color.Blue, fillColor, LineStyle.Solid, 1);
            HideVolume();

            for (int bar = Math.Max(SqueezeLen, BBAvg); bar < Bars.Count; bar++)
            {
                PctBBWMin = Lowest.Series(PctBBW, SqueezeLen)[bar];

                if (IsLastPositionActive)
                {
                    // Exit trade after N bars
                    if (bar + 1 - LastPosition.EntryBar >= 10)
                        ExitAtMarket(bar + 1, LastPosition, "Time-Based");
                    // Exit at profit target
                    ExitAtLimit(bar + 1, LastPosition, (double)LastPosition.Tag, "Limit");
                }
                else
                {
                    if (PctBBW[bar] == PctBBWMin)
                        if ((Date[bar].TimeOfDay.Hours < 17.0) &
                        (Date[bar].TimeOfDay.Hours >= 13.0))
                        {
                            SetBackgroundColor(bar, Color.FromArgb(231, 255, 231));
                            if (BuyAtMarket(bar + 1, Date[bar].TimeOfDay.TotalHours.ToString()) != null)
                            {
                                LastActivePosition.Priority = Close[bar];
                                // Store target price in the position's tag property
                                LastActivePosition.Tag = (Close[bar] + 3 * ATR.Series(Bars, 10)[bar]);
                            }
                        }
                }
            }
        }
    }

    public class ATMMar2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return
                "<p>This strategy was featured in the March 2008 issue of <b>Active Trader magazine</b>.<br></p>" +

                "<p><b>System concept:</b>This system uses Bollinger Bands to identify exceptionally low-volatility conditions in an attempt to " +
                "get a jump on the price burst - a volatility breakout - that is expected to follow.<br></p>" +

                "<p><b>Trading rules:</b><br>" +
                "1. If the BandWidth indicator is the lowest of the past 100 hourly bars, and the time is between 13:00 and 16:30 ET (1 and 4:30 p.m.), go long at the market on the next bar.<br>" +
                "2. Exit at the market after 10 hourly bars.<br>" +
                "3. Alternately, exit using a limit order at the close of the bar preceding the entry bar plus three times the 10-bar ATR.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("14f21220-ca80-4df5-941e-779c3c43e2dd");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2008-03 | Bollinger Band intraday breakout anticipation"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMMar2008); }
        }
    }

    public class ATMApr2008 : WealthScript
    {
        protected override void Execute()
        {
            WealthLab.LineStyle solid = WealthLab.LineStyle.Solid;
            WealthLab.LineStyle dashed = WealthLab.LineStyle.Dashed;
            WealthLab.Indicators.EMACalculation modern = WealthLab.Indicators.EMACalculation.Modern;

            /* To be used with Pinnacle data only. Works only with Daily timeframe.
            The Pinnacle COT data fields contain:			
            Open|High				Commercial Long|Short Contacts
            Low|Close				Non Commercial Long|Short Contracts
            Volume|Open_Interest	Small Trader Long|Short Contracts
            */
            if (Bars.Scale != BarScale.Daily)
                return;

            try
            {
                Bars cotIdxSym = GetExternalSymbol("ES_NCOTI", true);		// COT Index, Non commercials				
                Font font = new Font("Impact", 12, FontStyle.Regular);
                ChartPane idxPane = CreatePane(30, false, false);
                DrawHorzLine(idxPane, 10, Color.Blue, dashed, 2);
                DrawHorzLine(idxPane, 90, Color.Red, dashed, 2); DataSeries LargeSpeculatorsCOTIndex = (cotIdxSym.Close);
                DataSeries upper = BBandUpper.Series(LargeSpeculatorsCOTIndex, 100, 2.0);
                DataSeries lower = BBandLower.Series(LargeSpeculatorsCOTIndex, 100, 2.0);
                DataSeries ema13 = EMA.Series(Close, 13, modern); ema13.Description = "EMA(13)";

                LargeSpeculatorsCOTIndex.Description = "COT Index (NonCommercials)";
                upper.Description = "Adaptive COT Index (Upper)";
                lower.Description = "Adaptive COT Index (Lower)"; PlotSeries(idxPane, LargeSpeculatorsCOTIndex, Color.DarkBlue, solid, 2);
                PlotSeries(idxPane, upper, Color.FromArgb(0, 128, 128), solid, 2);
                PlotSeries(idxPane, lower, Color.FromArgb(0, 128, 128), solid, 2);
                PlotSeries(PricePane, ema13, Color.Red, dashed, 2);
                HideVolume();

                for (int bar = 300; bar < Bars.Count; bar++)
                {
                    if (IsLastPositionActive)
                    {
                        Position p = LastPosition;
                        double stop = (p.PositionType == PositionType.Long) ? p.EntryPrice * 0.90 : p.EntryPrice * 1.10;
                        if (p.PositionType == PositionType.Long)
                        {
                            if (LargeSpeculatorsCOTIndex[bar] <= lower[bar])
                            {
                                SetBackgroundColor(bar, Color.FromArgb(231, 255, 231));
                                if (SellAtMarket(bar + 1, p, "Lower limit reached"))
                                    AnnotateBar("Sell", bar, false, Color.Red, Color.Empty, font);
                            }
                            else
                                if (SellAtStop(bar + 1, p, stop, "Stop Loss"))
                                    AnnotateBar("Sell", bar, false, Color.Red, Color.Empty, font);
                        }
                        else
                        {
                            if (LargeSpeculatorsCOTIndex[bar] >= upper[bar])
                            {
                                SetBackgroundColor(bar, Color.FromArgb(255, 227, 231));
                                if (CoverAtMarket(bar + 1, p, "Opposite signal"))
                                    AnnotateBar("Cover", bar, true, Color.DarkGreen, Color.Empty, font);
                            }
                            else
                                if (CoverAtStop(bar + 1, p, stop, "Stop Loss"))
                                    AnnotateBar("Cover", bar, true, Color.DarkGreen, Color.Empty, font);
                        }
                    }
                    else
                    {
                        if (LargeSpeculatorsCOTIndex[bar] <= lower[bar])
                        {
                            SetBackgroundColor(bar, Color.FromArgb(231, 255, 231));
                            if (Close[bar] < ema13[bar])
                                if (ShortAtMarket(bar + 1, "Lower limit reached") != null)
                                {
                                    LastActivePosition.Priority = Momentum.Value(bar, Close, 13);
                                    AnnotateBar("Short", bar, true, Color.Brown, Color.Empty, font);
                                }
                        }
                        else
                            if (LargeSpeculatorsCOTIndex[bar] >= upper[bar])
                            {
                                SetBackgroundColor(bar, Color.FromArgb(255, 227, 231));
                                if (Close[bar] > ema13[bar])
                                    if (BuyAtMarket(bar + 1, "Upper limit reached") != null)
                                    {
                                        LastActivePosition.Priority = Momentum.Value(bar, Close, 13);
                                        AnnotateBar("Buy", bar, false, Color.Blue, Color.Empty, font);
                                    }
                            }
                    }
                }
            }
            catch
            {
                DrawLabel(PricePane, "No supporting data for ES_NCOTI", Color.Red);
            }
        }
    }

    public class ATMApr2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return "<p>This strategy was featured in the April 2008 issue of <b>Active Trader magazine.</b><br></p>" +
                "<p>It uses a dynamic approach to finding extremes in large-trader open positions in the S&P 500 stock index futures that are then used to trigger trades.<br></p>" +
                "<p><font color=red>Note:</font> The code makes use of the Pinnacle Data's symbol naming conventions.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("8075b15d-9506-4800-9b55-baa72d9f97e9");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2008-04 | Adaptive COT index"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMApr2008); }
        }
    }

    public class ATMMay2008 : WealthScript
    {

        protected override void Execute()
        {
            WealthLab.LineStyle ls = WealthLab.LineStyle.Solid;
            WealthLab.LineStyle lh = WealthLab.LineStyle.Histogram;

            DataSeries priority = Close;

            try
            {

                SetScaleWeekly();
                DataSeries index = GetExternalSymbol("^VAY", true).Close;
                RestoreScale();
                index = Synchronize(index); index.Description = "Value Line Arithmetic Index";
                DataSeries weeklyChange = ROC.Series(index, 1); weeklyChange.Description = "Weekly % Change";

                ChartPane vayPane = CreatePane(30, true, true);
                ChartPane WeeklyPane = CreatePane(30, true, true);
                PlotSeries(vayPane, index, Color.Plum, ls, 2);
                PlotSeries(WeeklyPane, weeklyChange, Color.Black, lh, 3);
                DrawHorzLine(WeeklyPane, 4, Color.Green, ls, 1);
                DrawHorzLine(WeeklyPane, -4, Color.Red, ls, 1); for (int bar = 10; bar < Bars.Count; bar++)
                {
                    if (IsLastPositionActive)
                    {
                        Position p = LastPosition;
                        if (weeklyChange[bar] <= -4)
                        {
                            SetBackgroundColor(bar, Color.FromArgb(255, 227, 231));
                            SellAtMarket(bar + 1, p);
                        }
                    }
                    else
                    {
                        if (weeklyChange[bar] >= 4)
                        {
                            SetBackgroundColor(bar, Color.FromArgb(231, 255, 231));
                            if (BuyAtMarket(bar + 1) != null)
                                LastActivePosition.Priority = -priority[bar];
                        }
                    }
                }
            }
            catch
            {
                DrawLabel(PricePane, "No supporting data for ^VAY", Color.Red);
                DrawLabel(PricePane, "Please download the data for ^VAY using Yahoo! provider", Color.DarkGreen);
            }
        }
    }

    public class ATMMay2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return
                "<p>This strategy was featured in the May 2008 issue of <b>Active Trader magazine</b>.<br></p>" +

                "<p><b>System concept:</b>The 4-percent model was developed by Ned Davis and popularized by Martin Zweig. It is based on weekly percent changes in the Value Line Composite index. The model goes long when the market makes a 4-percent up move and gets out during downtrends.<br></p>" +

                "<p><b>Strategy Rules:</b></p><br>" +
                "<p>1. When the VLA rises 4 percent or more from the previous week's close, go long at the market on the next bar.<br>" +
                "2. When the VLA falls 4 percent or more from the previous week's close, exit tomorrow at market.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("c6730cf0-c4e4-4838-826d-b6663653fb04");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 11); }
        }

        public override string Name
        {
            get { { return "ATM 2008-05 | The 4-percent model"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMMay2008); }
        }
    }

    public class ATMJul2008 : WealthScript
    {
        private StrategyParameter paramSwing;
        private StrategyParameter paramDays;
        private StrategyParameter paramDistance;
        private StrategyParameter paramSupport;

        public ATMJul2008()
        {
            paramSwing = CreateParameter("Swing %", 3, 2, 10, 0.5);
            paramDays = CreateParameter("Time-based", 30, 15, 50, 5);
            paramDistance = CreateParameter("Distance", 30, 10, 50, 10);
            paramSupport = CreateParameter("Support, days", 30, 10, 100, 10);
        }

        protected override void Execute()
        {
            // Find the recent Spring setup
            PeakTroughMode mode = PeakTroughMode.Percent;
            LineStyle ld = LineStyle.Dashed;
            double swing = paramSwing.Value;
            int days = paramDays.ValueInt;
            int distance = paramDistance.ValueInt;
            int support = paramSupport.ValueInt;
            bool foundSpring = false;
            int detectedBar = 0;
            double t1 = 0; double p1 = 0; double t2 = 0;
            int tb1 = 0; int pb1 = 0; int tb2 = 0;

            DataSeries pkHigh = Peak.Series(High, swing, mode);
            DataSeries pkBarHigh = PeakBar.Series(High, swing, mode);
            DataSeries tgLow = Trough.Series(Low, swing, mode);
            DataSeries tgBarLow = TroughBar.Series(Low, swing, mode);


            SetBarColors(Color.Silver, Color.Silver);
            for (int bar = 30; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (bar + 1 - p.EntryBar >= days)
                        SellAtMarket(bar + 1, p, "Timed");
                }
                else
                {
                    if (foundSpring)
                    {
                        foundSpring = false;
                        if (BuyAtStop(bar + 1, High[bar]) != null)
                            LastActivePosition.Priority = Close[bar];
                    }
                    else
                    {
                        //determine locations and values of most recent peaks and troughs
                        t1 = tgLow[bar];
                        tb1 = (int)tgBarLow[bar];
                        if (tb1 == -1) continue;

                        p1 = pkHigh[bar];
                        pb1 = (int)pkBarHigh[bar];
                        if (pb1 == -1) continue;

                        t2 = tgLow[pb1];
                        tb2 = (int)tgBarLow[pb1];
                        if (tb2 == -1) continue;

                        //is the first swing low higher than the second?
                        if ((t1 < t2) & (Math.Abs(tb2 - tb1) <= distance) &
                            (Low[tb1] <= Lowest.Value(bar, Low, support)))
                        {
                            //don't detect the same setup twice
                            if (tb2 != detectedBar)
                            {
                                foundSpring = true;
                                detectedBar = tb2;

                                AnnotateBar("T1", tb2, false, Color.Red);
                                AnnotateBar("P1", pb1, false, Color.Blue);
                                AnnotateBar("T2", tb1, false, Color.Orange);

                                DrawLine(PricePane, tb2, High[tb2], tb2, Low[tb2], Color.Red, ld, 2);
                                DrawLine(PricePane, tb1, Low[tb1], pb1, High[pb1], Color.Orange, ld, 2);
                                DrawLine(PricePane, pb1, High[pb1], tb2, Low[tb2], Color.Blue, ld, 2);
                                DrawLine(PricePane, bar, High[bar], tb1, Low[tb1], Color.Green, ld, 2);
                            }
                        }
                    }
                }
            }
        }
    }

    public class ATMJul2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return

                "<p>This strategy was featured in the July 2008 issue of <b>Active Trader magazine</b>.<br></p>" +
                "<p><b>System concept:</b>This system attempts to profit from Richard Wyckoff's 'spring' setup - the rally off the second bottom and back into the trading range.<br></p>" +
                "<p><b>Strategy Rules:</b></p><br>" +
                "<p>1. Setup:<br>" +
                "1.1. First swing low: A downward reversal of the low price three percent or greater.<br>" +
                "1.2. Rally: An upward reversal off the first swing low – a move of the high price greater than or equal to 3 percent.<br>" +
                "1.3. Second swing low: Finally, second trough of the low price of 3 percent and greater, that should be lower than the first swing low.<br>" +
                "1.4. The pivot lows must occur within 30 days of each other.<br>" +
                "1.5. The second pivot point (second swing low) should break down through a support level of the lowest low of the last 30 days.<br>" +
                "2. Enter long with a stop order at the high price of bar when the second swing low is detected.<br>" +
                "3. Exit long position after 30 days.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("09b3cb30-58b2-4957-96e8-644b4472d4b1");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2008-07 | Wyckoff spring setup"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJul2008); }
        }
    }

    public class ChandelierLong : DataSeries
    {
        public ChandelierLong(Bars bars, double Factor)
            : base(bars, "Chandelier Long")
        {
            int period = 14;	// ATR lookback
            for (int bar = period; bar < bars.Count; bar++)
                this[bar] = Highest.Value(bar, bars.High, period) - Factor * ATR.Value(bar, bars, period);
        }
        public static ChandelierLong Series(Bars bars, double Factor)
        {
            ChandelierLong _ChandelierLong = new ChandelierLong(bars, Factor);
            return _ChandelierLong;
        }
    }

    public class ATMAug2008_1 : WealthScript
    {
        private StrategyParameter paramMult;
        private StrategyParameter paramMode;
        private StrategyParameter paramChEx;

        public ATMAug2008_1()
        {
            paramMult = CreateParameter("Multiple", 1.5, 1, 4, 0.25);
            paramMode = CreateParameter("White/Johnson", 1, 1, 2, 1);

            /*
            paramMult:	A close X% of price higher than yesterday's close that 
                        fails to become a new 20-day highest close.
            paramMode: 	Selects between original "percentage change" (Adam White) 
                        and "ATR change" (Mark Johnson) approach.
            */

            paramChEx = CreateParameter("Chandelier mult.", 4, 2, 5, 0.25);
        }

        public bool shoulderette(int bar, double jumppct, int mode)
        {
            double jump_amount = -1;
            if (mode != 2)
                jump_amount = jumppct * (Close[bar] / 100);
            else
                jump_amount = jumppct * ATR.Series(Bars, 14)[bar];
            if ((Close[bar] > (Close[bar - 1] + jump_amount)) &
                (High[bar] < Highest.Value(bar, High, 20)))
                return true;
            else return false;
        }

        protected override void Execute()
        {
            double jumppct = paramMult.Value;
            int mode = paramMode.ValueInt;

            double factor = paramChEx.ValueInt;
            ChandelierLong cl = new ChandelierLong(Bars, factor);
            PlotStops();

            SetBarColors(Color.Silver, Color.Silver);

            for (int bar = 50; bar < Bars.Count; bar++)
            {
                if (shoulderette(bar, jumppct, mode))
                    SetBarColor(bar, Color.Blue);

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (!SellAtStop(bar + 1, p, cl[bar], "Chandelier"))
                    {
                        if (shoulderette(bar, jumppct, mode))
                            if (p.NetProfitAsOfBarPercent(bar) > 20)
                                SellAtLimit(bar + 1, p, Close[bar], "Shoulderette");
                    }
                }
                else
                {
                    if (BuyAtStop(bar + 1, High[bar - 20] + (4 * ATR.Series(Bars, 14)[bar])) != null)
                        LastPosition.Priority = -Close[bar];
                }
            }
        }
    }

    public class ATMAug2008_1Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the August 2008 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 1 strategy rules (trend following):</b></p><br>" +
                "<p>1. Go long the following day on a stop order at the highest 20-day close plus four 14-period ATRs if the current close exceeds the close of the past 20 days plus four 14-day ATRs.<br>" +
                "2. Close the position at the highest price registered in the position minus four 14-period ATRs." +
                "3. If a shoulderette pattern is detected after the position reaches a profit of 20 percent or more, close the position the following day at the previous day's closing price.</p><br>" +
                "<p>In Test 1, a shoulderette occurs when the closing price is 1.5 percent above yesterday's close " +
                "but the bar does not make a new 20-day high.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("5122f343-0978-490d-b6df-f2156b84817a");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2008-08 | Bearish shoulderette exit #1"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMAug2008_1); }
        }
    }

    public class ATMAug2008_2 : WealthScript
    {
        private StrategyParameter paramPeriod;
        private StrategyParameter paramMult;

        public ATMAug2008_2()
        {
            paramPeriod = CreateParameter("Period", 20, 5, 100, 5);
            paramMult = CreateParameter("Multiple", 1, 0.50, 1.50, 0.1);
        }

        protected override void Execute()
        {
            int period = paramPeriod.ValueInt;
            double jumppct = paramMult.Value;
            DataSeries jump_amount = jumppct * ATR.Series(Bars, 14);
            bool longSetup = false; bool shortSetup = false;

            SetBarColors(Color.Silver, Color.Silver);
            PlotSeries(PricePane, SMA.Series(Close, 100), Color.Blue, LineStyle.Solid, 1);
            for (int bar = 100; bar < Bars.Count; bar++)
            {
                longSetup = (Close[bar] > (Close[bar - 1] + jump_amount[bar])) &
                    (High[bar] < Highest.Value(bar, High, period)) &
                    (Close[bar] > SMA.Value(bar, Close, 100));
                shortSetup = (Close[bar] < SMA.Series(Close, 20)[bar]) &
                    (Close[bar] > (Close[bar - 1] + jump_amount[bar]) &
                    (High[bar] < Highest.Value(bar, High, period)));

                if (!IsLastPositionActive)
                {
                    if (longSetup)
                    {
                        SetBarColor(bar, Color.Blue);
                        if (BuyAtMarket(bar + 1, "Fade (long)") != null)
                            LastPosition.Priority = -RSI.Series(Close, 14)[bar];
                    }
                    else if (shortSetup)
                    {
                        SetBarColor(bar, Color.Blue);
                        if (ShortAtLimit(bar + 1, Close[bar], "Fade (short)") != null)
                            LastPosition.Priority = -RSI.Series(Close, 14)[bar];
                    }
                }
                else
                {
                    Position p = LastPosition;
                    if (p.PositionType == PositionType.Long)
                    {
                        if (CrossUnder(bar, SMA.Series(Close, 15), SMA.Series(Close, 30)))
                            SellAtMarket(bar + 1, p, "MA CrossUnder");
                    }
                    else
                    {
                        if (!CoverAtStop(bar + 1, p, p.EntryPrice * 1.05, "-5%"))
                            CoverAtLimit(bar + 1, p, p.EntryPrice * 0.94, "+6%");
                    }
                }
            }
        }
    }

    public class ATMAug2008_2Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the August 2008 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 2 strategy rules (long and short - fading the shoulderette):</b></p><br>" +
                "<p>1. Go long at the next market open if the closing price is higher than the 100-day simple moving average (SMA) and a shoulderette pattern occurs.<br>" +
                "2. Go short the following day at the current close if the closing price is lower than the 20-day SMA and a shoulderette pattern occurs.<br>" +
                "3. Close the long position the next day at market when the current day's 15-bar SMA crosses under the 30-period SMA.<br>" +
                "4. Close the short position either at the 5-percent stop loss or when the trade reaches 6-percent profit.<br>" +
                "<p>In Test 2, a shoulderette appears when a closing price is one 14-period ATR above yesterday's close but fails to become the new highest close of the past 20 days.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("e041ee5b-1bdf-401d-94c7-c525cc33a94f");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2008-08 | Bearish shoulderette exit #2"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMAug2008_2); }
        }
    }

    public class ATMSep2008_1 : WealthScript
    {
        private StrategyParameter paramX;
        private StrategyParameter paramPeriod;
        private StrategyParameter paramLookback;

        public ATMSep2008_1()
        {
            paramX = CreateParameter("X ATRs", 4.5, 0.5, 5, 0.5);
            paramPeriod = CreateParameter("n-Period High", 20, 5, 50, 5);
            paramLookback = CreateParameter("ATR Period", 14, 5, 30, 1);
        }

        protected override void Execute()
        {
            double TopStop = 0;
            double X = paramX.Value;
            int period = paramPeriod.ValueInt;
            int lookback = paramLookback.ValueInt;

            /* The distance from price and Daily adjustment value */
            DataSeries Distance = ATR.Series(Bars, lookback) * X;
            DataSeries Adjustment = DataSeries.Abs(Close - (Close >> 1));
            Distance.Description = "Distance";
            Adjustment.Description = "Adjustment";
            DataSeries dpa = Distance + Adjustment;
            DataSeries dma = Distance - Adjustment;

            string ClickedSym = Bars.Symbol;
            SetBarColors(Color.Silver, Color.Silver);
            HideVolume();

            for (int bar = 100; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;

                    if (bar == p.EntryBar) TopStop = p.EntryPrice + Distance[bar];
                    else
                    {
                        if (bar == HighestBar.Series(Close, period)[bar])
                        {
                            TopStop += Adjustment[bar];
                            double[] upBarRectangle = { bar, High[bar]+0.01, bar, TopStop, 
								bar+1, TopStop, bar+1, High[bar]+0.01 };
                            DrawPolygon(PricePane, Color.Transparent, Color.FromArgb(40, 50, 230, 50),
                                LineStyle.Solid, 2, true, upBarRectangle);
                            DrawCircle(PricePane, 3, bar, TopStop, Color.FromArgb(90, 50, 230, 50),
                                Color.FromArgb(90, 50, 230, 50), LineStyle.Solid, 1, false);
                        }
                        else
                        {
                            TopStop -= Adjustment[bar];
                            double[] downBarRectangle = { bar, High[bar]+0.01, bar, TopStop, 
								bar+1, TopStop, bar+1, High[bar]+0.01 };
                            DrawPolygon(PricePane, Color.Transparent, Color.FromArgb(40, 230, 50, 50),
                                LineStyle.Solid, 2, true, downBarRectangle);
                            DrawCircle(PricePane, 3, bar, TopStop, Color.FromArgb(90, 230, 50, 50),
                                Color.FromArgb(90, 230, 50, 50), LineStyle.Solid, 1, false);
                        }
                    }
                    SellAtLimit(bar + 1, p, TopStop);
                }
                else
                {
                    if (Close[bar] > SMA.Value(bar, Close, 100))
                        if (CumDown.Value(bar, Close, 1) >= 3)
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = -RSI.Series(Close, 14)[bar];
                }
            }
        }
    }

    public class ATMSep2008_1Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 8, 25); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the September 2008 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 1 strategy rules (trend following):</b></p><br>" +
                "<p>1. Go long tomorrow at the market when today's closing price is higher than the 100-day simple moving average (SMA).<br>" +
                "2. Place a limit order to exit the position at 4.5 times the 14-day ATR plus the absolute value of today's closing price minus yesterday's closing price.<br>" +
                "3. If the closing price is the highest of the past 20 days, raise the exit by the absolute value of today's closing price minus yesterday's closing price. If price fails to make a 20-day high close, lower it by the same amount.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("3e057e10-c030-4e30-b45a-8747105cc356");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 8, 25); }
        }

        public override string Name
        {
            get { { return "ATM 2008-09 | Top Stop exit #1 (trend-following)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMSep2008_1); }
        }
    }

    public class ATMSep2008_2 : WealthScript
    {
        private StrategyParameter paramX;
        private StrategyParameter paramPeriod;
        private StrategyParameter paramLookback;

        public ATMSep2008_2()
        {
            paramX = CreateParameter("X ATRs", 2.5, 0.5, 5, 0.5);
            paramPeriod = CreateParameter("n-Period High", 10, 5, 50, 5);
            paramLookback = CreateParameter("ATR Period", 10, 5, 30, 1);
        }

        protected override void Execute()
        {
            double TopStop = 0;
            double X = paramX.Value;
            int period = paramPeriod.ValueInt;
            int lookback = paramLookback.ValueInt;

            /* The distance from price and Daily adjustment value */
            DataSeries Distance = ATR.Series(Bars, lookback) * X;
            DataSeries Adjustment = DataSeries.Abs(Close - (Close >> 1));
            Distance.Description = "Distance";
            Adjustment.Description = "Adjustment";
            DataSeries dpa = Distance + Adjustment;
            DataSeries dma = Distance - Adjustment;

            string ClickedSym = Bars.Symbol;
            SetBarColors(Color.Silver, Color.Silver);

            for (int bar = Bars.FirstActualBar + 4; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (bar == p.EntryBar) TopStop = p.EntryPrice + Distance[bar];
                    else
                    {
                        if (bar == HighestBar.Series(Close, period)[bar])
                        {
                            TopStop += Adjustment[bar];
                            double[] upBarRectangle = { bar, High[bar]+0.01, bar, TopStop, 
								bar+1, TopStop, bar+1, High[bar]+0.01 };
                            DrawPolygon(PricePane, Color.Transparent, Color.FromArgb(40, 50, 230, 50),
                                LineStyle.Solid, 2, true, upBarRectangle);
                            DrawCircle(PricePane, 3, bar, TopStop, Color.FromArgb(90, 50, 230, 50),
                                Color.FromArgb(90, 50, 230, 50), LineStyle.Solid, 1, false);
                        }
                        else
                        {
                            TopStop -= Adjustment[bar];
                            double[] downBarRectangle = { bar, High[bar]+0.01, bar, TopStop, 
								bar+1, TopStop, bar+1, High[bar]+0.01 };
                            DrawPolygon(PricePane, Color.Transparent, Color.FromArgb(40, 230, 50, 50),
                                LineStyle.Solid, 2, true, downBarRectangle);
                            DrawCircle(PricePane, 3, bar, TopStop, Color.FromArgb(90, 230, 50, 50),
                                Color.FromArgb(90, 230, 50, 50), LineStyle.Solid, 1, false);
                        }
                    }
                    SellAtLimit(bar + 1, p, TopStop);
                }
                else
                {
                    double priority = High[bar];
                    if (Low[bar] < (High[bar - 4] * 0.9))
                        if (BuyAtMarket(bar + 1) != null)
                            LastPosition.Priority = priority;
                }
            }
        }
    }

    public class ATMSep2008_2Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 8, 25); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the September 2008 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 2 strategy rules (countertrend):</b></p><br>" +

                "<p>1. Go long tomorrow at the market when today's low is more than 10 percent below the high four days ago.<br>" +
                "2. Place a limit order to exit the position at 2.5 times the 10-day ATR plus the absolute value of today's closing price minus yesterday's closing price.<br>" +
                "3. If the closing price is the highest of the past 10 days, raise the limit order by the absolute value of today's closing price minus yesterday's closing price. If it fails to make a 10-day high close, lower it by the same amount.";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("1f71453a-e9f9-4203-a07d-cb49212362ca");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 8, 25); }
        }

        public override string Name
        {
            get { { return "ATM 2008-09 | Top Stop exit #2 (countertrend)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMSep2008_2); }
        }
    }

    public class ATMOct2008_1 : WealthScript
    {
        private StrategyParameter paramThreshold;
        private StrategyParameter paramExitAfter;

        public ATMOct2008_1()
        {
            paramThreshold = CreateParameter("Threshold $", 20000000, 1000000, 50000000, 1000000);
            paramExitAfter = CreateParameter("Exit after, days", 30, 5, 50, 5);
        }

        protected override void Execute()
        {
            /*
          	Insider transactions series are not split-adjusted neither in WLP4 nor in WLP5.
			Therefore the need to create the adjusted series by multipling the insider trade volume 
			by an adjustment factor. This adjustment factor is created using the 'split' item, 
			so the insider trade volume would remain relative to the stock's split-adjusted volume.
          	*/

            DataSeries reverseAdjustment;
            DataSeriesOp.SplitReverseFactor(this, "split", out reverseAdjustment);
            DataSeries dsAdjusted = Close * reverseAdjustment;
            ChartPane adjPane = PricePane;
            if (Close.Description.Contains("Volume"))
            {
                dsAdjusted = Close / reverseAdjustment;
                adjPane = VolumePane;
            }

            // Now let's create the adjusted insider buying data series.

            IList<FundamentalItem> fListS = FundamentalDataItems("insider sell");
            DataSeries InsiderSellingSeries = new DataSeries(Bars, "Insider Selling");
            InsiderSellingSeries.Description = "Insider Selling $";

            for (int bar = 0; bar < Bars.Count; bar++)
            {
                double sumS = 0;
                foreach (FundamentalItem fi in fListS)
                {
                    if (fi.Bar == bar)
                    {
                        sumS += fi.Value;
                        InsiderSellingSeries[bar] = sumS * dsAdjusted[bar];
                    }
                }
            }

            ChartPane iS = CreatePane(20, true, true);
            int high = 200;

            DataSeries isSumHighest = Highest.Series(InsiderSellingSeries, high);
            isSumHighest.Description = high.ToString() + "-day Highest Aggregated Insider Selling ($)";
            PlotSeries(iS, InsiderSellingSeries, Color.Red, LineStyle.Histogram, 2);
            PlotSeries(iS, isSumHighest, Color.Blue, LineStyle.Dashed, 2);
            // Split-adjusted price
            PlotSeries(adjPane, dsAdjusted, Color.FromArgb(50, Color.Blue), LineStyle.Solid, 1);

            // A dollar threshold for insider selling activity
            double threshold = paramThreshold.Value;
            // Time-based exit
            int daysToExit = paramExitAfter.ValueInt;

            for (int bar = high; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (bar + 1 - p.EntryBar >= daysToExit)
                    {
                        CoverAtMarket(bar + 1, p, "Timed");
                    }
                    else
                    {
                        if (!CoverAtStop(bar + 1, p, p.EntryPrice + ATR.Series(Bars, 14)[bar] * 3, "Loss 3*ATR"))
                            CoverAtLimit(bar + 1, p, p.EntryPrice - ATR.Series(Bars, 14)[bar] * 3, "Profit 3*ATR");
                    }
                }
                else
                {
                    if (InsiderSellingSeries[bar] > threshold)
                        if (InsiderSellingSeries[bar] >= isSumHighest[bar])
                            ShortAtStop(bar + 1, Low[bar]);

                }
            }
        }
    }

    public class ATMOct2008_1Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 9, 8); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the October 2008 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 1 strategy rules (Shorting with the insiders ):</b></p><br>" +
                "<p>1. Short on a stop tomorrow at today's low when insider selling is more than $20,000,000 and is equal to or greater than the highest level of the past 200 days.<br>" +
                "2. Cover short position on a stop equal to the entry price plus three times the 14-day average true range (ATR).<br>" +
                "3. Cover short position at a limit equal to the price of entry minus three times the 14-day ATR.<br>" +
                "4. Or exit the trade after 30 days.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("d6f57e73-2178-4b66-8c50-e1352686a0ca");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 9, 8); }
        }

        public override string Name
        {
            get { { return "ATM 2008-10 | Insider selling #1 (Shorting)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMOct2008_1); }
        }
    }

    public class ATMOct2008_2 : WealthScript
    {
        private StrategyParameter paramThreshold;
        private StrategyParameter paramExitAfter;

        public ATMOct2008_2()
        {
            paramThreshold = CreateParameter("Threshold $", 20000000, 1000000, 50000000, 1000000);
            paramExitAfter = CreateParameter("Exit after, days", 30, 5, 50, 5);
        }

        protected override void Execute()
        {
            /*
Insider transactions series are not split-adjusted neither in WLP4 nor in WLP5.
Therefore the need to create the adjusted series by multipling the insider trade volume 
by an adjustment factor. This adjustment factor is created using the 'split' item, 
so the insider trade volume would remain relative to the stock's split-adjusted volume.
*/

            DataSeries reverseAdjustment;
            DataSeriesOp.SplitReverseFactor(this, "split", out reverseAdjustment);
            DataSeries dsAdjusted = Close * reverseAdjustment;
            ChartPane adjPane = PricePane;
            if (Close.Description.Contains("Volume"))
            {
                dsAdjusted = Close / reverseAdjustment;
                adjPane = VolumePane;
            }

            // Now let's create the adjusted insider buying data series.
            IList<FundamentalItem> fListS = FundamentalDataItems("insider sell");
            DataSeries InsiderSellingSeries = new DataSeries(Bars, "Insider Selling");
            InsiderSellingSeries.Description = "Insider Selling $";

            for (int bar = 0; bar < Bars.Count; bar++)
            {
                double sumS = 0;
                foreach (FundamentalItem fi in fListS)
                {
                    if (fi.Bar == bar)
                    {
                        sumS += fi.Value;
                        InsiderSellingSeries[bar] = sumS * dsAdjusted[bar];
                    }
                }
            }


            int high = 200;
            ChartPane iS = CreatePane(20, true, true);
            DataSeries isSum = InsiderSellingSeries;
            DataSeries isSumHighest = Highest.Series(isSum, high);
            isSum.Description = "Insider Selling ($)";
            isSumHighest.Description = high.ToString() + "-day Highest Aggregated Insider Selling ($)";
            PlotSeries(iS, isSum, Color.Red, LineStyle.Histogram, 2);
            PlotSeries(iS, isSumHighest, Color.Blue, LineStyle.Dashed, 2);
            // Split-adjusted price
            PlotSeries(adjPane, dsAdjusted, Color.FromArgb(50, Color.Blue), LineStyle.Solid, 1);

            // Finally, some action!
            double threshold = paramThreshold.Value;
            int daysToExit = paramExitAfter.ValueInt;

            for (int bar = high; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (bar + 1 - p.EntryBar >= daysToExit)
                        SellAtMarket(bar + 1, p, "Timed");
                }
                else
                {
                    {
                        if (isSum[bar] > threshold)
                            if (isSum[bar] >= isSumHighest[bar])
                                if (BuyAtMarket(bar + 1) != null)
                                    LastPosition.Priority = Close[bar];
                    }
                }
            }
        }
    }

    public class ATMOct2008_2Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 9, 8); }
        }

        public override string Description
        {
            get
            {
                return
                "This strategy was featured in the October 2008 issue of <b>Active Trader magazine.</b><br>" +
                "<p><b>Test 2 strategy rules (Fading the insiders ):</b></p><br>" +
                "<p>1. Buy tomorrow at market when insider selling today is more than $20,000,000 and is equal to or greater than the highest level of the past 200 days.<br>" +
                "2. Exit long position after 30 days.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("941bcb1a-00ed-4540-a027-632fb251c872");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 9, 8); }
        }

        public override string Name
        {
            get { { return "ATM 2008-10 | Insider selling #2 (Fading)"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMOct2008_2); }
        }
    }

    public class ATMNov2008 : WealthScript
    {
        private StrategyParameter paramPeriod;
        private StrategyParameter paramThreshold;
        private StrategyParameter paramDecline;
        private StrategyParameter paramExitAfter;

        public ATMNov2008()
        {
            paramPeriod = CreateParameter("Time span, days", 50, 5, 300, 5);
            paramThreshold = CreateParameter("Threshold $", 300000, 100000, 3000000, 100000);
            paramDecline = CreateParameter("Decline %", 50, 10, 95, 5); //40
            paramExitAfter = CreateParameter("Exit after, days", 50, 5, 300, 5); //150|200
        }

        protected override void Execute()
        {

            /*
          	Insider transactions series are not split-adjusted neither in WLP4 nor in WLP5.
			Therefore the need to create the adjusted series by multipling the insider trade volume 
			by an adjustment factor. This adjustment factor is created using the 'split' item, 
			so the insider trade volume would remain relative to the stock's split-adjusted volume.
		    */

            DataSeries reverseAdjustment;
            DataSeriesOp.SplitReverseFactor(this, "split", out reverseAdjustment);
            DataSeries dsAdjusted = Close * reverseAdjustment;
            ChartPane adjPane = PricePane;
            if (Close.Description.Contains("Volume"))
            {
                dsAdjusted = Close / reverseAdjustment;
                adjPane = VolumePane;
            }
            dsAdjusted.Description = "Non-adjusted price";

            // Split-adjusted insider buying data series

            int period = paramPeriod.ValueInt;
            IList<FundamentalItem> fListB = FundamentalDataItems("insider buy");
            DataSeries InsiderBuyingSeries = new DataSeries(Bars, "Insider Buying");
            InsiderBuyingSeries.Description = "Insider Buying $";

            for (int bar = period; bar < Bars.Count; bar++)
            {
                double sumB = 0;
                foreach (FundamentalItem fi in fListB)
                {
                    if (fi.Bar == bar)
                    {
                        sumB += fi.Value;
                        InsiderBuyingSeries[bar] = sumB * dsAdjusted[bar];
                    }
                }
            }

            ChartPane iB = CreatePane(20, true, true);
            PlotSeries(adjPane, dsAdjusted, Color.FromArgb(50, Color.Blue), LineStyle.Solid, 1);
            DataSeries ibSum = Sum.Series(InsiderBuyingSeries, period);
            ibSum.Description = "Aggregated Insider Buying ($), Last " + period.ToString() + " Bars";
            PlotSeries(iB, ibSum, Color.Green, LineStyle.Histogram, 2);

            // Previous high
            DataSeries highest = Highest.Series(High, 250);
            highest.Description = "250-day high";
            PlotSeries(PricePane, highest, Color.FromArgb(90, Color.Blue), LineStyle.Dashed, 2);

            // Declined from the high by NN %
            DataSeries DeclineSeries = (100 - paramDecline.Value) / 100 * highest;
            DeclineSeries.Description = paramDecline.Value.ToString() + "% decline from the 250-day high";
            PlotSeries(PricePane, DeclineSeries, Color.Red, LineStyle.Dotted, 3);

            bool Setup = false;
            int SetupBar = -1;
            DataSeries atr = ATR.Series(Bars, 14);

            // Timeout:    Price hit the 250-day low within Y days, after which time the action is inhibited.
            int timeoutEntry = 50;
            double threshold = paramThreshold.Value;
            int daysToExit = paramExitAfter.ValueInt;

            for (int bar = paramPeriod.ValueInt; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (bar + 1 - p.EntryBar >= daysToExit)
                        SellAtMarket(bar + 1, p, "Timed");
                    else
                        if (!SellAtStop(bar + 1, p, p.EntryPrice * 0.75, "Stop -25%"))
                            SellAtTrailingStop(bar + 1, p, p.HighestHighAsOfBar(bar) - 10 * atr[bar], "10x ATR Stop");
                }
                else
                {
                    if (!Setup)
                    {
                        if (Low[bar] <= DeclineSeries[bar])
                        {
                            Setup = true;
                            SetupBar = bar;
                        }
                    }
                    if (Setup)
                    {
                        SetBackgroundColor(bar, Color.FromArgb(30, Color.Red));
                        if ((bar + 1 - SetupBar) < timeoutEntry)
                        {
                            if (ibSum[bar] > threshold)
                                if (BuyAtMarket(bar + 1) != null)
                                {
                                    Setup = false;
                                    LastPosition.Priority = -DeclineSeries[bar];
                                }
                        }
                        else
                            // Reset if setup has timed out
                            Setup = false;
                    }
                }
            }
        }
    }

    public class ATMNov2008Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2008, 10, 8); }
        }

        public override string Description
        {
            get
            {
                return
                "<p>This strategy was featured in the November 2008 issue of <b>Active Trader magazine.</b><br></p>" +

                "<p>This system adapts an idea from Anthony Gallea and William Patalon's book <i>Contrarian Investing: Buy and Sell When Others Won't " +
                "and Make Money Doing It</i>. Using a countertrend setup, this system buys a stock that is down significantly from its high over the past year, " +
                "adding insider transaction signals to take advantage of potential insider bargain hunting or <i>bottom fishing.</i></p>" +

                "<p><b>Note</b>: the strategy code uses Fidelity security sentiment data, therefore it is intended to be used in Wealth-Lab <b><font color=red>Pro</font></b> V5.x <b>only</b>.</p>" +

                "<p><b>Strategy rules:</b></p><br>" +
                "<p>1. <b>Enter long</b> tomorrow if the stock has declined at least 50 percent (determined by today's low price) from its 250-day high and has insider buying of at least $300,000 during the past 50 days. (Note: Insider buying is the aggregated dollar value of combined insider buy transactions on the open market of a security .)<br>" +
                "2. <b>Exit</b> the position with a stop-loss if stock price drops 25 percent from the entry price.<br>" +
                "3. <b>Exit</b> the position with a trailing stop placed 10 times the 14-day ATR from the highest price in the trade.<br>" +
                "4. <b>Exit</b> any open positions after 50 days tomorrow at the market.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("ef20dd04-36af-4d35-926e-ec87b818fa9c");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 10, 8); }
        }

        public override string Name
        {
            get { { return "ATM 2008-11 | Insider buying"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMNov2008); }
        }
    }
}
