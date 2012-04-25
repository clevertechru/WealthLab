using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;

namespace Strategies.ActiveTrader
{
    public class CrossOverBar : DataSeries
    {
        public CrossOverBar(DataSeries ds1, DataSeries ds2, string description)
            : base(ds1, description)
        {
            bool Crossover = false;
            base[0] = -1;

            base.FirstValidValue = Math.Max(ds1.FirstValidValue, ds2.FirstValidValue);

            for (int bar = 1; bar < ds1.Count; bar++)
            {
                Crossover = ((ds1[bar] > ds2[bar]) & (ds1[bar - 1] <= ds2[bar - 1]));

                if (Crossover)
                    base[bar] = bar;
                else
                    base[bar] = base[bar - 1];
            }
        }

        public static CrossOverBar Series(DataSeries ds1, DataSeries ds2)
        {
            string description = string.Concat(new object[] { "CrossOverBar(", ds1.Description, ",", ds2.Description, ")" });
            if (ds1.Cache.ContainsKey(description))
            {
                return (CrossOverBar)ds1.Cache[description];
            }

            CrossOverBar _CrossOverBar = new CrossOverBar(ds1, ds2, description);
            ds1.Cache[description] = _CrossOverBar;
            return _CrossOverBar;
        }
    }

    public class CrossOverValueBar : DataSeries
    {
        public CrossOverValueBar(DataSeries ds, double value, string description)
            : base(ds, description)
        {
            bool Crossunder = false;
            base[0] = -1;

            base.FirstValidValue = ds.FirstValidValue;

            for (int bar = 1; bar < ds.Count; bar++)
            {
                Crossunder = ((ds[bar] > value) & (ds[bar - 1] <= value));

                if (Crossunder)
                    base[bar] = bar;
                else
                    base[bar] = base[bar - 1];
            }
        }

        public static CrossOverValueBar Series(DataSeries ds, double value)
        {
            string description = string.Concat(new object[] { "CrossOverValueBar(", ds.Description, ",", value.ToString(), ")" });
            if (ds.Cache.ContainsKey(description))
            {
                return (CrossOverValueBar)ds.Cache[description];
            }

            CrossOverValueBar _CrossOverValueBar = new CrossOverValueBar(ds, value, description);
            ds.Cache[description] = _CrossOverValueBar;
            return _CrossOverValueBar;
        }
    }

    public class CrossUnderBar : DataSeries
    {
        public CrossUnderBar(DataSeries ds1, DataSeries ds2, string description)
            : base(ds1, description)
        {
            bool Crossunder = false;
            base[0] = -1;

            base.FirstValidValue = Math.Max(ds1.FirstValidValue, ds2.FirstValidValue);

            for (int bar = 1; bar < ds1.Count; bar++)
            {
                Crossunder = ((ds1[bar] < ds2[bar]) & (ds1[bar - 1] >= ds2[bar - 1]));

                if (Crossunder)
                    base[bar] = bar;
                else
                    base[bar] = base[bar - 1];
            }
        }

        public static CrossUnderBar Series(DataSeries ds1, DataSeries ds2)
        {
            string description = string.Concat(new object[] { "CrossUnderBar(", ds1.Description, ",", ds2.Description, ")" });
            if (ds1.Cache.ContainsKey(description))
            {
                return (CrossUnderBar)ds1.Cache[description];
            }

            CrossUnderBar _CrossUnderBar = new CrossUnderBar(ds1, ds2, description);
            ds1.Cache[description] = _CrossUnderBar;
            return _CrossUnderBar;
        }
    }

    public class CrossUnderValueBar : DataSeries
    {
        public CrossUnderValueBar(DataSeries ds, double value, string description)
            : base(ds, description)
        {
            bool Crossunder = false;
            base[0] = -1;

            base.FirstValidValue = ds.FirstValidValue;

            for (int bar = 1; bar < ds.Count; bar++)
            {
                Crossunder = ((ds[bar] < value) & (ds[bar - 1] >= value));

                if (Crossunder)
                    base[bar] = bar;
                else
                    base[bar] = base[bar - 1];
            }
        }

        public static CrossUnderValueBar Series(DataSeries ds, double value)
        {
            string description = string.Concat(new object[] { "CrossUnderValueBar(", ds.Description, ",", value.ToString(), ")" });
            if (ds.Cache.ContainsKey(description))
            {
                return (CrossUnderValueBar)ds.Cache[description];
            }

            CrossUnderValueBar _CrossUnderValueBar = new CrossUnderValueBar(ds, value, description);
            ds.Cache[description] = _CrossUnderValueBar;
            return _CrossUnderValueBar;
        }
    }

