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
using static RoR2.ItemTag;
using BransItems.Modules.ItemTiers.HighlanderTier;

namespace BransItems.Modules.Pickups.Items.HighlanderItems
{
    class CurvedHorn : ItemBase<CurvedHorn>
    {
        public override string ItemName => "Curved Horn";
        public override string ItemLangTokenName => "CURVED_HORN";
        public override string ItemPickupDesc => "Slightly increase damage.";
        public override string ItemFullDescription => $"Increase your <style=cIsDamage>damage by {DamageGain}</style><style=cStack>(+{DamageGain})</style>.";

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/CurvedHorn/HornItem.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/CurvedHorn/HornIcon.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] {ItemTag.Damage };

        public static float DamageGain;


        public override void Init(ConfigFile config)
        {
            //ItemDef._itemTierDef = EssenceHelpers.essenceTierDef;
            CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();

        }

        public void CreateConfig(ConfigFile config)
        {
            DamageGain = config.Bind<float>("Item: " + ItemName, "Base damage given to character", 3f, "How much base damage should Curved Horn grant?").Value;
            //AdditionalDamageOfMainProjectilePerStack = config.Bind<float>("Item: " + ItemName, "Additional Damage of Projectile per Stack", 100f, "How much more damage should the projectile deal per additional stack?").Value;
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
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.baseDamageAdd += DamageGain * GetCount(sender);
        }
    }
}
