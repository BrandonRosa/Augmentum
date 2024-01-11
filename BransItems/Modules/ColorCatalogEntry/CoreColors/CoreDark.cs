using System;
using System.Collections.Generic;
using System.Text;

namespace BransItems.Modules.ColorCatalogEntry.CoreColors
{
    class CoreDark : ColorCatalogEntryBase<CoreDark>
    {
        public override byte r => 60;//1;

        public override byte g => 90;//126;

        public override byte b => 90;//62;

        public override byte a => 255;

        public override string ColorCatalogEntryName => "CoreDark";

        public override void Init()
        {
            CreateColorCatalogEntry();
        }
    }
}
