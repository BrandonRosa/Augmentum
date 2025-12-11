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
using static Augmentum.Modules.Pickups.Items.Essences.EssenceHelpers;
using UnityEngine.Networking;
using Augmentum.Modules.Pickups.Items.Essences;
using Augmentum.Modules.Pickups.Items.NoTier;
using Augmentum.Modules.Utils;
using UnityEngine.AddressableAssets;
using Augmentum.Modules.ItemTiers.CoreTier;
using Augmentum.Modules.Pickups.Items.Tier3;

namespace Augmentum.Modules.Pickups.Items.CoreItems
{
    class MiniMatroyshka : ItemBase<MiniMatroyshka>
    {
        public override string ItemName => "Mini Matroyshka";
        public override string ItemLangTokenName => "MINI_MATROYSHKA";
        public override string ItemPickupDesc => "The next time you open a chest, crack open for an " + Augmentum.EssenceKeyword + ".";
        public override string ItemFullDescription => $"On next <style=cisUtility>chest purchase</style>, cracks open for an "+Augmentum.EssenceKeyword+". \n<style=cMono>There is nothing left.</style>";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        public override ItemTierDef ModdedTierDef => Core.instance.itemTierDef;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/Matroyshka/Mini2.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/Matroyshka/MiniIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.CannotCopy };


        public static int DropCount=>DropCountEntry.Value;
        public static ConfigEntry<int> DropCountEntry;

        public static GameObject potentialPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();

        public static Dictionary<CharacterBody, int> MiniList = new Dictionary<CharacterBody, int>();

        public static List<string> whiteList = new List<string>() { "CategoryChestDamage", "CategoryChestHealing", "CategoryChestUtility", "Chest1", "Chest1Stealthed", "Chest2" };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            DropCountEntry = ConfigManager.ConfigOption<int>("Item: " + ItemName, "Number of essence items dropped", 1, "How many essence items should drop from this item?");
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
            //On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            //On.RoR2.CharacterMaster.OnItemAddedClient += CharacterMaster_OnItemAddedClient;
            //TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            //CharacterBody.instancesList.
            //On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            //On.RoR2.EquipmentSlot.OnEquipmentExecuted += EquipmentSlot_OnEquipmentExecuted;
            //On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
            //On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += PurchaseInteraction_CanBeAffordedByInteractor;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
            
        }

        private void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self, GameObject modelObjectOverride)
        {
            orig(self,modelObjectOverride);
            if (self && self.modelPrefab && self.modelObject && self.modelPrefab.name == ItemModel.name)
            {
                self.modelObject.transform.localScale *= .7f;
            }
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);

            //if (whiteList.Contains(self.name.Substring(0, self.name.Length - 7)))
            //{
            //    List<PlayerCharacterMasterController> masterList = new List<PlayerCharacterMasterController>(PlayerCharacterMasterController.instances);
            //    for (int i = 0; i < masterList.Count; i++)
            //    {
            //        //If the player isnt dead
            //        if (!masterList[i].master.IsDeadAndOutOfLivesServer())
            //        {
            //            //if the player has a body and an inventory AND they have the item
            //            if (masterList[i].body && masterList[i].body.inventory && masterList[i].body.inventory.GetItemCount(ItemDef) > 0 && activator.gameObject== masterList[i].body.gameObject)
            //            {
            //                int itemCount = masterList[i].body.inventory.GetItemCount(ItemDef);
            //                DropMini(masterList[i].body, itemCount*DropCount);
            //                //GiveTiny(body, MiniList[body]);
            //                BreakItem(masterList[i].body,itemCount);
            //                break;
            //            }
            //        }
            //    }
            //}
            if (whiteList.Contains(self.name.Substring(0, self.name.Length - 7)))
            {

                //if the player has a body and an inventory AND they have the the item
                CharacterBody body =activator.gameObject.GetComponent<CharacterBody>();
                if (body && body.inventory && body.inventory.GetItemCount(ItemDef) > 0 && body.isPlayerControlled)
                {
                    int itemCount = body.inventory.GetItemCount(ItemDef);
                    DropMini(body, itemCount * DropCount);
                    //GiveTiny(body, MiniList[body]);
                    BreakItem(body, itemCount);

                }
            }
        }


        private void DropMini(CharacterBody body, int count)
        {
            //Get Count of LootedBloodburst clams
            int MegaShellCount = body.inventory.GetItemCount(MegaMatroyshkaShells.instance.ItemDef.itemIndex);
            int DiscoveryMedallionCount = body.inventory.GetItemCount(DiscoveryMedallionConsumed.instance.ItemDef.itemIndex);
            int BloodBurstClamCount = body.inventory.GetItemCount(LootedBloodburstClam.instance.ItemDef.itemIndex);
            int WishOptions = 1 + MegaShellCount * MegaMatroyshka.AdditionalChoices + DiscoveryMedallionCount * DiscoveryMedallion.AdditionalChoices;
            bool isWish = MegaShellCount > 0;
            count += BloodBurstClamCount;

            float dropUpVelocityStrength = 25f;
            float dropForwardVelocityStrength = 5f;

            Transform dropTransform = body.transform;

            //Get array of loot
            //List<PickupIndex> dropList = EssenceHelpers.;
            

            float num = 360f / (float)count;
            if (count == 1)
                dropForwardVelocityStrength = 0f;



            Vector3 val = Vector3.up * dropUpVelocityStrength + dropTransform.forward * dropForwardVelocityStrength;
            Quaternion val2 = Quaternion.AngleAxis(num, Vector3.up);

            for (int i = 0; i < count; i++)
            {
                if (isWish)
                {
                    PickupIndex[] drops = EssenceHelpers.GetEssenceDropsWithoutRepeating(RoR2Application.rng, WishOptions);
                    PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
                    {
                        pickerOptions = PickupPickerController.GenerateOptionsFromArray(drops),
                        prefabOverride = potentialPrefab,
                        position = dropTransform.position + Vector3.up * 1.5f,
                        rotation = Quaternion.identity,
                        pickupIndex = drops[0]
                    },
                             dropTransform.position + Vector3.up * 1.5f, val);
                }
                else
                {
                    PickupIndex drop = EssenceHelpers.GetEssenceIndex(RoR2Application.rng);
                    PickupDropletController.CreatePickupDroplet(drop, dropTransform.position + Vector3.up * 1.5f, val);
                }
                val = val2 * val;
            }
        }

        private void BreakItem(CharacterBody self, int count)
        {
            self.inventory.RemoveItem(MiniMatroyshka.instance.ItemDef.itemIndex,count);

        }
    }
    
    
}
