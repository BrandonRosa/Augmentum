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
using BransItems.Modules.StandaloneBuffs;
using RoR2.ExpansionManagement;
using BransItems.Modules.Pickups.Items.HighlanderItems;

namespace BransItems.Modules.Pickups.Items.Tier2
{
    class NovaRing : ItemBase<NovaRing>
    {
        public override string ItemName => "Nova Band";
        public override string ItemLangTokenName => "NOVA_BAND";
        public override string ItemPickupDesc => $"High damage hits also create a shielding nova. Give more shield for every ally in range. Recharges over time. <style=cIsVoid>Corrupts all {HealRing.instance.ItemName.Replace(" Band","")} and {BarrierRing.instance.ItemName.Replace(" Band", "")} Bands.</style>";
        public override string ItemFullDescription => $"Hits that deal <style=cIsDamage>more than 400% damage</style> create a " +
            //$"<style=cIsHealing>{InitialRange}m</style> <style=cStack>(+{AdditionalRange}m per stack)</style> " +
            $"<style=cIsHealing>shielding nova</style>" +
            $" which gives <style=cIsHealing>{InitialPercent*100}%</style> <style=cStack>(+{AdditionalPercent*100}% per stack)</style> TOTAL damage as <style=cIsHealing>temporary shield</style> up to <style=cIsHealing>{InitialMaxHealing*100}%</style> <style=cStack>(+{AdditionalMaxHealing*100}% per stack)</style> max health. " +
            $"Give <style=cIsHealing>{(AllyBonus)*100}% more shield</style> per Ally in range. Recharges every <style=cIsUtility>{CooldownDuration}</style> seconds. <style=cIsVoid>Corrupts all {HealRing.instance.ItemName.Replace(" Band","")} and {BarrierRing.instance.ItemName.Replace(" Band", "")} Bands.</style> "; //Lasts <style=cIsHealing>{TempShieldDuration}</style> seconds. 

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.VoidTier2;

        public override GameObject ItemModel => SetupModel();
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Item/NovaBand/NovaBand.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override string[] CorruptsItem => new string[] { "ITEM_"+BarrierRing.instance.ItemLangTokenName+"_NAME", "ITEM_"+HealRing.instance.ItemLangTokenName + "_NAME" };

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public static float InitialPercent;
        public static float AdditionalPercent;
        public static float InitialMaxHealing;
        public static float AdditionalMaxHealing;
        public static float InitialRange;
        public static float AdditionalRange;
        public static float TempShieldDuration;
        public static float AllyBonus;
        public static float CooldownDuration;
        public static bool AllyBonusAfterCalculation = true;
        public static bool AccountForPreexistingTempShield = true;

        public static GameObject potentialPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();

        public static GameObject effectPrefab= Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthHealExplosion.prefab").WaitForCompletion();

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();

            //ItemDef.requiredExpansion = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();

            SetupEffectPrefab();
            //CreateBuff();
            CreateItem();
            Hooks();
        }

