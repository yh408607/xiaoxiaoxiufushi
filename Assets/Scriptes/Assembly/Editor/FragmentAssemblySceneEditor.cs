#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// 在 Scene 视图中调整碎片的初始位置、目标位置和旋转。
/// </summary>
[CustomEditor(typeof(FragmentAssemblyLevelData))]
public class FragmentAssemblySceneEditor : Editor
{
    private void OnSceneGUI()
    {
        FragmentAssemblyLevelData levelData =
            (FragmentAssemblyLevelData)target;

        if (levelData.pieces == null)
        {
            return;
        }

        for (int i = 0; i < levelData.pieces.Count; i++)
        {
            FragmentAssemblyPieceData piece =
                levelData.pieces[i];

            if (piece == null)
            {
                continue;
            }

            DrawStartHandle(piece);
            DrawTargetHandle(piece);
        }
    }

    /// <summary>
    /// 绘制碎片初始位置和初始旋转控制柄。
    /// </summary>
    private void DrawStartHandle(
        FragmentAssemblyPieceData piece)
    {
        Handles.color = Color.cyan;

        EditorGUI.BeginChangeCheck();

        Vector3 newPosition =
            Handles.PositionHandle(
                piece.startPosition,
                Quaternion.Euler(
                    0f,
                    0f,
                    piece.startRotation
                )
            );

        Quaternion newRotation =
            Handles.RotationHandle(
                Quaternion.Euler(
                    0f,
                    0f,
                    piece.startRotation
                ),
                piece.startPosition
            );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(
                target,
                "Edit Fragment Start Transform"
            );

            piece.startPosition = newPosition;
            piece.startRotation =
                newRotation.eulerAngles.z;

            EditorUtility.SetDirty(target);
        }

        Handles.Label(
            piece.startPosition + Vector3.up * 0.25f,
            "Start: " + piece.id
        );
    }

    /// <summary>
    /// 绘制碎片目标位置和目标旋转控制柄。
    /// </summary>
    private void DrawTargetHandle(
        FragmentAssemblyPieceData piece)
    {
        Handles.color = Color.green;

        EditorGUI.BeginChangeCheck();

        Vector3 newPosition =
            Handles.PositionHandle(
                piece.targetPosition,
                Quaternion.Euler(
                    0f,
                    0f,
                    piece.targetRotation
                )
            );

        Quaternion newRotation =
            Handles.RotationHandle(
                Quaternion.Euler(
                    0f,
                    0f,
                    piece.targetRotation
                ),
                piece.targetPosition
            );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(
                target,
                "Edit Fragment Target Transform"
            );

            piece.targetPosition = newPosition;
            piece.targetRotation =
                newRotation.eulerAngles.z;

            EditorUtility.SetDirty(target);
        }

        Handles.Label(
            piece.targetPosition + Vector3.up * 0.25f,
            "Target: " + piece.id
        );
    }
}

#endif
