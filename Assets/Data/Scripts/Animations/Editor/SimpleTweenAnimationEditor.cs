using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SimpleTweenAnimation))]
[CanEditMultipleObjects]
public class SimpleTweenAnimationEditor : Editor
{
    private SerializedProperty _playMode;
    private SerializedProperty _autoKill;
    private SerializedProperty _rebuildOnPlay;
    private SerializedProperty _playBackwards;
    private SerializedProperty _restoreOnRewind;
    private SerializedProperty _restoreOnDisable;

    private SerializedProperty _onPlay;
    private SerializedProperty _onStepComplete;
    private SerializedProperty _onComplete;

    private SerializedProperty _steps;

    private ReorderableList _stepsList;
    private bool _eventsFoldout;

    private void OnEnable()
    {
        _playMode = serializedObject.FindProperty("_playMode");
        _autoKill = serializedObject.FindProperty("_autoKill");
        _rebuildOnPlay = serializedObject.FindProperty("_rebuildOnPlay");
        _playBackwards = serializedObject.FindProperty("_playBackwards");
        _restoreOnRewind = serializedObject.FindProperty("_restoreOnRewind");
        _restoreOnDisable = serializedObject.FindProperty("_restoreOnDisable");

        _onPlay = serializedObject.FindProperty("_onPlay");
        _onStepComplete = serializedObject.FindProperty("_onStepComplete");
        _onComplete = serializedObject.FindProperty("_onComplete");

        _steps = serializedObject.FindProperty("_steps");

        _stepsList = new ReorderableList(serializedObject, _steps, true, true, true, true)
        {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Tween Steps"),
            elementHeightCallback = GetElementHeight,
            drawElementCallback = DrawElement
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Playback", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_playMode);
        EditorGUILayout.PropertyField(_autoKill);
        EditorGUILayout.PropertyField(_rebuildOnPlay);
        EditorGUILayout.PropertyField(_playBackwards);
        EditorGUILayout.PropertyField(_restoreOnRewind);
        EditorGUILayout.PropertyField(_restoreOnDisable);

        EditorGUILayout.Space();
        _eventsFoldout = EditorGUILayout.Foldout(_eventsFoldout, "Sequence Events", true);
        if (_eventsFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_onPlay);
            EditorGUILayout.PropertyField(_onStepComplete);
            EditorGUILayout.PropertyField(_onComplete);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        _stepsList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();

        DrawPlaybackButtons();
    }

    private void DrawPlaybackButtons()
    {
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.LabelField("Preview (Play Mode only)", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Play"))
                    ForEachTarget(t => t.Play());
                if (GUILayout.Button("Restart"))
                    ForEachTarget(t => t.Restart());
                if (GUILayout.Button("Rewind"))
                    ForEachTarget(t => t.Rewind());
                if (GUILayout.Button("Complete"))
                    ForEachTarget(t => t.Complete());
                if (GUILayout.Button("Kill"))
                    ForEachTarget(t => t.Kill());
            }
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play Mode to preview the animation.", MessageType.Info);
    }

    private void ForEachTarget(System.Action<SimpleTweenAnimation> action)
    {
        foreach (Object obj in targets)
        {
            if (obj is SimpleTweenAnimation anim)
                action(anim);
        }
    }

    private float GetElementHeight(int index)
    {
        SerializedProperty step = _steps.GetArrayElementAtIndex(index);
        float line = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        float height = line; // foldout row

        if (!step.isExpanded)
            return height + EditorGUIUtility.standardVerticalSpacing;

        foreach (string name in GetVisibleFields(step))
        {
            SerializedProperty prop = step.FindPropertyRelative(name);
            if (prop != null)
                height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return height + EditorGUIUtility.standardVerticalSpacing * 2f;
    }

    private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty step = _steps.GetArrayElementAtIndex(index);
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        Rect line = new Rect(rect.x, rect.y + spacing, rect.width, EditorGUIUtility.singleLineHeight);

        SerializedProperty enabled = step.FindPropertyRelative("enabled");
        SerializedProperty name = step.FindPropertyRelative("name");
        SerializedProperty animType = step.FindPropertyRelative("animationType");

        string label = string.IsNullOrEmpty(name.stringValue)
            ? $"{index}: {(SimpleTweenAnimation.TweenAnimationType)animType.enumValueIndex}"
            : $"{index}: {name.stringValue} ({(SimpleTweenAnimation.TweenAnimationType)animType.enumValueIndex})";

        Rect toggleRect = new Rect(line.x, line.y, 18f, line.height);
        enabled.boolValue = EditorGUI.Toggle(toggleRect, enabled.boolValue);

        Rect foldoutRect = new Rect(line.x + 20f, line.y, line.width - 20f, line.height);
        step.isExpanded = EditorGUI.Foldout(foldoutRect, step.isExpanded, label, true);

        if (!step.isExpanded)
            return;

        EditorGUI.indentLevel++;
        float y = line.y + line.height + spacing;

        foreach (string field in GetVisibleFields(step))
        {
            SerializedProperty prop = step.FindPropertyRelative(field);
            if (prop == null)
                continue;

            float h = EditorGUI.GetPropertyHeight(prop, true);
            Rect r = new Rect(rect.x, y, rect.width, h);
            EditorGUI.PropertyField(r, prop, true);
            y += h + spacing;
        }

        EditorGUI.indentLevel--;
    }

    private static readonly string[] CommonHead =
    {
        "name", "insertMode", "animationType", "targetType", "targetOverride",
        "duration", "delay", "ease", "loops", "loopType", "ignoreTimeScale"
    };

    private System.Collections.Generic.IEnumerable<string> GetVisibleFields(SerializedProperty step)
    {
        foreach (string s in CommonHead)
            yield return s;

        var type = (SimpleTweenAnimation.TweenAnimationType)step.FindPropertyRelative("animationType").enumValueIndex;

        switch (type)
        {
            case SimpleTweenAnimation.TweenAnimationType.Move:
            case SimpleTweenAnimation.TweenAnimationType.LocalMove:
            case SimpleTweenAnimation.TweenAnimationType.AnchoredPosition:
                yield return "toVector3";
                yield return "isRelative";
                yield return "snapping";
                foreach (string s in FromBlock(step, "fromVector3")) yield return s;
                break;

            case SimpleTweenAnimation.TweenAnimationType.Scale:
            case SimpleTweenAnimation.TweenAnimationType.Rotate:
            case SimpleTweenAnimation.TweenAnimationType.LocalRotate:
                yield return "toVector3";
                yield return "isRelative";
                foreach (string s in FromBlock(step, "fromVector3")) yield return s;
                break;

            case SimpleTweenAnimation.TweenAnimationType.Fade:
            case SimpleTweenAnimation.TweenAnimationType.FillAmount:
                yield return "toFloat";
                foreach (string s in FromBlock(step, "fromFloat")) yield return s;
                break;

            case SimpleTweenAnimation.TweenAnimationType.Color:
                yield return "toColor";
                foreach (string s in FromBlock(step, "fromColor")) yield return s;
                break;

            case SimpleTweenAnimation.TweenAnimationType.PunchScale:
            case SimpleTweenAnimation.TweenAnimationType.PunchRotation:
                yield return "toVector3";
                yield return "vibrato";
                yield return "elasticity";
                break;

            case SimpleTweenAnimation.TweenAnimationType.PunchPosition:
                yield return "toVector3";
                yield return "vibrato";
                yield return "elasticity";
                yield return "snapping";
                break;

            case SimpleTweenAnimation.TweenAnimationType.ShakeScale:
            case SimpleTweenAnimation.TweenAnimationType.ShakeRotation:
                yield return "toVector3";
                yield return "vibrato";
                yield return "randomness";
                break;

            case SimpleTweenAnimation.TweenAnimationType.ShakePosition:
                yield return "toVector3";
                yield return "vibrato";
                yield return "randomness";
                yield return "snapping";
                break;
        }

        yield return "onStepComplete";
    }

    private System.Collections.Generic.IEnumerable<string> FromBlock(SerializedProperty step, string fromField)
    {
        yield return "setFrom";

        if (!step.FindPropertyRelative("setFrom").boolValue)
            yield break;

        yield return "useCustomFromValue";

        if (step.FindPropertyRelative("useCustomFromValue").boolValue)
            yield return fromField;
    }
}