        private void SetupEffectPrefab()
        {
            GameObject temp= Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthHealExplosion.prefab").WaitForCompletion().InstantiateClone("ShieldExplosion"); //"RoR2/Base/Common/ColorRamps/texRampHuntressSoft2.png"
            Texture2D shieldMat = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/ColorRamps/texRampLightning2.png").WaitForCompletion();// RoR2/Base/Common/ColorRamps/texRampHuntressSoft2.png
            float playbackspeed = .3f;
            Color shieldColor = new Color32(68,94,182,255);
            Color shieldColorLight = new Color32(104, 125, 217, 255);

            ParticleSystem particleSystem1 = temp.transform.GetChild(0).GetComponent<ParticleSystem>();
            ParticleSystem.MainModule MainChild1 = particleSystem1.main;
            MainChild1.simulationSpeed = playbackspeed;
            ParticleSystem.ColorOverLifetimeModule COLM1 = particleSystem1.colorOverLifetime;
            COLM1.color = shieldColor;

            ParticleSystem particleSystem2 = temp.transform.GetChild(1).GetComponent<ParticleSystem>();
            particleSystem2.startColor = shieldColor;
            ParticleSystem.MainModule MainChild2 = particleSystem2.main;
            MainChild2.simulationSpeed = playbackspeed;
            ParticleSystem.ColorOverLifetimeModule COLM2 = particleSystem2.colorOverLifetime;
            COLM2.color = shieldColor;

            ParticleSystem particleSystem3 = temp.transform.GetChild(2).GetComponent<ParticleSystem>();
            particleSystem3.startColor = shieldColor;
            ParticleSystem.MainModule MainChild3 = particleSystem3.main;
            MainChild3.simulationSpeed = playbackspeed;
            ParticleSystem.ColorOverLifetimeModule COLM3 = particleSystem3.colorOverLifetime;
            COLM3.color = shieldColor;
            ParticleSystemRenderer particleSystemRenderer3 = temp.transform.GetChild(2).GetComponent<ParticleSystemRenderer>();
            particleSystemRenderer3.GetMaterial().SetTexture("_RemapTex", shieldMat);

            temp.transform.GetChild(3).GetComponent<Light>().color=shieldColorLight;

            ParticleSystem particleSystem5 = temp.transform.GetChild(4).GetComponent<ParticleSystem>();
            particleSystem5.startColor = shieldColor;
            ParticleSystem.MainModule MainChild5 = particleSystem5.main;
            MainChild5.simulationSpeed = playbackspeed;
            ParticleSystem.ColorOverLifetimeModule COLM5 = particleSystem2.colorOverLifetime;
            COLM5.color = shieldColor;
            ParticleSystemRenderer particleSystemRenderer5 = temp.transform.GetChild(4).GetComponent<ParticleSystemRenderer>();
            particleSystemRenderer5.GetMaterial().SetTexture("_RemapTex", shieldMat);

            ParticleSystem particleSystem6 = temp.transform.GetChild(5).GetComponent<ParticleSystem>();
            particleSystem6.startColor = shieldColor;
            ParticleSystem.MainModule MainChild6 = particleSystem6.main;
            MainChild6.simulationSpeed = playbackspeed;
            ParticleSystem.ColorOverLifetimeModule COLM6 = particleSystem6.colorOverLifetime;
            COLM6.color = shieldColor;

            ParticleSystem particleSystem7 = temp.transform.GetChild(6).GetComponent<ParticleSystem>();
            particleSystem7.startColor = shieldColor;
            ParticleSystem.MainModule MainChild7 = particleSystem7.main;
            MainChild7.simulationSpeed = playbackspeed;
            ParticleSystem.ColorOverLifetimeModule COLM7 = particleSystem6.colorOverLifetime;
            COLM7.color = shieldColor;

            effectPrefab = temp;

            ContentAddition.AddEffect(effectPrefab);

        }

        private GameObject SetupModel()
        {
            //GameObject TempItemModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElementalRings/PickupIceRing.prefab").WaitForCompletion().InstantiateClone("NovaRing", false); SWAP WITH BOTTOM TO LICK SICK AS FUCK
            GameObject TempItemModel = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ElementalRingVoid/PickupVoidRing.prefab").WaitForCompletion().InstantiateClone("NovaRing", false);
            GameObject NewModel= MainAssets.LoadAsset<GameObject>("Assets/Models/Meshes/NovaBand.blend");
            Texture2D shieldMat = MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Ramps/texRampNullifier3.png");
            Texture2D shieldMatOffset = MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Ramps/texRampNullifierOffset3.png");//Assets/Textrures/Ramps/texNovaRingDiffuse.png
            Texture2D MainTex = MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Ramps/texNovaRingDiffuse.png");

            MeshFilter Model = TempItemModel.GetComponentInChildren<MeshFilter>();
            Model.mesh = NewModel.GetComponentInChildren<MeshFilter>().mesh;

            Material material = TempItemModel.GetComponentInChildren<MeshRenderer>().material;

            //Material Stuff
            material.SetColor("_EmissionColor", new Color32(0, 70, 255,255));
            material.SetColor("_MainColor", new Color32(0, 100, 255, 255));
            material.SetTexture("_FresnelRamp", shieldMatOffset);
            material.SetTexture("_PrintRamp", shieldMat); //_MainTex
            material.SetTexture("_MainTex", MainTex);

            return TempItemModel;
        }

