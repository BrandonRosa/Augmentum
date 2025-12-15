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
using static Augmentum.Augmentum;
using static Augmentum.Modules.Utils.ItemHelpers;
using Augmentum.Modules.Utils;
using static RoR2.EquipmentSlot;
using static Augmentum.Modules.Pickups.Items.Essences.EssenceHelpers;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;
using Augmentum.Modules.Compatability;
using System.Runtime.Serialization;
using UnityEngine.AddressableAssets;
using System.Runtime.CompilerServices;

namespace Augmentum.Modules.Pickups.Equipments
{
    class EarthTotem : EquipmentBase<EarthTotem>
    {
        public override string EquipmentName => "Reuser";

        public override string EquipmentLangTokenName => "REUSER";

        public override string EquipmentPickupDesc => "Absorb an equipment and gain its effect.";

        public override string EquipmentFullDescriptionRaw => "<style=cIsUtility>Absorb</style> an equipment and add its effect to this equipment.";

        public override string EquipmentFullDescriptionFormatted => GetLangDesc();

        public override string EquipmentLore =>

            $"";
        public override bool UseTargeting => true;

        public override GameObject EquipmentModel => SetupModel();
        public override Sprite EquipmentIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Equipment/Reuser/ReuserIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public override float Cooldown { get; } = 15;//140f;

        public float StartCooldown = 15f;

