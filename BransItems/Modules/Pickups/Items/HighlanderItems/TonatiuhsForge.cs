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
using Augmentum.Modules.StandaloneBuffs;

namespace Augmentum.Modules.Pickups.Items.HighlanderItems
{
    class TonatiuhsForge : ItemBase<TonatiuhsForge>
    {
        public override string ItemName => "Overclocked Pacemaker";
        public override string ItemLangTokenName => "OVERCLOCKED_PACEMAKER";
        public override string ItemPickupDesc => "When using your Special skill, increase your damage. Scales with cooldown.";
        public override string ItemFullDescription =>  $"When using your <style=cIsUtility>Special skill</style>, increase your total <style=cIsDamage>damage</style> by <style=cIsDamage>{DamageGain*100}%</style> for every second of cooldown. Lasts {Duration} seconds.";

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/Pacemaker/PacemakerModel.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/Pacemaker/Pacemaker.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public static float DamageGain => DamageGainEntry.Value;
        public static ConfigEntry<float> DamageGainEntry;
        public static float Duration => DurationEntry.Value;
        public static ConfigEntry<float> DurationEntry;

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
            string ConfigItemName = ItemName.Replace("\'", "");
            DamageGainEntry = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Damage percent given to character", .10f, "How much percent damage per second of cooldown should this give?");
            DurationEntry = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Duration of damage boost", 5f, "How long should the damage boost last?");
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
            On.RoR2.GenericSkill.OnExecute += GenericSkill_OnExecute;
        }

        private void GenericSkill_OnExecute(On.RoR2.GenericSkill.orig_OnExecute orig, GenericSkill self)
        {
            if (self && self.characterBody.inventory && GetCount(self.characterBody) > 0)
            {
                SkillLocator skillLocator = self.characterBody.skillLocator;
                if (self == skillLocator.special || self == skillLocator.specialBonusStockSkill || self == skillLocator.specialBonusStockOverrideSkill)
                {
                    float duration = self.CalculateFinalRechargeInterval();
                    for (int i = 0; i < duration; i++)
                        self.characterBody.AddTimedBuff(DivineHeat.instance.BuffDef, Duration);
                    self.characterBody.RecalculateStats();
                }
            }
            orig(self);
        }
    }

    public class DivineHeat : BuffBase<DivineHeat>
    {
        public override string BuffName => "Overclocked Heat";

        public override Color Color => new Color32(250, 250, 250, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/Pacemaker/PacemakerBuff2.png");
        public override bool CanStack => true;
        public virtual bool IsDebuff => false;

        public virtual bool IsCooldown => false;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int count = sender.GetBuffCount(DivineHeat.instance.BuffDef);
            if (count > 0)
            {
                args.damageMultAdd += count * TonatiuhsForge.DamageGain;
            }
        }
    }
}
