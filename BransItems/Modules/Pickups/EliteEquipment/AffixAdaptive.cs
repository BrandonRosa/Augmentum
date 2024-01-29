using BepInEx.Configuration;
using BransItems.Modules.StandaloneBuffs;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static BransItems.BransItems;

namespace BransItems.Modules.Pickups.EliteEquipments
{
    class AffixAdaptive : EliteEquipmentBase<AffixAdaptive>
    {
        public override string EliteEquipmentName => "Its Whispered Secrets";

        public override string EliteAffixToken => "AFFIX_ADAPTIVE";

        public override string EliteEquipmentPickupDesc => "Become an aspect of evolution.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Adaptive";

        public override GameObject EliteEquipmentModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        //public override Material EliteMaterial { get; set; } = MainAssets.LoadAsset<Material>("AffixPureOverlay.mat");

        public override Sprite EliteEquipmentIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override Sprite EliteBuffIcon => AffixAdaptiveBuff.instance.BuffIcon;

        public override float HealthMultiplier => 4f;

        public override float DamageMultiplier => 2f;

        public override float CostMultiplierOfElite => 3f;

        public static int PreHitArmorAdd;

        public static float CooldownReductionPreHit;

        public static float CooldownReductionAfterHit;

        public static float AttackSpeedPreHit=.15f;

        public static float AttackSpeedAfterHit=.40f;

        public static float MoveSpeedAfterHit=.50f;

        public static float MoveSpeedInvisible=2f;

        public static float AdaptiveCooldownTimer = 30f;

        public static float AdaptiveBoostTimer = 10f;

        public static float StacksOfRepulsionArmorPerHealth=1f/400f;

        public static float MinimumStacksofRepulsionArmor = 4f;

        public static float DamageTakenModifierTimer = 5f;

        public static float InvisibleTimer = 3.5f;

        public static int LacerationCount = 20;

        public static float LacerationDuration = 5f;

        public static int MaxLaceration = 50;

        public static bool ProcIsChance = true;

        /// <summary>
        /// SafegaurdPercent is equal to:SafegaurdPercent * MaxCombinedHealth =MaxDamageTakenInOneAttack
        /// </summary>
        /// <value>
        /// The Safegaurd Percent Should be in the range [0f, 1f]. The lower the vale the more protection.
        /// </value>
        public static float SafegaurdPercent = .30f;

       // public override Color EliteBuffColor { get => base.EliteBuffColor; set => base.EliteBuffColor = value; }

        //private GameObject purifiedEffect;
        //private GameObject nullifiedEffect;

        public override void Init(ConfigFile config)
        {
            EliteBuffDef = AffixAdaptiveBuff.instance.BuffDef;
            EliteBuffColor = new Color32(255, 190, 200,255);
            MakeMaterial();
            CreateConfig(config);
            CreateLang();
            //CreateAffixBuffDef();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
            EliteRamp.AddRamp(EliteDef, MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Ramps/texRampAdaptive4.png"));
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
            CostMultiplierOfElite = config.Bind<float>("Elite: " + EliteModifier, "Cost Multiplier", 4f, "Cost to spawn the elite is multiplied by this. Decrease to make the elite spawn more.").Value;
            PreHitArmorAdd = config.Bind<int>("Elite: " + EliteModifier, "Amount of Armor", 100, "Armor added to elite before getting hit for the first time.").Value;
            CooldownReductionPreHit= config.Bind<float>("Elite: " + EliteModifier, "Cooldown Reduction", 1f, "Cooldown reduction in seconds before getting hit.").Value;
            CooldownReductionAfterHit = config.Bind<float>("Elite: " + EliteModifier, "Cooldown Reduction", 1.5f, "Cooldown reduction in seconds during boost.").Value;

            //purifiedEffect = Plugin.assetBundle.LoadAsset<GameObject>("PureEffect.prefab");
            //ContentAddition.AddEffect(purifiedEffect);

            //nullifiedEffect = Plugin.assetBundle.LoadAsset<GameObject>("NullEffect.prefab");
            //ContentAddition.AddEffect(nullifiedEffect);
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
            return new ItemDisplayRuleDict();
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
            if (victim)
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
                self.body.AddTimedBuff(RoR2Content.Buffs.AffixHauntedRecipient, AffixAdaptive.InvisibleTimer);

                for (int i = 0; i < AffixAdaptive.AdaptiveCooldownTimer; i++)
                    self.body.AddTimedBuff(AdaptiveCooldown.instance.BuffDef, i);

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
                    self.modelLocator.modelTransform.gameObject.GetComponent<RoR2.TemporaryOverlay>().duration = 0;
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
                    RoR2.TemporaryOverlay overlay = self.modelLocator.modelTransform.gameObject.AddComponent<RoR2.TemporaryOverlay>();
                    overlay.duration = float.PositiveInfinity;
                    overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    overlay.animateShaderAlpha = true;
                    overlay.destroyComponentOnEnd = true;
                    overlay.originalMaterial = AffixAdaptive.instance.EliteMaterial;
                    overlay.AddToCharacerModel(self.modelLocator.modelTransform.GetComponent<RoR2.CharacterModel>());

                    AOT.hasOverlay = true;
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

        }

        
    }
    class AdaptiveCooldown : BuffBase<AdaptiveCooldown>
    {
        public override string BuffName => "Adaptive Cooldown";

        public override Color Color => Color.gray;

        public override bool CanStack => true;

        public override bool IsDebuff => false;

        public override bool IsCooldown => true;

        public override bool IsHidden => false;

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
}