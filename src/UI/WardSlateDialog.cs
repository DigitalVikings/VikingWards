using System;
using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using VikingWards.Configuration;
using VikingWards.Conduits;

namespace VikingWards.UI
{
    public sealed class WardSlateDialog : MonoBehaviour
    {
        private static WardSlateDialog? _instance;
        private static GameObject? _uiRoot;

        private PrivateArea? _targetWard;
        private Player? _player;
        private DeityType _selectedDeity = DeityType.Odin;
        private int _selectedSlotIndex = 0;
        private float _openTime;

        private GameObject? _dialogPanel;
        private Text? _titleText;
        private Text? _statusText;

        // Selected rite details
        private Text? _riteTitleText;
        private Text? _riteDescText;
        private GameObject? _reqContainer;
        private readonly List<RequirementCard> _reqCards = new();

        // Bottom action buttons
        private Button? _dedicateBtn;
        private Text? _dedicateBtnText;
        private Button? _consecrateBtn;
        private Text? _consecrateBtnText;

        private readonly List<Button> _godButtons = new();
        private readonly List<Button> _slotButtons = new();
        private readonly List<Text> _slotTexts = new();

        private class RequirementCard
        {
            public GameObject Root = null!;
            public Image Icon = null!;
            public Text AmountText = null!;
            public Text NameText = null!;
        }

        private struct OfferingDisplayData
        {
            public string LocalizedName;
            public Sprite? Icon;
            public bool IsKnown;
            public int RequiredAmount;
            public int CurrentAmount;
        }

        public static bool IsOpen()
        {
            return _instance != null && _instance.gameObject.activeSelf;
        }

        public static void Open(PrivateArea ward, Player player)
        {
            if (_instance == null)
            {
                CreateUI();
            }

            if (_instance != null)
            {
                _instance._targetWard = ward;
                _instance._player = player;
                var currentGod = WardConduitManager.GetActiveGod(ward);
                _instance._selectedDeity = (currentGod != DeityType.None) ? currentGod : DeityType.Odin;
                _instance._selectedSlotIndex = 0;
                _instance.Show();
            }
        }

        public static void Close()
        {
            if (_instance != null)
            {
                _instance.Hide();
            }
        }

        private static void CreateUI()
        {
            Transform? parentTransform = null;
            if (GUIManager.CustomGUIFront != null)
            {
                parentTransform = GUIManager.CustomGUIFront.transform;
            }
            else if (Menu.instance != null)
            {
                parentTransform = Menu.instance.transform.parent;
            }

            if (parentTransform == null) return;

            _uiRoot = new GameObject("VikingWards_UI", typeof(RectTransform));
            _uiRoot.transform.SetParent(parentTransform, false);

            var rect = _uiRoot.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            _instance = _uiRoot.AddComponent<WardSlateDialog>();
            _instance.BuildVisualHierarchy();
            _instance.Hide();
        }

