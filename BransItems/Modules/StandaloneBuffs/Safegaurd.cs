using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using static Augmentum.Augmentum;
using static Augmentum.Modules.Utils.ItemHelpers;
using RoR2;

namespace Augmentum.Modules.StandaloneBuffs
{
    public class Safegaurd : BuffBase<Safegaurd>
    {
        public override string BuffName => "Safegaurd";

        public override Color Color => new Color32(255, 215, 0, 255);

        //public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("DoubleGoldDoubleXPBuffIcon.png");
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
            if (self)
            {
                if (self.body)
                {
                    int buffCount = self.body.GetBuffCount(BuffDef);
                    if (buffCount > 0)
                    {
                        float maxDamage = ((100f - (float)buffCount) / 100f) * self.fullCombinedHealth;
                        if (damageInfo.damage > maxDamage)
                            damageInfo.damage = maxDamage;

                    }
                }
            }
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