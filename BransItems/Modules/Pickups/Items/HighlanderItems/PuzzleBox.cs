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

namespace Augmentum.Modules.Pickups.Items.HighlanderItems
{
    class PuzzleBox : ItemBase<PuzzleBox>
    {
        public override string ItemName => "Pharaohs Puzzlebox";
        public override string ItemLangTokenName => "PUZZLEBOX";
        public override string ItemPickupDesc => "Slightly Increase Luck. First chest item of every stage has a chance to be upgraded.";
        public override string ItemFullDescription => $"Slightly Increase Luck. First chest item of every stage has a chance to be upgraded.";

        public override string ItemLore => "An ancient duelist from a forgotten time once was said to hold this. Some stories say he was a master strategist, who slayed dragons and held the world in the palm of his hands, others say he was simply lucky";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/PuzzleBox/puzzleboxItem.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/PuzzleBox/puzzlebox.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Damage };

        public static float LuckGain;
        public static float UpgradeChance;

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
            LuckGain = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Luck Gained", .4f, "How much luck should this give the player?");
            UpgradeChance = ConfigManager.ConfigOption<float>("Item: " + ItemName, "Upgrade Chance", .25f, "Chance that the first chest has to give a higher tier item?");
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
            Augmentum.ModLogger.LogWarning("PB Flag0");
            if ((bool)chestBehavior)
            {
                Augmentum.ModLogger.LogWarning("PB Flag1");
                CharacterMaster mast = activator.gameObject.GetComponent<CharacterBody>().master;
                if (mast?.inventory.GetItemCountEffective(ItemDef) > mast?.inventory.GetItemCountEffective(PuzzleBoxHiddenChestOpened.instance.ItemDef))
                {
                    Augmentum.ModLogger.LogWarning("PB Flag2");
                    if (chestBehavior.currentPickup == UniquePickup.none) chestBehavior.Roll();
                    Augmentum.ModLogger.LogWarning("PB2.1 "+chestBehavior.currentPickup.pickupIndex);
                    Augmentum.ModLogger.LogWarning("PB2.2 " + PickupCatalog.GetPickupDef(chestBehavior.currentPickup.pickupIndex).itemIndex);
                    Augmentum.ModLogger.LogWarning("PB2.3 " + PickupCatalog.GetPickupDef(chestBehavior.currentPickup.pickupIndex).itemTier);
                    ItemTier chestItemTier = PickupCatalog.GetPickupDef(chestBehavior.currentPickup.pickupIndex).itemTier;
                    if (chestItemTier is ItemTier.Tier1 or ItemTier.Tier2 or ItemTier.Tier3)
                    {
                        Augmentum.ModLogger.LogWarning("PB Flag3");
                        bool upgrade=RoR2.Util.CheckRoll(UpgradeChance*100f+ (mast.inventory.GetItemCountEffective(ItemDef)-1)*10f, mast);
                        mast.inventory.GiveItemPermanent(PuzzleBoxHiddenChestOpened.instance.ItemDef);
                        if(upgrade)
                        {
                            PickupIndex newPI = PickupIndex.none;
                            switch(chestItemTier)
                            {
                                case ItemTier.Tier1:
                                    newPI = ItemHelpers.GetRandomSelectionFromArray(Run.instance.availableTier2DropList,1,Run.instance.runRNG)[0];
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
                            
                            mast.inventory.GiveItemPermanent(PuzzleBoxHiddenLuckGain.instance.ItemDef);
                            mast.inventory.GiveItemPermanent(CloverSprout.instance.ItemDef,2);
                            AssetAsyncReferenceManager<GameObject>.LoadAsset(new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_Common_VFX.ShrineChanceDollUseEffect_prefab)).Completed += x => 
                            {
                                EffectManager.SpawnEffect(x.Result, new EffectData
                                {
                                    origin = self.transform.position,
                                    rotation = UnityEngine.Quaternion.identity,
                                    scale = 1f,
                                    color = Color.yellow,
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
                                baseToken = "<color=#FAF7B9><size=120%>" + "The heart of the cards responds! My path is clear." + "</color></size>"
                            });
                        }
                        else
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = "<color=#FAF7B9><size=120%>" + "A tough draw, but the duel isn’t over." + "</color></size>"
                            });
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

        public override string ItemFullDescription => "";

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

        public override string ItemFullDescription => "";

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
