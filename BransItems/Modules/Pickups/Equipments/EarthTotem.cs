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
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace BransItems.Modules.Pickups.Equipments
{
    class EarthTotem : EquipmentBase<EarthTotem>
    {
        public override string EquipmentName => "Earth Totem";

        public override string EquipmentLangTokenName => "EARTH_TOTEM";

        public override string EquipmentPickupDesc => "On use, absorb an equipment and fire all absorbed equipments.";

        public override string EquipmentFullDescription => "On use, absorb an equipment and fire all absorbed equipments.";

        public override string EquipmentLore =>

            $"Found on a scrap of paper in an ornate case along with the device: \"[...] at that point, the binding process will become automatic, " +
            $"and all the user needs to do is sever the specimen's connection to its soul.\"";
        public override bool UseTargeting => true;

        public static GameObject ItemBodyModelPrefab;

        public override float Cooldown { get; } = 1;//140f;

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
            On.RoR2.EquipmentSlot.FixedUpdate += FixedUpdate;
            On.RoR2.EquipmentSlot.UpdateTargets += UpdateTargets;
        }

        private void FixedUpdate(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, EquipmentSlot self)
        {
            orig(self);
            var cpt = self.characterBody.GetComponent<EarthTotemTracker>();
            if (!cpt) cpt = self.characterBody.gameObject.AddComponent<EarthTotemTracker>();

            if (cpt.Firing ==true)
            {
                EquipmentDef fireNext = cpt.GetEquipDefDuringFiringSequence();
                if(fireNext!=null)
                {
                    self.UpdateTargets(fireNext.equipmentIndex, false);
                    self.PerformEquipmentAction(fireNext);
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            if (slot.currentTarget.pickupController
                && Run.instance)
            {

                //PIKCUP TO EQUIPMENT DINDEX
                PickupIndex pickupIndex = slot.currentTarget.pickupController.pickupIndex;
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                EquipmentIndex equipIndex = pickupDef.equipmentIndex;
                if (equipIndex.Equals(EquipmentIndex.None))
                {
                    return false;
                }
                EquipmentDef EquipDef = EquipmentCatalog.GetEquipmentDef(equipIndex); 

                var cpt = slot.characterBody.GetComponent<EarthTotemTracker>();
                if (!cpt) cpt = slot.characterBody.gameObject.AddComponent<EarthTotemTracker>();

                cpt.EquipDefList.Add(EquipDef);

                GameObject.Destroy(slot.currentTarget.rootObject);

                cpt.StartFire();
                return true;
            }
            else if(true && Run.instance)
            {
                var cpt = slot.characterBody.GetComponent<EarthTotemTracker>();
                if (!cpt) cpt = slot.characterBody.gameObject.AddComponent<EarthTotemTracker>();

                //foreach (EquipmentDef ED in cpt.EquipDefList)
                //slot.PerformEquipmentAction(ED);
                cpt.StartFire();
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

            var cpt = self.characterBody.GetComponent<EarthTotemTracker>();
            if (!cpt) cpt = self.characterBody.gameObject.AddComponent<EarthTotemTracker>();

            var targetingComponent = self.GetComponent<TargetingControllerComponent>();
            if (self.currentTarget.rootObject != null)
            {
                //PIKCUP TO EQUIPMENT DINDEX
                PickupIndex pickupIndex = self.currentTarget.pickupController.pickupIndex;
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                EquipmentIndex equipIndex = pickupDef.equipmentIndex;


                ModLogger.LogInfo("PickupIndex:" + pickupIndex.ToString());
                ModLogger.LogInfo("EquipIndex:" + equipIndex.ToString());
                //ModLogger.LogDebug("EquipDef:" + EquipmentCatalog.GetEquipmentDef(equipIndex).nameToken);
                if (!equipIndex.Equals(EquipmentIndex.None))
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

        

        /*
        public struct MsgEarthTotemAbsorb : INetMessage
        {
            GameObject _target;
            GameObject[] _aux;
            GameObject _owner;

            public void Deserialize(NetworkReader reader)
            {
                _target = reader.ReadGameObject();
                _owner = reader.ReadGameObject();
                _aux = new GameObject[reader.ReadInt32()];
                for (var i = 0; i < _aux.Length; i++)
                    _aux[i] = reader.ReadGameObject();
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(_target);
                writer.Write(_owner);
                writer.Write(_aux.Length);
                for (var i = 0; i < _aux.Length; i++)
                    writer.Write(_aux[i]);
            }

            public void OnReceived()
            {
                if (!_target)
                {
                    ModLogger.LogError($"Received MsgPackboxPack for null GameObject");
                    return;
                }
                if (!_owner)
                {
                    ModLogger.LogError($"Received MsgPackboxPack for null GameObject");
                    return;
                }

                var pbh = _target.GetComponent<EarthTotemAbsorbHandler>();
                if (!pbh) pbh = _target.AddComponent<EarthTotemAbsorbHandler>();

                var pbt = _owner.GetComponent<EarthTotemTracker>();
                if (!pbt) pbt = _owner.AddComponent<EarthTotemTracker>();

                pbh.AbsorbGlobal(pbt, _aux);
            }

            public MsgEarthTotemAbsorb(GameObject target, EarthTotemTracker owner, GameObject[] aux)
            {
                _target = target;
                _owner = owner.gameObject;
                _aux = aux;
            }
        }
        */

        /*
        public struct MsgPackboxPlace : INetMessage
        {
            GameObject _target;
            GameObject _owner;
            Vector3 _pos;

            public void Deserialize(NetworkReader reader)
            {
                _target = reader.ReadGameObject();
                _owner = reader.ReadGameObject();
                _pos = reader.ReadVector3();
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(_target);
                writer.Write(_owner);
                writer.Write(_pos);
            }

            public void OnReceived()
            {
                if (!_target)
                {
                    ModLogger.LogError($"Received MsgPackboxPlace for null GameObject");
                    return;
                }
                if (!_owner)
                {
                    ModLogger.LogError($"Received MsgPackboxPlace for null GameObject");
                    return;
                }

                var pbh = _target.GetComponent<EarthTotemAbsorbHandler>();
                var pbt = _owner.GetComponent<EarthTotemTracker>();

                if (!pbh || !pbt)
                {
                    ModLogger.LogError($"MsgPackboxPlace has an invalid GameObject (names: {_target.name} {_owner.name})");
                    return;
                }

                pbh.FireGlobal(pbt, _pos);
            }

            public MsgPackboxPlace(GameObject target, EarthTotemTracker owner, Vector3 pos)
            {
                _target = target;
                _owner = owner.gameObject;
                _pos = pos;
            }
        }
        */
    }

    public class EarthTotemTracker : MonoBehaviour
    {
        public List<EquipmentDef> EquipDefList;
        public float FireFrequency;
        public float TimeUntilNextFire;
        public bool Firing;
        public int EquipDefFireIndex;
        //public Transform groundTarget;

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void Awake()
        {
            EquipDefList = new List<EquipmentDef>();
            FireFrequency = .25f; //in seconds
            Firing = false;
            EquipDefFireIndex = -1;
            TimeUntilNextFire = -1;
        }

        public void StartFire()
        {
            Firing = true;
            EquipDefFireIndex = 0;
            TimeUntilNextFire = FireFrequency;
        }

        void StopFire()
        {
            Firing = false;
            EquipDefFireIndex = -1;
            TimeUntilNextFire = -1;
        }

        void Update()
        {

        }

        public EquipmentDef GetEquipDefDuringFiringSequence()
        {
            if (Firing == true)
            {
                TimeUntilNextFire = Mathf.Max(TimeUntilNextFire - Time.fixedDeltaTime, 0f);
                if (TimeUntilNextFire <= 0)
                {
                    EquipmentDef equipDef = EquipDefList[EquipDefFireIndex];
                    EquipDefFireIndex++;
                    if(EquipDefFireIndex>EquipDefList.Count-1)
                    {
                        StopFire();
                    }
                    return equipDef;
                }
                else
                {
                    
                }
            }
            
            return null;
        }
    }

    public class EarthTotemAbsorbHandler : MonoBehaviour
    {
        public bool isAbsorbed = false;
        public bool queuedDeactivate = false;
        public Dictionary<GameObject, Vector3> auxiliaryPackedObjects = new();

        public void CollectAuxiliary(GameObject[] auxOverride)
        {
            auxiliaryPackedObjects.Clear();

            if (auxOverride != null && auxOverride.Length > 0)
            {
                foreach (var obj in auxOverride)
                {
                    if (!obj) continue;
                    auxiliaryPackedObjects.Add(obj, obj.transform.position - transform.position);
                }
            }
            else
            {
                var shopcpt = gameObject.GetComponent<MultiShopController>();
                if (shopcpt && shopcpt._terminalGameObjects != null)
                {
                    foreach (var terminal in shopcpt._terminalGameObjects)
                    {
                        if (!terminal) continue;
                        auxiliaryPackedObjects.Add(terminal, terminal.transform.position - transform.position);
                    }
                }
                var healcpt = gameObject.GetComponent<ShrineHealingBehavior>();
                if (healcpt && healcpt.wardInstance != null)
                {
                    auxiliaryPackedObjects.Add(healcpt.wardInstance, healcpt.wardInstance.transform.position - transform.position);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        void LateUpdate()
        {
            if (queuedDeactivate)
            {
                queuedDeactivate = false;
                isAbsorbed = true;
                var loc = gameObject.GetComponentInChildren<ModelLocator>();
                if (loc)
                    loc.modelTransform.gameObject.SetActive(false);
                foreach (var obj in auxiliaryPackedObjects)
                {
                    obj.Key.SetActive(false);
                    loc = obj.Key.GetComponentInChildren<ModelLocator>();
                    if (loc)
                        loc.modelTransform.gameObject.SetActive(false);
                }
                gameObject.SetActive(false);
            }
        }

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
    }


}

