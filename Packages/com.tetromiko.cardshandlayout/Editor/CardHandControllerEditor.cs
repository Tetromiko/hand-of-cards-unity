using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Tetromiko.CardsHandLayout.Editor
{
    [CustomEditor(typeof(CardHandController))]
    public class CardHandControllerEditor : UnityEditor.Editor
    {
        [MenuItem("GameObject/UI/Cards Hand (Tetromiko)", false, 10)]
        public static void CreateCardHandInHierarchy(MenuCommand menuCommand)
        {
            // Ensure Canvas exists
#if UNITY_2023_1_OR_NEWER
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
#else
            Canvas canvas = Object.FindObjectOfType<Canvas>();
#endif
            if (canvas == null)
            {
                var canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }

            // Ensure Adaptive EventSystem exists (New Input System / Legacy compliant)
            var es = EventSystemAdapter.EnsureAdaptiveEventSystem();
            if (es != null)
            {
                Undo.RegisterCreatedObjectUndo(es.gameObject, "Create EventSystem");
            }

            // Create Card Hand Object
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = canvas.gameObject;
            }

            var handObj = new GameObject("CardHand", typeof(RectTransform), typeof(CardHandController));
            GameObjectUtility.SetParentAndAlign(handObj, parent);

            var rt = handObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 100f);
            rt.sizeDelta = new Vector2(900f, 260f);

            Undo.RegisterCreatedObjectUndo(handObj, "Create CardHand");
            Selection.activeObject = handObj;
        }

        [MenuItem("GameObject/UI/Cards Hand Web-Clone Demo (Tetromiko)", false, 11)]
        [MenuItem("Tools/Tetromiko/Setup Web-Style Demo Scene")]
        public static void CreateCompleteWebDemoScene()
        {
            var demoType = System.Type.GetType("Tetromiko.CardsHandLayout.Samples.CardHandDemoController, Assembly-CSharp")
                ?? System.Type.GetType("Tetromiko.CardsHandLayout.Samples.CardHandDemoController, com.tetromiko.cardshandlayout")
                ?? System.Type.GetType("Tetromiko.CardsHandLayout.Samples.CardHandDemoController");

            var demoObj = new GameObject("WebStyleCardDemo");
            if (demoType != null)
            {
                var comp = demoObj.AddComponent(demoType);
                var method = demoType.GetMethod("BuildCompleteDemoUI");
                if (method != null) method.Invoke(comp, null);
            }
            else
            {
                CreateCardHandInHierarchy(new MenuCommand(null));
            }

            Undo.RegisterCreatedObjectUndo(demoObj, "Create Web Style Card Demo");
            Selection.activeObject = demoObj;
        }

        public override void OnInspectorGUI()
        {
            CardHandController controller = (CardHandController)target;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Debug & Tools", EditorStyles.boldLabel);

            float minHandWidth = CardHandLayoutEngine.CalculateMinHandWidth(
                controller.CardsCount,
                controller.Settings.minCardDistance,
                controller.Settings.hoverDistance,
                controller.Settings.cardWidth
            );

            EditorGUILayout.HelpBox(
                $"Cards in Hand: {controller.CardsCount}\n" +
                $"Current Hand Width: {controller.Settings.handWidth:F0} px\n" +
                $"Minimum Hand Width: {minHandWidth:F0} px",
                MessageType.Info
            );

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Card"))
            {
                controller.AddCard();
                EditorUtility.SetDirty(controller);
            }
            if (GUILayout.Button("- Remove Card"))
            {
                controller.RemoveLastCard();
                EditorUtility.SetDirty(controller);
            }
            if (GUILayout.Button("Reset Hand (5)"))
            {
                controller.CreateDefaultCards(5);
                EditorUtility.SetDirty(controller);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear All Cards"))
            {
                if (EditorUtility.DisplayDialog("Clear Hand", "Remove all cards from hand?", "Yes", "No"))
                {
                    controller.ClearHand();
                    EditorUtility.SetDirty(controller);
                }
            }
        }
    }
}
