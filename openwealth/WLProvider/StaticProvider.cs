using System;
using System.Collections.Generic;
using System.Net;

using WealthLab;
using WealthLab.DataProviders.Helper;

namespace OpenWealth.WLProvider
{
    /// <summary>
    /// WLProvider
    /// </summary>
    public class StaticProvider : StaticDataProvider
    {
        /// <summary>
        /// Class level variables
        /// </summary>

        Zaglushka zaglushka;

        private bool _cancelUpdate;
        private BarDataStore _dataStore;
        private IDataUpdateMessage _dataUpdateMsg;
        private bool _updating = false;

        /// Wizard pages
        private WizardPage Page;

        public override void Initialize(IDataHost dataHost)
        {
            base.Initialize(dataHost);
            this._dataStore = new BarDataStore(dataHost, this);
            this.zaglushka = new Zaglushka();
        }

        // Checking connection with server (not implemented here)
        public override void CheckConnectionWithServer()
        {
            base.CheckConnectionWithServer();
        }

        #region Descriptive

        public override string FriendlyName
        {
            get { return "OpenWealth"; }
        }

        public override string Description
        {
            get { return "Provides historical stock data from OpenWelth"; }
        }

        public override System.Drawing.Bitmap Glyph
        {
            get { return Properties.Resources.Image1; }
        }

        public override string URL
        {
            get { return @"http://www.OpenWealth.ru/"; }
        }

        #endregion

        #region Provider capabilities

        // Indicates that provider supports modifying dataset composition on-the-fly
        public override bool CanModifySymbols
        {
            get
            {
                return true;
            }
        }

        // Indicates that provider supports deleting symbols
        public override bool CanDeleteSymbolDataFile
        {
            get
            {
                return true;
            }
        }

        // Strategy Monitor support not implemented
        public override bool CanRequestUpdates
        {
            get
            {
                return false;
            }
        }

        // Должен вернуть True, если данный scale поддерживается
        // Вызывается, когда меняешь таймфрэйм через интерфейс у существующего символа
        public override bool SupportsDynamicUpdate(BarScale scale)
        {
            return true;
        }

        // Indicates that dataset updates are supported by provider
        public override bool SupportsDataSourceUpdate
        {
            get
            {
                return true;
            }
        }

        // Indicates that provider updates are supported ("Update all data for..." in the Data Manager)
        public override bool SupportsProviderUpdate
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Wizard pages

        public override System.Windows.Forms.UserControl WizardFirstPage()
        {
            if (Page == null)
                Page = new WizardPage();

            Page.Initialize(_dataStore.RootPath);

            return Page;
        }

        public override System.Windows.Forms.UserControl WizardNextPage(System.Windows.Forms.UserControl currentPage)
        {
            return null;
        }

        public override System.Windows.Forms.UserControl WizardPreviousPage(System.Windows.Forms.UserControl currentPage)
        {
            return null;
        }

        #endregion Wizard pages

        #region Implementing StaticDataProvider

        public override DataSource CreateDataSource()
        {
            if (this.Page == null)
                return null;

            DataSource ds = new DataSource(this);

            ds.DSString = Page.Symbols();

            // TODO Другие таймфрэймы
            ds.Scale = BarScale.Daily;
            ds.BarInterval = 0;

            return ds;
        }

        // TODO You can return a suggested DataSet name here if you like
        public override string SuggestedDataSourceName
        {
            get { return "OpenWealth"; }
        }

        public override string ModifySymbols(DataSource ds, List<string> symbols)
        {
            ds.DSString = symbols.ToString();
            return ds.DSString;
        }

        public override void DeleteSymbolDataFile(DataSource ds, string symbol)
        {
            this._dataStore.RemoveFile(symbol, ds.Scale, ds.BarInterval);
        }

        public override void PopulateSymbols(DataSource ds, List<string> symbols)
        {
            SymbolList symbolList = new SymbolList();
            symbolList.AddText(ds.DSString);
            symbols.AddRange(symbolList.Items);
        }

