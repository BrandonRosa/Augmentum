using BepInEx.Configuration;
using Augmentum.Modules.StandaloneBuffs;
using Augmentum.Modules.Utils;

using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Augmentum.Augmentum;
using static Augmentum.Modules.Utils.ItemHelpers;


namespace Augmentum.Modules.Pickups.EliteEquipments
{
    class AffixAdaptive : EliteEquipmentBase<AffixAdaptive>
    {
        public override string EliteEquipmentName => "Its Whispered Secrets";

        public override string EliteAffixToken => "AFFIX_ADAPTIVE";

        public override string EliteEquipmentPickupDesc => "Become an aspect of evolution.";

        public override string EliteEquipmentFullDescription => EliteEquipmentPickupDesc+$"\nWhen hit, gain a <style=cIsHealing>defensive boost</style> for <style=cIsHealing>{AdaptiveBoostTimer}</style> seconds, become <style=cIsUtility>invisible</style> for <style=cIsUtility>{InvisibleTimer}</style> , and gain a massive speed boost. Recharges after {AdaptiveCooldownTimer} seconds.\n" +
            $"Attacks apply 20 <style=cIsDamage>laceration</style> on hit for <style=cIsDamage>{LacerationDuration}</style> seconds, every 10 stacks increases <style=cIsDamage>incoming damage</style> by <style=cIsDamage>1</style>.";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Adaptive";

        public override GameObject EliteEquipmentModel => MainAssets.LoadAsset<GameObject>("Assets/Models/Prefavs/Elite/Gills/BothGills.prefab");//Assets/Textrures/Icons/Equipment/AdaptiveAffix/AdaptiveEquip.png

        public override GameObject EliteBodyModel => MainAssets.LoadAsset<GameObject>("Assets/Models/Prefavs/Elite/Gills/BothGills.prefab");

        public GameObject InvisibleEffect;
        public GameObject TrailEffect;

        public Texture2D RemapTexture = MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Ramps/texRampAdaptive4.png");

        public static GameObject ItemBodyModelPrefab;

        //public override Material EliteMaterial { get; set; } = MainAssets.LoadAsset<Material>("AffixPureOverlay.mat");

        public override Sprite EliteEquipmentIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Equipment/AdaptiveAffix/AdaptiveEquip.png");

        public override Sprite EliteBuffIcon => AffixAdaptiveBuff.instance.BuffIcon;

        public override float HealthMultiplier => 4f;

        public override float DamageMultiplier => 2f;

        public override int VanillaTier => 1;

        public override float CostMultiplierOfElite { get; set; } = 6;

        public static int PreHitArmorAdd=150;

        public static float CooldownReductionPreHit=1f;

        public static float CooldownReductionAfterHit=1.5f;

        public static float AttackSpeedPreHit=.15f;

        public static float AttackSpeedAfterHit=.40f;

        public static float MoveSpeedAfterHit=.50f;

        public static float MoveSpeedInvisible=2f;

        public static float AdaptiveCooldownTimer = 25f;

        public static float AdaptiveBoostTimer = 12f;

        public static float StacksOfRepulsionArmorPerHealth=1f/450f;

        public static float MinimumStacksofRepulsionArmor = 2f;

        public static float MaxStacksofRepulsionArmor = 10f;

        public static float DamageTakenModifierTimer = 8f;

        public static float InvisibleTimer = 3f;

        public static int LacerationCount = 20;

        public static float LacerationDuration = 7.5f;

        public static int MaxLaceration = 50;

        public static bool ProcIsChance = true;

        public static bool EnableInvisibility = true;

        

        /// <summary>
        /// SafegaurdPercent is equal to:SafegaurdPercent * MaxCombinedHealth =MaxDamageTakenInOneAttack
        /// </summary>
        /// <value>
        /// The Safegaurd Percent Should be in the range [0f, 1f]. The lower the vale the more protection.
        /// </value>
        public static float SafegaurdPercent = .20f;

       // public override Color EliteBuffColor { get => base.EliteBuffColor; set => base.EliteBuffColor = value; }

        //private GameObject purifiedEffect;
        //private GameObject nullifiedEffect;

        public override void Init(ConfigFile config)
        {
            EliteBuffDef = AffixAdaptiveBuff.instance.BuffDef;
            EliteBuffColor = new Color32(255, 190, 200,255);
            MakeMaterial();
            MakeInvisEffect();
            MakeTrailEffect();
            CreateConfig(config);
            CreateLang();
            //CreateAffixBuffDef();

            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
            EliteRamp.AddRamp(EliteDef,RemapTexture); 
        }

