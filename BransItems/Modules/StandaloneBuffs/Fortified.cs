using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;
using RoR2;

namespace BransItems.Modules.StandaloneBuffs
{
    public class Fortified : BuffBase<Fortified>
    {
        public override string BuffName => "Fortified";

        public override Color Color => new Color32(245, 245, 245, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Textrures/Icons/Buff/Fortified.png");
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
                    self.itemCounts.armorPlate += self.body.GetBuffCount(BuffDef);
                    //ModLogger.LogWarning("Blocked" + self.body.GetBuffCount(BuffDef)+"    CanStack"+instance.CanStack);
                    orig(self, damageInfo);
                    usedOrig = true;
                    self.itemCounts.armorPlate -= self.body.GetBuffCount(BuffDef);
                }
            }
            //if Orig wasnt called, call it
            if(!usedOrig)
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