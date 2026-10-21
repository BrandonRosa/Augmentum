using Augmentum.Modules.ItemTiers;
using RoR2;
using System;
using UnityEngine;

namespace Augmentum.Modules.Utils
{
    /// <summary>
    /// A monobehaviour that displays a custom ItemTier pickup display
    /// 1000% stolen from moonstorm
    /// </summary>
    public class ItemTierPickupVFXHelper : MonoBehaviour
    {
        [SystemInitializer(new Type[] { typeof(ItemTierCatalog), typeof(ItemTierBase) })]
        public static void SystemInitializer()
        {
            On.RoR2.PickupDisplay.DestroyModel += PickupDisplay_DestroyModel;
            On.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
        }

        private static void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self, GameObject modelObjectOverride)
        {
            ItemTierPickupVFXHelper ITPVFXHelper = self.gameObject.GetComponent<ItemTierPickupVFXHelper>();
            if(!ITPVFXHelper)
            {
                self.gameObject.AddComponent<ItemTierPickupVFXHelper>();
                ITPVFXHelper = self.gameObject.GetComponent<ItemTierPickupVFXHelper>();
            }
            orig(self, modelObjectOverride);
            ITPVFXHelper.OnPickupDisplayRebuildModel();
        }

        private static void PickupDisplay_DestroyModel(On.RoR2.PickupDisplay.orig_DestroyModel orig, PickupDisplay self)
        {
            ItemTierPickupVFXHelper ITPVFXHelper = self.gameObject.GetComponent<ItemTierPickupVFXHelper>();
            if (!ITPVFXHelper)
            {
                self.gameObject.AddComponent<ItemTierPickupVFXHelper>();
                ITPVFXHelper = self.gameObject.GetComponent<ItemTierPickupVFXHelper>();
            }
            orig(self);
            ITPVFXHelper.OnPickupDisplayDestroyModel();
        }

        private PickupDisplay display;
        private GameObject effectInstance;

        private void Awake()
        {
            display = GetComponent<PickupDisplay>();
        }

        private void OnPickupDisplayRebuildModel()
        {
            //Augmentum.ModLogger.LogWarning("REBUILD");
            if (!display)
                return;

            PickupDef pickupDef = PickupCatalog.GetPickupDef(display.GetPickupIndex());
            ItemIndex itemIndex = pickupDef?.itemIndex ?? ItemIndex.None;
            if (itemIndex != ItemIndex.None)
            {
                ItemTier itemTier = ItemCatalog.GetItemDef(itemIndex).tier;
                ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemTier);
                if (itemTierDef && ItemTierBase.IsBransCustomTier(itemTierDef, out var itemTierBase))
                {
                    if (itemTierBase != null && itemTierBase.PickupDisplayVFX)
                    {
                        //Augmentum.ModLogger.LogWarning("Final");
                        effectInstance = Instantiate(itemTierBase.PickupDisplayVFX, display.gameObject.transform);
                        effectInstance.transform.position -= Vector3.up*1.5f;
                        effectInstance.SetActive(true);

                        Color32 color = ColorCatalog.GetColor(itemTierDef.colorIndex);

                        ParticleSystem[] array = effectInstance.GetComponentsInChildren<ParticleSystem>();
                        foreach (ParticleSystem obj in array)
                        {
                            ((Component)obj).gameObject.SetActive(true);
                            ParticleSystem.MainModule main = obj.main;
                            main.startColor =new ParticleSystem.MinMaxGradient(color);
                        }
                    }
                }
            }
        }

        private void OnPickupDisplayDestroyModel()
        {
            //Augmentum.ModLogger.LogWarning("Destroy");
            if (effectInstance)
            {
                Destroy(effectInstance);
            }
        }
    }
}