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
using static RoR2.ItemTag;
using BransItems.Modules.ItemTiers.HighlanderTier;
using BransItems.Modules.Utils;
using BransItems.Modules.StandaloneBuffs;
using System.Linq;

namespace BransItems.Modules.Pickups.Items.HighlanderItems
{
    class SecondDeath : ItemBase<SecondDeath>
    {
        public override string ItemName => "Second Death";
        public override string ItemLangTokenName => "SECOND_DEATH";
        public override string ItemPickupDesc => $"On-Kill effects trigger twice. Bosses trigger On-Kill effects every {HealthPercent*100}% of health lost.";
        public override string ItemFullDescription => $"<style=cIsDamage>On-Kill</style> effects <style=cIsDamage>trigger twice</style>. Bosses <style=cIsDamage>trigger On-Kill</style> effects every <style=cIsHealth>{HealthPercent * 100}%</style> of <style=cIsHealth>health lost</style>.";

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/SecondDeath/SecondDeathModel.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/SecondDeath/SecondDeath.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public static float HealthPercent;
        public static bool DisableSpleenOnSecondTrigger;

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
            HealthPercent = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Health Percent To Trigger Effects", .25f, "What percent of health must be lost for this effect to trigger?");
            DisableSpleenOnSecondTrigger = ConfigManager.ConfigOption<bool>("Item: " + ItemName, "Disable shatterspleen interaction", true, "Setting this to true will disable this item's additional interactions with shatterspleen.");
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
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            //On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            ItemHelpers.ToggleBossGroupDamageHooks(true);
            BossGroupDamageTakenTracker.AddToPercentLostActionList(HealthPercent, BossHealthPercentageTrigger);
        }

        private void BossHealthPercentageTrigger(BossGroup bossGroup, DamageInfo damageInfo, int triggerCount)
        {
            if (!bossGroup || !bossGroup.combatSquad || bossGroup.combatSquad.membersList==null || bossGroup.combatSquad.membersList.Count<=0)
                return;
            List<CharacterBody> PlayersWithItem = CharacterMaster.instancesList
                    .Select(master => master.GetBody())
                    .Where(body => body && body.teamComponent.teamIndex==TeamIndex.Player && GetCount(body) > 0)
                    .ToList();
            if (PlayersWithItem == null)
                return;
            int count = PlayersWithItem.Sum(body => GetCount(body));
            if (count > 0)
            {
                for (int i = 0; i < triggerCount; i++)
                {
                    List<CharacterMaster> masterList = new List<CharacterMaster>(bossGroup.combatSquad.membersList);
                    foreach(CharacterMaster master in masterList)
                        if(master.GetBody() && master.GetBody().healthComponent && master.GetBody().healthComponent.alive && master.GetBody().healthComponent.combinedHealth>0)
                            MakeFakeDeath(master.GetBody().healthComponent, damageInfo, PlayersWithItem);
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if(self && damageInfo.attacker && self.body.isBoss)
            {
                List<CharacterBody> PlayersWithItem = CharacterMaster.instancesList
                    .Select(master => master.GetBody())
                    .Where(body => GetCount(body)>0 && body.isPlayerControlled)
                    .ToList();
                int count = PlayersWithItem.Sum(body => GetCount(body));
                if(count>0)
                {
                    int triggerCount = GetBossDamageTriggerCount(self.combinedHealth+damageInfo.damage,self.combinedHealth,self.fullCombinedHealth,HealthPercent);
                    for(int i=0;i<triggerCount;i++)
                    {
                        MakeFakeDeath(self,damageInfo,PlayersWithItem);
                    }
                }
            }
        }

        public int GetBossDamageTriggerCount(float PreviousHealth, float CurrentHealth, float TotalHealth, float HealthTriggerPercentage)
        {
            int ans = 0;
            if (CurrentHealth <= 0)
                return ans;
            for(float i=TotalHealth;i>CurrentHealth;i-=TotalHealth*HealthTriggerPercentage)
            {
                if (i < PreviousHealth)
                    ans++;
            }
            return ans;
        }

        private void MakeFakeDeath(HealthComponent self, DamageInfo damageInfo, List<CharacterBody> attackers)
        {
            foreach (CharacterBody attacker in attackers)
            {
                DamageInfo damageInfoFake = new DamageInfo
                {
                    attacker = attacker?.gameObject,
                    crit = false,
                    damage = damageInfo.damage,
                    position = damageInfo.position,
                    procCoefficient = damageInfo.procCoefficient,
                    damageType = damageInfo.damageType,
                    damageColorIndex = damageInfo.damageColorIndex
                };
                HealthComponent victim = self;//new HealthComponent();
                DamageReport damageReport = new DamageReport(damageInfoFake, victim, damageInfo.damage, self.combinedHealth);
                int SpleenCount = 0;
                if (DisableSpleenOnSecondTrigger && attacker?.inventory)
                {
                    SpleenCount = attacker.inventory.itemStacks[(int)RoR2Content.Items.BleedOnHitAndExplode.itemIndex];
                    attacker.inventory.itemStacks[(int)RoR2Content.Items.BleedOnHitAndExplode.itemIndex] = 0;
                }
                GlobalEventManager.instance.OnCharacterDeath(damageReport);
                if(SpleenCount>0)
                {
                    attacker.inventory.itemStacks[(int)RoR2Content.Items.BleedOnHitAndExplode.itemIndex] = SpleenCount;
                }
            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if(self && damageReport.attackerBody)
            {
                if(GetCount(damageReport.attackerBody)>0)
                {
                    /*
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = damageReport.attacker,
                        crit = damageReport.damageInfo.crit,
                        damage = damageReport.damageInfo.damage,
                        position = damageReport.damageInfo.position,
                        procCoefficient = damageReport.damageInfo.procCoefficient,
                        damageType = damageReport.damageInfo.damageType,
                        damageColorIndex = damageReport.damageInfo.damageColorIndex
                    };
                    HealthComponent victim = healthComponent;
                    DamageReport DR = new DamageReport(damageInfo, victim, damageInfo.damage, healthComponent.combinedHealth);
                    GlobalEventManager.instance.OnCharacterDeath(DR);
                    */
                    /*
                    foreach (EliteDef ED in EliteCatalog.eliteDefs)
                    {
                        damageReport.victimBody.SetBuffCount(ED.eliteEquipmentDef.passiveBuffDef.buffIndex,0);
                    }
                    */

                    int SpleenCount = 0;
                    if (DisableSpleenOnSecondTrigger && damageReport.attackerBody.inventory)
                    {
                        SpleenCount = damageReport.attackerBody.inventory.itemStacks[(int)RoR2Content.Items.BleedOnHitAndExplode.itemIndex];
                        damageReport.attackerBody.inventory.itemStacks[(int)RoR2Content.Items.BleedOnHitAndExplode.itemIndex] = 0;
                    }
                    orig(self, damageReport);
                    if (SpleenCount > 0)
                    {
                        damageReport.attackerBody.inventory.itemStacks[(int)RoR2Content.Items.BleedOnHitAndExplode.itemIndex] = SpleenCount;
                    }

                }
            }
        }

        
    }

  
}
