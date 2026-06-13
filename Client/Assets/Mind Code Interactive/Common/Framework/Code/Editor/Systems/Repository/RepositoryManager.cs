/// <summary>
/// Project : Easy Build System
/// Class : RepositoryManager.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Attributes;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository
{
    public static class RepositoryManager
    {
        private static bool s_isRunning;
        private static readonly List<IntegrityResult> s_lastResults = new List<IntegrityResult>();

        private static string ValidationKey => $"MCI_REPOSITORY_INIT_{Application.dataPath}";

        public static bool IsRunning => s_isRunning;

        public static bool IsValidationInitialized(string key)
        {
            return EditorUserSettings.GetConfigValue(key) == "1";
        }

        public static void ResetValidationState()
        {
            EditorUserSettings.SetConfigValue(ValidationKey, null);
            s_lastResults.Clear();
        }

        public static List<IntegrityResult> GetLastResults() => s_lastResults;

        public static void InitializeProject(RepositoryManifest manifest, string validationKey, Action<float, string, int, int> onProgress, Action<string, string, bool> onComplete)
        {
            if (s_isRunning)
            {
                return;
            }

            s_isRunning = true;
            s_lastResults.Clear();

            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (manifest == null)
                    {
                        Debug.LogError("[RepositoryManager] Manifest not found");
                        onComplete?.Invoke("System", "Manifest not found", false);
                        s_isRunning = false;
                        return;
                    }

                    List<IIntegrityCheck> checks = DiscoverIntegrityChecks(manifest);
                    int totalSteps = checks.Count;
                    int currentStep = 0;

                    foreach (IIntegrityCheck check in checks)
                    {
                        try
                        {
                            currentStep++;
                            float progress = totalSteps > 0 ? (float)currentStep / totalSteps : 1f;
                            onProgress?.Invoke(progress, check.Id, currentStep, totalSteps);
                            check.RunFix();
                            bool isValid = check.RunCheck();
                            s_lastResults.Add(new IntegrityResult
                            {
                                Id = check.Id,
                                IsValid = isValid,
                                Reason = isValid ? string.Empty : check.FailReason
                            });
                        }
                        catch (Exception checkEx)
                        {
                            Debug.LogError($"[RepositoryManager] Error during {check.Id}: {checkEx.Message}");
                            s_lastResults.Add(new IntegrityResult
                            {
                                Id = check.Id,
                                IsValid = false,
                                Reason = "Exception: " + checkEx.Message
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(validationKey))
                    {
                        EditorUserSettings.SetConfigValue(validationKey, "1");
                    }

                    bool allValid = s_lastResults.All(r => r.IsValid);
                    onComplete?.Invoke("System", "Initialization complete", allValid);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[RepositoryManager] Initialization failed: {ex.Message}");
                    onComplete?.Invoke("System", "Initialization failed", false);
                }
                finally
                {
                    s_isRunning = false;
                }
            };
        }

        public static void RunChecksWithFixes(RepositoryManifest manifest, Action<float, string, int, int> onProgress, Action<string, string, bool> onComplete)
        {
            if (s_isRunning)
            {
                return;
            }

            s_isRunning = true;
            s_lastResults.Clear();

            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (manifest == null)
                    {
                        Debug.LogError("[RepositoryManager] Manifest not found");
                        onComplete?.Invoke("System", "Manifest not found", false);
                        s_isRunning = false;
                        return;
                    }

                    List<IIntegrityCheck> checks = DiscoverIntegrityChecks(manifest);
                    int totalSteps = checks.Count;
                    int currentStep = 0;

                    foreach (IIntegrityCheck check in checks)
                    {
                        currentStep++;
                        float progress = totalSteps > 0 ? (float)currentStep / totalSteps : 1f;

                        onProgress?.Invoke(progress, "Checking " + check.Id, currentStep, totalSteps);

                        bool checkResult = check.RunCheck();

                        if (!checkResult)
                        {
                            onProgress?.Invoke(progress, "Fixing " + check.Id, currentStep, totalSteps);
                            check.RunFix();

                            bool isValidAfterFix = check.RunCheck();
                            s_lastResults.Add(new IntegrityResult
                            {
                                Id = check.Id,
                                IsValid = isValidAfterFix,
                                Reason = isValidAfterFix ? string.Empty : check.FailReason
                            });
                        }
                        else
                        {
                            s_lastResults.Add(new IntegrityResult
                            {
                                Id = check.Id,
                                IsValid = true,
                                Reason = string.Empty
                            });
                        }
                    }

                    bool allValid = s_lastResults.All(r => r.IsValid);
                    onComplete?.Invoke("System", allValid ? "All checks passed" : "Some issues remain", allValid);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[RepositoryManager] Validate and fix failed: {ex.Message}");
                    onComplete?.Invoke("System", "Operation failed", false);
                }
                finally
                {
                    s_isRunning = false;
                }
            };
        }

        private static List<IIntegrityCheck> DiscoverIntegrityChecks(RepositoryManifest manifest)
        {
            List<IIntegrityCheck> checks = new List<IIntegrityCheck>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.IsAbstract || type.IsInterface || !typeof(IIntegrityCheck).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    object[] attributes = type.GetCustomAttributes(typeof(IntegrityCheckAttribute), false);
                    if (attributes.Length == 0)
                    {
                        continue;
                    }

                    IIntegrityCheck instance = (IIntegrityCheck)Activator.CreateInstance(type);
                    instance.Manifest = manifest;

                    if (instance.ShouldRun(manifest))
                    {
                        checks.Add(instance);
                    }
                }
            }

            return checks.OrderBy(c => c.Priority).ToList();
        }
    }

    public class IntegrityResult
    {
        public string Id;
        public bool IsValid;
        public string Reason;
    }
}