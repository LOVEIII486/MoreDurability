using System;
using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem; // 引用游戏物品命名空间
using MoreDurability.ModSettingsApi;
using UnityEngine;

namespace MoreDurability.Settings
{
    public static class DurabilityConfig
    {
        // ==================== 配置项 Key ====================
        public const string Key_DurabilityMultiplier = "DurabilityMultiplier";
        public const string Key_NoMaxDurabilityLoss = "NoMaxDurabilityLoss";
        public const string Key_RestoreMaxDurability = "RestoreMaxDurability";
        public const string Key_RestoreCostMultiplier = "RestoreCostMultiplier";
        public const string Key_WhitelistedTags = "WhitelistedTags";

        // ==================== 默认值 ====================
        private const float Default_Multiplier = 1.0f;
        private const bool Default_NoLoss = false;
        private const bool Default_RestoreMax = false;
        private const float Default_RestoreCost = 2.0f;
        private const string Default_WhitelistedTags = "Weapon,Armor"; 

        // ==================== 配置变更事件 ====================
        public static event Action OnConfigChanged;

        // ==================== 配置属性 ====================
        private static float _multiplier = Default_Multiplier;
        private static bool _noMaxDurabilityLoss = Default_NoLoss;
        private static bool _restoreMaxDurability = Default_RestoreMax;
        private static float _restoreCostMultiplier = Default_RestoreCost;
        private static string _whitelistedTags = Default_WhitelistedTags;
        
        // 缓存解析后的标签集合
        private static HashSet<string> _tagSet = new HashSet<string>();

        /// <summary>
        /// 物品耐久度倍率
        /// </summary>
        public static float Multiplier 
        { 
            get => _multiplier;
            set
            {
                if (Math.Abs(_multiplier - value) > 0.001f)
                {
                    _multiplier = value;
                    OnConfigChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 维修时不掉耐久度上限
        /// </summary>
        public static bool NoMaxDurabilityLoss 
        { 
            get => _noMaxDurabilityLoss;
            set
            {
                if (_noMaxDurabilityLoss != value)
                {
                    _noMaxDurabilityLoss = value;
                    OnConfigChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 维修时恢复满耐久度上限
        /// </summary>
        public static bool RestoreMaxDurability 
        { 
            get => _restoreMaxDurability;
            set
            {
                if (_restoreMaxDurability != value)
                {
                    _restoreMaxDurability = value;
                    OnConfigChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 恢复耐久上限的价格倍率
        /// </summary>
        public static float RestoreCostMultiplier 
        { 
            get => _restoreCostMultiplier;
            set
            {
                if (Math.Abs(_restoreCostMultiplier - value) > 0.001f)
                {
                    _restoreCostMultiplier = value;
                    OnConfigChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 标签白名单字符串
        /// </summary>
        public static string WhitelistedTags
        {
            get => _whitelistedTags;
            set
            {
                if (_whitelistedTags != value)
                {
                    _whitelistedTags = value;
                    ParseTags();
                    OnConfigChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 解析逗号分隔的标签字符串到 HashSet
        /// </summary>
        private static void ParseTags()
        {
            _tagSet.Clear();
            if (string.IsNullOrWhiteSpace(_whitelistedTags)) return;

            var tags = _whitelistedTags.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                _tagSet.Add(tag.Trim());
            }
            // Debug.Log($"[MoreDurability] 已更新标签白名单: {string.Join(", ", _tagSet)}");
        }

        /// <summary>
        /// 检查物品是否在允许的标签白名单内
        /// </summary>
        public static bool IsWhitelisted(Item item)
        {
            if (item == null) return false;
            
            if (_tagSet == null || _tagSet.Count == 0) return true;

            if (item.Tags == null) return false;

            foreach (var tag in _tagSet)
            {
                if (item.Tags.Contains(tag))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从 ModSetting 读取已保存的配置
        /// </summary>
        public static void Load()
        {
            if (ModSettingAPI.GetSavedValue(Key_DurabilityMultiplier, out float savedMulti))
            {
                if (savedMulti >= 1.0f) _multiplier = savedMulti;
            }

            if (ModSettingAPI.GetSavedValue(Key_NoMaxDurabilityLoss, out bool savedNoLoss))
            {
                _noMaxDurabilityLoss = savedNoLoss;
            }

            if (ModSettingAPI.GetSavedValue(Key_RestoreMaxDurability, out bool savedRestore))
            {
                _restoreMaxDurability = savedRestore;
            }

            if (ModSettingAPI.GetSavedValue(Key_RestoreCostMultiplier, out float savedRestoreCost))
            {
                if (savedRestoreCost >= 0.5f && savedRestoreCost <= 3.0f)
                {
                    _restoreCostMultiplier = savedRestoreCost;
                }
            }

            if (ModSettingAPI.GetSavedValue(Key_WhitelistedTags, out string savedTags))
            {
                _whitelistedTags = savedTags;
            }
            
            ParseTags();
            
            Debug.Log($"[MoreDurability] 配置已加载: 倍率={_multiplier:F1}, 不掉上限={_noMaxDurabilityLoss}, " +
                      $"恢复上限={_restoreMaxDurability}, 恢复价格倍率={_restoreCostMultiplier:F1}, " +
                      $"白名单=[{string.Join(", ", _tagSet)}]");
            
            OnConfigChanged?.Invoke();
        }
    }
}