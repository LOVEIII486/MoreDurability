using System.Collections.Generic;
using MoreDurability.ModSettingsApi;
using MoreDurability.Localization;
using UnityEngine;

namespace MoreDurability.Settings
{
    public static class SettingsUI
    {
        public static void Register()
        {
            if (!ModSettingAPI.IsInit) return;

            ModSettingAPI.Clear();

            // ==================== 1. 物品耐久度倍率 ====================
            ModSettingAPI.AddSlider(
                DurabilityConfig.Key_DurabilityMultiplier,
                LocalizationManager.GetText("Setting_DurabilityMultiplier"),
                DurabilityConfig.Multiplier,
                new Vector2(1.0f, 10.0f),
                (value) =>
                {
                    DurabilityConfig.Multiplier = value;
                },
                1, 5
            );

            // ==================== 2. 维修不掉耐久度上限 ====================
            ModSettingAPI.AddToggle(
                DurabilityConfig.Key_NoMaxDurabilityLoss,
                LocalizationManager.GetText("Setting_NoMaxDurabilityLoss"),
                DurabilityConfig.NoMaxDurabilityLoss,
                (value) =>
                {
                    DurabilityConfig.NoMaxDurabilityLoss = value;
                }
            );

            // ==================== 3. 维修恢复满耐久度上限 ====================
            ModSettingAPI.AddToggle(
                DurabilityConfig.Key_RestoreMaxDurability,
                LocalizationManager.GetText("Setting_RestoreMaxDurability"),
                DurabilityConfig.RestoreMaxDurability,
                (value) =>
                {
                    DurabilityConfig.RestoreMaxDurability = value;
                }
            );

            // ==================== 4. 恢复上限价格倍率 ====================
            ModSettingAPI.AddSlider(
                DurabilityConfig.Key_RestoreCostMultiplier,
                LocalizationManager.GetText("Setting_RestoreCostMultiplier"),
                DurabilityConfig.RestoreCostMultiplier,
                new Vector2(0.1f, 10.0f),
                (value) =>
                {
                    DurabilityConfig.RestoreCostMultiplier = value;
                },
                2, 5
            );

            // ==================== 5. 标签白名单 ====================
            ModSettingAPI.AddInput(
                DurabilityConfig.Key_WhitelistedTags,
                LocalizationManager.GetText("Setting_WhitelistedTags"),
                DurabilityConfig.WhitelistedTags,
                100,
                (value) =>
                {
                    DurabilityConfig.WhitelistedTags = value;
                }
            );

            ModSettingAPI.AddGroup(
                "MoreDurability_MainGroup",
                LocalizationManager.GetText("Settings_Group_Title"),
                new List<string> 
                { 
                    DurabilityConfig.Key_DurabilityMultiplier, 
                    DurabilityConfig.Key_NoMaxDurabilityLoss, 
                    DurabilityConfig.Key_RestoreMaxDurability,
                    DurabilityConfig.Key_RestoreCostMultiplier,
                    DurabilityConfig.Key_WhitelistedTags
                },
                0.8f, true, true
            );
        }
    }
}