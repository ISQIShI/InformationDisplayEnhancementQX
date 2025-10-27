using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    public class HarmonyHelper
    {
        public Harmony HarmonyInstance { get; }

        private IEnumerable<Type> _harmonyPatchClasses;

        public IEnumerable<Type> HarmonyPatchClasses
        {
            get
            {
                if (_harmonyPatchClasses == null)
                {

                }
                return _harmonyPatchClasses;
            }
        }

        public HarmonyHelper(string harmonyId)
        {
            HarmonyInstance = new Harmony(harmonyId);
        }

        public static void CheckHarmonyVersion()
        {
            try
            {
                var versionDic = Harmony.VersionInfo(out var version);
                Debug.LogWarning($"Harmony 版本: {version}");

                var harmonyAssembly = typeof(Harmony).Assembly;
                var assemblyVersion = harmonyAssembly.GetName().Version;
                Debug.LogWarning($"Harmony 程序集版本: {assemblyVersion}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"检查 Harmony 版本失败: {ex}");
            }
        }

        public static IEnumerable<Type> GetHarmonyPatchClasses(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), true).Length > 0);
        }

        public static void CheckHarmonyPatchClasses(Assembly assembly)
        {
            var types = GetHarmonyPatchClasses(assembly);
            Debug.LogWarning($"程序集信息：{assembly}");
            Debug.LogWarning($"检测到的 Harmony 补丁类数量: {types.Count()}");
            foreach (var type in types)
            {
                Debug.LogWarning($"Harmony 补丁类: {type.FullName}");
            }
        }

        public void PatchAllUngrouped(Assembly assembly)
        {
            var types = GetHarmonyPatchClasses(assembly).Where(type =>
            {
                var categoryAttr = type.GetCustomAttribute<PatchGroup>();
                return categoryAttr == null;
            });
            foreach (var type in types)
            {
                try
                {
                    HarmonyInstance.CreateClassProcessor(type).Patch();
                    Console.ResetColor(); // 重置颜色
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"已加载补丁: {type.Name} (无组别)");
                    Console.ResetColor(); // 重置颜色
                }
                catch (Exception ex)
                {
                    Debug.LogError($"加载补丁失败: {type.Name}, 错误: {ex.Message}");
                }
            }
        }

        public void PatchGroup(Assembly assembly, string group)
        {
            var types = GetHarmonyPatchClasses(assembly).Where(type =>
            {
                var categoryAttr = type.GetCustomAttribute<PatchGroup>();
                return categoryAttr != null && categoryAttr.GroupName == group;
            });

            foreach (var type in types)
            {
                try
                {
                    HarmonyInstance.CreateClassProcessor(type).Patch();
                    Debug.Log($"已加载补丁: {type.Name} (组别: {group})");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"加载补丁失败: {type.Name}, 错误: {ex.Message}");
                }
            }
        }
    }
}
