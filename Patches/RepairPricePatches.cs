using System;
using System.Linq;
using System.Reflection;
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
        private const string LogTag = "[MoreDurability.RepairPrice]";
        
        [HarmonyPostfix]
        public static void Postfix(Item item, ref int __result)
        {
            try
            {
                bool restoreEnabled = Settings.DurabilityConfig.RestoreMaxDurability;
                
                if (!DurabilityConfig.IsWhitelisted(item)) return;
                
                if (!restoreEnabled || item == null || item.DurabilityLoss <= 0f)
                {
                    return;
                }

                float restoreMultiplier = Settings.DurabilityConfig.RestoreCostMultiplier;

                int restorePrice = Mathf.CeilToInt(item.Value * item.DurabilityLoss * restoreMultiplier * 0.5f);
                
                __result += restorePrice;

                // Debug.Log($"{LogTag} {item.DisplayName} - 增加恢复费: {restorePrice}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogTag} 价格计算错误: {ex.Message}");
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
                    // 让原版逻辑运行
                    return true; 
                }

                // 如果启用了恢复上限功能，且有耐久损失，允许维修
                if (Settings.DurabilityConfig.RestoreMaxDurability && selectedItem.DurabilityLoss > 0f)
                {
                    __result = true;
                    return false;
                }
         
                // 当前耐久 < 最大可用耐久时可维修
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