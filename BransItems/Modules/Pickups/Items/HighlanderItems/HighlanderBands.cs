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
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.Projectile;

namespace Augmentum.Modules.Pickups.Items.HighlanderItems
{
    class CooldownBand : ItemBase<CooldownBand>
    {
        public override string ItemName => "Chal’s Band";
        public override string ItemLangTokenName => "COOLDOWN_BAND";
        public override string ItemPickupDesc => "Reduces Band cooldowns. High damage hits deal more damage.";
        public override string ItemFullDescription => $"Reduces Band <style=cIsUtility>cooldowns</style> by <style=cIsUtility>{CooldownReduction * 100f}%</style>. Hits that deal <style=cIsDamage>more than 350% damage</style> deal <style=cIsDamage>50% more</style>.";

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/HighlanderBands/SilverRingModel.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/HighlanderBands/CooldownBand.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public static float CooldownReduction = .5f;

        public static ProcType DamageBoostRing = (ProcType)(341);


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
            string ConfigItemName = ItemName.Replace("\'", "");
            CooldownReduction = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Band cooldown reduction", .5f, "How much cooldown reduction should this item grant?");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            //rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.42142F, -0.10234F),
            //        localAngles = new Vector3(351.1655F, 45.64202F, 351.1029F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlHuntress", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.35414F, -0.14761F),
            //        localAngles = new Vector3(356.5505F, 45.08208F, 356.5588F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlToolbot", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0, 2.46717F, 2.64379F),
            //        localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
            //        localScale = new Vector3(.2F, .2F, .2F)
            //    }
            //});
            //rules.Add("mdlEngi", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "HeadCenter",
            //        localPos = new Vector3(0, 0.24722F, -0.01662F),
            //        localAngles = new Vector3(10.68209F, 46.03322F, 11.01807F),
            //        localScale = new Vector3(0.025F, 0.025F, 0.025F)
            //    }
            //});
            //rules.Add("mdlMage", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.24128F, -0.14951F),
            //        localAngles = new Vector3(6.07507F, 45.37084F, 6.11489F),
            //        localScale = new Vector3(0.017F, 0.017F, 0.017F)
            //    }
            //});
            //rules.Add("mdlMerc", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.31304F, -0.00747F),
            //        localAngles = new Vector3(359.2931F, 45.00048F, 359.2912F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlTreebot", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "FlowerBase",
            //        localPos = new Vector3(0, 1.94424F, -0.47558F),
            //        localAngles = new Vector3(20.16552F, 48.87548F, 21.54582F),
            //        localScale = new Vector3(.15F, .15F, .15F)
            //    }
            //});
            //rules.Add("mdlLoader", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.30118F, -0.0035F),
            //        localAngles = new Vector3(8.31363F, 45.67525F, 8.41428F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlCroco", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, -0.65444F, 1.64345F),
            //        localAngles = new Vector3(326.1803F, 277.2657F, 249.9269F),
            //        localScale = new Vector3(.2F, .2F, .2F)
            //    }
            //});
            //rules.Add("mdlCaptain", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(-0.0068F, 0.3225F, -0.03976F),
            //        localAngles = new Vector3(0F, 45F, 0F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(-0.14076F, 0.15542F, -0.04648F),
            //        localAngles = new Vector3(356.9802F, 81.10978F, 353.687F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Hat",
            //        localPos = new Vector3(0F, 0.01217F, -0.00126F),
            //        localAngles = new Vector3(356.9376F, 25.8988F, 14.69767F),
            //        localScale = new Vector3(0.001F, 0.001F, 0.001F)
            //    }
            //});
            //rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0.00042F, 0.46133F, 0.01385F),
            //        localAngles = new Vector3(355.2848F, 47.55381F, 355.0908F),
            //        localScale = new Vector3(0.020392F, 0.020392F, 0.020392F)
            //    }
            //});
            //rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Chest",
            //        localPos = new Vector3(0.00076F, -0.0281F, 0.09539F),
            //        localAngles = new Vector3(338.9489F, 145.7505F, 217.6883F),
            //        localScale = new Vector3(0.005402F, 0.005402F, 0.005402F)
            //    }
            //});
            //rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, -0.00277F, -0.13259F),
            //        localAngles = new Vector3(322.1495F, 124.8318F, 235.476F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.32104F, 0F),
            //        localAngles = new Vector3(0F, 321.2954F, 0F),
            //        localScale = new Vector3(0.024027F, 0.024027F, 0.024027F)
            //    }
            //});
            //rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "HeadCenter",
            //        localPos = new Vector3(0.00216F, 0.01033F, 0F),
            //        localAngles = new Vector3(0F, 323.6887F, 355.1232F),
            //        localScale = new Vector3(0.000551F, 0.000551F, 0.000551F)
            //    }
            //});
            return rules;
        }


