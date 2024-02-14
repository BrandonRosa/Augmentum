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
using UnityEngine.Networking;
using BransItems.Modules.Pickups.Items.Tier3;

namespace BransItems.Modules.Pickups.Items.NoTier
{
    class ShatteredPiggyBrine : ItemBase<ShatteredPiggyBrine>
    {
        public override string ItemName => "Piggy Brine (Shattered)";
        public override string ItemLangTokenName => "SHATTERED_PIG_JAR";
        public override string ItemPickupDesc => "It's what he wouldve wanted.... Does nothing anymore.";
        public override string ItemFullDescription => ItemPickupDesc;

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/Prefavs/Item/Essence_of_Strength/EssenceOfStrength.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/Piggy/PigJarShattered.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { };



        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            
            CreateItem();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            
            return rules;
        }


        public override void Hooks()
        {
            
        }
    }
}
