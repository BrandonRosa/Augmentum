using System;
using System.Collections.Generic;
using System.Text;
using BransItems.Modules.ItemTiers;
using BransItems.Modules.Pickups.Items.Essences;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BransItems.Modules.ItemTiers.CoreTier
{
    class Core : ItemTierBase<Core>
    {
        public override ItemTierDef itemTierDef => ScriptableObject.CreateInstance<ItemTierDef>(); // new ItemTierDef();

        //public override GameObject PickupDisplayVFX => throw new NotImplementedException();

        public override GameObject highlightPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();

        public override GameObject dropletDisplayPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();

        public override bool canRestack => true;

        public override bool canScrap => false;

        public override bool isDroppable => true;

        public override string TierName => "Core";

        public override ColorCatalog.ColorIndex colorIndex => ColorCatalog.ColorIndex.Money;

        public override ColorCatalog.ColorIndex darkColorIndex => ColorCatalog.ColorIndex.Teleporter;

        public override void Init()
        {
            CreateTier();

        }
    }
}
