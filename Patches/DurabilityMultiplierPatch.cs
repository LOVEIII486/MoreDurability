using System;
using System.Collections.Generic;
using HarmonyLib;
using ItemStatsSystem;
using MoreDurability.Settings;
using UnityEngine;
using Duckov.Utilities;

namespace MoreDurability.Patches
{
    /// <summary>
    /// 全局耐久度倍率修改器
    /// </summary>
    [HarmonyPatch]
    public static class DurabilityMultiplierPatch
    {
        private const string BackupKey = "MoreDurability_BaseMax";
        private const string BackupDefaultDurabilityKey = "MoreDurability_BaseDefaultDurability";
        private const string LogTag = "[MoreDurability.MultiplierPatch]";

        /// <summary>
        /// 初始化方法
        /// </summary>
        public static void Initialize()
        {
            DurabilityConfig.OnConfigChanged += ApplyToAllItems;
            ApplyToAllItems();
        }

        /// <summary>
        /// 拦截动态添加的物品
        /// </summary>
        [HarmonyPatch(typeof(ItemAssetsCollection), "AddDynamicEntry")]
        [HarmonyPostfix]
        public static void OnAddDynamicEntry(Item prefab)
        {
            if (prefab != null)
            {
                ApplyBuff(prefab, DurabilityConfig.Multiplier);
            }
        }

        /// <summary>
        /// 遍历游戏内所有物品并应用倍率
        /// </summary>
        public static void ApplyToAllItems()
        {
            if (ItemAssetsCollection.Instance == null) return;

            float multiplier = DurabilityConfig.Multiplier;
            int count = 0;

            if (ItemAssetsCollection.Instance.entries != null)
            {
                foreach (var entry in ItemAssetsCollection.Instance.entries)
                {
                    if (entry != null && entry.prefab != null)
                    {
                        if (ApplyBuff(entry.prefab, multiplier)) count++;
                    }
                }
            }
            
            var dynamicDic = AccessTools.Field(typeof(ItemAssetsCollection), "dynamicDic")
                .GetValue(null) as Dictionary<int, ItemAssetsCollection.DynamicEntry>;
            if (dynamicDic != null)
            {
                foreach (var entry in dynamicDic.Values)
                {
                    if (entry != null && entry.prefab != null)
                    {
                        if (ApplyBuff(entry.prefab, multiplier)) count++;
                    }
                }
            }

            // Debug.Log($"{LogTag} 已更新 {count} 个物品的耐久度上限，当前倍率: {multiplier}x");
        }

        /// <summary>
        /// 对单个物品应用耐久修改
        /// </summary>
        private static bool ApplyBuff(Item item, float multiplier)
        {
            try
            {
                // 过滤掉没有耐久度的物品
                if (item.Constants == null || !item.UseDurability) return false;

                // 检查白名单
                if (!DurabilityConfig.IsWhitelisted(item)) return false;

                // 1. 处理最大耐久度
                float originalMax;
                if (item.Constants.GetEntry(BackupKey) != null)
                {
                    originalMax = item.Constants.GetFloat(BackupKey);
                }
                else
                {
                    originalMax = item.Constants.GetFloat("MaxDurability");
                    item.Constants.SetFloat(BackupKey, originalMax, true);
                }
                if (originalMax <= 1f) return false;

                float newMax = originalMax * multiplier;
                item.Constants.SetFloat("MaxDurability", newMax, true);
                
                // 2. 处理默认当前耐久度
                float originalDefaultDurability;
                
                // 备份原始默认值，否则多次调整倍率会出错
                if (item.Variables.GetEntry(BackupDefaultDurabilityKey) != null)
                {
                    originalDefaultDurability = item.Variables.GetFloat(BackupDefaultDurabilityKey);
                }
                else
                {
                    originalDefaultDurability = item.Variables.GetFloat("Durability");
                    item.Variables.SetFloat(BackupDefaultDurabilityKey, originalDefaultDurability, true);
                }

                // 按倍率等比提升默认耐久
                // 情况A（满耐久）：原默认100，倍率5x -> 新默认500。生成物品 500/500。
                // 情况B（预设残损）：原默认50，倍率5x -> 新默认250。生成物品 250/500 (保持50%)。
                float newDefaultDurability = originalDefaultDurability * multiplier;
                
                item.Variables.SetFloat("Durability", newDefaultDurability, true);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{LogTag} 处理物品 {item.name} 时出错: {ex.Message}");
                return false;
            }
        }
    }
}