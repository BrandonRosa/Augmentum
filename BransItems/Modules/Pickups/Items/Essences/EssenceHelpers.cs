using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;
using BransItems.Modules;

namespace BransItems.Modules.Pickups.Items.Essences
{
    public static class EssenceHelpers
    {
        //public 
        public static ItemDef[] MainEssenceList = { };

        public static PickupIndex[] GetBasicEssencePickupIndex()
        {
            PickupIndex[] EssenceIndicies = new PickupIndex[3];
            //PickupCatalog.FindPickupIndex(EOAcuity.itemIndex);
            EssenceIndicies[0] = PickupCatalog.FindPickupIndex("ESSENCE_OF_ACUITY");
            EssenceIndicies[1] = PickupCatalog.FindPickupIndex("ESSENCE_OF_LIFE");
            EssenceIndicies[2] = PickupCatalog.FindPickupIndex("ESSENCE_OF_Strength");

            return EssenceIndicies;
        }
    }
}
