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
using static RoR2.ItemTag;
using Augmentum.Modules.ItemTiers.HighlanderTier;
using Augmentum.Modules.Utils;

namespace Augmentum.Modules.Pickups.Items.HighlanderItems
{
    class CurvedHorn : ItemBase<CurvedHorn>
    {
        public override string ItemName => "Curved Horn";
        public override string ItemLangTokenName => "CURVED_HORN";
        public override string ItemPickupDesc => "Increases damage and primary charges.";
        public override string ItemFullDescription => $"Increase your <style=cIsDamage>damage</style> by <style=cIsDamage>{DamageGain*100}%</style>. Gain <style=cIsUtility>{(PrimaryChargeMultiplier - 1) * 100f}%</style> more <style=cIsUtility>Primary skill</style> charges.";

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
        public static float PrimaryChargeMultiplier;

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
            DamageGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Damage percent given to character", .30f, "How much percent damage should Curved Horn grant?");
            PrimaryChargeMultiplier = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Multiplier to increase primary charges (rounded up)", 1.5f, "What multiplier should curved horn increase primary charges by? (1.5=150%)");
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
            On.RoR2.GenericSkill.RecalculateMaxStock += GenericSkill_RecalculateMaxStock;

        }

        private void GenericSkill_RecalculateMaxStock(On.RoR2.GenericSkill.orig_RecalculateMaxStock orig, GenericSkill self)
        {
            orig(self);
            if (GetCount(self.characterBody) > 0)
            {

                SkillLocator skillLocator = self.characterBody.skillLocator;
                if (self == skillLocator.primaryBonusStockSkill)
                {
                    int additional = Mathf.CeilToInt(self.skillDef.baseMaxStock * (PrimaryChargeMultiplier - 1));
                    self.maxStock += additional;

                    //if (self.rechargeStock == self.skillDef.baseMaxStock && self.rechargeStock > 1)
                        //self.rechargeStock += additional;
                }
            }
        }


        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int count = GetCount(sender);
            args.damageMultAdd += DamageGain * count;

            if (count > 0 && sender.skillLocator.primaryBonusStockSkill)
            {
                sender.skillLocator.primaryBonusStockSkill.RecalculateMaxStock();
            }


        }
    }
}