        private void MakeInvisEffect()
        {
            GameObject temp= Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2KillEffect.prefab").WaitForCompletion().InstantiateClone("InvisStart"); //Assets/Textrures/Icons/Buff/AdaptiveIconBWMap.png
            Texture2D texture = MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Icons/Buff/AdaptiveIconBWMap.png");
            ParticleSystem[] systems= temp.GetComponentsInChildren<ParticleSystem>();
            ParticleSystemRenderer[] renders = temp.GetComponentsInChildren<ParticleSystemRenderer>();

            EffectHelpers.SetParticleSystemColorOverTime(ref systems[0], EliteBuffColor);
            renders[0].GetMaterial().SetTexture("_RemapTex", RemapTexture);

            EffectHelpers.SetParticleSystemColorOverTime(ref systems[1], EliteBuffColor);
            renders[1].GetMaterial().SetTexture("_RemapTex", RemapTexture);

            EffectHelpers.SetParticleSystemColorOverTime(ref systems[2], EliteBuffColor);
            renders[2].GetMaterial().SetTexture("_RemapTex", RemapTexture);

            EffectHelpers.SetParticleSystemColorOverTime(ref systems[3], EliteBuffColor);
            renders[3].GetMaterial().SetTexture("_RemapTex", RemapTexture);

            EffectHelpers.SetParticleSystemColorOverTime(ref systems[4], EliteBuffColor);
            renders[4].GetMaterial().SetTexture("_RemapTex", RemapTexture);
            renders[4].GetMaterial().SetTexture("_MainTex", texture);
            renders[4].GetMaterial().SetColor("_EmissionColor", EliteBuffColor);

            temp.GetComponentInChildren<Light>().color = EliteBuffColor;

            InvisibleEffect = temp;
            ContentAddition.AddEffect(InvisibleEffect);
        }

        private void MakeTrailEffect()
        {
            GameObject pinkTrail= Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/FireTrail.prefab").WaitForCompletion().InstantiateClone("PinkTrail");//RoR2/Base/Common/FireTrailSegment.prefab
            GameObject pinkTrailSegment = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/FireTrailSegment.prefab").WaitForCompletion().InstantiateClone("PinkTrailSegment");

            ParticleSystem PS = pinkTrailSegment.GetComponent<ParticleSystem>();
            EffectHelpers.SetParticleSystemColorOverTime(ref PS,EliteBuffColor);
            //EffectHelpers.SetParticleSystemLightColor(ref PS, EliteBuffColor);
            ParticleSystemRenderer PSR= pinkTrailSegment.GetComponent<ParticleSystemRenderer>();
            PSR.GetMaterial().SetTexture("_RemapTex", RemapTexture);


            DamageTrail DT = pinkTrail.GetComponent<DamageTrail>();
            DT.damageUpdateInterval = 10f;
            DT.pointUpdateInterval = .5f;
            DT.damagePerSecond = 0;
            DT.pointLifetime = .3f;
            DT.segmentPrefab = pinkTrailSegment;

            TrailEffect = pinkTrail;
            //ContentAddition.AddEffect(TrailEffect);
        }

        private void MakeMaterial()
        {
            //Material mat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/maBrotherGlassOverlay.mat").WaitForCompletion());
            Material mat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matEnergyShield.mat").WaitForCompletion());


            //SKYBOX
            //Material mat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/crystalworld/matSkyboxCrystalworld.mat").WaitForCompletion());
            //mat.SetFloat("_Rotation", 0);
            //mat.SetInt("_Rotation", 0);


            mat.color = EliteBuffColor;
            mat.SetColor("_TintColor", EliteBuffColor);
            mat.SetColor("_Tint", EliteBuffColor);
            EliteMaterial = mat;
        }

        private void CreateConfig(ConfigFile config)
        {
            CostMultiplierOfElite = ConfigManager.ConfigOption<float>("Elite: " + EliteModifier, "Cost Multiplier", 6f, "Cost to spawn the elite is multiplied by this. Decrease to make the elite spawn more.");
            EnableInvisibility =  ConfigManager.ConfigOption<bool>("Elite: " + EliteModifier, "Enable Invisibility", true, "Enable Adaptive Elites to be invisible.");

        }

