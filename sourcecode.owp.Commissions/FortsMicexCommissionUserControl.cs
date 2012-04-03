using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;

namespace owp
{
    public partial class FortsMicexCommissionUserControl : UserControl
    {
        public FortsMicexCommissionUserControl()
        {
            InitializeComponent();
        }
        public Double F
        {
            get
            {
                return (double)this.forF.Value;
            }
            set {   
                this.forF.Text = value.ToString();
            }
        }
        public Double M
        {
            get
            {
                return (double)this.forM.Value;
            }
            set
            {
                this.forM.Text = value.ToString();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://code.google.com/p/open-wealth-project/");
        }
    }
}
