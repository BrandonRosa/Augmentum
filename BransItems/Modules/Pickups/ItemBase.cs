
using BepInEx.Configuration;
using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BransItems.Modules.Pickups
{
    public abstract class ItemBase
    {
		public abstract string ItemName { get; }
		public abstract string ItemLangTokenName { get; }
		public abstract string ItemPickupDesc { get; }
		public abstract string ItemFullDescription { get; }
		public abstract string ItemLore { get; }

		public abstract ItemTier Tier { get; }
		public virtual ItemTag[] ItemTags { get; } = new ItemTag[] { };

		public virtual GameObject ItemModel { get; } = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
		public virtual Sprite ItemIcon { get; } = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");

		public virtual bool CanRemove { get; } = true;
		public virtual bool Hidden { get; } = false;
        public bool AIBlacklisted { get; internal set; }
        public bool PrinterBlacklisted { get; internal set; }

        public ItemDef ItemDef;

		public abstract void Init(ConfigFile config);

		protected void CreateLang()
		{
			LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
			LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
			LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
			LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);
		}

		public abstract ItemDisplayRuleDict CreateItemDisplayRules();

		protected void CreateItem()
		{
			ItemDef = ScriptableObject.CreateInstance<ItemDef>();
			ItemDef.name = "ITEM_" + ItemLangTokenName;
			ItemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
			ItemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
			ItemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
			ItemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";
			ItemDef.pickupModelPrefab = ItemModel;
			ItemDef.pickupIconSprite = ItemIcon;
			ItemDef.hidden = Hidden;
			ItemDef.canRemove = CanRemove;
			//ItemDef.tier = Tier;
			ItemDef.deprecatedTier = Tier;
			ItemDef.tags = ItemTags;
			//ItemTag.WorldUnique
			var itemDisplayRuleDict = CreateItemDisplayRules();
			ItemAPI.Add(new CustomItem(ItemDef, itemDisplayRuleDict));
		}

		public abstract void Hooks();

		public int GetCount(CharacterBody body)
		{
			if (!body || !body.inventory) { return 0; }

			return body.inventory.GetItemCount(ItemDef);
		}

		public int GetCount(CharacterMaster master)
		{
			if (!master || !master.inventory) { return 0; }

			return master.inventory.GetItemCount(ItemDef);
		}
	}
}
