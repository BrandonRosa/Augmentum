using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using R2API;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Augmentum.Modules.ItemTiers.CoreTier;
using Augmentum.Modules.ItemTiers;
using Augmentum.Modules.ColorCatalogEntry;
using Augmentum.Modules.ItemTiers.HighlanderTier;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Augmentum.Modules.Pickups.EliteEquipments;
using System.Runtime.Serialization;
using Augmentum.Modules.Pickups.Equipments;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;

namespace Augmentum.Modules.Compatability
{
    internal static partial class ModCompatability
    {
        internal static class ShareSuiteCompat
        {
            public static bool IsShareSuiteInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.funkfrog_sipondo.sharesuite");

            public static string CustomBlacklist = null;

            public static Hook CoreValidMethodHook;
            public static Hook BransItemsBlacklistHook;

            // Define a delegate type that matches the signature of IsValidItemPickup
            public delegate bool IsValidItemPickupDelegate(PickupIndex pickup);
            public delegate void LoadBlacklistDelegate();
            public static void AddTierToShareSuite()
            {
                //// Get a reference to the original method
                MethodInfo originalMethod = typeof(ShareSuite.ItemSharingHooks).GetMethod("IsValidItemPickup");
                //
                //// Create a delegate to the original method
                IsValidItemPickupDelegate origDelegate = (IsValidItemPickupDelegate)Delegate.CreateDelegate(typeof(IsValidItemPickupDelegate), originalMethod);
                
                CoreValidMethodHook = new Hook(typeof(ShareSuite.ItemSharingHooks).GetMethod("IsValidItemPickup", (System.Reflection.BindingFlags)(-1)), typeof(ModCompatability.ShareSuiteCompat).GetMethod("CoreIsValidPickup"));
            }

            public static bool CoreIsValidPickup(IsValidItemPickupDelegate orig , PickupIndex pickup)
            {
                //Augmentum.ModLogger.LogWarning("IT WORKS");
                bool ans = orig(pickup);
                if (!ans)
                {
                    var pickupDef = PickupCatalog.GetPickupDef(pickup);
                    if (pickupDef != null && pickupDef.itemIndex != ItemIndex.None)
                    {
                        var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                        if (itemDef._itemTierDef == Core.instance.itemTierDef)
                            return true;
                    }
                }
                return ans;
            }

            public static void AddBransItemsBlacklist(string customBlacklist)
            {
                CustomBlacklist = customBlacklist;

                MethodInfo originalMethod = typeof(ShareSuite.Blacklist).GetMethod("LoadBlackListItems", BindingFlags.NonPublic | BindingFlags.Static);

                LoadBlacklistDelegate origDelegate = (LoadBlacklistDelegate)Delegate.CreateDelegate(typeof(LoadBlacklistDelegate), originalMethod);

                BransItemsBlacklistHook = new Hook(typeof(ShareSuite.Blacklist).GetMethod("LoadBlackListItems", (System.Reflection.BindingFlags)(-1)), typeof(ModCompatability.ShareSuiteCompat).GetMethod("LoadBlackListBransItems"));
            }

            public static void LoadBlackListBransItems(LoadBlacklistDelegate orig)
            {
                string oldValue = ShareSuite.ShareSuite.ItemBlacklist.Value;
                ShareSuite.ShareSuite.ItemBlacklist.Value += "," + CustomBlacklist;
                orig();
                ShareSuite.ShareSuite.ItemBlacklist.Value = oldValue;
            }


        }
        internal static class BiggerBazaarCompat
        {
            public const string GUID = "com.MagnusMagnuson.BiggerBazaar";
            public static bool IsBiggerBazaarInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);

            private static bool _lookupDone;
            private static FieldInfo _bazaarField;
            private static MethodInfo _isDisplayItemMethod;

            /// <summary>
            /// BiggerBazaar spawns real GenericPickups above its chests purely for display. Those must never be consumed by our equipment.
            /// Its Bazaar class is internal, so this goes through reflection instead of a hard reference.
            /// </summary>
            public static bool IsDisplayItem(GameObject gameObject)
            {
                if (!gameObject || !IsBiggerBazaarInstalled)
                    return false;

                try
                {
                    var plugin = BepInEx.Bootstrap.Chainloader.PluginInfos[GUID].Instance;
                    if (!plugin)
                        return false;

                    if (!_lookupDone)
                    {
                        _lookupDone = true;
                        _bazaarField = plugin.GetType().GetField("bazaar", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        _isDisplayItemMethod = _bazaarField?.FieldType.GetMethod("IsDisplayItem", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(GameObject) }, null);
                        if (_isDisplayItemMethod == null)
                            Augmentum.ModLogger.LogWarning("ModCompatability: Could not find BiggerBazaar's display item check, display items may be targetable.");
                    }

                    if (_isDisplayItemMethod == null)
                        return false;

                    var bazaar = _bazaarField.GetValue(plugin);
                    return bazaar != null && (bool)_isDisplayItemMethod.Invoke(bazaar, new object[] { gameObject });
                }
                catch (Exception e)
                {
                    Augmentum.ModLogger.LogWarning("ModCompatability: BiggerBazaar display item check failed: " + e.Message);
                    _isDisplayItemMethod = null;
                    return false;
                }
            }
        }

