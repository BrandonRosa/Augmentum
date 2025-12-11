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
using System.Linq;
using UnityEngine.Networking;

namespace Augmentum.Modules.Pickups.Items.HighlanderItems
{
    class BlazeOfGlory : ItemBase<BlazeOfGlory>
    {
        public override string ItemName => "Blaze of Glory";
        public override string ItemLangTokenName => "BLAZE_OF_GLORY";
        public override string ItemPickupDesc => $"After a boss loses {HealthPercent * 100}% health, summon {SpawnCount} elite Jellyfish. When your allies die, send them to the afterlife in a blaze of glory dealing massive explosive damage to enemies.";
        public override string ItemFullDescription => $"After a boss <style=cIsHealth>loses {HealthPercent * 100}% health</style>, summon {SpawnCount} elite <style=cIsDamage>Jellyfish</style>. When allies <style=cDeath>die</style>, send them to the afterlife in a <style=cIsDamage>blaze of glory</style> dealing <style=cIsDamage>{DamageDealt}% explosive damage</style> to enemies.";

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/BlazeOfGlory/BlazeOfGloryModel.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/BlazeOfGlory/GlazeOfGloryIcon.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => (!AllowItemInheritance)?(new ItemTag[] { ItemTag.Damage, ItemTag.CannotCopy }): (new ItemTag[] { ItemTag.Damage});

        public static float DamageDealt => DamageDealtEntry.Value;
        public static ConfigEntry<float> DamageDealtEntry;
        public static float Radius => RadiusEntry.Value;
        public static ConfigEntry<float> RadiusEntry;
        public static float HealthPercent => HealthPercentEntry.Value;
        public static ConfigEntry<float> HealthPercentEntry;
        public static float SpawnCount => SpawnCountEntry.Value;
        public static ConfigEntry<float> SpawnCountEntry;
        public static float JellyfishDamageReductionMult => JellyfishDamageReductionMultEntry.Value;
        public static ConfigEntry<float> JellyfishDamageReductionMultEntry;
        public static bool AllowItemInheritance => AllowItemInheritanceEntry.Value;
        public static ConfigEntry<bool> AllowItemInheritanceEntry;

        public static Dictionary<TeamIndex, HashSet<CharacterMaster>> TeamsWithItem= new Dictionary<TeamIndex, HashSet<CharacterMaster>>();

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
            AllowItemInheritanceEntry = ConfigManager.ConfigOption<bool>("Item: " + ItemName, "Allow Item Inheritance", false, "Allows item to be inherited by allies with the capacity to copy (Aka engie turrets)");
            DamageDealtEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Damage Dealt In Explosion", 2500f, "How much damage will the explosion deal? (1200f=1200%)");
            HealthPercentEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Health Percent To Trigger Effects", .2f, "What percent of health must be lost for this effect to trigger?");
            SpawnCountEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Summon Jellyfish Count", 1f, "How many fellyfish should this item summon?");
            RadiusEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Explosion Radius", 50f, "What is the radius for this effect?");
            JellyfishDamageReductionMultEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Damage Mult On Jellyfish", 1.2f, "What should the damage multiplier be on the summoned Jellyfish");
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
            On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int += Inventory_RemoveItem_ItemIndex_int;
            On.RoR2.HealthComponent.Suicide += HealthComponent_Suicide;
            On.RoR2.Run.OnClientGameOver += Run_OnClientGameOver;
        }

        private void Run_OnClientGameOver(On.RoR2.Run.orig_OnClientGameOver orig, Run self, RunReport runReport)
        {
            orig(self, runReport);
            TeamsWithItem = new Dictionary<TeamIndex, HashSet<CharacterMaster>>();
        }

