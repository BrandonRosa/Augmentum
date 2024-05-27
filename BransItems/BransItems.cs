using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using R2API.Utils;
using System.Linq;
using BransItems.Modules.Pickups;
using System.Collections.Generic;
using BransItems.Modules.ItemTiers;
using RoR2.ContentManagement;
//using BransItems.Modules.ColorCatalogEntry;
using BransItems.Modules.ItemTiers.CoreTier;
using BransItems.Modules.ItemTiers.HighlanderTier;
using BransItems.Modules.Utils;
using BransItems.Modules.StandaloneBuffs;
using BransItems.Modules.ColorCatalogEntry;
using BransItems.Modules.Pickups.Equipments;
using BransItems.Modules.Compatability;
using BransItems.Modules.Pickups.Items.Essences;
using BransItems.Modules.Pickups.Items.HighlanderItems;
using BepInEx.Configuration;
using HarmonyLib;

namespace BransItems
{

    
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    [BepInPlugin(ModGuid, ModName, ModVer)]
    public class BransItems : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string ModGuid = "com.BrandonRosa.Augmentum"; //Our Package Name
        public const string ModName = "Augmentum";
        public const string ModVer = "1.0.4";


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
        //public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        public List<BuffBase> Buffs = new List<BuffBase>();
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public List<ItemTierBase> ItemTiers = new List<ItemTierBase>();
        public List<EliteEquipmentBase> EliteEquipments = new List<EliteEquipmentBase>();
       // public List<InteractableBase> Interactables = new List<InteractableBase>();
       // public List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public static HashSet<ItemDef> BlacklistedFromPrinter = new HashSet<ItemDef>();

        //public static ExpansionDef AetheriumExpansionDef = ScriptableObject.CreateInstance<ExpansionDef>();


        // For modders that seek to know whether or not one of the items or equipment are enabled for use in...I dunno, adding grip to Blaster Sword?
        //public static Dictionary<ArtifactBase, bool> ArtifactStatusDictionary = new Dictionary<ArtifactBase, bool>();
        //public static Dictionary<BuffBase, bool> BuffStatusDictionary = new Dictionary<BuffBase, bool>();
        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new Dictionary<ItemBase, bool>();
        public static Dictionary<EquipmentBase, bool> EquipmentStatusDictionary = new Dictionary<EquipmentBase, bool>();
        public static Dictionary<BuffBase, bool> BuffStatusDictionary = new Dictionary<BuffBase, bool>();
        public static Dictionary<EliteEquipmentBase, bool> EliteEquipmentStatusDictionary = new Dictionary<EliteEquipmentBase, bool>();
        //public static Dictionary<InteractableBase, bool> InteractableStatusDictionary = new Dictionary<InteractableBase, bool>();
        // public static Dictionary<SurvivorBase, bool> SurvivorStatusDictionary = new Dictionary<SurvivorBase, bool>();

        //public static ColorCatalog.ColorIndex TempCoreLight = ColorsAPI.RegisterColor(Color.cyan);//new Color32(21, 99, 58, 255));//ColorCatalogUtils.RegisterColor(new Color32(21, 99, 58, 255));
        //public static ColorCatalog.ColorIndex TempCoreDark = ColorsAPI.RegisterColor(Color.cyan); //new Color32(1, 126, 62, 255)); //ColorCatalogUtils.RegisterColor(new Color32(1, 126, 62, 255));
        public static string EssenceKeyword => "<color=#" + ColorCatalog.GetColorHexString(Colors.TempCoreLight) + ">Essence</color>";
        public static string EssencesKeyword => "<color=#" + ColorCatalog.GetColorHexString(Colors.TempCoreLight) + ">Essences</color>";

