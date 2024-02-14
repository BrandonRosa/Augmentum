using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;
using RoR2;
using UnityEngine.AddressableAssets;

namespace BransItems.Modules.StandaloneBuffs
{
    public class Laceration : BuffBase<Laceration>
    {
        public override string BuffName => "Laceration"; //RoR2/Base/ArmorReductionOnHit/texBuffPulverizeIcon.tif

        public override Color Color => new Color32(240, 0, 0, 255);

        public override Sprite BuffIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ArmorReductionOnHit/texBuffPulverizeIcon.tif").WaitForCompletion();
        public override bool CanStack => true;
        public override bool IsDebuff => false;

        public override bool IsCooldown => false;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            //RoR2Application.onLoad += OnLoadModCompat;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool usedOrig = false;
            if (self)
            {
                if (self.body && self.body.GetBuffCount(BuffDef) > 0)
                {
                    ModLogger.LogWarning("Damage:" + damageInfo.damage);
                    damageInfo.damage += (float)self.body.GetBuffCount(BuffDef)*.1f; //Multiply by 2 cuz damage is wierd???
                    orig(self, damageInfo);
                    usedOrig = true;
                    
                }
            }
            //if Orig wasnt called, call it
            if (!usedOrig)
                orig(self, damageInfo);
        }

        private void OnLoadModCompat()
        {
            //if (Compatability.ModCompatability.BetterUICompat.IsBetterUIInstalled)
            //{
            //    var buffInfo = CreateBetterUIBuffInformation($"AETHERIUM_DOUBLE_GOLD_DOUBLE_XP_BUFF", BuffName, "All kills done by you grant double gold and double xp to you.");
            //    RegisterBuffInfo(BuffDef, buffInfo.Item1, buffInfo.Item2);
            //}

            //if (Aetherium.Interactables.BuffBrazier.instance != null)
            //{
            //    AddCuratedBuffType("Double Gold and Double XP", BuffDef, Color, 1.25f, false);
            //}
        }

    }

}