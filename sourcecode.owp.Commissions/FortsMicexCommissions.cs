using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

using WealthLab;
using Fidelity.Components;

namespace owp
{
    public class FortsMicexCommissions : Commission, ICustomSettings

    {
        double F = 0;
        double M = 0;

        public override double Calculate(TradeType tradeType, OrderType orderType, double orderPrice, double shares, Bars bars)
        {
            return F * shares + (M / 100) * (shares * orderPrice);
        }
        
        public override string Description
        {
            get
            {
                return F+" рублей за контракт в одну сторону + "+M+" % от суммы сделки.";
            }
        }

        public override string FriendlyName
        {
            get
            {
                return "FORTS MICEX";
            }
        }


        public void ChangeSettings(UserControl ui)
        {
            FortsMicexCommissionUserControl settings = ui as FortsMicexCommissionUserControl;
            this.F = settings.F;
            this.M = settings.M;
        }

        public UserControl GetSettingsUI()
        {
            FortsMicexCommissionUserControl settings = new FortsMicexCommissionUserControl();
            settings.F = this.F;
            settings.M = this.M;
            return settings;
        }

        public void ReadSettings(ISettingsHost host)
        {
            this.F = host.Get("FortsMicexCommissions.F", (double)1);
            this.M = host.Get("FortsMicexCommissions.M", (double)0);
        }

        public void WriteSettings(ISettingsHost host)
        {
            host.Set("FortsMicexCommissions.F", this.F);
            host.Set("FortsMicexCommissions.M", this.M);
        }
    }
}