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
using BransItems.Modules.ColorCatalogEntry;
using BransItems.Modules.ItemTiers.CoreTier;
using BransItems.Modules.Utils;

namespace BransItems
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    //[BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    //[BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(PrefabAPI), nameof(RecalculateStatsAPI), nameof(ColorsAPI))]//nameof(BuffAPI), nameof(ResourcesAPI), nameof(EffectAPI), nameof(ProjectileAPI), nameof(ArtifactAPI), nameof(LoadoutAPI),   
                             // nameof(PrefabAPI), nameof(SoundAPI), nameof(OrbAPI),
                             // nameof(NetworkingAPI), nameof(DirectorAPI), nameof(RecalculateStatsAPI), nameof(UnlockableAPI), nameof(EliteAPI),
                             // nameof(CommandHelper), nameof(DamageAPI))]


    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class BransItems : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string ModGuid = "com.BrandonRosa.BransItems"; //Our Package Name
        public const string ModName = "BransItems";
        public const string ModVer = "0.4.0";


        internal static BepInEx.Logging.ManualLogSource ModLogger;


        //We need our item definition to persist through our functions, and therefore make it a class field.
        //private static ItemDef myItemDef;

        //The Awake() method is run at the very start when the game is initialized.
        public static AssetBundle MainAssets;

        //public List<CoreModule> CoreModules = new List<CoreModule>();
        //public List<ArtifactBase> Artifacts = new List<ArtifactBase>();
        //public List<BuffBase> Buffs = new List<BuffBase>();
        public List<ItemBase> Items = new List<ItemBase>();
        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public List<ItemTierBase> ItemTiers = new List<ItemTierBase>();
        //public List<EliteEquipmentBase> EliteEquipments = new List<EliteEquipmentBase>();
       // public List<InteractableBase> Interactables = new List<InteractableBase>();
       // public List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public static HashSet<ItemDef> BlacklistedFromPrinter = new HashSet<ItemDef>();

        //public static ExpansionDef AetheriumExpansionDef = ScriptableObject.CreateInstance<ExpansionDef>();


        // For modders that seek to know whether or not one of the items or equipment are enabled for use in...I dunno, adding grip to Blaster Sword?
        //public static Dictionary<ArtifactBase, bool> ArtifactStatusDictionary = new Dictionary<ArtifactBase, bool>();
        //public static Dictionary<BuffBase, bool> BuffStatusDictionary = new Dictionary<BuffBase, bool>();
        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new Dictionary<ItemBase, bool>();
        public static Dictionary<EquipmentBase, bool> EquipmentStatusDictionary = new Dictionary<EquipmentBase, bool>();
        // public static Dictionary<EliteEquipmentBase, bool> EliteEquipmentStatusDictionary = new Dictionary<EliteEquipmentBase, bool>();
        //public static Dictionary<InteractableBase, bool> InteractableStatusDictionary = new Dictionary<InteractableBase, bool>();
        // public static Dictionary<SurvivorBase, bool> SurvivorStatusDictionary = new Dictionary<SurvivorBase, bool>();

        public static ColorCatalog.ColorIndex TempCoreLight = ColorCatalogUtils.RegisterColor(new Color32(21, 99, 58, 255));
        public static ColorCatalog.ColorIndex TempCoreDark = ColorCatalogUtils.RegisterColor(new Color32(1, 126, 62, 255));

        public void Awake()
        {
            ModLogger = this.Logger;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BransItems.bransitems_assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

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
                    if (true)//ValidateSurvivor(itemtier, Survivors))
                    {
                        itemtier.Init();

                        ModLogger.LogInfo("ItemTier: " + itemtier.TierName + " Initialized!");
                    }
                }
            }

            var disableItems = Config.Bind<bool>("Items", "Disable All Items?", false, "Do you wish to disable every item in BransItems?");
            if (!disableItems.Value)
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

                        if(item.ItemDef==Core.instance.itemTierDef)
                        {
                            Core.instance.ItemsWithThisTier.Add(item.ItemDef.itemIndex);
                            Core.instance.AvailableTierDropList.Add(PickupCatalog.FindPickupIndex(item.ItemDef.itemIndex));
                        }
                    }
                }

                //IL.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += ItemBase.BlacklistFromPrinter;
                //On.RoR2.Items.ContagiousItemManager.Init += ItemBase.RegisterVoidPairings;
            }
            var disableEquipment = Config.Bind<bool>("Equipment", "Disable All Equipment?", false, "Do you wish to disable every equipment in BransItems?");
            if (!disableEquipment.Value)
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

            

            
        }

        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.ItemName, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            var printerBlacklist = Config.Bind<bool>("Item: " + item.ItemName, "Blacklist Item from Printers?", false, "Should the printers be able to print this item?").Value;
            var requireUnlock = Config.Bind<bool>("Item: " + item.ItemName, "Require Unlock", true, "Should we require this item to be unlocked before it appears in runs? (Will only affect items with associated unlockables.)").Value;

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

        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            var enabled = Config.Bind<bool>("Equipment: " + equipment.EquipmentName, "Enable Equipment?", true, "Should this equipment appear in runs?").Value;

            EquipmentStatusDictionary.Add(equipment, enabled);

            if (enabled)
            {
                equipmentList.Add(equipment);
                return true;
            }
            return false;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            /*
            //If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody)
            {
                return;
            }

            var attackerCharacterBody = report.attackerBody;

            //We need an inventory to do check for our item
            if (attackerCharacterBody.inventory)
            {
                //store the amount of our item we have
                var garbCount = attackerCharacterBody.inventory.GetItemCount(myItemDef.itemIndex);
                if (garbCount > 0 &&
                    //Roll for our 50% chance.
                    Util.CheckRoll(50, attackerCharacterBody.master))
                {
                    //Since we passed all checks, we now give our attacker the cloaked buff.
                    //Note how we are scaling the buff duration depending on the number of the custom item in our inventory.
                    attackerCharacterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, 3 + garbCount);
                }
            }
            */
        }

        //The Update() method is run on every frame of the game.
        private void Update()
        {
            /*
            //This if statement checks if the player has currently pressed F2.
            if (Input.GetKeyDown(KeyCode.F2))
            {
                //Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then drop our defined item in front of the player.

                Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(myItemDef.itemIndex), transform.position, transform.forward * 20f);
            }
            */
        }
    }
}
