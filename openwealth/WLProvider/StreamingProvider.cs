﻿using System;
using System.Collections.Generic;
using System.Text;

using WealthLab;
using WealthLab.DataProviders.Helper;

namespace OpenWealth.WLProvider
{
    class StreamingProvider : StreamingDataProvider
    {
        System.Windows.Forms.Timer t;
        string symbol = string.Empty;
        private StaticProvider rayProvider;

        public StreamingProvider()
        {
            t = new System.Windows.Forms.Timer();
            t.Interval = 1000;
            t.Tick += new System.EventHandler(OnTimerEvent);
        }

        public override void ConnectStreaming(IConnectionStatus connStatus)
        {
            t.Enabled = true;
        }
        public override void DisconnectStreaming(IConnectionStatus connStatus)
        {
            t.Enabled = false;
        }
        public override StaticDataProvider GetStaticProvider()
        {
            rayProvider = new StaticProvider();
            return rayProvider;
        }

        protected override void Subscribe(string symbol)
        {
            this.symbol = symbol;
        }

        protected override void UnSubscribe(string symbol)
        {
            return;
        }


        private void OnTimerEvent(object sender, EventArgs e)
        {
            if (symbol == string.Empty)
                return;
            Random random = new Random();
            Quote q = new Quote();

            q.TimeStamp = DateTime.Now;

            
            q.Ask = 50*random.NextDouble()+50;
            q.Bid = q.Ask - 50*random.NextDouble();
            q.Open = 50*random.NextDouble()+50;
            q.PreviousClose = 50*random.NextDouble()+50;
            q.Price = 50*random.NextDouble()+50;
            q.Size = random.Next(50);
            q.Symbol = symbol;

            //Hearbeat(q.TimeStamp); // Why do we need this method?

            UpdateMiniBar(q, q.Open, q.Open + 10, q.Open - 10);
            
            //UpdateQuote(q); // does not establish
        }


        #region Descriptive

        public override string Description { get { return "Provides realtime stock data from OpenWealth"; } }
        public override string FriendlyName { get { return "OpenWealth"; } }
        public override System.Drawing.Bitmap Glyph { get { return Properties.Resources.Image1; } }
        public override bool IsConnected { get { return t.Enabled; } }
        public override bool StreamingAtDisconnect { get { return !t.Enabled; } }
        public override string URL { get { return "http://openwealth.ru/"; } }

        #endregion Descriptive

        /* в FidelityStreamingProvider not implemented

        public IConnectionStatus ConnectionStatus { get; }
        public IDataHost DataHost { get; }

        public void ClearRequests(IStreamingUpdate requestor);
        public virtual void DisconnectStreaming();
        public List<string> GetSymbolsSubscribed(IStreamingUpdate requestor);
        public virtual void Initialize(IDataHost dataHost);
        public bool IsSymbolStreaming(string symbol, IStreamingUpdate requestor);
        public static void SetBadTickFilterSettings(bool badTickFilter, double threshold);
        public void Subscribe(string symbol, IStreamingUpdate streamingUpdate);
        public void UnSubscribe(string symbol, IStreamingUpdate streamingUpdate);

        #region interface implementation IStreamingUpdate
        public void Hearbeat(DateTime timeStamp);
        public void UpdateMiniBar(Quote q, double open, double high, double low);
        public void UpdateQuote(Quote q);
        #endregion interface implementation IStreamingUpdate
 */
    }
}
