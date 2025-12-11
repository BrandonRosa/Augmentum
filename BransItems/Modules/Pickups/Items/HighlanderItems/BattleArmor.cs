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
    class BattleArmor : ItemBase<CurvedHorn>
    {
        public override string ItemName => "Platinum Fists";
        public override string ItemLangTokenName => "PLATINUM_FISTS";
        public override string ItemPickupDesc => "While only using your Primary skill, increase attack speed and armor.";
        public override string ItemFullDescription => $"While using your <style=cIsUtility>Primary skill</style>, gain <style=cIsDamage>{AttackSpeedIncrease}% attack speed</style> and <style=cIsHealing>{ArmorIncrease} armor</style> every second up to {MaxSeconds} seconds."+FinishDescription();

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/PlatinumFists/PlatinumFistsModel.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/PlatinumFists/PlatinumFistsIcon.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };


        public enum PunishTypes { Reset, SubtractTime, Divide, IncreaseLossRate };

        public static float AttackSpeedIncrease => AttackSpeedIncreaseEntry.Value;
        public static ConfigEntry<float> AttackSpeedIncreaseEntry;
        public static float ArmorIncrease => ArmorIncreaseEntry.Value;
        public static ConfigEntry<float> ArmorIncreaseEntry;
        public static float MaxSeconds => MaxSecondsEntry.Value;
        public static ConfigEntry<float> MaxSecondsEntry;
        public static float DecelerationRate => DecelerationRateEntry.Value;
        public static ConfigEntry<float> DecelerationRateEntry;
        public static bool EnableTech => EnableTechEntry.Value;
        public static ConfigEntry<bool> EnableTechEntry;
        public static PunishTypes PunishType => PunishTypeEntry.Value;
        public static ConfigEntry<PunishTypes> PunishTypeEntry;
        public static float PunishSubractSeconds => PunishSubractSecondsEntry.Value;
        public static ConfigEntry<float> PunishSubractSecondsEntry;
        public static float PunishDivisorValue => PunishDivisorValueEntry.Value;
        public static ConfigEntry<float> PunishDivisorValueEntry;
        public static float PunishLossRateMultiplier => PunishLossRateMultiplierEntry.Value;
        public static ConfigEntry<float> PunishLossRateMultiplierEntry;

        public static int MaxStacks=100;

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
            AttackSpeedIncreaseEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Attack speed increase", 4f, "How much attack speed per second should this item give?");
            ArmorIncreaseEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Armor increase", 1f, "How much armor should this item grant?");
            MaxSecondsEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Maximum Seconds", 20f, "How long does this effect take to get to max potential?");
            DecelerationRateEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Deceleration Rate", .5f, "How fast does this item lose its stacks of Platinum Surge? (.3=30% as fast as the gain rate, 0 means no stacks are lost)");

            EnableTechEntry = ConfigManager.ConfigOption<bool>("Item: " + ItemName, "Enable Tech", false , "As long as the primary fire button is held, you wont be punished. Enable this behavior?");

            PunishTypeEntry = ConfigManager.ConfigOption<PunishTypes>("Item: " + ItemName, "Other Skill Punishment Type", PunishTypes.IncreaseLossRate, "How should the item react when using non-primary abilities?");

            PunishSubractSecondsEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Subract Type", 10f, "How many seconds will the Subract Punish Type remove?");
            PunishDivisorValueEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Divide Type", 2f, "How much should the divide punish type divide the current stacks of Platinum Surge?");
            PunishLossRateMultiplierEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Increase Loss Type", 5f, "What should the multiplier be for the LossRate Punish Type? (5=-5xGrowthRate. MaxSeconds/LossRatePunishMultiplier=Seconds to loose max stacks)");

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

        public static string FinishDescription()
        {
            string ans = $" After ";
            if (EnableTech)
                ans += "only ";
            ans+="using a <style=cIsUtility>different skill</style>, ";

            switch(PunishType)
            {
                case PunishTypes.SubtractTime:
                    ans += $" remove {PunishSubractSeconds} seconds from the bonus.";
                    break;
                case PunishTypes.Divide:
                    ans += $" divide the bonus by {PunishDivisorValue}.";
                    break;
                case PunishTypes.IncreaseLossRate:
                    ans += $" quickly lose the bonus until the primary skill is used again.";
                    break;
                case PunishTypes.Reset:
                default:
                    ans += $" reset the bonus to 0.";
                    break;
            }

            return ans;
        }

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (self && self.inventory && GetCount(self)>0)
            {
                var cpt = self.master.GetComponent<BattleArmorTracker>();
                if(cpt)
                {
                    bool JustDownPunish = (self.inputBank.skill2.justPressed || self.inputBank.skill3.justPressed || self.inputBank.skill4.justPressed);
                    bool DownPunish = self.inputBank.skill2.down || self.inputBank.skill3.down || self.inputBank.skill4.down;
                    if (EnableTech)
                    {
                        if (self.inputBank.skill1.down)
                        {
                            cpt.UsePunishDecreaseVelocity = false;
                            cpt.Increase(Time.fixedDeltaTime);
                            int buffCountCurrent = self.GetBuffCount(BattleSurge.instance.BuffDef);
                            int buffCountCorrect = Mathf.CeilToInt(cpt.CurrentPosition);
                            if (buffCountCorrect > buffCountCurrent)
                            {
                                for (int i = buffCountCorrect - buffCountCurrent; i > 0; i--)
                                    self.AddBuff(BattleSurge.instance.BuffDef);
                                //self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, buffCountCorrect);
                            }
                        }
                        else if ((PunishType == PunishTypes.Reset && DownPunish) || (PunishType != PunishTypes.Reset && JustDownPunish))
                        {
                            switch (PunishType)
                            {
                                case PunishTypes.SubtractTime:

                                    cpt.Decrease(PunishSubractSeconds);
                                    self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, Mathf.CeilToInt(cpt.CurrentPosition));
                                    break;
                                case PunishTypes.Divide:
                                    cpt.CurrentPosition /= PunishDivisorValue;
                                    self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, Mathf.CeilToInt(cpt.CurrentPosition));
                                    break;
                                case PunishTypes.IncreaseLossRate:
                                    cpt.UsePunishDecreaseVelocity = true;
                                    cpt.DecreaseWithCustomVelocity(Time.fixedDeltaTime, PunishLossRateMultiplier);
                                    int buffCountCurrent = self.GetBuffCount(BattleSurge.instance.BuffDef);
                                    int buffCountCorrect = Mathf.CeilToInt(cpt.CurrentPosition);
                                    if (buffCountCorrect < buffCountCurrent)
                                    {
                                        self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, buffCountCorrect);
                                    }
                                    break;
                                case PunishTypes.Reset:
                                default:
                                    cpt.Reset();
                                    self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, 0);
                                    break;
                            }

                        }
                        else
                        {
                            if (cpt.UsePunishDecreaseVelocity)
                                cpt.DecreaseWithCustomVelocity(Time.fixedDeltaTime, PunishLossRateMultiplier);
                            else
                                cpt.Decrease(Time.fixedDeltaTime);
                            int buffCountCurrent = self.GetBuffCount(BattleSurge.instance.BuffDef);
                            int buffCountCorrect = Mathf.CeilToInt(cpt.CurrentPosition);
                            if (buffCountCorrect < buffCountCurrent)
                            {
                                self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, buffCountCorrect);
                            }
                        }
                    }
                    else
                    {
                        if ((PunishType == PunishTypes.Reset && DownPunish) || (PunishType != PunishTypes.Reset && JustDownPunish))
                        {
                            switch (PunishType)
                            {
                                case PunishTypes.SubtractTime:
                                    cpt.Decrease(PunishSubractSeconds);
                                    self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, Mathf.CeilToInt(cpt.CurrentPosition));
                                    break;
                                case PunishTypes.Divide:
                                    cpt.CurrentPosition /= PunishDivisorValue;
                                    self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, Mathf.CeilToInt(cpt.CurrentPosition));
                                    break;
                                case PunishTypes.IncreaseLossRate:
                                    cpt.UsePunishDecreaseVelocity = true;
                                    cpt.DecreaseWithCustomVelocity(Time.fixedDeltaTime, PunishLossRateMultiplier);
                                    int buffCountCurrent = self.GetBuffCount(BattleSurge.instance.BuffDef);
                                    int buffCountCorrect = Mathf.CeilToInt(cpt.CurrentPosition);
                                    if (buffCountCorrect < buffCountCurrent)
                                    {
                                        self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, buffCountCorrect);
                                    }
                                    break;
                                case PunishTypes.Reset:
                                default:
                                    cpt.Reset();
                                    self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, 0);
                                    break;
                            }

                        }
                        else if (self.inputBank.skill1.down)
                        {
                            cpt.UsePunishDecreaseVelocity = false;
                            cpt.Increase(Time.fixedDeltaTime);
                            int buffCountCurrent = self.GetBuffCount(BattleSurge.instance.BuffDef);
                            int buffCountCorrect = Mathf.CeilToInt(cpt.CurrentPosition);
                            if (buffCountCorrect > buffCountCurrent)
                            {
                                for (int i = buffCountCorrect - buffCountCurrent; i > 0; i--)
                                    self.AddBuff(BattleSurge.instance.BuffDef);
                                //self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, buffCountCorrect);
                            }
                        }
                        else
                        {
                            if (cpt.UsePunishDecreaseVelocity)
                                cpt.DecreaseWithCustomVelocity(Time.fixedDeltaTime, PunishLossRateMultiplier);
                            else
                                cpt.Decrease(Time.fixedDeltaTime);
                            int buffCountCurrent = self.GetBuffCount(BattleSurge.instance.BuffDef);
                            int buffCountCorrect = Mathf.CeilToInt(cpt.CurrentPosition);
                            if (buffCountCorrect < buffCountCurrent)
                            {
                                self.SetBuffCount(BattleSurge.instance.BuffDef.buffIndex, buffCountCorrect);
                            }
                        }
                    }
                }
            }
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if(self && self.inventory)
            {
                var cpt = self.master.GetComponent<BattleArmorTracker>();

                if (GetCount(self) > 0)
                {
                    if (!cpt) cpt = self.master.gameObject.AddComponent<BattleArmorTracker>();
                }
                else if (cpt)
                    UnityEngine.Object.Destroy(cpt);
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            
        }
    }

    public class BattleSurge : BuffBase<BattleSurge>
    {
        public override string BuffName => "Platinum Surge";

        public override Color Color => new Color32(250, 250, 250, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/PlatinumFists/PlatFistBuff.png");
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
            int count = sender.GetBuffCount(BattleSurge.instance.BuffDef);
            if (count>0)
            {
                float armorPerStack = BattleArmor.ArmorIncrease * BattleArmor.MaxSeconds/BattleArmor.MaxStacks;
                float attackSpeedPerStack = BattleArmor.AttackSpeedIncrease * BattleArmor.MaxSeconds / BattleArmor.MaxStacks;
                args.attackSpeedMultAdd += .01f* attackSpeedPerStack * count;
                args.armorAdd += armorPerStack* count;
            }
        }
    }

    public class BattleArmorTracker : MonoBehaviour
    {
        public float SecondsSinceFirstTrigger;
        public float IncreaseVelocity=BattleArmor.MaxStacks/BattleArmor.MaxSeconds;
        public float DecreaseVelocity =-BattleArmor.DecelerationRate*BattleArmor.MaxStacks / BattleArmor.MaxSeconds;
        public float CurrentPosition;
        public bool UsePunishDecreaseVelocity;

        public void Init()
        {
            CurrentPosition = 0;
            UsePunishDecreaseVelocity = false;
        }

        public void Increase(float dt)
        {
            CurrentPosition = Mathf.Min(CurrentPosition+ dt * IncreaseVelocity,BattleArmor.MaxStacks);
        }

        public void Decrease(float dt)
        {
            CurrentPosition = Mathf.Max(CurrentPosition+ dt * DecreaseVelocity,0f);
        }

        public void DecreaseWithCustomVelocity(float dt, float multiplier)
        {
            CurrentPosition = Mathf.Max(CurrentPosition + dt * IncreaseVelocity*(-multiplier), 0f);
        }

        public void Reset()
        {
            CurrentPosition = 0;
        }

    }
}
