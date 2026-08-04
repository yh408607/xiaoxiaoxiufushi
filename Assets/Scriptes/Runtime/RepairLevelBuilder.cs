using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairLevelBuilder : MonoBehaviour
{
    [Header("关卡数据")]
    [SerializeField] private RepairLevelData levelData;

    [Header("生成根节点")]
    [SerializeField] private Transform levelRoot;

    [Header("是否启动时自动生成")]
    [SerializeField] private bool buildOnStart = true;

    [Header("擦灰材质")]
    [SerializeField] private Material dustWipeMaterial;

    [Header("UI 抹布")]
    [SerializeField] private WiperUITool sceneWiperTool;

    [Header("手指引导")]
    [SerializeField] private FingerDragUI fingerDragUI;

    private readonly List<GameObject> generatedObjects = new List<GameObject>();

    private RepairManager currentManager;



    public RepairManager CurrentManager => currentManager;

    private DustWipeController currentDustController;
    private WiperUITool currentWiperTool;


    private GameObject repairBackgroundObj;
    private GameObject cleanBackgroundObj;
    private GameObject dustLayerObj;

    private readonly List<RepairSlot> currentSlots = new List<RepairSlot>();

    public GameObject RepairBackgroundObj => repairBackgroundObj;
    public GameObject CleanBackgroundObj => cleanBackgroundObj;




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
        currentDustController = null;

        repairBackgroundObj = null;
        cleanBackgroundObj = null;
        dustLayerObj = null;

        currentSlots.Clear();

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

        BuildRepairBackground();
        BuildCleanBackground();
        BuildDustLayer();
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

    //private void BuildBackground()
    //{
    //    if (levelData.backgroundSprite == null) return;

    //    GameObject bgObj = new GameObject("FullBackground");
    //    bgObj.transform.SetParent(levelRoot);
    //    bgObj.transform.position = levelData.backgroundPosition;

    //    SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
    //    sr.sprite = levelData.backgroundSprite;
    //    sr.sortingOrder = levelData.backgroundSortingOrder;

    //    generatedObjects.Add(bgObj);
    //}


    private void BuildRepairBackground()
    {
        if (levelData.repairBackgroundSprite == null) return;

        repairBackgroundObj = CreateSpriteObject(
            "RepairBackground",
            levelData.repairBackgroundSprite,
            levelData.backgroundPosition,
            levelData.repairBackgroundSortingOrder,
            levelRoot
        );

        repairBackgroundObj.transform.localScale = levelData.backgroundScale;

        repairBackgroundObj.SetActive(true);
    }


    private void BuildCleanBackground()
    {
        if (levelData.cleanBackgroundSprite == null) return;

        cleanBackgroundObj = CreateSpriteObject(
            "CleanBackground",
            levelData.cleanBackgroundSprite,
            levelData.backgroundPosition,
            levelData.cleanBackgroundSortingOrder,
            levelRoot
        );

        cleanBackgroundObj.transform.localScale = levelData.backgroundScale;

        // 初始隐藏，修复完成后再显示
        cleanBackgroundObj.SetActive(false);
    }



    [SerializeField] private float spawnDuration = 0.45f;
    [SerializeField] private float spawnStagger = 0.06f;
    [SerializeField] private Ease spawnEase = Ease.OutBack;

    private Sequence spawnSequence;
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

            currentSlots.Add(slot);

            DraggablePiece draggable = dragObj.AddComponent<DraggablePiece>();
            draggable.Init(slot, Camera.main, highlighter);

            draggable.PlaySpawnTween(point.slotPosition, point.dragStartPosition, spawnDuration, spawnEase);        
        }

        //spawnSequence?.Kill();
        //spawnSequence = DOTween.Sequence();

        //int index = 0;

        //foreach (RepairPointData point in levelData.repairPoints)
        //{
        //    if (point == null) continue;

        //    GameObject slotRoot = new GameObject("Slot_" + point.id);
        //    slotRoot.transform.SetParent(levelRoot);
        //    slotRoot.transform.position = point.slotPosition;
        //    generatedObjects.Add(slotRoot);

        //    GameObject grayObj = CreateSpriteObject(
        //        "GraySlot_" + point.id,
        //        point.graySlotSprite,
        //        point.slotPosition,
        //        point.slotSortingOrder,
        //        slotRoot.transform
        //    );

        //    // 先创建拖拽碎片（先放在修复点）
        //    GameObject dragObj = CreateSpriteObject(
        //        "DragPiece_" + point.id,
        //        point.dragPieceSprite,
        //        point.slotPosition, // 关键：出生在修复点
        //        point.dragSortingOrder,
        //        levelRoot
        //    );

        //    AddColliderByType(dragObj, point.colliderType);

        //    SpriteOutlineHighlighter highlighter = null;
        //    if (point.enableOutline)
        //    {
        //        highlighter = dragObj.AddComponent<SpriteOutlineHighlighter>();
        //        highlighter.InitConfig(point.outlineColor, point.outlineSize);
        //    }

        //    PieceIdentity identity = dragObj.AddComponent<PieceIdentity>();
        //    identity.Init(point.id);

        //    RepairSlot slot = slotRoot.AddComponent<RepairSlot>();
        //    slot.Init(
        //        point.id,
        //        point.snapDistance,
        //        grayObj,
        //        slotRoot.transform
        //    );
        //    currentSlots.Add(slot);

        //    DraggablePiece draggable = dragObj.AddComponent<DraggablePiece>();
        //    draggable.Init(slot, Camera.main, highlighter);

        //    // 动画前禁止拖拽
        //    draggable.SetDragEnabled(false);

        //    // 错峰播放：从修复点 -> 摆放点
        //    float delay = index * spawnStagger;
        //    Vector3 endPos = point.dragStartPosition;

        //    spawnSequence.Insert(
        //        delay,
        //        dragObj.transform.DOMove(endPos, spawnDuration)
        //            .SetEase(spawnEase)
        //            .OnComplete(() =>
        //            {
        //                if (draggable != null)
        //                    draggable.SetDragEnabled(true);
        //            })
        //    );

        //    index++;
        //}

        //spawnSequence.OnComplete(() =>
        //{
        //    Debug.Log("碎片出生动画播放完成");
        //    // TODO: 这里可以触发你的引导逻辑
        //    // guideController?.Show();
        //});

        //spawnSequence.Play();
    }

    private void BuildManager()
    {
        GameObject managerObj = new GameObject("RepairManager");
        managerObj.transform.SetParent(levelRoot);

        currentManager = managerObj.AddComponent<RepairManager>();


        if (sceneWiperTool == null)
        {
            sceneWiperTool = FindObjectOfType<WiperUITool>(true);
        }

        if (fingerDragUI == null)
        {
            fingerDragUI = FindObjectOfType<FingerDragUI>(true);
        }

        currentWiperTool = sceneWiperTool;
        currentManager.Init( currentSlots, currentDustController,currentWiperTool, fingerDragUI, repairBackgroundObj,cleanBackgroundObj, levelData.threeStarTime, levelData.twoStarTime);

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

    private void BuildDustLayer()
    {
        if (levelData.dustSprite == null) return;

        GameObject dustObj = CreateSpriteObject(
            "DustLayer",
            levelData.dustSprite,
            levelData.backgroundPosition,
            levelData.dustSortingOrder,
            levelRoot
        );

        currentDustController = dustObj.AddComponent<DustWipeController>();
        currentDustController.InitConfig(
            dustWipeMaterial,
            levelData.wipeBrushSize,
            levelData.wipeCompletePercent
        );

        // 初始隐藏灰尘，修复完成后再显示
        currentDustController.DisableWiping();
    }

    //private void BuildWiperTool()
    //{
    //    if (currentDustController == null) return;
    //    if (wiperSprite == null)
    //    {
    //        Debug.LogWarning("没有配置抹布 Sprite，擦灰阶段将没有抹布");
    //        return;
    //    }

    //    GameObject wiperObj = CreateSpriteObject(
    //        "WiperTool",
    //        wiperSprite,
    //        wiperStartPosition,
    //        wiperSortingOrder,
    //        levelRoot
    //    );

    //    BoxCollider2D collider = wiperObj.AddComponent<BoxCollider2D>();

    //    currentWiperTool = wiperObj.AddComponent<WiperTool>();
    //    currentWiperTool.Init(currentDustController, Camera.main);
    //}


    // 假设你已有的数据结构
    // pieceDatas: 每个碎片包含 spawnWorldPos(摆放位) 与 targetWorldPos(修复点)


    private void OnDisable()
    {
        spawnSequence?.Kill();
    }

    private void OnDestroy()
    {
        spawnSequence?.Kill();
    }
}
