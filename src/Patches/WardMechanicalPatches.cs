using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using VikingWards.Conduits;

namespace VikingWards.Patches
{
    // ==========================================
    // 1. WearNTear (Thor T1 Armor & Storm Weather Decay)
    // ==========================================
    [HarmonyPatch(typeof(WearNTear))]
    public static class WearNTearWardPatch
    {
        [HarmonyPatch(nameof(WearNTear.Damage))]
        [HarmonyPrefix]
        public static void Damage_Prefix(WearNTear __instance, ref HitData hit)
        {
            if (__instance == null || hit == null) return;

            // Thor Tier 1: +35 Flat Piece Armor
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Thor, 1))
            {
                hit.ApplyArmor(35f);
            }
        }

        [HarmonyPatch(nameof(WearNTear.IsWet))]
        [HarmonyPostfix]
        public static void IsWet_Postfix(WearNTear __instance, ref bool __result)
        {
            // Thor Tier 3: Rain and weather cannot decay wood pieces
            if (__result && WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Thor, 3))
            {
                __result = false;
            }
        }
    }

    // ==========================================
    // 2. Fireplace (Thor T3: Storms never extinguish fires)
    // ==========================================
    [HarmonyPatch(typeof(Fireplace))]
    public static class FireplaceWardPatch
    {
        [HarmonyPatch("CheckWet")]
        [HarmonyPrefix]
        public static bool CheckWet_Prefix(Fireplace __instance)
        {
            if (__instance == null) return true;
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Thor, 3))
            {
                // Rain and storms cannot extinguish fires under Thor's ward
                return false;
            }
            return true;
        }
    }

    // ==========================================
    // 3. EnvMan (Thor T3: Weather Immunity - Freezing, Cold, Wet)
    // ==========================================
    [HarmonyPatch(typeof(EnvMan))]
    public static class EnvManWardPatch
    {
        [HarmonyPatch("CalculateCold")]
        [HarmonyPostfix]
        public static void CalculateCold_Postfix(ref bool __result)
        {
            if (__result && Player.m_localPlayer != null)
            {
                if (WardConduitManager.IsPlayerInsideActiveWard(Player.m_localPlayer, DeityType.Thor, 3))
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch("CalculateFreezing")]
        [HarmonyPostfix]
        public static void CalculateFreezing_Postfix(ref bool __result)
        {
            if (__result && Player.m_localPlayer != null)
            {
                if (WardConduitManager.IsPlayerInsideActiveWard(Player.m_localPlayer, DeityType.Thor, 3))
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch("CalculateWet")]
        [HarmonyPostfix]
        public static void CalculateWet_Postfix(ref bool __result)
        {
            if (__result && Player.m_localPlayer != null)
            {
                if (WardConduitManager.IsPlayerInsideActiveWard(Player.m_localPlayer, DeityType.Thor, 3))
                {
                    __result = false;
                }
            }
        }
    }

    // ==========================================
    // 4. Character / Player Fall Damage (Odin T3) & Armor (Tyr T2)
    // ==========================================
    [HarmonyPatch(typeof(Character))]
    public static class CharacterWardPatch
    {
        [HarmonyPatch(nameof(Character.Damage))]
        [HarmonyPrefix]
        public static void Damage_Prefix(Character __instance, HitData hit)
        {
            if (__instance == null || hit == null) return;

            // Odin Tier 3: Zero Fall Damage
            if (hit.m_hitType == HitData.HitType.Fall)
            {
                if (__instance is Player player && WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Odin, 3))
                {
                    hit.m_damage.m_damage = 0f;
                    return;
                }
            }
        }

        [HarmonyPatch(nameof(Character.GetBodyArmor))]
        [HarmonyPostfix]
        public static void GetBodyArmor_Postfix(Character __instance, ref float __result)
        {
            // Tyr Tier 2: +30 Flat Body Armor for permitted defenders
            if (__instance is Player player && WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Tyr, 2))
            {
                __result += 30f;
            }
        }
    }

    // ==========================================
    // 5. MonsterAI (Odin T2 Aggro Cloak & Tyr T5 Slow Field)
    // ==========================================
    [HarmonyPatch(typeof(MonsterAI))]
    public static class MonsterAIWardPatch
    {
        [HarmonyPatch(nameof(MonsterAI.UpdateAI))]
        [HarmonyPrefix]
        public static void UpdateAI_Prefix(MonsterAI __instance)
        {
            if (__instance == null) return;
            var character = __instance.GetComponent<Character>();
            if (character == null) return;

            // Tyr Tier 5: Hostile creatures inside the ward take 35% movement slow
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Tyr, 5))
            {
                if (!character.IsTamed())
                {
                    character.m_speed = character.m_walkSpeed * 0.65f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(BaseAI))]
    public static class BaseAIWardPatch
    {
        [HarmonyPatch(nameof(BaseAI.CanSeeTarget), new Type[] { typeof(Character) })]
        [HarmonyPostfix]
        public static void CanSeeTarget_Postfix(BaseAI __instance, Character target, ref bool __result)
        {
            if (__result && target is Player player)
            {
                // Odin Tier 2: 85% reduced detection range against wild roaming creatures
                if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Odin, 2))
                {
                    float dist = Vector3.Distance(__instance.transform.position, player.transform.position);
                    float baseRange = __instance.m_viewRange;
                    if (dist > (baseRange * 0.15f))
                    {
                        __result = false; // Cloaked by Odin's shroud
                    }
                }
            }
        }
    }

    // ==========================================
    // 6. RandEventSystem (Odin T4: 65% Raid Frequency Reduction)
    // ==========================================
    [HarmonyPatch(typeof(RandEventSystem))]
    public static class RandEventWardPatch
    {
        [HarmonyPatch("GetValidEventPoints")]
        [HarmonyPostfix]
        public static void GetValidEventPoints_Postfix(RandomEvent ev, List<RandEventSystem.PlayerEventData> characters, ref List<Vector3> __result)
        {
            if (__result == null || __result.Count == 0) return;

            var filtered = new List<Vector3>(__result.Count);
            foreach (var point in __result)
            {
                if (WardConduitManager.IsPositionInsideActiveWard(point, DeityType.Odin, 4))
                {
                    // 65% chance to ignore this base for random raids
                    if (UnityEngine.Random.value < 0.65f)
                    {
                        continue;
                    }
                }
                filtered.Add(point);
            }
            __result = filtered;
        }
    }

    // ==========================================
    // 7. Plant Growth (Frey T1: 35% Faster Crops)
    // ==========================================
    [HarmonyPatch(typeof(Plant))]
    public static class PlantWardPatch
    {
        [HarmonyPatch("TimeSincePlanted")]
        [HarmonyPostfix]
        public static void TimeSincePlanted_Postfix(Plant __instance, ref double __result)
        {
            if (__instance == null) return;
            // Frey Tier 1: 35% faster crop growth (time accelerates by 1.35x)
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Frey, 1))
            {
                __result *= 1.35;
            }
        }
    }

    // ==========================================
    // 8. Food Duration Freeze (Frey T2)
    // ==========================================
    [HarmonyPatch(typeof(Player))]
    public static class PlayerFoodWardPatch
    {
        [HarmonyPatch("UpdateFood")]
        [HarmonyPrefix]
        public static void UpdateFood_Prefix(Player __instance, ref float dt, bool forceUpdate)
        {
            // Frey Tier 2: Frozen food duration while standing inside active ward
            if (!forceUpdate && WardConduitManager.IsPlayerInsideActiveWard(__instance, DeityType.Frey, 2))
            {
                dt = 0f;
            }
        }
    }

    // ==========================================
    // 9. CookingStation & Fermenter (Frey T3: 60% Faster)
    // ==========================================
    [HarmonyPatch(typeof(CookingStation))]
    public static class CookingStationWardPatch
    {
        [HarmonyPatch("GetDeltaTime")]
        [HarmonyPostfix]
        public static void GetDeltaTime_Postfix(CookingStation __instance, ref float __result)
        {
            if (__instance == null) return;
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Frey, 3))
            {
                __result *= 1.60f; // 60% faster cooking
            }
        }
    }

    [HarmonyPatch(typeof(Fermenter))]
    public static class FermenterWardPatch
    {
        [HarmonyPatch("GetFermentationTime")]
        [HarmonyPostfix]
        public static void GetFermentationTime_Postfix(Fermenter __instance, ref double __result)
        {
            if (__instance == null) return;
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Frey, 3))
            {
                __result *= 1.60; // 60% faster fermentation
            }
        }
    }

    // ==========================================
    // 10. Smelter / Windmill (Frey T4: Double Speed)
    // ==========================================
    [HarmonyPatch(typeof(Smelter))]
    public static class SmelterWardPatch
    {
        [HarmonyPatch("GetDeltaTime")]
        [HarmonyPostfix]
        public static void GetDeltaTime_Postfix(Smelter __instance, ref double __result)
        {
            if (__instance == null || __instance.m_windmill == null) return; // Only affect windmills
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Frey, 4))
            {
                __result *= 2.0; // Double speed windmill
            }
        }
    }

    // ==========================================
    // 11. Skills XP (Odin T5: +50% Skill XP)
    // ==========================================
    [HarmonyPatch(typeof(Skills))]
    public static class SkillsWardPatch
    {
        [HarmonyPatch(nameof(Skills.RaiseSkill))]
        [HarmonyPrefix]
        public static void RaiseSkill_Prefix(Skills __instance, Skills.SkillType skillType, ref float factor)
        {
            var player = __instance.GetComponent<Player>();
            if (player != null)
            {
                if (WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Odin, 5))
                {
                    factor *= 1.50f; // +50% skill XP
                }
            }
        }
    }

    // ==========================================
    // 12. Procreation & Tameable (Freya T1 & T3)
    // ==========================================
    [HarmonyPatch(typeof(Procreation))]
    public static class ProcreationWardPatch
    {
        [HarmonyPatch("IsDue")]
        [HarmonyPostfix]
        public static void IsDue_Postfix(Procreation __instance, ref bool __result)
        {
            if (!__result && __instance != null)
            {
                // Freya Tier 1: 60% faster procreation
                if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Freya, 1))
                {
                    // Check if pregnancy has reached 60% reduced threshold
                    var nview = __instance.GetComponent<ZNetView>();
                    if (nview != null && nview.IsValid())
                    {
                        long pregnantTicks = nview.GetZDO().GetLong(ZDOVars.s_pregnant, 0L);
                        if (pregnantTicks > 0)
                        {
                            var timeSpan = ZNet.instance.GetTime() - new DateTime(pregnantTicks);
                            if (timeSpan.TotalSeconds > (__instance.m_pregnancyDuration * 0.40f))
                            {
                                __result = true;
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Tameable))]
    public static class TameableWardPatch
    {
        private static readonly Action<Tameable, float> s_decreaseRemainingTime =
            AccessTools.MethodDelegate<Action<Tameable, float>>(AccessTools.Method(typeof(Tameable), "DecreaseRemainingTime"));

        [HarmonyPatch("TamingUpdate")]
        [HarmonyPrefix]
        public static void TamingUpdate_Prefix(Tameable __instance)
        {
            if (__instance == null) return;
            var nview = __instance.GetComponent<ZNetView>();
            if (nview == null || !nview.IsValid() || !nview.IsOwner()) return;

            // Freya Tier 3: Taming progress 2x as fast
            if (WardConduitManager.IsPositionInsideActiveWard(__instance.transform.position, DeityType.Freya, 3))
            {
                if (__instance.IsHungry())
                {
                    // Decrease remaining time by an extra 3 seconds per tick
                    s_decreaseRemainingTime(__instance, 3.0f);
                }
            }
        }
    }

    // ==========================================
    // 13. Comfort Level (Freya T4: +3 Comfort)
    // ==========================================
    [HarmonyPatch(typeof(SE_Rested))]
    public static class SERestedWardPatch
    {
        [HarmonyPatch(nameof(SE_Rested.CalculateComfortLevel), new Type[] { typeof(Player) })]
        [HarmonyPostfix]
        public static void CalculateComfortLevel_Postfix(Player player, ref int __result)
        {
            if (player != null && WardConduitManager.IsPlayerInsideActiveWard(player, DeityType.Freya, 4))
            {
                __result += 3; // +3 Comfort
            }
        }
    }
}
