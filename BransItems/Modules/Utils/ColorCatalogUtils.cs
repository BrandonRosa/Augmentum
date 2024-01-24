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

        private static List<Color32> indexToColor32 = new List<Color32>();
        private static List<string> indexToHexString = new List<string>();
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
            if ((int)colorIndex < 0)
                return indexToHexString[-1 - ((int)colorIndex)];
            return orig(colorIndex);
            /*
            if (colorIndex < ColorCatalog.ColorIndex.None || (int)colorIndex >= ColorCatalog.indexToColor32.Length || colorIndex == ColorCatalog.ColorIndex.Count)
            {
                colorIndex = ColorCatalog.ColorIndex.Error;
            }
            else if (colorIndex < ColorCatalog.ColorIndex.Count)
                colorIndex--;

            return ColorCatalog.indexToHexString[(int)colorIndex - 1];*/
        }

        private static UnityEngine.Color32 ColorCatalog_GetColor(On.RoR2.ColorCatalog.orig_GetColor orig, RoR2.ColorCatalog.ColorIndex colorIndex)
        {

            if ((int)colorIndex < 0)
                return indexToColor32[-1 - ((int)colorIndex)];
            return orig(colorIndex);
            /*
            if (colorIndex < ColorCatalog.ColorIndex.None || (int)colorIndex >= ColorCatalog.indexToColor32.Length || colorIndex == ColorCatalog.ColorIndex.Count)
            {
                colorIndex = ColorCatalog.ColorIndex.Error;
            }
            else if (colorIndex < ColorCatalog.ColorIndex.Count)
                colorIndex--;

            return ColorCatalog.indexToColor32[(int)colorIndex - 1];*/
        }

        public static ColorCatalog.ColorIndex RegisterColor(Color color)
        {
            ColorCatalogUtils.SetHooks();
            int nextColorIndex = -indexToColor32.Count-1;
            ColorCatalog.ColorIndex newIndex = (ColorCatalog.ColorIndex)nextColorIndex;
            //List<Color32> colorList = new List<Color32> (ColorCatalog.indexToColor32);
            //colorList.Add(color);
            //ColorCatalog.indexToColor32 = (colorList.ToArray());
            //
            //List<string> hexList = new List<string>(ColorCatalog.indexToHexString);
            //hexList.Add(Util.RGBToHex(color));
            //ColorCatalog.indexToHexString = (hexList.ToArray());
            //HG.ArrayUtils.ArrayAppend(ref ColorCatalog.indexToColor32, color);
            //HG.ArrayUtils.ArrayAppend(ref ColorCatalog.indexToHexString, Util.RGBToHex(color));
            indexToColor32.Add(color);
            indexToHexString.Add(Util.RGBToHex(color));
            return newIndex;
        }
    }
}
