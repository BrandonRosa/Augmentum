# Adding LookingGlass Item Stats to Your Mod

[LookingGlass](https://thunderstore.io/package/DropPod/LookingGlass/) can show extra stat lines under an item's description, for example:

```
Crit Chance: 15%
Duration: 4s
```

Vanilla items get these lines automatically. Modded items and equipment only get them if the mod registers them. This guide shows how to do that without making LookingGlass a hard requirement: your mod still loads normally when LookingGlass isn't installed.

The working reference is Augmentum's [`LookingGlassCompat.cs`](../BransItems/Modules/Compatability/LookingGlassCompat.cs). If you get stuck, compare against that file.

---

## Contents

1. [How it works](#how-it-works)
2. [Step 1: Reference the DLL](#step-1-reference-the-dll)
3. [Step 2: Add a soft dependency](#step-2-add-a-soft-dependency)
4. [Step 3: Call Init from your plugin](#step-3-call-init-from-your-plugin)
5. [Step 4: Create the compat class](#step-4-create-the-compat-class)
6. [Step 5: Register your items](#step-5-register-your-items)
7. [Step 6: Register your equipment](#step-6-register-your-equipment)
8. [Choosing value types and units](#choosing-value-types-and-units)
9. [Troubleshooting: "Nothing shows up"](#troubleshooting-nothing-shows-up)
10. [Checklist](#checklist)

---

## How it works

LookingGlass keeps two arrays, indexed by item index and equipment index:

- `ItemDefinitions.allItemDefinitions`
- `ItemDefinitions.allEquipmentDefinitions`

Each entry is an `ItemStatsDef`, which describes:

| Field | What it is |
|---|---|
| `descriptions` | The label for each line, e.g. `"Crit Chance: "` |
| `valueTypes` | The colour/category of each line (Damage, Healing, Utility, ...) |
| `measurementUnits` | How the number is formatted (%, seconds, meters, ...) |
| a `calculateValues...` callback | Returns a `List<float>` with one value per line |

To add stats, build an `ItemStatsDef` for your item and put it in the right slot of the array. That's all.

> **The golden rule:** `descriptions`, `valueTypes`, `measurementUnits` and the list your callback returns must all be **the same length and in the same order**. Line 1's label goes with value 1, and so on.

---

## Step 1: Reference the DLL

Copy `LookingGlass.dll` from the LookingGlass Thunderstore package into your project (for example a `libs/` folder) and reference it in your `.csproj`:

```xml
<Reference Include="LookingGlass">
  <HintPath>libs\LookingGlass.dll</HintPath>
  <Private>false</Private>
</Reference>
```

`<Private>false</Private>` stops the DLL from being copied into your build output. **Don't ship LookingGlass.dll with your mod.**

---

## Step 2: Add a soft dependency

On your plugin class:

```csharp
[BepInDependency("droppod.lookingglass", BepInDependency.DependencyFlags.SoftDependency)]
public class MyPlugin : BaseUnityPlugin
```

`SoftDependency` means "load after LookingGlass if it's there, but don't require it."

---

## Step 3: Call Init from your plugin

At the **end** of your `Awake()`, after all your items and equipment have been created:

```csharp
var enableLookingGlass = Config.Bind(
    "Mod Compatibility: LookingGlass",
    "Enable Compatibility Patches?",
    true,
    "Add item stats to LookingGlass (if installed)?").Value;

if (LookingGlassCompat.IsLookingGlassInstalled && enableLookingGlass)
    LookingGlassCompat.Init();
```

The config toggle is optional, but it gives users a way out if something breaks.

> ⚠️ **Don't use any LookingGlass type in your plugin class.** Keep every reference to LookingGlass inside the compat class. Otherwise the game tries to load LookingGlass when your plugin loads, and crashes for anyone who doesn't have it installed.

---

## Step 4: Create the compat class

Paste this in as a new file and change the namespace and logger to match your mod. It contains everything except your actual items.

```csharp
using LookingGlass.ItemStatsNameSpace;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StatType = LookingGlass.ItemStatsNameSpace.ItemStatsDef.ValueType;
using Units = LookingGlass.ItemStatsNameSpace.ItemStatsDef.MeasurementUnits;

internal static class LookingGlassCompat
{
    public const string GUID = "droppod.lookingglass";
    public static bool IsLookingGlassInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Init()
    {
        // Item and equipment indices don't exist until the catalogs are ready.
        ItemCatalog.availability.CallWhenAvailable(RegisterItemStats);
        EquipmentCatalog.availability.CallWhenAvailable(RegisterEquipmentStats);
    }

    // One stat line: label + colour + unit.
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

    // Values that only depend on the stack count.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddItem(ItemDef item, Func<int, List<float>> values, params Line[] lines)
    {
        var def = Create(lines);
        def.calculateValuesFlat = values;
        Register(item, def);
    }

    // Values that include a proc chance, which LookingGlass should show adjusted for luck.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddChanceItem(ItemDef item, Func<float, int, List<float>> values, params Line[] lines)
    {
        var def = Create(lines);
        def.hasChance = true;
        def.calculateValuesNew = (luck, stackCount, procChance) => values(luck, stackCount);
        Register(item, def);
    }

    // Values that depend on the holder's other items. master can be null (e.g. in the logbook).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddMasterItem(ItemDef item, Func<CharacterMaster, int, List<float>> values, params Line[] lines)
    {
        var def = Create(lines);
        def.calculateValues = values;
        Register(item, def);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddEquipment(EquipmentDef equipmentDef, Func<List<float>> values, params Line[] lines)
    {
        if (!equipmentDef || equipmentDef.equipmentIndex == EquipmentIndex.None)
            return; // disabled equipment
        var def = Create(lines);
        def.calculateValuesFlat = count => values();
        ItemDefinitions.allEquipmentDefinitions[(int)equipmentDef.equipmentIndex] = def;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Register(ItemDef item, ItemStatsDef def)
    {
        if (!item || item.itemIndex == ItemIndex.None)
            return; // disabled item
        ItemDefinitions.allItemDefinitions[(int)item.itemIndex] = def;
    }

    // Linear stacking: "X (+Y per stack)".
    private static float Stack(float initial, float additional, int count) => initial + additional * (count - 1);

    // Same luck rerolls as Util.CheckRoll (LookingGlass's own helper is internal). chance is 0..1.
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
            // Your items go here (see Step 5).
        }
        catch (Exception e)
        {
            Log.Error("Failed to register LookingGlass item stats: " + e);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterEquipmentStats()
    {
        try
        {
            // Your equipment goes here (see Step 6).
        }
        catch (Exception e)
        {
            Log.Error("Failed to register LookingGlass equipment stats: " + e);
        }
    }
}
```

### Why all the `[MethodImpl(MethodImplOptions.NoInlining)]`?

The .NET runtime loads a type's assembly when it compiles a method that uses that type. If such a method gets inlined into your plugin's code, LookingGlass gets loaded when your plugin loads, and the mod crashes for users without LookingGlass. `NoInlining` keeps those references contained in the compat class, which only runs after we've checked that LookingGlass is installed.

**Rule of thumb:** any method in this class that mentions a LookingGlass type gets the attribute.

### If your mod uses an item base class

Many mods wrap items in something like `ItemBase` with a static `instance` and an `ItemDef` property. In that case you can change the helpers to take your base type directly. That's what Augmentum does:

```csharp
private static void AddItem(ItemBase item, Func<int, List<float>> values, params Line[] lines) { ... }

private static void Register(ItemBase item, ItemStatsDef def)
{
    if (item == null || !item.ItemDef || item.ItemDef.itemIndex == ItemIndex.None)
        return;
    ItemDefinitions.allItemDefinitions[(int)item.ItemDef.itemIndex] = def;
}

// Handy for AddMasterItem: read another item's stack count from the holder.
private static int Count(ItemBase item, CharacterMaster master) => item != null ? item.GetCount(master) : 0;
```

---

## Step 5: Register your items

Put one call per item inside `RegisterItemStats()`. Pick the helper based on what the numbers depend on:

| Helper | Use when the values depend on... |
|---|---|
| `AddItem` | only the stack count (most items) |
| `AddChanceItem` | stack count **and** there's a proc chance that luck affects |
| `AddMasterItem` | other items the player is holding |

> **Read the item's actual code** (`RecalculateStats`, on-hit hooks, and so on), not just its description. The numbers you show should match what the item really does, including how it stacks.

> **Note:** Apart from the first one, the examples below come from Augmentum, which uses the `ItemBase` helper variant. That's why they pass `NovaRing.instance` instead of `NovaRing.instance?.ItemDef`. With the plain template, pass the `ItemDef`.

### Example: a simple stacking stat

An item that gives "10% crit chance (+5% per stack)", with config values stored as whole numbers (`10`, `5`):

```csharp
AddItem(MyCritItem.instance?.ItemDef, count => new List<float>
    {
        Stack(MyCritItem.BaseCrit * .01f, MyCritItem.CritPerStack * .01f, count),
    },
    new Line("Crit Chance: ", StatType.Damage, Units.Percentage));
```

### Example: several lines

```csharp
AddItem(NovaRing.instance, count => new List<float>
    {
        Stack(NovaRing.InitialPercent, NovaRing.AdditionalPercent, count),
        Stack(NovaRing.InitialMaxHealing, NovaRing.AdditionalMaxHealing, count),
        Stack(NovaRing.InitialRange, NovaRing.AdditionalRange, count),
    },
    new Line("Shield: ", StatType.Healing, Units.Percentage),
    new Line("Max Shield: ", StatType.Healing, Units.PercentHealth),
    new Line("Radius: ", StatType.Utility, Units.Meters));
```

Three values, three lines, in the same order.

### Example: non-linear stacking

If the item uses a curve, copy the formula exactly and leave a comment saying where it came from:

```csharp
AddItem(SafetyBlanket.instance, count => new List<float>
    {
        // Same curve SafetyBlanket applies to oneShotProtectionFraction.
        (.75f - .15f) * (1f - (float)Math.Exp(-(count - 1f) / 13f)) + .15f,
        Stack(SafetyBlanket.InitialIFrames, SafetyBlanket.AdditionalIFrames, count),
    },
    new Line("One-Shot Protection: ", StatType.Healing, Units.PercentHealth),
    new Line("Invincibility Time: ", StatType.Utility, Units.Seconds));
```

### Example: a luck-affected chance

```csharp
AddChanceItem(PuzzleBox.instance, (luck, count) => new List<float>
    {
        PuzzleBox.LuckGain * count,
        ChanceWithLuck(Math.Min(1f, PuzzleBox.UpgradeChance + (count - 1) * .10f), luck),
    },
    new Line("Luck: ", StatType.Utility, Units.Percentage),
    new Line("Upgrade Chance: ", StatType.Utility, Units.Percentage));
```

### Example: depends on other items

`master` is **null** when the player looks at the item in the logbook, so always guard it:

```csharp
AddMasterItem(MirrorShard.instance, (master, count) =>
    {
        int whites = master && master.inventory
            ? master.inventory.GetTotalItemCountOfTier(ItemTier.Tier1) - Count(MirrorShard.instance, master)
            : 0;
        return new List<float> { Math.Max(0, whites) };
    },
    new Line("White Items To Copy: ", StatType.Utility, Units.Number));
```

### Tips

- **Large mod?** Split registration into several methods (`RegisterTier1()`, `RegisterTier2()`, ...) and call them all from `RegisterItemStats()`. Mark each one `NoInlining`.
- **Items with no real numbers** (pure on/off effects) can just be left out. Don't invent stats.
- **End every label with `": "`**, e.g. `"Damage: "`. LookingGlass appends the value directly after it.

---

## Step 6: Register your equipment

Equipment works the same way, except the callback takes **no arguments**. There's no stack count and no master.

```csharp
AddEquipment(AirTotem.instance?.EquipmentDef, () => new List<float>
    {
        AirTotem.T1EssenceCount,
        AirTotem.T2EssenceCount,
        AirTotem.T3EssenceCount,
    },
    new Line("White Item Essences: ", StatType.Utility, Units.Number),
    new Line("Green Item Essences: ", StatType.Utility, Units.Number),
    new Line("Red Item Essences: ", StatType.Utility, Units.Number));
```

If you need the player's state, get the local player yourself and null-check everything:

```csharp
AddEquipment(EarthTotem.instance?.EquipmentDef, () =>
    {
        CharacterMaster master = LocalUserManager.GetFirstLocalUser()?.cachedMaster;
        EarthTotemTracker tracker = master ? master.GetComponent<EarthTotemTracker>() : null;
        return new List<float> { tracker ? tracker.EquipDefList.Count : 0 };
    },
    new Line("Absorbed Equipment: ", StatType.Utility, Units.Number));
```

---

## Choosing value types and units

### Units (how the number is displayed)

| Unit | Displays | Value you pass |
|---|---|---|
| `Units.Percentage` | `28%` | a **fraction**: `0.28` |
| `Units.PercentHealth` | `28%` of max HP | a fraction: `0.28` |
| `Units.FlatHealing` | HP or HP/s | raw number |
| `Units.FlatHealth` | flat HP | raw number |
| `Units.Number` | `3` | raw number |
| `Units.Seconds` | `4s` | raw seconds |
| `Units.Meters` | `12m` | raw meters |

> ⚠️ **The most common mistake:** `Percentage` expects `0.28`, not `28`. If your config stores `28`, multiply by `.01f`. Check each config value separately, because some mods store some values as whole numbers and others as fractions.

### Value types (the colour of the line)

`Damage`, `Healing`, `Health` (red), `Utility`, `Armor`, `Void`, `Death`, `Gold`, `HumanObjective`, `LunarObjective`, `WorldEvent`, `UserSetting`

Common choices:

| Stat kind | Value type |
|---|---|
| Damage, attack speed, crit | `Damage` |
| Healing, regen, barrier, shields | `Healing` |
| Armor, damage reduction | `Armor` |
| Movement, cooldowns, durations, counts | `Utility` |
| Void items | `Void` |

### Stacking formulas

| Description says | Formula |
|---|---|
| "X (+Y per stack)" | `Stack(X, Y, count)`, i.e. `X + Y * (count - 1)` |
| "X per stack" | `X * count` |
| Hyperbolic / exponential / capped | copy the item's own formula and add a comment |

---

## Troubleshooting: "Nothing shows up"

### 1. The description token isn't a real language token

This is the most common cause. **LookingGlass skips any item whose `descriptionToken` isn't a registered language token.** Some mods set `ItemDef.descriptionToken` directly to the finished description text instead of a token like `ITEM_FOO_DESC`. LookingGlass can't find that "token", so it shows nothing.

Fix: register the text under a real token and point the ItemDef at it. In Augmentum's `ItemBase`:

```csharp
public string FormattedDescriptionToken => "ITEM_" + ItemLangTokenName + "_DESCRIPTION_FORMATTED";

public void UpdateDescriptionToken()
{
    if (!ItemDef) return;
    Language.currentLanguage?.SetStringByToken(FormattedDescriptionToken, ItemFullDescriptionFormatted);
    ItemDef.descriptionToken = FormattedDescriptionToken;
}
```

Call `UpdateDescriptionToken()`:
- when the item is created
- on `Config.SettingChanged` (so description edits from config still show up)
- after `Language.LoadStrings`
- after `Language.SetCurrentLanguage`

Do the same for equipment if it uses the same pattern.

### 2. Other things to check

- **Is `Init()` actually called?** Add a log line next to it.
- **Was the item created before `Init()`?** Call `Init()` at the end of `Awake`.
- **Is the item disabled in config?** Disabled items are skipped on purpose.
- **Wrong numbers?** Check the `* .01f` scaling and that the list order matches the line order.
- **Errors in the log?** The try/catch logs them. A length mismatch or a null reference inside a callback are the usual causes.

---

## Checklist

- [ ] `LookingGlass.dll` referenced with `<Private>false</Private>`
- [ ] `[BepInDependency("droppod.lookingglass", SoftDependency)]` on the plugin
- [ ] `Init()` called at the end of `Awake`, behind the installed check (and the config toggle)
- [ ] No LookingGlass types used outside the compat class
- [ ] Every method that uses LookingGlass types has `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Every item's labels, value types, units and values have the same length and order
- [ ] Percentages are passed as fractions
- [ ] `master` null-checked in `AddMasterItem` callbacks
- [ ] Description tokens are real language tokens
- [ ] **In game:** stats show when hovering items and update as stacks change
- [ ] **In game:** stats show in the logbook without errors
- [ ] **Without LookingGlass installed:** the mod still loads fine
