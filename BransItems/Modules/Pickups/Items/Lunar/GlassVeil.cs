using BepInEx.Configuration;
using BransItems.Modules.Pickups.Items.NoTier;
using BransItems.Modules.Pickups.Items.Tier3;
using BransItems.Modules.StandaloneBuffs;
using BransItems.Modules.Utils;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static BransItems.Modules.Utils.ItemHelpers;
using static BransItems.BransItems;

namespace BransItems.Modules.Pickups.Items.Lunar
{
    class GlassVeil : ItemBase<GlassVeil>
    {
        public override string ItemName => "Glass Veil";

        public override string ItemLangTokenName => "GLASS_VEIL";

        public override string ItemPickupDesc => $"Gain a veil which blocks damage then gives you invincibility and makes you immune to all indirect damage... <style=cDeath>BUT your max health cannot exceed 1</style> ";

        public override string ItemFullDescription => "Gain a veil which <style=cIsHealing>blocks</style> damage then gives you <style=cIsHealing>5 seconds</style> of <style=cIsHealing>invincibility</style>. Recharges after <style=cIsUtility>120 seconds</style>. <style=cIsHealing>Ignore indirect damage</style>. <style=cIsUtility>Discover replacements for useless items</style>. <style=cDeath>Set max health to 1</style>."; 

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Lunar;

        public override GameObject ItemModel => MainAssets.LoadAsset<GameObject>("Assets/Models/GlassVeil/GlassVeilModel.prefab");
        public override Sprite ItemIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/GlassVeil/GlassVeilIcon.png");

        public static GameObject ItemBodyModelPrefab;

        public string ConfigBlacklist;

        public bool BlacklistHealCatagory;

        public string HealWhitelist;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist };

        public static int DefaultWishOptions = 2;

        public static string DefaultBlacklist = $"ArmorPlate, BarrierOnKill, BarrierOnOverHeal, BoostHP, CritHeal, FlatHealth, GoldOnHurt, HalfSpeedDoubleHealth, HealOnCrit, HealWhileSafe, HealingPotion, ITEM_ADAPTIVE_ARMOR, ITEM_BARRIER_BAND, ITEM_ESSENCE_OF_LIFE, ITEM_HEALING_BAND, ITEM_NOVA_BAND, ITEM_SAFETY_BLANKET, IncreasedHealing, Infusion, Knurl, " +
            $"MissileVoid, Mushroom, MushroomVoid, NovaOnHeal, NovaOnLowHealth, OutOfCombatArmor, Pearl, PersonalShield, Phasing, RepeatHeal, Seed, ShieldOnly, SiphonOnLowHealth, SprintArmor,TPHealingNova, Thorns, Tooth";

        public static string DefaultHealWhitelist = $"ShinyPearl";

        public static float DefaultInvincibleTime = 10f;

        public static float DefaultCooldownTime = 120f;

        public static GameObject potentialPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();

        //public static HashSet<ItemDef> ItemBlacklist
        //{
        //    get
        //    {
        //        if (ItemBlacklist == null || ItemBlacklist.Count < 1)
        //            ItemBlacklist = GetItemBlacklist();
        //        return ItemBlacklist;
        //    }
        //    set
        //    {
        //        ItemBlacklist = value;
        //    }
        //}
        private static HashSet<ItemDef> _itemBlacklist;

        public static HashSet<ItemDef> ItemBlacklist
        {
            get
            {
                if (_itemBlacklist == null || _itemBlacklist.Count < 1)
                    _itemBlacklist = InstantiateItemBlacklist();
                return _itemBlacklist;
            }
            set
            {
                _itemBlacklist = value;
            }
        }

        //public static HashSet<ItemIndex> ItemIndexBlacklist
        //{
        //    get 
        //    {
        //        if (ItemIndexBlacklist == null || ItemIndexBlacklist.Count < 1)
        //            ItemIndexBlacklist = new HashSet<ItemIndex>(ItemBlacklist.Select(x => x.itemIndex));
        //        return ItemIndexBlacklist;
        //    }
        //    set
        //    {
        //        ItemIndexBlacklist = value;
        //    }
        //}

