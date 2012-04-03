using System;
using System.Collections.Generic;
using System.Text;

namespace Child
{
    public class ChildClass: Parent.ParentClass
    {
        public ChildClass()
        {
            FirstValidBar = 0;
        }
        public override void ExecBar()
        {
            if (IsLastPositionActive)
            {
                SellAtMarket(bar + 1, LastPosition);
            }
            if (bar == (Bars.Count - 3))
            {
                 BuyAtMarket(bar + 1);
            }
        }
    }

    public class ChildClassHelper : WealthLab.StrategyHelper
    {
        public override string Author { get { return "open-wealth-project"; } }
        public override DateTime CreationDate { get { return new DateTime(2010, 04, 23); } }
        public override string Description { get { return "<a href=http://code.google.com/p/open-wealth-project/>http://code.google.com/p/open-wealth-project/</a>"; } }
        public override Guid ID { get { return new Guid("871416ad-737e-4f9d-a74e-f8c3b2f5237d"); } }
        public override DateTime LastModifiedDate { get { return new DateTime(2010, 04, 23); } }
        public override string Name { get { return "OWP | ParentChild Script Simple"; } }
        public override string URL { get { return "http://code.google.com/p/open-wealth-project/"; } }
        public override Type WealthScriptType { get { return typeof(ChildClass); } }
    }

}
