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
        double up, down, highest, lowest, lastVolume, minSize, lastMinute, firstOpen;
        Quote q;
        DateTime rightnow, date1, date2;
        public StreamingProvider()
        {
            up = 7836;
            down = 7673;
            lastVolume = 65500;
            lastMinute = 44;
            firstOpen = 0;
            highest = down;
            lowest = up;
            rightnow = DateTime.Today;
            date1 = new DateTime(rightnow.Year, rightnow.Month, rightnow.Day, 8, 45, 0);
            date2 = new DateTime(rightnow.Year, rightnow.Month, rightnow.Day, 13, 45, 59);
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
            bars = new Bars(symbol, BarScale.Second, rayProvider.barinterval);
            barsNew = new Bars(symbol, BarScale.Tick, 1);
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
            rightnow = DateTime.Now;

            if (DateTime.Compare(rightnow, date1) >= 0)
            {
                int leftIndex = 0, rightIndex = 0;
                double hours = 0, minutes = 0, seconds = 0;
                bool somethinghappen = false;
                // Receive the response from the remote device.
                String receivedata = rayclient.rayreceive();
                q.Size = 0;
                q.Open = q.Price;
                leftIndex = receivedata.IndexOf("Hour:", leftIndex);

                while (leftIndex >= 0)
                {

                    leftIndex += "Hour:".Length;
                    hours = Double.Parse(receivedata.Substring(leftIndex, 2));
                    leftIndex = receivedata.IndexOf("Minute:", leftIndex) + "Minute:".Length;
                    minutes = Double.Parse(receivedata.Substring(leftIndex, 2));
                    minutes = (minutes == 00 ? 59 : minutes - 1);
                    leftIndex = receivedata.IndexOf("Second:", leftIndex) + "Second:".Length;
                    seconds = Double.Parse(receivedata.Substring(leftIndex, 2));

                    q.TimeStamp = DateTime.Today;
                    q.TimeStamp = q.TimeStamp.AddHours(hours);
                    q.TimeStamp = q.TimeStamp.AddMinutes(minutes);
                    q.TimeStamp = q.TimeStamp.AddSeconds(seconds);

                    if (lastMinute != minutes && false == somethinghappen)
                    {
                        barsNew.Add(q.TimeStamp, firstOpen, highest, lowest, q.Price, minSize);
                        rayProvider.LoadAndUpdateBars(ref bars, barsNew);
                        minSize = 0;
                        firstOpen = 0;
                        highest = down;
                        lowest = up;
                    }

                    leftIndex = receivedata.IndexOf("Price:", leftIndex) + "Price:".Length;
                    rightIndex = receivedata.IndexOf("#B", leftIndex);
                    String rayTemp = receivedata.Substring(leftIndex, rightIndex - leftIndex);
                    if (false == String.IsNullOrEmpty(rayTemp))
                    {
                        q.Price = Double.Parse(rayTemp);
                        q.PreviousClose = q.Open;
                        q.Open = q.Price;
                        if (0 == firstOpen)
                        {
                            firstOpen = q.Open;
                        }
                    }
                    leftIndex = rightIndex + 1;

                    leftIndex = receivedata.IndexOf("Bid:", leftIndex) + "Bid:".Length;
                    rightIndex = receivedata.IndexOf("$A", leftIndex);
                    rayTemp = receivedata.Substring(leftIndex, rightIndex - leftIndex);
                    if (false == String.IsNullOrEmpty(rayTemp))
                    {
                        q.Bid = Double.Parse(rayTemp);
                    }
                    leftIndex = rightIndex + 1;

                    leftIndex = receivedata.IndexOf("Ask:", leftIndex) + "Ask:".Length;
                    rightIndex = receivedata.IndexOf("%V", leftIndex);
                    rayTemp = receivedata.Substring(leftIndex, rightIndex - leftIndex);
                    if (false == String.IsNullOrEmpty(rayTemp))
                    {
                        q.Ask = Double.Parse(rayTemp);
                    }
                    leftIndex = rightIndex + 1;

                    q.Symbol = symbol;

                    leftIndex = receivedata.IndexOf("Volume:", leftIndex) + "Volume:".Length;
                    rightIndex = receivedata.IndexOf("(", leftIndex);
                    rayTemp = receivedata.Substring(leftIndex, rightIndex - leftIndex);
                    if (false == String.IsNullOrEmpty(rayTemp))
                    {
                        double nowVolume = Double.Parse(rayTemp);
                        q.Size = nowVolume - lastVolume;
                        minSize += q.Size;
                        lastVolume = nowVolume;
                    }
                    leftIndex = rightIndex + 1;

                    highest = Math.Max(highest, q.Price);
                    lowest = Math.Min(lowest, q.Price);
                    //Hearbeat(q.TimeStamp); // Зачем нужен данный метод?

                    //UpdateStreamingBar(symbol, 0, q.Open, highest, lowest, q.Open, q.Size, q.TimeStamp, "Ray");
                    UpdateMiniBar(q, q.Open, highest, lowest);
                    //UpdateQuote(q); // не устанавливает 
                    somethinghappen = true;

                    lastMinute = minutes;
                    leftIndex = receivedata.IndexOf("Hour:", leftIndex);

                }

                if (false == somethinghappen)
                {
                    if (DateTime.Compare(rightnow, date2) <= 0)
                    {
                        if (rightnow.Minute == date2.Minute && rightnow.Hour == date2.Hour)
                        {
                            q.TimeStamp = rightnow;
                            q.TimeStamp = q.TimeStamp.AddMinutes(-1);
                        }
                        //UpdateStreamingBar(symbol, 0, q.Open, highest, lowest, q.Open, q.Size, q.TimeStamp, "Ray");
                        UpdateMiniBar(q, q.Open, highest, lowest);
                        //UpdateQuote(q); // не устанавливает
                    }
                }

                rayclient.rayclean();
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