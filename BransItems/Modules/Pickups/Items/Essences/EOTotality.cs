using BepInEx.Configuration;
using BransItems.Modules.Utils;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;

namespace BransItems.Modules.Pickups.Items.Essences
{
    class EOTotality : ItemBase<EOTotality>
    {
        public override string ItemName => "Essence of Totality";
        public override string ItemLangTokenName => "ESSENCE_OF_TOTALITY";
        public override string ItemPickupDesc => "Slightly increase all stats.";
        public override string ItemFullDescription => $"Gain: \n<style=cIsDamage>{AttackSpeedGain}%</style><style=cStack> (+{AttackSpeedGain}% per stack)</style> <style=cIsDamage>attack speed</style>, \n<style=cIsUtility>{MoveSpeedGain}%</style><style=cStack> (+{MoveSpeedGain}% per stack)</style> <style=cIsUtility>movement speed</style>, \n<style=cIsDamage>{CritChanceGain}%</style><style=cStack> (+{CritChanceGain}% per stack)</style> <style=cIsDamage>Critical Strike</style> chance, " +
            $"\n<style=cIsDamage>{DamageGain*100}%</style><style=cStack> (+{DamageGain*100}% per stack)</style> <style=cIsDamage>damage</style>, \n<style=cIsHealing>{HealthGain*100}% </style><style=cStack> (+{HealthGain*100}% per stack)</style> <style=cIsHealing>health</style>, \nand <style=cIsHealing>{ArmorGain } </style><style=cStack> (+{ArmorGain } per stack)</style> <style=cIsHealing>armor</style>.";

        public override string ItemLore => $"Excerpt from the folk tale \"The Radiant Luminance:\"\n\n" +
            $"\"In the twilight of ancient realms, a tale unfolds of the rarest gem, a stone coveted by noble souls of valor. Legends speak of the Radiant Luminance, an ethereal jewel that bestowed boundless strength upon those deemed worthy. " +
            $"Whispers carried through the annals of time tell of heroes who, in their noble deeds, unwittingly birthed this precious gemstone from the very essence of their valorous souls.\n" +
            $"As the story goes, these radiant stones shimmered with a transcendent brilliance, hidden within the deepest recesses of the heroic heart.Only those who carried the weight of righteous purpose could uncover this extraordinary relic, glistening with the combined virtues of strength, speed, and resilience." +
            $" It is said that the Radiant Luminance, a manifestation of the noble and the just, would reveal itself only to those whose souls burned with an unwavering commitment to the greater good.A testament to the valor of heroes, this luminous gem became a beacon of hope and an eternal echo of their righteous deeds, whispered through the pages of time.\"";

        public override ItemTierDef ModdedTierDef => EssenceHelpers.essenceTierDef; ////ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        public override GameObject ItemModel => SetModel();
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Temporary/crystal5/source/TotalityIcon2.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => EssenceHelpers.canRemoveEssence;

        public override ItemTag[] ItemTags => EssenceHelpers.essenceItemTags;

        public static float ReplaceChance;

        public static float MoveSpeedGain;
        public static float AttackSpeedGain;
        public static float CritChanceGain;
        public static float HealthGain;
        public static float DamageGain;
        public static float ArmorGain;


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
            ReplaceChance = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Chance to replace other Essences", .05f, "What percent of the time will Essence of Totality replace other Essences?");
            MoveSpeedGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Move Speed given to character", 6, "How much movement speed should Essence of Totality grant?");
            AttackSpeedGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Attack Speed given to character", 6, "How much attack speed should Essence of Totality grant?");
            CritChanceGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Crit Chance given to character", 3, "How much crit chance should Essence of Totality grant?");
            //HealthGain = config.Bind<float>("Item: " + ItemName, "Health given to character", 15, "How much health should Essence of Totality grant?").Value;
            //DamageGain = config.Bind<float>("Item: " + ItemName, "Damage given to character", 1, "How much damage should Essence of Totality grant?").Value;

            HealthGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Health percent given to character", .04f, "How much health percent should Essence of Totality grant?");
            ArmorGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Armor given to character", 4f, "How much armor should Essence of Totality grant?");
            DamageGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Damage percent given to character", .04f, "How much damage should percent Essence of Totality grant?");
        }
        private GameObject SetModel()//RoR2/Base/ShinyPearl/matShinyPearl.mat
        {
            GameObject temp = MainAssets.LoadAsset<GameObject>("Assets/Textrures/Icons/Temporary/crystal5/source/crystal5.prefab");//("Assets/Models/Prefavs/Item/Essence_of_Strength/EssenceOfStrength.prefab");
            Material pearlMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/ShinyPearl/matShinyPearl.mat").WaitForCompletion();

            temp.GetComponentInChildren<MeshRenderer>().SetMaterial(pearlMat);

            return temp;
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
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.moveSpeedMultAdd += MoveSpeedGain * .01f * GetCount(sender);
            args.attackSpeedMultAdd += AttackSpeedGain * .01f * GetCount(sender);
            args.critAdd += CritChanceGain * GetCount(sender);
            //args.baseDamageAdd += DamageGain * GetCount(sender);
            //args.baseHealthAdd += HealthGain * GetCount(sender);
            args.damageMultAdd += DamageGain * GetCount(sender);
            args.healthMultAdd += HealthGain * GetCount(sender);
            args.armorAdd += ArmorGain * GetCount(sender);
        }
    }
}
