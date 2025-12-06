using BepInEx.Configuration;
using Augmentum.Modules.Compatability;
using Augmentum.Modules.ItemTiers.CoreTier;
using Augmentum.Modules.ItemTiers.HighlanderTier;
using Augmentum.Modules.Utils;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Augmentum.Augmentum;
using static Augmentum.Modules.Utils.ItemHelpers;

namespace Augmentum.Modules.Pickups.Items.Essences
{
    class Conquest : ArtifactBase<Conquest>
    {
        public override string ArtifactName => "Artifact of Conquest";
        public override string ArtifactLangTokenName => "CONQUEST";
        public override string ArtifactFullDescription => $"Highlander drops are no longer random. They now drop at the end of the teleporter event of every even stage.";

        public override GameObject ArtifactModel => MainAssets.LoadAsset<GameObject>("Assets/Textrures/Icons/Temporary/crystal3/source/Ferocity.prefab");
        public override Sprite ArtifactIconDeselected => MainAssets.LoadAsset<Sprite>("Assets/Models/ArtifactOfConquest/Conquest2Disabled.png");
        public override Sprite ArtifactIconSelected => MainAssets.LoadAsset<Sprite>("Assets/Models/ArtifactOfConquest/Conquest2Enabled.png");


        public override void Init(ConfigFile config)
        {

            CreateConfig(config);
            CreateLang();
            CreateArtifact();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            
        }

        public override void Hooks()
        {
            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterInteraction_onTeleporterChargedGlobal;
            On.RoR2.InfiniteTowerRun.OnWaveAllEnemiesDefeatedServer += InfiniteTowerRun_OnWaveAllEnemiesDefeatedServer;
        }

        private void InfiniteTowerRun_OnWaveAllEnemiesDefeatedServer(On.RoR2.InfiniteTowerRun.orig_OnWaveAllEnemiesDefeatedServer orig, InfiniteTowerRun self, InfiniteTowerWaveController wc)
        {
            orig(self, wc);
            if (NetworkServer.active && self && self.safeWardController && self.safeWardController.transform && self.IsStageTransitionWave() && this.IsSelectedAndInRun() && (Run.instance.NetworkstageClearCount + 1) % 2 == 0)
            {
                if (ModCompatability.JudgementCompat.IsInJudgementRun() && (Run.instance.NetworkstageClearCount + 1) % 4 != 0)
                    return;
                int count = PlayerCharacterMasterController.instances.Count;
                DropHighlanders(count, self.safeWardController.transform);
            }
        }

        private void TeleporterInteraction_onTeleporterChargedGlobal(TeleporterInteraction obj)
        {
            if (NetworkServer.active && obj && obj.transform && this.IsSelectedAndInRun() && (Run.instance.NetworkstageClearCount + 1) % 2 == 0)
            {
                int count = PlayerCharacterMasterController.instances.Count;
                DropHighlanders(count, obj.transform);
            }
        }



        public static void DropHighlanders(int dropCount, Transform dropPosition)
        {
            int num = dropCount;
            float num2 = 360f / (float)num;
            Vector3 val = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            Quaternion val2 = Quaternion.AngleAxis(num2, Vector3.up);
            int num3 = 0;
            while (num3 < num)
            {
                if (Spoils.instance.IsSelectedAndInRun())
                {

                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "<color=#FAF7B9><size=120%>" + "You have been rewarded with a gift from the Highlands." + "</color></size>"
                    });

                    PickupDropletController.CreatePickupDroplet(Spoils.SpoilsPickupInfo(dropPosition.position + Vector3.up * 3f), dropPosition.position, val);
                    num3++;
                    val = val2 * val;
                }
                else
                {
                    List<PickupDef> HighList = ItemHelpers.PickupDefsWithTier(Highlander.instance.itemTierDef);
                    int picked = RoR2Application.rng.RangeInt(0, HighList.Count);

                    PickupDef pickupDef = HighList[picked];
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = "<color=#FAF7B9><size=120%>" + "You have been rewarded with a gift from the Highlands." + "</color></size>"
                    });
                    PickupIndex pickupIndex = pickupDef.pickupIndex;
                    PickupDropletController.CreatePickupDroplet(pickupIndex, dropPosition.position + Vector3.up * 3f, val);
                    num3++;
                    val = val2 * val;
                }
            }
        }
    }
}
