using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BransItems.Modules.Pickups
{
    public abstract class EquipmentBase
    {
		public abstract string EquipmentName { get; }
		public abstract string EquipmentLangTokenName { get; }
		public abstract string EquipmentPickupDesc { get; }
		public abstract string EquipmentFullDescription { get; }
		public abstract string EquipmentLore { get; }

		public abstract GameObject EquipmentModel { get; }
		public abstract Sprite EquipmentIcon { get; }

		public EquipmentDef EquipmentDef;

		public virtual bool AppearsInSinglePlayer { get; } = true;
		public virtual bool AppearsInMultiPlayer { get; } = true;
		public virtual bool CanDrop { get; } = true;
		public virtual float Cooldown { get; } = 60f;
		public virtual bool EnigmaCompatible { get; } = true;
		public virtual bool IsBoss { get; } = false;
		public virtual bool IsLunar { get; } = false;

		public abstract void Init(ConfigFile config);

		protected void CreateLang()
		{
			LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_NAME", EquipmentName);
			LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP", EquipmentPickupDesc);
			LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION", EquipmentFullDescription);
			LanguageAPI.Add("EQUIPMENT_" + EquipmentLangTokenName + "_LORE", EquipmentLore);
		}

		public abstract ItemDisplayRuleDict CreateItemDisplayRules();

		protected void CreateEquipment()
		{
			EquipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();
			EquipmentDef.name = "EQUIPMENT_" + EquipmentLangTokenName;
			EquipmentDef.nameToken = "EQUIPMENT_" + EquipmentLangTokenName + "_NAME";
			EquipmentDef.pickupToken = "EQUIPMENT_" + EquipmentLangTokenName + "_PICKUP";
			EquipmentDef.descriptionToken = "EQUIPMENT_" + EquipmentLangTokenName + "_DESCRIPTION";
			EquipmentDef.loreToken = "EQUIPMENT_" + EquipmentLangTokenName + "_LORE";
			EquipmentDef.pickupModelPrefab = EquipmentModel;
			EquipmentDef.pickupIconSprite = EquipmentIcon;
			EquipmentDef.appearsInSinglePlayer = AppearsInSinglePlayer;
			EquipmentDef.appearsInMultiPlayer = AppearsInMultiPlayer;
			EquipmentDef.canDrop = CanDrop;
			EquipmentDef.cooldown = Cooldown;
			EquipmentDef.enigmaCompatible = EnigmaCompatible;
			EquipmentDef.isBoss = IsBoss;
			EquipmentDef.isLunar = IsLunar;

			ItemAPI.Add(new CustomEquipment(EquipmentDef, CreateItemDisplayRules()));
			On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
		}

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
		{
			if (equipmentDef == EquipmentDef)
			{
				return ActivateEquipment(self);
			}
			else
			{
				return orig(self, equipmentDef);
			}
		}

		protected abstract bool ActivateEquipment(EquipmentSlot slot);

		public abstract void Hooks();
	}
}
