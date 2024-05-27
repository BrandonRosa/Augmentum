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
using BransItems.Modules.Utils;

namespace BransItems.Modules.Pickups.Items.HighlanderItems
{
    class CurvedHorn : ItemBase<CurvedHorn>
    {
        public override string ItemName => "Curved Horn";
        public override string ItemLangTokenName => "CURVED_HORN";
        public override string ItemPickupDesc => "Increases damage.";
        public override string ItemFullDescription => $"Increase your <style=cIsDamage>damage</style> by <style=cIsDamage>{DamageGain*100}%</style>.";

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
        public static float PrimaryChargePercentIncrease;

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
            DamageGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Damage percent given to character", .25f, "How much percent damage should Curved Horn grant?");
            PrimaryChargePercentIncrease = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Multiplier to increase primary charges (rounded up)", 1.5f, "What multiplier should curved horn increase primary charges by? (1.5=150%)");
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
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if(GetCount(self)>0)
            {
                SkillLocator skillLocator = self.skillLocator;
                int charges = Mathf.CeilToInt(skillLocator.primary.baseStock*PrimaryChargePercentIncrease);
                skillLocator.primary.SetBonusStockFromBody(skillLocator.secondary.bonusStockFromBody + charges);
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //args.baseDamageAdd += DamageGain * GetCount(sender);
            args.damageMultAdd += DamageGain * GetCount(sender);
            
        }
    }
}