        //public List<EquipmentDef> AbsorbedEquipments;

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
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/RecyclerIndicator"), "AirTotemIndicator", false);
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);
        }

        private GameObject SetupModel()//RoR2/Base/Recycle/PickupRecycler.prefab
        {
            GameObject temp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Recycle/PickupRecycler.prefab").WaitForCompletion().InstantiateClone("Reuser", false);
            Texture2D texture = MainAssets.LoadAsset<Texture2D>("Assets/Textrures/Icons/Equipment/Reuser/ReuserTexture.png");

            temp.GetComponentInChildren<MeshRenderer>().GetMaterial().SetTexture("_MainTex", texture);

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
            On.RoR2.EquipmentSlot.MyFixedUpdate += EquipmentSlot_MyFixedUpdate;
            
            On.RoR2.EquipmentSlot.UpdateTargets += UpdateTargets;
            

            CompatHook();

        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void CompatHook()
        {
            if (ModCompatability.ProperSaveCompat.IsProperSaveInstalled && ModCompatability.ProperSaveCompat.AddProperSaveFunctionality)
            {
                ModCompatability.FinishedLoadingCompatability += ModCompatability.ProperSaveCompat.Hooks;
            }
        }

        private void EquipmentSlot_MyFixedUpdate(On.RoR2.EquipmentSlot.orig_MyFixedUpdate orig, EquipmentSlot self, float deltaTime)
        {
            orig(self,deltaTime);
            if (self.equipmentIndex == EarthTotem.instance.EquipmentDef.equipmentIndex)
            {
                var cpt = self.characterBody.master.GetComponent<EarthTotemTracker>();
                if (!cpt) cpt = self.characterBody.master.gameObject.AddComponent<EarthTotemTracker>();

                if (cpt.Firing == true)
                {

                    cpt.FireSequence(self);
                    //if(fireNext!=null)
                    //{

                    // }
                    // else if(cpt.Firing ==false)
                    // {

                    //cpt.StopFire();
                    // }
                }
            }
        }
        

        

        public static float CalcAdditionalCooldownComplex(float numOfAbsorb, float highestCooldown)
        {
            //For Number of Items Y=MaxCooldownFromNumberOfAbsorbtions*(1-e^(-x/3))
            //For Highest Cooldown Y=HighestCooldown/3
            float numOfItemsCooldown = (float)(45 * (1 - Math.Exp(-(double)numOfAbsorb / 4)));
            float highestCooldownCooldown = (float)highestCooldown / 1.25f;
            return numOfItemsCooldown + highestCooldownCooldown;
        }

        public static float CalcAdditionalCooldownByAbsorb(float numOfAbsorb)
        {
            //Y=(140-35)(1-exp(-(x-1)/5.3))+35-Cooldown;
            //y=(180-45)(1-exp(-(x-1)/10))+45-cooldown
            return (280f-45f)*(1f- (float)Math.Exp(-((double)numOfAbsorb-1)/6.6f))+45-(float)instance.Cooldown;
        }

        //private void FixedUpdate(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, EquipmentSlot self)
        //{
        //    orig(self);
        //    if (self.equipmentIndex == EarthTotem.instance.EquipmentDef.equipmentIndex)
        //    {
        //        var cpt = self.characterBody.master.GetComponent<EarthTotemTracker>();
        //        if (!cpt) cpt = self.characterBody.master.gameObject.AddComponent<EarthTotemTracker>();

        //        if (cpt.Firing == true)
        //        {

        //            cpt.FireSequence(self);
        //            //if(fireNext!=null)
        //            //{

        //            // }
        //            // else if(cpt.Firing ==false)
        //            // {

        //            //cpt.StopFire();
        //            // }
        //        }
        //    }
        //}

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            //See if a pikcupController is being targetted
            if (slot.currentTarget.pickupController
                && Run.instance)
            {

                //PIKCUP TO EQUIPMENT DINDEX
                PickupIndex pickupIndex = slot.currentTarget.pickupController.pickupIndex;
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                EquipmentIndex equipIndex = pickupDef.equipmentIndex;

                //If its not an equipment, dont run return false;
                if (equipIndex.Equals(EquipmentIndex.None))
                {
                    return false;
                }
                EquipmentDef EquipDef = EquipmentCatalog.GetEquipmentDef(equipIndex); 

                //Look for EarthTotemTracker, if its not there, add it.
                var cpt = slot.characterBody.master.GetComponent<EarthTotemTracker>();
                if (!cpt) cpt = slot.characterBody.master.gameObject.AddComponent<EarthTotemTracker>();

                //Check for Equipment exceptions:
                bool SkipAbsorbtion = false;
                if(equipIndex==EarthTotem.instance.EquipmentDef.equipmentIndex)
                {
                    SkipAbsorbtion = true;
                    cpt.EarthTotemAbsorbedCount++;
                }

                //Add Absorbed Item to list if SkipAbsorbtion flag wasnt triggered
                if(!SkipAbsorbtion)
                    cpt.AddToList(EquipDef);

                //Destroy the pickup
                GameObject.Destroy(slot.currentTarget.rootObject);

                //Start the Firing Sequence
                slot.subcooldownTimer = cpt.TimeUntilNextFire * cpt.EquipDefList.Count;
                cpt.StartFire();
                return true;
            }
            else if(true && Run.instance)
            {
                //Look for EarthTotemTracker, if its not there, add it.
                var cpt = slot.characterBody.master.GetComponent<EarthTotemTracker>();
                if (!cpt) cpt = slot.characterBody.master.gameObject.AddComponent<EarthTotemTracker>();

                //If the list is empty dont trigger equipment
                if (cpt.EquipDefList.Count == 0)
                    return false;

                //Start the Firing Sequence
                slot.subcooldownTimer = cpt.TimeUntilNextFire * cpt.EquipDefList.Count;
                cpt.StartFire();
                return true;
            }
            return false;
        }
        private void UpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            if (targetingEquipmentIndex != EquipmentDef.equipmentIndex)
            {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
                return;
            }
            self.currentTarget = new UserTargetInfo(self.FindPickupController(self.GetAimRay(), 10f, 30f, requireLoS: true, true));

            //Check to see if the master has the EarthTotemTracker, if not, add it.
            var cpt = self.characterBody.master.GetComponent<EarthTotemTracker>();
            if (!cpt) cpt = self.characterBody.master.gameObject.AddComponent<EarthTotemTracker>();

            var targetingComponent = self.GetComponent<TargetingControllerComponent>();
            if (self.currentTarget.rootObject != null)
            {
                //PIKCUP TO EQUIPMENT DINDEX
                PickupIndex pickupIndex = self.currentTarget.pickupController.pickupIndex;
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                EquipmentIndex equipIndex = pickupDef.equipmentIndex;


                
                //See if Equipment is being targetted
                if (!equipIndex.Equals(EquipmentIndex.None))
                {
                    self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
                    self.targetIndicator.active = true;
                    self.targetIndicator.targetTransform = self.currentTarget.transformToIndicateAt;
                }
                else
                {
                    //self.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerBadIndicator");
                }

                
            }
            else
            {
                self.targetIndicator.active = false;
            }
        }

        

    }

    public class EarthTotemTracker : MonoBehaviour
    {
        [DataMember(Name ="EquipList")]
        public List<EquipmentDef> EquipDefList;
        [DataMember(Name = "EarthAbsorbedCount")]
        public int EarthTotemAbsorbedCount;

        [IgnoreDataMember]
        public CharacterMaster Master => this.gameObject.GetComponent<CharacterMaster>();
        [IgnoreDataMember]
        public float FireFrequency;
        [IgnoreDataMember]
        public float TimeUntilNextFire;
        [IgnoreDataMember]
        public bool Firing;
        [IgnoreDataMember]
        public int EquipDefFireIndex;
        [IgnoreDataMember]
        public float HighestCooldown;

        
        //public Transform groundTarget;

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void Awake()
        {
            EquipDefList = new List<EquipmentDef>();
            FireFrequency = .25f; //in seconds
            Firing = false;
            EquipDefFireIndex = -1;
            TimeUntilNextFire = -1;
            HighestCooldown = 0;
            EarthTotemAbsorbedCount = 0;
        }

        public void StartFire()
        {
            Firing = true;
            EquipDefFireIndex = 0;
            TimeUntilNextFire = FireFrequency+Time.fixedDeltaTime;
        }

        public void StopFire()
        {
            Firing = false;
            EquipDefFireIndex = -1;
            TimeUntilNextFire = -1;
        }

        public void AddToList(EquipmentDef ED)
        {
            EquipDefList.Add(ED);
            if (HighestCooldown < ED.cooldown)
                HighestCooldown = ED.cooldown;
        }

        public EquipmentDef FireSequence(EquipmentSlot self)
        {
            if (Firing == true)
            {
                TimeUntilNextFire = Mathf.Max(TimeUntilNextFire - Time.fixedDeltaTime, 0f);
                if (TimeUntilNextFire <= 0)
                {
                    TimeUntilNextFire = FireFrequency + Time.fixedDeltaTime;
                    EquipmentDef equipDef = EquipDefList[EquipDefFireIndex];
                    EquipDefFireIndex++;
                    
                    if(EquipDefFireIndex>EquipDefList.Count-1)
                    {
                        //NEGATIVE to ADD to cooldown!
                        //ModLogger.LogInfo("Count Absorbed:" + EquipDefList.Count + "   Count Absorbed:" + HighestCooldown + "Cooldown Added:" + EarthTotem.CalcAdditionalCooldownComplex(EquipDefList.Count, HighestCooldown));
                        float cooldownAdded = EarthTotem.CalcAdditionalCooldownByAbsorb(Math.Max(EquipDefList.Count-EarthTotemAbsorbedCount,1));

                        //Adjust cooldown by gesture of drowned and fuelcells
                        int gesture = self.inventory.GetItemCount(RoR2Content.Items.AutoCastEquipment);
                        if (gesture > 0)
                            gesture += self.inventory.GetItemCount(RoR2Content.Items.EquipmentMagazine);

                        //Do Gesture Math
                        float adjustedCooldownAdded = cooldownAdded * .5f * (float)Math.Pow(.75f, gesture - 1);
                        self.characterBody.inventory.DeductActiveEquipmentCooldown(-adjustedCooldownAdded);
                        StopFire();
                    }
                    self.UpdateTargets(equipDef.equipmentIndex, false);
                    self.PerformEquipmentAction(equipDef);
                }
                else
                {
                    
                }
            }
            
            return null;
        }


    }

    //public class EarthTotemAbsorbHandler : MonoBehaviour
    //{
    //    public bool isAbsorbed = false;
    //    public bool queuedDeactivate = false;
    //    public Dictionary<GameObject, Vector3> auxiliaryPackedObjects = new();

    //    public void CollectAuxiliary(GameObject[] auxOverride)
    //    {
    //        auxiliaryPackedObjects.Clear();

    //        if (auxOverride != null && auxOverride.Length > 0)
    //        {
    //            foreach (var obj in auxOverride)
    //            {
    //                if (!obj) continue;
    //                auxiliaryPackedObjects.Add(obj, obj.transform.position - transform.position);
    //            }
    //        }
    //        else
    //        {
    //            var shopcpt = gameObject.GetComponent<MultiShopController>();
    //            if (shopcpt && shopcpt._terminalGameObjects != null)
    //            {
    //                foreach (var terminal in shopcpt._terminalGameObjects)
    //                {
    //                    if (!terminal) continue;
    //                    auxiliaryPackedObjects.Add(terminal, terminal.transform.position - transform.position);
    //                }
    //            }
    //            var healcpt = gameObject.GetComponent<ShrineHealingBehavior>();
    //            if (healcpt && healcpt.wardInstance != null)
    //            {
    //                auxiliaryPackedObjects.Add(healcpt.wardInstance, healcpt.wardInstance.transform.position - transform.position);
    //            }
    //        }
    //    }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        //void LateUpdate()
        //{
        //    if (queuedDeactivate)
        //    {
        //        queuedDeactivate = false;
        //        isAbsorbed = true;
        //        var loc = gameObject.GetComponentInChildren<ModelLocator>();
        //        if (loc)
        //            loc.modelTransform.gameObject.SetActive(false);
        //        foreach (var obj in auxiliaryPackedObjects)
        //        {
        //            obj.Key.SetActive(false);
        //            loc = obj.Key.GetComponentInChildren<ModelLocator>();
        //            if (loc)
        //                loc.modelTransform.gameObject.SetActive(false);
        //        }
        //        gameObject.SetActive(false);
        //    }
        //}

        /*
        public bool TryFireServer(EarthTotemTracker from, Vector3 pos)
        {
            if (!NetworkServer.active)
            {
                ModLogger.LogError("PackBoxHandler.TryPlaceServer called on client");
                return false;
            }
            if (!from || from.EquipDefList != gameObject)
            {
                ModLogger.LogError("PackBoxHandler.TryPlaceServer called on null PackBoxTracker, or this PackBoxHandler was not contained in it");
                return false;
            }

            new PackBox.MsgPackboxPlace(gameObject, from, pos).Send(R2API.Networking.NetworkDestination.Clients);

            return true;
        }

        public void FireClient(Vector3 pos)
        {
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutBoom"), new EffectData
            {
                origin = pos,
                rotation = transform.rotation
            }, true);
        }

        public void FireGlobal(EarthTotemTracker from, Vector3 pos)
        {
            var body = GetComponent<CharacterBody>();
            if (body && body.master)
                transform.position =
                    body.master.CalculateSafeGroundPosition(pos, body)
                    + (body.corePosition - body.footPosition);
            else transform.position = pos;
            gameObject.SetActive(true);
            var singleLoc = gameObject.GetComponentInChildren<ModelLocator>();
            if (singleLoc)
                singleLoc.modelTransform.gameObject.SetActive(true);
            foreach (var aux in auxiliaryPackedObjects)
            {
                aux.Key.transform.position = pos + aux.Value;
                aux.Key.SetActive(true);
                var locs = aux.Key.gameObject.GetComponentsInChildren<ModelLocator>();
                foreach (var loc in locs)
                {
                    loc.modelTransform.gameObject.SetActive(true);
                }
            }
            from.EquipDefList = null;
            isAbsorbed = false;
            if (NetworkClient.active)
                FireClient(pos);
        }

        public bool TryAbsorbServer(EarthTotemTracker into)
        {
            if (!NetworkServer.active)
            {
                ModLogger.LogError("PackBoxHandler.TryPackServer called on client");
                return false;
            }
            if (!into)
            {
                ModLogger.LogError("PackBoxHandler.TryPackServer called on null PackBoxTracker");
                return false;
            }

            new PackBox.MsgPackboxPack(gameObject, into, auxiliaryPackedObjects.Select(x => x.Key).ToArray()).Send(R2API.Networking.NetworkDestination.Clients);
            return true;
        }

        public void AbsorbClient()
        {
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TeleportOutBoom"), new EffectData
            {
                origin = transform.position,
                rotation = transform.rotation
            }, true);
        }

        public void AbsorbGlobal(EarthTotemTracker into, GameObject[] auxOverride)
        {
            DirectorCore.instance.RemoveAllOccupiedNodes(gameObject);
            into.EquipDefList = gameObject;
            queuedDeactivate = true;
            CollectAuxiliary(auxOverride);
            if (NetworkClient.active)
                AbsorbClient();
        }
        */
    //}


}

