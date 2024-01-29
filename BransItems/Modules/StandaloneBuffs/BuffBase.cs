﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BransItems.Modules.StandaloneBuffs
{

    public abstract class BuffBase<T> : BuffBase where T : BuffBase<T>
    {
        public static T instance { get; private set; }

        public BuffBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting BuffBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class BuffBase
    {
        public abstract string BuffName { get; }
        public abstract Color Color { get; }
        public virtual bool CanStack { get;} = false;
        public virtual bool IsDebuff { get;} = false;

        public virtual bool IsCooldown { get; } = true;

        public virtual bool IsHidden { get; } = false;
        public virtual Sprite BuffIcon { get; } = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

        public BuffDef BuffDef;

        public abstract void Init(ConfigFile config);

        public void CreateBuff()
        {
            BuffDef = ScriptableObject.CreateInstance<BuffDef>();
            BuffDef.name = BuffName;
            BuffDef.buffColor = Color;
            BuffDef.canStack = CanStack;
            //BransItems.ModLogger.LogWarning("CanStack: " + BuffDef.canStack);
            BuffDef.isDebuff = IsDebuff;
            BuffDef.iconSprite = BuffIcon;
            BuffDef.isCooldown = IsCooldown;
            BuffDef.isHidden = IsHidden;

            ContentAddition.AddBuffDef(BuffDef);
        }

        public abstract void Hooks();
    }
}