using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using static Augmentum.Augmentum;
using static Augmentum.Modules.Utils.ItemHelpers;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Augmentum.Modules.StandaloneBuffs
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

        /// <summary>
        /// Grants <paramref name="amount"/> shield for <paramref name="duration"/> seconds.
        /// One stack is one point of shield, so this sets the stack count once instead of calling AddTimedBuff per point,
        /// which got very expensive (and flooded the network) when thousands of points were given at once.
        /// </summary>
        public static void AddTemporaryShield(CharacterBody body, int amount, float duration)
        {
            if (!NetworkServer.active || !body || amount <= 0 || duration <= 0f)
                return;

            var tracker = body.GetComponent<TemporaryShieldTracker>();
            if (!tracker)
                tracker = body.gameObject.AddComponent<TemporaryShieldTracker>();

            tracker.Add(amount, duration);
        }

        public class TemporaryShieldTracker : MonoBehaviour
        {
            private struct Grant
            {
                public int amount;
                public float expiresAt;
            }

            private readonly List<Grant> grants = new List<Grant>();
            private CharacterBody body;

            private void Awake()
            {
                body = GetComponent<CharacterBody>();
            }

            public void Add(int amount, float duration)
            {
                grants.Add(new Grant { amount = amount, expiresAt = Time.fixedTime + duration });
                ChangeCount(amount);
            }

            private void ChangeCount(int delta)
            {
                BuffIndex buffIndex = instance.BuffDef.buffIndex;
                body.SetBuffCount(buffIndex, Math.Max(0, body.GetBuffCount(buffIndex) + delta));
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active || !body)
                {
                    Destroy(this);
                    return;
                }

                int expired = 0;
                for (int i = grants.Count - 1; i >= 0; i--)
                {
                    if (grants[i].expiresAt <= Time.fixedTime)
                    {
                        expired += grants[i].amount;
                        grants.RemoveAt(i);
                    }
                }

                if (expired > 0)
                    ChangeCount(-expired);
            }
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