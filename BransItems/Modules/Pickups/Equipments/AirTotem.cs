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
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;
using BransItems.Modules.Utils;
using static RoR2.EquipmentSlot;
using static BransItems.Modules.Pickups.Items.Essences.EssenceHelpers;
using BransItems.Modules.Pickups.Items.NoTier;
using BransItems.Modules.Pickups.Items.Tier3;
using UnityEngine.AddressableAssets;

namespace BransItems.Modules.Pickups.Equipments
{
    class AirTotem : EquipmentBase<AirTotem>
    {
        public override string EquipmentName => "Reducer";

        public override string EquipmentLangTokenName => "Reducer";

        public override string EquipmentPickupDesc => "Transform an item to its "+BransItems.EssencesKeyword+".";

        public override string EquipmentFullDescription => "<style=cIsUtility>Transform</style> an item to an " + BransItems.EssenceKeyword + " which gives a slight <style=cIsUtility>stat boost</style>.<style=cIsUtility> Scales with item rarity</style>.";

        public override string EquipmentLore =>
            $"";
        public override bool UseTargeting => true;

        public override GameObject EquipmentModel => SetupModel();

        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Equipment/Reducer/ReducerIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public override float Cooldown { get; } = 30f;


        // private UserTargetInfo currentTarget;

        //private InputBankTest inputBank;

        //private Indicator targetIndicator;

        public override void Init(ConfigFile config)
        {
            CreateLang();
            //CreateBuff();
            CreateTargetingIndicator();

            //CreateProjectile();

            CreateEquipment();
            Hooks();

            
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
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot.currentTarget.pickupController
                && Run.instance)
            {

                //var pickupControllerPickupIndex = slot.currentTarget.pickupController.GetComponent<PickupIndex>();
                ItemTier Tier= ItemTierCatalog.GetItemTierDef(ItemCatalog.GetItemDef(slot.currentTarget.pickupController.pickupIndex.itemIndex).tier).tier;  
                if (false)
                {
                   // if (!oldPurch.available) return false;
                }
                // Vector3 vector = (slot.currentTarget.pickupController.transform ? slot.currentTarget.pickupController.transform.position : Vector3.zero);
                Transform playerTransform = slot.transform;
                Vector3 vector = slot.transform.position;
                Vector3 normalized = (vector - slot.characterBody.corePosition).normalized;
                
                PickupIndex pickup = GetEssenceIndex(slot.rng);
                int LootedClamCount = slot.inventory.GetItemCount(LootedBloodburstClam.instance.ItemDef.itemIndex);

                Ray aimRay = slot.GetAimRay();
                Vector3 direction=aimRay.direction;
                //Quaternion quaternion = Quaternion.LookRotation(aimRay.direction);
                ModLogger.LogInfo("Dir" + direction.ToString());
                //PickupDropletController.CreatePickupDroplet(GetEssenceIndex(slot.rng), vector + new Vector3(0, 3, 0), direction * 40f); //normalized * 15f);
                int spawnTotal = 0;
                switch(Tier)
                {
                    case ItemTier.Tier1:
                    case ItemTier.VoidTier1:
                        spawnTotal = 1;
                        break;
                    case ItemTier.Tier2:
                    case ItemTier.VoidTier2:
                        spawnTotal = 3;
                        break;
                    case ItemTier.Tier3:
                    case ItemTier.VoidTier3:
                        spawnTotal = 15;
                            break;
                    case ItemTier.Boss:
                        spawnTotal = 8;
                        break;
                    case ItemTier.Lunar:
                        pickup = PickupCatalog.FindPickupIndex(RoR2Content.Items.LunarSpecialReplacement.itemIndex);
                        PickupDropletController.CreatePickupDroplet(pickup, vector, direction * 40f);
                        GameObject.Destroy(slot.currentTarget.rootObject);
                        return true;
                        break;

                }
                spawnTotal += LootedClamCount*BloodburstClam.AdditionalDrops;
                for (int i = 1; i <= spawnTotal; i++)
                {
                    pickup = GetEssenceIndex(slot.rng);
                    //ModLogger.LogInfo("Item" + PickupCatalog.GetPickupDef(pickup).internalName);
                    PickupDropletController.CreatePickupDroplet(pickup, vector + new Vector3(.1f * i, 3, 0), direction * 40f); // normalized * 15f);
                }
                GameObject.Destroy(slot.currentTarget.rootObject);
                //var pos = slot.currentTarget.rootObject.transform.position;

                return true;
            }
            return false;
        }
        //private void EquipmentSlot_UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        private void UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            if (targetingEquipmentIndex != EquipmentDef.equipmentIndex)
            {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                return;
            }
            self.currentTarget= new UserTargetInfo(self.FindPickupController(self.GetAimRay(), 10f, 30f, requireLoS: true, true));
            //var res = CommonCode.FindNearestInteractable(self.gameObject, validObjectNames, self.GetAimRay(), 10f, 20f, false);
            // Transform tsf = null;
            //if (res) tsf = res.transform;
            //self.currentTarget = new EquipmentSlot.UserTargetInfo
            // {
            //    transformToIndicateAt = tsf,
            //     pickupController = null,
            //     hurtBox = null,
            //     rootObject = res
            // };
            var targetingComponent = self.GetComponent<TargetingControllerComponent>();
            if (self.currentTarget.rootObject != null)
            {
                //var purch = res.GetComponent<PurchaseInteraction>();
                if (true)//!res.GetComponent<RecombobulatorFlag>() && (!purch || purch.available))
                {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
                }
                else
                {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator");
                }

                self.targetIndicator.active = true;
                self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
            }
            else
            {
                self.targetIndicator.active = false;
            }
        }
    }
    

}