        private static HashSet<ItemIndex> _itemIndexBlacklist;

        public static HashSet<ItemIndex> ItemIndexBlacklist
        {
            get
            {
                if (_itemIndexBlacklist == null || _itemIndexBlacklist.Count < 1)
                    _itemIndexBlacklist = new HashSet<ItemIndex>(ItemBlacklist.Select(x => x.itemIndex));
                return _itemIndexBlacklist;
            }
            set
            {
                _itemIndexBlacklist = value;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemBodyModelPrefab = ItemModel;
            var itemDisplay = ItemBodyModelPrefab.AddComponent<RoR2.ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(ItemBodyModelPrefab, true);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            /*
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.
            
            
            
            
            edPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.42142F, -0.10234F),
                    localAngles = new Vector3(351.1655F, 45.64202F, 351.1029F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.35414F, -0.14761F),
                    localAngles = new Vector3(356.5505F, 45.08208F, 356.5588F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0, 2.46717F, 2.64379F),
                    localAngles = new Vector3(315.5635F, 233.7695F, 325.0397F),
                    localScale = new Vector3(.2F, .2F, .2F)
                }
            });
            rules.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0, 0.24722F, -0.01662F),
                    localAngles = new Vector3(10.68209F, 46.03322F, 11.01807F),
                    localScale = new Vector3(0.025F, 0.025F, 0.025F)
                }
            });
            rules.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.24128F, -0.14951F),
                    localAngles = new Vector3(6.07507F, 45.37084F, 6.11489F),
                    localScale = new Vector3(0.017F, 0.017F, 0.017F)
                }
            });
            rules.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.31304F, -0.00747F),
                    localAngles = new Vector3(359.2931F, 45.00048F, 359.2912F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0, 1.94424F, -0.47558F),
                    localAngles = new Vector3(20.16552F, 48.87548F, 21.54582F),
                    localScale = new Vector3(.15F, .15F, .15F)
                }
            });
            rules.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.30118F, -0.0035F),
                    localAngles = new Vector3(8.31363F, 45.67525F, 8.41428F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.65444F, 1.64345F),
                    localAngles = new Vector3(326.1803F, 277.2657F, 249.9269F),
                    localScale = new Vector3(.2F, .2F, .2F)
                }
            });
            rules.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.0068F, 0.3225F, -0.03976F),
                    localAngles = new Vector3(0F, 45F, 0F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.14076F, 0.15542F, -0.04648F),
                    localAngles = new Vector3(356.9802F, 81.10978F, 353.687F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("CHEF", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hat",
                    localPos = new Vector3(0F, 0.01217F, -0.00126F),
                    localAngles = new Vector3(356.9376F, 25.8988F, 14.69767F),
                    localScale = new Vector3(0.001F, 0.001F, 0.001F)
                }
            });
            rules.Add("RobPaladinBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.00042F, 0.46133F, 0.01385F),
                    localAngles = new Vector3(355.2848F, 47.55381F, 355.0908F),
                    localScale = new Vector3(0.020392F, 0.020392F, 0.020392F)
                }
            });
            rules.Add("RedMistBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.00076F, -0.0281F, 0.09539F),
                    localAngles = new Vector3(338.9489F, 145.7505F, 217.6883F),
                    localScale = new Vector3(0.005402F, 0.005402F, 0.005402F)
                }
            });
            rules.Add("ArbiterBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, -0.00277F, -0.13259F),
                    localAngles = new Vector3(322.1495F, 124.8318F, 235.476F),
                    localScale = new Vector3(0.02F, 0.02F, 0.02F)
                }
            });
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0F, 0.32104F, 0F),
                    localAngles = new Vector3(0F, 321.2954F, 0F),
                    localScale = new Vector3(0.024027F, 0.024027F, 0.024027F)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HeadCenter",
                    localPos = new Vector3(0.00216F, 0.01033F, 0F),
                    localAngles = new Vector3(0F, 323.6887F, 355.1232F),
                    localScale = new Vector3(0.000551F, 0.000551F, 0.000551F)
                }
            });*/
            return rules;
        }

        public override void Hooks()
        {
            //On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;


            On.RoR2.CharacterMaster.OnItemAddedClient += CharacterMaster_OnItemAddedClient;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if(self && self.healthComponent!=null && self.inventory!=null && self.inventory.GetItemCount(ItemDef)>0)
            {
                self.healthComponent.Networkbarrier = 0;
                self.healthComponent.Networkhealth = 1;
                self.healthComponent.Networkshield = 0;
                self.maxBarrier = 0;
                self.maxShield = 0;
                self.maxHealth = 1;
                self.regen = 0;
                
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool ignoreDamage = false;
            if(self!=null && damageInfo!=null && self.body && self.body.inventory && self.body.inventory.GetItemCount(ItemDef)>0)
            {
                if(damageInfo.damageType!=DamageType.Generic && damageInfo.attacker!=self.gameObject)
                {
                    ignoreDamage = true;
                }
                else
                {
                    self.ospTimer = 0;
                }
            }

            if(!ignoreDamage)
            {
                orig(self, damageInfo);
            }
        }

        private void CharacterMaster_OnItemAddedClient(On.RoR2.CharacterMaster.orig_OnItemAddedClient orig, CharacterMaster self, ItemIndex itemIndex)
        {
            if (self && itemIndex != null && self.inventory && (self.inventory.GetItemCount(ItemDef)>0 || ItemDef.itemIndex==itemIndex))
            {
                //Pickup blacklisted Item
                ItemDef addedItem = ItemCatalog.GetItemDef(itemIndex);
                if(ItemBlacklist.Contains(addedItem))
                {
                    RerollItem(self, addedItem);
                }
            
                //Pickup OUR ITEM
                if(itemIndex==ItemDef.itemIndex)
                {
                    HashSet<ItemIndex> itemsInInventory=new HashSet<ItemIndex>(self.inventory.itemAcquisitionOrder);
                    //Filter out stuff NOT in blacklist
                    itemsInInventory.IntersectWith(ItemIndexBlacklist);
                    //Run everythin in list through "Rerollitem"
                    HashSet<ItemIndex>.Enumerator Enum=itemsInInventory.GetEnumerator();
                    while(Enum.MoveNext())
                    {
                        RerollItem(self, ItemCatalog.GetItemDef(Enum.Current));
            
                    }
            
                    //Give the curse?
                    //Recalculate Stats

                    //if(self.inventory.GetItemCount(ItemDef)==1)
                    //    self.GetBody().ex
                    self.GetBody().RecalculateStats();
            
                }
            
            }
            orig(self, itemIndex);
        }

        private void RerollItem(CharacterMaster self, ItemDef itemDef)
        {
            int itemCount = self.inventory.GetItemCount(itemDef);
            self.inventory.RemoveItem(itemDef,itemCount);

            ItemTier itemTier = itemDef.tier;//itemDef._itemTierDef;
            if(itemTier!=ItemTier.NoTier && self.GetBody() && self.GetBody().isPlayerControlled)
            {
                for (int i = 0; i < itemCount; i++)
                {


                    List<ItemDef> AllItemsInTier = ItemDefsWithTier(ItemTierCatalog.GetItemTierDef(itemTier));
                    AllItemsInTier.RemoveAll(x => ItemBlacklist.Contains(x));
                    BransItems.ModLogger.LogWarning("Rerollable to:" + AllItemsInTier.ToString());


                    int WishOptions = DefaultWishOptions + self.inventory.GetItemCount(DiscoveryMedallionConsumed.instance.ItemDef) * DiscoveryMedallion.AdditionalChoices;

                    float dropUpVelocityStrength = 20f;

                    float dropForwardVelocityStrength = 2f;

                    Transform dropTransform = self.transform;



                    ItemDef[] chosenItems = ItemHelpers.GetRandomSelectionFromArray(AllItemsInTier, WishOptions, RoR2Application.rng);
                    PickupIndex[] pickupIndex = chosenItems.Select((ItemDef x) => PickupCatalog.FindPickupIndex(x.itemIndex)).ToArray();

                    PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
                    {
                        pickerOptions = PickupPickerController.GenerateOptionsFromArray(pickupIndex),
                        prefabOverride = potentialPrefab,
                        position = self.GetBody().transform.position,
                        rotation = Quaternion.identity,
                        pickupIndex = pickupIndex[0] //PickupCatalog.FindPickupIndex(ItemTier.Tier3)
                    },
                                     self.GetBody().transform.position, Vector3.up * dropUpVelocityStrength);
                }
            }

        }

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            //CreateBuff();
            CreateItem();
            Hooks();
        }

