using System.Collections.Generic;

namespace VikingWards.Conduits
{
    public static class WardRiteRegistry
    {
        private static readonly Dictionary<string, WardRiteDefinition> _byId = new();
        private static readonly Dictionary<DeityType, List<WardRiteDefinition>> _byDeity = new();

        static WardRiteRegistry()
        {
            RegisterOdin();
            RegisterThor();
            RegisterFrey();
            RegisterTyr();
            RegisterFreya();
        }

        private static void Register(WardRiteDefinition def)
        {
            _byId[def.Id] = def;
            if (!_byDeity.TryGetValue(def.Deity, out var list))
            {
                list = new List<WardRiteDefinition>();
                _byDeity[def.Deity] = list;
            }
            list.Add(def);
        }

        public static WardRiteDefinition? GetById(string id)
        {
            _byId.TryGetValue(id, out var def);
            return def;
        }

        public static IReadOnlyList<WardRiteDefinition> GetForDeity(DeityType deity)
        {
            if (_byDeity.TryGetValue(deity, out var list))
            {
                return list;
            }
            return System.Array.Empty<WardRiteDefinition>();
        }

        private static void RegisterOdin()
        {
            Register(new WardRiteDefinition(
                "odin_1_sanctuary", DeityType.Odin, 1,
                "Aegis of the Allfather", "ᚩ",
                "Enforces domestic sanctuary. Unauthorized players in PvP cannot build, deconstruct, or access containers and doors.",
                new OfferingCost("GreydwarfEye", 10),
                new OfferingCost("Feathers", 5),
                new OfferingCost("Ruby", 1)));

            Register(new WardRiteDefinition(
                "odin_2_shroud", DeityType.Odin, 2,
                "Raven's Shroud", "ᚱ",
                "Reduces wild roaming creature detection range by 85% outside the perimeter.",
                new OfferingCost("Guck", 15),
                new OfferingCost("BoneFragments", 10),
                new OfferingCost("Feathers", 2)));

            Register(new WardRiteDefinition(
                "odin_3_featherfall", DeityType.Odin, 3,
                "Rune of Unyielding Stone", "ᛋ",
                "Eliminates 100% of fall damage within the ward boundary.",
                new OfferingCost("Obsidian", 15),
                new OfferingCost("WolfFang", 5),
                new OfferingCost("DragonEgg", 1)));

            Register(new WardRiteDefinition(
                "odin_4_silent_horizon", DeityType.Odin, 4,
                "Silent Horizon", "ᚼ",
                "Reduces the frequency of base raid rolls targeting this settlement by 65%.",
                new OfferingCost("GoblinTotem", 4),
                new OfferingCost("Needle", 10),
                new OfferingCost("Tar", 15)));

            Register(new WardRiteDefinition(
                "odin_5_thought_memory", DeityType.Odin, 5,
                "Thought & Memory", "ᛗ",
                "Permitted players gain +50% increased skill XP gain on all actions performed inside the ward boundary.",
                new OfferingCost("Eitr", 10),
                new OfferingCost("BlackMarble", 10),
                new OfferingCost("DvergrNeedle", 3)));
        }

        private static void RegisterThor()
        {
            Register(new WardRiteDefinition(
                "thor_1_armor", DeityType.Thor, 1,
                "Sparks of Bilskirnir", "ᚦ",
                "Structural pieces gain +35 flat piece armor against all damage types.",
                new OfferingCost("Copper", 10),
                new OfferingCost("Tin", 10),
                new OfferingCost("SurtlingCore", 5)));

            Register(new WardRiteDefinition(
                "thor_2_stride", DeityType.Thor, 2,
                "Charged Stride", "ᛏ",
                "Permitted players gain +30% movement and sprint speed inside the barrier.",
                new OfferingCost("Iron", 15),
                new OfferingCost("SurtlingCore", 5),
                new OfferingCost("Ooze", 10)));

            Register(new WardRiteDefinition(
                "thor_3_weather", DeityType.Thor, 3,
                "Stormcaller's Aegis", "ᛊ",
                "Immunity to adverse weather debuffs (Freezing, Cold, Wet) inside the sphere. Fires never extinguish from rain.",
                new OfferingCost("Silver", 15),
                new OfferingCost("FreezeGland", 5),
                new OfferingCost("Crystal", 1)));

            Register(new WardRiteDefinition(
                "thor_4_craftspeed", DeityType.Thor, 4,
                "Craftsman's Spark", "ᛈ",
                "Crafting, building, and hammer repairs are accelerated by 60%.",
                new OfferingCost("BlackMetal", 20),
                new OfferingCost("LinenThread", 10),
                new OfferingCost("SurtlingCore", 10)));

            Register(new WardRiteDefinition(
                "thor_5_rebate", DeityType.Thor, 5,
                "Forge of Thrudvang", "ᚹ",
                "Crafting weapons, armor, and base structures grants a 15% chance to duplicate/refund materials.",
                new OfferingCost("FlametalNew", 15),
                new OfferingCost("Softtissue", 10),
                new OfferingCost("WoodAsh", 5)));
        }

