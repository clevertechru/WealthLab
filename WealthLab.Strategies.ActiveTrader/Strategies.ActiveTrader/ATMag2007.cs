using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;

namespace Strategies.ActiveTrader
{
    public class ATMJan2007 : WealthScript
    {
        protected override void Execute()
        {
            int bars, ProfitableClosesTrend, ProfitableClosesCounter;
            double stopLevel = 0;
            bars = 0;

            // Number of profitable closes, trend mode
            ProfitableClosesTrend = 20;
            // Number of profitable closes, counter-trend mode
            ProfitableClosesCounter = 8; for (int bar = 20; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    stopLevel = (LastPosition.PositionType == PositionType.Long) ? LastPosition.EntryPrice * 0.9 : LastPosition.EntryPrice * 1.1;
                    ExitAtStop(bar + 1, LastPosition, stopLevel, "Stop Loss");

                    if (Bars.Close[bar] > LastPosition.EntryPrice)
                        bars++;
                    if (LastPosition.EntrySignal == "Channel Breakout")
                    {
                        if (bars == ProfitableClosesTrend)
                            SellAtMarket(bar + 1, LastPosition, "Profitable Closes.Trend");
                    }
                    else
                        if (LastPosition.EntrySignal == "Counter Trend")
                        {
                            if (bars == ProfitableClosesCounter)
                                SellAtMarket(bar + 1, LastPosition, "Profitable Closes.CounterTrend");
                        }
                }
                else
                {
                    if ((BuyAtStop(bar + 1, Highest.Series(Bars.High, 20)[bar], "Channel Breakout") != null) ||
                    (BuyAtLimit(bar + 1, Lowest.Series(Bars.Low, 10)[bar], "Counter Trend") != null))
                        bars = 0;
                }
            }
        }
    }

    public class ATMJan2007Helper : StrategyHelper
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
                    "<p>This strategy was featured in the January 2007 issue of <b>Active Trader magazine</b>.<br></p>" +

                    "<p>Exiting after a certain number of profitable closes is a time-based technique designed to take profits when " +
                    "a market is moving in your direction. This long-only system combines simple trend and countertrend entry rules, " +
                    "each of which will be exited after different numbers of profitable closes. A weakness of this technique is that " +
                    "by factoring out price action, it does not adapt to changing market conditions.<br></p>" +

                    "<b>Trend entry rule: </b>Buy with a stop order on a breakout of the highest high of the past 20 days.<br>" +
                    "<b>Countertrend entry rule: </b>Buy with a limit order at the lowest low of the past 10 days.<br>" +
                    "<b>Trend exit rule: </b>Exit at the market on the next bar's open when the number of profitable closes (excluding the entry bar) reaches 20.<br>" +
                    "<b>Countertrend exit rule: </b>Exit at the market on the next bar's open when the number of profitable closes (excluding the entry bar) reaches eight.<br>" +
                    "<b>Stop-loss: </b>: Protect each trade with a stoploss order 10-percent below the entry price.";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("4372a019-e9c8-4f13-88d7-5d606b9e202d");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2007-01 | Exiting after profitable closes"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJan2007); }
        }
    }

    public class ATMMar2007 : WealthScript
    {
        protected override void Execute()
        {
            DataSeries NRTR_ATR = new DataSeries(Bars, "NRTR_ATR"); 
            int Period = 20;
            double Multiple = 2.0;
            int Trend = 0;
            double Reverse = 0;
            double HPrice = 0;
            double LPrice = 0;
            DataSeries K = EMA.Series(TrueRange.Series(Bars), Period, WealthLab.Indicators.EMACalculation.Modern) * Multiple;

            for (int bar = K.FirstValidValue * 3; bar < Bars.Count; bar++)
            {
                // Calculate NRTR_ATR Series
                if (Trend >= 0)
                {
                    HPrice = Math.Max(Close[bar], HPrice);
                    Reverse = HPrice - K[bar];

                    if (Bars.Close[bar] <= Reverse)
                    {
                        Trend = -1;
                        LPrice = Close[bar];
                        Reverse = LPrice + K[bar];
                    }
                }
                if (Trend <= 0)
                {
                    LPrice = Math.Min(Close[bar], LPrice);
                    Reverse = LPrice + K[bar];

                    if (Bars.Close[bar] >= Reverse)
                    {
                        Trend = 1;
                        HPrice = Close[bar];
                        Reverse = HPrice - K[bar];
                    }
                }
                NRTR_ATR[bar] = Reverse;
            }
            
            PlotSeries(PricePane, NRTR_ATR, Color.Teal, WealthLab.LineStyle.Dotted, 3);

            for (int bar = K.FirstValidValue * 3; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    // Exit rules
                    if (Close[bar] < NRTR_ATR[bar])
                        SellAtMarket(bar + 1, LastPosition, "NRTR_ATR Exit Long");

                }
                else
                {
                    // Entry rules
                    if (Close[bar] > NRTR_ATR[bar])
                        BuyAtMarket(bar + 1, "NRTR_ATR Long Entry");
                }
            }
        }
    }

    public class ATMMar2007Helper : StrategyHelper
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
                "<p>This strategy was featured in the March 2007 issue of <b>Active Trader magazine</b>.<br></p>" +
                "<p><b>System concept:</b>Basic premise of the system is to use a trailing reverse level to trigger trades" +
                "which is adaptive to volatility, whe price crosses over it - an idea made popular by Russian trader Konstantin Kopyrkin..<br></p>" +

                "<p><b>Trading rules:</b><br>" +
                "1. ATR look-back period = 20 and ATR multiplier (k) = 2.<br>" +
                "2. Establish long position next bar at market if the closing price crosses above the NRTR-ATR level.<br>" +
                "3. Sell long position next bar at market if closing price crosses under the NRTR-ATR level.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("5f3ea8e3-8935-4f27-8246-4ad0a0c89e75");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2007-03 | NRTR_ATR Intraday system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMMar2007); }
        }
    }

    public class ATMApr2007_1 : WealthScript
    {
        protected override void Execute()
        {
            double BoxTop, BoxBottom;
            bool BoxBottomDef, BoxTopDef, BoxDef, First;
            int BoxTopCount, BoxBotCount, BoxDays, LastBoxBar;
            WealthLab.LineStyle ls = WealthLab.LineStyle.Solid;
            First = true;
            BoxDays = 2;
            LastBoxBar = 0;

            // my initialization
            BoxTop = Bars.High[0];
            BoxBottom = Bars.Low[0];
            BoxTopCount = BoxBotCount = 0;
            BoxTopDef = BoxBottomDef = BoxDef = false; for (int bar = 5; bar < Bars.Count; bar++)
            {
                if (First == true)
                {
                    BoxTop = Bars.High[bar];
                    First = false;
                    BoxTopCount = 0;
                    BoxBotCount = 0;
                    BoxBottomDef = false;
                    BoxTopDef = false;
                    LastBoxBar = bar;
                } if ((BoxDef == false) & (Bars.High[bar] > BoxTop))
                {
                    BoxTopCount = 0;
                    BoxTop = Bars.High[bar];
                    // Resets BoxBotCount if New High is made again
                    BoxBotCount = 0;
                } if ((BoxDef == false) & (Bars.High[bar] <= BoxTop))
                    BoxTopCount++; if ((BoxDef == false) & (BoxTopCount == BoxDays) & (Bars.High[bar] <= BoxTop))
                    BoxTopDef = true; if ((BoxDef == false) & (BoxTopDef == true) & (BoxBottomDef == false) & (BoxTopCount >= BoxDays + 1))
                {
                    if (BoxBotCount == 0)
                        BoxBottom = Bars.Low[bar]; if (Bars.Low[bar] < BoxBottom)
                    {
                        BoxBotCount = 0;
                        BoxBottom = Bars.Low[bar];
                    }

                    if (Bars.Low[bar] >= BoxBottom)
                        BoxBotCount++; if (BoxBotCount == BoxDays + 1)
                        BoxBottomDef = true;
                }
                // test				
                if ((BoxDef == false) & (BoxTopDef == true))
                {
                    if (Bars.High[bar] > BoxTop)
                        First = true;
                } if ((BoxDef == false) & (BoxBottomDef == true) & (BoxTopDef == true))
                {
                    BoxDef = true;
                    BoxBottomDef = false;
                    BoxTopDef = false;
                }
                // Darvas Box defined				
                if (BoxDef == true)
                {
                    DrawLine(PricePane, bar, BoxTop, LastBoxBar, BoxTop, Color.Green, ls, 2);
                    DrawLine(PricePane, bar, BoxBottom, LastBoxBar, BoxBottom, Color.Red, ls, 2); if (Bars.High[bar] > BoxTop)
                    {
                        BoxDef = false;

                        if (!IsLastPositionActive)
                            BuyAtStop(bar + 1, BoxTop); DrawLine(PricePane, LastBoxBar, BoxBottom, LastBoxBar, BoxTop, Color.Green, ls, 2);
                        DrawLine(PricePane, bar, BoxBottom, bar, BoxTop, Color.Green, ls, 2); First = true;
                        BoxTop = Bars.High[bar];
                    } if (Bars.Low[bar] < BoxBottom)
                    {
                        BoxDef = false;

                        if (IsLastPositionActive)
                            SellAtStop(bar + 1, LastPosition, BoxBottom); DrawLine(PricePane, LastBoxBar, BoxBottom, LastBoxBar, BoxTop, Color.Green, ls, 2);
                        DrawLine(PricePane, bar, BoxBottom, bar, BoxTop, Color.Red, ls, 2);
                        First = true;
                        BoxTop = Bars.High[bar];
                    }
                }
                // second true				
                if (First == true)
                {
                    BoxTop = Bars.High[bar];
                    First = false;
                    BoxTopCount = 1;
                    BoxBotCount = 0;
                    BoxBottomDef = false;
                    BoxTopDef = false;
                    LastBoxBar = bar;
                }
            }
        }
    }

    public class ATMApr2007_1Helper : StrategyHelper
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
                return "This strategy was featured in the April 2007 issue of <b>Active Trader magazine.</b><br>" +

                    "<p>Coded for WLD3 by <a href=http://wl4.wealth-lab.com/cgi-bin/WealthLab.DLL/profile?user=cclee>cclee</a> " +
                    "<a href=http://wl4.wealth-lab.com/cgi-bin/WealthLab.DLL/editsystem?id=51571>HERE</a>.</p><br>" +

                    "<p>System Concept: Nicolas Darvas was a successful professional dancer who also made millions " +
                    "in the stock market in the 50s using a charting method he described in his classic book How I Made $2,000,000 " +
                    "in the Stock Market. His basic approach (not including the stock selection process) is essentially a breakout technique.<br>" +

                    "<p>Darvas defined consolidations as 'boxes' and traded upside breakouts of these boxes. A Darvas Box is formed when both " +
                    "the box top and box bottom have been established, as follows:<br>" +

                    "* When a stock fails to make a new high for three days, the most recent high that is higher than the three subsequent highs becomes the box top.<br>" +
                    "* If price breaks out of the box top, repeat step 1.<br>" +
                    "* When a stock fails to make new lows after three days, the most recent low that is lower than the three subsequent lows becomes the box bottom.<br>" +
                    "* The box is broken when today's high price breaks out above the box top or below the box bottom.</p><br>" +

                    "<p><b>Trade rules:</b></p>" +

                    "<p>* Go long when the price exceeds the top of the Darvas Box.<br>" +
                    "* Sell when the price of the stock falls below the bottom of the Darvas Box.<br>" +

                    "The Darvas Box system has the benefit of being very simple and it has the potential to beat the market with proper money management.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("79601577-9d9e-4f28-bae4-ca8aacc9af3d");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 17); }
        }

        public override string Name
        {
            get { { return "ATM 2007-04 | Darvas Box"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMApr2007_1); }
        }
    }

    public class ATMApr2007_2 : WealthScript
    {
        protected override void Execute()
        {
            DataSeries RENKO_UP = new DataSeries(Bars, "Renko Up");
            DataSeries RENKO_DN = new DataSeries(Bars, "Renko Down");
            double UP_line = Bars.High[0];
            double DOWN_line = Bars.Low[0];
            double Mult = 2.0;
            double Brick = 0;
            int Period = 10; for (int bar = 1; bar < Bars.Count; bar++)
            {
                if (bar < 10)
                {
                    UP_line = Bars.High[bar];
                    DOWN_line = Bars.Low[bar];
                    Brick = Mult * (Bars.High[bar] - Bars.Low[bar]);
                }
                else
                    if (bar > 10)
                    {
                        Brick = Mult * ATR.Value(bar, Bars, Period);
                        if (Bars.Close[bar] > (UP_line + Brick))
                        {
                            UP_line += Brick;
                            DOWN_line = UP_line - Brick;
                        }
                        else
                            if (Bars.Close[bar] < (DOWN_line - Brick))
                            {
                                DOWN_line -= Brick;
                                UP_line = DOWN_line + Brick;
                            }
                    }
                RENKO_UP[bar] = UP_line;
                RENKO_DN[bar] = DOWN_line;
            }

            for (int bar = 20; bar < Bars.Count; bar++)
            {
                if (!IsLastPositionActive)
                {
                    // Entry rules
                    if (RENKO_UP[bar] > RENKO_UP[bar - 1])
                        BuyAtMarket(bar + 1, "Market"); if (RENKO_DN[bar] < RENKO_DN[bar - 1])
                        ShortAtMarket(bar + 1, "Market");
                }
                else
                {
                    // Exit rules
                    if ((RENKO_UP[bar] < RENKO_UP[bar - 1]) & LastPosition.PositionType == PositionType.Long)
                    {
                        SellAtMarket(bar + 1, Position.AllPositions, "Market");
                        ShortAtMarket(bar + 1, "Market");
                    } if ((RENKO_DN[bar] > RENKO_DN[bar - 1]) & LastPosition.PositionType == PositionType.Short)
                    {
                        CoverAtMarket(bar + 1, Position.AllPositions, "Market");
                        BuyAtMarket(bar + 1, "Market");
                    }
                }
            }			// Display Unit
            PlotSeries(PricePane, RENKO_UP, Color.Green, WealthLab.LineStyle.Solid, 2);
            PlotSeries(PricePane, RENKO_DN, Color.Red, WealthLab.LineStyle.Solid, 2);
            ChartPane ATRPane = CreatePane(20, false, false);
            PlotSeries(ATRPane, ATR.Series(Bars, 10), Color.Blue, WealthLab.LineStyle.Solid, 2);
            HideVolume();
        }
    }

    public class ATMApr2007_2Helper : StrategyHelper
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
                return "This strategy was featured in the April 2007 issue of <b>Active Trader magazine.</b><br>" +

                    "<p>Coded for WLD3 by <a href=http://wl4.wealth-lab.com/cgi-bin/WealthLab.DLL/profile?user=der_aspirant>der_aspirant</a> " +
                    "<a href=http://wl4.wealth-lab.com/cgi-bin/WealthLab.DLL/editsystem?id=18172>Renko Adaptive Avg-Up System v2.1</a></p><br>" +

                    "<p>Rules:<br>" +

                    "The brick size is two times the 10-day ATR.<br>" +
                    "Exit short and go long tomorrow at the market after a new white (up) brick forms.<br>" +
                    "Exit long and go short tomorrow at the market when a new black (down) brick forms.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("f42a4d4f-ea3b-4186-9811-441fdde1f557");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2007-04 | Adaptive Renko"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMApr2007_2); }
        }
    }

    public class ATMDec2007 : WealthScript
    {
        protected override void Execute()
        {
            // Dip size %
            double Dip = 0.5;
            // Stop Loss %
            double StopLoss = 3.2;
            int firstBar = 0;
            double prevClose = 0;
            // Check for intraday data
            if (Bars.IsIntraday)
            {
                // Get daily bars
                SetScaleDaily();
                Bars daily = Bars;
                RestoreScale();
                daily = Synchronize(daily);
                PlotSeries(PricePane, daily.Close, Color.Black, WealthLab.LineStyle.Solid, 1);

                for (int bar = 20; bar < Bars.Count; bar++)
                {
                    if (daily.IntradayBarNumber(bar) == 0)
                    {
                        if (daily.Date[bar] > daily.Date[bar - 1])
                        {
                            firstBar = bar;
                            prevClose = daily.Close[bar - 1];
                        }
                        if (bar == firstBar)
                            SetBarColor(bar, Color.Yellow);
                    }
                    if (IsLastPositionActive)
                    {
                        if (Bars.IsLastBarOfDay(bar))
                            SellAtClose(bar, LastPosition, "At Close");
                        SellAtStop(bar + 1, LastPosition, LastPosition.EntryPrice * (1 - StopLoss / 100), "Stop");
                    }
                    else
                    {
                        if (bar >= firstBar)
                            if (Bars.Close[bar] < (Bars.Low[bar - 1] * (1 - Dip / 100)))
                                if (Bars.Close[bar] > prevClose)
                                    BuyAtMarket(bar + 1, "Buy");
                    }
                }
            }
            else
                DrawLabel(PricePane, "For use on intraday data", Color.Red);
        }
    }

    public class ATMDec2007Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Volker Knapp"; } }
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
                    "<p>This strategy was featured in the December 2007 issue of <b>Active Trader magazine.</b><br></p>" +
                    "<p>The idea behind this system is to find opportune points at which to catch an intraday trend. " +
                    " It measures the change between five-minute bars to determine when a sharp, short-term move against " +
                    " the prevailing trend has occurred that is likely to be reversed. The goal is to enter when the market " +
                    "is actually in the process of making the pullback, rather than waiting for the pullback to complete and " +
                    "price to move beyond the most recent high (as do most breakout systems).</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("2b3058e6-206e-45fb-99fd-400011253f91");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2008, 6, 10); }
        }

        public override string Name
        {
            get { { return "ATM 2007-12 | Intraday pullback trader"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMDec2007); }
        }
    }
    /*
    public class ATMOct2007 : WealthScript
    {
        protected override void Execute()
        {
            try
            {
                //DataSeries Ratio = GetExternalSeries("TOTALPC", Close);
                DataSeries Ratio = GetExternalSymbol("TOTALPC", true).Close;

                for (int bar = 20; bar < Bars.Count; bar++)
                {
                    if (IsLastPositionActive)
                    {
                        Position p = LastPosition;
                        if (p.PositionType == PositionType.Long)
                        {
                            if (Ratio[bar] <= 0.6)
                                SellAtMarket(bar + 1, p, "Exit");
                        }
                        else
                        {
                            if (Ratio[bar] >= 1.0)
                                CoverAtMarket(bar + 1, p, "Cover");
                        }
                    }
                    else
                    {
                        if (Ratio[bar] >= 1.0)
                            BuyAtMarket(bar + 1, "Long");
                        if (Ratio[bar] <= 0.6)
                            ShortAtMarket(bar + 1, "Short");
                    }
                }

                ChartPane rPane = CreatePane(50, false, true);
                PlotSeries(rPane, Ratio, Color.Gray, LineStyle.Solid, 2, Ratio.Description);
                DrawHorzLine(rPane, 1.0, Color.Blue, LineStyle.Solid, 1);
                DrawHorzLine(rPane, 0.6, Color.Red, LineStyle.Solid, 1);
                HideVolume();
            }
            catch
            {
                DrawLabel(PricePane, "No data or could not find series", Color.Red);
                Abort();
            }
        }
    }

    public class ATMOct2007Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2007, 07, 07); }
        }

        public override string Description
        {
            get
            {
                return
                    "<p>This strategy was featured in the October 2007 issue of <b>Active Trader magazine.</b><br></p>" +
                    "<p>The idea behind this system is to fade extreme Put/Call Ratio levels.</p>" +
                    "<p><b>System rules</b></p>" +
                    "<p>1. Enter long tomorrow at the market if the CBOE total Put/Call Ratio is at or above 1.0.<br>" +
                    "2. Close long position tomorrow at the market when the ratio is at or below 0.6.<br>" +
                    "3. Enter short position tomorrow at the market if the CBOE total Put/Call Ratio is at or below 0.6.<br>" +
                    "4. Cover short position tomorrow at the market when the ratio is at or above 1.0.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("04047216-8807-4d32-bf6c-c0cc69633ca0");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2010, 05, 29); }
        }

        public override string Name
        {
            get { { return "ATM 2007-10 | CBOE Put/Call Ratio"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMOct2007); }
        }
    }
    */
}
