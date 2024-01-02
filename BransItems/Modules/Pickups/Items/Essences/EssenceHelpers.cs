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
            PickupIndex[] EssenceIndicies = new PickupIndex[5];
            //PickupCatalog.FindPickupIndex(EOAcuity.itemIndex);
            EssenceIndicies[0] = PickupCatalog.FindPickupIndex(EOAcuity.instance.ItemDef.itemIndex);
            EssenceIndicies[1] = PickupCatalog.FindPickupIndex(EOStrength.instance.ItemDef.itemIndex);
            EssenceIndicies[2] = PickupCatalog.FindPickupIndex(EOLife.instance.ItemDef.itemIndex);
            EssenceIndicies[3] = PickupCatalog.FindPickupIndex(EOVelocity.instance.ItemDef.itemIndex);
            EssenceIndicies[4] = PickupCatalog.FindPickupIndex(EOFerocity.instance.ItemDef.itemIndex);


            return EssenceIndicies;
        }

        public static PickupIndex GetEssenceIndex(Xoroshiro128Plus rng)
        {
            if (rng.RangeFloat(0, 1) < EOTotality.ReplaceChance)
                return PickupCatalog.FindPickupIndex(EOTotality.instance.ItemDef.itemIndex);
            else
                return GetBasicEssencePickupIndex()[rng.RangeInt(0, 5)];
        }
    }
}
