using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BransItems.Modules.ItemTiers
{


    public abstract class ItemTierBase<T> : ItemTierBase where T : ItemTierBase<T>
    {
        public static T instance { get; private set; }

        public ItemTierBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    /// <summary>
    /// A <see cref="ContentBase"/> that represents an <see cref="RoR2.ItemTierDef"/> for the game, the ItemTierDef is represented via the <see cref="itemTierDef"/>
    /// <para>Its tied module base is the <see cref="ItemTierModuleBase"/></para>
    /// </summary>
    public abstract class ItemTierBase 
    {
        /// <summary>
        /// Optional, if supplied, it'll override the <see cref="ItemTierDef.colorIndex"/> for this custom color.
        /// </summary>
        public abstract ColorCatalog.ColorIndex colorIndex { get; }


        //public virtual SerializableColorCatalogEntry ColorIndex { get; }
        /// <summary>
        /// Optional, if supplied, it'll override the <see cref="ItemTierDef.darkColorIndex"/> for this custom color.
        /// </summary>
        public abstract ColorCatalog.ColorIndex darkColorIndex { get; }

        //public virtual SerializableColorCatalogEntry DarkColorIndex { get; }

        /// <summary>
        /// The ItemTierDef that represents this ItemTierBase
        /// </summary>
        public ItemTierDef itemTierDef  = ScriptableObject.CreateInstance<ItemTierDef>();

        /// <summary>
        /// The pickup display for this ItemTierDef, note that this is not the droplet VFX, but the VFX that appears when the item's pickup is in the world.
        /// </summary>
        //public abstract GameObject PickupDisplayVFX { get; }

        /// <summary>
        /// A list of all the items that have this Tier
        /// </summary>
        public List<ItemIndex> ItemsWithThisTier { get; internal set; } = new List<ItemIndex>();

        /// <summary>
        /// A list of the available items that can drop in the current run, returns null or an outdated list if a run is not active.
        /// </summary>
        public List<PickupIndex> AvailableTierDropList { get; internal set; } = new List<PickupIndex>();

        public GameObject highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();

        public GameObject dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();

        public virtual bool canRestack { get; } = true;

        public virtual bool canScrap { get; } = false;

        public virtual bool isDroppable { get; } = true;

        public virtual string TierName { get; internal set; } = "DEFAULT";

 

        public abstract void Init();

        public void CreateTier()
        {
            itemTierDef.canRestack = canRestack;
            itemTierDef.canScrap = canScrap;
            itemTierDef.isDroppable = isDroppable;
            itemTierDef.highlightPrefab = highlightPrefab;//Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            itemTierDef.dropletDisplayPrefab = dropletDisplayPrefab;//Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();
            //TierName = "Core";
            ItemsWithThisTier = new List<ItemIndex>();
            itemTierDef.name = TierName;
            itemTierDef.colorIndex = colorIndex; //ColorCatalog.ColorIndex.Money;
            itemTierDef.darkColorIndex = darkColorIndex;  //ColorCatalog.ColorIndex.Teleporter;
            itemTierDef.tier= ItemTier.AssignedAtRuntime;
            
            //BransItems.ModLogger.LogWarning(itemTierDef.tier.ToString());
            //BransItems.ModLogger.LogWarning("Correct:"+ ItemTier.AssignedAtRuntime.ToString());
            //BransItems.ModLogger.LogWarning("MyTierName:" + itemTierDef.name);
           // BransItems.ModLogger.LogWarning("MyTierCanScrap:" + itemTierDef.canScrap);

            //if(!itemTierDef.name.Equals(TierName))
               // BransItems.ModLogger.LogWarning("NAME DIDNT SAVE");
            //ItemTierDef.bgIconTexture.
            ContentAddition.AddItemTierDef(itemTierDef);
        }
    }
}