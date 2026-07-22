using UnityEngine;
using System.Collections.Generic;

public class RepairLevelBuilder : MonoBehaviour
{
    [Header("关卡数据")]
    [SerializeField] private RepairLevelData levelData;
    public RepairLevelData LevelData
    {
        get => levelData;
        set => levelData = value;
    }

    [Header("生成根节点")]
    [SerializeField] private Transform levelRoot;

    [Header("是否启动时自动生成")]
    [SerializeField] private bool buildOnStart = true;

    private readonly List<GameObject> generatedObjects = new List<GameObject>();

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

        GameObject bgObj = new GameObject("Background");
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

            GameObject repairedObj = CreateSpriteObject(
                "Repaired_" + point.id,
                point.repairedSprite,
                point.slotPosition,
                point.repairedSortingOrder,
                slotRoot.transform
            );

            repairedObj.SetActive(false);

            GameObject dragObj = CreateSpriteObject(
                "DragPiece_" + point.id,
                point.dragPieceSprite,
                point.dragStartPosition,
                point.dragSortingOrder,
                levelRoot
            );

            if (point.addPolygonCollider )
            {
                if (dragObj.GetComponent<Collider2D>() == null)
                {
                    dragObj.AddComponent<PolygonCollider2D>();
                }
            }

            //PieceIdentity identity = dragObj.AddComponent<PieceIdentity>();
            //SetPieceId(identity, point.id);

            //DraggablePiece draggable = dragObj.AddComponent<DraggablePiece>();

            //RepairSlot slot = slotRoot.AddComponent<RepairSlot>();

            //SetRepairSlotData(
            //    slot,
            //    point.id,
            //    point.snapDistance,
            //    grayObj,
            //    repairedObj,
            //    slotRoot.transform
            //);

            //SetDraggableTarget(draggable, slot);

            PieceIdentity identity = dragObj.AddComponent<PieceIdentity>();
            identity.Init(point.id);

            RepairSlot slot = slotRoot.AddComponent<RepairSlot>();
            slot.Init(
                point.id,
                point.snapDistance,
                grayObj,
                repairedObj,
                slotRoot.transform
            );

            DraggablePiece draggable = dragObj.AddComponent<DraggablePiece>();
            draggable.Init(slot, Camera.main);
        }
    }

    private void BuildManager()
    {
        GameObject managerObj = new GameObject("RepairManager");
        managerObj.transform.SetParent(levelRoot);
        managerObj.AddComponent<RepairManager>();
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

    private void SetPieceId(PieceIdentity identity, string id)
    {
        var field = typeof(PieceIdentity).GetField(
            "pieceId",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            field.SetValue(identity, id);
        }
    }

    private void SetDraggableTarget(DraggablePiece draggable, RepairSlot slot)
    {
        var field = typeof(DraggablePiece).GetField(
            "targetSlot",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            field.SetValue(draggable, slot);
        }
    }

    private void SetRepairSlotData(
        RepairSlot slot,
        string requiredId,
        float snapDistance,
        GameObject grayObj,
        GameObject repairedObj,
        Transform snapPoint
    )
    {
        System.Type type = typeof(RepairSlot);

        SetPrivateField(type, slot, "requiredPieceId", requiredId);
        SetPrivateField(type, slot, "snapDistance", snapDistance);
        SetPrivateField(type, slot, "graySlotObject", grayObj);
        SetPrivateField(type, slot, "repairedImageObject", repairedObj);
        SetPrivateField(type, slot, "snapPoint", snapPoint);
    }

    private void SetPrivateField(System.Type type, object target, string fieldName, object value)
    {
        var field = type.GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