        public static string CoreColorString => "<color=#" + ColorCatalog.GetColorHexString(Colors.TempCoreLight) + ">";
        public void Awake()
        {
            ModLogger = this.Logger;

            AugBackupConfig = new(Paths.ConfigPath + "\\" + ModGuid + "." + ModName + ".Backup.cfg", true);
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

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BransItems.bransitems_assets"))
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

            var disableItems = ConfigManager.ConfigOption<bool>("Items", "Disable All Items?", false, "Do you wish to disable every item in BransItems?");
            if (!disableItems)
            {
                //Item Initialization
                var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

                ModLogger.LogInfo("----------------------ITEMS--------------------");

                foreach (var itemType in ItemTypes)
                {
                    ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                    if (ValidateItem(item, Items))
                    {
                        item.Init(Config);

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
            var disableEquipment = ConfigManager.ConfigOption<bool>("Equipment", "Disable All Equipment?", false, "Do you wish to disable every equipment in BransItems?");
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

                    ModLogger.LogInfo("Elite Equipment: " + eliteEquipment.EliteEquipmentName + " Initialized!");
                }
            }

            //Compatability
            ModLogger.LogInfo("-------------COMPATIBILITY---------------------");
            ValidateModCompatability();


        }

        private void ValidateModCompatability()
        {
            string defaultShareSuiteBlacklist= "ITEM_MINI_MATROYSHKA,ITEM_ABYSSAL_BEACON,ITEM_AUGMENTED_CONTACT,ITEM_CURVED_HORN,ITEM_GOAT_LEG,ITEM_MEDIUM_MATROYSHKA,ITEM_CHARM_OF_DESIRES,ITEM_MASSIVE_MATROYSHKA,ITEM_BLOODBURST_CLAM,ITEM_DISCOVERY_MEDALLION,ITEM_MEGA_MATROYSHKA";


            var enabledShareSuite = ConfigManager.ConfigOption<bool>("Mod Compatability: " + "ShareSuite", "Enable Compatability Patches?", true, "Attempt to patch ShareSuite (if installed) to work with this mod?");
            var ShareSuiteBlackList= ConfigManager.ConfigOption("Mod Compatability: " + "ShareSuite", "ShareSuite Blacklist", defaultShareSuiteBlacklist, "Add items to ShareSuite blacklist?");
            if (ModCompatability.ShareSuiteCompat.IsShareSuiteInstalled && enabledShareSuite)
            {
                ModLogger.LogInfo("ModCompatability: " + "ShareSuite Recognized!");

                ModCompatability.ShareSuiteCompat.AddTierToShareSuite();
                ModLogger.LogInfo("ModCompatability: " + "ShareSuite CoreTier added to Whitelist!");

                ModCompatability.ShareSuiteCompat.AddBransItemsBlacklist(ShareSuiteBlackList);
                ModLogger.LogInfo("ModCompatability: " + "ShareSuite Blacklist added to Whitelist!");
            }

            var enabledHIV = ConfigManager.ConfigOption("Mod Compatability: " + "HighItemVizability", "Enable Compatability Patches?", true, "Attempt to patch HighItemVizability (if installed) to work with this mod?");
            if (ModCompatability.HighItemVizabilityCompat.IsHighItemVizabilityInstalled && enabledHIV)
            {
                ModLogger.LogInfo("ModCompatability: " + "HighItemVizability Recognized!");
            }

            var enabledProperSave = ConfigManager.ConfigOption("Mod Compatability: " + "ProperSave", "Enable Compatability Patches?", true, "Attempt to add Propersave compatability (if installed)?");
            if (ModCompatability.ProperSaveCompat.IsProperSaveInstalled && enabledProperSave)
            {
                ModLogger.LogInfo("ModCompatability: " + "ProperSave Recognized!");
                ModCompatability.ProperSaveCompat.AddProperSaveFunctionality = true;
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
            var enabled = ConfigManager.ConfigOption<bool>("Item: " + item.ConfigItemName, "Enable Item?", true, "Should this item appear in runs?");
            var aiBlacklist = ConfigManager.ConfigOption<bool>("Item: " + item.ConfigItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?");
            var printerBlacklist = ConfigManager.ConfigOption("Item: " + item.ConfigItemName, "Blacklist Item from Printers?", false, "Should the printers be able to print this item?");
            var requireUnlock = ConfigManager.ConfigOption<bool>("Item: " + item.ConfigItemName, "Require Unlock", true, "Should we require this item to be unlocked before it appears in runs? (Will only affect items with associated unlockables.)");

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
            var enabled = ConfigManager.ConfigOption<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?");

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
            var enabled = ConfigManager.ConfigOption<bool>("Elite: " + eliteEquipment.EliteModifier, "Enable Elite Equipment?", true, "Should this elite equipment appear in runs? If disabled, the associated elite will not appear in runs either.");

            EliteEquipmentStatusDictionary.Add(eliteEquipment, enabled);

            if (enabled)
            {
                eliteEquipmentList.Add(eliteEquipment);
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
