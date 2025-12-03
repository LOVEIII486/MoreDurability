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
                // 必须同时满足：全局开启 && UI开关开启
                bool restoreEnabled = DurabilityConfig.RestoreMaxDurability && RepairToggleUI.IsRestoreModeEnabled;
                
                if (!DurabilityConfig.IsWhitelisted(item)) return;
                
                if (!restoreEnabled || item == null || item.DurabilityLoss <= 0f)
                {
                    return;
                }

                float restoreMultiplier = Settings.DurabilityConfig.RestoreCostMultiplier;
                int restorePrice = Mathf.CeilToInt(item.Value * item.DurabilityLoss * restoreMultiplier * 0.5f);
                
                __result += restorePrice;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoreDurability.RepairPrice] 价格计算错误: {ex.Message}");
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