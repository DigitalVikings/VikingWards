using System.Collections.Generic;
using UnityEngine;
using VikingLib.Combat;
using VikingLib.Stats;
using VikingWards.Conduits;

namespace VikingWards.Effects
{
    public static class WardStatSyncService
    {
        public const string OwnerKey = "VikingWards";

        public static void UpdatePlayerStats(Player player)
        {
            if (player == null) return;

            var mods = new List<StatModifier>();

            // --- THOR CONDUIT ---
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Thor, 2))
            {
                // Tier 2: Charged Stride (+30% move speed)
                mods.Add(StatModifier.Increased(CharacterStats.RunSpeed, 0.30f, "Thor Charged Stride", owner: OwnerKey));
                mods.Add(StatModifier.Increased(CharacterStats.JogSpeed, 0.30f, "Thor Charged Stride", owner: OwnerKey));
            }
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Thor, 5))
            {
                // Tier 5: Forge of Thrudvang (+15% material save / CraftingYield)
                mods.Add(StatModifier.Increased(CharacterStats.CraftingYield, 0.15f, "Thor Forge of Thrudvang", owner: OwnerKey));
            }

            // --- FREY CONDUIT ---
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Frey, 5))
            {
                // Tier 5: Feast of Skirnir (+30% Foraging Yield)
                mods.Add(StatModifier.Increased(CharacterStats.ForagingYield, 0.30f, "Frey Feast of Skirnir", owner: OwnerKey));
            }

            // --- TYR CONDUIT ---
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Tyr, 1))
            {
                // Tier 1: Vanguard Stance (+30% damage)
                mods.Add(StatModifier.Increased(CharacterStats.DamagePhysical, 0.30f, "Tyr Vanguard Stance", owner: OwnerKey));
            }
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Tyr, 3))
            {
                // Tier 3: Unyielding Endurance (+35% Attack Stamina Efficiency)
                mods.Add(StatModifier.Increased(CharacterStats.MeleeStaminaEfficiency, 0.35f, "Tyr Endurance", owner: OwnerKey));
                mods.Add(StatModifier.Increased(CharacterStats.RangedStaminaEfficiency, 0.35f, "Tyr Endurance", owner: OwnerKey));
            }
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Tyr, 4))
            {
                // Tier 4: Tyr's Judgment (+15 HP, +30 Stam on Kill)
                mods.Add(StatModifier.Flat(CharacterStats.HealthOnKill, 15f, "Tyr Judgment", owner: OwnerKey));
                mods.Add(StatModifier.Flat(CharacterStats.StaminaOnKill, 30f, "Tyr Judgment", owner: OwnerKey));
            }

            // --- FREYA CONDUIT ---
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Freya, 2))
            {
                // Tier 2: Sylvan Shelter (+60% health regen)
                mods.Add(StatModifier.Increased(CharacterStats.HealthRegen, 0.60f, "Freya Sylvan Shelter", owner: OwnerKey));
            }
            if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Freya, 5))
            {
                // Tier 5: Sanctuary of Folkvangr (+80% Tool Stamina Efficiency)
                mods.Add(StatModifier.Increased(CharacterStats.ToolStaminaEfficiency, 0.80f, "Freya Folkvangr", owner: OwnerKey));
            }

            // Atomic writeback to VikingLib
            CharacterStatManager.ReplaceModifiersByOwner(player, OwnerKey, mods);
        }
    }
}