        public void CreateConfig(ConfigFile config)
        {
            ConfigBlacklist = ConfigManager.ConfigOption<string>("Item: " + ItemName, "Item blacklist", DefaultBlacklist , "Items (by internal name) that are rerolled on pickup, comma separated. Please find the item \"Code Names\" at: https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names");
            BlacklistHealCatagory = ConfigManager.ConfigOption<bool>("Item: " + ItemName, "Auto blacklist heal catagory", true, "Enable to automatically add all items within the \"healing\" category");
            HealWhitelist = ConfigManager.ConfigOption<string>("Item: " + ItemName, "Heal catagory exclude blacklist", DefaultHealWhitelist, "Healing catagory Items (by internal name) that are excluded from \"Auto Blacklist\", comma separated. Please find the item \"Code Names\" at: https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names");
        }

        private static HashSet<ItemDef> InstantiateItemBlacklist()
        {
            HashSet<ItemDef> blacklist = new HashSet<ItemDef>();
            blacklist=GetConfigBlacklist();

            if (GlassVeil.instance.BlacklistHealCatagory)
            {
                HashSet<ItemDef> HealCatagoryItems = new HashSet<ItemDef>();
                HashSet<ItemDef> HealingWhitelist = GetConfigHealingWhitelist();


                //ItemMask.Enumerator itemEnum =Run.instance.availableItems.GetEnumerator();

                //while(itemEnum.MoveNext() && itemEnum.Current!=null)
                //{
                //    ItemDef currentItemDef = ItemCatalog.GetItemDef(itemEnum.Current);
                //    if (!HealingWhitelist.Contains(currentItemDef))
                //    {
                //        HealCatagoryItems.Add(currentItemDef);
                //    }
                //}

                var healingItems = ItemCatalog.allItemDefs.Where(itemDef => itemDef.tags.Contains(ItemTag.Healing));
                HealCatagoryItems.UnionWith(healingItems);
                BransItems.ModLogger.LogWarning("1:" + HealCatagoryItems.Count);
                blacklist.UnionWith(HealCatagoryItems);
                BransItems.ModLogger.LogWarning("2:" + blacklist.Count);
                blacklist.ExceptWith(HealingWhitelist);
                BransItems.ModLogger.LogWarning("3:" + blacklist.Count);
            }

            return blacklist;
        }

