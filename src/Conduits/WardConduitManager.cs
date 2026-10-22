using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using VikingWards.Configuration;

namespace VikingWards.Conduits
{
    public static class WardConduitManager
    {
        public const string ZDO_GodKey = "VL_Ward_God";
        public const string ZDO_UnlocksKey = "VL_Ward_Unlocks";

        private static readonly AccessTools.FieldRef<PrivateArea, ZNetView> s_nviewRef =
            AccessTools.FieldRefAccess<PrivateArea, ZNetView>("m_nview");

        private static readonly AccessTools.FieldRef<PrivateArea, Piece> s_pieceRef =
            AccessTools.FieldRefAccess<PrivateArea, Piece>("m_piece");

        private static readonly System.Reflection.FieldInfo s_allAreasField =
            AccessTools.Field(typeof(PrivateArea), "m_allAreas");

        private static readonly Func<PrivateArea, long, bool> s_isPermittedRef =
            AccessTools.MethodDelegate<Func<PrivateArea, long, bool>>(AccessTools.Method(typeof(PrivateArea), "IsPermitted"));

        public static ZNetView? GetNetView(PrivateArea ward)
        {
            if (ward == null) return null;
            return s_nviewRef(ward) ?? ward.GetComponent<ZNetView>();
        }

        public static Piece? GetPiece(PrivateArea ward)
        {
            if (ward == null) return null;
            return s_pieceRef(ward) ?? ward.GetComponent<Piece>();
        }

        public static List<PrivateArea> GetAllAreas()
        {
            return (List<PrivateArea>?)s_allAreasField?.GetValue(null) ?? new List<PrivateArea>();
        }

        public static bool IsAreaEnabled(PrivateArea ward)
        {
            if (ward == null) return false;
            var nv = GetNetView(ward);
            return nv != null && nv.IsValid() && nv.GetZDO().GetBool(ZDOVars.s_enabled, ward.m_enabledByDefault);
        }

        public static bool IsPlayerPermitted(PrivateArea ward, long playerId)
        {
            if (ward == null) return false;
            return s_isPermittedRef(ward, playerId);
        }

        public static DeityType GetActiveGod(PrivateArea ward)
        {
            var nv = GetNetView(ward);
            if (nv == null || !nv.IsValid()) return DeityType.None;
            string godStr = nv.GetZDO().GetString(ZDO_GodKey, "");
            if (Enum.TryParse<DeityType>(godStr, true, out var deity))
            {
                return deity;
            }
            return DeityType.None;
        }

        public static void SetActiveGod(PrivateArea ward, DeityType deity)
        {
            var nv = GetNetView(ward);
            if (nv == null || !nv.IsValid() || !nv.IsOwner()) return;
            nv.GetZDO().Set(ZDO_GodKey, deity.ToString());
            ApplyVisualAndRadius(ward);
        }

        public static HashSet<string> GetUnlockedRites(PrivateArea ward)
        {
            var set = new HashSet<string>();
            var nv = GetNetView(ward);
            if (nv == null || !nv.IsValid()) return set;
            string raw = nv.GetZDO().GetString(ZDO_UnlocksKey, "");
            if (string.IsNullOrEmpty(raw)) return set;

            string[] tokens = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                set.Add(t.Trim());
            }
            return set;
        }

        public static bool IsRiteUnlocked(PrivateArea ward, string riteId)
        {
            return GetUnlockedRites(ward).Contains(riteId);
        }

