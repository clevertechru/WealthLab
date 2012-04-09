using System;
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
        StaticProvider rayProvider;
        Bars bars;
        Bars barsNew;
        AsynchronousClient rayclient;
        double lastMinute = 0, previousClose = 0, highest = 0, lowest = 0, firstOpen = 0;
        Quote q;

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
            bars = new Bars(symbol, BarScale.Daily, 0);
            barsNew = new Bars(symbol, BarScale.Tick, 0);
            rayclient = new AsynchronousClient(11000);
            q = new Quote();
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
            
            int leftIndex = 0, rightIndex = 0;
            double hours = 0, minutes = 0, seconds = 0;
            
            // Receive the response from the remote device.
            String receivedata = rayclient.rayreceive();
            
            leftIndex = receivedata.IndexOf("Hour:");
            
            if(leftIndex > 0)
            {
                hours = Double.Parse(receivedata.Substring(leftIndex, 2));
                leftIndex = receivedata.IndexOf("Minute:", leftIndex);
                minutes = Double.Parse(receivedata.Substring(leftIndex, 2));

                if (lastMinute != minutes)
                {
                    barsNew.Add(q.TimeStamp, firstOpen, highest, lowest, q.Price, q.Size);
                    rayProvider.LoadAndUpdateBars(ref bars, barsNew);
                }

                leftIndex = receivedata.IndexOf("Second:", leftIndex);
                seconds = Double.Parse(receivedata.Substring(leftIndex, 2));

                q.TimeStamp = DateTime.Today;
                q.TimeStamp.AddHours(hours);
                q.TimeStamp.AddMinutes(minutes);
                q.TimeStamp.AddSeconds(seconds);
            
                leftIndex = receivedata.IndexOf("Price:",leftIndex);
                rightIndex = receivedata.IndexOf("#B",leftIndex);
                String rayTemp = receivedata.Substring(leftIndex, rightIndex);
                if (false == String.IsNullOrEmpty(rayTemp))
                {
                    q.Price = Double.Parse(rayTemp);
                    q.Open = q.Price;
                    if (lastMinute != minutes)
                    {
                        firstOpen = q.Open;
                    }
                }
                leftIndex = rightIndex + 1;
                    
                leftIndex = receivedata.IndexOf("Bid:",leftIndex);
                rightIndex = receivedata.IndexOf("$A",leftIndex);
                rayTemp = receivedata.Substring(leftIndex, rightIndex);
                if (false == String.IsNullOrEmpty(rayTemp))
                {
                    q.Bid = Double.Parse(rayTemp);
                }
                leftIndex = rightIndex + 1;
            
                leftIndex = receivedata.IndexOf("Ask:",leftIndex);
                rightIndex = receivedata.IndexOf("%V",leftIndex);
                rayTemp = receivedata.Substring(leftIndex, rightIndex);
                if (false == String.IsNullOrEmpty(rayTemp))
                {
                    q.Ask = Double.Parse(rayTemp);
                }
                leftIndex = rightIndex + 1;

                q.PreviousClose = previousClose;
                q.Symbol = symbol;
            
                leftIndex = receivedata.IndexOf("Volume:",leftIndex);
                rightIndex = receivedata.IndexOf("^O",leftIndex);
                rayTemp = receivedata.Substring(leftIndex, rightIndex);
                if (false == String.IsNullOrEmpty(rayTemp))
                {
                    q.Size = Double.Parse(rayTemp);
                }
                leftIndex = rightIndex + 1;
            
                previousClose = q.Open;
                highest = Math.Max(highest, q.Price);
                lowest = Math.Min(lowest, q.Price);
                //Hearbeat(q.TimeStamp); // Зачем нужен данный метод?

                UpdateMiniBar(q, q.Open, highest, lowest);
                //UpdateQuote(q); // не устанавливает 

                lastMinute = minutes;
            }
        }


        #region Descriptive

        public override string Description { get { return "Provides realtime stock data from OpenWealth"; } }
        public override string FriendlyName { get { return "OpenWealth"; } }
        public override System.Drawing.Bitmap Glyph { get { return Properties.Resources.Image1; } }
        public override bool IsConnected { get { return t.Enabled; } }
        public override bool StreamingAtDisconnect { get { return !t.Enabled; } }
        public override string URL { get { return "http://openwealth.ru/"; } }

        #endregion Descriptive

        /* в FidelityStreamingProvider не реализованно

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

        #region реализация интерфейса IStreamingUpdate
        public void Hearbeat(DateTime timeStamp);
        public void UpdateMiniBar(Quote q, double open, double high, double low);
        public void UpdateQuote(Quote q);
        #endregion реализация интерфейса IStreamingUpdate
 */
    }
}
