using System.Text;
using HarmonyLib;
using UnityEngine;
using VikingWards.Configuration;
using VikingWards.Conduits;
using VikingWards.Effects;
using VikingWards.UI;

namespace VikingWards.Patches
{
    [HarmonyPatch(typeof(PrivateArea))]
    public static class PrivateAreaPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake_Postfix(PrivateArea __instance)
        {
            WardConduitManager.SyncPieceResources(__instance);
            WardConduitManager.ApplyVisualAndRadius(__instance);
        }

        [HarmonyPatch(nameof(PrivateArea.GetHoverText))]
        [HarmonyPostfix]
        public static void GetHoverText_Postfix(PrivateArea __instance, ref string __result)
        {
            if (__instance == null) return;

            var god = WardConduitManager.GetActiveGod(__instance);
            int count = WardConduitManager.GetUnlockedCountForDeity(__instance, god);
            float radius = WardConduitManager.CalculateRadius(__instance);

            var sb = new StringBuilder(__result);
            sb.AppendLine();
            sb.AppendLine();
            if (god == DeityType.None)
            {
                sb.AppendLine("<color=#A0A0A0>Divine Conduit: Unconsecrated</color>");
            }
            else
            {
                string godColorHex = ColorUtility.ToHtmlStringRGB(ModConfig.GetDeityColor(god));
                sb.AppendLine($"<color=#{godColorHex}>Patron Deity: {god} (Runes: {count}/5 | Radius: {radius:F0}m)</color>");
            }
            sb.Append(Localization.instance.Localize("[<color=yellow><b>Left Shift + $KEY_Use</b></color>] Commune with the Gods"));
            __result = sb.ToString();
        }

        [HarmonyPatch(nameof(PrivateArea.Interact))]
        [HarmonyPrefix]
        public static bool Interact_Prefix(PrivateArea __instance, Humanoid human, bool hold, bool alt, ref bool __result)
        {
            if (hold) return true;

            // Check if player is holding LeftShift + E to commune
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (human is Player player)
                {
                    WardSlateDialog.Open(__instance, player);
                    __result = true;
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch("SetEnabled")]
        [HarmonyPrefix]
        public static bool SetEnabled_Prefix(PrivateArea __instance, bool enabled)
        {
            if (enabled)
            {
                if (WardConduitManager.CheckOverlapWithOtherActiveWards(__instance))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Cannot activate ward: Divine boundary overlaps another active ward!");
                    }
                    return false; // Prevent activation
                }
            }
            return true;
        }

        [HarmonyPatch("UpdateStatus")]
        [HarmonyPostfix]
        public static void UpdateStatus_Postfix(PrivateArea __instance)
        {
            WardConduitManager.ApplyVisualAndRadius(__instance);

            if (Player.m_localPlayer != null)
            {
                WardStatSyncService.UpdatePlayerStats(Player.m_localPlayer);
            }
        }
    }
}