        public static bool UnlockRite(PrivateArea ward, Player player, WardRiteDefinition rite)
        {
            var nv = GetNetView(ward);
            if (nv == null || !nv.IsValid() || player == null) return false;
            if (!nv.IsOwner())
            {
                nv.ClaimOwnership();
            }

            var unlocks = GetUnlockedRites(ward);
            if (unlocks.Contains(rite.Id)) return false;

            // Check if player has inventory items (matching on ItemData.m_shared.m_name)
            var inv = player.GetInventory();
            if (inv == null) return false;

            var itemDB = ObjectDB.instance;
            if (itemDB == null) return false;

            foreach (var cost in rite.Costs)
            {
                var prefab = itemDB.GetItemPrefab(cost.ItemPrefab);
                if (prefab == null || !prefab.TryGetComponent<ItemDrop>(out var drop)) return false;
                string sharedName = drop.m_itemData.m_shared.m_name;

                int count = inv.CountItems(sharedName);
                if (count < cost.Amount)
                {
                    player.Message(MessageHud.MessageType.Center, $"Missing required offering: {cost.Amount}x {Localization.instance.Localize(sharedName)}");
                    return false;
                }
            }

            // Consume items from player inventory
            foreach (var cost in rite.Costs)
            {
                var prefab = itemDB.GetItemPrefab(cost.ItemPrefab);
                if (prefab != null && prefab.TryGetComponent<ItemDrop>(out var drop))
                {
                    inv.RemoveItem(drop.m_itemData.m_shared.m_name, cost.Amount);
                }
            }

            unlocks.Add(rite.Id);
            string serialized = string.Join(",", unlocks);
            nv.GetZDO().Set(ZDO_UnlocksKey, serialized);

            // Rebuild Piece.m_resources so native Piece.DropResources drops 100% of sacrifices
            SyncPieceResources(ward);
            ApplyVisualAndRadius(ward);

            // Audio & VFX feedback
            ward.m_addPermittedEffect?.Create(ward.transform.position, Quaternion.identity);
            player.Message(MessageHud.MessageType.Center, $"Consecrated: {rite.Name} ({rite.RuneGlyph})");
            return true;
        }

        public static int GetUnlockedCountForDeity(PrivateArea ward, DeityType deity)
        {
            var unlocks = GetUnlockedRites(ward);
            int count = 0;
            foreach (var rite in WardRiteRegistry.GetForDeity(deity))
            {
                if (unlocks.Contains(rite.Id)) count++;
            }
            return count;
        }

        public static int GetTotalUnlockedRunes(PrivateArea ward)
        {
            return GetUnlockedRites(ward).Count;
        }

        public static float CalculateRadius(PrivateArea ward)
        {
            if (ward == null) return ModConfig.GetWardRadius(0);
            var activeGod = GetActiveGod(ward);
            if (activeGod == DeityType.None) return ModConfig.BaseRadius;

            int count = GetUnlockedCountForDeity(ward, activeGod);
            return ModConfig.GetWardRadius(count);
        }

        public static void RefreshAllWards()
        {
            foreach (var area in GetAllAreas())
            {
                if (area != null)
                {
                    ApplyVisualAndRadius(area);
                }
            }
        }

        public static void SyncPieceResources(PrivateArea ward)
        {
            if (ward == null) return;
            var piece = GetPiece(ward);
            if (piece == null) return;

            var baseReqs = new List<Piece.Requirement>();
            if (piece.m_resources != null)
            {
                foreach (var req in piece.m_resources)
                {
                    if (req?.m_resItem != null)
                    {
                        baseReqs.Add(req);
                    }
                }
            }

            var unlocks = GetUnlockedRites(ward);
            var itemDB = ObjectDB.instance;
            if (itemDB == null) return;

            var aggregatedOfferings = new Dictionary<string, int>();
            foreach (var riteId in unlocks)
            {
                var rite = WardRiteRegistry.GetById(riteId);
                if (rite == null) continue;
                foreach (var cost in rite.Costs)
                {
                    if (!aggregatedOfferings.TryGetValue(cost.ItemPrefab, out int val)) val = 0;
                    aggregatedOfferings[cost.ItemPrefab] = val + cost.Amount;
                }
            }

            var newReqList = new List<Piece.Requirement>(baseReqs);
            foreach (var kvp in aggregatedOfferings)
            {
                var prefab = itemDB.GetItemPrefab(kvp.Key);
                if (prefab != null && prefab.TryGetComponent<ItemDrop>(out var drop))
                {
                    newReqList.Add(new Piece.Requirement
                    {
                        m_resItem = drop,
                        m_amount = kvp.Value,
                        m_recover = true
                    });
                }
            }

            piece.m_resources = newReqList.ToArray();
        }

