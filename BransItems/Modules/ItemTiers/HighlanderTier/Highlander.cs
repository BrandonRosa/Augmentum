using System;
using System.Collections.Generic;
using System.Text;
using Augmentum.Modules.ColorCatalogEntry;
//using Augmentum.Modules.ColorCatalogEntry.CoreColors;
using Augmentum.Modules.ItemTiers;
using Augmentum.Modules.Pickups.Items.Essences;
using Augmentum.Modules.Utils;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Augmentum.Augmentum;

namespace Augmentum.Modules.ItemTiers.HighlanderTier
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

            CreateDropletPrefab();
            CreateVFXPrefab();
            CreateTier();
            //itemTierDef.highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            //itemTierDef.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();
            SetHooks();


        }
        private void CreateDropletPrefab()
        {
            GameObject Temp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/LunarOrb.prefab").WaitForCompletion().InstantiateClone("HighlanderOrb", true);
            //GameObject child1 =Temp.transform.GetChild(0).gameObject;
            Color colorLight = ColorCatalog.GetColor(colorIndex);
            Color colorDark = ColorCatalog.GetColor(darkColorIndex);

            Gradient gradient = new Gradient();

            // Blend color from red at 0% to blue at 100%
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(colorDark, 0.0f);
            colors[1] = new GradientColorKey(colorDark, 1.0f);
            //colors[0] = new GradientColorKey(Color.red, 0.0f);
            //colors[1] = new GradientColorKey(Color.red, 1.0f);

            // Blend alpha from opaque at 0% to transparent at 100%
            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphas[1] = new GradientAlphaKey(0.0f, 1.0f);

            gradient.SetKeys(colors, alphas);

            Temp.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().startColor = colorLight;
            Temp.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().set_startColor_Injected(ref colorLight);
            Temp.transform.GetChild(0).gameObject.GetComponent<TrailRenderer>().SetColorGradient(gradient);

            //Temp.transform.GetChild(0).GetChild(2).GetComponent<Light>().color = colorLight;
            //Temp.transform.GetChild(0).GetChild(2).GetComponent<Light>().set_color_Injected(ref colorLight);
            Light[] lights = Temp.GetComponentsInChildren<Light>();
            foreach (Light thisLight in lights)
            {
                thisLight.color = colorLight;
            }

            ParticleSystem[] array = Temp.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem obj in array)
            {
                //((Component)obj).gameObject.SetActive(true);
                ParticleSystem.MainModule main = obj.main;
                ParticleSystem.ColorOverLifetimeModule COL = obj.colorOverLifetime;
                main.startColor = new ParticleSystem.MinMaxGradient(colorLight);
                COL.color = colorLight;
            }
            //Temp.GetComponentInChildren<Light>().set_color_Injected(ref colorLight);
            dropletDisplayPrefab = Temp;

        }

        private void CreateVFXPrefab()
        {
            Color colorLight = ColorCatalog.GetColor(colorIndex);
            Color colorDark = ColorCatalog.GetColor(darkColorIndex);
            //GameObject Temp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/SetpiecePickup.prefab").WaitForCompletion();
            GameObject Temp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/GenericPickup.prefab").WaitForCompletion();
            GameObject VFX = Temp.transform.GetChild(11).gameObject.InstantiateClone("HighlanderVFX", false);

            //GameObject DistantGlow = VFX.transform.GetChild(0).transform.GetChild(0).gameObject; //Dark
            //GameObject Swirls = VFX.transform.GetChild(0).transform.GetChild(1).gameObject; //Light
            //GameObject PointLight = VFX.transform.GetChild(0).transform.GetChild(2).gameObject; //Light
            //GameObject Glowies = VFX.transform.GetChild(0).transform.GetChild(3).gameObject; //Light



            ParticleSystem.MainModule NewColor = VFX.transform.GetChild(0).GetChild(0).gameObject.GetComponent<ParticleSystem>().main; //Distant soft
            NewColor.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);


            ParticleSystem.MainModule NewColor2 = VFX.transform.GetChild(0).GetChild(1).gameObject.GetComponent<ParticleSystem>().main; //Swirls
            NewColor2.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);


            VFX.transform.GetChild(0).GetChild(2).gameObject.GetComponent<Light>().color = colorLight;
            VFX.transform.GetChild(0).GetChild(2).gameObject.GetComponent<Light>().set_color_Injected(ref colorLight);

            ParticleSystem.MainModule NewColor3 = VFX.transform.GetChild(0).GetChild(1).gameObject.GetComponent<ParticleSystem>().main; //Glowies
            NewColor3.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);

            ParticleSystem.MainModule NewColor4 = VFX.transform.GetChild(1).GetChild(0).gameObject.GetComponent<ParticleSystem>().main; //Flash
            NewColor4.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);

            ParticleSystem.MainModule NewColor5 = VFX.transform.GetChild(1).GetChild(1).gameObject.GetComponent<ParticleSystem>().main;//Rings
            NewColor5.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);


            PickupDisplayVFX = VFX;
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
            if (Conquest.instance.IsSelectedAndInRun())
                return;
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
            if (Conquest.instance.IsSelectedAndInRun())
                return;
            if (damageReport.victimBody.isElite)
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
            if (Conquest.instance.IsSelectedAndInRun())
                return;
            float playerScale = (BarrelScalePlayers ? Math.Max(1f, (float)Math.Sqrt(PlayerCharacterMasterController.instances.Count)) : 1);
            if (RoR2Application.rng.RangeFloat(0f, 1f) <= BarrelChance*playerScale)
            {
                DropItem(self.gameObject.transform, 5.5f);
            }
        }

        private void ShrineCombatBehavior_OnDefeatedServer(On.RoR2.ShrineCombatBehavior.orig_OnDefeatedServer orig, ShrineCombatBehavior self)
        {
            orig(self);
            if (Conquest.instance.IsSelectedAndInRun())
                return;
            float playerScale = (CombatShrineScalePlayers ? Math.Max(1f, (float)Math.Sqrt(PlayerCharacterMasterController.instances.Count)) : 1);
            if (RoR2Application.rng.RangeFloat(0f, 1f) <= CompatShrineChance*playerScale)
            {
                DropItem(self.gameObject.transform, 8.5f);
            }
        }

        private void DropItem(Transform location, float height)
        {
            if (Spoils.instance.IsSelectedAndInRun())
            {
                PickupDropletController.CreatePickupDroplet(Spoils.SpoilsPickupInfo(location.position + Vector3.up * height), location.position + Vector3.up * height, Vector3.up * 25f);
            }
            else
            {
                List<PickupDef> HighList = ItemHelpers.PickupDefsWithTier(this.itemTierDef);
                int picked = RoR2Application.rng.RangeInt(0, HighList.Count);

                PickupDef pickupDef = HighList[picked];
                PickupDropletController.CreatePickupDroplet(pickupDef.pickupIndex, location.position + Vector3.up * height, Vector3.up * 25f);
            }
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = "<color=#FAF7B9><size=120%>" + "You have been rewarded with a gift from the Highlands." + "</color></size>"
            });
        }

        private void CharacterMaster_OnItemAddedClient(On.RoR2.CharacterMaster.orig_OnItemAddedClient orig, CharacterMaster self, ItemIndex itemIndex)
        {
            
            orig(self, itemIndex); //JUST ADDED, adjust if it breaks stuff
            if (ItemCatalog.GetItemDef(itemIndex)._itemTierDef==itemTierDef)
            {
                int count = self.inventory.GetTotalItemCountOfTier(itemTierDef.tier);
               
                
                if (count >= 2)
                {

                    for (int i = 0; i < self.inventory.itemAcquisitionOrder.Count; i++)
                    {
                        ItemIndex temp = self.inventory.itemAcquisitionOrder[i];

                        if ((ItemCatalog.GetItemDef(temp)._itemTierDef == itemTierDef))
                        {
                            ItemIndex toss = self.inventory.itemAcquisitionOrder[i];
                            self.inventory.RemoveItemPermanent(toss);

                            Vector3 val = Vector3.up * 25f; //dropTransform.forward * dropForwardVelocityStrength;
                            PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(toss);

                            PickupDropletController.CreatePickupDroplet(pickupIndex, self.GetBody().transform.position + Vector3.up * 1.5f, val);
                            break;
                        }
                    }
                }
            }
        }
    }
}
