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
using RoR2.ContentManagement;
using UnityEngine.AddressableAssets;
using Augmentum.Modules.Pickups.Items.CoreItems;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Augmentum.Modules.ColorCatalogEntry;
using UnityEngine.UI;
using System.Drawing;

namespace Augmentum.Modules.Pickups.Items.HighlanderItems
{
    class PuzzleBox : ItemBase<PuzzleBox>
    {
        public override string ItemName => "Pharaohs Puzzlebox";
        public override string ItemLangTokenName => "PUZZLEBOX";
        public override string ItemPickupDesc => "The first chest has a chance to have its tier upgraded. Gain a bit of semi-permanent luck when this happens.";
        public override string ItemFullDescriptionRaw =>
        @"Gain <style=cIsUtility>{0}% Luck</style>. The first chest opened each stage has a <style=cIsUtility>{1}%</style> chance upgrade its item. Gain {2}3 Clover Sprouts</color> if successful, {3}1</color> if not. When parting with the <color=#C8A200>Pharaoh</color>, <style=cDeath>split</style> your {4}Clovers</color>.";

        public override string ItemFullDescriptionFormatted =>
            string.Format(GetLangDesc(), LuckGain * 100f, UpgradeChance * 100f, Augmentum.CoreColorString, Augmentum.CoreColorString, Augmentum.CoreColorString);
        public override string ItemLore => "An ancient duelist from a forgotten time once was said to hold this. \nSome stories say he was a master strategist, who slayed dragons and held the world in the palm of his hands, others say he was simply lucky.. \n \n - Designed By paranoidhawklet \n - 3rd Place Winner of the 2024 Design Contest";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/PuzzleBox/pb_correct.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/PuzzleBox/puzzlebox.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public static float LuckGain=>LuckGainEntry.Value;
        public static float UpgradeChance=>UpgradeChanceEntry.Value;

        public static ConfigEntry<float> LuckGainEntry;
        public static ConfigEntry<float> UpgradeChanceEntry;

        public override void Init(ConfigFile config)
        {
            //ItemDef._itemTierDef = EssenceHelpers.essenceTierDef;
            CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateItem();
            SetLogbookCameraPosition();
            Hooks();

        }

        public void CreateConfig(ConfigFile config)
        {
            LuckGainEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Luck Gained", .35f, "How much luck should this give the player?");
            UpgradeChanceEntry = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Upgrade Chance", .25f, "Chance that the first chest has to give a higher tier item?");
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
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;

        }


        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            ChestBehavior chestBehavior = self.GetComponent<ChestBehavior>();
            