        public static void ApplyVisualAndRadius(PrivateArea ward)
        {
            if (ward == null) return;
            float targetRadius = CalculateRadius(ward);
            ward.m_radius = targetRadius;

            if (ward.m_areaMarker != null)
            {
                ward.m_areaMarker.m_radius = targetRadius;
            }

            var activeGod = GetActiveGod(ward);
            Color godColor = (activeGod != DeityType.None) ? ModConfig.GetDeityColor(activeGod) : new Color(0f, 0.65f, 1f);

            // 1. Tint the illuminated ridges / fissures on the ward statue (Guardstone_OdenGlow_mat)
            if (ward.m_model != null)
            {
                var mats = ward.m_model.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    // The glowing cracks material has "Glow" in its name
                    if (m.name.IndexOf("Glow", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (activeGod != DeityType.None)
                        {
                            m.EnableKeyword("_EMISSION");
                            // 2.0x gives rich, vibrant colored emission in HDR bloom
                            m.SetColor("_EmissionColor", godColor * 2.0f);
                            m.SetColor("_Color", godColor);
                        }
                        else
                        {
                            // Reset to vanilla default amber glow
                            m.EnableKeyword("_EMISSION");
                            m.SetColor("_EmissionColor", new Color(1.934f, 1.017f, 0.185f, 1f));
                            m.SetColor("_Color", Color.white);
                        }
                    }
                }
                ward.m_model.materials = mats;
            }

            // 2. Ensure point light is a gentle ambient torchlight (no blinding light ball)
            var lights = ward.GetComponentsInChildren<Light>(true);
            foreach (var l in lights)
            {
                if (l != null)
                {
                    l.color = new Color(1.0f, 0.75f, 0.35f, 1f);
                    l.intensity = 2.5f;
                    l.range = 3.0f;
                }
            }

            // 3. Completely disable the large aura VFX (glow, pulse, flare) to eliminate the aura cloud
            var particleSystems = ward.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particleSystems)
            {
                if (ps == null) continue;
                string psName = ps.gameObject.name.ToLowerInvariant();
                if (psName.Contains("glow") || psName.Contains("pulse") || psName.Contains("flare"))
                {
                    var em = ps.emission;
                    em.enabled = false;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                }
                else
                {
                    // Restore native start color on tiny floating sparks
                    var main = ps.main;
                    main.startColor = new Color(1.0f, 0.5f, 0.1f, 1.0f);
                }
            }

            // 4. Tint circular ward boundary projector
            if (ward.m_areaMarker != null)
            {
                var rend = ward.m_areaMarker.GetComponent<LineRenderer>();
                if (rend != null)
                {
                    Color markerColor = (activeGod != DeityType.None) ? godColor : Color.white;
                    rend.startColor = markerColor;
                    rend.endColor = markerColor;
                }
            }
        }

        public static bool CheckOverlapWithOtherActiveWards(PrivateArea candidateWard)
        {
            if (candidateWard == null) return false;
            float candidateRadius = CalculateRadius(candidateWard);
            Vector3 pos = candidateWard.transform.position;

            foreach (var area in GetAllAreas())
            {
                if (area == null || area == candidateWard) continue;
                if (!IsAreaEnabled(area)) continue;

                float otherRadius = CalculateRadius(area);
                float dist = Vector3.Distance(pos, area.transform.position);
                // Wards cannot overlap: if radius is 45m, clearance must be 45m + 45m = 90m
                if (dist < (candidateRadius + otherRadius))
                {
                    return true; // Overlap detected!
                }
            }

            return false;
        }

        public static bool IsPositionInsideActiveWard(Vector3 pos, DeityType deity, int minTier = 1)
        {
            foreach (var area in GetAllAreas())
            {
                if (area == null || !IsAreaEnabled(area)) continue;
                if (GetActiveGod(area) != deity) continue;

                float r = CalculateRadius(area);
                if (Vector3.Distance(pos, area.transform.position) <= r)
                {
                    if (GetUnlockedCountForDeity(area, deity) >= minTier)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsPlayerInsideActiveWard(Player player, DeityType deity, int minTier = 1)
        {
            if (player == null) return false;
            Vector3 pos = player.transform.position;

            foreach (var area in GetAllAreas())
            {
                if (area == null || !IsAreaEnabled(area)) continue;
                if (GetActiveGod(area) != deity) continue;

                float r = CalculateRadius(area);
                if (Vector3.Distance(pos, area.transform.position) <= r)
                {
                    if (IsPlayerPermitted(area, player.GetPlayerID()))
                    {
                        if (GetUnlockedCountForDeity(area, deity) >= minTier)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
