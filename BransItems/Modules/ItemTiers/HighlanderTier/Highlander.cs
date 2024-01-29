using System;
using System.Collections.Generic;
using System.Text;
using BransItems.Modules.ColorCatalogEntry;
using BransItems.Modules.ColorCatalogEntry.CoreColors;
using BransItems.Modules.ItemTiers;
using BransItems.Modules.Pickups.Items.Essences;
using BransItems.Modules.Utils;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static BransItems.BransItems;

namespace BransItems.Modules.ItemTiers.HighlanderTier
{
    class Highlander : ItemTierBase<Highlander>
    {
        //public override ItemTierDef itemTierDef = ScriptableObject.CreateInstance<ItemTierDef>(); // new ItemTierDef();

        //public override GameObject PickupDisplayVFX => throw new NotImplementedException();

        //public override GameObject highlightPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();

        //public override GameObject dropletDisplayPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();

        public override bool canRestack => false;

        public override bool canScrap => false;

        public override bool isDroppable => true;

        public override string TierName => "Highlander";

        public static float CompatShrineChance = .20f;

        public static bool CombatShrineScalePlayers = true;

        public static float BarrelChance = .025f;

        public static bool BarrelScalePlayers = true;

        public static float EliteDeathChance = .01f;

        public static bool EliteScalePlayers = true;

        public static float ChanceShrineFail = .025f;

        public static float ChanceShrineSuceed = .01f;

        public static bool ChanceShrineScalePlayers = true;

        public override Texture backgroundTexture => MainAssets.LoadAsset<Texture>("Assets/Textrures/Icons/TierBackground/BgHighlander.png");

        //public ColorCatalog.ColorIndex colorIndex = ColorsAPI.RegisterColor(new Color32(21,99,58,255));//ColorCatalog.ColorIndex.Money;//CoreLight.instance.colorCatalogEntry.ColorIndex;

        //public ColorCatalog.ColorIndex darkColorIndex = ColorsAPI.RegisterColor(new Color32(1,126,62,255));//ColorCatalog.ColorIndex.Money;//CoreDark.instance.colorCatalogEntry.ColorIndex;
        /// <summary>
        ///  CoreLight.instance.colorCatalogEntry.ColorIndex;
        /// 
        /// x => CoreDark.instance.colorCatalogEntry.ColorIndex;
        /// </summary>
        public override void Init()
        {
            colorIndex = Colors.TempHighlandLight;//ColorsAPI.RegisterColor(Color.gray);
            darkColorIndex = Colors.TempHighlandDark;//ColorsAPI.RegisterColor(Color.gray);
            itemTierDef.pickupRules = ItemTierDef.PickupRules.ConfirmAll;
            //colorIndex = CoreLight.instance.colorIndex;//CoreLight.instance.colorCatalogEntry.ColorIndex;//new Color(.08f, .39f, .23f));//(new Color32(21, 99, 58, 255)));
            //darkColorIndex = CoreDark.instance.colorIndex; //new Color(0f,.49f,.24f));//(new Color32(1, 126, 62, 255)));
            BransItems.ModLogger.LogWarning("ArraySize:" + (ColorCatalog.indexToHexString.Length));
            BransItems.ModLogger.LogWarning("LastElementHex:" + (ColorCatalog.indexToHexString[ColorCatalog.indexToHexString.Length - 1]));
            CreateTier();
            itemTierDef.highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            itemTierDef.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();
            SetHooks();
            //BransItems.ModLogger.LogWarning(itemTierDef.tier.ToString());
            //BransItems.ModLogger.LogWarning("Correct:" + ItemTier.AssignedAtRuntime.ToString());
            //BransItems.ModLogger.LogWarning("MyTierName:" + itemTierDef.name);
            //BransItems.ModLogger.LogWarning("MyTierCanScrap:" + itemTierDef.canScrap);

        }

        public void SetHooks()
        {
            //On.RoR2.CharacterBody.OnInventoryChanged;
            On.RoR2.CharacterMaster.OnItemAddedClient += CharacterMaster_OnItemAddedClient;
            On.RoR2.ShrineCombatBehavior.OnDefeatedServer += ShrineCombatBehavior_OnDefeatedServer;
            On.RoR2.BarrelInteraction.CoinDrop += BarrelInteraction_CoinDrop;
            On.RoR2.DeathRewards.OnKilledServer += DeathRewards_OnKilledServer;
            On.RoR2.ShrineChanceBehavior.AddShrineStack += ShrineChanceBehavior_AddShrineStack;
        }