    public class ATMJan2010 : WealthScript
    {
        private StrategyParameter paramBrk1;
        private StrategyParameter paramBrk2;
        private StrategyParameter paramWindow;
        private StrategyParameter paramExit;

        public ATMJan2010()
        {
            paramBrk1 = CreateParameter("1st breakout", 30, 6, 30, 2);
            paramBrk2 = CreateParameter("2nd breakout", 40, 10, 100, 2);
            paramWindow = CreateParameter("Window", 8, 2, 30, 2);
            paramExit = CreateParameter("Exit after", 30, 5, 50, 5);
        }

        protected override void Execute()
        {
            int waitBars = paramWindow.ValueInt;
            int exitBars = paramExit.ValueInt;
            int BreakoutPeriod = paramBrk1.ValueInt;
            int BreakoutPeriod2 = paramBrk2.ValueInt;

            DataSeries l1 = Lowest.Series(Close, BreakoutPeriod) >> 1;
            DataSeries h2 = Highest.Series(Close, BreakoutPeriod2) >> 1;
            DataSeries l2 = Lowest.Series(Close, BreakoutPeriod2) >> 1;
            DataSeries h1 = Highest.Series(Close, BreakoutPeriod) >> 1;
            ADX adx = ADX.Series(Bars, 30);

            l1.Description = String.Concat("Lowest Close of ", BreakoutPeriod.ToString(), " bars");
            h2.Description = String.Concat("Highest Close of ", BreakoutPeriod2.ToString(), " bars");
            l2.Description = String.Concat("Lowest Close of ", BreakoutPeriod2.ToString(), " bars");
            h1.Description = String.Concat("Highest Close of ", BreakoutPeriod.ToString(), " bars");

            PlotSeries(PricePane, l1, Color.Red, LineStyle.Dashed, 1);
            PlotSeries(PricePane, h2, Color.Blue, LineStyle.Solid, 1);
            PlotSeries(PricePane, l2, Color.Red, LineStyle.Solid, 1);
            PlotSeries(PricePane, h1, Color.Blue, LineStyle.Dashed, 1);
            PlotStops();

            SetBarColors(Color.Silver, Color.Silver);

            for (int bar = Math.Max(BreakoutPeriod2, waitBars); bar < Bars.Count; bar++)
            {
                if (bar == CrossUnderBar.Series(Close, l1)[bar])
                    SetBarColor(bar, Color.Red);
                if (bar == CrossOverBar.Series(Close, h2)[bar])
                    SetBarColor(bar, Color.Blue);

                if (IsLastPositionActive)
                {
                    Position p = LastPosition;

                    if (bar + 1 - p.EntryBar >= exitBars)
                        ExitAtMarket(bar + 1, p, "Timed");
                }
                else
                {
                    // Ensure we're not fading a strong trend
                    if (adx[bar] < 25)
                    {
                        if (CrossOver(bar, Close, h2) &&
                            (CrossUnderBar.Series(Close, l1)[bar] > (bar - waitBars)))
                            if (BuyAtMarket(bar + 1) != null)
                                LastPosition.Priority = -Close[bar];

                        if (CrossUnder(bar, Close, l2) &&
                            (CrossOverBar.Series(Close, h1)[bar] > (bar - waitBars)))
                            if (ShortAtMarket(bar + 1) != null)
                                LastPosition.Priority = Close[bar];
                    }
                }
            }
        }
    }

