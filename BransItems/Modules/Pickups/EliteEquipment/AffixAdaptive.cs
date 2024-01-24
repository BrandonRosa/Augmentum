using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BransItems.BransItems;

namespace BransItems.Modules.Pickups.EliteEquipments
{
    class AffixAdaptive : EliteEquipmentBase<AffixAdaptive>
    {
        public override string EliteEquipmentName => "Its Whispered Secrets";

        public override string EliteAffixToken => "AFFIX_ADAPTIVE";

        public override string EliteEquipmentPickupDesc => "Become an aspect of purity.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Adaptive";

        public override GameObject EliteEquipmentModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

        public override Material EliteMaterial { get; set; } = MainAssets.LoadAsset<Material>("AffixPureOverlay.mat");

        public override Sprite EliteEquipmentIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public override Sprite EliteBuffIcon => MainAssets.LoadAsset<Sprite>("NullifyingBuffIcon.png");

        public override float HealthMultiplier => 2f;

        public override float DamageMultiplier => 2f;

        public override float CostMultiplierOfElite => 5f;

        //private GameObject purifiedEffect;
        //private GameObject nullifiedEffect;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateEquipment();
            CreateEliteTiers();
            CreateElite();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            CostMultiplierOfElite = config.Bind<float>("Elite: " + EliteModifier, "Cost Multiplier", 5.2f, "Cost to spawn the elite is multiplied by this. Decrease to make the elite spawn more.").Value;

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
            //// Removes debuffs from self
            //On.RoR2.CharacterBody.UpdateBuffs += (orig, self, deltaTime) =>
            //{
            //    if (self.HasBuff(EliteBuffDef))
            //    {
            //        /*
            //        bool check = false;
            //        foreach (BuffIndex i in self.activeBuffsList)
            //        {
            //            if (BuffCatalog.GetBuffDef(i).isDebuff && !BuffCatalog.GetBuffDef(i).isHidden)
            //            {
            //                if (Plugin.DEBUG) {
            //                    Log.LogInfo(BuffCatalog.GetBuffDef(i).name);
            //                }
            //                check = true;
            //            }
            //        }
            //        if (check)
            //        {
            //            EffectData effectData = new EffectData
            //            {
            //                origin = self.transform.position
            //            };
            //            EffectManager.SpawnEffect(purifiedEffect, effectData, true);
            //        }
            //        */
            //        Util.CleanseBody(self, true, false, false, true, true, false);
            //    }
            //    orig(self, deltaTime);
            //};

            //// Removes buffs from hit enemies
            //On.RoR2.GlobalEventManager.OnHitAll += (orig, self, DamageInfo, hitObject) =>
            //{
            //    if (DamageInfo.attacker && hitObject.GetComponent<CharacterBody>())
            //    {
            //        if (DamageInfo.attacker.GetComponent<CharacterBody>().HasBuff(EliteBuffDef))
            //        {
            //            CharacterBody body = hitObject.GetComponent<CharacterBody>();
            //            bool check = false;
            //            foreach (BuffIndex i in body.activeBuffsList)
            //            {
            //                if (!BuffCatalog.GetBuffDef(i).isDebuff && !BuffCatalog.GetBuffDef(i).isCooldown)
            //                {
            //                    check = true;
            //                }
            //            }
            //            if (check)
            //            {
            //                EffectData effectData = new EffectData
            //                {
            //                    origin = body.transform.position
            //                };
            //                EffectManager.SpawnEffect(nullifiedEffect, effectData, true);
            //            }

            //            Util.CleanseBody(body, false, true, false, false, false, false);
            //        }
            //    }
            //};
        }

        //If you want an on use effect, implement it here as you would with a normal equipment.
        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}