using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace OpenWealth.WLProvider
{
    public partial class WizardPage : UserControl
    {
        public WizardPage()
        {
            InitializeComponent();
        }
        public void Initialize(string dataPath)
        {
            // Clear entered symbols
            txtSymbols.Clear(); //TODO Загрузка настроек
        }

        public string Symbols() { return txtSymbols.Text; }

    }
}
