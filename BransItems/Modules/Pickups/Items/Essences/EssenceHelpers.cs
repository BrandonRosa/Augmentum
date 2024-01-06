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
using BransItems.Modules.ItemTiers.CoreTier;

namespace BransItems.Modules.Pickups.Items.Essences
{
    public static class EssenceHelpers
    {
        //public static ItemTier essenceTiers => ItemTier.;
        public static ItemTierDef essenceTierDef => Core.instance.itemTierDef; 
        public static ItemTier essenceTier => Core.instance.itemTierDef._tier;
        public static bool canRemoveEssence = false;
        public static ItemTag[] essenceItemTags => new ItemTag[] { RoR2.ItemTag.WorldUnique };

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


        public static PickupIndex[] GetEssenceDrops(Xoroshiro128Plus rng, int dropCount)
        {
            PickupIndex[] pickupArray = new PickupIndex[dropCount];
            for (int i = 0; i < dropCount; i++)
                pickupArray[i] = GetEssenceIndex(rng);
            return pickupArray;
        }
    }
}
