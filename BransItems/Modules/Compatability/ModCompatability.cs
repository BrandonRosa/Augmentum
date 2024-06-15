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
using BransItems.Modules.ItemTiers.CoreTier;
using BransItems.Modules.ItemTiers;
using BransItems.Modules.ColorCatalogEntry;
using BransItems.Modules.ItemTiers.HighlanderTier;
using System.Reflection;
using MonoMod.RuntimeDetour;
using BransItems.Modules.Pickups.EliteEquipments;

namespace BransItems.Modules.Compatability
{
    internal static class ModCompatability
    {
        /*
        internal static class BetterUICompat
        {
            public static bool IsBetterUIInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI");

            public static Tuple<string, string> CreateBetterUIBuffInformation(string langTokenName, string name, string description, bool isBuff = true)
            {
                string nameToken = isBuff ? $"BUFF_{langTokenName}_NAME" : $"DEBUFF_{langTokenName}_NAME";
                string descToken = isBuff ? $"BUFF_{langTokenName}_DESC" : $"DEBUFF_{langTokenName}_DESC";

                LanguageAPI.Add(nameToken, name);
                LanguageAPI.Add(descToken, description);

                return Tuple.Create(nameToken, descToken);
            }

            public static void RegisterBuffInfo(BuffDef buffDef, string nameToken, string descriptionToken)
            {
                BetterUI.Buffs.RegisterBuffInfo(buffDef, nameToken, descriptionToken);
            }

            public static void CreateAccursedPotionStatDef()
            {
                BetterUI.ItemStats.ItemStat SipCooldownStatDef = new BetterUI.ItemStats.ItemStat
                {
                    nameToken = "AETHERIUM_ITEM_STAT_DEF_ACCURSED_POTION_SIP"
                };
            }
        }

        */

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
                //BransItems.ModLogger.LogWarning("IT WORKS");
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
        internal static class HighItemVizabilityCompat
        {
            public static bool IsHighItemVizabilityInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("VizMod.HighItemVizbility");

            public static Hook CustomTiersVizLines;

        }

        internal static class ProperSaveCompat
        {
            public static bool IsProperSaveInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");

            public static bool AddProperSaveFunctionality = false;


        }

        internal static class EliteReworksCompat
        {
            public static bool IsEliteReworksInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Moffein.EliteReworks");

            public static bool AddEliteReworksScaling = true;


        }

        internal static class ZetAspectsCompat
        {
            public static bool IsZetAspectsInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.TPDespair.ZetAspects");

            public static void ForceZetAspectCompat()
            {
                ZetAdaptiveDrop.instance.Init(BransItems.AugConfig);

                BransItems.ModLogger.LogInfo("Item: " + ZetAdaptiveDrop.instance.ItemName + " Initialized!");

                On.RoR2.PickupDropletController.CreatePickupDroplet_PickupIndex_Vector3_Vector3 += (orig, pickupIndex, position, velocity) =>
                {
                    if (!TPDespair.ZetAspects.DropHooks.CanObtainEquipment())
                    {
                        EquipmentIndex equipIndex = PickupCatalog.GetPickupDef(pickupIndex).equipmentIndex;

                        if (equipIndex != EquipmentIndex.None && equipIndex==AffixAdaptive.instance.EliteEquipmentDef.equipmentIndex)
                        {
                            ItemIndex newIndex = ZetAdaptiveDrop.instance.ItemDef.itemIndex;

                            if (newIndex != ItemIndex.None) pickupIndex = PickupCatalog.FindPickupIndex(newIndex);
                        }
                    }

                    orig(pickupIndex, position, velocity);
                };
            }
        }

        public static event Action FinishedLoadingCompatability;

        public static void FinishedLoading()
        {
            FinishedLoadingCompatability.Invoke();
        }
    }

    
}