        public void CreateConfig(ConfigFile config)
        {
            CooldownDuration = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Duration of cooldown", 20f, "What should be the duration of the cooldown for this tiem?");
            InitialPercent = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Percent of total damage converted to shield", .10f, "What percent of total damage should be healed from the first stack of this item?");
            AdditionalPercent = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Percent of additional damage converted to shield", .05f, "What percent of total damage should be healed from additional stacks of this item?");
            InitialMaxHealing = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Max percent of max health you can gain in shield", .20f, "What is the maximum percent of your health you can heal from this item from the first stack?");
            AdditionalMaxHealing = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Additional percent of max health you can heal", .10f, "What is the maximum percent of your health you can heal from this item from additional stacks?");
            AllyBonus = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Bonus Percent of shield from allies", .4f, "What is the bonus shield gained from giving shields to allies?");
            InitialRange = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Range of nova on first stack", 15f, "What is the initial range of the nova from the first stack of this item?");
            AdditionalRange = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Range of nova on additional stacks", 4f, "What is the additional range of the nova from additional stacks of this item?");
            TempShieldDuration = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Duration in seconds", 15f, "What is the duration of the temporary shields?");
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
            CharacterBody component2=null;
            if (self && damageInfo.attacker && victim)
            {
                safe = true;
                component2 = damageInfo.attacker.GetComponent<CharacterBody>();
                if (component2 && damageInfo.damage / component2.damage >= 4f)
                {
                    if (component2.HasBuff(NovaRingReady.instance.BuffDef))
                    {
                        int itemCount = component2.inventory.GetItemCount(ItemDef);
                        triggered = true;
                        if (itemCount > 0)
                        {
                            //component2.healthComponent.Heal(Math.Min(damageInfo.damage * (InitialPercent + AdditionalPercent * ((float)itemCount - 1f)), component2.healthComponent.fullHealth * (InitialMaxHealing + AdditionalMaxHealing * ((float)itemCount - 1f))), damageInfo.procChainMask);
                            int preexistingTempShield = AccountForPreexistingTempShield?component2.GetBuffCount(TemporaryShield.instance.BuffDef):0;
                            if (AllyBonusAfterCalculation)
                            {
                                float initialShield;
                                initialShield = Math.Min(damageInfo.damage * (InitialPercent + AdditionalPercent * ((float)itemCount - 1f)), (component2.healthComponent.fullCombinedHealth-preexistingTempShield) * (InitialMaxHealing + AdditionalMaxHealing * ((float)itemCount - 1f)));
                                ShieldNovaPostCalculation(initialShield, damageInfo.attacker, itemCount, component2.teamComponent.teamIndex);
                            }
                            else
                            {
                                ShieldNovaPreCalculation(damageInfo.damage * (InitialPercent + AdditionalPercent * ((float)itemCount - 1f)), (component2.healthComponent.fullCombinedHealth-preexistingTempShield) * (InitialMaxHealing + AdditionalMaxHealing * ((float)itemCount - 1f)), damageInfo.attacker, itemCount, component2.teamComponent.teamIndex);
                            }
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);

            if (safe && component2!=null && triggered && component2.HasBuff(NovaRingReady.instance.BuffDef))
            {
                component2.RemoveBuff(NovaRingReady.instance.BuffDef);
                float cooldown = NovaRing.CooldownDuration;
                if (component2.inventory.GetItemCount(HighlanderItems.CooldownBand.instance.ItemDef) > 0)
                    cooldown *= HighlanderItems.CooldownBand.CooldownReduction;
                for (int i = 0; i <= component2.inventory.GetItemCount(DoubleBand.instance.ItemDef); i++)
                    for (int k = 1; (float)k <= cooldown; k++)
                    {
                        component2.AddTimedBuff(NovaRingCooldown.instance.BuffDef, k);
                    }
            }
        }

        private void ShieldNovaPostCalculation(float shieldAmount, GameObject victim, int itemCount, TeamIndex spawnerTeam)
        {
            List<CharacterBody> list = new List<CharacterBody>();
            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.radius = InitialRange+AdditionalRange*((float)itemCount - 1f);
            sphereSearch.origin = victim.transform.position;
            sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)1;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
            for (int i = 0; i < hurtBoxes.Length; i++)
            {
                CharacterBody item = hurtBoxes[i].healthComponent.body;
                if (!list.Contains(item) && item.teamComponent.teamIndex==spawnerTeam)
                {
                    list.Add(item);
                }
            }
            int totalShield = (int)(shieldAmount * (1+AllyBonus*(list.Count-1f)));
            foreach (CharacterBody item2 in list)
            {
               
                for (int i = 0; i < totalShield; i++)
                    item2.AddTimedBuff(TemporaryShield.instance.BuffDef, TempShieldDuration);
            }
            effectPrefab.transform.localScale = Vector3.one*sphereSearch.radius;
            EffectManager.SimpleEffect(effectPrefab, victim.transform.position, Quaternion.identity, transmit: true);
            //effectPrefab.transform.localScale = Vector3.one * 12;
        }

        private void ShieldNovaPreCalculation(float damageCalculation, float healthCalculation, GameObject victim, int itemCount, TeamIndex spawnerTeam)
        {
            List<CharacterBody> list = new List<CharacterBody>();
            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.radius = InitialRange + AdditionalRange * ((float)itemCount - 1f);
            sphereSearch.origin = victim.transform.position;
            sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)1;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
            for (int i = 0; i < hurtBoxes.Length; i++)
            {
                CharacterBody item = hurtBoxes[i].healthComponent.body;
                if (!list.Contains(item) && item.teamComponent.teamIndex == spawnerTeam)
                {
                    list.Add(item);
                }
            }
            int totalShieldPercent = (int)(1 + AllyBonus * (list.Count - 1f));
            float totalShield;
            totalShield = Math.Min(damageCalculation*totalShieldPercent, healthCalculation );
            foreach (CharacterBody item2 in list)
            {
                for (int i = 0; i < totalShield; i++)
                    item2.AddTimedBuff(TemporaryShield.instance.BuffDef, TempShieldDuration);
            }
            EffectManager.SimpleEffect(effectPrefab, victim.transform.position, Quaternion.identity, transmit: true);
        }
    }

