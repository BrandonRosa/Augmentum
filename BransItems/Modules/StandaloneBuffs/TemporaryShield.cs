using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using static BransItems.BransItems;
using static BransItems.Modules.Utils.ItemHelpers;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace BransItems.Modules.StandaloneBuffs
{
    public class TemporaryShield : BuffBase<TemporaryShield>
    {
        public override string BuffName => "Temporary Shield";

        public override Color Color => new Color32(68,94,182,255);

        public override Sprite BuffIcon => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffGenericShield.tif").WaitForCompletion();
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
            //On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            //RoR2Application.onLoad += OnLoadModCompat;
            //On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.baseShieldAdd += sender.GetBuffCount(BuffDef);
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