        public override Bars RequestData(DataSource ds, string symbol, DateTime startDate, DateTime endDate, int maxBars, bool includePartialBar)
        {
            Bars bars = new Bars(symbol.Trim(new char[] { ' ', '"' }), ds.Scale, ds.BarInterval);
            Bars barsNew;

            if (this._dataStore.ContainsSymbol(symbol, ds.Scale, ds.BarInterval))
            {
                if ((base.DataHost.OnDemandUpdateEnabled || this._updating) && this.UpdateRequired(ds, symbol))
                {
                    DateTime lastBar = this._dataStore.SymbolLastUpdated(symbol, BarScale.Daily, 0);

                    barsNew = zaglushka.RequestData(ds, symbol, startDate, endDate, maxBars, includePartialBar);

                    if (barsNew != null)
                        this.LoadAndUpdateBars(ref bars, barsNew);
                }

                _dataStore.LoadBarsObject(bars, startDate, DateTime.MaxValue, maxBars);
            }
            else
                if (base.DataHost.OnDemandUpdateEnabled || this._updating)
                {
                    bars = zaglushka.RequestData(ds, symbol, startDate, endDate, maxBars, includePartialBar);
                    this._dataStore.SaveBarsObject(bars);
                    this._dataStore.LoadBarsObject(bars);
                }

            return bars;
        }

