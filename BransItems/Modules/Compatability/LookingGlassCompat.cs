using Augmentum.Modules.Pickups;
using Augmentum.Modules.Pickups.Equipments;
using Augmentum.Modules.Pickups.Items.CoreItems;
using Augmentum.Modules.Pickups.Items.Essences;
using Augmentum.Modules.Pickups.Items.HighlanderItems;
using Augmentum.Modules.Pickups.Items.Lunar;
using Augmentum.Modules.Pickups.Items.NoTier;
using Augmentum.Modules.Pickups.Items.Tier1;
using Augmentum.Modules.Pickups.Items.Tier2;
using Augmentum.Modules.Pickups.Items.Tier3;
using LookingGlass.ItemStatsNameSpace;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StatType = LookingGlass.ItemStatsNameSpace.ItemStatsDef.ValueType;
using Units = LookingGlass.ItemStatsNameSpace.ItemStatsDef.MeasurementUnits;

namespace Augmentum.Modules.Compatability
{
    internal static partial class ModCompatability
    {
        /// <summary>
        /// Adds the extra stat lines LookingGlass shows under item descriptions (e.g. "Root Chance: 28%").
        /// Every method touching LookingGlass types is NoInlining so nothing loads unless LookingGlass is installed.
        /// </summary>
        internal static class LookingGlassCompat
        {
            public const string GUID = "droppod.lookingglass";
            public static bool IsLookingGlassInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void Init()
            {
                ItemCatalog.availability.CallWhenAvailable(RegisterItemStats);
                EquipmentCatalog.availability.CallWhenAvailable(RegisterEquipmentStats);
            }

            private struct Line
            {
                public string description;
                public StatType valueType;
                public Units units;

