using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using R2API.Utils;
using System.Linq;
using Augmentum.Modules.Pickups;
using System.Collections.Generic;
using Augmentum.Modules.ItemTiers;
using RoR2.ContentManagement;
using Augmentum.Modules.ItemTiers.CoreTier;
using Augmentum.Modules.ItemTiers.HighlanderTier;
using Augmentum.Modules.Utils;
using Augmentum.Modules.StandaloneBuffs;
using Augmentum.Modules.ColorCatalogEntry;
using Augmentum.Modules.Pickups.Equipments;
using Augmentum.Modules.Compatability;
using Augmentum.Modules.Pickups.Items.Essences;
using Augmentum.Modules.Pickups.Items.HighlanderItems;
using HarmonyLib;

namespace Augmentum
{

    
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency("com.KingEnderBrine.ProperSave", BepInDependency.DependencyFlags.SoftDependency)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class Augmentum : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string ModGuid = "com.BrandonRosa.Augmentum"; //Our Package Name
        public const string ModName = "Augmentum";
        public const string ModVer = "1.1.6";


        internal static BepInEx.Logging.ManualLogSource ModLogger;

        public static ConfigFile AugConfig;
        public static ConfigFile AugBackupConfig;
        public static ConfigEntry<bool> enableAutoConfig { get; set; }
        public static ConfigEntry<string> latestVersion { get; set; }

        public static bool _preVersioning = false;

        //We need our item definition to persist through our functions, and therefore make it a class field.
        //private static ItemDef myItemDef;

        public static AssetBundle MainAssets;

        //public List<CoreModule> CoreModules = new List<CoreModule>();
        public List<BuffBase> Buffs = new List<BuffBase>();
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public List<ItemTierBase> ItemTiers = new List<ItemTierBase>();
        public List<EliteEquipmentBase> EliteEquipments = new List<EliteEquipmentBase>();
        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
       // public List<InteractableBase> Interactables = new List<InteractableBase>();
       // public List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public static HashSet<ItemDef> BlacklistedFromPrinter = new HashSet<ItemDef>();

        //public static ExpansionDef AetheriumExpansionDef = ScriptableObject.CreateInstance<ExpansionDef>();


        // For modders that seek to know whether or not one of the items or equipment are enabled for use in...I dunno, adding grip to Blaster Sword?

        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new Dictionary<ItemBase, bool>();
        public static Dictionary<EquipmentBase, bool> EquipmentStatusDictionary = new Dictionary<EquipmentBase, bool>();
        public static Dictionary<BuffBase, bool> BuffStatusDictionary = new Dictionary<BuffBase, bool>();
        public static Dictionary<EliteEquipmentBase, bool> EliteEquipmentStatusDictionary = new Dictionary<EliteEquipmentBase, bool>();
        public static Dictionary<ArtifactBase, bool> ArtifactStatusDictionary = new Dictionary<ArtifactBase, bool>();
        //public static Dictionary<InteractableBase, bool> InteractableStatusDictionary = new Dictionary<InteractableBase, bool>();
        // public static Dictionary<SurvivorBase, bool> SurvivorStatusDictionary = new Dictionary<SurvivorBase, bool>();

        public static string EssenceKeyword => "<color=#" + ColorCatalog.GetColorHexString(Colors.TempCoreLight) + ">Essence</color>";
        public static string EssencesKeyword => "<color=#" + ColorCatalog.GetColorHexString(Colors.TempCoreLight) + ">Essences</color>";

        public static string CoreColorString => "<color=#" + ColorCatalog.GetColorHexString(Colors.TempCoreLight) + ">";
        public void Awake()
        {
            ModLogger = this.Logger;
            AugConfig = Config;

            AugBackupConfig = new(Paths.ConfigPath + "\\" + ModGuid + "." + ".Backup.cfg", true);
            AugBackupConfig.Bind(": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :");

            enableAutoConfig = AugConfig.Bind("Config", "Enable Auto Config Sync", true, "Disabling this would stop Augmentum from syncing config whenever a new version is found.");
            _preVersioning = !((Dictionary<ConfigDefinition, string>)AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke(AugConfig, null)).Keys.Any(x => x.Key == "Latest Version");
            latestVersion = AugConfig.Bind("Config", "Latest Version", ModVer, "DO NOT CHANGE THIS");
            if (enableAutoConfig.Value && (_preVersioning || (latestVersion.Value != ModVer)))
            {
                latestVersion.Value = ModVer;
                ConfigManager.VersionChanged = true;
                ModLogger.LogInfo("Config Autosync Enabled.");
            }
        }

