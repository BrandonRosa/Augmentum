using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Augmentum.Augmentum;
using static Augmentum.Modules.Utils.ItemHelpers;
using Augmentum.Modules.Utils;
using Augmentum.Modules.Compatability;
using static RoR2.EquipmentSlot;
using static Augmentum.Modules.Pickups.Items.Essences.EssenceHelpers;
using Augmentum.Modules.Pickups.Items.NoTier;
using Augmentum.Modules.Pickups.Items.Tier3;
using UnityEngine.AddressableAssets;

namespace Augmentum.Modules.Pickups.Equipments
{
    class AirTotem : EquipmentBase<AirTotem>
    {
        public override string EquipmentName => "Reducer";

        public override string EquipmentLangTokenName => "Reducer";

        public override string EquipmentPickupDesc => "Transform an item to its "+Augmentum.EssencesKeyword+".";

        public override string EquipmentFullDescriptionRaw =>
            @"<style=cIsUtility>Transform</style> an item to an {0} which gives a slight <style=cIsUtility>stat boost</style>.<style=cIsUtility> Scales with item rarity</style>.";

        public override string EquipmentFullDescriptionFormatted =>
            string.Format(GetLangDesc(), Augmentum.EssenceKeyword);
        public override string EquipmentLore =>
            $"";
        public override bool UseTargeting => true;

        public override GameObject EquipmentModel => SetupModel();

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Equipment/Reducer/ReducerIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public override float Cooldown { get; } = 30f;

        public static int T1EssenceCount => T1EssenceCountEntry != null ? T1EssenceCountEntry.Value : default;
        public static int T2EssenceCount=> T2EssenceCountEntry != null ? T2EssenceCountEntry.Value : default;
        public static int T3EssenceCount=> T3EssenceCountEntry != null ? T3EssenceCountEntry.Value : default;
        public static int BossEssenceCount=> BossEssenceCountEntry != null ? BossEssenceCountEntry.Value : default;

        public static ConfigEntry<int> T1EssenceCountEntry;
        public static ConfigEntry<int> T2EssenceCountEntry;
        public static ConfigEntry<int> T3EssenceCountEntry;
        public static ConfigEntry<int> BossEssenceCountEntry;


        // private UserTargetInfo currentTarget;

        //private InputBankTest inputBank;

        //private Indicator targetIndicator;

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateTargetingIndicator();

            //CreateProjectile();

            CreateEquipment();
            Hooks();

            
        }

        private void CreateConfig(ConfigFile config)
        {
            T1EssenceCountEntry = ConfigManager.ConfigOption<int>("Equipment: " + EquipmentName, "Drops from T1 items", 1, "How many essences should drop from T1 Items?");
            T2EssenceCountEntry = ConfigManager.ConfigOption<int>("Equipment: " + EquipmentName, "Drops from T2 items", 3, "How many essences should drop from T2 Items?");
            T3EssenceCountEntry = ConfigManager.ConfigOption<int>("Equipment: " + EquipmentName, "Drops from T3 items", 15, "How many essences should drop from T3 Items?");
            BossEssenceCountEntry = ConfigManager.ConfigOption<int>("Equipment: " + EquipmentName, "Drops from Boss items", 8, "How many essences should drop from Boss Items?");
        }

