using System;
using HarmonyLib;
using Duckov.Modding;
using MoreDurability.Localization;
using MoreDurability.ModSettingsApi;
using MoreDurability.Settings;
using UnityEngine;

namespace MoreDurability
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public static ModBehaviour Instance { get; private set; }
        
        private const string HarmonyId = "com.LOVEIII486.MoreDurability"; 
        private const string LogTag = "[MoreDurability]";
        
        private Harmony _harmony;
        private bool _isPatched = false;

        private void OnEnable()
        {
            Instance = this;
            
            if (HarmonyLoad.LoadHarmony() == null)
            {
                Debug.LogError($"{LogTag} 模组启动失败: 缺少 Harmony 依赖。");
                return;
            }
            
            InitializeHarmonyPatches();
            
            Debug.Log($"{LogTag} 模组已启用");
        }
        
        protected override void OnAfterSetup()
        {
            base.OnAfterSetup();
            
            InitializeLocalization();
            
            if (ModSettingsApi.ModSettingAPI.Init(info))
            {
                DurabilityConfig.Load();
                SettingsUI.Register(); 
                Debug.Log("[MoreDurability] 配置系统初始化完成");
                MoreDurability.Patches.DurabilityMultiplierPatch.Initialize();
            }
            else
            {
                Debug.LogError("[MoreDurability] ModSetting 依赖缺失或初始化失败！");
            }
        }

        private void OnDisable()
        {
            CleanupLocalization();
            CleanupHarmonyPatches();

            Instance = null;
            Debug.Log($"{LogTag} 模组已禁用");
        }

        #region Harmony Management

        private void InitializeHarmonyPatches()
        {
            if (_isPatched) return;
            
            try
            {
                if (_harmony == null)
                {
                    _harmony = new Harmony(HarmonyId);
                }
                _harmony.PatchAll();
                _isPatched = true;
                Debug.Log($"{LogTag} Harmony 补丁应用成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogTag} Harmony 补丁应用失败: {ex}");
            }
        }

        private void CleanupHarmonyPatches()
        {
            if (!_isPatched || _harmony == null) return;

            try
            {
                _harmony.UnpatchAll(_harmony.Id);
                _isPatched = false;
                Debug.Log($"{LogTag} Harmony 补丁已移除");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LogTag} 移除 Harmony 补丁时发生错误: {ex}");
            }
        }

        #endregion
        #region Localization

        private void InitializeLocalization()
        {
            // 加载当前语言
            LocalizationManager.Initialize(info.path);
            
            // 监听游戏本身的语言切换事件 (SodaCraft)
            SodaCraft.Localizations.LocalizationManager.OnSetLanguage += OnLanguageChanged;
        }

        private void CleanupLocalization()
        {
            SodaCraft.Localizations.LocalizationManager.OnSetLanguage -= OnLanguageChanged;
            LocalizationManager.Cleanup();
        }

        private void OnLanguageChanged(SystemLanguage lang)
        {
            // 刷新字典
            LocalizationManager.Refresh();
            // 重新注册 UI 以应用新文本
            SettingsUI.Register();
        }

        #endregion
    }
}