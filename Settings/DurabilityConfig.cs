using System;
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

        // ==================== 默认值 ====================
        private const float Default_Multiplier = 1.0f;
        private const bool Default_NoLoss = false;
        private const bool Default_RestoreMax = false;
        private const float Default_RestoreCost = 2.0f;

        // ==================== 配置变更事件 ====================
        public static event Action OnConfigChanged;

        // ==================== 配置属性 ====================
        private static float _multiplier = Default_Multiplier;
        private static bool _noMaxDurabilityLoss = Default_NoLoss;
        private static bool _restoreMaxDurability = Default_RestoreMax;
        private static float _restoreCostMultiplier = Default_RestoreCost;
        
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
        /// 优先级高于不掉耐久上限
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
            
            Debug.Log($"[MoreDurability] 配置已加载: 倍率={_multiplier:F1}, 不掉上限={_noMaxDurabilityLoss}, " +
                      $"恢复上限={_restoreMaxDurability}, 恢复价格倍率={_restoreCostMultiplier:F1}");
            
            OnConfigChanged?.Invoke();
        }
    }
}