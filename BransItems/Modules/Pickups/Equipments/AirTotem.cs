using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using static RoR2.EquipmentSlot;
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

namespace BransItems.Modules.Pickups.Equipments
{
    class AirTotem : EquipmentBase
    {
        public override string EquipmentName => "Air Totem";

        public override string EquipmentLangTokenName => "AIR_TOTEM";

        public override string EquipmentPickupDesc => "On use, transform an item to its base essece.";

        public override string EquipmentFullDescription => "On use, transform an item to a essence which gives a slight stat boost.";

        public override string EquipmentLore =>

            $"Found on a scrap of paper in an ornate case along with the device: \"[...] at that point, the binding process will become automatic, " +
            $"and all the user needs to do is sever the specimen's connection to its soul.\"";
        public override bool UseTargeting => true;

        public static GameObject ItemBodyModelPrefab;

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
        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = EquipmentModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);
            //ItemBodyModelPrefab.AddComponent<SoulPinDisplayHandler>();

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

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
            return rules;
        }

        public override void Hooks()
        {
            //On.RoR2.CharacterBody.OnBuffFirstStackGained += RemoveBuffFromNonElites;
            //On.RoR2.GlobalEventManager.OnCharacterDeath += MorphEquipmentIntoAffix;
            //On.RoR2.EquipmentSlot.Update += UpdateTargets;
            //On.RoR2.EquipmentSlot.Start += Start;
            //On.RoR2.EquipmentSlot.UpdateTargets += UpdateTargets; 
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot.currentTarget.pickupController
               // && validObjectNames.Contains(slot.currentTarget.rootObject.name.Replace("(Clone)", ""))
                //&& !slot.currentTarget.rootObject.GetComponent<RecombobulatorFlag>()
                //&& mostRecentDeck != null
                && Run.instance)
            {

                var pickupControllerPickupIndex = slot.currentTarget.pickupController.GetComponent<PickupIndex>(); 
                PickupCatalog.
                if (oldPurch)
                {
                    if (!oldPurch.available) return false;
                }

                var shopcpt = slot.currentTarget.rootObject.GetComponent<ShopTerminalBehavior>();
                if (shopcpt && shopcpt.serverMultiShopController)
                {
                    slot.currentTarget.rootObject = shopcpt.serverMultiShopController.transform.root.gameObject;
                    foreach (var term in shopcpt.serverMultiShopController.terminalGameObjects)
                        GameObject.Destroy(term);
                }

                GameObject.Destroy(slot.currentTarget.rootObject);

                var pos = slot.currentTarget.rootObject.transform.position;

                WeightedSelection<DirectorCard> filteredDeck = new(8);
                var matchCategories = mostRecentDeckCategories.Where(kvp => kvp.Key.spawnCard.prefab.name == slot.currentTarget.rootObject.name.Replace("(Clone)", "")).Select(kvp => kvp.Value);
                for (var i = 0; i < mostRecentDeck.Count; i++)
                {
                    var card = mostRecentDeck.GetChoice(i);
                    if (card.value == null || !card.value.IsAvailable()) continue;
                    if (!validObjectNames.Contains(card.value.spawnCard.prefab.name))
                        continue;
                    if (respectCategory && (
                        !mostRecentDeckCategories.TryGetValue(card.value, out var thisCategory)
                        || !matchCategories.Contains(thisCategory)
                        ))
                        continue;
                    filteredDeck.AddChoice(card);
                }
                if (filteredDeck.Count == 0)
                    return false;

                var draw = filteredDeck.Evaluate(rng.nextNormalizedFloat);

                if (Compat_ClassicItems.enabled)
                {
                    var rerolls = Compat_ClassicItems.CheckEmbryoProc(slot, equipmentDef);
                    for (var i = 0; i < rerolls; i++)
                    {
                        var draw2 = filteredDeck.Evaluate(rng.nextNormalizedFloat);
                        if (draw2.selectionWeight < draw.selectionWeight)
                            draw = draw2;
                    }
                }

                var obj = DirectorCore.instance.TrySpawnObject(
                    new DirectorSpawnRequest(
                        draw.spawnCard,
                        new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Direct,
                            position = pos,
                            preventOverhead = false
                        },
                        this.rng
                        ));
                if (!obj)
                {
                    TinkersSatchelPlugin._logger.LogError("Recombobulator failed to replace interactable!");
                    return false;
                }
                var purch = obj.GetComponent<PurchaseInteraction>();
                if (purch && purch.costType == CostTypeIndex.Money)
                {
                    purch.Networkcost = Run.instance.GetDifficultyScaledCost(purch.cost);
                }
                obj.AddComponent<RecombobulatorFlag>();

                var shopcpt2 = obj.GetComponent<MultiShopController>();
                if (shopcpt2)
                {
                    foreach (var term in shopcpt2.terminalGameObjects)
                        term.AddComponent<RecombobulatorFlag>();
                }

                return true;
            }
            return false;
        }
    }
    /*

    protected override bool ActivateEquipment(EquipmentSlot slot)
    {
        if (!slot.characterBody || !slot.characterBody.inputBank) { return false; }

        var targetComponent = slot.GetComponent<TargetingControllerComponent>();
        var displayTransform = slot.FindActiveEquipmentDisplay();


        return FireAirTotem();
    }

    private void InvalidateCurrentTarget()
    {
        currentTarget = default(UserTargetInfo);
    }
    private bool FireAirTotem()
    {
        //UpdateTargets();
        GenericPickupController pickupController = currentTarget.pickupController;
        if ((bool)pickupController)
        {
            PickupIndex initialPickupIndex = pickupController.pickupIndex;
            //subcooldownTimer = 0.2f;
            PickupIndex[] array = GetBasicEssencePickupIndex();
                //(from pickupIndex in PickupTransmutationManager.GetAvailableGroupFromPickupIndex(pickupController.pickupIndex)
                                  // where pickupIndex != initialPickupIndex
                                  // select pickupIndex).ToArray();
            if (array == null)
            {
                return false;
            }
            if (array.Length == 0)
            {
                return false;
            }
            pickupController.NetworkpickupIndex = Run.instance.treasureRng.NextElementUniform(array);
            EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniRecycleEffect"), pickupController.pickupDisplay.transform.position, Quaternion.identity, transmit: true);
            //pickupController.NetworkRecycled = true;
            InvalidateCurrentTarget();
            return true;
        }
        return false;
    }

    private void Start(On.RoR2.EquipmentSlot.orig_Start orig, global::RoR2.EquipmentSlot self)
    {
        inputBank = self.GetComponent<InputBankTest>();
        targetIndicator = new Indicator(self.gameObject, null);
    }
    private Ray GetAimRay()
    {
        Ray result = default(Ray);
        result.direction = inputBank.aimDirection;
        result.origin = inputBank.aimOrigin;
        return result;
    }
    private void UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
    {
        //orig(self);
        bool AirFlag = targetingEquipmentIndex == EquipmentDef.equipmentIndex;
        if (AirFlag)//(self.equipmentIndex == EquipmentDef.equipmentIndex)
        {
            var targetingComponent = self.GetComponent<TargetingControllerComponent>();
            if (targetingComponent)
            {
                currentTarget = new UserTargetInfo(self.FindPickupController(self.GetAimRay(), 10f, 30f, requireLoS: true, true));


                GenericPickupController pickupController = currentTarget.pickupController;
                bool flag5 = currentTarget.transformToIndicateAt;
                if (flag5)
                {

                    if (!pickupController.Recycled)
                    {
                        targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
                    }

                }
                targetIndicator.active = flag5;
                targetIndicator.targetTransform = (flag5 ? currentTarget.transformToIndicateAt : null);
            }
        }
    }

    private void RemoveNonElitesFromTargeting(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
    {
        orig(self);
        if (self.equipmentIndex == EquipmentDef.equipmentIndex)
        {
            var targetingComponent = self.GetComponent<TargetingControllerComponent>();
            if (targetingComponent)
            {
                targetingComponent.AdditionalBullseyeFunctionality = (bullseyeSearch) => bullseyeSearch.FilterElites();
            }
        }
    }
    */

}
}