    public class NovaRingCooldown : BuffBase<NovaRingCooldown>
    {
        public override string BuffName => "Nova Ring Cooldown";

        public override Color Color => new Color32(245, 245, 245, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Buff/NovaBandCooldown.png");
        public override bool CanStack => true;
        public override bool IsDebuff => true;

        public override bool IsCooldown => true;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
        }

        private void OnLoadModCompat()
        {
            //if (Compatability.ModCompatability.BetterUICompat.IsBetterUIInstalled)
            //{
            //    var buffInfo = CreateBetterUIBuffInformation($"AETHERIUM_DOUBLE_GOLD_DOUBLE_XP_BUFF", BuffName, "All kills done by you grant double gold and double xp to you.");
            //    RegisterBuffInfo(BuffDef, buffInfo.Item1, buffInfo.Item2);
            //}

            //if (Aetherium.Interactables.BuffBrazier.instance != null)
            //{
            //    AddCuratedBuffType("Double Gold and Double XP", BuffDef, Color, 1.25f, false);
            //}
        }

    }
    public class NovaRingReady : BuffBase<NovaRingReady>
    {
        public override string BuffName => "Nova Ring Ready";

        public override Color Color => new Color32(245, 245, 245, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Buff/NovaBandReady.png");
        public override bool CanStack => false;
        public override bool IsDebuff => false;

        public override bool IsCooldown => false;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            //RoR2Application.onLoad += OnLoadModCompat;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            //Basically if the player exists, and doesnt have "Fortified Ready" or "Fortified Cooldown"- Check if the player has the Fortified Tracker component- if they do add the bufff
            if (self)
            {
                if (!self.HasBuff(BuffDef))
                {
                    if (!self.HasBuff(NovaRingCooldown.instance.BuffDef)) // && !self.HasBuff(Fortified.instance.BuffDef))
                    {
                        Inventory inv = self.inventory;
                        if (inv && inv.GetItemCount(NovaRing.instance.ItemDef)>0) //cpt = self.gameObject.AddComponent<FortifiedTracker>();
                        {
                            self.AddBuff(BuffDef);
                        }

                    }
                }
            }
        }

        private void OnLoadModCompat()
        {
            //if (Compatability.ModCompatability.BetterUICompat.IsBetterUIInstalled)
            //{
            //    var buffInfo = CreateBetterUIBuffInformation($"AETHERIUM_DOUBLE_GOLD_DOUBLE_XP_BUFF", BuffName, "All kills done by you grant double gold and double xp to you.");
            //    RegisterBuffInfo(BuffDef, buffInfo.Item1, buffInfo.Item2);
            //}

            //if (Aetherium.Interactables.BuffBrazier.instance != null)
            //{
            //    AddCuratedBuffType("Double Gold and Double XP", BuffDef, Color, 1.25f, false);
            //}
        }

    }
}