        internal static class ProperSaveCompat
        {
            public static bool IsProperSaveInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");

            public static bool AddProperSaveFunctionality = false;

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void Hooks()
            {
                ProperSave.SaveFile.OnGatherSaveData += SaveFile_OnGatherSaveData;
                ProperSave.Loading.OnLoadingEnded += Loading_OnLoadingStarted;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void Loading_OnLoadingStarted(ProperSave.SaveFile obj)
            {
                string ETTDictKey = "BransExpansion_EarthTotemTrackers";

                List<EarthTotemTrackerSaveStructure> ETTSStructures = obj.GetModdedData<List<EarthTotemTrackerSaveStructure>>(ETTDictKey);
                if (ETTSStructures == null)
                    return;

                foreach (EarthTotemTrackerSaveStructure ETTS in ETTSStructures)
                {
                    if (ETTS.userID == null)
                        continue;
                    NetworkUserId NUID = ETTS.userID.Load();
                    CharacterMaster master = NetworkUser.readOnlyInstancesList.FirstOrDefault(Nuser => Nuser && Nuser.id.Equals(NUID))?.master;
                    if (!master)
                        continue;

                    EarthTotemTracker temp = EarthTotem.GetOrAddTracker(master);
                    temp.EquipDefList.Clear();
                    temp.HighestCooldown = 0;
                    // Equipment from mods that are no longer installed can't be found and is skipped.
                    foreach (string equip in ETTS.EquipList ?? new List<string>())
                        temp.AddToList(EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(equip)));
                    temp.EarthTotemAbsorbedCount = ETTS.EarthTotemsAbsorbed;
                }
            }
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void SaveFile_OnGatherSaveData(Dictionary<string, object> obj)
            {
                //string will be : BransExpansion_EarthTotemTrackers
                //object will be string with this:
                //Identifier for the player
                //Number of absorbed earthTotems
                //Number of absorbed equipments
                //List of equipment names
                //^^ all that in each line
                //At the end of the string write ENDOFLOADINGEARTHTOTEM

                string ETTDictKey = "BransExpansion_EarthTotemTrackers";

                //List<CharacterMaster> MastersWithEquipment = CharacterMaster.instancesList.Where(master => master.GetComponent<EarthTotemTracker>() != null).ToList();
                List<EarthTotemTracker> earthTotemTrackers = CharacterMaster.instancesList
                .Select(master => master.GetComponent<EarthTotemTracker>())
                .Where(tracker => tracker != null)
                .ToList();

                //List<string> ETTEntries = new List<string>();
                List<EarthTotemTrackerSaveStructure> ETTSSList = new List<EarthTotemTrackerSaveStructure>();
                foreach (EarthTotemTracker ETT in earthTotemTrackers)
                {
                    // Only players can be matched back up on load.
                    NetworkUser networkUser = ETT.Master && ETT.Master.playerCharacterMasterController ? ETT.Master.playerCharacterMasterController.networkUser : null;
                    if (!networkUser)
                        continue;

                    List<string> ETTEquipList = new List<string>();
                    foreach (EquipmentDef ED in ETT.EquipDefList)
                        if (ED)
                            ETTEquipList.Add(ED.name);

                    ETTSSList.Add(new EarthTotemTrackerSaveStructure
                    {
                        userID = ProperSave.Data.UserIDData.Create(networkUser.id),
                        EarthTotemsAbsorbed = ETT.EarthTotemAbsorbedCount,
                        EquipList = ETTEquipList
                    });
                }

                obj.Add(ETTDictKey, ETTSSList);
            }

            /// <summary>
            /// This in charge of saving/loading absorbed equipments.
            /// </summary>
            public struct EarthTotemTrackerSaveStructure
            {
                [DataMember(Name = "UserID")]
                public ProperSave.Data.UserIDData userID;
                [DataMember(Name = "EarthTotemsAbsorbed")]
                public int EarthTotemsAbsorbed;
                [DataMember(Name = "AbsorbList")]
                public List<string> EquipList;


            }
        }

        public static class RiskOfOptionsCompatability
        {
            public const string GUID = "com.rune580.riskofoptions";
            public static bool IsShareSuiteInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);

            public static void AddConfig<T>(T config,bool requiresRestart) where T : ConfigEntryBase
            {
                BaseOption option = null;
                if (config is ConfigEntry<float>)
                {
                    option = new FloatFieldOption(config as ConfigEntry<float>,requiresRestart);
                }
                else if (config is ConfigEntry<bool>)
                {
                    option = new CheckBoxOption(config as ConfigEntry<bool>,requiresRestart);
                }
                else if (config is ConfigEntry<int>)
                {
                    option = new IntFieldOption(config as ConfigEntry<int>,requiresRestart);
                }
                else if (config is ConfigEntry<string>)
                {
                    option = new StringInputFieldOption(config as ConfigEntry<string>, requiresRestart);
                }
                else
                {
                    option=new RiskOfOptions.Options.ChoiceOption(config,requiresRestart);
                }
                ModSettingsManager.AddOption(option);
                return;
            }
                
            
        }

        public static event Action FinishedLoadingCompatability;

        public static void FinishedLoading()
        {
            FinishedLoadingCompatability?.Invoke();
        }
    }

    
}