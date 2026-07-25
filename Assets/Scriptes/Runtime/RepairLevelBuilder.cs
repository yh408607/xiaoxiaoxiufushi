using UnityEngine;
using System.Collections.Generic;

public class RepairLevelBuilder : MonoBehaviour
{
    [Header("关卡数据")]
    [SerializeField] private RepairLevelData levelData;

    [Header("生成根节点")]
    [SerializeField] private Transform levelRoot;

    [Header("是否启动时自动生成")]
    [SerializeField] private bool buildOnStart = true;

    private readonly List<GameObject> generatedObjects = new List<GameObject>();

    private RepairManager currentManager;

    public RepairManager CurrentManager => currentManager;

    private void Start()
    {
        if (buildOnStart)
        {
            BuildLevel();
        }
    }

    public void SetLevelData(RepairLevelData data)
    {
        levelData = data;
    }

    public void BuildLevel()
    {
        ClearLevel();

        currentManager = null;

        if (levelData == null)
        {
            Debug.LogWarning("RepairLevelBuilder：没有配置关卡数据");
            return;
        }

        if (levelRoot == null)
        {
            GameObject rootObj = new GameObject(levelData.levelName + "_Root");
            levelRoot = rootObj.transform;
        }

        BuildBackground();
        BuildRepairPoints();
        BuildManager();
    }

    public void ClearLevel()
    {
        for (int i = generatedObjects.Count - 1; i >= 0; i--)
        {
            if (generatedObjects[i] != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(generatedObjects[i]);
                }
                else
                {
                    Destroy(generatedObjects[i]);
                }
#else
                Destroy(generatedObjects[i]);
#endif
            }
        }

        generatedObjects.Clear();
    }

    private void BuildBackground()
    {
        if (levelData.backgroundSprite == null) return;

        GameObject bgObj = new GameObject("FullBackground");
        bgObj.transform.SetParent(levelRoot);
        bgObj.transform.position = levelData.backgroundPosition;

        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = levelData.backgroundSprite;
        sr.sortingOrder = levelData.backgroundSortingOrder;

        generatedObjects.Add(bgObj);
    }

    private void BuildRepairPoints()
    {
        foreach (RepairPointData point in levelData.repairPoints)
        {
            if (point == null) continue;

            GameObject slotRoot = new GameObject("Slot_" + point.id);
            slotRoot.transform.SetParent(levelRoot);
            slotRoot.transform.position = point.slotPosition;
            generatedObjects.Add(slotRoot);

            GameObject grayObj = CreateSpriteObject(
                "GraySlot_" + point.id,
                point.graySlotSprite,
                point.slotPosition,
                point.slotSortingOrder,
                slotRoot.transform
            );

            GameObject dragObj = CreateSpriteObject(
                "DragPiece_" + point.id,
                point.dragPieceSprite,
                point.dragStartPosition,
                point.dragSortingOrder,
                levelRoot
            );

            AddColliderByType(dragObj, point.colliderType);

            SpriteOutlineHighlighter highlighter = null;

            if (point.enableOutline)
            {
                highlighter = dragObj.AddComponent<SpriteOutlineHighlighter>();
                highlighter.InitConfig(point.outlineColor, point.outlineSize);
            }

            PieceIdentity identity = dragObj.AddComponent<PieceIdentity>();
            identity.Init(point.id);

            RepairSlot slot = slotRoot.AddComponent<RepairSlot>();
            slot.Init(
                point.id,
                point.snapDistance,
                grayObj,
                slotRoot.transform
            );

            DraggablePiece draggable = dragObj.AddComponent<DraggablePiece>();
            draggable.Init(slot, Camera.main, highlighter);
        }
    }

    private void BuildManager()
    {
        GameObject managerObj = new GameObject("RepairManager");
        managerObj.transform.SetParent(levelRoot);

        currentManager = managerObj.AddComponent<RepairManager>();

        generatedObjects.Add(managerObj);
    }

    private GameObject CreateSpriteObject(
        string objName,
        Sprite sprite,
        Vector3 position,
        int sortingOrder,
        Transform parent
    )
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(parent);
        obj.transform.position = position;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        generatedObjects.Add(obj);

        return obj;
    }

    private void AddColliderByType(GameObject obj, RepairColliderType colliderType)
    {
        if (obj == null) return;

        Collider2D oldCollider = obj.GetComponent<Collider2D>();

        if (oldCollider != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(oldCollider);
            }
            else
            {
                Destroy(oldCollider);
            }
#else
            Destroy(oldCollider);
#endif
        }

        switch (colliderType)
        {
            case RepairColliderType.None:
                break;

            case RepairColliderType.Box:
                obj.AddComponent<BoxCollider2D>();
                break;

            case RepairColliderType.Polygon:
                obj.AddComponent<PolygonCollider2D>();
                break;
        }
    }
}