        private static void RegisterFrey()
        {
            Register(new WardRiteDefinition(
                "frey_1_crops", DeityType.Frey, 1,
                "Sun of the Vanir", "ᚠ",
                "Crops, flora, and trees planted within the ward grow 35% faster.",
                new OfferingCost("CarrotSeeds", 10),
                new OfferingCost("AncientSeed", 10),
                new OfferingCost("Honey", 20)));

            Register(new WardRiteDefinition(
                "frey_2_food_freeze", DeityType.Frey, 2,
                "Preserving Cellar", "ᛃ",
                "Active food buff decay timers are frozen on permitted players while resting or standing within the ward sphere.",
                new OfferingCost("TurnipSeeds", 10),
                new OfferingCost("Entrails", 15),
                new OfferingCost("Bloodbag", 10)));

            Register(new WardRiteDefinition(
                "frey_3_cooking_speed", DeityType.Frey, 3,
                "Ever-Warm Hearth", "ᚲ",
                "Cauldrons, fermenters, and cooking ovens process foods, meads, and potions 60% faster.",
                new OfferingCost("Sausages", 10),
                new OfferingCost("OnionSeeds", 10),
                new OfferingCost("WolfMeat", 10)));

            Register(new WardRiteDefinition(
                "frey_4_mill_speed", DeityType.Frey, 4,
                "Bountiful Grain", "ᛒ",
                "Windmills and grain processing operate at double speed (+100%).",
                new OfferingCost("Barley", 30),
                new OfferingCost("Cloudberry", 20),
                new OfferingCost("LoxMeat", 10)));

            Register(new WardRiteDefinition(
                "frey_5_harvest_yield", DeityType.Frey, 5,
                "Feast of Skírnir", "ᛜ",
                "Gathering and harvesting plants and crops yields +30% bonus produce (handled via VikingLib ForagingYield).",
                new OfferingCost("MushroomMagecap", 20),
                new OfferingCost("MushroomJotunPuffs", 20),
                new OfferingCost("RoyalJelly", 5)));
        }

        private static void RegisterTyr()
        {
            Register(new WardRiteDefinition(
                "tyr_1_damage", DeityType.Tyr, 1,
                "Vanguard Stance", "ᛏ",
                "Permitted defenders deal +30% increased damage against all targets while inside the ward boundary.",
                new OfferingCost("Bronze", 10),
                new OfferingCost("BoneFragments", 10),
                new OfferingCost("TrollHide", 5)));

            Register(new WardRiteDefinition(
                "tyr_2_armor", DeityType.Tyr, 2,
                "Shieldwall Rite", "ᛉ",
                "Permitted defenders gain +30 flat body armor while inside the ward boundary.",
                new OfferingCost("Iron", 20),
                new OfferingCost("Chain", 10),
                new OfferingCost("Root", 5)));

            Register(new WardRiteDefinition(
                "tyr_3_stamina", DeityType.Tyr, 3,
                "Unyielding Endurance", "ᚢ",
                "Weapon attack stamina efficiency is increased by +35% for all permitted players.",
                new OfferingCost("Silver", 15),
                new OfferingCost("WolfFang", 10),
                new OfferingCost("WolfClaw", 5)));

            Register(new WardRiteDefinition(
                "tyr_4_kill_restore", DeityType.Tyr, 4,
                "Tyr's Judgment", "ᛞ",
                "Slaying an enemy inside the perimeter restores +15 Health and +30 Stamina to the killer.",
                new OfferingCost("BlackMetal", 20),
                new OfferingCost("Needle", 10),
                new OfferingCost("TrophyGoblinBrute", 1)));

            Register(new WardRiteDefinition(
                "tyr_5_slow_field", DeityType.Tyr, 5,
                "Biting Fetters", "ᚾ",
                "Hostile creatures entering the ward boundary suffer a continuous 35% movement speed slow debuff.",
                new OfferingCost("Carapace", 15),
                new OfferingCost("Mandible", 10),
                new OfferingCost("Sinew", 5)));
        }

        private static void RegisterFreya()
        {
            Register(new WardRiteDefinition(
                "freya_1_procreation", DeityType.Freya, 1,
                "Mother of Beasts", "ᛖ",
                "Tamed animals (boars, wolves, lox, asksvin) mate and procreate 60% faster within the ward.",
                new OfferingCost("Dandelion", 20),
                new OfferingCost("Raspberry", 20),
                new OfferingCost("TrophyDeer", 5)));

            Register(new WardRiteDefinition(
                "freya_2_health_regen", DeityType.Freya, 2,
                "Sylvan Shelter", "ᛚ",
                "Permitted players gain +60% health regeneration rate inside the perimeter.",
                new OfferingCost("ElderBark", 20),
                new OfferingCost("Guck", 15),
                new OfferingCost("Thistle", 10)));

            Register(new WardRiteDefinition(
                "freya_3_taming_speed", DeityType.Freya, 3,
                "Soothing Caress", "ᛈ",
                "Acclimating and taming wild beasts progresses twice as fast (+100%).",
                new OfferingCost("FreezeGland", 20),
                new OfferingCost("WolfPelt", 10),
                new OfferingCost("Cloudberry", 5)));

            Register(new WardRiteDefinition(
                "freya_4_comfort", DeityType.Freya, 4,
                "Cat-Drawn Carriage", "ᛟ",
                "Base Comfort level is increased by +3 within the ward sphere (+3 min Rested duration).",
                new OfferingCost("Tar", 15),
                new OfferingCost("LoxPelt", 10),
                new OfferingCost("Flax", 15)));

            Register(new WardRiteDefinition(
                "freya_5_tool_stamina", DeityType.Freya, 5,
                "Sanctuary of Fólkvangr", "ᛝ",
                "Tool stamina efficiency (hammer, hoe, cultivator, axes, pickaxes) is increased by +80% within the perimeter.",
                new OfferingCost("Softtissue", 15),
                new OfferingCost("Sap", 15),
                new OfferingCost("AsksvinHide", 5)));
        }
    }
}
