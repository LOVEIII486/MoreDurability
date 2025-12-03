using System;
using System.Linq;
using Duckov.UI;
using HarmonyLib;
using MoreDurability.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoreDurability.Patches
{
    /// <summary>
    /// 在维修界面注入“是否恢复上限”的开关
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "Awake")]
    public static class RepairToggleUI
    {
        public static bool IsRestoreModeEnabled { get; set; } = true;
        
        private static GameObject _toggleGo;
        private static Toggle _toggleComponent;
        private static Image _badgeBgImage;
        private static TextMeshProUGUI _toggleLabel;
        
        private static readonly Color ColorActiveBg = new Color(0.2f, 0.7f, 0.3f, 0.9f); // 激活时的绿色
        private static readonly Color ColorInactiveBg = new Color(0.3f, 0.3f, 0.3f, 0.6f); // 未激活时的灰色
        private static readonly Color ColorTextWhite = Color.white;
        private static readonly Color ColorTextGray = new Color(0.8f, 0.8f, 0.8f);

        [HarmonyPostfix]
        public static void Postfix(ItemRepairView __instance)
        {
            if (_toggleGo != null) return;

            try
            {
                Transform operationTransform = __instance.transform.Find("Content/VertLayout/Operation");
                if (operationTransform == null)
                {
                    var btn = __instance.transform.GetComponentInChildren<Button>();
                    if (btn != null) operationTransform = btn.transform.parent;
                }

                if (operationTransform == null)
                {
                    Debug.LogError("[MoreDurability] 无法找到维修界面 UI 挂载点");
                    return;
                }

                Sprite checkmarkSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "Yes");
                Sprite backgroundSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "Rect16");

         
                _toggleGo = new GameObject("RestoreMaxToggle", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(Toggle));
                _toggleGo.transform.SetParent(operationTransform, false);
                _toggleGo.transform.SetAsFirstSibling();

                HorizontalLayoutGroup hg = _toggleGo.GetComponent<HorizontalLayoutGroup>();
                hg.childAlignment = TextAnchor.MiddleRight;
                hg.spacing = 8f; 
                hg.childControlWidth = false;
                hg.childControlHeight = false;

                ContentSizeFitter csf = _toggleGo.GetComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                _toggleComponent = _toggleGo.GetComponent<Toggle>();
                _toggleComponent.isOn = IsRestoreModeEnabled;

                GameObject badgeGo = new GameObject("BadgeContainer", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
                badgeGo.transform.SetParent(_toggleGo.transform, false);

                _badgeBgImage = badgeGo.GetComponent<Image>();
                _badgeBgImage.sprite = backgroundSprite;
                _badgeBgImage.type = Image.Type.Sliced;
                _badgeBgImage.pixelsPerUnitMultiplier = 2f;

                HorizontalLayoutGroup badgeLayout = badgeGo.GetComponent<HorizontalLayoutGroup>();
                badgeLayout.padding = new RectOffset(12, 12, 6, 6);
                badgeLayout.childControlWidth = true;
                badgeLayout.childControlHeight = true;

                GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGo.transform.SetParent(badgeGo.transform, false);
                
                _toggleLabel = labelGo.GetComponent<TextMeshProUGUI>();
                _toggleLabel.text = "恢复上限"; 
                _toggleLabel.fontSize = 22;     
                _toggleLabel.fontStyle = FontStyles.Bold;
                _toggleLabel.alignment = TextAlignmentOptions.Center;
                
                GameObject checkBorderGo = new GameObject("CheckBorder", typeof(RectTransform), typeof(Image));
                checkBorderGo.transform.SetParent(_toggleGo.transform, false);
                
                Image borderImage = checkBorderGo.GetComponent<Image>();
                borderImage.sprite = backgroundSprite;
                borderImage.color = new Color(0.5f, 0.5f, 0.5f);
                
                RectTransform borderRect = checkBorderGo.GetComponent<RectTransform>();
                borderRect.sizeDelta = new Vector2(34, 34);
                
                GameObject checkBgGo = new GameObject("CheckBackground", typeof(RectTransform), typeof(Image));
                checkBgGo.transform.SetParent(checkBorderGo.transform, false);
                
                Image bgImage = checkBgGo.GetComponent<Image>();
                bgImage.sprite = backgroundSprite;
                bgImage.color = new Color(0.1f, 0.1f, 0.1f);
                
                RectTransform bgRect = checkBgGo.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(30, 30);
                
                GameObject checkMarkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
                checkMarkGo.transform.SetParent(checkBgGo.transform, false);
                
                Image checkImage = checkMarkGo.GetComponent<Image>();
                checkImage.sprite = checkmarkSprite;
                checkImage.color = new Color(0f, 0.9f, 0.9f);
                
                RectTransform checkRect = checkMarkGo.GetComponent<RectTransform>();
                checkRect.sizeDelta = new Vector2(24, 24);
                
                _toggleComponent.targetGraphic = bgImage;
                _toggleComponent.graphic = checkImage;
                
                _toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);

                UpdateVisualState(IsRestoreModeEnabled);
                UpdateVisibility();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoreDurability] UI 注入失败: {ex.Message}");
            }
        }

        private static void OnToggleValueChanged(bool isOn)
        {
            IsRestoreModeEnabled = isOn;
            UpdateVisualState(isOn);
            
            if (ItemRepairView.Instance != null)
            {
                Traverse.Create(ItemRepairView.Instance).Method("RefreshSelectedItemInfo").GetValue();
            }
        }

        private static void UpdateVisualState(bool isOn)
        {
            if (_badgeBgImage != null && _toggleLabel != null)
            {
                if (isOn)
                {
                    _badgeBgImage.color = ColorActiveBg;
                    _toggleLabel.color = ColorTextWhite;
                }
                else
                {
                    _badgeBgImage.color = ColorInactiveBg;
                    _toggleLabel.color = ColorTextGray;
                }
            }
        }

        public static void UpdateVisibility()
        {
            if (_toggleGo != null)
            {
                bool globalEnabled = DurabilityConfig.RestoreMaxDurability;
                _toggleGo.SetActive(globalEnabled);
            }
        }
    }
}