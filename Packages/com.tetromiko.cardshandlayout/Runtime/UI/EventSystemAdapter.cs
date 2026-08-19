using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tetromiko.CardsHandLayout
{
    /// <summary>
    /// Decoupled, adaptive EventSystem helper that configures the appropriate
    /// UI Input Module dynamically (New Input System vs. Legacy Input System).
    /// </summary>
    public static class EventSystemAdapter
    {
        public static EventSystem EnsureAdaptiveEventSystem()
        {
            EventSystem es = EventSystem.current;
            if (es == null)
            {
#if UNITY_2023_1_OR_NEWER
                es = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
#else
                es = UnityEngine.Object.FindObjectOfType<EventSystem>();
#endif
            }

            if (es == null)
            {
                var esObj = new GameObject("EventSystem", typeof(EventSystem));
                es = esObj.GetComponent<EventSystem>();
                AttachBestInputModule(esObj);
            }
            else
            {
                // If an EventSystem exists, verify it has a working input module
                ValidateAndFixInputModule(es.gameObject);
            }

            return es;
        }

        public static void AttachBestInputModule(GameObject targetObj)
        {
            // 1. Try New Input System UI Input Module (Unity.InputSystem)
            Type newModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem")
                ?? Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");

            if (newModuleType != null)
            {
                // Remove legacy module if present to prevent InvalidOperationException
                var legacyModule = targetObj.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(legacyModule);
                    else
                        UnityEngine.Object.DestroyImmediate(legacyModule);
                }

                if (targetObj.GetComponent(newModuleType) == null)
                {
                    targetObj.AddComponent(newModuleType);
                }
                return;
            }

            // 2. Fallback to Legacy StandaloneInputModule
            if (targetObj.GetComponent<BaseInputModule>() == null)
            {
                targetObj.AddComponent<StandaloneInputModule>();
            }
        }

        public static void ValidateAndFixInputModule(GameObject targetObj)
        {
            Type newModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem")
                ?? Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");

            var legacyModule = targetObj.GetComponent<StandaloneInputModule>();
            var newModule = newModuleType != null ? targetObj.GetComponent(newModuleType) : null;

            // If New Input System is active in the project and legacy module is attached, upgrade it
            if (newModuleType != null && legacyModule != null && newModule == null)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(legacyModule);
                else
                    UnityEngine.Object.DestroyImmediate(legacyModule);

                targetObj.AddComponent(newModuleType);
            }
            else if (targetObj.GetComponent<BaseInputModule>() == null)
            {
                AttachBestInputModule(targetObj);
            }
        }
    }
}
