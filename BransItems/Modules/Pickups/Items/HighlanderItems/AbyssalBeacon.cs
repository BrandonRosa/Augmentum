using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;
using static RoR2.ItemTag;
using BransItems.Modules.ItemTiers.HighlanderTier;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace BransItems.Modules.Pickups.Items.HighlanderItems
{
    class AbyssalBeacon : ItemBase<AbyssalBeacon>
    {
        public override string ItemName => "Abyssal Beacon";
        public override string ItemLangTokenName => "ABYSSAL_BEACON";
        public override string ItemPickupDesc => "After completing a teleporter event, create a null portal. Destroy this if you travel through it.";
        public override string ItemFullDescription => $"After completing a teleporter event, create a null portal. Destroy this if you travel through it.";

        public override string ItemLore => "";

        public override ItemTierDef ModdedTierDef => Highlander.instance.itemTierDef; //ItemTier.AssignedAtRuntime;

        public override ItemTier Tier => ItemTier.AssignedAtRuntime;

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("EssenceOfStrength.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("EssenceOfStrength.png");

        //public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Textrures/Icons/Temporary/crystal4/source/crystal4.prefab");
        //public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Temporary/crystal4/source/TempCrystal4.png");

        //public override GameObject ItemModel => Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
        //public override Sprite ItemIcon => Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => false;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.CannotCopy, ItemTag.AIBlacklist };


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
            
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.42142F, -0.10234F),
                    localAngles = new Vector3(351.1655F, 45.64202F, 351.1029F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.35414F, -0.14761F),
                    localAngles = new Vector3(356.5505F, 45.08208F, 356.5588F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 2.46717F, 2.64379F),
                    localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
                    localScale = new Vector3(.2F, .2F, .2F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0, 0.24722F, -0.01662F),
                    localAngles = new Vector3(10.68209F, 46.03322F, 11.01807F),
                    localScale = new Vector3(0.025F, 0.025F, 0.025F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.24128F, -0.14951F),
                    localAngles = new Vector3(6.07507F, 45.37084F, 6.11489F),
                    localScale = new Vector3(0.017F, 0.017F, 0.017F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.31304F, -0.00747F),
                    localAngles = new Vector3(359.2931F, 45.00048F, 359.2912F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0, 1.94424F, -0.47558F),
                    localAngles = new Vector3(20.16552F, 48.87548F, 21.54582F),
                    localScale = new Vector3(.15F, .15F, .15F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.30118F, -0.0035F),
                    localAngles = new Vector3(8.31363F, 45.67525F, 8.41428F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.65444F, 1.64345F),
                    localAngles = new Vector3(326.1803F, 277.2657F, 249.9269F),
                    localScale = new Vector3(.2F, .2F, .2F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0068F, 0.3225F, -0.03976F),
                    localAngles = new Vector3(0F, 45F, 0F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.14076F, 0.15542F, -0.04648F),
                    localAngles = new Vector3(356.9802F, 81.10978F, 353.687F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hat",
                    localPos = new Vector3(0F, 0.01217F, -0.00126F),
                    localAngles = new Vector3(356.9376F, 25.8988F, 14.69767F),
                    localScale = new Vector3(0.001F, 0.001F, 0.001F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00042F, 0.46133F, 0.01385F),
                    localAngles = new Vector3(355.2848F, 47.55381F, 355.0908F),
                    localScale = new Vector3(0.020392F, 0.020392F, 0.020392F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00076F, -0.0281F, 0.09539F),
                    localAngles = new Vector3(338.9489F, 145.7505F, 217.6883F),
                    localScale = new Vector3(0.005402F, 0.005402F, 0.005402F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.00277F, -0.13259F),
                    localAngles = new Vector3(322.1495F, 124.8318F, 235.476F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.32104F, 0F),
                    localAngles = new Vector3(0F, 321.2954F, 0F),
                    localScale = new Vector3(0.024027F, 0.024027F, 0.024027F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.00216F, 0.01033F, 0F),
                    localAngles = new Vector3(0F, 323.6887F, 355.1232F),
                    localScale = new Vector3(0.000551F, 0.000551F, 0.000551F)
                }
            });
            return rules;
        }


        public override void Hooks()
        {
            //TeleporterInteraction.onTeleporterFinishGlobal += TeleporterInteraction_onTeleporterFinishGlobal;
            On.RoR2.GlobalEventManager.OnInteractionBegin += GlobalEventManager_OnInteractionBegin;
            On.RoR2.TeleporterInteraction.AttemptToSpawnAllEligiblePortals += TeleporterInteraction_AttemptToSpawnAllEligiblePortals;
        }

        private void TeleporterInteraction_AttemptToSpawnAllEligiblePortals(On.RoR2.TeleporterInteraction.orig_AttemptToSpawnAllEligiblePortals orig, TeleporterInteraction self)
        {
            ModLogger.LogWarning("FINISH");
            ActivateItem();
            orig(self);
        }

        private void GlobalEventManager_OnInteractionBegin(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            orig(self, interactor, interactable, interactableObject);
            if(interactableObject.name== "PortalArena(Clone)")
            {
                SceneDef sceneDef = Stage.instance.sceneDef;
                if (sceneDef != SceneCatalog.GetSceneDefFromSceneName("bazaar"))
                {
                    RemoveClosestPlayerItem(interactableObject.transform);
                }
            }
            //ModLogger.LogWarning("OBJECT" + interactableObject.name);
        }

        private void TeleporterInteraction_onTeleporterFinishGlobal(TeleporterInteraction obj)
        {
            ModLogger.LogWarning("FINISH");
            ActivateItem();
        }

        private void ActivateItem()
        {
            //Get list of all Players
            List<PlayerCharacterMasterController> masterList = new List<PlayerCharacterMasterController>(PlayerCharacterMasterController.instances);
            for(int i=0;i<masterList.Count;i++)
            {
                //If the player isnt dead
                if(!masterList[i].master.IsDeadAndOutOfLivesServer())
                {
                    //if the player has a body and an inventory AND they have the item
                    if(masterList[i].body && masterList[i].body.inventory && masterList[i].body.inventory.GetItemCount(ItemDef)>0)
                    {
                        CharacterBody body = masterList[i].body;
                        Vector3 footPosition = body.footPosition;
                        GameObject value=Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalArena/PortalArena.prefab").WaitForCompletion();
                        GameObject val = UnityEngine.Object.Instantiate<GameObject>(value, footPosition, Quaternion.LookRotation(body.characterDirection.forward));
                        SceneExitController component = val.GetComponent<SceneExitController>();
                        SceneDef sceneDef = Stage.instance.sceneDef;
                        component.useRunNextStageScene = (UnityEngine.Object)(object)component.destinationScene == (UnityEngine.Object)(object)sceneDef;
                        NetworkServer.Spawn(val);
                    }

                }
            }
        }

        private void RemoveClosestPlayerItem(Transform transform)
        {
            List<PlayerCharacterMasterController> masterList = new List<PlayerCharacterMasterController>(PlayerCharacterMasterController.instances);
            int closestInventoryIndex = -1;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < masterList.Count; i++)
            {
                //If the player isnt dead
                if (!masterList[i].master.IsDeadAndOutOfLivesServer())
                {
                    //if the player has a body and an inventory AND they have the item
                    if (masterList[i].body && masterList[i].body.inventory && masterList[i].body.inventory.GetItemCount(ItemDef) > 0)
                    {
                        //if there is no closest index make the first hit the closest player
                        if (closestInventoryIndex == -1)
                        {
                            closestInventoryIndex = i;
                            closestDistance = Vector3.Distance(masterList[i].body.transform.position, transform.position);
                        }
                        else
                        {
                            float distance = Vector3.Distance(masterList[i].body.transform.position, transform.position);
                            if (distance < closestDistance)
                            {
                                closestInventoryIndex = i;
                                closestDistance = distance;
                            }
                        }
                    }
                }
            }

            masterList[closestInventoryIndex].body.inventory.RemoveItem(ItemDef);
        }

    }
}
