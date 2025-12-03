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
using Augmentum.Modules.Pickups.Items.CoreItems;
using Augmentum.Modules.Pickups.Items.Tier3;

namespace Augmentum.Modules.Pickups.Items.Tier1
{
    class MirrorShard : ItemBase<MirrorShard>
    {
        public override string ItemName => "Prototype 3D Printer";
        public override string ItemLangTokenName => "PRINTER_PROTOTYPE";
        public override string ItemPickupDesc => "Becomes a copy of a white item in your inventory";
        public override string ItemFullDescription => $"On pickup, becomes a copy of a white item in your inventory";

        public override string ItemLore => $"\"Hey boss, I have news about the prototype.\"" + "\n" +
            $"\"What is it? Did the output object not print correctly?\"" + "\n" +
            $"\"No, not that. Weirdly enough it actually printed perfectly. I sent you pictures of the two objects, they are nearly indistinguishable.\"" + "\n" +
            $"\"I don't understand then, what's the problem? Also, where is the printer? It's missing from the photo.\"" + "\n" +
            $"\"Yeah, that's the thing. You know how it requires an input material to print?\"" + "\n" +
            $"\"Yes?\"" + "\n" +
            $"\"I forgot to add some.\"" + "\n" +
            $"\"And?\"" + "\n" +
            $"\"It used itself as input material.\"" + "\n" +
            $"\"Oh.\"" + "\n" +
            $"\"Yeah.\"";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => SetupModel();

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Item/PrototypeScanner/PrototypeScanner.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.Utility };


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
        }

        private GameObject SetupModel()
        {
            GameObject ItemMesh = MainAssets.LoadAsset<GameObject>("Assets/Models/Meshes/P3SMesh.prefab");
            Material mat = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Duplicator/Duplicator.prefab").WaitForCompletion().GetComponentInChildren<SkinnedMeshRenderer>().GetMaterial();
            ItemMesh.GetComponentInChildren<MeshRenderer>().SetMaterial(mat);
            return ItemMesh;

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
            //On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            //On.RoR2.CharacterMaster.OnItemAddedClient += CharacterMaster_OnItemAddedClient;
            //TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            //CharacterBody.instancesList.
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            //On.RoR2.EquipmentSlot.OnEquipmentExecuted += EquipmentSlot_OnEquipmentExecuted;

        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if(self && self.isPlayerControlled && self.inventory && self.inventory.GetItemCount(ItemDef)>0 && self.inventory.GetTotalItemCountOfTier(ItemTier.Tier1)>self.inventory.GetItemCount(ItemDef))
            {
                List<ItemIndex> whiteItemsInInventory = new List<ItemIndex>();
                for(int i=0;i<self.inventory.itemAcquisitionOrder.Count;i++)
                {
                    if(ItemCatalog.tier1ItemList.Contains(self.inventory.itemAcquisitionOrder[i]) && self.inventory.itemAcquisitionOrder[i]!=ItemDef.itemIndex)
                    {
                        ItemIndex index = self.inventory.itemAcquisitionOrder[i];
                        for (int j = 0; j < self.inventory.effectiveItemStacks.GetStackValue(index);j++)
                            whiteItemsInInventory.Add(index);
                    }
                }

                TransformItem(self,(ItemIndex)ItemHelpers.GetRandomSelectionFromArray<ItemIndex>(whiteItemsInInventory, 1, RoR2Application.rng)[0]);
            }
        }

        private void TransformItem(CharacterBody body,ItemIndex index)
        {
            ModLogger.LogWarning("index" + index);
            body.inventory.RemoveItem(ItemDef);
            
            if (body.master)
            {
                //GenericPickupController.SendPickupMessage(body.master, PickupCatalog.FindPickupIndex(MassiveMatroyshka.instance.ItemDef.itemIndex));
                //CharacterMasterNotificationQueue.SendTransformNotification(body.master, MegaMatroyshka.instance.ItemDef.itemIndex, MegaMatroyshkaShells.instance.ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                CharacterMasterNotificationQueue.SendTransformNotification(body.master, ItemDef.itemIndex, index, CharacterMasterNotificationQueue.TransformationType.Default);
            }
            body.inventory.GiveItem(index);
        }
    }
}