        private void HealthComponent_Suicide(On.RoR2.HealthComponent.orig_Suicide orig, HealthComponent self, GameObject killerOverride, GameObject inflictorOverride, DamageTypeCombo damageType)
        {

            if (self && killerOverride == null && inflictorOverride == null)
            {
                if (self.regenAccumulator <= 0f && self.health <= 0)
                {
                    if (self.body && self.body.master && TeamsWithItem.ContainsKey(self.body.master.teamIndex) && TeamsWithItem[self.body.master.teamIndex].Count > 0)
                    {
                        if (self.body.baseNameToken == "AFFIXEARTH_HEALER_BODY_NAME")
                            return;
                        int count = TeamsWithItem[self.body.master.teamIndex].Count;
                        if (count > 0)
                        {
                            NovaOnCommand(self.body, DamageDealt / 100f, Radius);
                        }

                        if (GetCount(self.body) > 0 && self.body.master.teamIndex != TeamIndex.Player)
                        {
                            TeamsWithItem[self.body.master.teamIndex].Remove(self.body.master);
                        }
                    }
                }
            }
            orig(self, killerOverride, inflictorOverride, damageType);
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);
            if (self)
            {
                if (itemIndex == instance.ItemDef.itemIndex)
                {
                    CharacterMaster master = self.GetComponent<CharacterMaster>();
                    TeamIndex TI = master.teamIndex;
                    if (TeamsWithItem.ContainsKey(TI))
                    {
                        if(self.GetItemCount(instance.ItemDef) == 0)
                            TeamsWithItem[TI].Remove(master);
                    }
                    else
                    {
                        TeamsWithItem[TI] = new HashSet<CharacterMaster>();
                    }
                }
            }
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);
            if(self)
            {
                if(itemIndex==instance.ItemDef.itemIndex)
                {
                    CharacterMaster master = self.GetComponent<CharacterMaster>();
                    TeamIndex TI = master.teamIndex;
                    if (TeamsWithItem.ContainsKey(TI))
                    {
                        TeamsWithItem[TI].Add(master);
                    }
                    else
                    {
                        TeamsWithItem[TI] = new HashSet<CharacterMaster>();
                        TeamsWithItem[TI].Add(master);
                    }
                }
            }    
        }


        private void BossHealthPercentageTrigger(BossGroup bossGroup, DamageInfo damageInfo, int triggerCount)
        {
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
                    MakeJellyfish(PlayersWithItem);
                }
            }
        }

        public int GetBossDamageTriggerCount(float PreviousHealth, float CurrentHealth, float TotalHealth, float HealthTriggerPercentage)
        {
            int ans = 0;
            if (CurrentHealth <= 0)
                return ans;
            for (float i = TotalHealth; i > CurrentHealth; i -= TotalHealth * HealthTriggerPercentage)
            {
                if (i < PreviousHealth)
                    ans++;
            }
            return ans;
        }

        private void MakeJellyfish(List<CharacterBody> attackers)
        {
            
            EliteDef eliteDef = R2API.EliteAPI.VanillaEliteTiers[1].eliteTypes[(RoR2Application.rng.RangeInt(0, R2API.EliteAPI.VanillaEliteTiers[1].eliteTypes.Length))];
            EquipmentIndex victimEquipment =eliteDef.eliteEquipmentDef.equipmentIndex;

            foreach (CharacterBody attacker in attackers)
            {
                bool isOdd = (SpawnCount % 2 == 1);
                float dist = 1f;
                float offset =  isOdd? dist : dist/2;
                if(isOdd)
                {
                    JellyFish(attacker, victimEquipment, Vector3.zero);
                }
                for (int i = 1; i < SpawnCount; i+=2)
                {
                    Vector3 rowdisplacement1 = (Quaternion.AngleAxis(90, Vector3.up) * attacker.aimOrigin.normalized)*(offset+dist*i);
                    Vector3 rowdisplacement2 = (Quaternion.AngleAxis(-90, Vector3.up) * attacker.aimOrigin.normalized) * (offset + dist * i);
                    JellyFish(attacker, victimEquipment, rowdisplacement1);
                    JellyFish(attacker, victimEquipment, rowdisplacement2);
                }
            }
        }

        private void JellyFish(CharacterBody attacker, EquipmentIndex victimEquipment, Vector3 rowdisplacement)
        {
            GameObject JellyPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/JellyfishMaster");
            var jellySummon = new MasterSummon();
            jellySummon.position = attacker.corePosition + (Vector3.up * 3) + rowdisplacement;
            jellySummon.masterPrefab = JellyPrefab;
            jellySummon.summonerBodyObject = attacker.gameObject;
            jellySummon.ignoreTeamMemberLimit = true;
            var jellyMaster = jellySummon.Perform();
            if (jellyMaster)
            {
                //jellyMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = baseLifeTime + (stackLifeTime * (stack - 1));

                //CharacterBody jellyBody = jellyMaster.GetBody();
                //jellyBody.baseDamage *= (baseDamage * stack);

                Inventory jellyInventory = jellyMaster.inventory;

                jellyInventory.SetEquipmentIndex(victimEquipment);
                jellyMaster.GetBody().baseDamage *= JellyfishDamageReductionMult;
                jellyMaster.GetBody().PerformAutoCalculateLevelStats();
                //Util.PlaySound("DroidHead", body.gameObject);
            }
        }

        public void NovaOnCommand(CharacterBody body,float damageMultiplier,float radius)
        {

            Vector3 position = body.transform.position;
            Util.PlaySound(EntityStates.VagrantMonster.FireMegaNova.novaSoundString, body.gameObject);
            if (EntityStates.VagrantMonster.FireMegaNova.novaEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(EntityStates.VagrantMonster.FireMegaNova.novaEffectPrefab, body.gameObject, "NovaCenter", transmit: false);
            }
            Transform modelTransform = body.transform;

            if (NetworkServer.active)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = body.gameObject;
                blastAttack.baseDamage = body.damage * damageMultiplier;
                blastAttack.baseForce = EntityStates.VagrantMonster.FireMegaNova.novaForce;
                blastAttack.bonusForce = Vector3.zero;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.crit = body.RollCrit();
                blastAttack.damageColorIndex = DamageColorIndex.Default;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.inflictor = body.gameObject;
                blastAttack.position = position;
                blastAttack.procChainMask = default(ProcChainMask);
                blastAttack.procCoefficient = 3f;
                blastAttack.radius = radius;
                blastAttack.losType = BlastAttack.LoSType.NearestHit;
                blastAttack.teamIndex = body.teamComponent.teamIndex;
                blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(EntityStates.VagrantMonster.FireMegaNova.novaEffectPrefab);
                blastAttack.Fire();

            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (self && damageReport.victimMaster && TeamsWithItem.ContainsKey(damageReport.victimMaster.teamIndex) && TeamsWithItem[damageReport.victimMaster.teamIndex].Count>0)
            {
                if (damageReport.victimBody.baseNameToken == "AFFIXEARTH_HEALER_BODY_NAME")
                    return;
                int count = TeamsWithItem[damageReport.victimMaster.teamIndex].Count;
                if (count > 0)
                {
                    NovaOnCommand(damageReport.victimBody, DamageDealt/100f, Radius);
                }

                if(GetCount(damageReport.victimBody)>0 && damageReport.victimMaster.teamIndex!=TeamIndex.Player)
                {
                    TeamsWithItem[damageReport.victimMaster.teamIndex].Remove(damageReport.victimMaster);
                }
            }
        }


    }


}
