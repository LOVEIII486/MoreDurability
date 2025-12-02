using System;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;

namespace MoreDurability.Patches
{
    /// <summary>
    /// 修复执行
    /// </summary>
    [HarmonyPatch(typeof(ItemRepairView), "Repair", new Type[] { typeof(Item), typeof(bool) })]
    public static class RepairExecutionPatch
    {
        private const string LogTag = "[MoreDurability.Repair]";

        /// <summary>
        /// 前置，保存维修前的耐久损失值
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(ref Item __0, ref float __state)
        {
            if (__0 != null)
            {
                __state = __0.DurabilityLoss;
            }
        }

        /// <summary>
        /// 后置，修改耐久度
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(ref Item __0, ref float __state)
        {
            if (__0 == null) return;

            bool restoreEnabled = Settings.DurabilityConfig.RestoreMaxDurability;
            bool noLossEnabled = Settings.DurabilityConfig.NoMaxDurabilityLoss;

            // 修复恢复耐久上限
            if (restoreEnabled && __0.DurabilityLoss > 0f)
            {
                __0.DurabilityLoss = 0f;
                __0.Durability = __0.MaxDurability;
                //Debug.Log($"{LogTag} {__0.DisplayName} 已恢复满耐久上限");
            }
            // 维修不掉耐久上限
            else if (noLossEnabled && Math.Abs(__0.DurabilityLoss - __state) > 0.001f)
            {
                __0.DurabilityLoss = __state;
                __0.Durability = __0.MaxDurability * (1f - __state);
                //Debug.Log($"{LogTag} {__0.DisplayName} 已保持耐久上限不变");
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