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
using static BransItems.Modules.Pickups.Items.Essences.EssenceHelpers;
using UnityEngine.Networking;
using BransItems.Modules.Pickups.Items.Essences;
using BransItems.Modules.Pickups.Items.NoTier;
using BransItems.Modules.Utils;
using UnityEngine.AddressableAssets;
using BransItems.Modules.Pickups.Items.CoreItems;
using BransItems.Modules.Pickups.Items.Tier1;
using BransItems.Modules.Pickups.Items.Tier3;

namespace BransItems.Modules.Pickups.Items.Tier2
{
    class BarrierRing : ItemBase<BarrierRing>
    {
        public override string ItemName => "Barrier Band";
        public override string ItemLangTokenName => "BARRIER_BAND";
        public override string ItemPickupDesc => "High damage hits also give barrier to you. Recharges over time.";
        public override string ItemFullDescription => $"Hits that deal <style=cIsDamage>more than 400% damage</style> also grant a <style=cIsHealing>temporary barrier</style> for <style=cIsHealing>{InitialPercent*100}%</style><style=cStack>(+{AdditionalPercent*100}% per stack)</style> TOTAL damage as " +
            $"<style=cIsHealing>barrier</style> up to <style=cIsHealing>{InitialMaxHealing*100}%</style><style=cStack>(+{AdditionalMaxHealing*100}% per stack)</style> max combined health. Recharges every <style=cIsUtility>10</style> seconds.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => EditItemModel(Color.yellow);
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Item/HealingBands/BarrierBand.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility};

        public static float InitialPercent;
        public static float AdditionalPercent;
        public static float InitialMaxHealing;
        public static float AdditionalMaxHealing;

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
            InitialPercent = config.Bind<float>("Item: " + ItemName, "Percent of total damage heal", .20f, "What percent of total damage should be healed from the first stack of this item?").Value;
            AdditionalPercent = config.Bind<float>("Item: " + ItemName, "Percent of additional damage heal", .07f, "What percent of total damage should be healed from additional stacks of this item?").Value;
            InitialMaxHealing = config.Bind<float>("Item: " + ItemName, "Max percent of max health you can heal", .25f, "What is the maximum percent of your health you can heal from this item from the first stack?").Value;
            AdditionalMaxHealing = config.Bind<float>("Item: " + ItemName, "Additional percent of max health you can heal", .125f, "What is the maximum percent of your health you can heal from this item from additional stacks?").Value;

            //AdditionalDamageOfMainProjectilePerStack = config.Bind<float>("Item: " + ItemName, "Additional Damage of Projectile per Stack", 100f, "How much more damage should the projectile deal per additional stack?").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.42142F, -0.10234F),
                    localAngles = new Vector3(351.1655F, 45.64202F, 351.1029F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.35414F, -0.14761F),
                    localAngles = new Vector3(356.5505F, 45.08208F, 356.5588F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 2.46717F, 2.64379F),
                    localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
                    localScale = new Vector3(.2F, .2F, .2F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0, 0.24722F, -0.01662F),
                    localAngles = new Vector3(10.68209F, 46.03322F, 11.01807F),
                    localScale = new Vector3(0.025F, 0.025F, 0.025F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.24128F, -0.14951F),
                    localAngles = new Vector3(6.07507F, 45.37084F, 6.11489F),
                    localScale = new Vector3(0.017F, 0.017F, 0.017F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.31304F, -0.00747F),
                    localAngles = new Vector3(359.2931F, 45.00048F, 359.2912F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0, 1.94424F, -0.47558F),
                    localAngles = new Vector3(20.16552F, 48.87548F, 21.54582F),
                    localScale = new Vector3(.15F, .15F, .15F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.30118F, -0.0035F),
                    localAngles = new Vector3(8.31363F, 45.67525F, 8.41428F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.65444F, 1.64345F),
                    localAngles = new Vector3(326.1803F, 277.2657F, 249.9269F),
                    localScale = new Vector3(.2F, .2F, .2F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0068F, 0.3225F, -0.03976F),
                    localAngles = new Vector3(0F, 45F, 0F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.14076F, 0.15542F, -0.04648F),
                    localAngles = new Vector3(356.9802F, 81.10978F, 353.687F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hat",
                    localPos = new Vector3(0F, 0.01217F, -0.00126F),
                    localAngles = new Vector3(356.9376F, 25.8988F, 14.69767F),
                    localScale = new Vector3(0.001F, 0.001F, 0.001F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00042F, 0.46133F, 0.01385F),
                    localAngles = new Vector3(355.2848F, 47.55381F, 355.0908F),
                    localScale = new Vector3(0.020392F, 0.020392F, 0.020392F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00076F, -0.0281F, 0.09539F),
                    localAngles = new Vector3(338.9489F, 145.7505F, 217.6883F),
                    localScale = new Vector3(0.005402F, 0.005402F, 0.005402F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.00277F, -0.13259F),
                    localAngles = new Vector3(322.1495F, 124.8318F, 235.476F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.32104F, 0F),
                    localAngles = new Vector3(0F, 321.2954F, 0F),
                    localScale = new Vector3(0.024027F, 0.024027F, 0.024027F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.00216F, 0.01033F, 0F),
                    localAngles = new Vector3(0F, 323.6887F, 355.1232F),
                    localScale = new Vector3(0.000551F, 0.000551F, 0.000551F)
                }
            });
            return rules;
        }

        public GameObject EditItemModel(Color color)
        {
            GameObject TempItemModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElementalRings/PickupIceRing.prefab").WaitForCompletion().InstantiateClone("BarrierRing", false);
            Material mat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ElementalRings/matIceRing.mat").WaitForCompletion());
            Material mat2 = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ElementalRings/matIceRingGemstone.mat").WaitForCompletion());

            //All this just incase of of these changes the color.
            mat.SetColor("_MainColor", color);
            mat.SetColor("_Color", color);
            mat.SetColor("_TintColor", color);
            mat.color = color;
            mat2.SetColor("_MainColor", color);
            mat2.SetColor("_Color", color);
            mat2.SetColor("_TintColor", color);
            mat2.color = color;


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
                            component2.healthComponent.AddBarrier(Math.Min(damageInfo.damage * (InitialPercent + AdditionalPercent * ((float)itemCount - 1f)), component2.healthComponent.fullCombinedHealth * (InitialMaxHealing + AdditionalMaxHealing * ((float)itemCount - 1f))));
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);

            if (triggered && component2.HasBuff(HealingRingsReady.instance.BuffDef))
            {
                component2.RemoveBuff(HealingRingsReady.instance.BuffDef);
                for (int k = 1; (float)k <= 20f; k++)
                {
                    component2.AddTimedBuff(HealingRingsCooldown.instance.BuffDef, k);
                }
            }
        }
    }
}