            if ((bool)chestBehavior)
            {
                
                CharacterMaster mast = activator.gameObject.GetComponent<CharacterBody>().master;
                if (mast?.inventory.GetItemCountEffective(ItemDef) > mast?.inventory.GetItemCountEffective(PuzzleBoxHiddenChestOpened.instance.ItemDef))
                {
                    
                    if (chestBehavior.currentPickup == UniquePickup.none) chestBehavior.Roll();

                    PickupDef pickupDef = PickupCatalog.GetPickupDef(chestBehavior.currentPickup.pickupIndex);
                    if (pickupDef != null)
                    {
                        ItemTier chestItemTier = pickupDef.itemTier;
                        ItemIndex chestItemIndex = pickupDef.itemIndex;
                        if (chestItemTier is ItemTier.Tier1 or ItemTier.Tier2 or ItemTier.Tier3)
                        {

                            bool upgrade = RoR2.Util.CheckRoll(UpgradeChance * 100f + (mast.inventory.GetItemCountEffective(ItemDef) - 1) * 10f, mast);
                            mast.inventory.GiveItemPermanent(PuzzleBoxHiddenChestOpened.instance.ItemDef);
                            if (upgrade)
                            {
                                PickupIndex newPI = PickupIndex.none;
                                switch (chestItemTier)
                                {
                                    case ItemTier.Tier1:
                                        newPI = ItemHelpers.GetRandomSelectionFromArray(Run.instance.availableTier2DropList, 1, Run.instance.runRNG)[0];
                                        chestBehavior.currentPickup = new UniquePickup(newPI);
                                        break;
                                    case ItemTier.Tier2:
                                        newPI = ItemHelpers.GetRandomSelectionFromArray(Run.instance.availableTier3DropList, 1, Run.instance.runRNG)[0];
                                        chestBehavior.currentPickup = new UniquePickup(newPI);
                                        break;
                                    case ItemTier.Tier3:
                                        newPI = ItemHelpers.GetRandomSelectionFromArray(Run.instance.availableBossDropList, 1, Run.instance.runRNG)[0];
                                        Drop_Yellow(new UniquePickup(newPI), chestBehavior);
                                        break;
                                }


                                AssetAsyncReferenceManager<GameObject>.LoadAsset(new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_Common_VFX.ShrineChanceDollUseEffect_prefab)).Completed += x =>
                                {
                                    EffectManager.SpawnEffect(x.Result, new EffectData
                                    {
                                        origin = self.transform.position,
                                        rotation = UnityEngine.Quaternion.identity,
                                        scale = 1f,
                                        color = UnityEngine.Color.yellow,
                                    }, transmit: true);
                                };

                                //EffectManager.SpawnEffect(effectPrefabShrineRewardJackpotVFX, new EffectData
                                //{
                                //    origin = base.transform.position,
                                //    rotation = Quaternion.identity,
                                //    scale = 1f,
                                //    color = colorShrineRewardJackpot
                                //}, transmit: true);

                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken = "<color=#C8A200><size=120%>" + "The heart of the cards responds! My path is clear." + "</color></size>"
                                });

                                mast.inventory.GiveItemPermanent(PuzzleBoxHiddenLuckGain.instance.ItemDef, 3);
                                mast.inventory.GiveItemPermanent(CloverSprout.instance.ItemDef, 3);

                                Chat.AddPickupMessage(mast.GetBody(), CloverSprout.instance.ItemDef.nameToken, ColorCatalog.GetColor(Colors.TempCoreLight), 3);

                                CharacterMasterNotificationQueue.PushItemTransformNotification(mast, instance.ItemDef.itemIndex, PickupCatalog.GetPickupDef(newPI).itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                                CharacterMasterNotificationQueue.PushItemTransformNotification(mast, instance.ItemDef.itemIndex, CloverSprout.instance.ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                            }
                            else
                            {
                                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                                {
                                    baseToken = "<color=#C8A200><size=120%>" + "A tough draw, but the duel isn’t over." + "</color></size>"
                                });
                                mast.inventory.GiveItemPermanent(CloverSprout.instance.ItemDef, 1);
                                mast.inventory.GiveItemPermanent(PuzzleBoxHiddenLuckGain.instance.ItemDef, 1);

                                Chat.AddPickupMessage(mast.GetBody(), CloverSprout.instance.ItemDef.nameToken, ColorCatalog.GetColor(Colors.TempCoreLight), 1);
                                CharacterMasterNotificationQueue.PushItemTransformNotification(mast, instance.ItemDef.itemIndex, CloverSprout.instance.ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);

                            }
                        }
                    }
                }
            }
            orig(self, activator);
        }

        private void Drop_Yellow(UniquePickup uniquePickup, ChestBehavior chestBehavior)
        {
            Vector3 velocity = (Vector3.up * chestBehavior.dropUpVelocityStrength + chestBehavior.dropTransform.forward * chestBehavior.dropForwardVelocityStrength)*.9f;
            GenericPickupController.CreatePickupInfo createPickupInfo = default(GenericPickupController.CreatePickupInfo);
            createPickupInfo.pickup = uniquePickup;
            createPickupInfo.position = chestBehavior.dropTransform.position + Vector3.up * 1.5f;
            createPickupInfo.chest = chestBehavior;
            createPickupInfo.artifactFlag = (chestBehavior.isCommandChest ? GenericPickupController.PickupArtifactFlag.COMMAND : GenericPickupController.PickupArtifactFlag.NONE);
            GenericPickupController.CreatePickupInfo pickupInfo = createPickupInfo;
            PickupDropletController.CreatePickupDroplet(pickupInfo, pickupInfo.position, velocity);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.luckAdd += LuckGain * GetCount(sender);
        }
    }

    class PuzzleBoxHiddenLuckGain : ItemBase<PuzzleBoxHiddenLuckGain>
    {
        public override string ItemName => "HiddenLuckGain";

        public override string ItemLangTokenName => "HIDDENLUCKGAIN";

        public override string ItemPickupDesc => "";

        public override string ItemFullDescriptionRaw => "";

        public override string ItemFullDescriptionFormatted => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override bool Hidden => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
        }

        private void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            int count = self.inventory.GetItemCountEffective(instance.ItemDef);
            bool hasPuzzleBox = self.inventory.GetItemCountEffective(PuzzleBox.instance.ItemDef) > 0;
            if (!hasPuzzleBox && count > 0)
            {
                int toRemove = count / 2;
                if (toRemove > 0)
                {
                    self.inventory.RemoveItemPermanent(CloverSprout.instance.ItemDef, toRemove);
                   
                    CharacterMasterNotificationQueue.PushItemTransformNotification(self, CloverSprout.instance.ItemDef.itemIndex, PuzzleBox.instance.ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Suppressed);
                }

                self.inventory.RemoveItemPermanent(instance.ItemDef, count);


                ItemHelpers.DelayChatMessage("<color=#C8A200><size=120%>" + "Goodbye then.</color></size>", 1f);
                ItemHelpers.DelayChatMessage("<color=#C8A200><size=120%>" + "I will take my share and leave you " + Mathf.Max((count - toRemove), 0) + "." + "</color></size>", 1.2f);
                //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                //{
                //    baseToken = "<color=#C8A200><size=120%>" + "Goodbye then.</color></size>"
                //});
                
                //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                //{
                //    baseToken = "<color=#C8A200><size=120%>" + "I will take my share and leave you " + Mathf.Max((count - toRemove), 0) + "." + " </ color ></ size > "
                //});
            }
        }

        public override void Init(ConfigFile config)
        {
            //ItemDef._itemTierDef = EssenceHelpers.essenceTierDef;
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();

        }
    }

    class PuzzleBoxHiddenChestOpened: ItemBase<PuzzleBoxHiddenChestOpened>
    {
        public override string ItemName => "HiddenPuzzleBoxOpened";

        public override string ItemLangTokenName => "HIDDENPUZZLEBOXOPENED";

        public override string ItemPickupDesc => "";

        public override string ItemFullDescriptionRaw => "";

        public override string ItemFullDescriptionFormatted => "";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.NoTier;

        public override bool Hidden => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            return rules;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
        }

        private void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            int count = self.inventory.GetItemCountEffective(instance.ItemDef);
            bool hasPuzzleBox = self.inventory.GetItemCountEffective(PuzzleBox.instance.ItemDef) > 0;
            if (count > 0)
            {
                if (hasPuzzleBox)
                {
                    //Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    //{
                    //    baseToken = "<color=#C8A200><size=120%>" + "I stand ready. Show me the strength of your spirit!</color></size>"
                    //});
                    ItemHelpers.DelayChatMessage("<color=#C8A200><size=120%>" + "I stand ready. Show me the strength of your spirit!</color></size>", 1f);
                    CharacterMasterNotificationQueue.PushPickupNotification(self, PickupCatalog.FindPickupIndex(PuzzleBox.instance.ItemDef.itemIndex), false);
                }

                self.inventory.RemoveItemPermanent(instance.ItemDef, count);
                
            }
        }

        public override void Init(ConfigFile config)
        {
            //ItemDef._itemTierDef = EssenceHelpers.essenceTierDef;
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();

        }
    }
}