        private static HashSet<ItemDef> GetConfigBlacklist()
        {
            HashSet<ItemDef> _items = new HashSet<ItemDef>();

            char[] separators = new char[] { ' ', ',' };
            foreach (var piece in GlassVeil.instance.ConfigBlacklist.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                // if (int.TryParse(piece.Trim(), out var itemIndex))
                //     _items.Add((ItemIndex) itemIndex);
                BransItems.ModLogger.LogWarning("Name:" + piece);
                var item = ItemCatalog.FindItemIndex(piece);
                if (item == ItemIndex.None) continue;
                BransItems.ModLogger.LogWarning("itemIndex" + item);
                _items.Add(ItemCatalog.GetItemDef(item));
            }

            return _items;
        }

        private static HashSet<ItemDef> GetConfigHealingWhitelist()
        {
            HashSet<ItemDef> _items = new HashSet<ItemDef>();

            char[] separators = new char[] { ' ', ',' };
            foreach (var piece in GlassVeil.instance.HealWhitelist.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                // if (int.TryParse(piece.Trim(), out var itemIndex))
                //     _items.Add((ItemIndex) itemIndex);
                var item = ItemCatalog.FindItemIndex(piece);
                if (item == ItemIndex.None) continue;

                _items.Add(ItemCatalog.GetItemDef(item));
            }

            return _items;
        }
    }

    public class VeilCooldown : BuffBase<VeilCooldown>
    {
        public override string BuffName => "Veil Cooldown";

        public override Color Color => new Color32(40, 40, 40, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/GlassVeil/GhostIcon.png");
        public virtual bool CanStack => true;
        public virtual bool IsDebuff => true;

        public virtual bool IsCooldown => true;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
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
    public class VeilReady : BuffBase<VeilReady>
    {
        public override string BuffName => "Veil Ready";

        public override Color Color => new Color32(140, 230, 240, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/GlassVeil/GhostIcon.png");
        public virtual bool CanStack => true;
        public virtual bool IsDebuff => false;

        public virtual bool IsCooldown => false;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            //RoR2Application.onLoad += OnLoadModCompat;
        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            //Basically if the player exists, and doesnt have "Fortified Ready" or "Fortified Cooldown"- Check if the player has the Fortified Tracker component- if they do add the bufff
            if (self && self.inventory)
            {
                if (!self.HasBuff(BuffDef))
                {
                    if (!self.HasBuff(VeilCooldown.instance.BuffDef)) // && !self.HasBuff(Fortified.instance.BuffDef))
                    {
                        int veilCount = self.inventory.GetItemCount(GlassVeil.instance.ItemDef);
                        if (veilCount>0) //cpt = self.gameObject.AddComponent<FortifiedTracker>();
                        {
                            self.AddBuff(BuffDef);
                        }

                    }
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool skipDamage = false;
            if (self && self.body && self.body.inventory)
            {
                if (self.body && self.body.GetBuffCount(BuffDef) > 0)
                {
                    int veilCount = self.body.inventory.GetItemCount(GlassVeil.instance.ItemDef);
                    if (veilCount>0)
                    {
                        self.body.RemoveBuff(BuffDef);
                        for (int i = 1; (float)i <= GlassVeil.DefaultInvincibleTime; i++)
                            self.body.AddTimedBuff(VeilInvincibility.instance.BuffDef, i);
                        for (int i = 1; (float)i <= GlassVeil.DefaultCooldownTime; i++)
                            self.body.AddTimedBuff(VeilCooldown.instance.BuffDef, i);
                        skipDamage = true;
                    }
                }
            }
            if(!skipDamage)
            {
                orig(self, damageInfo);
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

    public class VeilInvincibility : BuffBase<VeilInvincibility>
    {
        public override string BuffName => "Invincibility Veil";

        public override Color Color => new Color32(240, 200, 62, 255);

        public override Sprite BuffIcon => MainAssets.LoadAsset<Sprite>("Assets/Models/GlassVeil/GhostIcon.png");
        public virtual bool CanStack => true;
        public virtual bool IsDebuff => false;

        public virtual bool IsCooldown => false;

        public override void Init(ConfigFile config)
        {
            CreateBuff();
            Hooks();
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            //On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
            //RoR2Application.onLoad += OnLoadModCompat;
        }


        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool skipDamage = false;
            if (self && self.body)
            {
                if (self.body && self.body.GetBuffCount(BuffDef) > 0)
                {
                    damageInfo.damage = 0;
                    skipDamage = true;
                }
            }
            if (!skipDamage)
                orig(self, damageInfo);
            
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
