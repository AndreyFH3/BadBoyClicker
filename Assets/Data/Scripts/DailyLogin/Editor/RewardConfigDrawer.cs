using UnityEditor;
using UnityEngine;

namespace DailyLogin
{
    [CustomPropertyDrawer(typeof(RewardConfig))]
    public class RewardConfigDrawer : PropertyDrawer
    {
        private const float LineSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty rewardType = property.FindPropertyRelative("_rewardType");
            SerializedProperty currencyType = property.FindPropertyRelative("_currencyType");
            SerializedProperty amount = property.FindPropertyRelative("_amount");
            SerializedProperty amountMax = property.FindPropertyRelative("_amountMax");
            SerializedProperty rewardId = property.FindPropertyRelative("_rewardId");
            SerializedProperty icon = property.FindPropertyRelative("_icon");
            SerializedProperty displayTextLocalizationKey = property.FindPropertyRelative("_displayTextLocalizationKey");
            SerializedProperty displayText = property.FindPropertyRelative("_displayText");

            Rect line = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            RewardType type = (RewardType)rewardType.enumValueIndex;

            DrawLine(ref line, rewardType);

            switch (type)
            {
                case RewardType.Currency:
                    DrawLine(ref line, currencyType);
                    DrawLine(ref line, amount, new GUIContent("Amount (Min)"));
                    DrawLine(ref line, amountMax, new GUIContent("Amount Max (0 = fixed)"));
                    break;
                case RewardType.Boost:
                    DrawLine(ref line, rewardId, new GUIContent("Boost Id"));
                    break;
                case RewardType.Chest:
                    DrawLine(ref line, rewardId, new GUIContent("Chest Id"));
                    break;
                case RewardType.Cosmetic:
                    DrawLine(ref line, rewardId, new GUIContent("Cosmetic Id"));
                    break;
            }

            DrawLine(ref line, icon);
            DrawLine(ref line, displayTextLocalizationKey);
            DrawLine(ref line, displayText);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RewardType type = (RewardType)property.FindPropertyRelative("_rewardType").enumValueIndex;

            int lineCount = type switch
            {
                RewardType.Currency => 7,
                RewardType.Boost => 5,
                RewardType.Chest => 5,
                RewardType.Cosmetic => 5,
                _ => 5
            };

            float lineHeight = EditorGUIUtility.singleLineHeight + LineSpacing;
            return lineCount * lineHeight - LineSpacing;
        }

        private static void DrawLine(ref Rect line, SerializedProperty property, GUIContent label = null)
        {
            EditorGUI.PropertyField(line, property, label ?? new GUIContent(property.displayName));
            line.y += line.height + LineSpacing;
        }
    }
}