                public Line(string description, StatType valueType, Units units)
                {
                    this.description = description;
                    this.valueType = valueType;
                    this.units = units;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static ItemStatsDef Create(Line[] lines)
            {
                var def = new ItemStatsDef();
                foreach (var line in lines)
                {
                    def.descriptions.Add(line.description);
                    def.valueTypes.Add(line.valueType);
                    def.measurementUnits.Add(line.units);
                }
                return def;
            }

            /// <summary>Stats that only depend on the stack count.</summary>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void AddItem(ItemBase item, Func<int, List<float>> values, params Line[] lines)
            {
                var def = Create(lines);
                def.calculateValuesFlat = values;
                Register(item, def);
            }

            /// <summary>Stats with a chance that LookingGlass should adjust for luck.</summary>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void AddChanceItem(ItemBase item, Func<float, int, List<float>> values, params Line[] lines)
            {
                var def = Create(lines);
                def.hasChance = true;
                def.calculateValuesNew = (luck, stackCount, procChance) => values(luck, stackCount);
                Register(item, def);
            }

            /// <summary>Stats that depend on other items the holder has. The master can be null (e.g. logbook).</summary>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void AddMasterItem(ItemBase item, Func<CharacterMaster, int, List<float>> values, params Line[] lines)
            {
                var def = Create(lines);
                def.calculateValues = values;
                Register(item, def);
            }

            private static int Count(ItemBase item, CharacterMaster master) => item != null ? item.GetCount(master) : 0;

            /// <summary>Choices every Matroyshka discovery gets, matching the WishOptions formula they use.</summary>
            private static float MatroyshkaChoices(CharacterMaster master)
            {
                return 1 + Count(MegaMatroyshkaShells.instance, master) * MegaMatroyshka.AdditionalChoices
                    + Count(DiscoveryMedallionConsumed.instance, master) * DiscoveryMedallion.AdditionalChoices;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void AddEquipment(EquipmentDef equipmentDef, Func<List<float>> values, params Line[] lines)
            {
                if (!equipmentDef || equipmentDef.equipmentIndex == EquipmentIndex.None)
                    return;
                var def = Create(lines);
                def.calculateValuesFlat = count => values();
                ItemDefinitions.allEquipmentDefinitions[(int)equipmentDef.equipmentIndex] = def;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void Register(ItemBase item, ItemStatsDef def)
            {
                // Disabled items never create an ItemDef.
                if (item == null || !item.ItemDef || item.ItemDef.itemIndex == ItemIndex.None)
                    return;
                ItemDefinitions.allItemDefinitions[(int)item.ItemDef.itemIndex] = def;
            }

            private static float Stack(float initial, float additional, int count) => initial + additional * (count - 1);

            /// <summary>Same rerolling Util.CheckRoll does for luck (LookingGlass's own helper is internal).</summary>
            private static float ChanceWithLuck(float chance, float luck)
            {
                chance = Math.Min(chance, 1f);
                int rolls = (int)Math.Ceiling(luck);
                if (rolls > 0)
                    return 1f - (float)Math.Pow(1f - chance, rolls + 1);
                if (rolls < 0)
                    return (float)Math.Pow(chance, -rolls + 1);
                return chance;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void RegisterItemStats()
            {
                try
                {
                    RegisterEssences();
                    RegisterTieredItems();
                    RegisterHighlanderItems();
                    RegisterMatroyshkasAndDiscoveries();
                }
                catch (Exception e)
                {
                    Augmentum.ModLogger.LogError("ModCompatability: Failed to register LookingGlass item stats: " + e);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void RegisterEssences()
            {
                AddItem(EOAcuity.instance, count => new List<float> { EOAcuity.CritChanceGain * .01f * count },
                    new Line("Crit Chance: ", StatType.Damage, Units.Percentage));

                AddItem(EOFerocity.instance, count => new List<float> { EOFerocity.AttackSpeedGain * .01f * count },
                    new Line("Attack Speed: ", StatType.Damage, Units.Percentage));

                AddItem(EOLife.instance, count => new List<float> { EOLife.HealthGain * count },
                    new Line("Max Health: ", StatType.Healing, Units.Percentage));

                AddItem(EOResilience.instance, count => new List<float> { EOResilience.ArmorGain * count },
                    new Line("Armor: ", StatType.Armor, Units.Number));

                AddItem(EOStrength.instance, count => new List<float> { EOStrength.DamageGain * count },
                    new Line("Damage: ", StatType.Damage, Units.Percentage));

                AddItem(EOVelocity.instance, count => new List<float> { EOVelocity.MoveSpeedGain * .01f * count },
                    new Line("Movement Speed: ", StatType.Utility, Units.Percentage));

                AddItem(EOTotality.instance, count => new List<float>
                    {
                        EOTotality.AttackSpeedGain * .01f * count,
                        EOTotality.MoveSpeedGain * .01f * count,
                        EOTotality.CritChanceGain * .01f * count,
                        EOTotality.DamageGain * count,
                        EOTotality.HealthGain * count,
                        EOTotality.ArmorGain * count,
                    },
                    new Line("Attack Speed: ", StatType.Damage, Units.Percentage),
                    new Line("Movement Speed: ", StatType.Utility, Units.Percentage),
                    new Line("Crit Chance: ", StatType.Damage, Units.Percentage),
                    new Line("Damage: ", StatType.Damage, Units.Percentage),
                    new Line("Max Health: ", StatType.Healing, Units.Percentage),
                    new Line("Armor: ", StatType.Armor, Units.Number));

                AddItem(CloverSprout.instance, count => new List<float> { CloverSprout.LuckGain * count },
                    new Line("Luck: ", StatType.Utility, Units.Percentage));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void RegisterTieredItems()
            {
                AddItem(PiggyBrine.instance, count => new List<float>
                    {
                        Stack(PiggyBrine.RegenPercent, PiggyBrine.RegenAdditionalPercent, count),
                        Stack(PiggyBrine.RegenBase, PiggyBrine.RegenAdditionalBase, count),
                    },
                    new Line("Regen Bonus: ", StatType.Healing, Units.Percentage),
                    new Line("Flat Regen: ", StatType.Healing, Units.FlatHealing));

                AddItem(SafetyBlanket.instance, count => new List<float>
                    {
                        // Same curve SafetyBlanket applies to oneShotProtectionFraction.
                        (.75f - .15f) * (1f - (float)Math.Exp(-(count - 1f) / 13f)) + .15f,
                        Stack(SafetyBlanket.InitialIFrames, SafetyBlanket.AdditionalIFrames, count),
                    },
                    new Line("One-Shot Protection: ", StatType.Healing, Units.PercentHealth),
                    new Line("Invincibility Time: ", StatType.Utility, Units.Seconds));

                AddItem(AdaptiveArmor.instance, count => new List<float>
                    {
                        // Each Fortified stack blocks 5 damage.
                        5f * Stack(AdaptiveArmor.InitialFortify, AdaptiveArmor.AdditionalFortify, count),
                        Stack(AdaptiveArmor.InitialFortifyTime, AdaptiveArmor.AdditionalFortifyTime, count),
                    },
                    new Line("Damage Reduction: ", StatType.Armor, Units.Number),
                    new Line("Duration: ", StatType.Utility, Units.Seconds));

                AddItem(BarrierRing.instance, count => new List<float>
                    {
                        Stack(BarrierRing.InitialPercent, BarrierRing.AdditionalPercent, count),
                        Stack(BarrierRing.InitialMaxHealing, BarrierRing.AdditionalMaxHealing, count),
                    },
                    new Line("Barrier: ", StatType.Healing, Units.Percentage),
                    new Line("Max Barrier: ", StatType.Healing, Units.PercentHealth));

                AddItem(HealRing.instance, count => new List<float>
                    {
                        Stack(HealRing.InitialPercent, HealRing.AdditionalPercent, count),
                        Stack(HealRing.InitialMaxHealing, HealRing.AdditionalMaxHealing, count),
                    },
                    new Line("Healing: ", StatType.Healing, Units.Percentage),
                    new Line("Max Healing: ", StatType.Healing, Units.PercentHealth));

                AddItem(NovaRing.instance, count => new List<float>
                    {
                        Stack(NovaRing.InitialPercent, NovaRing.AdditionalPercent, count),
                        Stack(NovaRing.InitialMaxHealing, NovaRing.AdditionalMaxHealing, count),
                        Stack(NovaRing.InitialRange, NovaRing.AdditionalRange, count),
                    },
                    new Line("Shield: ", StatType.Healing, Units.Percentage),
                    new Line("Max Shield: ", StatType.Healing, Units.PercentHealth),
                    new Line("Radius: ", StatType.Utility, Units.Meters));

                AddItem(CharmOfDesires.instance, count => new List<float> { CharmOfDesires.DropCount + (count - 1) * CharmOfDesires.AdditionalDrops },
                    new Line("Essences Per Stage: ", StatType.Utility, Units.Number));

                AddItem(LootedBloodburstClam.instance, count => new List<float> { BloodburstClam.AdditionalDrops * count },
                    new Line("Extra Essences: ", StatType.Utility, Units.Number));

                AddItem(DiscoveryMedallionConsumed.instance, count => new List<float> { DiscoveryMedallion.AdditionalChoices * count },
                    new Line("Extra Choices: ", StatType.Utility, Units.Number));

                AddItem(MegaMatroyshkaShells.instance, count => new List<float> { MegaMatroyshka.AdditionalChoices * count },
                    new Line("Extra Matroyshka Choices: ", StatType.Utility, Units.Number));

                AddItem(GlassVeil.instance, count => new List<float> { GlassVeil.DefaultInvincibleTime, GlassVeil.DefaultCooldownTime },
                    new Line("Invincibility: ", StatType.Healing, Units.Seconds),
                    new Line("Recharge: ", StatType.Utility, Units.Seconds));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void RegisterHighlanderItems()
            {
                AddItem(AugmentedContact.instance, count => new List<float>
                    {
                        AugmentedContact.CritChanceGain * .01f * count,
                        AugmentedContact.DamageGain * .01f * count,
                    },
                    new Line("Crit Chance: ", StatType.Damage, Units.Percentage),
                    new Line("Crit Damage: ", StatType.Damage, Units.Percentage));

                AddItem(BattleArmor.instance, count => new List<float>
                    {
                        BattleArmor.AttackSpeedIncrease * .01f * BattleArmor.MaxSeconds,
                        BattleArmor.ArmorIncrease * BattleArmor.MaxSeconds,
                        BattleArmor.MaxSeconds,
                    },
                    new Line("Max Attack Speed: ", StatType.Damage, Units.Percentage),
                    new Line("Max Armor: ", StatType.Armor, Units.Number),
                    new Line("Time To Max: ", StatType.Utility, Units.Seconds));

                AddItem(BlazeOfGlory.instance, count => new List<float>
                    {
                        BlazeOfGlory.DamageDealt * .01f,
                        BlazeOfGlory.Radius,
                        BlazeOfGlory.SpawnCount,
                    },
                    new Line("Explosion Damage: ", StatType.Damage, Units.Percentage),
                    new Line("Explosion Radius: ", StatType.Damage, Units.Meters),
                    new Line("Jellyfish Summoned: ", StatType.Utility, Units.Number));

                AddItem(CurvedHorn.instance, count => new List<float>
                    {
                        CurvedHorn.DamageGain * count,
                        CurvedHorn.PrimaryChargeMultiplier - 1f,
                    },
                    new Line("Damage: ", StatType.Damage, Units.Percentage),
                    new Line("Primary Charges: ", StatType.Utility, Units.Percentage));

                AddItem(GoatLeg.instance, count => new List<float>
                    {
                        GoatLeg.MovementGain * .01f * count,
                        GoatLeg.SprintGain * .01f * count,
                        GoatLeg.JumpPowerGain * .01f * count,
                    },
                    new Line("Movement Speed: ", StatType.Utility, Units.Percentage),
                    new Line("Sprint Speed: ", StatType.Utility, Units.Percentage),
                    new Line("Jump Height: ", StatType.Utility, Units.Percentage));

                // The band cooldown is multiplied by CooldownReduction, so the actual reduction is the remainder.
                AddItem(CooldownBand.instance, count => new List<float> { 1f - CooldownBand.CooldownReduction, .5f },
                    new Line("Band Cooldown Reduction: ", StatType.Utility, Units.Percentage),
                    new Line("Bonus Damage On 350%+ Hits: ", StatType.Damage, Units.Percentage));

                // Band effects loop once per stack plus the normal trigger.
                AddItem(DoubleBand.instance, count => new List<float> { count + 1, .5f },
                    new Line("Band Triggers: ", StatType.Utility, Units.Number),
                    new Line("Bonus Damage On 400%+ Hits: ", StatType.Damage, Units.Percentage));

                AddItem(AbyssalBeacon.instance, count => new List<float> { 1f },
                    new Line("Null Portals Per Teleporter: ", StatType.Void, Units.Number));

                AddChanceItem(PuzzleBox.instance, (luck, count) => new List<float>
                    {
                        PuzzleBox.LuckGain * count,
                        ChanceWithLuck(Math.Min(1f, PuzzleBox.UpgradeChance + (count - 1) * .10f), luck),
                    },
                    new Line("Luck: ", StatType.Utility, Units.Percentage),
                    new Line("Upgrade Chance: ", StatType.Utility, Units.Percentage));

                AddItem(SecondDeath.instance, count => new List<float> { SecondDeath.HealthPercent },
                    new Line("Boss On-Kill Every: ", StatType.Health, Units.PercentHealth));

                AddItem(TonatiuhsForge.instance, count => new List<float>
                    {
                        TonatiuhsForge.DamageGain,
                        TonatiuhsForge.Duration,
                    },
                    new Line("Damage Per Cooldown Second: ", StatType.Damage, Units.Percentage),
                    new Line("Duration: ", StatType.Utility, Units.Seconds));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void RegisterMatroyshkasAndDiscoveries()
            {
                // Mini adds one essence per Looted Clam (not scaled by the clam config).
                AddMasterItem(MiniMatroyshka.instance, (master, count) => new List<float>
                    {
                        count * MiniMatroyshka.DropCount + Count(LootedBloodburstClam.instance, master),
                        MatroyshkaChoices(master),
                    },
                    new Line("Essences On Chest Purchase: ", StatType.Utility, Units.Number),
                    new Line("Choices Each: ", StatType.Utility, Units.Number));

                AddMasterItem(MediumMatroyshka.instance, (master, count) => new List<float>
                    {
                        count * MediumMatroyshka.DropCount,
                        MatroyshkaChoices(master),
                        count,
                    },
                    new Line("White Items On Equipment Use: ", StatType.Utility, Units.Number),
                    new Line("Choices Each: ", StatType.Utility, Units.Number),
                    new Line("Mini Matroyshkas Gained: ", StatType.Utility, Units.Number));

                AddMasterItem(MassiveMatroyshka.instance, (master, count) => new List<float>
                    {
                        count,
                        MatroyshkaChoices(master),
                        count,
                    },
                    new Line("Green Items On Teleporter: ", StatType.Healing, Units.Number),
                    new Line("Choices Each: ", StatType.Utility, Units.Number),
                    new Line("Medium Matroyshkas Gained: ", StatType.Utility, Units.Number));

                AddMasterItem(MegaMatroyshka.instance, (master, count) => new List<float>
                    {
                        count,
                        MatroyshkaChoices(master),
                        count,
                    },
                    new Line("Red Items On Elite Kill: ", StatType.Health, Units.Number),
                    new Line("Choices Each: ", StatType.Utility, Units.Number),
                    new Line("Massive Matroyshkas Gained: ", StatType.Utility, Units.Number));

                // White stacks Mirror Shard can become (it copies one at random, weighted by stack size).
                AddMasterItem(MirrorShard.instance, (master, count) =>
                    {
                        int whites = master && master.inventory ? master.inventory.GetTotalItemCountOfTier(ItemTier.Tier1) - Count(MirrorShard.instance, master) : 0;
                        return new List<float> { Math.Max(0, whites) };
                    },
                    new Line("White Items To Copy: ", StatType.Utility, Units.Number));

                AddMasterItem(ShatteredPiggyBrine.instance, (master, count) => new List<float> { count },
                    new Line("Brines Broken: ", StatType.Death, Units.Number));

                // The medallion is consumed before rolling, so it already counts towards its own discovery.
                AddMasterItem(DiscoveryMedallion.instance, (master, count) => new List<float>
                    {
                        DiscoveryMedallion.Choices + (Count(DiscoveryMedallionConsumed.instance, master) + 1) * DiscoveryMedallion.AdditionalChoices,
                    },
                    new Line("Red Item Choices: ", StatType.Health, Units.Number));

                // Same: the clam turns into a Looted Clam before it drops.
                AddMasterItem(BloodburstClam.instance, (master, count) => new List<float>
                    {
                        BloodburstClam.DropCount + (Count(LootedBloodburstClam.instance, master) + 1) * BloodburstClam.AdditionalDrops,
                    },
                    new Line("Essences On Pickup: ", StatType.Utility, Units.Number));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void RegisterEquipmentStats()
            {
                try
                {
                    AddEquipment(AirTotem.instance?.EquipmentDef, () => new List<float> { AirTotem.T1EssenceCount, AirTotem.T2EssenceCount, AirTotem.T3EssenceCount, AirTotem.BossEssenceCount },
                        new Line("White Item Essences: ", StatType.Utility, Units.Number),
                        new Line("Green Item Essences: ", StatType.Utility, Units.Number),
                        new Line("Red Item Essences: ", StatType.Utility, Units.Number),
                        new Line("Boss Item Essences: ", StatType.Utility, Units.Number));

                    // Equipment stats get no master from LookingGlass, so read the local player's tracker.
                    AddEquipment(EarthTotem.instance?.EquipmentDef, () =>
                        {
                            CharacterMaster master = LocalUserManager.GetFirstLocalUser()?.cachedMaster;
                            EarthTotemTracker tracker = master ? master.GetComponent<EarthTotemTracker>() : null;
                            return new List<float>
                            {
                                tracker ? tracker.EquipDefList.Count : 0,
                                tracker ? tracker.EarthTotemAbsorbedCount : 0,
                            };
                        },
                        new Line("Absorbed Equipment: ", StatType.Utility, Units.Number),
                        new Line("Reusers Absorbed: ", StatType.Utility, Units.Number));

                    AddEquipment(Pickups.EliteEquipments.AffixAdaptive.instance?.EliteEquipmentDef, () => new List<float>
                        {
                            Pickups.EliteEquipments.AffixAdaptive.AdaptiveBoostTimer,
                            Pickups.EliteEquipments.AffixAdaptive.InvisibleTimer,
                            Pickups.EliteEquipments.AffixAdaptive.AdaptiveCooldownTimer,
                        },
                        new Line("Defensive Boost: ", StatType.Healing, Units.Seconds),
                        new Line("Invisibility: ", StatType.Utility, Units.Seconds),
                        new Line("Recharge: ", StatType.Utility, Units.Seconds));
                }
                catch (Exception e)
                {
                    Augmentum.ModLogger.LogError("ModCompatability: Failed to register LookingGlass equipment stats: " + e);
                }
            }
        }
    }
}
