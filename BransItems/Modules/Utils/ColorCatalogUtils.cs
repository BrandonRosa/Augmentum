using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BransItems.Modules.Utils
{
    public static partial class ColorCatalogUtils
    {
        private static bool _hookEnabled = false;
        internal static void SetHooks()
        {
            if (_hookEnabled)
            {
                return;
            }

            //Color Catalog
            On.RoR2.ColorCatalog.GetColor += ColorCatalog_GetColor;
            On.RoR2.ColorCatalog.GetColorHexString += ColorCatalog_GetColorHexString;

            //DamageColor
            //On.RoR2.DamageColor.FindColor += DamageColor_FindColor;

            _hookEnabled = true;
        }

        private static UnityEngine.Color DamageColor_FindColor(On.RoR2.DamageColor.orig_FindColor orig, RoR2.DamageColorIndex colorIndex)
        {
            return orig(colorIndex);
        }
        //ColorCatalog has 0-28 Enums, the array has 0-27 slots 
        private static string ColorCatalog_GetColorHexString(On.RoR2.ColorCatalog.orig_GetColorHexString orig, RoR2.ColorCatalog.ColorIndex colorIndex)
        {
            if(colorIndex>= RoR2.ColorCatalog.ColorIndex.Count && (int)colorIndex< RoR2.ColorCatalog.indexToHexString.Length)
                return RoR2.ColorCatalog.indexToHexString[(int)colorIndex];
            return orig(colorIndex);
        }

        private static UnityEngine.Color32 ColorCatalog_GetColor(On.RoR2.ColorCatalog.orig_GetColor orig, RoR2.ColorCatalog.ColorIndex colorIndex)
        {
            if (colorIndex >= RoR2.ColorCatalog.ColorIndex.Count && (int)colorIndex < RoR2.ColorCatalog.indexToColor32.Length)
                return RoR2.ColorCatalog.indexToColor32[(int)colorIndex];
            return orig(colorIndex);
        }

        public static ColorCatalog.ColorIndex RegisterColor(Color color)
        {
            ColorCatalogUtils.SetHooks();
            int nextColorIndex = ColorCatalog.indexToColor32.Length;
            ColorCatalog.ColorIndex newIndex = (ColorCatalog.ColorIndex)nextColorIndex;
            HG.ArrayUtils.ArrayAppend(ref ColorCatalog.indexToColor32, color);
            HG.ArrayUtils.ArrayAppend(ref ColorCatalog.indexToHexString, Util.RGBToHex(color));
            return newIndex;
        }
    }
}