        private void ShrineChanceBehavior_AddShrineStack(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator)
        {
            int purchaseCount = self.successfulPurchaseCount;
            orig(self, activator);
            bool success = purchaseCount < self.successfulPurchaseCount;

            float playerScale =(ChanceShrineScalePlayers? Math.Max(1f, (float)Math.Sqrt(PlayerCharacterMasterController.instances.Count)) : 1);

            if(success)
            {
                if (RoR2Application.rng.RangeFloat(0f, 1f) <= ChanceShrineSuceed*playerScale)
                {
                    DropItem(self.gameObject.transform, 8.5f);
                }
            }
            else
            {
                if (RoR2Application.rng.RangeFloat(0f, 1f) <= ChanceShrineFail)
                {
                    DropItem(self.gameObject.transform, 8.5f);
                }
            }
        }

        private void DeathRewards_OnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if(damageReport.victimBody.isElite)
            {
                float playerScale = (EliteScalePlayers ? Math.Max(1f, (float)Math.Sqrt(PlayerCharacterMasterController.instances.Count)) : 1);
                if (RoR2Application.rng.RangeFloat(0f, 1f) <= EliteDeathChance*playerScale)
                {
                    DropItem(damageReport.victimBody.transform, 5.5f);
                }
            }
        }

        private void BarrelInteraction_CoinDrop(On.RoR2.BarrelInteraction.orig_CoinDrop orig, BarrelInteraction self)
        {
            orig(self);
            float playerScale = (BarrelScalePlayers ? Math.Max(1f, (float)Math.Sqrt(PlayerCharacterMasterController.instances.Count)) : 1);
            if (RoR2Application.rng.RangeFloat(0f, 1f) <= BarrelChance*playerScale)
            {
                DropItem(self.gameObject.transform, 5.5f);
            }
        }

        private void ShrineCombatBehavior_OnDefeatedServer(On.RoR2.ShrineCombatBehavior.orig_OnDefeatedServer orig, ShrineCombatBehavior self)
        {
            orig(self);
            float playerScale = (CombatShrineScalePlayers ? Math.Max(1f, (float)Math.Sqrt(PlayerCharacterMasterController.instances.Count)) : 1);
            if (RoR2Application.rng.RangeFloat(0f, 1f) <= CompatShrineChance*playerScale)
            {
                DropItem(self.gameObject.transform, 8.5f);
            }
        }

        private void DropItem(Transform location, float height)
        {
            List<PickupDef> HighList = ItemHelpers.PickupDefsWithTier(this.itemTierDef);
            int picked = RoR2Application.rng.RangeInt(0, HighList.Count);
            //BransItems.ModLogger.LogWarning("Item#"+picked+"   Count:"+HighList.Count+"    Name"+ItemCatalog.GetItemDef(HighList[picked].itemIndex).nameToken+"     index"+ HighList[picked].itemIndex);
            PickupDef pickupDef = HighList[picked];
            PickupDropletController.CreatePickupDroplet(pickupDef.pickupIndex, location.position + Vector3.up * height, Vector3.up * 25f);
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#FAF7B9><size=120%>" + "You have been rewarded with a gift from the Highlands." + "</color></size>"
            });
        }

        private void CharacterMaster_OnItemAddedClient(On.RoR2.CharacterMaster.orig_OnItemAddedClient orig, CharacterMaster self, ItemIndex itemIndex)
        {
            //BransItems.ModLogger.LogWarning("Indexed Pickup" + itemIndex.ToString());
            //BransItems.ModLogger.LogWarning("All Highlander" + Highlander.instance.ItemsWithThisTier.Count);
            if (ItemCatalog.GetItemDef(itemIndex)._itemTierDef==itemTierDef)
            {
                int count = self.inventory.GetTotalItemCountOfTier(itemTierDef.tier);
                BransItems.ModLogger.LogWarning("IN LIST, Count:"+count);
                
                if (count >= 2)
                {
                    BransItems.ModLogger.LogWarning("Inventory Count" + self.inventory.itemAcquisitionOrder.Count);
                    for (int i = 0; i < self.inventory.itemAcquisitionOrder.Count; i++)
                    {
                        ItemIndex temp = self.inventory.itemAcquisitionOrder[i];
                        BransItems.ModLogger.LogWarning("i:" + i + "    index" + temp.ToString());
                        if ((ItemCatalog.GetItemDef(temp)._itemTierDef == itemTierDef))
                        {
                            ItemIndex toss = self.inventory.itemAcquisitionOrder[i];
                            self.inventory.RemoveItem(toss);

                            Vector3 val = Vector3.up * 25f; //dropTransform.forward * dropForwardVelocityStrength;
                            PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(toss);
                            BransItems.ModLogger.LogWarning("DropIndex:" + pickupIndex);
                            PickupDropletController.CreatePickupDroplet(pickupIndex, self.GetBody().transform.position + Vector3.up * 1.5f, val);
                            break;
                        }
                    }
                }
            }
        }
    }
}
