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

namespace BransItems.Modules.Pickups.Items.Tier2
{
    class AdaptiveArmor : ItemBase<AdaptiveArmor>
    {
        public override string ItemName => "Adaptive Armor";
        public override string ItemLangTokenName => "ADAPTIVE_ARMOR";
        public override string ItemPickupDesc => "After being hit twice in a short amount of time, receive a huge flat damage reduction from all attacks.";
        public override string ItemFullDescription => $"After taking damage twice in <style=cIsDamage>{DamageWindow}</style> seconds, " +
            //$"gain <style=cIsDamage>{InitialFortify}</style><style=cStack>(+{AdditionalFortify})</style> Fortify for <style=cIsDamage>{InitialFortifyTime}</style><style=cStack>(+{AdditionalFortifyTime})</style> seconds. Each stack of Fortify reduces all incoming damage by 5, but not below 1. Refreshes <style=cIsDamage>{CooldownTime}</style> seconds after triggering.";
            $"reduce all <style=cIsDamage>incoming damage</style> by <style=cIsDamage>{5f* InitialFortify} </style><style=cStack>(+{AdditionalFortify*5f} per stack)</style> for <style=cIsDamage>{InitialFortifyTime} </style><style=cStack>(+{AdditionalFortifyTime} per stack)</style> seconds. Cannot be reduced below <style=cIsDamage>1</style>. Recharges after <style=cIsDamage>{CooldownTime}</style> seconds.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/AdaptiveArmor/AArmor.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/AdaptiveArmor/AAIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };


        public static float DamageWindow;
        public static int InitialFortify;
        public static int AdditionalFortify;
        public static float InitialFortifyTime;
        public static float AdditionalFortifyTime;
        public static float CooldownTime;

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
            DamageWindow = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Time Window", 2f, "How long is the time window between the first and second hit?");
            InitialFortify = ConfigManager.ConfigOption<int>("Item: " + ItemName, "Initial Fortify Count", 5, "How many stacks of Fortify does the fist stack give?");
            AdditionalFortify = ConfigManager.ConfigOption<int>("Item: " + ItemName, "Additional Fortify Count", 2, "How many addtional stacks of Fortify does Adaptive Armor give after the fist stack?");
            InitialFortifyTime = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Initial Fortify Duration", 10f, "How many seconds is the duration of Fortify for the first stack of Adaptive Armor?");
            AdditionalFortifyTime = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Additional Fortify Duration", 1f, "How many additional seconds is the duration of Fortify for Adaptive Armor after the first stack?");
            CooldownTime = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Duration of cooldown", 20f, "How many  seconds is the duration of Fortify for Adaptive Armor after the first stack?");
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
            //On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            //On.RoR2.CharacterMaster.OnItemAddedClient += CharacterMaster_OnItemAddedClient;
            //TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            //CharacterBody.instancesList.
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            //On.RoR2.EquipmentSlot.OnEquipmentExecuted += EquipmentSlot_OnEquipmentExecuted;
        }


        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            //If Self exists
            if (self)
            {
                bool flag = false;
                //Check to see if the Tracket Exists
                var cpt = self.GetComponent<AdaptiveArmorTracker>();
                if (cpt) //cpt = self.gameObject.AddComponent<FortifiedTracker>();
                {
                    flag = true;
                }
                int currentCount = self.inventory.GetItemCount(ItemDef.itemIndex);
                if (currentCount > 0)
                {
                    int stacks = InitialFortify + (currentCount - 1) * AdditionalFortify;
                    float duration = InitialFortifyTime + (currentCount - 1f) * AdditionalFortifyTime;
                    if (!flag)
                    {
                        cpt = self.gameObject.AddComponent<AdaptiveArmorTracker>();
                        cpt.Init(stacks, duration, CooldownTime, DamageWindow);
                        self.AddBuff(AdaptiveArmourReady.instance.BuffDef);

                    }
                    else if(cpt.StackCount!=stacks)
                    {
                        cpt.Init(stacks, duration, CooldownTime, DamageWindow);
                        //self.AddBuff(AdaptiveArmourReady.instance.BuffDef);
                    }
                    
                }
                else
                {
                    AdaptiveArmorTracker.Destroy(cpt);
                }
            }
        }

    }

    public class AdaptiveArmorCooldown : BuffBase<AdaptiveArmorCooldown>
    {
        public override string BuffName => "Adaptive Armor Cooldown";

        public override Color Color => new Color32(250, 250, 250, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/AdaptiveArmor/AACooldownIcon.png");
        public virtual bool CanStack => true;
        public virtual bool IsDebuff => true;

        public virtual bool IsCooldown => true;

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
    public class AdaptiveArmourReady : BuffBase<AdaptiveArmourReady>
    {
        public override string BuffName => "Adaptive Armor Ready";

        public override Color Color => new Color32(250,250,250, 255);

       public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/AdaptiveArmor/AAReadyIcon.png");
        public virtual bool CanStack => false;
        public virtual bool IsDebuff => false;

        public virtual bool IsCooldown => false;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
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
                    if (!self.HasBuff(AdaptiveArmorCooldown.instance.BuffDef)) // && !self.HasBuff(Fortified.instance.BuffDef))
                    {
                        var cpt = self.GetComponent<AdaptiveArmorTracker>();
                        if (cpt) //cpt = self.gameObject.AddComponent<FortifiedTracker>();
                        {
                            self.AddBuff(BuffDef);
                        }

                    }
                }
                else
                {
                    var cpt = self.GetComponent<AdaptiveArmorTracker>();
                    if (cpt) //cpt = self.gameObject.AddComponent<FortifiedTracker>();
                    {
                        cpt.FixedUpdate();
                    }
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (self)
            {
                //ModLogger.LogWarning("Trigger1");
                if (self.body && self.body.GetBuffCount(BuffDef) > 0)
                {
                    var cpt = self.GetComponent<AdaptiveArmorTracker>();
                    if (cpt) //cpt = self.gameObject.AddComponent<FortifiedTracker>();
                    {
                        if (cpt.Hit())
                        {
                            self.body.RemoveBuff(BuffDef);
                            //ModLogger.LogWarning("Stacks " + cpt.StackCount);
                            for (int i = 0; i < cpt.StackCount; i++)
                            {
                                //ModLogger.LogWarning("stack:" + i + 1);
                                self.body.AddTimedBuff(Fortified.instance.BuffDef, cpt.BuffDuration);
                            }
                            self.body.AddTimedBuff(AdaptiveArmorCooldown.instance.BuffDef, cpt.CooldownDuration);
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
    public class AdaptiveArmorTracker : MonoBehaviour
    {
        public float CooldownDuration;
        public float BuffDuration;
        public int StackCount;
        public float lastTimeHitDuration;
        public float HitTwiceWindow;
        public void Init(int stackCount, float buffDuration, float cooldownDuration, float hitTwiceWindow = 2f)
        {
            StackCount = stackCount;
            BuffDuration = buffDuration;
            CooldownDuration = cooldownDuration;
            HitTwiceWindow = hitTwiceWindow;
            lastTimeHitDuration = HitTwiceWindow;
        }

        public bool Hit()
        {
            float prevDuration = lastTimeHitDuration;
            lastTimeHitDuration = HitTwiceWindow;
            if (prevDuration>0)
                return true;
            return false;
        }

        internal void FixedUpdate()
        {
            lastTimeHitDuration = Math.Max(lastTimeHitDuration - Time.fixedDeltaTime, 0);
        }
    }

}