        public override void Hooks()
        {
            //IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_ILOnHitEnemy;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void GlobalEventManager_ILOnHitEnemy(ILContext il)
        {
            ILCursor c = new(il);

            // Find the insertion point based on surrounding context
            if (c.TryGotoNext(MoveType.Before,
                              x => x.MatchCall(typeof(EffectManager), "SimpleImpactEffect"),
                              x => x.MatchLdarg(0),
                              x => x.MatchCallvirt<CharacterBody>("AddTimedBuff"),
                              x => x.MatchLdarg(0),
                              x => x.MatchLdfld<CharacterBody>("healthComponent"),
                              x => x.MatchLdarg(1),
                              x => x.MatchCallvirt<HealthComponent>("TakeDamage")))
            {
                // Move to just before the call to TakeDamage
                c.Index += 6;

                // Insert the initialization of the loop variable i
                c.Emit(OpCodes.Ldc_I4_0); // Push 0 onto the stack
                c.Emit(OpCodes.Stloc_0); // Store 0 in local variable 0 (int i = 0)

                // Define labels for loop start and loop end
                ILLabel loopStart = c.DefineLabel();
                ILLabel loopEnd = c.DefineLabel();

                // Insert the loop start label.
                c.MarkLabel(loopStart);

                // Insert the comparison for the loop condition (i <= component2.inventory.GetItemCount(DoubleRing.instance.ItemDef)).
                c.Emit(OpCodes.Ldloc_0); // Load local variable 0 (i).
                c.Emit(OpCodes.Ldarg_0); // Load argument 0 (this).
                c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_inventory")); // Call get_inventory() on this.
                c.Emit(OpCodes.Ldarg_0); // Load argument 0 (this).
                c.Emit(OpCodes.Callvirt, typeof(DoubleBand).GetProperty("instance").GetGetMethod()); // Get DoubleRing.instance.
                c.Emit(OpCodes.Callvirt, typeof(DoubleBand).GetProperty("ItemDef").GetGetMethod()); // Get instance.ItemDef.
                c.Emit(OpCodes.Callvirt, typeof(Inventory).GetMethod("GetItemCount")); // Call GetItemCount(ItemDef).
                c.Emit(OpCodes.Bgt, loopEnd); // Branch to loopEnd if i > GetItemCount.

                // Insert the line of code to be looped.
                c.Emit(OpCodes.Ldarg_0); // Load this.
                c.Emit(OpCodes.Ldarg_1); // Load damageInfo.
                c.Emit(OpCodes.Ldarg_2); // Load victim.
                c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("TakeDamage")); // Call TakeDamage(damageInfo).

                // Increment the loop variable (i++).
                c.Emit(OpCodes.Ldloc_0); // Load local variable 0 (i).
                c.Emit(OpCodes.Ldc_I4_1); // Push 1 onto the stack.
                c.Emit(OpCodes.Add); // Add 1 to i.
                c.Emit(OpCodes.Stloc_0); // Store the result in local variable 0 (i).

                // Jump back to the loop start.
                c.Emit(OpCodes.Br, loopStart);

                // Insert the loop end label.
                c.MarkLabel(loopEnd);
            }
            else
            {
                // Handle the case where the target instruction is not found.
                Augmentum.ModLogger.LogWarning("Could not find the target instruction.");
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self && damageInfo.attacker)
            {

                if (!damageInfo.procChainMask.HasProc(DamageBoostRing))
                {
                    CharacterBody attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (attacker)
                    {
                        bool HasCooldown = GetCount(attacker) > 0;
                        bool HasDouble = attacker.inventory && attacker.inventory.GetItemCount(DoubleBand.instance.ItemDef) > 0;
                        if (HasCooldown || HasDouble)
                        {
                            float currentDamage = attacker.damage;
                            if (HasCooldown && damageInfo.damage >= currentDamage * 3.5f)
                            {
                                damageInfo.damage += currentDamage * .5f;
                                damageInfo.procChainMask.AddProc(DamageBoostRing);
                            }
                            if (HasDouble && damageInfo.damage >= currentDamage * 4f)
                            {
                                damageInfo.damage += currentDamage * .5f;
                                damageInfo.procChainMask.AddProc(DamageBoostRing);
                            }
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            bool HadReadyBuff = false;
            bool IsVoid = false;
            bool Cooldown = false;
            bool DoubleB = false;
            CharacterBody attackerBody = null;
            if (self && victim)
            {
                Cooldown = ShouldModifyLoopCooldown(damageInfo, victim);
                DoubleB = ShouldDouble(damageInfo, victim);
                if (Cooldown || DoubleB)
                {
                    attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    //float currentDamage = attackerBody.damage;
                    //if (damageInfo.damage >= currentDamage * 3.5f)
                    //damageInfo.damage += currentDamage * .5f;

                    IsVoid = attackerBody.HasBuff(DLC1Content.Buffs.ElementalRingVoidReady);
                    HadReadyBuff = (attackerBody.HasBuff(RoR2Content.Buffs.ElementalRingsReady) || IsVoid);
                }
                else
                    HadReadyBuff = false;
            }
            orig(self, damageInfo, victim);
            if (HadReadyBuff && (!attackerBody.HasBuff(RoR2Content.Buffs.ElementalRingsReady) && !attackerBody.HasBuff(DLC1Content.Buffs.ElementalRingVoidReady)) && attackerBody)
            {
                if (!IsVoid)
                {
                    if (Cooldown)
                    {
                        float value = 10f * CooldownReduction;
                        attackerBody.ClearTimedBuffs(RoR2Content.Buffs.ElementalRingsCooldown);
                        for (int k = 1; (float)k <= value; k++)
                        {
                            attackerBody.AddTimedBuff(RoR2Content.Buffs.ElementalRingsCooldown, k);
                        }
                    }
                    if (DoubleB)
                    {
                        int iceCount = attackerBody.inventory.GetItemCount(RoR2Content.Items.IceRing.itemIndex);
                        int fireCount = attackerBody.inventory.GetItemCount(RoR2Content.Items.FireRing.itemIndex);
                        CharacterBody characterBody = victim.GetComponent<CharacterBody>();

                        ProcChainMask procChainMask5 = damageInfo.procChainMask;
                        procChainMask5.AddProc(ProcType.Rings);
                        if (iceCount>0)
                        {
                            float damageCoefficient8 = 2.5f * (float)iceCount;
                            float damage2 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient8);
                            DamageInfo damageInfo2 = new DamageInfo
                            {
                                damage = damage2,
                                damageColorIndex = DamageColorIndex.Item,
                                damageType = DamageType.Generic,
                                attacker = damageInfo.attacker,
                                crit = damageInfo.crit,
                                force = Vector3.zero,
                                inflictor = null,
                                position = damageInfo.position,
                                procChainMask = procChainMask5,
                                procCoefficient = 1f
                            };
                            EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/IceRingExplosion"), damageInfo.position, Vector3.up, transmit: true);
                            characterBody.AddTimedBuff(RoR2Content.Buffs.Slow80, 3f * (float)iceCount);
                            characterBody.healthComponent.TakeDamage(damageInfo2);
                        }
                        if(fireCount>0)
                        {
                            GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FireTornado");
                            float resetInterval = val.GetComponent<ProjectileOverlapAttack>().resetInterval;
                            float lifetime = val.GetComponent<ProjectileSimple>().lifetime;
                            float damageCoefficient9 = 3f * (float)fireCount;
                            float damage3 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient9) / lifetime * resetInterval;
                            float speedOverride = 0f;
                            Quaternion rotation2 = Quaternion.identity;
                            Vector3 val2 = damageInfo.position - attackerBody.aimOrigin;
                            val2.y = 0f;
                            if (val2 != Vector3.zero)
                            {
                                speedOverride = -1f;
                                rotation2 = Util.QuaternionSafeLookRotation(val2, Vector3.up);
                            }
                            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                            {
                                damage = damage3,
                                crit = damageInfo.crit,
                                damageColorIndex = DamageColorIndex.Item,
                                position = damageInfo.position,
                                procChainMask = procChainMask5,
                                force = 0f,
                                owner = damageInfo.attacker,
                                projectilePrefab = val,
                                rotation = rotation2,
                                speedOverride = speedOverride,
                                target = null
                            });
                        }
                    }

                }
                else
                {
                    if (Cooldown)
                    {
                        float value = 20f * CooldownReduction;
                        attackerBody.ClearTimedBuffs(DLC1Content.Buffs.ElementalRingVoidCooldown);
                        for (int l = 1; (float)l <= value; l++)
                        {
                            attackerBody.AddTimedBuff(DLC1Content.Buffs.ElementalRingVoidCooldown, l);
                        }
                    }
                    if (DoubleB)
                    {
                        int itemCount13 = attackerBody.inventory.GetItemCount(DLC1Content.Items.ElementalRingVoid);
                        ProcChainMask procChainMask5 = damageInfo.procChainMask;
                        procChainMask5.AddProc(ProcType.Rings);
                        if (itemCount13 > 0)
                        {
                            GameObject projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ElementalRingVoidBlackHole");
                            float damageCoefficient10 = 1f * (float)itemCount13;
                            float damage4 = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient10);
                            ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                            {
                                damage = damage4,
                                crit = damageInfo.crit,
                                damageColorIndex = DamageColorIndex.Void,
                                position = damageInfo.position,
                                procChainMask = procChainMask5,
                                force = 6000f,
                                owner = damageInfo.attacker,
                                projectilePrefab = projectilePrefab,
                                rotation = Quaternion.identity,
                                target = null
                            });
                        }
                    }
                }
            }
        }

        public bool ShouldModifyLoopCooldown(DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.attacker || !victim)
                return false;
            int count = GetCount(damageInfo.attacker.GetComponent<CharacterBody>());
            if (count > 0)
                return true;
            return false;
        }

        public bool ShouldDouble(DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.attacker || !victim)
                return false;
            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!body || !body.inventory)
                return false;
            int count = body.inventory.GetItemCount(DoubleBand.instance.ItemDef);
            if (count > 0)
                return true;
            return false;
        }

    }

    class DoubleBand : ItemBase<DoubleBand>
    {
        public override string ItemName => "Tlaloc’s Band";
        public override string ItemLangTokenName => "DOUBLE_BAND";
        public override string ItemPickupDesc => "Band effects trigger twice. High damage hits deal more damage.";
        public override string ItemFullDescription => $"Band <style=cIsUtility>effects trigger twice</style>. Hits that deal <style=cIsDamage>more than 400% damage</style> deal <style=cIsDamage>50% more</style>.";

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/HighlanderBands/GoldRingModel.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/HighlanderBands/DoubleBand.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

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
            string ConfigItemName = ItemName.Replace("\'", "");

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            //rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.42142F, -0.10234F),
            //        localAngles = new Vector3(351.1655F, 45.64202F, 351.1029F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlHuntress", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.35414F, -0.14761F),
            //        localAngles = new Vector3(356.5505F, 45.08208F, 356.5588F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlToolbot", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0, 2.46717F, 2.64379F),
            //        localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
            //        localScale = new Vector3(.2F, .2F, .2F)
            //    }
            //});
            //rules.Add("mdlEngi", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "HeadCenter",
            //        localPos = new Vector3(0, 0.24722F, -0.01662F),
            //        localAngles = new Vector3(10.68209F, 46.03322F, 11.01807F),
            //        localScale = new Vector3(0.025F, 0.025F, 0.025F)
            //    }
            //});
            //rules.Add("mdlMage", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.24128F, -0.14951F),
            //        localAngles = new Vector3(6.07507F, 45.37084F, 6.11489F),
            //        localScale = new Vector3(0.017F, 0.017F, 0.017F)
            //    }
            //});
            //rules.Add("mdlMerc", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.31304F, -0.00747F),
            //        localAngles = new Vector3(359.2931F, 45.00048F, 359.2912F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlTreebot", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "FlowerBase",
            //        localPos = new Vector3(0, 1.94424F, -0.47558F),
            //        localAngles = new Vector3(20.16552F, 48.87548F, 21.54582F),
            //        localScale = new Vector3(.15F, .15F, .15F)
            //    }
            //});
            //rules.Add("mdlLoader", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.30118F, -0.0035F),
            //        localAngles = new Vector3(8.31363F, 45.67525F, 8.41428F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlCroco", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, -0.65444F, 1.64345F),
            //        localAngles = new Vector3(326.1803F, 277.2657F, 249.9269F),
            //        localScale = new Vector3(.2F, .2F, .2F)
            //    }
            //});
            //rules.Add("mdlCaptain", new ItemDisplayRule[]
            //{
            //    new ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(-0.0068F, 0.3225F, -0.03976F),
            //        localAngles = new Vector3(0F, 45F, 0F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(-0.14076F, 0.15542F, -0.04648F),
            //        localAngles = new Vector3(356.9802F, 81.10978F, 353.687F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Hat",
            //        localPos = new Vector3(0F, 0.01217F, -0.00126F),
            //        localAngles = new Vector3(356.9376F, 25.8988F, 14.69767F),
            //        localScale = new Vector3(0.001F, 0.001F, 0.001F)
            //    }
            //});
            //rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0.00042F, 0.46133F, 0.01385F),
            //        localAngles = new Vector3(355.2848F, 47.55381F, 355.0908F),
            //        localScale = new Vector3(0.020392F, 0.020392F, 0.020392F)
            //    }
            //});
            //rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Chest",
            //        localPos = new Vector3(0.00076F, -0.0281F, 0.09539F),
            //        localAngles = new Vector3(338.9489F, 145.7505F, 217.6883F),
            //        localScale = new Vector3(0.005402F, 0.005402F, 0.005402F)
            //    }
            //});
            //rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, -0.00277F, -0.13259F),
            //        localAngles = new Vector3(322.1495F, 124.8318F, 235.476F),
            //        localScale = new Vector3(0.02F, 0.02F, 0.02F)
            //    }
            //});
            //rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Head",
            //        localPos = new Vector3(0F, 0.32104F, 0F),
            //        localAngles = new Vector3(0F, 321.2954F, 0F),
            //        localScale = new Vector3(0.024027F, 0.024027F, 0.024027F)
            //    }
            //});
            //rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "HeadCenter",
            //        localPos = new Vector3(0.00216F, 0.01033F, 0F),
            //        localAngles = new Vector3(0F, 323.6887F, 355.1232F),
            //        localScale = new Vector3(0.000551F, 0.000551F, 0.000551F)
            //    }
            //});
            return rules;
        }

        public override void Hooks()
        {
            
        }
    }
}
