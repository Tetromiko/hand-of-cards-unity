using UnityEditor;
using UnityEngine;

namespace Tetromiko.CardsHandLayout.Editor
{
    [CustomEditor(typeof(CardHandController))]
    public class CardHandControllerEditor : UnityEditor.Editor
    {
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