    public class ATMJan2010Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2009, 12, 7); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the January 2010 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b><br><br>Instead of simply fading a breakout move, the following shorter term system waits for a strong confirmation of a reversal - i.e., a downside breakdown after an upside breakout or vice versa.<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p>1. <b>Buy</b> the next bar at the open if the closing price is above the 40-period highest closing price no more than eight bars after a signal in the opposite direction (a close below the 30-period lowest close), and the 30-period ADX is 25 or lower.<br><br>" +
                "2. <b>Short</b> the next bar at the open if the closing price is below the 40-period lowest closing price no more than eight bars after a signal in the opposite direction (a close above the 30-period highest close), and the 30-period ADX is 25 or lower.<br><br>" +
                "3. <b>Exit</b> the position at the market after 30 bars.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("3098fd86-54b2-450b-b9ba-b8d2a69ef6c1");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2009, 12, 7); }
        }

        public override string Name
        {
            get { { return "ATM 2010-01 | Intraday false-breakout system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJan2010); }
        }
    }

    public class ATMFeb2010 : WealthScript
    {
        StrategyParameter paramStart;
        StrategyParameter paramStep;
        StrategyParameter strategyParameter3;
        StrategyParameter strategyParameter4;

        public ATMFeb2010()
        {
            paramStart = CreateParameter("RSI Start", 30, 20, 40, 10);
            paramStep = CreateParameter("RSI Step", 15, 15, 20, 5);
            strategyParameter3 = CreateParameter("Timeout", 300, 100, 300, 100);
            strategyParameter4 = CreateParameter("ATR Stop", 5, 3, 5, 1);
        }

        protected override void Execute()
        {
            RSI rsi14 = RSI.Series(Close, 14);
            ChartPane rsiPane = CreatePane(40, true, true);
            PlotSeries(rsiPane, rsi14, Color.Chocolate, WealthLab.LineStyle.Solid, 2);
            HideVolume();

            bool rsiEntry = false;
            double entryLevel = paramStart.Value; int step = paramStep.ValueInt;
            double exitLevel1 = entryLevel + step; double exitLevel2 = exitLevel1 + step;
            double exitLevel3 = exitLevel2 + step; double exitLevel4 = exitLevel3 + step;
            int canSplit = 4;
            bool scaleOut1 = false; bool scaleOut2 = false; bool scaleOut3 = false; bool scaleOut4 = false;

            DrawHorzLine(rsiPane, exitLevel1, Color.Blue, LineStyle.Dashed, 1);
            DrawHorzLine(rsiPane, exitLevel2, Color.Blue, LineStyle.Dashed, 1);
            DrawHorzLine(rsiPane, exitLevel3, Color.Blue, LineStyle.Dashed, 1);
            DrawHorzLine(rsiPane, exitLevel4, Color.Blue, LineStyle.Dashed, 1);

            for (int bar = rsi14.FirstValidValue * 3; bar < Bars.Count; bar++)
            {
                rsiEntry = rsi14[bar] <= entryLevel;

                if (ActivePositions.Count > 0)
                {
                    scaleOut1 = rsi14[bar] >= exitLevel1;
                    scaleOut2 = rsi14[bar] >= exitLevel2;
                    scaleOut3 = rsi14[bar] >= exitLevel3;
                    scaleOut4 = rsi14[bar] >= exitLevel4;

                    if (bar + 1 - ActivePositions[0].EntryBar >= strategyParameter3.ValueInt)
                        SellAtMarket(bar + 1, Position.AllPositions, "Timed");
                    else

                        if (!SellAtStop(bar + 1, Position.AllPositions, RiskStopLevel, "Stop"))
                        {
                            if (scaleOut1 && canSplit == 3)
                            {
                                Position s1 = SplitPosition(LastPosition, 24.99);
                                s1.Priority = Close[bar];
                                SellAtMarket(bar + 1, s1, "RSI " + exitLevel1.ToString());
                                canSplit--;
                            }
                            if (scaleOut2 && canSplit == 2)
                            {
                                Position s2 = SplitPosition(LastActivePosition, 24.99);
                                s2.Priority = Close[bar];
                                SellAtMarket(bar + 1, s2, "RSI " + exitLevel2.ToString());
                                canSplit--;
                            }
                            if (scaleOut3 && canSplit == 1)
                            {
                                Position s3 = SplitPosition(LastActivePosition, 24.99);
                                s3.Priority = Close[bar];
                                SellAtMarket(bar + 1, s3, "RSI " + exitLevel3.ToString());
                                canSplit--;
                            }
                            if (scaleOut4 && canSplit == 0)
                            {
                                SellAtMarket(bar + 1, LastActivePosition, "RSI " + exitLevel4.ToString());
                            }
                        }
                }
                else
                {
                    if (rsiEntry)
                    {
                        RiskStopLevel = Low[bar] - ATR.Series(Bars, 20)[bar] * strategyParameter4.Value;

                        if (BuyAtMarket(bar + 1, "Initial") != null)
                        {
                            Position p = LastPosition;
                            p.Priority = Close[bar];
                            double ep = p.EntryPrice; double er = RiskStopLevel;
                            canSplit = 3;
                        }
                    }
                }
            }
        }
    }

    public class ATMFeb2010Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2010, 1, 3); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the February 2010 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b><br><br>This system focuses on an exit technique that uses the relative strength index (RSI) to scale out of trades.<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p>1. <b>Buy</b> at tomorrow's open when the 14-period RSI goes below 30.<br><br>" +
                "2. <b>Exit</b> a quarter of the position at tomorrow's open each time the 14-period RSI increases by 15 points (e.g., 45, 60, 75, and 90).<br><br>" +
                "3. <b>Exit</b> the entire position when price falls below the entry price by five times the 20-day ATR.<br><br>" +
                "4. <b>Exit</b> the entire position at the market after 300 days of inactivity (i.e., no exit condition has triggered).</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("f45d541d-8ef5-4404-8318-518ffddf795a");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2010, 1, 3); }
        }

        public override string Name
        {
            get { { return "ATM 2010-02 | RSI scale-out system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMFeb2010); }
        }
    }

    public class iHolder
    {
        internal string symbol;
        internal double roc;
    }

    public class ATMMar2010 : WealthScript, IComparer<iHolder>
    {
        StrategyParameter numberOfSymbols;
		StrategyParameter rocPeriod;
		StrategyParameter paramDays;

        public int Compare(iHolder item1, iHolder item2)
        {
            return item1.roc.CompareTo(item2.roc);
        }

        public ATMMar2010()
        {
            numberOfSymbols = CreateParameter("n Symbols", 5, 2, 10, 1);
            rocPeriod = CreateParameter("ROC Period", 14, 6, 20, 2);
            paramDays = CreateParameter("Keep >= N days", 5, 2, 30, 1);
        }

        protected override void Execute()
        {
            //Clear cached symbol list so they can be re-requested with synchronization
            ClearExternalSymbols();
            int number = numberOfSymbols.ValueInt;
            int Period = rocPeriod.ValueInt;
            int days = paramDays.ValueInt;

            ChartPane rocPane = CreatePane(30, true, true);
            PlotSeries(rocPane, ROC.Series(Close, Period), Color.DarkRed, LineStyle.Solid, 2);
            HideVolume();

            //Execute rotation strategy
            List<iHolder> list = new List<iHolder>();
            for (int bar = Period * 2; bar < Bars.Count; bar++)
            {
                //Identify the Top-N symbols that have the lowest ROC
                list.Clear();
                foreach (string symbol in DataSetSymbols)
                {
                    SetContext(symbol, true);
                    if (Bars.FirstActualBar < bar)
                    {
                        iHolder holder = new iHolder();
                        holder.symbol = symbol;
                        holder.roc = ROC.Series(Bars.Close, Period)[bar];

                        // Average ROC
                        //holder.roc = ( ROC.Series(Bars.Close, 10)[bar] + ROC.Series(Bars.Close, 20)[bar] + ROC.Series(Bars.Close, 30)[bar] ) / 3;

                        // MACD
                        //holder.roc = MACD.Series(Bars.Close)[bar];

                        // TrendScore						
                        //holder.roc = TrendScore.Series(Bars)[bar];

                        // "Efficient" stocks i.e. steady, consistent, non-volatile
                        //holder.roc = ( ROC.Series(Bars.Close, 5)[bar] + ROC.Series(Bars.Close, 20)[bar] ) /
                        //( ATRP.Series(Bars, 5)[bar] + ATRP.Series(Bars, 20)[bar] );

                        list.Add(holder);
                    }
                    RestoreContext();
                }
                list.Sort(this);
                // Instead of Bottom-N, deal with Top-N stocks:
                //list.Reverse();

                //keep top number only
                int c = list.Count;
                for (int i = c - 1; i >= number; i--)
                    list.RemoveAt(i);

                //Close positions that are not in the new lowest N
                for (int pos = ActivePositions.Count - 1; pos >= 0; pos--)
                {
                    Position p = ActivePositions[pos];
                    bool keepPosition = false;
                    foreach (iHolder holder in list)
                        if (holder.symbol == p.Bars.Symbol)
                        {
                            keepPosition = true;
                            break;
                        }
                    if (!keepPosition)
                        // Hold for several days to minimize re-entries
                        if (bar + 1 - p.EntryBar >= days)
                            SellAtMarket(bar + 1, p);
                }

                //Buy new positions
                foreach (iHolder holder in list)
                {
                    bool buyPosition = true;
                    foreach (Position p in ActivePositions)
                        if (p.Bars.Symbol == holder.symbol)
                        {
                            buyPosition = false;
                            break;
                        }
                    if (buyPosition)
                    {
                        SetContext(holder.symbol, true);
                        if (BuyAtMarket(bar + 1) != null)
                            LastPosition.Priority = holder.roc;
                        RestoreContext();
                    }
                }
            }   
        }
    }

    public class ATMMar2010Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2010, 2, 4); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the March 2010 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b><br><br>This trading method actively rotates into weak stocks in the expectation they will rebound.<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p>1. <b>Go long</b> tomorrow at the open the five stocks that made the largest percentage declines over the past 14 days.<br><br>" +
                "2. <b>Hold</b> the stocks at least five days.<br><br>" +
                "3. <b>Sell</b> tomorrow at the open when a stock is no longer among the five biggest percentage decliners over the past 14 days.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("dbc813e6-c9b3-4a02-a534-7784374e6e35");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2010, 2, 4); }
        }

        public override string Name
        {
            get { { return "ATM 2010-03 | Weak-stock rotation"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMMar2010); }
        }
    }

    public class ATMApr2010 : WealthScript
    {
        protected override void Execute()
        {
            DataSeries maFast = EMAModern.Series(Close,20);
			DataSeries maSlow = EMAModern.Series(Close,50);
       
			int entries = 4;
			int cnt = 0;
			double lastEntryPrice = 0;
			
			for(int bar = maSlow.FirstValidValue*4; bar < Bars.Count; bar++)
			{
				for (int p = ActivePositions.Count - 1; p > -1; p-- )
				{
					Position pos = ActivePositions[p];
					lastEntryPrice = ActivePositions[ActivePositions.Count - 1].EntryPrice;
					bool priceGoesUp = ( High[bar] >= lastEntryPrice + ATR.Series(Bars,10)[bar]*3.0 );
					bool canPyramid = ( ActivePositions.Count < entries ) & priceGoesUp;
					
					if (CrossUnder(bar, maFast, maSlow))
					{
						if( SellAtClose(bar, Position.AllPositions, "MA XU") )
						{
							cnt = 0;
							canPyramid = false;
							break;
						}
					}
					else
						if( SellAtStop(bar+1, Position.AllPositions, lastEntryPrice - ATR.Series(Bars,10)[bar]*6, "6xATR Stop") )
					{
						cnt = 0;
						canPyramid = false;
						break;
					}
					
					if( canPyramid )
					{
						if( priceGoesUp ) 
							if( BuyAtMarket( bar+1, (cnt+1).ToString() ) != null )
							{
								lastEntryPrice = LastPosition.EntryPrice;
								LastPosition.Priority = -Close[bar];
								cnt++;
							}
					}
				}
				
				if ( ActivePositions.Count < 1 )
					if( Close[bar] > SMA.Series( Close,200 )[bar] )
						if (CrossOver(bar, maFast, maSlow))                               
							if( BuyAtClose(bar, (cnt+1).ToString()) != null )
							{
								lastEntryPrice = LastPosition.EntryPrice;
								LastPosition.Priority = -Close[bar];
								cnt++;
							}
			}
			
			PlotSeries( PricePane, maFast, Color.Red, LineStyle.Dashed, 2 );
			PlotSeries( PricePane, maSlow, Color.Blue, LineStyle.Solid, 2 );

        }
    }

    public class ATMApr2010Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2010, 2, 18); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the April 2010 issue of <b>Active Trader magazine.</b><br>" +

                "<p><h3><font color=red>Important:</font></h3></p>" +
                "<p>For proper results, this system should be tested together with the 'Pyramiding' PosSizer from MS123.PosSizers library.<br>" +
                "Portfolio backtesting results obtained without the 'Pyramiding' PosSizer might be incorrect!</p>" +

                "<p><b>System Concept:</b><br><br>This system tests one of scale-entry techniques: the 'reverse pyramid' technique, or 'averaging up,' as Perry Kaufman referred to it in his book 'Trading Systems & Methods'.<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p>1. <b>Buy</b> tomorrow's open when the 20-day exponential moving average (EMA) of closing prices crosses above the 50-day EMA of closing prices and the daily close is above the 200-day simple moving average (SMA) of closing prices.<br><br>" +
                "2. <b>Scale in</b> by adding to the position every time prices advance three times the 10-day ATR above the last entry price. Limit the number of total positions in a stock to four (i.e., scale in no more than three times).<br><br>" +
                "3. <b>Exit</b> the entire position at the close when the 20-day EMA of closing prices crosses below the 50-day EMA of closing prices.<br><br>" +
                "4. <b>Exit</b> the whole position with a stop at six times the 10-period ATR if price goes against the most recently entered position.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("faafd2f0-7f2b-4626-b36e-4c9c03ca5b88");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2010, 2, 18); }
        }

        public override string Name
        {
            get { { return "ATM 2010-04 | Volatility scale-in system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMApr2010); }
        }
    }

    public class ATMMay2010 : WealthScript
    {
        private StrategyParameter paramOB;
		private StrategyParameter paramOS;
		private StrategyParameter paramShouldStay;
		private StrategyParameter paramExitType; // 0 - normal, 1 - inverse, 2 - time-based
		private StrategyParameter paramExitBars;

        public ATMMay2010()
		{
			paramOB = CreateParameter("Overbought", 70, 65, 90, 5);
			paramOS = CreateParameter("Oversold", 30, 15, 35, 5);
			paramShouldStay = CreateParameter("Should stay", 8, 6, 10, 2);
			paramExitType = CreateParameter("Exit type", 0, 0, 2, 1);
			paramExitBars = CreateParameter("Exit after", 5, 1, 20, 1);
		}

        protected override void Execute()
        {
            double oversold = paramOS.Value;
            double overbot = paramOB.Value;
            int shouldStay = paramShouldStay.ValueInt;
            int exitStyle = paramExitType.ValueInt;
            RSI rsi = RSI.Series(Close, 2);

            DataSeries isAbove = Lowest.Series(rsi - overbot, shouldStay); isAbove.Description = "isAbove";
            DataSeries isBelow = Highest.Series(rsi - oversold, shouldStay); isBelow.Description = "isBelow";

            SetBarColors(Color.Silver, Color.Silver);
            ChartPane pRSI = CreatePane(30, true, true);
            PlotSeriesOscillator(pRSI, rsi, overbot, oversold, Color.Blue, Color.Red, Color.Black, LineStyle.Solid, 1);
            PlotStops(); HideVolume();

            bool setupLong = false; bool setupShort = false;

            for (int bar = rsi.FirstValidValue * 3; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;

                    if (exitStyle == 0)
                    {
                        if ((p.PositionType == PositionType.Long) & (CrossOver(bar, rsi, overbot)) |
                            (p.PositionType == PositionType.Short) & (CrossUnder(bar, rsi, oversold)))
                            ExitAtMarket(bar + 1, p);
                    }
                    else
                        if (exitStyle == 1)
                        {
                            if ((p.PositionType == PositionType.Long) & (CrossUnder(bar, rsi, overbot)) |
                                (p.PositionType == PositionType.Short) & (CrossOver(bar, rsi, oversold)))
                                ExitAtClose(bar, p);
                        }
                        else
                            if (exitStyle == 2)
                            {
                                if (bar + 1 - p.EntryBar >= paramExitBars.ValueInt)
                                    ExitAtMarket(bar + 1, p, "Timed");
                            }
                }

                {
                    if (!setupLong)
                    {
                        if (isAbove[bar] > 0)
                        {
                            SetBackgroundColor(bar, Color.FromArgb(20, Color.Blue));
                            setupLong = true;
                        }
                    }

                    if (setupLong)
                    {
                        if (CrossOver(bar, rsi, overbot))
                            setupLong = false;

                        if (CrossUnder(bar, rsi, oversold))
                        {
                            SetBackgroundColor(bar, Color.FromArgb(50, Color.Blue));
                            if (BuyAtMarket(bar + 1) != null)
                            {
                                setupLong = false;
                                continue;
                                LastPosition.Priority = -Close[bar];
                            }
                        }
                    }

                    if (!setupShort)
                    {
                        if (isBelow[bar] < 0)
                        {
                            SetBackgroundColor(bar, Color.FromArgb(20, Color.Red));
                            setupShort = true;
                        }
                    }

                    if (setupShort)
                    {
                        if (CrossUnder(bar, rsi, oversold))
                            setupShort = false;

                        if (CrossOver(bar, rsi, overbot))
                        {
                            SetBackgroundColor(bar, Color.FromArgb(50, Color.Red));
                            if (ShortAtMarket(bar + 1) != null)
                            {
                                setupShort = false;
                                continue;
                                LastPosition.Priority = Close[bar];
                            }
                        }
                    }
                }
            }
        }
    }

    public class ATMMay2010Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2010, 3, 17); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the May 2010 issue of <b>Active Trader magazine.</b><br>" +

                "<p><b>System Concept:</b><br><br>" +
                "This system tries to jump on temporary corrections and ride the resumption of the trend by exploiting the RSI's trait of remaining in overbought or oversold territory during strong trends - i.e., the tendency to produce prolonged "+
                "overbought/oversold readings. Brief exits from these conditions signal buying and selling opportunities." +

                "<p><b>Strategy rules:</b></p>" +

                "<p>1. <b>Buy</b> the next day at the market when the 2-day RSI crosses below 30 (the oversold level) after having spent 8 days or more above 70 (the overbought level).<br><br>" +
                "2. <b>Sell</b> at the market the next day on the opposite signal (an RSI cross above 70).<br><br>" +
                "3. <b>Short</b> the next day at the market when the 2-day RSI crosses above 70 after having spent 8 days or more below 30.<br><br>" +
                "4. <b>Cover</b> short at the market the next day on the opposite signal (an RSI cross below 30).</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("606f65db-9e92-4288-8863-a23a819ebf94");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2010, 3, 17); }
        }

        public override string Name
        {
            get { { return "ATM 2010-05 | Oscillator pullback system"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMMay2010); }
        }
    }
    /*
    public class ATMJun2010 : WealthScript
    {
        private StrategyParameter longChannelPer;
		private StrategyParameter shortChannelPer;

        public ATMJun2010()
		{
            longChannelPer = CreateParameter("Long Chnl Period", 80, 20, 100, 5);
            shortChannelPer = CreateParameter("Short Chnl Period", 20, 5, 50, 5);
		}

        protected override void Execute()
        {
            //Obtain periods from parameters
            int longPer = longChannelPer.ValueInt;
            int shortPer = shortChannelPer.ValueInt;

            DataSeries H1 = Highest.Series(High, longPer);
            DataSeries L1 = Lowest.Series(Low, longPer);
            DataSeries H2 = Highest.Series(High, shortPer);
            DataSeries L2 = Lowest.Series(Low, shortPer);

            // shift the plotted series to the right one bar to visualize the crossings
            PlotSeries(PricePane, H1 >> 1, Color.Green, LineStyle.Solid, 2);
            PlotSeries(PricePane, L2 >> 1, Color.Red, LineStyle.Solid, 1);
            PlotSeries(PricePane, H2 >> 1, Color.Blue, LineStyle.Solid, 1);
            PlotSeries(PricePane, L1 >> 1, Color.Fuchsia, LineStyle.Solid, 2);

            for (int bar = GetTradingLoopStartBar(longPer); bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position Pos = LastPosition;
                    if (Pos.PositionType == PositionType.Long)
                        SellAtStop(bar + 1, LastPosition, L2[bar]);
                    else
                        CoverAtStop(bar + 1, LastPosition, H2[bar]);
                }
                else
                {
                    RiskStopLevel = L2[bar];
                    if (BuyAtStop(bar + 1, H1[bar]) == null)
                    {
                        RiskStopLevel = H2[bar];
                        if (ShortAtStop(bar + 1, L1[bar]) != null)
                            LastPosition.Priority = -Close[bar];
                    }
                    else LastPosition.Priority = Close[bar];
                }
            }
        }
    }

    public class ATMJun2010Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2010, 4, 29); }
        }

        public override string Description
        {
            get
            {
                return "This strategy was featured in the June 2010 issue of <b>Active Trader magazine.</b><br>" +

                "<p><font color=red><b>Note:</b></font> This is nothing but a primitive channel breakout system for testing purposes.</p>" +

                "<p>The article reviews the money/risk-managemement method by Dr. Alexander Elder, implemented in " +
                "<a href=http://www2.wealth-lab.com/WL5WIKI/MS123PosSizersMain.ashx><b>MS123.PosSizers</b></a> library and available to registered Wealth-Lab 5" +
                "customers only. It can't be programmed in a Wealth-Lab Strategy.</p>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p>1. Go long with a stop order when price trades above the highest high of the past x days.<br>" +
                "2. Sell short with a stop order when price trades below the lowest low of the past x days.<br>" +
                "3. Exit long with a stop order when price trades below the lowest low of the past y days.<br>" +
                "4. Cover short with a stop order when price trades above the highest high of the past y days.</p>" +
                
                "<p><b>Money management:</b> Allocate 2 percent of account equity per trade.</p>" +
                
                "<p><b>Risk management:</b> Suspend trading until the next month when account equity falls 6 percent from the end of previous month.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("6077decb-5db0-4b92-949a-779d01ac7a26");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2010, 4, 29); }
        }

        public override string Name
        {
            get { { return "ATM 2010-06 | Risk management: Fighting off sharks and piranhas"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJun2010); }
        }
    }
    */
    public class ATMJul2010 : WealthScript
    {
        protected override void Execute()
        {
            Bars SPX = GetExternalSymbol("SPY", true);
            DataSeries SPX_Close = SPX.Close;
            SMA SPX_MA = SMA.Series(SPX_Close, 200);
            SPX_MA.Description = "200-day SMA of " + SPX.Symbol;
            DataSeries rsi2 = RSI.Series(Close, 2);

            ChartPane extPane = CreatePane(30, true, true); HideVolume();
            PlotSymbol(extPane, SPX, Color.Green, Color.Red);
            PlotSeries(extPane, SPX_MA, Color.Red, LineStyle.Solid, 1);
            ChartPane rsiPane = CreatePane(30, true, true);
            PlotSeries(rsiPane, rsi2, Color.Violet, LineStyle.Solid, 2);
            DrawHorzLine(rsiPane, 25, Color.Blue, LineStyle.Dashed, 1);
            DrawHorzLine(rsiPane, 35, Color.Black, LineStyle.Dashed, 1);
            DrawHorzLine(rsiPane, 65, Color.Black, LineStyle.Dashed, 1);
            DrawHorzLine(rsiPane, 75, Color.Blue, LineStyle.Dashed, 1);

            for (int bar = SPX_MA.FirstValidValue; bar < Bars.Count; bar++)
            {
                if (IsLastPositionActive)
                {
                    Position p = LastPosition;
                    if (p.PositionType == PositionType.Long)
                    {
                        if (rsi2[bar] > 75)
                            SellAtMarket(bar + 1, LastPosition);
                    }
                    else
                    {
                        if (rsi2[bar] < 25)
                            CoverAtMarket(bar + 1, LastPosition);
                    }
                }
                else
                {
                    bool above = SPX_Close[bar] > SPX_MA[bar];
                    bool day1 = rsi2[bar - 2] < 65;
                    bool day2 = rsi2[bar - 1] < rsi2[bar - 2];
                    bool day3 = rsi2[bar] < rsi2[bar - 1];

                    bool below = SPX_Close[bar] < SPX_MA[bar];
                    bool day1s = rsi2[bar - 2] > 35;
                    bool day2s = rsi2[bar - 1] > rsi2[bar - 2];
                    bool day3s = rsi2[bar] > rsi2[bar - 1];

                    if (above && day1 && day2 && day3)
                    {
                        if (BuyAtClose(bar) != null)
                            LastPosition.Priority = -Close[bar];
                    }
                    else
                        if (below && day1s && day2s && day3s)
                        {
                            if (ShortAtClose(bar) != null)
                                LastPosition.Priority = +Close[bar];
                        }
                }
            }
        }
    }

    public class ATMJul2010Helper : StrategyHelper
    {
        public override string Author
        {
            get { { return "Eugene."; } }
        }

        public override DateTime CreationDate
        {
            get { return new DateTime(2010, 6, 25); }
        }

        public override string Description
        {
            get
            {
                return "The Improved R2 strategy from the July 2010 issue of <b>Active Trader magazine</b> was created by Larry Connors. It was designed to capture pullbacks within an established trend, and incorporates a long-term trend filter (the S&P 500 index or its derivative staying above/below an SMA) and a shorter-term mean-reversion component (represented by an ultra-responsive 2-period RSI).<br>" +

                "<p><b>Strategy rules:</b></p>" +

                "<p>Setup for <b>long</b> trades:</p>" +

                "<p>1. SPY must be above its 200-day SMA.<br>" +
                "2. The two-day RSI for the individual stock or ETF to be traded must be below 65.<br>" +
                "3. If 1 and 2 are true, enter long on the close when the RSI declines for three consecutive days.<br>" +
                "4. Exit long next bar at open when the two-day RSI closes above 75.</p>" +
                
                "<p>Setup for <b>short</b> trades:</p>" +

                "<p>1. SPY must be below its 200-day SMA.<br>" +
                "2. The two-day RSI for the individual stock or ETF to be traded must be above 35.<br>" +
                "3. If 1 and 2 are true, enter short on the close when the RSI rises for three consecutive days.<br>" +
                "4. Cover short next bar at open when the two-day RSI closes below 25.</p>";
            }
        }

        public override Guid ID
        {
            get
            {
                return new Guid("03941468-cf8e-4640-b682-7e2bb0cda7ca");
            }
        }

        public override DateTime LastModifiedDate
        {
            get { return new DateTime(2010, 6, 25); }
        }

        public override string Name
        {
            get { { return "ATM 2010-07 | Improved R2 Strategy"; } }
        }

        public override Type WealthScriptType
        {
            get { return typeof(ATMJul2010); }
        }
    }


}