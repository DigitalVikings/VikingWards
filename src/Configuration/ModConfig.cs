using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using VikingWards.Conduits;

namespace VikingWards.Configuration
{
    public static class ModConfig
    {
        public static ConfigEntry<bool>? DynamicRadius;
        public static ConfigEntry<float>? WardRadius;

        public static float BaseRadius = 32f;

        public static void Init(ConfigFile config)
        {
            DynamicRadius = config.Bind(
                "General",
                "DynamicRadius",
                false,
                new ConfigDescription(
                    "If true, ward radius scales dynamically with the number of consecrated runes (32m to 68m). If false, the fixed WardRadius slider value is used.",
                    null,
                    new ConfigurationManagerAttributes { Order = 10 }
                )
            );

            WardRadius = config.Bind(
                "General",
                "WardRadius",
                45f,
                new ConfigDescription(
                    "Radius of the consecrated ward zone in meters.",
                    new AcceptableValueRange<float>(32f, 68f),
                    new ConfigurationManagerAttributes { Order = 9 }
                )
            );

            WardRadius.SettingChanged += (sender, args) =>
            {
                WardConduitManager.RefreshAllWards();
            };
            DynamicRadius.SettingChanged += (sender, args) =>
            {
                WardConduitManager.RefreshAllWards();
            };
        }

        public static float GetWardRadius(int runeCount = 0)
        {
            if (DynamicRadius != null && DynamicRadius.Value)
            {
                return GetRadiusForRuneCount(runeCount);
            }
            return WardRadius?.Value ?? 45f;
        }

        public static float GetRadiusForRuneCount(int count)
        {
            return count switch
            {
                <= 0 => 32f,
                1 => 38f,
                2 => 44f,
                3 => 50f,
                4 => 58f,
                _ => 68f
            };
        }

        public static Color GetDeityColor(DeityType deity)
        {
            return deity switch
            {
                DeityType.Odin => new Color(0.35f, 0.65f, 1.00f, 1f),    // Sapphire Blue (Wisdom)
                DeityType.Thor => new Color(1.00f, 0.82f, 0.15f, 1f),    // Lightning Gold (Storms & Thunder)
                DeityType.Frey => new Color(0.30f, 0.88f, 0.45f, 1f),    // Harvest Emerald (Fertility & Bounty)
                DeityType.Tyr => new Color(0.95f, 0.25f, 0.25f, 1f),     // Martial Crimson (Valor & War)
                DeityType.Freya => new Color(0.95f, 0.35f, 0.70f, 1f),   // Sylvan Rose (Life & Love)
                _ => Color.white
            };
        }
    }
}
