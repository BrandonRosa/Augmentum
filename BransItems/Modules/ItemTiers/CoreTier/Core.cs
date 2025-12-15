using System;
using System.Collections.Generic;
using System.Text;
using Augmentum.Modules.ColorCatalogEntry;
//using Augmentum.Modules.ColorCatalogEntry.CoreColors;
using Augmentum.Modules.ItemTiers;
using Augmentum.Modules.Pickups.Items.Essences;
using static Augmentum.Augmentum;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Augmentum.Modules.Pickups;

namespace Augmentum.Modules.ItemTiers.CoreTier
{
    class Core : ItemTierBase<Core>
    {
        //public override ItemTierDef itemTierDef = ScriptableObject.CreateInstance<ItemTierDef>(); // new ItemTierDef();

        //public override GameObject PickupDisplayVFX => throw new NotImplementedException();

        //public override GameObject highlightPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();

        //public override GameObject dropletDisplayPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();

        public override bool canRestack => true;

        public override bool canScrap => false;

        public override bool isDroppable => true;

        public override string TierName => "Core";

        public override Texture backgroundTexture => MainAssets.LoadAsset<Texture>("Assets/Textrures/Icons/TierBackground/BgCore.png");

        //public ColorCatalog.ColorIndex colorIndex = ColorsAPI.RegisterColor(new Color32(21,99,58,255));//ColorCatalog.ColorIndex.Money;//CoreLight.instance.colorCatalogEntry.ColorIndex;

        //public ColorCatalog.ColorIndex darkColorIndex = ColorsAPI.RegisterColor(new Color32(1,126,62,255));//ColorCatalog.ColorIndex.Money;//CoreDark.instance.colorCatalogEntry.ColorIndex;
        /// <summary>
        ///  CoreLight.instance.colorCatalogEntry.ColorIndex;
        /// 
        /// x => CoreDark.instance.colorCatalogEntry.ColorIndex;
        /// </summary>
        public override void Init()
        {
            colorIndex = Colors.TempCoreLight;//ColorsAPI.RegisterColor(Color.gray);
            darkColorIndex = Colors.TempCoreDark;//ColorsAPI.RegisterColor(Color.gray);

            CreateDropletPrefab();
            CreateVFXPrefab();
            CreateLang();
            CreateTier();

        }

        private void CreateLang()
        {
            LanguageAPI.Add("TIER_CORE_ESSENCE_KEYWORD", "Essence");
            LanguageAPI.Add("TIER_CORE_ESSENCES_KEYWORD", "Essences");
        }

        private void CreateDropletPrefab()
        {
            GameObject Temp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/Tier2Orb.prefab").WaitForCompletion().InstantiateClone("CoreOrb", false);
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
            foreach(Light thisLight in lights)
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
            GameObject VFX = Temp.transform.GetChild(7).gameObject.InstantiateClone("CoreVFX", false);

            //GameObject DistantGlow = VFX.transform.GetChild(0).transform.GetChild(0).gameObject; //Dark
            //GameObject Swirls = VFX.transform.GetChild(0).transform.GetChild(1).gameObject; //Light
            //GameObject PointLight = VFX.transform.GetChild(0).transform.GetChild(2).gameObject; //Light
            //GameObject Glowies = VFX.transform.GetChild(0).transform.GetChild(3).gameObject; //Light

            

            ParticleSystem.MainModule NewColor = VFX.transform.GetChild(0).GetChild(0).gameObject.GetComponent<ParticleSystem>().main; //GlowA
            NewColor.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);
            

            ParticleSystem.MainModule NewColor2 = VFX.transform.GetChild(0).GetChild(1).gameObject.GetComponent<ParticleSystem>().main; //Swirls
            NewColor2.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);
            

            VFX.transform.GetChild(0).GetChild(2).gameObject.GetComponent<Light>().color = colorLight;
            VFX.transform.GetChild(0).GetChild(2).gameObject.GetComponent<Light>().set_color_Injected(ref colorLight);

            ParticleSystem.MainModule NewColor3 = VFX.transform.GetChild(0).GetChild(0).gameObject.GetComponent<ParticleSystem>().main;
            NewColor3.startColor = new ParticleSystem.MinMaxGradient(colorLight, colorDark);
            

            PickupDisplayVFX = VFX;
        }
    }
}
