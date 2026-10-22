using HarmonyLib;
using UnityEngine;
using VikingWards.Configuration;
using VikingWards.Conduits;

namespace VikingWards.Patches
{
    [HarmonyPatch(typeof(Player))]
    public static class PlayerPlacementPatch
    {
        private static readonly AccessTools.FieldRef<Player, GameObject> _ghostRef =
            AccessTools.FieldRefAccess<Player, GameObject>("m_placementGhost");

        private static readonly AccessTools.FieldRef<Player, Player.PlacementStatus> _statusRef =
            AccessTools.FieldRefAccess<Player, Player.PlacementStatus>("m_placementStatus");

        [HarmonyPatch("UpdatePlacementGhost")]
        [HarmonyPostfix]
        public static void UpdatePlacementGhost_Postfix(Player __instance)
        {
            if (__instance != Player.m_localPlayer) return;

            var ghost = _ghostRef(__instance);
            if (ghost == null) return;

            var ghostWard = ghost.GetComponent<PrivateArea>();
            if (ghostWard == null) return;

            Vector3 ghostPos = ghost.transform.position;
            float candidateRadius = ModConfig.GetWardRadius();

            // 1. Show the boundary marker of the placement ghost itself
            if (ghostWard.m_areaMarker != null)
            {
                ghostWard.m_areaMarker.gameObject.SetActive(true);
                ghostWard.m_areaMarker.m_radius = candidateRadius;
            }

            // 2. Reveal boundaries of all active/standing wards within visual range (200m)
            bool tooClose = false;
            foreach (var area in WardConduitManager.GetAllAreas())
            {
                if (area == null || !WardConduitManager.IsAreaEnabled(area)) continue;

                float areaRadius = WardConduitManager.CalculateRadius(area);
                float requiredDist = candidateRadius + areaRadius; // e.g. 45m + 45m = 90m
                float dist = Vector3.Distance(ghostPos, area.transform.position);

                if (dist <= 200f)
                {
                    area.ShowAreaMarker();
                }

                // If ghost is placed within overlapping boundary distance (e.g. < 90m)
                if (dist < requiredDist)
                {
                    tooClose = true;
                }
            }

            // 3. If too close to another active ward, turn the placement ghost RED and invalidate placement
            if (tooClose)
            {
                _statusRef(__instance) = Player.PlacementStatus.PrivateZone;
                var piece = ghost.GetComponent<Piece>();
                if (piece != null)
                {
                    piece.SetInvalidPlacementHeightlight(true);
                }
            }
        }

        [HarmonyPatch("TryPlacePiece")]
        [HarmonyPrefix]
        public static bool TryPlacePiece_Prefix(Player __instance, Piece piece, ref bool __result)
        {
            if (__instance != Player.m_localPlayer) return true;
            if (piece == null || piece.GetComponent<PrivateArea>() == null) return true;

            var ghost = _ghostRef(__instance);
            if (ghost == null) return true;

            Vector3 ghostPos = ghost.transform.position;
            float candidateRadius = ModConfig.GetWardRadius();

            foreach (var area in WardConduitManager.GetAllAreas())
            {
                if (area == null || !WardConduitManager.IsAreaEnabled(area)) continue;

                float areaRadius = WardConduitManager.CalculateRadius(area);
                float requiredDist = candidateRadius + areaRadius; // e.g. 45m + 45m = 90m
                float dist = Vector3.Distance(ghostPos, area.transform.position);

                if (dist < requiredDist)
                {
                    __instance.Message(MessageHud.MessageType.Center, $"<color=red>Cannot place ward: Must be at least {requiredDist:F0}m from other active wards!</color>");
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}
