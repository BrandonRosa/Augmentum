using BransItems.Modules.Utils;
using R2API;
using RoR2;
using UnityEngine;
//[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace BransItems.Modules.ColorCatalogEntry
{
    internal static class Colors
    {
        //public static DamageColorIndex ChargedColor;
        public static ColorCatalog.ColorIndex TempCoreLight { get; private set; } //= ColorsAPI.RegisterColor(Color.cyan);
        public static ColorCatalog.ColorIndex TempCoreDark { get; private set; } //= ColorsAPI.RegisterColor(Color.cyan);
        public static ColorCatalog.ColorIndex TempHighlandLight { get; private set; } //= ColorsAPI.RegisterColor(new Color(250, 247, 185));
        public static ColorCatalog.ColorIndex TempHighlandDark { get; private set; } //= ColorsAPI.RegisterColor(new Color(128, 126, 96));

        //[SystemInitializer(typeof(ColorsAPI))]
        public static void Init()
        {
            TempCoreLight = ColorsAPI.RegisterColor(new Color32(21, 99, 58, 255));//ColorsAPI.RegisterColor(Color.cyan);
            TempCoreDark = ColorsAPI.RegisterColor(new Color32(1, 126, 62, 255));

            TempHighlandLight = ColorsAPI.RegisterColor(new Color32(250, 247, 185,255));
            TempHighlandDark = ColorsAPI.RegisterColor(new Color32(128, 126, 96,255));
        }
    }
}
