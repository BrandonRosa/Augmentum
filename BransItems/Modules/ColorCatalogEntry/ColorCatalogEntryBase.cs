using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.ObjectModel;
using System.Linq;

namespace BransItems.Modules.ColorCatalogEntry
{


    public abstract class ColorCatalogEntryBase<T> : ColorCatalogEntryBase where T : ColorCatalogEntryBase<T>
    {
        public static T instance { get; private set; }

        public ColorCatalogEntryBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    /// <summary>
    /// A <see cref="ContentBase"/> that represents an <see cref="RoR2.ItemTierDef"/> for the game, the ItemTierDef is represented via the <see cref="itemTierDef"/>
    /// <para>Its tied module base is the <see cref="ItemTierModuleBase"/></para>
    /// </summary>
    public abstract class ColorCatalogEntryBase
    {
        //public SerializableColorCatalogEntry colorCatalogEntry = ScriptableObject.CreateInstance<SerializableColorCatalogEntry>();
        public ColorCatalog.ColorIndex colorIndex;

        public abstract byte r { get; }

        public abstract byte g { get; }

        public abstract byte b { get; }

        public abstract byte a { get; }


        public virtual string ColorCatalogEntryName { get; internal set; } = "DEFAULT";



        public abstract void Init();

        /*
        public void CreateColorCatalogEntry()
        {
            colorCatalogEntry.color32 = new Color32(r,g,b,a);
            colorCatalogEntry.name = ColorCatalogEntryName;
            ColorsAPI.AddSerializableColor(colorCatalogEntry);
            BransItems.ModLogger.LogWarning("Index:" + ((int)colorCatalogEntry.ColorIndex));
            BransItems.ModLogger.LogWarning("ArraySize:" + (ColorCatalog.indexToHexString.Length));
        }
        */
        public void CreateColorCatalogEntry()
        {
            float fr = (float)r / 255f;
            float fg = (float)g / 255f;
            float fb = (float)b / 255f;
            colorIndex = ColorsAPI.RegisterColor(new Color(fr,fg,fb));
            BransItems.ModLogger.LogWarning("Index:" + ((int)colorIndex));
            BransItems.ModLogger.LogWarning("ArraySize:" + (ColorCatalog.indexToHexString.Length));
        }
    }
}