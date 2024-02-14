using BransItems.Modules.Compatability;
using BransItems.Modules.ItemTiers;
using RoR2;
using System;
using UnityEngine;

namespace BransItems.Modules.Utils
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

        private static void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, PickupDisplay self)
        {
            ItemTierPickupVFXHelper ITPVFXHelper = self.gameObject.GetComponent<ItemTierPickupVFXHelper>();
            if(!ITPVFXHelper)
            {
                self.gameObject.AddComponent<ItemTierPickupVFXHelper>();
                ITPVFXHelper = self.gameObject.GetComponent<ItemTierPickupVFXHelper>();
            }
            orig(self);
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
            BransItems.ModLogger.LogWarning("REBUILD");
            if (!display)
                return;

            PickupDef pickupDef = PickupCatalog.GetPickupDef(display.pickupIndex);
            ItemIndex itemIndex = pickupDef?.itemIndex ?? ItemIndex.None;
            if (itemIndex != ItemIndex.None)
            {
                ItemTier itemTier = ItemCatalog.GetItemDef(itemIndex).tier;
                ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(itemTier);
                if (itemTierDef && ItemTierBase.IsBransCustomTier(itemTierDef, out var itemTierBase))
                {
                    if (itemTierBase != null && itemTierBase.PickupDisplayVFX)
                    {
                        BransItems.ModLogger.LogWarning("Final");
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

                        if(ModCompatability.HighItemVizabilityCompat.IsHighItemVizabilityInstalled)
                        {
                            float h, s, v = 0;
                            Color.RGBToHSV(color, out h, out s, out v);
                            Color32 colorBright = Color.HSVToRGB(h, 1f, v); //maybe replace v with 1f
                            GameObject lineStart = display.gameObject;
                            Vector3 lineStartPos = display.transform.position;
                            Vector3 lineEndPos = lineStartPos + new Vector3(0f, 100f, 0f);
                            LineRenderer lineRender = lineStart.AddComponent<LineRenderer>();
                            lineRender.positionCount = 2;
                            ((Renderer)lineRender).material = new Material(LegacyShaderAPI.Find("Hopoo Games/FX/Vertex Colors Only"));

                            lineRender.startColor = colorBright;
                            lineRender.endColor = colorBright;

                            lineRender.startWidth = 0.25f;
                            lineRender.endWidth = 0f;
                            lineRender.SetPosition(0, lineStartPos);
                            lineRender.SetPosition(1, lineEndPos);
                            BransItems.ModLogger.LogWarning("NOTPINK");
                            
                        }
                    }
                }
            }
        }

        private void OnPickupDisplayDestroyModel()
        {
            BransItems.ModLogger.LogWarning("Destroy");
            if (effectInstance)
            {
                Destroy(effectInstance);
            }
        }
    }
}