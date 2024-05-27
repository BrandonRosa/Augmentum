﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace BransItems.Modules.Utils
{
    internal class ItemHelpers
    {
        /// <summary>
        /// A helper that will set up the RendererInfos of a GameObject that you pass in.
        /// <para>This allows it to go invisible when your character is not visible, as well as letting overlays affect it.</para>
        /// </summary>
        /// <param name="obj">The GameObject/Prefab that you wish to set up RendererInfos for.</param>
        /// <param name="debugmode">Do we attempt to attach a material shader controller instance to meshes in this?</param>
        /// <returns>Returns an array full of RendererInfos for GameObject.</returns>
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj,bool IgnoreOverlays=false, bool debugmode = false)
        {

            List<Renderer> AllRenderers = new List<Renderer>();

            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0) { AllRenderers.AddRange(meshRenderers); }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length > 0) { AllRenderers.AddRange(skinnedMeshRenderers); }

            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[AllRenderers.Count];

            for (int i = 0; i < AllRenderers.Count; i++)
            {
                if (debugmode)
                {
                   // var controller = AllRenderers[i].gameObject.AddComponent<MaterialControllerComponents.HGControllerFinder>();
                    //controller.Renderer = AllRenderers[i];
                }

                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = AllRenderers[i] is SkinnedMeshRenderer ? AllRenderers[i].sharedMaterial : AllRenderers[i].material,
                    renderer = AllRenderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

        /// <summary>
        /// A complicated helper method that sets up strings entered into it to be formatted similar to Risk of Rain 1's manifest style.
        /// </summary>
        /// <param name="deviceName">Name of the Item or Equipment</param>
        /// <param name="estimatedDelivery">MM/DD/YYYY</param>
        /// <param name="sentTo">Specific Location, Specific Area, General Area. E.g. Neptune's Diner, Albatross City, Neptune.</param>
        /// <param name="trackingNumber">An order number. E.g. 667********</param>
        /// <param name="devicePickupDesc">The short description of the item or equipment.</param>
        /// <param name="shippingMethod">Specific instructions on how to handle it, delimited by /. E.g. Light / Fragile</param>
        /// <param name="orderDetails">The actual lore snippet for the item or equipment.</param>
        /// <returns>A string formatted with all of the above in the style of Risk of Rain 1's manifests.</returns>
        public static string OrderManifestLoreFormatter(string deviceName, string estimatedDelivery, string sentTo, string trackingNumber, string devicePickupDesc, string shippingMethod, string orderDetails)
        {
            string[] Manifest =
            {
                $"<align=left>Estimated Delivery:<indent=70%>Sent To:</indent></align>",
                $"<align=left>{estimatedDelivery}<indent=70%>{sentTo}</indent></align>",
                "",
                $"<indent=1%><style=cIsDamage><size=125%><u>  Shipping Details:</u></size></style></indent>",
                "",
                $"<indent=2%>-Order: <style=cIsUtility>{deviceName}</style></indent>",
                $"<indent=4%><style=cStack>Tracking Number:  {trackingNumber}</style></indent>",
                "",
                $"<indent=2%>-Order Description: {devicePickupDesc}</indent>",
                "",
                $"<indent=2%>-Shipping Method: <style=cIsHealth>{shippingMethod}</style></indent>",
                "",
                "",
                "",
                $"<indent=2%>-Order Details: {orderDetails}</indent>",
                "",
                "",
                "",
                "<style=cStack>Delivery being brought to you by the brand new </style><style=cIsUtility>Orbital Drop-Crate System (TM)</style>. <style=cStack><u>No refunds.</u></style>"
            };
            return String.Join("\n", Manifest);
        }

        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0) { return; }
            foreach (var buff in body.timedBuffs)
            {
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    buff.timer = duration;
                }
            }
        }

        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float taperStart, float taperDuration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0) { return; }
            int i = 0;
            foreach (var buff in body.timedBuffs)
            {
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    buff.timer = taperStart + i * taperDuration;
                    i++;
                }
            }
        }

        public static void AddBuffAndDot(BuffDef buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != RoR2.DotController.DotIndex.None)
                {
                    RoR2.DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    if (NetworkServer.active)
                    {
                        body.AddTimedBuff(buff.buffIndex, duration);
                    }
                }
            }
        }

        public static DotController.DotIndex FindAssociatedDotForBuff(BuffDef buff)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);

            return index;
        }

        public static  T[] GetRandomSelectionFromArray<T>(List<T> itemList, int maxCount, Xoroshiro128Plus rng)
        {
            int selectionSize = Math.Min(itemList.Count,maxCount);
            T[] selection = new T[selectionSize];
            HashSet<T> usedItems = new HashSet<T>();

            for(int i=0;i<selectionSize;i++)
            {
                T selectedItem;
                do
                {
                    selectedItem = itemList[rng.RangeInt(0, itemList.Count)];
                } while (usedItems.Contains(selectedItem));
                selection[i] = selectedItem;
                usedItems.Add(selectedItem);
            }
            //(T[])Convert.ChangeType(value, typeof(T[]));
            return selection;
        }

        public static List<ItemDef> ItemDefsWithTier(ItemTierDef itemTierDef)
        {
            HashSet<ItemDef > items = new HashSet<ItemDef>();
            foreach (ItemDef itemDef in ItemCatalog.allItemDefs)
            {
                if (itemDef.itemIndex != ItemIndex.None && itemDef.tier == itemTierDef.tier) //&& !itemDef.tags.Contains(ItemTag.WorldUnique))
                {
                    items.Add(itemDef);
                }
            }
            return items.ToList<ItemDef>();
        }

        public static List<PickupDef> PickupDefsWithTier(ItemTierDef itemTierDef)
        {
            HashSet<PickupDef> items = new HashSet<PickupDef>();
            foreach (PickupDef pickupDef in PickupCatalog.allPickups)
            {
                if (pickupDef.itemIndex!= ItemIndex.None && pickupDef.itemTier == itemTierDef.tier)// && !itemDef.tags.Contains(ItemTag.WorldUnique))
                {
                    items.Add(pickupDef);
                }
            }
            return items.ToList<PickupDef>();
        }

        //public static PickupIndex[] ItemIndexToPickupIndex
    }
}