        public override void UpdateDataSource(DataSource ds, IDataUpdateMessage dataUpdateMsg)
        {
            this._dataUpdateMsg = dataUpdateMsg;
            this._cancelUpdate = false;
            this._updating = true;
            Bars barsNew; // The Bars object for a newly downloaded symbol, or just the new data for an existing/updated symbol

            /* Main loop */
            try
            {
                // User requested 'Cancel update'
                if (this._cancelUpdate) return;

                List<string> up2date = new List<string>();
                List<string> updateRequired = new List<string>();
                List<string> newSymbols = new List<string>();
                string sym;

                foreach (string s in ds.Symbols)
                {
                    sym = s;

                    if (!this.UpdateRequired(ds, sym))
                        up2date.Add(sym);
                    else if (this._dataStore.ContainsSymbol(sym, ds.Scale, ds.BarInterval))
                        updateRequired.Add(sym);
                    else
                        if (!this._dataStore.ContainsSymbol(sym, ds.Scale, ds.BarInterval))
                            newSymbols.Add(sym);

                }
                // For debugging:
                dataUpdateMsg.DisplayUpdateMessage(
                    "Up-to-date symbols: " + up2date.Count.ToString() + ", " +
                    "Update required for: " + updateRequired.Count.ToString() + ", " +
                    "New symbols: " + newSymbols.Count.ToString());

                /* 1. Symbols already up-to-date */

                // Process symbols which require no data update
                if (up2date.Count > 0)
                {
                    string alreadyUp2Date = null;
                    foreach (string str in up2date)
                        alreadyUp2Date += str + ",";

                    dataUpdateMsg.DisplayUpdateMessage("Symbols already up to date: " + alreadyUp2Date);
                    dataUpdateMsg.ReportUpdateProgress((up2date.Count * 100) / ds.Symbols.Count);
                }

                /* 2. Symbols to update */

                Bars bars1;

                if (updateRequired.Count > 0)
                {
                    foreach (string s in updateRequired)
                    {
                        try
                        {
                            if (!this._cancelUpdate)
                            {
                                bars1 = new Bars(s, BarScale.Daily, 0);

                                if (_dataStore.ContainsSymbol(s, ds.Scale, ds.BarInterval))
                                    _dataStore.LoadBarsObject(bars1);

                                barsNew = zaglushka.RequestData(ds, s, bars1.Date[bars1.Count - 1], DateTime.Now, int.MaxValue, true);
                                this.LoadAndUpdateBars(ref bars1, barsNew);
                            }
                            else
                                return;
                        }
                        catch (Exception e)
                        {
                            dataUpdateMsg.DisplayUpdateMessage("Error: " + e.Message);
                        }
                    }
                }

                DateTime maxValue = DateTime.MaxValue;
                string[] newSymbolsArray = new string[newSymbols.Count];

                if (newSymbols.Count > 0)
                {
                    foreach (string str in newSymbols)
                    {
                        DateTime timeOfNewSymbol = this._dataStore.SymbolLastUpdated(str, ds.Scale, ds.BarInterval); //.Date;
                        if (timeOfNewSymbol < maxValue)
                            maxValue = timeOfNewSymbol;
                    }
                }

                dataUpdateMsg.DisplayUpdateMessage("Updating...");

                /* 3. New symbols */

                // Load the Bars object from BarDataStore for updating
                Bars bars2;

                foreach (string s in newSymbols)
                {
                    try
                    {
                        if (!this._cancelUpdate)
                        {
                            bars2 = new Bars(s, ds.Scale, ds.BarInterval);
                            _dataStore.LoadBarsObject(bars2);

                            // Google doesn't provide company names, so we'll get them from Yahoo!
                            if (zaglushka.GetCompanyName(s) != null)
                                bars2.SecurityName = zaglushka.GetCompanyName(s);

                            // After some trial and error, I figured out that setting the starting date to 1/1/1971 allows to fetch all available data
                            barsNew = zaglushka.RequestData(ds, s, DateTime.MinValue, DateTime.Now, int.MaxValue, true);
                            dataUpdateMsg.DisplayUpdateMessage("Symbol: " + s + ", existing bars : " + bars2.Count + ", new bars: " + barsNew.Count);
                            this.LoadAndUpdateBars(ref bars2, barsNew);
                        }
                        else
                            return;
                    }
                    catch (Exception e)
                    {
                        dataUpdateMsg.DisplayUpdateMessage("Error: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                dataUpdateMsg.DisplayUpdateMessage("Error: " + e.Message);
            }
            finally
            {
                this._updating = false;
            }
        }

        public override void UpdateProvider(IDataUpdateMessage dataUpdateMsg, List<DataSource> dataSources, bool updateNonDSSymbols, bool deleteNonDSSymbols)
        {
            this._cancelUpdate = false;

            try
            {
                dataUpdateMsg.DisplayUpdateMessage("Updating daily data from Google Finance...");

                // User requested 'Cancel update'
                if (this._cancelUpdate) return;

                // NOTE: Since only Daily data is made available by Google, this code does not support multiple bar scales
                // If your vendor supports different bar scales, you will need to handle it
                // by looping for all existing bar data scales in BarDataStore!

                // Create a list of all "visible" symbols of the provider
                List<string> allVisibleSymbols = new List<string>();
                foreach (DataSource ds in dataSources)
                    foreach (string sym in ds.Symbols)
                        if (!allVisibleSymbols.Contains(sym))
                            allVisibleSymbols.Add(sym);

                // If 'Update Non DS Symbols' is selected in DM, need to build the *complete* symbol list (incl. which have been deleted from existing DataSets)
                List<string> allExistingSymbols = new List<string>();
                allExistingSymbols.AddRange(allVisibleSymbols);
                IList<string> existingSymbols = this._dataStore.GetExistingSymbols(BarScale.Daily, 0);
                if (updateNonDSSymbols)
                {
                    foreach (string str in existingSymbols)
                        if (!allExistingSymbols.Contains(str))
                            allExistingSymbols.Add(str);
                }

                // Create a virtual DataSource on-the-fly that contains the entire symbol list of the provider
                DataSource ds_ = new DataSource(this);
                ds_.BarDataScale = new BarDataScale(BarScale.Daily, 0);
                ds_.DSString = allExistingSymbols.ToString();

                // Update all by creating a virtual DataSet
                this._updating = true;
                this.UpdateDataSource(ds_, dataUpdateMsg);

                // Delete symbols not present in existing DataSets
                if (deleteNonDSSymbols)
                {
                    int num = 0;
                    string str3 = "";
                    foreach (string str in existingSymbols)
                    {
                        if (!allVisibleSymbols.Contains(str))
                        {
                            num++;
                            // It's important to specify the right BarScale/Interval here (see note above re: multiple bar data scales)
                            this._dataStore.RemoveFile(str, BarScale.Daily, 0);
                            if (str3 != "")
                            {
                                str3 += ", ";
                            }
                            str3 = str3 + str;
                        }
                    }
                    if (num > 0)
                    {
                        dataUpdateMsg.DisplayUpdateMessage("Deleted Symbols " + str3);
                        dataUpdateMsg.DisplayUpdateMessage("Deleted " + num + " Symbol data files");
                    }
                }
                dataUpdateMsg.DisplayUpdateMessage("");

            }
            catch (Exception e)
            {
                dataUpdateMsg.DisplayUpdateMessage("Error: " + e.Message);
            }
            finally
            {
                this._updating = false;
                _dataUpdateMsg = null;
            }

        }

        public override void CancelUpdate()
        {
            this._cancelUpdate = true;
        }

        #endregion Implementing StaticDataProvider

        #region Helper methods

        private bool UpdateRequired(DataSource ds, string symbol)
        {
            bool result = false;

            // Last update time of a symbol as reported by the helpful BarsDataStore
            DateTime updateTime = this._dataStore.SymbolLastUpdated(symbol, ds.Scale, ds.BarInterval);

            // Update is required when symbol's date isn't current
            // As Google's got no partial bar for today, let's not request today's data if the last updated bar is "yesterday":
            if (DateTime.Now.Date > updateTime.Date.AddDays(1))
                result = true;

            return true;// result;
        }

        public void LoadAndUpdateBars(ref Bars bars, Bars barsUpdate)
        {
            this._dataStore.LoadBarsObject(bars);
            bars.Append(barsUpdate);
            this._dataStore.SaveBarsObject(bars);
        }

        #endregion Helper methods

    }
}