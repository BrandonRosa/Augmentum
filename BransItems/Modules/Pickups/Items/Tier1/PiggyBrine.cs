using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BransItems.Modules.Utils;
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;
using BepInEx.Configuration;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using BransItems.Modules.Pickups.Items.NoTier;

namespace BransItems.Modules.Pickups.Items.Tier1
{
    class PiggyBrine : ItemBase<PiggyBrine>
    {
        public override string ItemName => "Piggy Brine";
        public override string ItemLangTokenName => "PIG_JAR";
        public override string ItemPickupDesc => "Increase regen. Breaks at low health.";
        public override string ItemFullDescription => $"Increases <style=cIsHealing>health regeneration</style> by <style=cIsHealing>{RegenPercent*100}%</style><style=cStack>(+{RegenPercent*100}% per stack)</style> <style=cIsHealing>plus</style> an additional <style=cIsHealing>+{RegenBase} hp/s</style><style=cStack>(+{RegenBase} hp/s per stack)</style>. Taking damage to below <style=cIsHealth>25% health</style> <style=cIsUtility>breaks</style> this item. ";

        public override string ItemLore => $"";

        public override ItemTier Tier => ItemTier.Tier1;

        public override GameObject ItemModel => SetupModel();

        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/Piggy/PigJar.png");

        public static GameObject ItemBodyModelPrefab;

        public override bool Hidden => false;

        public override bool CanRemove => true;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Healing, ItemTag.LowHealth };

        public static float RegenPercent = .15f;
        public static float RegenBase = 1f;


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
            GameObject TempItemModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ExplodeOnDeath/PickupWilloWisp.prefab").WaitForCompletion().InstantiateClone("PigJar", false);
            GameObject WispRemove = TempItemModel.transform.GetChild(0).GetChild(2).gameObject;
            GameObject.Destroy(WispRemove.GetComponent<ParticleSystem>());

            GameObject ItemMesh = MainAssets.LoadAsset<GameObject>("Assets/Models/Piggy/LowPolyPig.prefab");
            MeshRenderer MR=WispRemove.AddComponent<MeshRenderer>(); 
            MR.SetMaterial(ItemMesh.GetComponentInChildren<MeshRenderer>().GetMaterial());
            MeshFilter MF = WispRemove.AddComponent<MeshFilter>();
            MF.mesh = ItemMesh.GetComponentInChildren<MeshFilter>().mesh;
            WispRemove.transform.Rotate(new Vector3(0,0,-90));
            WispRemove.transform.position= WispRemove.transform.position- .18f*Vector3.up;

            GameObject Jar = TempItemModel.transform.GetChild(0).GetChild(1).gameObject.InstantiateClone("PigLiquid");
            MeshRenderer JarRender = Jar.GetComponent<MeshRenderer>();
            Material mat = new Material(JarRender.GetMaterialArray()[1]);
            mat.SetTexture("_MainTex", null); 
            mat.SetTexture("_RemapTex", null);
            mat.SetColor("_TintColor", new Color32(230, 56, 68, 255));
            mat.SetFloat("_IntersectionStrength", 20f);
            mat.renderQueue = 2000;

            Jar.GetComponent<MeshRenderer>().SetMaterials(new Material[] { mat }, 1);
            Jar.transform.localScale *= 1.2f;
            Jar.transform.position = TempItemModel.transform.GetChild(0).transform.position;
            Jar.transform.SetParent(TempItemModel.transform.GetChild(0).transform);


            return TempItemModel;

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
            On.RoR2.HealthComponent.UpdateLastHitTime += BreakItem;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.regenMultAdd += RegenPercent * GetCount(sender);
            args.baseRegenAdd += RegenBase * GetCount(sender);
        }

        private void BreakItem(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker)
        {
            orig.Invoke(self, damageValue, damagePosition, damageIsSilent, attacker);
            if (NetworkServer.active && (bool)self && (bool)self.body && GetCount(self.body) > 0 && self.isHealthLow)
            {
                int count = GetCount(self.body);
                self.body.inventory.RemoveItem(ItemDef,count);
                self.body.inventory.GiveItem(ShatteredPiggyBrine.instance.ItemDef, count);
                CharacterMasterNotificationQueue.PushItemTransformNotification(self.body.master, ItemDef.itemIndex, ShatteredPiggyBrine.instance.ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            }
        }
    }
}

