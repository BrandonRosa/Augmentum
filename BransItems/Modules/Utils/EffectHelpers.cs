using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BransItems.Modules.Utils
{
    public static class EffectHelpers
    {
        public static void SetParticleSystemColorOverTime(ref ParticleSystem particleSystem, ParticleSystem.MinMaxGradient color)
        {
            ParticleSystem.ColorOverLifetimeModule COLM = particleSystem.colorOverLifetime;
            COLM.color = color;
        }

        public static void SetParticleSystemLightColor(ref ParticleSystem particleSystem, Color color)
        {
            ParticleSystem.LightsModule lightsModule = particleSystem.lights;
            lightsModule.light.color = color;
        }

        public static void SetTextureInParticleSystemRenderer(ref ParticleSystemRenderer particleSystemRenderer, string name, Texture2D texture)
        {
            particleSystemRenderer.GetMaterial().SetTexture(name,texture);
            
        }

    }
}
