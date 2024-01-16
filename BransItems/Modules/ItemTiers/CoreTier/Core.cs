using System;
using System.Collections.Generic;
using System.Text;
using BransItems.Modules.ColorCatalogEntry;
using BransItems.Modules.ColorCatalogEntry.CoreColors;
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
        //public override ItemTierDef itemTierDef = ScriptableObject.CreateInstance<ItemTierDef>(); // new ItemTierDef();

        //public override GameObject PickupDisplayVFX => throw new NotImplementedException();

        //public override GameObject highlightPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();

        //public override GameObject dropletDisplayPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();

        public override bool canRestack => true;

        public override bool canScrap => false;

        public override bool isDroppable => true;

        public override string TierName => "Core";

        //public ColorCatalog.ColorIndex colorIndex = ColorsAPI.RegisterColor(new Color32(21,99,58,255));//ColorCatalog.ColorIndex.Money;//CoreLight.instance.colorCatalogEntry.ColorIndex;

        //public ColorCatalog.ColorIndex darkColorIndex = ColorsAPI.RegisterColor(new Color32(1,126,62,255));//ColorCatalog.ColorIndex.Money;//CoreDark.instance.colorCatalogEntry.ColorIndex;
        /// <summary>
        ///  CoreLight.instance.colorCatalogEntry.ColorIndex;
        /// 
        /// x => CoreDark.instance.colorCatalogEntry.ColorIndex;
        /// </summary>
        public override void Init()
        {
            colorIndex = Colors.TempCoreLight;//ColorsAPI.RegisterColor(Color.gray);
            darkColorIndex = Colors.TempCoreDark;//ColorsAPI.RegisterColor(Color.gray);
            
            //colorIndex = CoreLight.instance.colorIndex;//CoreLight.instance.colorCatalogEntry.ColorIndex;//new Color(.08f, .39f, .23f));//(new Color32(21, 99, 58, 255)));
            //darkColorIndex = CoreDark.instance.colorIndex; //new Color(0f,.49f,.24f));//(new Color32(1, 126, 62, 255)));
            BransItems.ModLogger.LogWarning("ArraySize:" + (ColorCatalog.indexToHexString.Length));
            BransItems.ModLogger.LogWarning("LastElementHex:" + (ColorCatalog.indexToHexString[ColorCatalog.indexToHexString.Length-1]));
            CreateTier();
            itemTierDef.highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            itemTierDef.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();
            //BransItems.ModLogger.LogWarning(itemTierDef.tier.ToString());
            //BransItems.ModLogger.LogWarning("Correct:" + ItemTier.AssignedAtRuntime.ToString());
            //BransItems.ModLogger.LogWarning("MyTierName:" + itemTierDef.name);
            //BransItems.ModLogger.LogWarning("MyTierCanScrap:" + itemTierDef.canScrap);

        }
    }
}
