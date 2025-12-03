using System;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using MoreDurability.Settings;
using UnityEngine;

namespace MoreDurability.Patches
{
    /// <summary>
    /// 修复执行
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "Repair", new Type[] { typeof(Item), typeof(bool) })]
    public static class RepairExecutionPatch
    {  
        [HarmonyPrefix]
        public static void Prefix(ref Item __0, ref float __state)
        {
            if (__0 != null)
            {
                __state = __0.DurabilityLoss;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ref Item __0, ref float __state)
        {
            if (__0 == null) return;

            // 检查白名单
            if (!DurabilityConfig.IsWhitelisted(__0)) return;

            // UI 开关
            bool restoreEnabled = DurabilityConfig.RestoreMaxDurability && RepairToggleUI.IsRestoreModeEnabled;
            bool noLossEnabled = DurabilityConfig.NoMaxDurabilityLoss;

            // 修复恢复耐久上限
            if (restoreEnabled && __0.DurabilityLoss > 0f)
            {
                __0.DurabilityLoss = 0f;
                __0.Durability = __0.MaxDurability;
            }
            // 维修不掉耐久上限
            else if (noLossEnabled && Math.Abs(__0.DurabilityLoss - __state) > 0.001f)
            {
                __0.DurabilityLoss = __state;
                __0.Durability = __0.MaxDurability * (1f - __state);
            }
        }
    }

    /// <summary>
    /// 批量修复面板刷新补丁
    /// </summary>
    [HarmonyPatch(typeof(ItemRepair_RepairAllPanel), "OnEnable")]
    public static class RepairAllPanelRefreshPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ItemRepair_RepairAllPanel __instance)
        {
            Traverse.Create(__instance).Field("needsRefresh").SetValue(true);
        }
    }
}