        private void Start()
        { 

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Augmentum.bransitems_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }
            Modules.ColorCatalogEntry.Colors.Init();
            Modules.Utils.ItemTierPickupVFXHelper.SystemInitializer();
            //var disableSurvivor = Config.ActiveBind<bool>("Survivor", "Disable All Survivors?", false, "Do you wish to disable every survivor in Aetherium?");
            /*
            if (true)
            {
                //ItemTier Initialization
                var ColorCatalogEntries = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ColorCatalogEntryBase)));

                ModLogger.LogInfo("-----------------COLORS---------------------");

                foreach (var colorCatalogEntry in ColorCatalogEntries)
                {
                    ColorCatalogEntryBase color = (ColorCatalogEntryBase)System.Activator.CreateInstance(colorCatalogEntry);
                    if (true)//ValidateSurvivor(itemtier, Survivors))
                    {
                        color.Init();

                        ModLogger.LogInfo("Color: " + color.ColorCatalogEntryName + " Initialized!");
                    }
                }
            }
            */
            if (true)
            {
                //ItemTier Initialization
                var ItemTierTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemTierBase)));

                ModLogger.LogInfo("-----------------ITEMTIERS---------------------");

                foreach (var itemTierType in ItemTierTypes)
                {
                    ItemTierBase itemtier = (ItemTierBase)System.Activator.CreateInstance(itemTierType);
                    if (true)
                    {
                        itemtier.Init();

                        ModLogger.LogInfo("ItemTier: " + itemtier.TierName + " Initialized!");
                    }
                }
            }

            //var disableBuffs = Config.Bind<bool>("Buffs", "Disable All Standalone Buffs?", false, "Do you wish to disable every standalone buff in Aetherium?").Value;
            if (true)
            {
                //Standalone Buff Initialization
                var BuffTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(BuffBase)));

                ModLogger.LogInfo("--------------BUFFS---------------------");

                foreach (var buffType in BuffTypes)
                {
                    BuffBase buff = (BuffBase)System.Activator.CreateInstance(buffType);
                    if (ValidateBuff(buff, Buffs))
                    {
                        buff.Init(Config);

                        ModLogger.LogInfo("Buff: " + buff.BuffName + " Initialized!");
                    }
                }
            }

            var disableItems = ConfigManager.ConfigOptionValue<bool>("Items", "Disable All Items?", false, "Do you wish to disable every item in Augmentum?",true);
            if (!disableItems)
            {
                //Item Initialization
                var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

                ModLogger.LogInfo("----------------------ITEMS--------------------");

                foreach (var itemType in ItemTypes)
                {
                    ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                    if (!item.BlacklistFromPreLoad && ValidateItem(item, Items))
                    {
                        item.Init(Config);
                        Config.SettingChanged += (object o, SettingChangedEventArgs args) => { item.ItemDef.descriptionToken = item.ItemFullDescriptionFormatted; };
                        ModLogger.LogInfo("Item: " + item.ItemName + " Initialized!");
                        //if (item.ItemDef._itemTierDef==Core.instance.itemTierDef)
                        //{
                        //    Core.instance.ItemsWithThisTier.Add(item.ItemDef.itemIndex);
                        //    Core.instance.AvailableTierDropList.Add(PickupCatalog.FindPickupIndex(item.ItemDef.itemIndex));
                        //    ModLogger.LogWarning("Name" + item.ItemName);
                        //}
                        //if (item.ItemDef._itemTierDef== Highlander.instance.itemTierDef)
                        //{
                        //    Highlander.instance.ItemsWithThisTier.Add(item.ItemDef.itemIndex);
                        //    Highlander.instance.AvailableTierDropList.Add(PickupCatalog.FindPickupIndex(item.ItemDef.itemIndex));
                        //    ModLogger.LogWarning("Name" + item.ItemName);
                        //}
                    }
                }

                //IL.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += ItemBase.BlacklistFromPrinter;
                On.RoR2.Items.ContagiousItemManager.Init += ItemBase.RegisterVoidPairings;
            }
            var disableEquipment = ConfigManager.ConfigOptionValue<bool>("Equipment", "Disable All Equipment?", false, "Do you wish to disable every equipment in Augmentum?", true);
            if (!disableEquipment)
            {
                //Equipment Initialization
                var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

                ModLogger.LogInfo("-----------------EQUIPMENT---------------------");

                foreach (var equipmentType in EquipmentTypes)
                {
                    EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                    if (ValidateEquipment(equipment, Equipments))
                    {
                        equipment.Init(Config);
                        Config.SettingChanged += (object o, SettingChangedEventArgs args) => { equipment.EquipmentDef.descriptionToken = equipment.EquipmentFullDescriptionFormatted; };
                        ModLogger.LogInfo("Equipment: " + equipment.EquipmentName + " Initialized!");
                    }
                }
            }

            //Equipment Initialization
            var EliteEquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            ModLogger.LogInfo("-------------ELITE EQUIPMENT---------------------");

            foreach (var eliteEquipmentType in EliteEquipmentTypes)
            {
                EliteEquipmentBase eliteEquipment = (EliteEquipmentBase)System.Activator.CreateInstance(eliteEquipmentType);
                if (ValidateEliteEquipment(eliteEquipment, EliteEquipments))
                {
                    eliteEquipment.Init(Config);
                    Config.SettingChanged += (object o, SettingChangedEventArgs args) => { eliteEquipment.EliteEquipmentDef.descriptionToken = eliteEquipment.EliteEquipmentFullDescriptionFormatted; };
                    ModLogger.LogInfo("Elite Equipment: " + eliteEquipment.EliteEquipmentName + " Initialized!");
                }
            }

            //Artifact Initialization
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));

            ModLogger.LogInfo("-------------ARTIFACTS---------------------");

            foreach (var ArtifactType in ArtifactTypes)
            {
                ArtifactBase artifact = (ArtifactBase)System.Activator.CreateInstance(ArtifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);

                    ModLogger.LogInfo("Artifact: " + artifact.ArtifactName + " Initialized!");
                }
            }

            //Compatability
            ModLogger.LogInfo("-------------COMPATIBILITY---------------------");
            ValidateModCompatability();
            OptionPickupFix.Init();


        }

        private void ValidateModCompatability()
        {
            string defaultShareSuiteBlacklist= "ITEM_MINI_MATROYSHKA,ITEM_ABYSSAL_BEACON,ITEM_AUGMENTED_CONTACT,ITEM_CURVED_HORN,ITEM_GOAT_LEG,ITEM_MEDIUM_MATROYSHKA,ITEM_CHARM_OF_DESIRES,ITEM_MASSIVE_MATROYSHKA,ITEM_BLOODBURST_CLAM," +
                "ITEM_DISCOVERY_MEDALLION,ITEM_MEGA_MATROYSHKA";


            var enabledShareSuite = ConfigManager.ConfigOptionValue<bool>("Mod Compatability: " + "ShareSuite", "Enable Compatability Patches?", true, "Attempt to patch ShareSuite (if installed) to work with this mod?", true);
            var ShareSuiteBlackList= ConfigManager.ConfigOptionValue("Mod Compatability: " + "ShareSuite", "ShareSuite Blacklist", defaultShareSuiteBlacklist, "Add items to ShareSuite blacklist?",true);
            if (ModCompatability.ShareSuiteCompat.IsShareSuiteInstalled && enabledShareSuite)
            {
                ModLogger.LogInfo("ModCompatability: " + "ShareSuite Recognized!");

                ModCompatability.ShareSuiteCompat.AddTierToShareSuite();
                ModLogger.LogInfo("ModCompatability: " + "ShareSuite CoreTier added to Whitelist!");

                ModCompatability.ShareSuiteCompat.AddBransItemsBlacklist(ShareSuiteBlackList);
                ModLogger.LogInfo("ModCompatability: " + "ShareSuite Blacklist added to Whitelist!");
            }

            var enabledHIV = ConfigManager.ConfigOptionValue("Mod Compatability: " + "HighItemVizability", "Enable Compatability Patches?", true, "Attempt to patch HighItemVizability (if installed) to work with this mod?", true);
            if (ModCompatability.HighItemVizabilityCompat.IsHighItemVizabilityInstalled && enabledHIV)
            {
                ModLogger.LogInfo("ModCompatability: " + "HighItemVizability Recognized!");
            }

            var enabledProperSave = ConfigManager.ConfigOptionValue("Mod Compatability: " + "ProperSave", "Enable Compatability Patches?", true, "Attempt to add Propersave compatability (if installed)?", true);
            if (ModCompatability.ProperSaveCompat.IsProperSaveInstalled && enabledProperSave)
            {
                ModLogger.LogInfo("ModCompatability: " + "ProperSave Recognized!");
                ModCompatability.ProperSaveCompat.AddProperSaveFunctionality = true;
            }

            //var enabledZetAspects = ConfigManager.ConfigOption("Mod Compatability: " + "ZetAspects", "Enable Compatability Patches?", true, "Attempt to force ZetAspects compatability (if installed)?");
            //if (ModCompatability.ZetAspectsCompat.IsZetAspectsInstalled && enabledZetAspects)
            //{
            //    ModLogger.LogInfo("ModCompatability: " + "ZetAspects Recognized!");
            //    ModCompatability.ZetAspectsCompat.ForceZetAspectCompat();
            //}

            var enabledJudgement = ConfigManager.ConfigOptionValue("Mod Compatability: " + "Judgment", "Enable Compatability Patches?", true, "Attempt to add Judgment compatability (if installed)?", true);
            if (ModCompatability.JudgementCompat.IsJudgementInstalled && enabledJudgement)
            {
                ModLogger.LogInfo("ModCompatability: " + "Judgment Recognized!");
                ModCompatability.JudgementCompat.AddJudgementCompat = true;
            }


            //var enabledEliteReworks = Config.Bind<bool>("Mod Compatability: " + "EliteReworks", "Enable Compatability Patches?", true, "Attempt to add Elite Reworks compatability (if installed)?").Value;
            //if (ModCompatability.EliteReworksCompat.IsEliteReworksInstalled && enabledProperSave)
            //{
            //    ModLogger.LogInfo("ModCompatability: " + "EliteReworks!");
            //    ModCompatability.EliteReworksCompat.AddEliteReworksScaling = true;
            //}

            bool IfAnyLoaded = enabledShareSuite || enabledHIV || enabledProperSave;// || enabledEliteReworks;
            if(IfAnyLoaded)
            {
                ModCompatability.FinishedLoading();
            }


        }

        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = ConfigManager.ConfigOption<bool>("Item: " + item.ConfigItemName, "Enable Item?", true, "Should this item appear in runs?",true).Value;
            var aiBlacklist = ConfigManager.ConfigOptionValue<bool>("Item: " + item.ConfigItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?", false);
            var printerBlacklist = ConfigManager.ConfigOptionValue("Item: " + item.ConfigItemName, "Blacklist Item from Printers?", false, "Should the printers be able to print this item?", true);
            var requireUnlock = ConfigManager.ConfigOption<bool>("Item: " + item.ConfigItemName, "Require Unlock", true, "Should we require this item to be unlocked before it appears in runs? (Will only affect items with associated unlockables.)", true).Value;

            ItemStatusDictionary.Add(item, enabled);

            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
                if (printerBlacklist)
                {
                    item.PrinterBlacklisted = true;
                }

                //item.RequireUnlock = requireUnlock;
            }
            return enabled;
        }

        public bool ValidateBuff(BuffBase buff, List<BuffBase> buffList)
        {
            //var enabled = Config.Bind<bool>("Buff: " + buff.BuffName, "Enable Buff?", true, "Should this buff be registered for use in the game?").Value;

            BuffStatusDictionary.Add(buff, enabled);

            if (true)
            {
                buffList.Add(buff);
            }
            return enabled;
        }

        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            var enabled = ConfigManager.ConfigOption<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?", true).Value;

            EquipmentStatusDictionary.Add(equipment, enabled);

            if (enabled)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        public bool ValidateEliteEquipment(EliteEquipmentBase eliteEquipment, List<EliteEquipmentBase> eliteEquipmentList)
        {
            var enabled = ConfigManager.ConfigOption<bool>("Elite: " + eliteEquipment.EliteModifier, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.", true).Value;

            EliteEquipmentStatusDictionary.Add(eliteEquipment, enabled);

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
                return true;
            }
            return false;
        }

        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            var enabled = ConfigManager.ConfigOption<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear in the menu? If disabled, the associated artifact will not appear.", true).Value;

            ArtifactStatusDictionary.Add(artifact, enabled);

            if (enabled)
            {
                artifactList.Add(artifact);
                return true;
            }
            return false;
        }


        //SHARE SUITE ITEM LIST:ITEM_MINI_MATROYSHKA,ITEM_ABYSSAL_BEACON,ITEM_AUGMENTED_CONTACT,ITEM_CURVED_HORN,ITEM_GOAT_LEG,ITEM_MEDIUM_MATROYSHKA,ITEM_CHARM_OF_DESIRES,ITEM_MASSIVE_MATROYSHKA,ITEM_BLOODBURST_CLAM,ITEM_DISCOVERY_MEDALLION,ITEM_MEGA_MATROYSHKA
        //The Update() method is run on every frame of the game.
        private void Update()
        {
            if (false)
            {
                //This if statement checks if the player has currently pressed F2.
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    //Get the player body to use a position:
                    var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                    //And then drop our defined item in front of the player.

                    //Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(AirTotem.instance.EquipmentDef.equipmentIndex), transform.position, transform.forward * 20f);
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(EarthTotem.instance.EquipmentDef.equipmentIndex), transform.position, transform.forward * -20f);
                }

                if (Input.GetKeyDown(KeyCode.F3))
                {
                    //Get the player body to use a position:
                    var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                    //And then drop our defined item in front of the player.

                    //Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(EOAcuity.instance.ItemDef.itemIndex), transform.position, transform.forward * 20f);
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(AbyssalBeacon.instance.ItemDef.itemIndex), transform.position, transform.forward * -20f);
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(Modules.Pickups.Items.Tier1.MediumMatroyshka.instance.ItemDef.itemIndex), transform.position, transform.right * 20f);
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(Modules.Pickups.Items.Tier2.CharmOfDesires.instance.ItemDef.itemIndex), transform.position, transform.right * -20f);
                }
            }

        }
    }
}
