#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器布局预览对象。
/// 每个碎片会生成一个初始对象和一个半透明目标参考对象，
/// 方便关卡设计者直接在 Scene 视图中拖动并保存布局。
/// </summary>
public class FragmentAssemblyLayoutPreview : MonoBehaviour
{
    public string pieceId;
    public bool isTarget;
    public bool isReference;

    /// <summary>
    /// 创建或刷新当前关卡的编辑器布局预览。
    /// </summary>
    public static void CreateOrRefresh(
        FragmentAssemblyLevelData levelData
    )
    {
        if (levelData == null)
        {
            Debug.LogWarning("没有选择拼接关卡数据。");
            return;
        }

        Clear();

        GameObject rootObject = new GameObject(
            "FragmentAssemblyLayoutPreview"
        );

        Undo.RegisterCreatedObjectUndo(
            rootObject,
            "Create Assembly Layout Preview"
        );

        if (levelData.backgroundSprite != null)
        {
            CreateReferenceObject(
                rootObject.transform,
                "ReferenceArtwork",
                levelData.backgroundSprite,
                levelData.backgroundPosition,
                levelData.backgroundScale,
                0.22f,
                levelData.backgroundSortingOrder
            );
        }

        if (levelData.pieces == null)
        {
            Selection.activeGameObject = rootObject;
            return;
        }

        foreach (FragmentAssemblyPieceData piece in levelData.pieces)
        {
            if (piece == null || piece.sprite == null)
            {
                continue;
            }

            CreatePieceObject(
                rootObject.transform,
                piece,
                false
            );

            CreatePieceObject(
                rootObject.transform,
                piece,
                true
            );
        }

        Selection.activeGameObject = rootObject;
        SceneView.RepaintAll();
    }

    /// <summary>
    /// 将 Scene 中的初始对象和目标对象写回关卡数据。
    /// </summary>
    public static void SaveToLevelData(
        FragmentAssemblyLevelData levelData
    )
    {
        if (levelData == null)
        {
            Debug.LogWarning("没有选择拼接关卡数据。");
            return;
        }

        GameObject rootObject = GameObject.Find(
            "FragmentAssemblyLayoutPreview"
        );

        if (rootObject == null)
        {
            Debug.LogWarning(
                "没有找到编辑器布局预览，请先生成编辑器布局预览。"
            );
            return;
        }

        FragmentAssemblyLayoutPreview[] previewObjects =
            rootObject.GetComponentsInChildren<
                FragmentAssemblyLayoutPreview
            >();

        Undo.RecordObject(levelData, "Save Assembly Layout");

        foreach (FragmentAssemblyPieceData piece in levelData.pieces)
        {
            if (piece == null)
            {
                continue;
            }

            foreach (
                FragmentAssemblyLayoutPreview preview
                in previewObjects
            )
            {
                if (preview.pieceId != piece.id)
                {
                    continue;
                }

                Transform previewTransform =
                    preview.transform;

                if (preview.isTarget)
                {
                    piece.targetPosition =
                        previewTransform.position;
                    piece.targetRotation =
                        previewTransform.eulerAngles.z;
                }
                else
                {
                    piece.startPosition =
                        previewTransform.position;
                    piece.startRotation =
                        previewTransform.eulerAngles.z;
                }
            }
        }

        foreach (
            FragmentAssemblyLayoutPreview preview
            in previewObjects
        )
        {
            if (!preview.isReference)
            {
                continue;
            }

            levelData.backgroundPosition =
                preview.transform.position;
            levelData.backgroundScale =
                preview.transform.localScale;
        }

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();

        Debug.Log(
            "编辑器布局已保存：" + levelData.levelName
        );
    }

    /// <summary>
    /// 清理当前场景中的编辑器布局预览。
    /// </summary>
    public static void Clear()
    {
        GameObject rootObject = GameObject.Find(
            "FragmentAssemblyLayoutPreview"
        );

        if (rootObject == null)
        {
            return;
        }

        Object.DestroyImmediate(rootObject);
        SceneView.RepaintAll();
    }

    /// <summary>
    /// 创建一张半透明的参考图片。
    /// </summary>
    private static void CreateReferenceObject(Transform parent, string objectName,   Sprite sprite,  Vector3 position, Vector3 scale, float alpha,int sortingOrder)
    {
        GameObject referenceObject = new GameObject(objectName);
        referenceObject.transform.SetParent(parent);
        referenceObject.transform.position = position;
        referenceObject.transform.localScale = scale;

        SpriteRenderer renderer =  referenceObject.AddComponent<SpriteRenderer>();

        renderer.sprite = sprite;
        renderer.color = new Color(1f, 1f, 1f, alpha);
        renderer.sortingOrder = sortingOrder;

        FragmentAssemblyLayoutPreview preview = referenceObject.AddComponent<FragmentAssemblyLayoutPreview>();

        preview.isReference = true;
    }

    /// <summary>
    /// 创建一个初始碎片或目标参考碎片。
    /// </summary>
    private static void CreatePieceObject(
        Transform parent,
        FragmentAssemblyPieceData piece,
        bool isTarget
    )
    {
        GameObject pieceObject = new GameObject(
            (isTarget ? "Target__" : "Start__") + piece.id
        );

        pieceObject.transform.SetParent(parent);
        pieceObject.transform.position = isTarget
            ? piece.targetPosition
            : piece.startPosition;
        pieceObject.transform.rotation = Quaternion.Euler(
            0f,
            0f,
            isTarget ? piece.targetRotation : piece.startRotation
        );

        SpriteRenderer renderer =
            pieceObject.AddComponent<SpriteRenderer>();

        renderer.sprite = piece.sprite;
        renderer.color = isTarget
            ? new Color(0.2f, 1f, 0.3f, 0.3f)
            : Color.white;
        renderer.sortingOrder = isTarget
            ? piece.sortingOrder + 1000
            : piece.sortingOrder + 1001;

        FragmentAssemblyLayoutPreview preview =
            pieceObject.AddComponent<FragmentAssemblyLayoutPreview>();

        preview.pieceId = piece.id;
        preview.isTarget = isTarget;
        preview.isReference = false;
    }
}

#endif
