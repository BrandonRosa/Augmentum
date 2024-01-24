using System;
using System.Collections.Generic;
using System.Text;

namespace BransItems.Modules.ColorCatalogEntry.CoreColors
{
    class CoreLight : ColorCatalogEntryBase<CoreLight>
    {
        public override byte r => 113;//21;

        public override byte g => 170;//99;

        public override byte b => 170;//58;

        public override byte a => 255;

        public override string ColorCatalogEntryName => "CoreLight";

        public override void Init()
        {
            CreateColorCatalogEntry();
        }
    }
}
