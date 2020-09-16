using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(MissionPreset), true, isFallback = true)]
public class EditorMissionPreset : Editor
{
    protected MissionPreset missionPreset;

    protected ReorderableList waveList;
    protected int waveEntryIndex;

    protected void OnEnable()
    {
        missionPreset = (MissionPreset)target;

        waveList = new ReorderableList(serializedObject, serializedObject.FindProperty("waves"), true, true, true, true);
        waveList.drawHeaderCallback = DrawModelListHeaderCallback;
        waveList.drawElementCallback = DrawModelListElementCallback;
        //waveList.elementHeightCallback = ;
        waveList.onAddCallback = OnModelEntryAdded;
        waveList.onRemoveCallback = OnModelEntryRemoved;

        waveEntryIndex = -1;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), new GUIContent("Script", "Beware! Changing this will also change the component type"));

        DrawInspector();

        if (GUI.changed)
            OnApplyChanges();
    }

    protected void DrawInspector()
    {
        SerializedProperty settings = serializedObject.FindProperty("settings");
        EditorGUILayout.PropertyField(settings, new GUIContent("Settings"), true);

        if (waveList != null)
        {
            waveList.DoLayoutList();
        }
        else
        {
            EditorGUILayout.LabelField("ERROR: Wave list not initialized!");
        }

        if (waveEntryIndex >= 0)
        {
            SerializedProperty wave = waveList.serializedProperty.GetArrayElementAtIndex(waveEntryIndex);
            EditorGUILayout.PropertyField(wave, new GUIContent("Mission Wave " + waveEntryIndex), true);

            /*SerializedProperty time = property.FindPropertyRelative("startTime");
            time.floatValue = EditorGUILayout.FloatField(new GUIContent("Start Time"), time.floatValue);

            SerializedProperty prefab = property.FindPropertyRelative("pawnPrefab");
            EditorGUILayout.ObjectField(prefab, new GUIContent("Pawn Prefab"));

            SerializedProperty count = property.FindPropertyRelative("spawnCount");
            count.intValue = EditorGUILayout.IntField(new GUIContent("Spawn Count"), count.intValue);

            SerializedProperty interval = property.FindPropertyRelative("spawnInterval");
            interval.floatValue = EditorGUILayout.FloatField(new GUIContent("Spawn Interval"), interval.floatValue);*/
        }
        else
        {
            EditorGUILayout.LabelField("Select a Wave from the list to edit");
        }
    }

    protected virtual void OnApplyChanges()
    {
        serializedObject.ApplyModifiedProperties();

        if (PrefabUtility.GetPrefabAssetType(target) >= PrefabAssetType.Regular)
        {
            Object obj = PrefabUtility.GetPrefabInstanceHandle(target);
            if (obj != null) { EditorUtility.SetDirty(obj); }
        }
    }

    private void DrawModelListHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect, new GUIContent("Waves", "All the waves in the mission"));
    }

    private void DrawModelListElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.y += 2;
        rect.height -= 4;

        if (isFocused) { waveEntryIndex = index; }

        SerializedProperty property = waveList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty prefab = property.FindPropertyRelative("pawnPrefab");
        EditorGUI.LabelField(rect, new GUIContent(string.Format("Wave {0}: {1} - T={2:F1} N={3} every {4:F1}s", index,
            (prefab.objectReferenceValue != null ? prefab.objectReferenceValue.name : "NULL"),
            property.FindPropertyRelative("startTime").floatValue,
            property.FindPropertyRelative("spawnCount").intValue,
            property.FindPropertyRelative("spawnInterval").floatValue)));
    }

    private void OnModelEntryAdded(ReorderableList rList)
    {
        Undo.RecordObject(target, "Undo Add Wave entry");

        int index = rList.serializedProperty.arraySize;
        rList.serializedProperty.InsertArrayElementAtIndex(index);
        rList.index = index;
    }

    private void OnModelEntryRemoved(ReorderableList rList)
    {
        Undo.RecordObject(target, "Undo Remove Wave entry");
        rList.serializedProperty.DeleteArrayElementAtIndex(rList.index);
        rList.index = Mathf.Clamp(rList.index - 1, 0, rList.count - 1);
    }
}
