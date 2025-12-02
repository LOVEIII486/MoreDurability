using System;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
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
            Item selectedItem = ItemUIUtilities.SelectedItem;

            if (selectedItem == null || ___willLoseDurabilityText == null)
            {
                return;
            }

            bool restoreEnabled = Settings.DurabilityConfig.RestoreMaxDurability;
            bool noLossEnabled = Settings.DurabilityConfig.NoMaxDurabilityLoss;

            if (!restoreEnabled && !noLossEnabled)
            {
                return;
            }

            string baseLabel = "UI_MaxDurability".ToPlainText();

            if (restoreEnabled)
            {
                float originalMax = selectedItem.MaxDurability;
                float lossPct = selectedItem.DurabilityLoss; // 损耗百分比 (0.068)
                float currentDurability = selectedItem.Durability; // 当前耐久 (93.2)

                // 当前受损后的实际限制
                // 100 * (1 - 0.068) = 93.2
                float currentMax = originalMax * (1f - lossPct);

                // 常规维修：从当前耐久修到当前上限
                float normalRepairVal = currentMax - currentDurability;
                // 防止浮点数误差出现微小的负数
                if (normalRepairVal < 0.01f) normalRepairVal = 0f;

                // 上限恢复：从当前上限恢复到原始上限
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
    /// 修复价格显示补丁 ，基础维修 + 恢复上限   
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "RefreshSelectedItemInfo")]
    public static class RepairPriceDisplayPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void Postfix(ref TextMeshProUGUI ___repairPriceText)
        {
            Item selectedItem = ItemUIUtilities.SelectedItem;

            if (selectedItem == null || ___repairPriceText == null)
            {
                return;
            }

            bool restoreEnabled = Settings.DurabilityConfig.RestoreMaxDurability;

            if (!restoreEnabled || selectedItem.DurabilityLoss <= 0f)
            {
                return;
            }

            string totalPriceText = ___repairPriceText.text;
            if (!int.TryParse(totalPriceText, out int totalPrice))
            {
                return;
            }

            float restoreMultiplier = Settings.DurabilityConfig.RestoreCostMultiplier;
            int restorePrice =
                Mathf.CeilToInt(selectedItem.Value * selectedItem.DurabilityLoss * restoreMultiplier * 0.5f);
            int basePrice = totalPrice - restorePrice;

            if (restorePrice <= 0)
            {
                return;
            }

            string coloredText = $"{totalPrice} " +
                                 $"<size=80%>(<color=#AAAAAA>{basePrice}</color> " +
                                 $"<color=#00D0D0>+{restorePrice}</color>)</size>";

            ___repairPriceText.text = coloredText;
        }
    }
}