        private void CreateTargetingIndicator()
        {
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/RecyclerIndicator"), "AirTotemIndicator", false); //"SoulPinIndicator", false);
            //TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().sprite = MainAssets.LoadAsset<Sprite>("SoulPinReticuleIcon.png");
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);
        }

        private GameObject SetupModel()//RoR2/Base/Recycle/PickupRecycler.prefab
        {
            GameObject temp= Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Recycle/PickupRecycler.prefab").WaitForCompletion().InstantiateClone("Reducer", false);
            Texture2D texture = MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Icons/Equipment/Reducer/ReducerTexture.png");

            temp.GetComponentInChildren<MeshRenderer>().GetMaterial().SetTexture("_MainTex",texture);

            return temp;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = EquipmentModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
            //ItemBodyModelPrefab.AddComponent<SoulPinDisplayHandler>();

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            /*
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(-90, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(8f, -4, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-2f, 0, -2f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-8f, 0, 8f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.8f, 0.8f, 0.8f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Base",
                    localPos = new Vector3(-1f, 0, -1f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            */
            return rules;
        }

        public override void Hooks()
        {
            //On.RoR2.CharacterBody.OnBuffFirstStackGained += RemoveBuffFromNonElites;
            //On.RoR2.GlobalEventManager.OnCharacterDeath += MorphEquipmentIntoAffix;
            //On.RoR2.EquipmentSlot.Update += UpdateTargets;
            //On.RoR2.EquipmentSlot.Start += Start;
            On.RoR2.EquipmentSlot.UpdateTargets += UpdateTargets;

            On.RoR2.Language.LoadStrings += (orig, self) =>
            {
                orig(self);
                this.EquipmentDef.pickupToken = EquipmentPickupDesc;
            };
            On.RoR2.Language.SetCurrentLanguage += (orig, languageName, forceUnload) =>
            {
                orig(languageName, forceUnload);
                this.EquipmentDef.pickupToken = EquipmentPickupDesc;
            };
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (!Run.instance || !slot || !slot.characterBody)
                return false;

            GenericPickupController pickupController = slot.currentTarget.pickupController;
            if (!TryGetReducibleItem(pickupController, out ItemDef itemDef))
                return false;

            Vector3 vector = slot.transform.position;
            Vector3 direction = slot.GetAimRay().direction;

            int spawnTotal = 0;
            switch (itemDef.tier)
            {
                case ItemTier.Tier1:
                case ItemTier.VoidTier1:
                    spawnTotal = T1EssenceCount;
                    break;
                case ItemTier.Tier2:
                case ItemTier.VoidTier2:
                    spawnTotal = T2EssenceCount;
                    break;
                case ItemTier.Tier3:
                case ItemTier.VoidTier3:
                    spawnTotal = T3EssenceCount;
                    break;
                case ItemTier.Boss:
                case ItemTier.VoidBoss:
                    spawnTotal = BossEssenceCount;
                    break;
                case ItemTier.Lunar:
                    ConsumePickup(pickupController);
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(RoR2Content.Items.LunarSpecialReplacement.itemIndex), vector, direction * 40f);
                    return true;
            }

            if (LootedBloodburstClam.instance?.ItemDef && slot.inventory)
                spawnTotal += slot.inventory.GetItemCount(LootedBloodburstClam.instance.ItemDef) * BloodburstClam.AdditionalDrops;

            ConsumePickup(pickupController);
            for (int i = 1; i <= spawnTotal; i++)
            {
                PickupDropletController.CreatePickupDroplet(GetEssenceIndex(slot.rng), vector + new Vector3(.1f * i, 3, 0), direction * 40f);
            }

            return true;
        }

        /// <summary>
        /// Only real, still existing item pickups of a tier we know how to reduce. Equipment, essences, display-only pickups and pickups another Reducer already consumed are rejected.
        /// </summary>
        public static bool TryGetReducibleItem(GenericPickupController pickupController, out ItemDef itemDef)
        {
            itemDef = null;
            if (!IsUsablePickup(pickupController))
                return false;

            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupController.pickupIndex);
            if (pickupDef == null || pickupDef.itemIndex == ItemIndex.None)
                return false;

            itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
            if (!itemDef)
                return false;

            switch (itemDef.tier)
            {
                case ItemTier.Tier1:
                case ItemTier.Tier2:
                case ItemTier.Tier3:
                case ItemTier.Boss:
                case ItemTier.VoidTier1:
                case ItemTier.VoidTier2:
                case ItemTier.VoidTier3:
                case ItemTier.VoidBoss:
                case ItemTier.Lunar:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Shared by the Reducer and Reuser: the pickup exists, has not been eaten already this frame, and is not a BiggerBazaar display item.
        /// </summary>
        public static bool IsUsablePickup(GenericPickupController pickupController)
        {
            return pickupController
                && pickupController.gameObject.activeInHierarchy
                && !pickupController.GetComponent<ConsumedPickupMarker>()
                && !ModCompatability.BiggerBazaarCompat.IsDisplayItem(pickupController.gameObject);
        }

        /// <summary>
        /// Destroy is deferred to the end of the frame, so mark the pickup to stop a second equipment use from consuming it again.
        /// </summary>
        public static void ConsumePickup(GenericPickupController pickupController)
        {
            pickupController.gameObject.AddComponent<ConsumedPickupMarker>();
            GameObject.Destroy(pickupController.gameObject);
        }

        private void UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            if (targetingEquipmentIndex != EquipmentDef.equipmentIndex)
            {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                return;
            }
            self.currentTarget = new UserTargetInfo(self.FindPickupController(self.GetAimRay(), 10f, 30f, requireLoS: true, true));

            if (self.currentTarget.rootObject != null && self.currentTarget.transformToIndicateAt)
            {
                bool valid = TryGetReducibleItem(self.currentTarget.pickupController, out _);
                self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>(valid ? "Prefabs/RecyclerIndicator" : "Prefabs/RecyclerBadIndicator");
                self.targetIndicator.active = true;
                self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
            }
            else
            {
                self.targetIndicator.active = false;
            }
        }
    }

    public class ConsumedPickupMarker : MonoBehaviour
    {
    }

}