        private void CreateEliteTiers()
        {
            CanAppearInEliteTiers = new CombatDirector.EliteTierDef[]
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CostMultiplierOfElite,
                    eliteTypes = Array.Empty<EliteDef>(),
                    isAvailable = SetAvailability
                }
            };
        }

        private bool SetAvailability(SpawnCard.EliteRules arg)
        {
            return arg == SpawnCard.EliteRules.Default;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = EliteBodyModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, false);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            string DefaultChildName = "Head";
            Vector3 DefaultPos = new Vector3(0f, .20f, .025f);
            Vector3 DefaultAngles = new Vector3(-90f, 0f, 90f);//was 270
            Vector3 DefaultScale = new Vector3(.01f, .01f, .01f);
            ItemDisplayRule DefaultRule = new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = ItemBodyModelPrefab,
                childName = "Head",
                localPos = DefaultPos,
                localAngles = DefaultAngles,
                localScale = DefaultScale
            };

            //Perfect
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0,-.01f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //Perfect
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0,.005f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //z and y are swapped??
            //Eh its fine
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                { 
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,3f,1.89f),
                    localAngles = DefaultAngles+new Vector3(135f,0,0),
                    localScale = DefaultScale*8f
                }
            });

            //Not intended, looks cool tho
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = DefaultPos+new Vector3(0,-.5f,0),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*1.5f
                }
            });

            //It works
            rules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.5f,0),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*1.5f
                }
            });

            //Close Enough
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.2f,-.04f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*.8f
                }
            });
            //Perfect
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.05f,-.05f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*.8f
                }
            });

            //Meh but whatever
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = DefaultPos+new Vector3(0,-.05f,-.05f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*4f
                }
            });

            //Perfect
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.08f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //First try
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0,-.01f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*11f
                }
            });

            //Good
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.1f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //Good
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.1f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*.8f
                }
            });

            //MONSTERS
            //Not Bad
            rules.Add("mdlLemurian", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-1f,.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*11f
                }
            });

            //Good Enough
            rules.Add("mdlBeetle", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,.45f,-.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*4f
                }
            });

            rules.Add("mdlWisp1Mouth", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0f,-.15f),
                    localAngles = DefaultAngles+new Vector3(0f,0,180f),
                    localScale = DefaultScale*5f
                }
            });
            //Didnt apprear
            rules.Add("AcidLarva", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "mdlAcidLarvaBody",
                    localPos = DefaultPos+new Vector3(0,0f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*11f
                }
            });
            //good
            rules.Add("mdlJellyfish", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull2",
                    localPos = DefaultPos+new Vector3(0,0f,-.08f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*7f
                }
            });

            //Meh
            rules.Add("mdlFlyingVermin", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = DefaultPos+new Vector3(0,0f,-.08f),
                    localAngles = DefaultAngles+new Vector3(165f,0,0),
                    localScale = DefaultScale*5f
                }
            });

            //Nothing
            rules.Add("mdlVermin", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(100f,0f,-.08f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*4f
                }
            });
            //GoodEnough
            rules.Add("mdlHermitCrab", new RoR2.ItemDisplayRule[]
           {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = DefaultPos+new Vector3(0,.5f,-.03f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*3f
                }
           });
            //Perfect
            rules.Add("mdlVulture", new RoR2.ItemDisplayRule[]
           {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-1.05f,.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*11f
                }
           });
            //Nothing
            rules.Add("mdlMinorConstructEye", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ROOT",
                    localPos = DefaultPos+new Vector3(0,-1f,.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*11f
                }
            });

            rules.Add("mdlGolem", new RoR2.ItemDisplayRule[]
           {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Eye",
                    localPos = DefaultPos+new Vector3(0,-.05f,-.15f),
                    localAngles = DefaultAngles+new Vector3(-40f,0,0),
                    localScale = DefaultScale*3f
                }
           });

            return rules;
        }

        public override void Hooks()
        {
            
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }

        public override void OverlayManager(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            /*
            if (self && self.modelLocator && self.modelLocator.modelTransform && self.HasBuff(EliteBuffDef))
            {
                if(!self.GetComponent<EliteOverlayManager>())
                {
                    RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                    overlay.duration = float.PositiveInfinity;
                    overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    overlay.animateShaderAlpha = true;
                    overlay.destroyComponentOnEnd = true;
                    overlay.originalMaterial = EliteMaterial;
                    overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                    var EliteOverlayManager = self.gameObject.AddComponent<EliteOverlayManager>();
                    EliteOverlayManager.Overlay = overlay;
                    EliteOverlayManager.Body = self;
                    EliteOverlayManager.EliteBuffDef = EliteBuffDef;
                }
                else if (!self.HasBuff(AdaptiveBoost.instance.BuffDef) && self.GetComponent<EliteOverlayManager>().enabled ==true)
                {
                    self.GetComponent<EliteOverlayManager>().enabled = false;
                    self.modelLocator.modelTransform.gameObject.GetComponent<RoR2.TemporaryOverlay>().duration=0;
                }
                else if(self.GetComponent<EliteOverlayManager>().enabled == false && self.HasBuff(AdaptiveBoost.instance.BuffDef))
                {
                    RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                    overlay.duration = float.PositiveInfinity;
                    overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    overlay.animateShaderAlpha = true;
                    overlay.destroyComponentOnEnd = true;
                    overlay.originalMaterial = EliteMaterial;
                    overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());

                    self.GetComponent<EliteOverlayManager>().enabled = true;
                    self.GetComponent<EliteOverlayManager>().Overlay = overlay;
                }
            }
            */
            orig(self);
        }
    }

    class AffixAdaptiveBuff : BuffBase<AffixAdaptiveBuff>
    {
        public override string BuffName => "Adaptive";

        public override Color Color => Color.white;

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Buff/Status_AffixAdaptive.png");//Assets/Textrures/Icons/Buff/Status_AffixAdaptive.png

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (victim && damageInfo!=null && self && damageInfo.attacker)
            {
                CharacterBody body = victim.GetComponent<CharacterBody>();
                CharacterBody attacker= damageInfo.attacker.GetComponent<CharacterBody>();
                if (body != null &&  attacker!= null && attacker.HasBuff(BuffDef))
                {
                    float chance = 1;
                    if (AffixAdaptive.ProcIsChance)
                        chance = damageInfo.procCoefficient;

                    if (Util.CheckRoll(chance*100f, attacker.master))
                    {
                        int stackCount = AffixAdaptive.LacerationCount;
                        float duration = AffixAdaptive.LacerationDuration * damageInfo.procCoefficient;

                        for (int i = 0; i < stackCount; i++)
                            body.AddTimedBuff(Laceration.instance.BuffDef, duration, AffixAdaptive.MaxLaceration);
                    }
                }
            }
            orig(self, damageInfo, victim);
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            //If you dont have the dcooldown and they have the Affix
            if(self && self.HasBuff(BuffDef) && !self.HasBuff(AdaptiveCooldown.instance.BuffDef))
            {
                /*
                if(!self.HasBuff(RoR2Content.Buffs.ArmorBoost))
                {
                    self.AddBuff(RoR2Content.Buffs.ArmorBoost);

                    for (int i = 0; i < 80; i++)
                        self.AddBuff(Safegaurd.instance.BuffDef);
                }
                */
                //If they dont have adaptive boost, give it
                if(!self.HasBuff(AdaptiveBoost.instance.BuffDef))
                {
                    self.AddBuff(AdaptiveBoost.instance.BuffDef);
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {

            orig(self, damageInfo);

            //if you have adaptive boost and no cooldown, give cooldown and put adaptive boost on a timer and give cooldown
            if(self!=null && damageInfo!=null && self.body && self.body.HasBuff(AdaptiveBoost.instance.BuffDef) && !self.body.HasBuff(AdaptiveCooldown.instance.BuffDef))
            {
                self.body.RemoveBuff(AdaptiveBoost.instance.BuffDef);

                for (int i = 0; i < AffixAdaptive.AdaptiveBoostTimer; i++)
                    self.body.AddTimedBuff(AdaptiveBoost.instance.BuffDef, i);//10f


                //for (int i = 0; i < 4; i++)
                //    self.body.AddTimedBuff(Fortified.instance.BuffDef, 5f);
                if(AffixAdaptive.EnableInvisibility)
                    self.body.AddTimedBuff(RoR2Content.Buffs.AffixHauntedRecipient, AffixAdaptive.InvisibleTimer);

                for (int i = 0; i < AffixAdaptive.AdaptiveCooldownTimer; i++)
                    self.body.AddTimedBuff(AdaptiveCooldown.instance.BuffDef, i);

                EffectManager.SimpleEffect(AffixAdaptive.instance.InvisibleEffect, self.body.transform.position, Quaternion.identity, transmit: true);

                

                //for (int i = 0; i < 80; i++)
                //    self.body.RemoveBuff(Safegaurd.instance.BuffDef);

                //for (int i = 0; i < 80; i++)
                //    self.body.AddTimedBuff(Safegaurd.instance.BuffDef,5f);
            }
        }

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }
    }

    class AdaptiveBoost : BuffBase<AdaptiveBoost>
    {
        public override string BuffName => "Adaptive Boost";

        public override Color Color => new Color32(255, 190, 200, 255);

        public override bool CanStack => true;

        public override bool IsDebuff => false;

        public override bool IsCooldown => false;

        public override Sprite BuffIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffGenericShield.tif").WaitForCompletion();

        public override void Hooks()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.AddBuff_BuffIndex += AddOverlay;
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex += RemoveOverlay;
        }

        private void RemoveOverlay(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            if (self && self.modelLocator && self.modelLocator.modelTransform && buffType == BuffDef.buffIndex && self.GetBuffCount(BuffDef) == 1) //maybeBuffIndexProblem
            {
                AdaptiveOverlayTracker AOT;
                if (!self.TryGetComponent<AdaptiveOverlayTracker>(out AOT))
                {
                    AOT = self.gameObject.AddComponent<AdaptiveOverlayTracker>();
                }
                if (AOT.hasOverlay == true)
                {
                    AOT.hasOverlay = false;
                    //self.modelLocator.modelTransform.gameObject.GetComponent<RoR2.TemporaryOverlayInstance>().duration = 0;
                    AOT.TempOverlay.duration= 0;
                    
                }
                if(AOT.PinkTrail!=null)
                {
                    GameObject.Destroy(((Component)AOT.PinkTrail));
                    AOT.PinkTrail = null;
                }
            }

            orig(self, buffType);
        }


        private void AddOverlay(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            if (self && self.modelLocator && self.modelLocator.modelTransform && buffType == BuffDef.buffIndex) //maybeBuffIndexProblem
            {
                AdaptiveOverlayTracker AOT;
                if (!self.TryGetComponent<AdaptiveOverlayTracker>(out AOT))
                {
                    AOT = self.gameObject.AddComponent<AdaptiveOverlayTracker>();
                }
                if (AOT.hasOverlay == false)
                {
                    //RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                    //overlay.duration = float.PositiveInfinity;
                    //overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    //overlay.animateShaderAlpha = true;
                    //overlay.destroyComponentOnEnd = true;
                    //overlay.originalMaterial = AffixAdaptive.instance.EliteMaterial;
                    //overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                    TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(self.modelLocator.modelTransform.gameObject);
                    temporaryOverlayInstance.duration = float.PositiveInfinity;
                    temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlayInstance.animateShaderAlpha = true;
                    temporaryOverlayInstance.destroyComponentOnEnd = true;
                    temporaryOverlayInstance.originalMaterial = AffixAdaptive.instance.EliteMaterial;
                    temporaryOverlayInstance.AddToCharacterModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());
                    AOT.TempOverlay= temporaryOverlayInstance;

                    AOT.hasOverlay = true;
                }
                if (AOT.PinkTrail == null)
                {
                    AOT.PinkTrail = GameObject.Instantiate<GameObject>(AffixAdaptive.instance.TrailEffect,self.transform).GetComponent<DamageTrail>();
                    ((Component) AOT.PinkTrail).transform.position = self.footPosition;
                    AOT.PinkTrail.owner = self.gameObject;
                    AOT.PinkTrail.radius *= self.radius;
                }

            }

            orig(self, buffType);
        }


        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(BuffDef))
            {
                int count = 1;//sender.GetBuffCount(BuffDef);
                bool isInvisible = sender.HasBuff(RoR2Content.Buffs.AffixHauntedRecipient);
                bool startedCooldown = sender.HasBuff(AdaptiveCooldown.instance.BuffDef);

                if (!startedCooldown)
                {
                    args.armorAdd += AffixAdaptive.PreHitArmorAdd;
                    args.cooldownReductionAdd += AffixAdaptive.CooldownReductionPreHit * count;
                    args.attackSpeedMultAdd += AffixAdaptive.AttackSpeedPreHit * count;
                }
                else
                {
                    args.cooldownReductionAdd += AffixAdaptive.CooldownReductionAfterHit * count;
                    args.attackSpeedMultAdd += AffixAdaptive.AttackSpeedAfterHit * count;

                    if (!isInvisible)
                        args.moveSpeedMultAdd += AffixAdaptive.MoveSpeedAfterHit * count;
                    else
                        args.moveSpeedMultAdd += AffixAdaptive.MoveSpeedInvisible;
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool usedOrig = false;
            if (self)
            {
                if (self.body && self.body.HasBuff(BuffDef))
                {

                    int buffCount = Math.Max(self.body.GetBuffCount(BuffDef), 1);
                    //IF the cooldown is above AdaptiveBoostTimer-DamageTakenModifierTimer     OR     the buff is on AND they dont have the cooldown
                    if (buffCount > (AffixAdaptive.AdaptiveBoostTimer - AffixAdaptive.DamageTakenModifierTimer) || (buffCount == 1 && !self.body.HasBuff(AdaptiveCooldown.instance.BuffDef)))
                    {
                        //Repultion Armor Stacks
                        int stacks = (int)Math.Ceiling(Math.Max(AffixAdaptive.MinimumStacksofRepulsionArmor, AffixAdaptive.StacksOfRepulsionArmorPerHealth * self.combinedHealth));
                        stacks = (int) Math.Min(AffixAdaptive.MaxStacksofRepulsionArmor, stacks);
                        self.itemCounts.armorPlate += stacks;

                        //Max Damage per Attack
                        float maxDamage = AffixAdaptive.SafegaurdPercent * self.fullCombinedHealth;
                        if (damageInfo.damage > maxDamage)
                            damageInfo.damage = maxDamage;


                        orig(self, damageInfo);
                        usedOrig = true;

                        self.itemCounts.armorPlate -= stacks;
                    }
                }
            }
            //if Orig wasnt called, call it
            if (!usedOrig)
                orig(self, damageInfo);
        }

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public class AdaptiveOverlayTracker : MonoBehaviour
        {
            public bool hasOverlay = false;
            public DamageTrail PinkTrail=null;

            public TemporaryOverlayInstance TempOverlay { get; internal set; }
        }

        
    }
    class AdaptiveCooldown : BuffBase<AdaptiveCooldown>
    {
        public override string BuffName => "Adaptive Cooldown";

        public override Color Color => Color.gray;

        public override bool CanStack => true;

        public override bool IsDebuff => false;

        public override bool IsCooldown => true;

        public override bool IsHidden => true;

        public override Sprite BuffIcon => base.BuffIcon;

        public override void Hooks()
        {

        }

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }
    }

    class ZetAdaptiveDrop : ItemBase<ZetAdaptiveDrop>
    {
        public override string ItemName => AffixAdaptive.instance.EliteEquipmentName;

        public override string ItemLangTokenName => "ZET"+AffixAdaptive.instance.EliteAffixToken;

        public override string ItemPickupDesc => AffixAdaptive.instance.EliteEquipmentPickupDesc;

        public override string ItemFullDescription => AffixAdaptive.instance.EliteEquipmentFullDescription;

        public override string ItemLore => AffixAdaptive.instance.EliteEquipmentLore;

        public static GameObject ItemBodyModelPrefab;
        public  GameObject EliteBodyModel => AffixAdaptive.instance.EliteBodyModel;
        public override GameObject ItemModel => AffixAdaptive.instance.EliteEquipmentDef.pickupModelPrefab;
        public override Sprite ItemIcon => AffixAdaptive.instance.EliteEquipmentIcon;

        public override ItemTier Tier => ItemTier.Boss;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override bool BlacklistFromPreLoad => true;

        public override ItemTag[] ItemTags => new ItemTag[] {ItemTag.WorldUnique, ItemTag.CannotDuplicate };
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = EliteBodyModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, false);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            string DefaultChildName = "Head";
            Vector3 DefaultPos = new Vector3(0f, .20f, .025f);
            Vector3 DefaultAngles = new Vector3(-90f, 0f, 90f);//was 270
            Vector3 DefaultScale = new Vector3(.01f, .01f, .01f);
            ItemDisplayRule DefaultRule = new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = ItemBodyModelPrefab,
                childName = "Head",
                localPos = DefaultPos,
                localAngles = DefaultAngles,
                localScale = DefaultScale
            };

            //Perfect
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0,-.01f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //Perfect
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0,.005f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //z and y are swapped??
            //Eh its fine
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,3f,1.89f),
                    localAngles = DefaultAngles+new Vector3(135f,0,0),
                    localScale = DefaultScale*8f
                }
            });

            //Not intended, looks cool tho
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = DefaultPos+new Vector3(0,-.5f,0),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*1.5f
                }
            });

            //It works
            rules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.5f,0),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*1.5f
                }
            });

            //Close Enough
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.2f,-.04f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*.8f
                }
            });
            //Perfect
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.05f,-.05f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*.8f
                }
            });

            //Meh but whatever
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = DefaultPos+new Vector3(0,-.05f,-.05f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*4f
                }
            });

            //Perfect
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.08f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //First try
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0,-.01f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*11f
                }
            });

            //Good
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.1f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale
                }
            });

            //Good
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-.1f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*.8f
                }
            });

            //MONSTERS
            //Not Bad
            rules.Add("mdlLemurian", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-1f,.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*11f
                }
            });

            //Good Enough
            rules.Add("mdlBeetle", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,.45f,-.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*4f
                }
            });

            rules.Add("mdlWisp1Mouth", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,0f,-.15f),
                    localAngles = DefaultAngles+new Vector3(0f,0,180f),
                    localScale = DefaultScale*5f
                }
            });
            //Didnt apprear
            rules.Add("AcidLarva", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "mdlAcidLarvaBody",
                    localPos = DefaultPos+new Vector3(0,0f,-.08f),
                    localAngles = DefaultAngles,
                    localScale = DefaultScale*11f
                }
            });
            //good
            rules.Add("mdlJellyfish", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hull2",
                    localPos = DefaultPos+new Vector3(0,0f,-.08f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*7f
                }
            });

            //Meh
            rules.Add("mdlFlyingVermin", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Body",
                    localPos = DefaultPos+new Vector3(0,0f,-.08f),
                    localAngles = DefaultAngles+new Vector3(165f,0,0),
                    localScale = DefaultScale*5f
                }
            });

            //Nothing
            rules.Add("mdlVermin", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(100f,0f,-.08f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*4f
                }
            });
            //GoodEnough
            rules.Add("mdlHermitCrab", new RoR2.ItemDisplayRule[]
           {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = DefaultPos+new Vector3(0,.5f,-.03f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*3f
                }
           });
            //Perfect
            rules.Add("mdlVulture", new RoR2.ItemDisplayRule[]
           {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = DefaultPos+new Vector3(0,-1.05f,.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*11f
                }
           });
            //Nothing
            rules.Add("mdlMinorConstructEye", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ROOT",
                    localPos = DefaultPos+new Vector3(0,-1f,.1f),
                    localAngles = DefaultAngles+new Vector3(180f,0,0),
                    localScale = DefaultScale*11f
                }
            });

            rules.Add("mdlGolem", new RoR2.ItemDisplayRule[]
           {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Eye",
                    localPos = DefaultPos+new Vector3(0,-.05f,-.15f),
                    localAngles = DefaultAngles+new Vector3(-40f,0,0),
                    localScale = DefaultScale*3f
                }
           });

            return rules;
        }

        public override void Init(ConfigFile config)
        {

            //CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            bool hasItem = false;
            orig(self);
            if (self && self.inventory)
            {
                if (!self.HasBuff(AffixAdaptiveBuff.instance.BuffDef))
                {
                    int count = GetCount(self);
                    if (count > 0)
                    {
                        self.AddBuff(AffixAdaptiveBuff.instance.BuffDef);
                        CharacterModel model = self.modelLocator.modelTransform.GetComponent<CharacterModel>();
                        if(model && model.myEliteIndex==EliteIndex.None)
                        {
                            model.myEliteIndex = AffixAdaptive.instance.EliteDef.eliteIndex;
                            model.shaderEliteRampIndex = (int)AffixAdaptive.instance.EliteDef.eliteIndex;
                        }
                        hasItem = true;
                    }
                }
                else if (GetCount(self)<=0 && self.inventory.currentEquipmentIndex != AffixAdaptive.instance.EliteEquipmentDef.equipmentIndex)
                {
                    self.RemoveBuff(AffixAdaptiveBuff.instance.BuffDef);
                    self.modelLocator.modelTransform.GetComponent<CharacterModel>().myEliteIndex = AffixAdaptive.instance.EliteDef.eliteIndex;
                    self.modelLocator.modelTransform.GetComponent<CharacterModel>().shaderEliteRampIndex = (int)AffixAdaptive.instance.EliteDef.eliteIndex;
                }
            }
        }
    }
}