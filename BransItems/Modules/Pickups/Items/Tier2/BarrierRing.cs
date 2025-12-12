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
using static Augmentum.Modules.Pickups.Items.Essences.EssenceHelpers;
using UnityEngine.Networking;
using Augmentum.Modules.Pickups.Items.Essences;
using Augmentum.Modules.Pickups.Items.NoTier;
using Augmentum.Modules.Utils;
using UnityEngine.AddressableAssets;
using Augmentum.Modules.Pickups.Items.CoreItems;
using Augmentum.Modules.Pickups.Items.Tier1;
using Augmentum.Modules.Pickups.Items.Tier3;
using Augmentum.Modules.Pickups.Items.HighlanderItems;

namespace Augmentum.Modules.Pickups.Items.Tier2
{
    class BarrierRing : ItemBase<BarrierRing>
    {
        public override string ItemName => "Brindel's Band";
        public override string ItemLangTokenName => "BARRIER_BAND";
        public override string ItemPickupDesc => "High damage hits also grants barrier. Recharges over time.";
        public override string ItemFullDescriptionRaw =>
            @"Hits that deal <style=cIsDamage>more than 400% damage</style> also grant a <style=cIsHealing>temporary barrier</style> for <style=cIsHealing>{0}%</style><style=cStack>(+{1}% per stack)</style> of the TOTAL damage up to <style=cIsHealing>{2}%</style><style=cStack>(+{3}% per stack)</style> max combined health. Recharges every <style=cIsUtility>{4}</style> seconds.";

        public override string ItemFullDescriptionFormatted =>
            string.Format(ItemFullDescriptionRaw, InitialPercent * 100, AdditionalPercent * 100, InitialMaxHealing * 100, AdditionalMaxHealing * 100, HealRing.HealingRingsCooldownTime);
        public override string ItemLore => $"Should you become weak,\nShould harm come to you,\nI will protect you.\nI will be there.";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => EditItemModel(Color.yellow);
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Item/HealingBands/BarrierBand.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility};

        public static float InitialPercent => InitialPercentEntry.Value;
        public static ConfigEntry<float> InitialPercentEntry;
        public static float AdditionalPercent => AdditionalPercentEntry.Value;
        public static ConfigEntry<float> AdditionalPercentEntry;
        public static float InitialMaxHealing => InitialMaxHealingEntry.Value;
        public static ConfigEntry<float> InitialMaxHealingEntry;
        public static float AdditionalMaxHealing => AdditionalMaxHealingEntry.Value;
        public static ConfigEntry<float> AdditionalMaxHealingEntry;

        public static GameObject potentialPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();

        public override void Init(ConfigFile config)
        {
            
            CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            string ConfigItemName = ItemName.Replace("\'", "");
            InitialPercentEntry = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Percent of total damage heal", .25f, "What percent of total damage should be healed from the first stack of this item?");
            AdditionalPercentEntry = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Percent of additional damage heal", .0725f, "What percent of total damage should be healed from additional stacks of this item?");
            InitialMaxHealingEntry = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Max percent of max health you can gain in barrier", .20f, "What is the maximum percent of your health you can gain in barrier from this item from the first stack?");
            AdditionalMaxHealingEntry = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Additional percent of max health you can gain in barrier", .125f, "What is the maximum percent of your health you can gain in barrier from this item from additional stacks?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

           
            return rules;
        }

        public GameObject EditItemModel(Color color)
        {
            GameObject TempItemModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElementalRings/PickupIceRing.prefab").WaitForCompletion().InstantiateClone("BarrierRing", false);
            Material mat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ElementalRings/matIceRing.mat").WaitForCompletion());
            Material mat2 = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ElementalRings/matIceRingGemstone.mat").WaitForCompletion());
            Color emissionColor = new Color32(150, 150, 50, 255);
            //All this just incase of of these changes the color.
            mat.SetColor("_MainColor", color);
            mat.SetColor("_Color", color);
            mat.SetColor("_TintColor", color);
            mat.SetColor("Emission Color", emissionColor);
            //mat.color = color;
            mat2.SetColor("_MainColor", color);
            mat2.SetColor("_Color", color);
            mat2.SetColor("_TintColor", color);
            mat2.SetColor("Emission Color", emissionColor);
            //mat2.color = color;


            //Light light = TempItemModel.transform.GetChild(0).gameObject.AddComponent<Light>();
            //light.color = Color.white;
            //light.type = LightType.Area;
            //light.shadows = LightShadows.None;
            //light.range = 10f; //not .5f,1f
            //light.intensity = 10f;

            //Light light2 = new Light();
            //light2.color = Color.white;
            //light2.type = LightType.Area;
            //light2.shadows = LightShadows.None;
            //light2.range = 10f; //not .5f
            //light2.intensity = 10f;

            //light2.gameObject.transform.SetParent(TempItemModel.transform.GetChild(0));

            TempItemModel.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().SetMaterial(mat);
            TempItemModel.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().SetMaterial(mat2);

            return TempItemModel;
        }


        public override void Hooks()
        {
            //On.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
            //On.RoR2.CharacterMaster.SpawnBodyHere += CharacterMaster_SpawnBodyHere;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            //On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }


        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            bool triggered = false;
            bool safe = false;
            CharacterBody component2 = null;
            if (self && damageInfo.attacker && victim)
            {
                safe = true;
                component2 = damageInfo.attacker.GetComponent<CharacterBody>();
                if (component2 && damageInfo.damage / component2.damage >= 4f)
                {
                    if (component2.HasBuff(HealingRingsReady.instance.BuffDef))
                    {
                        int itemCount = component2.inventory.GetItemCount(ItemDef);
                        triggered = true;
                        if (itemCount > 0)
                        {
                            for(int i=0;i<=component2.inventory.GetItemCount(DoubleBand.instance.ItemDef);i++)
                                component2.healthComponent.AddBarrier(Math.Min(damageInfo.damage * (InitialPercent + AdditionalPercent * ((float)itemCount - 1f)), component2.healthComponent.fullCombinedHealth * (InitialMaxHealing + AdditionalMaxHealing * ((float)itemCount - 1f))));
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);

            if (triggered && component2.HasBuff(HealingRingsReady.instance.BuffDef))
            {
                component2.RemoveBuff(HealingRingsReady.instance.BuffDef);
                float cooldown = HealRing.HealingRingsCooldownTime;
                if (component2.inventory.GetItemCount(HighlanderItems.CooldownBand.instance.ItemDef) > 0)
                    cooldown *= HighlanderItems.CooldownBand.CooldownReduction;
                for (int k = 1; (float)k <= cooldown; k++)
                {
                    component2.AddTimedBuff(HealingRingsCooldown.instance.BuffDef, k);
                }
            }
        }
    }
}
