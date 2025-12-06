
using BepInEx.Configuration;
using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using On.RoR2.Items;
using System.Linq;
using HarmonyLib;

namespace Augmentum.Modules.Pickups
{

	public abstract class ArtifactBase<T> : ArtifactBase where T : ArtifactBase<T>
	{
		public static T instance { get; private set; }

		public ArtifactBase()
		{
			if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
			instance = this as T;
		}
	}
	public abstract class ArtifactBase
	{
		public abstract string ArtifactName { get; }

		public string ConfigItemName
		{
			get
			{
				return ArtifactName.Replace("\'", "");
			}
		}
		public abstract string ArtifactLangTokenName { get; }
		public abstract string ArtifactFullDescription { get; }

		public virtual GameObject ArtifactModel { get; } = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
		public virtual Sprite ArtifactIconDeselected { get; } = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
		public virtual Sprite ArtifactIconSelected { get; } = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

		public ArtifactDef ArtifactDef;

		public abstract void Init(ConfigFile config);

		protected void CreateLang()
		{
			LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_NAME", ArtifactName);
			LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION", ArtifactFullDescription);
		}



		protected void CreateArtifact()
		{
			ArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
			ArtifactDef.cachedName = "ARTIFACT_" + ArtifactLangTokenName;
			ArtifactDef.nameToken = "ARTIFACT_" + ArtifactLangTokenName + "_NAME";
			ArtifactDef.descriptionToken = "ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION";
			ArtifactDef.pickupModelPrefab = ArtifactModel;
			ArtifactDef.smallIconDeselectedSprite = ArtifactIconDeselected;
			ArtifactDef.smallIconSelectedSprite = ArtifactIconSelected;
			FixScriptableObjectName();
			ContentAddition.AddArtifactDef(ArtifactDef);
		}



		public abstract void Hooks();

		public bool IsSelectedAndInRun()
        {
			return Augmentum.ArtifactStatusDictionary.GetValueOrDefault(this,false) && RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(ArtifactDef);

		}

		public void FixScriptableObjectName()
		{
			(ArtifactDef as ScriptableObject).name = ArtifactDef.cachedName;
		}

	}


}
