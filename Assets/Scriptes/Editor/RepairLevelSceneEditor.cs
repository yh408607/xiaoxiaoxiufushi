#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RepairLevelBuilder))]
public class RepairLevelSceneEditor : Editor
{
    private RepairLevelBuilder builder;
    private RepairLevelData levelData;

    private void OnEnable()
    {
        builder = (RepairLevelBuilder)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (builder == null) return;

        levelData = GetPrivateField<RepairLevelData>(
            typeof(RepairLevelBuilder),
            builder,
            "levelData"
        );

        if (levelData == null) return;
        if (levelData.repairPoints == null) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        for (int i = 0; i < levelData.repairPoints.Count; i++)
        {
            RepairPointData point = levelData.repairPoints[i];

            DrawSlotHandle(point, i);
            DrawDragHandle(point, i);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelData);
        }
    }

    private void DrawSlotHandle(RepairPointData point, int index)
    {
        Handles.color = Color.green;

        EditorGUI.BeginChangeCheck();

        Vector3 newPos = Handles.PositionHandle(
            point.slotPosition,
            Quaternion.identity
        );

        Handles.Label(
            point.slotPosition + Vector3.up * 0.3f,
            $"Slot {index + 1}\n{point.id}"
        );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(levelData, "Move Repair Slot");
            point.slotPosition = newPos;
            EditorUtility.SetDirty(levelData);
        }
    }

    private void DrawDragHandle(RepairPointData point, int index)
    {
        Handles.color = Color.cyan;

        EditorGUI.BeginChangeCheck();

        Vector3 newPos = Handles.PositionHandle(
            point.dragStartPosition,
            Quaternion.identity
        );

        Handles.Label(
            point.dragStartPosition + Vector3.up * 0.3f,
            $"Drag {index + 1}\n{point.id}"
        );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(levelData, "Move Drag Piece");
            point.dragStartPosition = newPos;
            EditorUtility.SetDirty(levelData);
        }
    }

    private T GetPrivateField<T>(System.Type type, object target, string fieldName)
    {
        var field = type.GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field == null)
        {
            return default;
        }

        return (T)field.GetValue(target);
    }
}
#endif
