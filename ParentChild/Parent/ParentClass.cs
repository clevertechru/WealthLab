using System;
using System.Collections.Generic;
using System.Text;

namespace Parent
{
    public abstract class ParentClass : WealthLab.WealthScript
    {
        public int FirstValidBar = -1;
        public int bar           = -1;
        protected override void Execute()
        {
            if (FirstValidBar == -1) { PrintDebug("Значение FirstValidBar не определено"); }
            else
            bar = FirstValidBar;
            for (bar = FirstValidBar; bar < Bars.Count; bar++)
            {
                ExecBar();
            }           
        }
        public abstract void ExecBar();
    }
}
