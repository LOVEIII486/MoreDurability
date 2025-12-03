using System;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using MoreDurability.Settings;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

namespace MoreDurability.Patches
{
    /// <summary>
    /// 修复UI显示补丁
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "RefreshSelectedItemInfo")]
    public static class RepairUIDisplayPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref TextMeshProUGUI ___willLoseDurabilityText)
        {
            // 每次刷新界面时，检查一下开关是否应该显示
            RepairToggleUI.UpdateVisibility();

            Item selectedItem = ItemUIUtilities.SelectedItem;

            if (selectedItem == null || ___willLoseDurabilityText == null) return;
            
            if (!DurabilityConfig.IsWhitelisted(selectedItem)) return;
            
            bool restoreEnabled = DurabilityConfig.RestoreMaxDurability && RepairToggleUI.IsRestoreModeEnabled;
            bool noLossEnabled = DurabilityConfig.NoMaxDurabilityLoss;

            if (!restoreEnabled && !noLossEnabled) return;

            string baseLabel = "UI_MaxDurability".ToPlainText();

            if (restoreEnabled)
            {
                float originalMax = selectedItem.MaxDurability;
                float lossPct = selectedItem.DurabilityLoss;
                float currentDurability = selectedItem.Durability;
                float currentMax = originalMax * (1f - lossPct);

                float normalRepairVal = currentMax - currentDurability;
                if (normalRepairVal < 0.01f) normalRepairVal = 0f;

                float maxRestoreVal = originalMax - currentMax;
                float totalVal = normalRepairVal + maxRestoreVal;

                string totalStr = "+" + totalVal.ToString("0.#");
                string normalStr = "+" + normalRepairVal.ToString("0.#");
                string restoreStr = "+" + maxRestoreVal.ToString("0.#");

                string coloredText = $"{baseLabel} {totalStr} " +
                                     $"<size=80%>(<color=#AAAAAA>{normalStr}</color> " +
                                     $"<color=#00D0D0>{restoreStr}</color>)</size>";

                ___willLoseDurabilityText.text = coloredText;
            }
            else if (noLossEnabled)
            {
                ___willLoseDurabilityText.text = baseLabel + " <color=#AAAAAA>-0.0</color>";
            }
        }
    }

    /// <summary>
    /// 修复价格显示补丁
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "RefreshSelectedItemInfo")]
    public static class RepairPriceDisplayPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(ref TextMeshProUGUI ___repairPriceText)
        {
            Item selectedItem = ItemUIUtilities.SelectedItem;
            if (selectedItem == null || ___repairPriceText == null) return;
            
            if (!DurabilityConfig.IsWhitelisted(selectedItem)) return;
            
            bool restoreEnabled = DurabilityConfig.RestoreMaxDurability && RepairToggleUI.IsRestoreModeEnabled;

            if (!restoreEnabled || selectedItem.DurabilityLoss <= 0f) return;

            string totalPriceText = ___repairPriceText.text;
            if (!int.TryParse(totalPriceText, out int totalPrice)) return;

            float restoreMultiplier = DurabilityConfig.RestoreCostMultiplier;
            int restorePrice = Mathf.CeilToInt(selectedItem.Value * selectedItem.DurabilityLoss * restoreMultiplier * 0.5f);
            int basePrice = totalPrice - restorePrice;

            if (restorePrice <= 0) return;

            string coloredText = $"{totalPrice} " +
                                 $"<size=80%>(<color=#AAAAAA>{basePrice}</color> " +
                                 $"<color=#00D0D0>+{restorePrice}</color>)</size>";

            ___repairPriceText.text = coloredText;
        }
    }
}