using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Augmentum.Augmentum;
using static Augmentum.Modules.Utils.ItemHelpers;
using Augmentum.Modules;
using Augmentum.Modules.ItemTiers.CoreTier;

namespace Augmentum.Modules.Pickups.Items.Essences
{
    public static class EssenceHelpers
    {
        //public static ItemTier essenceTiers => ItemTier.;
        public static ItemTierDef essenceTierDef => Core.instance.itemTierDef; 
        public static ItemTier essenceTier => Core.instance.itemTierDef._tier;
        public static bool canRemoveEssence = true;
        public static ItemTag[] essenceItemTags => new ItemTag[] { RoR2.ItemTag.WorldUnique };

        //public 
        public static ItemDef[] MainEssenceList = { };

        public static PickupIndex[] GetBasicEssencePickupIndex()
        {
            PickupIndex[] EssenceIndicies = new PickupIndex[Core.instance.BaseEssences.Count];
            //PickupCatalog.FindPickupIndex(EOAcuity.itemIndex);
            //EssenceIndicies[0] = PickupCatalog.FindPickupIndex(EOAcuity.instance.ItemDef.itemIndex);
            //EssenceIndicies[1] = PickupCatalog.FindPickupIndex(EOStrength.instance.ItemDef.itemIndex);
            //EssenceIndicies[2] = PickupCatalog.FindPickupIndex(EOLife.instance.ItemDef.itemIndex);
            //EssenceIndicies[3] = PickupCatalog.FindPickupIndex(EOVelocity.instance.ItemDef.itemIndex);
            //EssenceIndicies[4] = PickupCatalog.FindPickupIndex(EOFerocity.instance.ItemDef.itemIndex);
            //EssenceIndicies[5] = PickupCatalog.FindPickupIndex(EOResilience.instance.ItemDef.itemIndex);
            int index = 0;
            foreach(var item in Core.instance.BaseEssences)
            {
                EssenceIndicies[index] = PickupCatalog.FindPickupIndex(item.ItemDef.itemIndex);
                index++;
            }

            return EssenceIndicies;
        }

        public static PickupIndex GetEssenceIndex(Xoroshiro128Plus rng)
        {
            if (EOTotality.instance!=null && rng.RangeFloat(0, 1) < EOTotality.ReplaceChance)
                return PickupCatalog.FindPickupIndex(EOTotality.instance.ItemDef.itemIndex);
            else
                return GetBasicEssencePickupIndex()[rng.RangeInt(0, Core.instance.BaseEssences.Count)];
        }


        public static PickupIndex[] GetEssenceDrops(Xoroshiro128Plus rng, int dropCount)
        {
            PickupIndex[] pickupArray = new PickupIndex[dropCount];
            for (int i = 0; i < dropCount; i++)
                pickupArray[i] = GetEssenceIndex(rng);
            return pickupArray;
        }

        public static PickupIndex[] GetEssenceDropsWithoutRepeating(Xoroshiro128Plus rng, int dropCount)
        {
            PickupIndex[] BasicEssence = GetBasicEssencePickupIndex();
            bool hasTotality = EOTotality.instance != null;

            if (hasTotality && dropCount >= BasicEssence.Length +1)
            {
                PickupIndex[] result = new PickupIndex[BasicEssence.Length+1];
                Array.Copy(BasicEssence, result, BasicEssence.Length);
                result[^1] = PickupCatalog.FindPickupIndex(EOTotality.instance.ItemDef.itemIndex);
                return result;
            }
            else if (dropCount >= BasicEssence.Length)
                return BasicEssence;
            List<PickupIndex> pickupList = new List<PickupIndex>();
            for(int i=0; i<dropCount;i++)
            {
                PickupIndex pick;
                do
                {
                    pick = GetEssenceIndex(rng);
                } while (pickupList.Contains(pick));
                pickupList.Add(pick);
            }

            return pickupList.ToArray();
        
        }
    }
}
