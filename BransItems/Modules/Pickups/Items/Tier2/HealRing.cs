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
using static Augmentum.Modules.Pickups.Items.Essences.EssenceHelpers;
using UnityEngine.Networking;
using Augmentum.Modules.Pickups.Items.Essences;
using Augmentum.Modules.Pickups.Items.NoTier;
using Augmentum.Modules.Utils;
using UnityEngine.AddressableAssets;
using Augmentum.Modules.Pickups.Items.CoreItems;
using Augmentum.Modules.Pickups.Items.Tier1;
using Augmentum.Modules.Pickups.Items.Tier3;
using Augmentum.Modules.StandaloneBuffs;
using Augmentum.Modules.Pickups.Items.HighlanderItems;

namespace Augmentum.Modules.Pickups.Items.Tier2
{
    class HealRing : ItemBase<HealRing>
    {
        public override string ItemName => "Halda's Band";
        public override string ItemLangTokenName => "HEALING_BAND";
        public override string ItemPickupDesc => "High damage hits also heal you. Recharges over time.";
        public override string ItemFullDescription => $"Hits that deal <style=cIsDamage>more than 400% damage</style> also <style=cIsHealing>heal</style> you for <style=cIsHealing>{InitialPercent*100}%</style> <style=cStack>(+{AdditionalPercent*100}% per stack)</style> of the TOTAL damage" +
            $" up to <style=cIsHealing>{InitialMaxHealing*100}%</style> <style=cStack>(+{AdditionalMaxHealing*100}% per stack)</style> max health. Recharges every <style=cIsUtility>{HealRing.HealingRingsCooldownTime}</style> seconds.";

        public override string ItemLore => $"Should sickness overtake you,\nShould death draw near,\nI will heal you.\nI will be there.";

        public override ItemTier Tier => ItemTier.Tier2;

        public override GameObject ItemModel => EditItemModel(Color.green);
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Item/HealingBands/HealingBand.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override ItemTag[] ItemTags => new ItemTag[] {ItemTag.Healing };

        public static float InitialPercent;
        public static float AdditionalPercent;
        public static float InitialMaxHealing;
        public static float AdditionalMaxHealing;
        public static float HealingRingsCooldownTime = 10f;

        public static GameObject potentialPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();

        public override void Init(ConfigFile config)
        {
            EditItemModel(Color.green);
            CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();
        }

        public GameObject EditItemModel(Color color)
        {
            GameObject TempItemModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElementalRings/PickupIceRing.prefab").WaitForCompletion().InstantiateClone("BarrierRing", false);
            Material mat = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ElementalRings/matIceRing.mat").WaitForCompletion());
            Material mat2 = UnityEngine.Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ElementalRings/matIceRingGemstone.mat").WaitForCompletion());
            Color emissionColor = new Color32(21, 70, 21, 255);

            //All this just incase of of these changes the color.
            mat.SetColor("_MainColor", color);
            mat.SetColor("_Color", color);
            mat.SetColor("_TintColor", color);
            mat.SetColor("Emission Color", emissionColor);
            mat.color = color;
            mat2.SetColor("_MainColor", color);
            mat2.SetColor("_Color", color);
            mat2.SetColor("_TintColor", color);
            mat.SetColor("Emission Color", emissionColor);
            mat2.color = color;


            TempItemModel.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().SetMaterial(mat);
            TempItemModel.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().SetMaterial(mat2);

            return TempItemModel;
        }

        public void CreateConfig(ConfigFile config)
        {
            //string ConfigItemName = ItemName.Replace("\'", "");
            InitialPercent = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Percent of total damage heal", .20f, "What percent of total damage should be healed from the first stack of this item?");
            AdditionalPercent = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Percent of additional damage heal", .05f, "What percent of total damage should be healed from additional stacks of this item?");
            InitialMaxHealing = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Max percent of max health you can heal", .15f, "What is the maximum percent of your health you can heal from this item from the first stack?");
            AdditionalMaxHealing = ConfigManager.ConfigOption<float>("Item: " + ConfigItemName, "Additional percent of max health you can heal", .15f, "What is the maximum percent of your health you can heal from this item from additional stacks?");

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
            //On.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
            //On.RoR2.CharacterMaster.SpawnBodyHere += CharacterMaster_SpawnBodyHere;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            //On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if(self && self.inventory && self.inventory.GetItemCount(ItemDef)>0)
            {
                if(!self.HasBuff(RoR2Content.Buffs.ElementalRingsReady))
                {
                    self.AddBuff(RoR2Content.Buffs.ElementalRingsReady);
                }
            }
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
                            for (int i = 0; i <= component2.inventory.GetItemCount(DoubleBand.instance.ItemDef); i++)
                                component2.healthComponent.Heal(Math.Min(damageInfo.damage * (InitialPercent + AdditionalPercent * ((float)itemCount - 1f)), component2.healthComponent.fullHealth * (InitialMaxHealing + AdditionalMaxHealing * ((float)itemCount - 1f))), damageInfo.procChainMask);
                        }
                    }
                }
            }
            orig(self, damageInfo, victim);

            if(safe && component2 != null && triggered && component2.HasBuff(HealingRingsReady.instance.BuffDef))
            {
                component2.RemoveBuff(HealingRingsReady.instance.BuffDef);
                float cooldown = HealRing.HealingRingsCooldownTime;
                if (component2.inventory.GetItemCount(HighlanderItems.CooldownBand.instance.ItemDef) > 0)
                    cooldown *= HighlanderItems.CooldownBand.CooldownReduction;
                for (int k = 1; (float)k <= cooldown; k++)
                {
                    component2.AddTimedBuff(HealingRingsCooldown.instance.BuffDef, k);
                }
            }
        }
    }

    public class HealingRingsCooldown : BuffBase<HealingRingsCooldown>
    {
        public override string BuffName => "Healing Rings Cooldown";

        public override Color Color => new Color32(255, 255, 255, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Buff/HealingRingsCooldown.png");
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
    public class HealingRingsReady : BuffBase<HealingRingsReady>
    {
        public override string BuffName => "Healing Rings Ready";

        public override Color Color => new Color32(255, 255, 255, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Buff/HealingRingsReady.png");
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
                    if (!self.HasBuff(HealingRingsCooldown.instance.BuffDef)) // && !self.HasBuff(Fortified.instance.BuffDef))
                    {
                        Inventory inv = self.inventory;
                        if (inv && inv.GetItemCount(HealRing.instance.ItemDef)+ inv.GetItemCount(BarrierRing.instance.ItemDef)>0) //cpt = self.gameObject.AddComponent<FortifiedTracker>();
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