        private void BuildVisualHierarchy()
        {
            Font font = GUIManager.Instance.AveriaSerifBold ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Dark modal background blocker (clicking outside closes the dialog)
            var blocker = new GameObject("Blocker", typeof(RectTransform), typeof(Image), typeof(Button));
            blocker.transform.SetParent(transform, false);
            var bRect = blocker.GetComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.sizeDelta = Vector2.zero;
            var bImg = blocker.GetComponent<Image>();
            bImg.color = new Color(0f, 0f, 0f, 0.65f);
            var bBtn = blocker.GetComponent<Button>();
            bBtn.onClick.AddListener(Close);

            // 2. Slate stone frame with Valheim woodpanel styling
            _dialogPanel = new GameObject("SlatePanel", typeof(RectTransform), typeof(Image));
            _dialogPanel.transform.SetParent(transform, false);
            var pRect = _dialogPanel.GetComponent<RectTransform>();
            pRect.sizeDelta = new Vector2(760f, 560f);
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            GUIManager.Instance.ApplyWoodpanelStyle(_dialogPanel.transform);

            // 3. Header Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(_dialogPanel.transform, false);
            var tRect = titleObj.GetComponent<RectTransform>();
            tRect.anchoredPosition = new Vector2(0f, 245f);
            tRect.sizeDelta = new Vector2(700f, 35f);
            _titleText = titleObj.GetComponent<Text>();
            _titleText.font = font;
            _titleText.fontSize = 24;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = GUIManager.Instance.ValheimOrange;

            // 4. Sub Status line
            var statObj = new GameObject("Status", typeof(RectTransform), typeof(Text));
            statObj.transform.SetParent(_dialogPanel.transform, false);
            var sRect = statObj.GetComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(0f, 212f);
            sRect.sizeDelta = new Vector2(700f, 25f);
            _statusText = statObj.GetComponent<Text>();
            _statusText.font = font;
            _statusText.fontSize = 13;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.color = GUIManager.Instance.ValheimBeige;

            // 5. Top Deity Selection Row
            float startX = -260f;
            float stepX = 130f;
            var deities = new[] { DeityType.Odin, DeityType.Thor, DeityType.Frey, DeityType.Tyr, DeityType.Freya };

            for (int i = 0; i < deities.Length; i++)
            {
                var d = deities[i];
                var godBtnObj = new GameObject($"GodBtn_{d}", typeof(RectTransform), typeof(Image), typeof(Button));
                godBtnObj.transform.SetParent(_dialogPanel.transform, false);
                var gRect = godBtnObj.GetComponent<RectTransform>();
                gRect.anchoredPosition = new Vector2(startX + (i * stepX), 162f);
                gRect.sizeDelta = new Vector2(115f, 36f);

                var btn = godBtnObj.GetComponent<Button>();
                GUIManager.Instance.ApplyButtonStyle(btn, 14);

                var btnTextObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
                btnTextObj.transform.SetParent(godBtnObj.transform, false);
                var btRect = btnTextObj.GetComponent<RectTransform>();
                btRect.sizeDelta = gRect.sizeDelta;
                var bt = btnTextObj.GetComponent<Text>();
                bt.font = font;
                bt.fontSize = 15;
                bt.alignment = TextAnchor.MiddleCenter;
                bt.text = d.ToString();
                bt.color = ModConfig.GetDeityColor(d);

                btn.onClick.AddListener(() =>
                {
                    _selectedDeity = d;
                    _selectedSlotIndex = 0;
                    RefreshUI();
                });
                _godButtons.Add(btn);
            }

            // 6. Horizontal Tier Progression Bar (Left to Right: Tier I -> Tier V)
            float[] tierXPositions = new[] { -250f, -125f, 0f, 125f, 250f };
            string[] tierRoman = new[] { "I", "II", "III", "IV", "V" };

            // Connecting arrows between tier boxes
            for (int i = 0; i < 4; i++)
            {
                var arrowObj = new GameObject($"TierArrow_{i}", typeof(RectTransform), typeof(Text));
                arrowObj.transform.SetParent(_dialogPanel.transform, false);
                var aRect = arrowObj.GetComponent<RectTransform>();
                aRect.anchoredPosition = new Vector2(-187.5f + (i * 125f), 78f);
                aRect.sizeDelta = new Vector2(30f, 30f);
                var aTxt = arrowObj.GetComponent<Text>();
                aTxt.font = font;
                aTxt.fontSize = 18;
                aTxt.alignment = TextAnchor.MiddleCenter;
                aTxt.text = "➔";
                aTxt.color = new Color(0.7f, 0.62f, 0.45f, 0.65f);
            }

            for (int i = 0; i < 5; i++)
            {
                int tierIndex = i + 1;
                var slotObj = new GameObject($"Slot_{tierIndex}", typeof(RectTransform), typeof(Image), typeof(Button));
                slotObj.transform.SetParent(_dialogPanel.transform, false);
                var rRect = slotObj.GetComponent<RectTransform>();
                rRect.anchoredPosition = new Vector2(tierXPositions[i], 78f);
                rRect.sizeDelta = new Vector2(100f, 64f);

                var btn = slotObj.GetComponent<Button>();
                GUIManager.Instance.ApplyButtonStyle(btn, 13);

                var txtObj = new GameObject("RuneText", typeof(RectTransform), typeof(Text));
                txtObj.transform.SetParent(slotObj.transform, false);
                var txRect = txtObj.GetComponent<RectTransform>();
                txRect.sizeDelta = rRect.sizeDelta;
                var txt = txtObj.GetComponent<Text>();
                txt.font = font;
                txt.fontSize = 15;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.text = $"Tier {tierRoman[i]}";
                txt.color = Color.white;

                int localIndex = i;
                btn.onClick.AddListener(() =>
                {
                    _selectedSlotIndex = localIndex;
                    RefreshUI();
                });

                _slotButtons.Add(btn);
                _slotTexts.Add(txt);
            }

            // 7. Selected Rite Title & Description
            var riteTitleObj = new GameObject("RiteTitle", typeof(RectTransform), typeof(Text));
            riteTitleObj.transform.SetParent(_dialogPanel.transform, false);
            var rtRect = riteTitleObj.GetComponent<RectTransform>();
            rtRect.anchoredPosition = new Vector2(0f, 15f);
            rtRect.sizeDelta = new Vector2(700f, 28f);
            _riteTitleText = riteTitleObj.GetComponent<Text>();
            _riteTitleText.font = font;
            _riteTitleText.fontSize = 17;
            _riteTitleText.alignment = TextAnchor.MiddleCenter;
            _riteTitleText.color = GUIManager.Instance.ValheimYellow;

            var riteDescObj = new GameObject("RiteDesc", typeof(RectTransform), typeof(Text));
            riteDescObj.transform.SetParent(_dialogPanel.transform, false);
            var rdRect = riteDescObj.GetComponent<RectTransform>();
            rdRect.anchoredPosition = new Vector2(0f, -22f);
            rdRect.sizeDelta = new Vector2(700f, 48f);
            _riteDescText = riteDescObj.GetComponent<Text>();
            _riteDescText.font = font;
            _riteDescText.fontSize = 13;
            _riteDescText.alignment = TextAnchor.MiddleCenter;
            _riteDescText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _riteDescText.verticalOverflow = VerticalWrapMode.Truncate;
            _riteDescText.color = GUIManager.Instance.ValheimBeige;

            // 8. Requirements Row (Crafting Table Workbench Style)
            _reqContainer = new GameObject("ReqContainer", typeof(RectTransform));
            _reqContainer.transform.SetParent(_dialogPanel.transform, false);
            var rcRect = _reqContainer.GetComponent<RectTransform>();
            rcRect.anchoredPosition = new Vector2(0f, -95f);
            rcRect.sizeDelta = new Vector2(700f, 50f);

            for (int i = 0; i < 4; i++)
            {
                var cardObj = new GameObject($"ReqCard_{i}", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(_reqContainer.transform, false);
                var cardRect = cardObj.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(130f, 46f);
                var cardImg = cardObj.GetComponent<Image>();
                cardImg.color = new Color(0.12f, 0.12f, 0.12f, 0.75f);

                // Icon frame
                var iconFrame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
                iconFrame.transform.SetParent(cardObj.transform, false);
                var ifRect = iconFrame.GetComponent<RectTransform>();
                ifRect.anchoredPosition = new Vector2(-42f, 0f);
                ifRect.sizeDelta = new Vector2(38f, 38f);
                var ifImg = iconFrame.GetComponent<Image>();
                ifImg.color = new Color(0.18f, 0.18f, 0.18f, 1f);

                // Item Icon
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(iconFrame.transform, false);
                var icRect = iconObj.GetComponent<RectTransform>();
                icRect.sizeDelta = new Vector2(34f, 34f);
                var icImg = iconObj.GetComponent<Image>();

                // Quantity Text
                var amtObj = new GameObject("Amount", typeof(RectTransform), typeof(Text));
                amtObj.transform.SetParent(cardObj.transform, false);
                var aRect = amtObj.GetComponent<RectTransform>();
                aRect.anchoredPosition = new Vector2(18f, 8f);
                aRect.sizeDelta = new Vector2(75f, 18f);
                var aText = amtObj.GetComponent<Text>();
                aText.font = font;
                aText.fontSize = 13;
                aText.alignment = TextAnchor.MiddleLeft;

                // Name Text
                var nameObj = new GameObject("Name", typeof(RectTransform), typeof(Text));
                nameObj.transform.SetParent(cardObj.transform, false);
                var nRect = nameObj.GetComponent<RectTransform>();
                nRect.anchoredPosition = new Vector2(18f, -9f);
                nRect.sizeDelta = new Vector2(75f, 16f);
                var nText = nameObj.GetComponent<Text>();
                nText.font = font;
                nText.fontSize = 11;
                nText.alignment = TextAnchor.MiddleLeft;

                _reqCards.Add(new RequirementCard
                {
                    Root = cardObj,
                    Icon = icImg,
                    AmountText = aText,
                    NameText = nText
                });
            }

            // 9. Bottom Action Buttons: Dedicate Ward (Left) & Consecrate Offering (Right)
            var dedBtnObj = new GameObject("DedicateBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            dedBtnObj.transform.SetParent(_dialogPanel.transform, false);
            var dbRect = dedBtnObj.GetComponent<RectTransform>();
            dbRect.anchoredPosition = new Vector2(-135f, -188f);
            dbRect.sizeDelta = new Vector2(240f, 40f);
            _dedicateBtn = dedBtnObj.GetComponent<Button>();
            GUIManager.Instance.ApplyButtonStyle(_dedicateBtn, 14);

            var dbTextObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            dbTextObj.transform.SetParent(dedBtnObj.transform, false);
            var dbtRect = dbTextObj.GetComponent<RectTransform>();
            dbtRect.sizeDelta = dbRect.sizeDelta;
            _dedicateBtnText = dbTextObj.GetComponent<Text>();
            _dedicateBtnText.font = font;
            _dedicateBtnText.fontSize = 14;
            _dedicateBtnText.alignment = TextAnchor.MiddleCenter;
            _dedicateBtnText.color = GUIManager.Instance.ValheimYellow;
            _dedicateBtn.onClick.AddListener(OnDedicateClicked);

            var consBtnObj = new GameObject("ConsecrateBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            consBtnObj.transform.SetParent(_dialogPanel.transform, false);
            var cbRect = consBtnObj.GetComponent<RectTransform>();
            cbRect.anchoredPosition = new Vector2(135f, -188f);
            cbRect.sizeDelta = new Vector2(240f, 40f);
            _consecrateBtn = consBtnObj.GetComponent<Button>();
            GUIManager.Instance.ApplyButtonStyle(_consecrateBtn, 14);

            var cbTextObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            cbTextObj.transform.SetParent(consBtnObj.transform, false);
            var cbtRect = cbTextObj.GetComponent<RectTransform>();
            cbtRect.sizeDelta = cbRect.sizeDelta;
            _consecrateBtnText = cbTextObj.GetComponent<Text>();
            _consecrateBtnText.font = font;
            _consecrateBtnText.fontSize = 14;
            _consecrateBtnText.alignment = TextAnchor.MiddleCenter;
            _consecrateBtnText.color = Color.white;
            _consecrateBtn.onClick.AddListener(OnConsecrateClicked);

            // 10. Close button ('X')
            var closeObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObj.transform.SetParent(_dialogPanel.transform, false);
            var cRect = closeObj.GetComponent<RectTransform>();
            cRect.anchoredPosition = new Vector2(345f, 245f);
            cRect.sizeDelta = new Vector2(35f, 35f);
            var cBtn = closeObj.GetComponent<Button>();
            GUIManager.Instance.ApplyButtonStyle(cBtn, 14);

            var cTextObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            cTextObj.transform.SetParent(closeObj.transform, false);
            var ctRect = cTextObj.GetComponent<RectTransform>();
            ctRect.sizeDelta = cRect.sizeDelta;
            var ct = cTextObj.GetComponent<Text>();
            ct.font = font;
            ct.fontSize = 16;
            ct.alignment = TextAnchor.MiddleCenter;
            ct.text = "X";
            ct.color = Color.white;
            cBtn.onClick.AddListener(Close);
        }

        private void Update()
        {
            if (!IsOpen()) return;

            // If player or ward is destroyed/dead, close immediately
            if (_player == null || _targetWard == null || _player.IsDead())
            {
                Close();
                return;
            }

            // Distance check: automatically close if player walks away from the ward (> 4.5m)
            float dist = Vector3.Distance(_player.transform.position, _targetWard.transform.position);
            if (dist > 4.5f)
            {
                Close();
                return;
            }

            // Debounce close input to avoid closing on the same frame as opening
            if (Time.time < _openTime + 0.25f) return;

            // Close on Escape, Tab, or Gamepad cancel
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || ZInput.GetKeyDown(KeyCode.Escape, true) || ZInput.GetButtonDown("JoyButtonB"))
            {
                Close();
                return;
            }
        }

        private void OnDedicateClicked()
        {
            if (_targetWard == null || _player == null) return;
            WardConduitManager.SetActiveGod(_targetWard, _selectedDeity);
            _player.Message(MessageHud.MessageType.Center, $"Ward Dedicated to {_selectedDeity}!");
            RefreshUI();
        }

        private void OnConsecrateClicked()
        {
            if (_targetWard == null || _player == null) return;
            var rites = WardRiteRegistry.GetForDeity(_selectedDeity);
            if (_selectedSlotIndex >= rites.Count) return;

            var rite = rites[_selectedSlotIndex];
            if (WardConduitManager.IsRiteUnlocked(_targetWard, rite.Id)) return;

            if (CanPlayerUnlockRite(_player, rite, out _))
            {
                if (WardConduitManager.UnlockRite(_targetWard, _player, rite))
                {
                    RefreshUI();
                }
            }
            else
            {
                _player.Message(MessageHud.MessageType.Center, "Missing required offerings in inventory!");
            }
        }

        private bool CanPlayerUnlockRite(Player player, WardRiteDefinition rite, out List<OfferingDisplayData> offeringList)
        {
            offeringList = new List<OfferingDisplayData>();
            var inv = player.GetInventory();
            var itemDB = ObjectDB.instance;
            bool hasAll = true;

            foreach (var cost in rite.Costs)
            {
                var prefab = itemDB != null ? itemDB.GetItemPrefab(cost.ItemPrefab) : null;
                ItemDrop? drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;

                string sharedName = drop != null ? drop.m_itemData.m_shared.m_name : cost.ItemPrefab;
                string localizedName = drop != null ? Localization.instance.Localize(sharedName) : cost.ItemPrefab;
                Sprite? icon = drop != null ? drop.m_itemData.GetIcon() : null;

                // Player material discovery check: m_knownMaterial stores m_shared.m_name
                bool isKnown = player.IsKnownMaterial(sharedName);
                int invCount = (inv != null && drop != null) ? inv.CountItems(sharedName) : 0;

                if (invCount < cost.Amount) hasAll = false;

                offeringList.Add(new OfferingDisplayData
                {
                    LocalizedName = localizedName,
                    Icon = icon,
                    IsKnown = isKnown,
                    RequiredAmount = cost.Amount,
                    CurrentAmount = invCount
                });
            }

            return hasAll;
        }

        private void Show()
        {
            gameObject.SetActive(true);
            _openTime = Time.time;
            GUIManager.BlockInput(true);
            RefreshUI();
        }

        private void Hide()
        {
            if (gameObject.activeSelf)
            {
                GUIManager.BlockInput(false);
                gameObject.SetActive(false);
            }
            _targetWard = null;
            _player = null;
        }

        private void OnDestroy()
        {
            if (IsOpen())
            {
                GUIManager.BlockInput(false);
            }
        }

        private void RefreshUI()
        {
            if (_targetWard == null || _player == null) return;

            var activeGod = WardConduitManager.GetActiveGod(_targetWard);
            int runesCount = WardConduitManager.GetUnlockedCountForDeity(_targetWard, _selectedDeity);
            float radius = ModConfig.GetWardRadius(runesCount);

            // Title and status
            _titleText!.text = $"Runic Ward of {_selectedDeity}";
            string activeTag = (activeGod == _selectedDeity) ? "<color=#55FF55>ACTIVE PATRON</color>" : "<color=#AAAAAA>INACTIVE</color>";
            _statusText!.text = $"{activeTag}  |  Consecrated Runes: {runesCount}/5  |  Zone Radius: {radius:F0}m";

            // Update Dedicate button
            if (activeGod == _selectedDeity)
            {
                _dedicateBtnText!.text = $"✓ Dedicated to {_selectedDeity}";
                _dedicateBtn!.interactable = false;
            }
            else
            {
                _dedicateBtnText!.text = $"Dedicate to {_selectedDeity}";
                _dedicateBtn!.interactable = true;
            }

            // Tier slots update
            string[] tierRoman = new[] { "I", "II", "III", "IV", "V" };
            var rites = WardRiteRegistry.GetForDeity(_selectedDeity);
            for (int i = 0; i < _slotButtons.Count; i++)
            {
                if (i >= rites.Count) continue;
                var r = rites[i];
                bool isUnlocked = WardConduitManager.IsRiteUnlocked(_targetWard, r.Id);
                var btn = _slotButtons[i];
                var btnImg = btn.GetComponent<Image>();
                var txt = _slotTexts[i];

                bool isSelected = (i == _selectedSlotIndex);

                if (isSelected)
                {
                    btnImg.color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    btnImg.color = isUnlocked ? new Color(0.85f, 0.85f, 0.85f, 0.95f) : new Color(0.6f, 0.6f, 0.6f, 0.8f);
                }

                if (isUnlocked)
                {
                    txt.text = $"Tier {tierRoman[i]}\n<size=11><color=#55FF55>✓ Consecrated</color></size>";
                    txt.color = isSelected ? Color.white : ModConfig.GetDeityColor(_selectedDeity);
                }
                else
                {
                    txt.text = $"Tier {tierRoman[i]}";
                    txt.color = isSelected ? GUIManager.Instance.ValheimYellow : new Color(0.6f, 0.6f, 0.6f);
                }
            }

            // Selected rite details
            if (_selectedSlotIndex >= 0 && _selectedSlotIndex < rites.Count)
            {
                var currentRite = rites[_selectedSlotIndex];
                bool isRiteUnlocked = WardConduitManager.IsRiteUnlocked(_targetWard, currentRite.Id);
                bool canUnlock = CanPlayerUnlockRite(_player, currentRite, out var offerings);

                _riteTitleText!.text = $"{currentRite.Name}  <color=#A0A0A0>(Tier {currentRite.Tier})</color>";
                _riteDescText!.text = currentRite.Description;

                // Center requirement cards
                int reqCount = offerings.Count;
                float cardSpacing = 140f;
                float totalWidth = reqCount * cardSpacing;
                float startX = (-totalWidth / 2f) + (cardSpacing / 2f);

                for (int i = 0; i < _reqCards.Count; i++)
                {
                    var card = _reqCards[i];
                    if (i < reqCount)
                    {
                        card.Root.SetActive(true);
                        var rect = card.Root.GetComponent<RectTransform>();
                        rect.anchoredPosition = new Vector2(startX + (i * cardSpacing), 0f);

                        var data = offerings[i];
                        if (data.Icon != null)
                        {
                            card.Icon.sprite = data.Icon;
                            card.Icon.enabled = true;

                            if (data.IsKnown)
                            {
                                card.Icon.color = Color.white;
                                card.NameText.text = data.LocalizedName;
                                card.NameText.color = GUIManager.Instance.ValheimBeige;
                            }
                            else
                            {
                                // Black silhouette for undiscovered materials
                                card.Icon.color = new Color(0.06f, 0.06f, 0.06f, 0.98f);
                                card.NameText.text = "???";
                                card.NameText.color = new Color(0.6f, 0.6f, 0.6f);
                            }
                        }
                        else
                        {
                            card.Icon.enabled = false;
                            card.NameText.text = data.IsKnown ? data.LocalizedName : "???";
                        }

                        string countColor = data.CurrentAmount >= data.RequiredAmount ? "#55FF55" : "#FF5555";
                        card.AmountText.text = $"<color={countColor}>{data.CurrentAmount}</color>/{data.RequiredAmount}";
                    }
                    else
                    {
                        card.Root.SetActive(false);
                    }
                }

                // Update Consecrate button
                if (isRiteUnlocked)
                {
                    _consecrateBtnText!.text = "✓ Consecrated";
                    _consecrateBtn!.interactable = false;
                }
                else if (canUnlock)
                {
                    _consecrateBtnText!.text = "<color=#55FF55>Consecrate Offering</color>";
                    _consecrateBtn!.interactable = true;
                }
                else
                {
                    _consecrateBtnText!.text = "Missing Offerings";
                    _consecrateBtn!.interactable = false;
                }
            }
        }
    }
}
