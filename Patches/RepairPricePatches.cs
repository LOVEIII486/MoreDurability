using System;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using MoreDurability.Settings;
using UnityEngine;

namespace MoreDurability.Patches
{
    /// <summary>
    /// 修复价格计算补丁
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "CalculateRepairPrice")]
    [HarmonyPatch(new Type[] { typeof(Item), typeof(float), typeof(float), typeof(float) }, 
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out })]
    public static class RepairPriceCalculationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Item item, ref int __result)
        {
            try
            {
                bool restoreEnabled = Settings.DurabilityConfig.RestoreMaxDurability && RepairToggleUI.IsRestoreModeEnabled;
                if (item == null || !restoreEnabled) return;

                // 1. 获取基础配置
                float restoreMultiplier = Settings.DurabilityConfig.RestoreCostMultiplier;
                float lossRate = Settings.DurabilityConfig.VanillaRepairLossRate;

                // 2. 计算已有的上限损耗
                float currentLossPercent = item.DurabilityLoss;

                // 3. 计算潜在损耗
                float repairAmount = item.MaxDurabilityWithLoss - item.Durability;
                float potentialLossAmount = repairAmount * lossRate;
                float potentialLossPercent = potentialLossAmount / item.MaxDurability;

                // 4. 总恢复占比 = 现有红色损耗 + 潜在新生损耗
                float totalRestorePercent = currentLossPercent + potentialLossPercent;

                if (totalRestorePercent > 0.001f)
                {
                    // 计算额外费用并累加到原价中
                    // 价值 * 总恢复比例 * 价格倍率 * 调节系数(0.5)
                    int extraPrice = Mathf.CeilToInt(item.Value * totalRestorePercent * restoreMultiplier * 0.5f);
                    __result += extraPrice;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoreDurability] 价格计算算法错误: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 修复可用性检查补丁
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "CanRepair", MethodType.Getter)]
    public static class RepairAvailabilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            try
            {
                Item selectedItem = ItemUIUtilities.SelectedItem;

                if (selectedItem == null || !selectedItem.UseDurability || 
                    selectedItem.MaxDurabilityWithLoss < 1f || 
                    !selectedItem.Tags.Contains("Repairable"))
                {
                    __result = false;
                    return false;
                }
                
                if (!DurabilityConfig.IsWhitelisted(selectedItem))
                {
                    return true; 
                }
                
                bool restoreEnabled = DurabilityConfig.RestoreMaxDurability && RepairToggleUI.IsRestoreModeEnabled;

                // 如果启用了恢复上限功能，且有耐久损失，允许维修
                if (restoreEnabled && selectedItem.DurabilityLoss > 0f)
                {
                    __result = true;
                    return false;
                }
         
                __result = selectedItem.Durability < selectedItem.MaxDurabilityWithLoss;
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoreDurability.CanRepair] 错误: {ex.Message}");
                return true;
            }
        }
    }
}