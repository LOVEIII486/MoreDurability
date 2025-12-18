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
                float currentDurability = selectedItem.Durability;
                float currentMax = selectedItem.MaxDurabilityWithLoss;

                // 1. 基础维修量
                float normalRepairVal = currentMax - currentDurability;
                if (normalRepairVal < 0f) normalRepairVal = 0f;

                // 2. 本次维修本应产生的损耗
                float potentialLoss = normalRepairVal * DurabilityConfig.VanillaRepairLossRate;

                // 3. 已有的红色损耗
                float existingLoss = originalMax - currentMax;

                // 4. 青色部分：显示模组共挽回的上限总量
                float totalSavedMax = existingLoss + potentialLoss;

                // 5. 总增加显示：修复后的最终耐久 - 修复前的当前耐久
                float totalDisplayVal = originalMax - currentDurability;

                string totalStr = "+" + totalDisplayVal.ToString("0.#");
                string normalStr = "+" + normalRepairVal.ToString("0.#");
                string savedStr = "+" + totalSavedMax.ToString("0.#");
                
                ___willLoseDurabilityText.text = $"{baseLabel} {totalStr} " +
                                                 $"<size=80%>(<color=#AAAAAA>{normalStr}</color> " +
                                                 $"<color=#00D0D0>{savedStr}</color>)</size>";
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
            if (!restoreEnabled) return;

            // 计算需要“保费”的总占比
            float repairAmount = selectedItem.MaxDurabilityWithLoss - selectedItem.Durability;
            float potentialLossPercent =
                (repairAmount * DurabilityConfig.VanillaRepairLossRate) / selectedItem.MaxDurability;
            float totalRestorePercent = selectedItem.DurabilityLoss + potentialLossPercent;

            if (totalRestorePercent <= 0.001f) return;

            string totalPriceText = ___repairPriceText.text;
            if (!int.TryParse(totalPriceText, out int totalPrice)) return;

            float restoreMultiplier = DurabilityConfig.RestoreCostMultiplier;
            // 使用总占比计算额外费用
            int restorePrice = Mathf.CeilToInt(selectedItem.Value * totalRestorePercent * restoreMultiplier * 0.5f);
            int basePrice = totalPrice - restorePrice;

            if (restorePrice <= 0) return;

            ___repairPriceText.text =
                $"{totalPrice} <size=80%>(<color=#AAAAAA>{basePrice}</color> <color=#00D0D0>+{restorePrice}</color>)</size>";
        }
    }
}