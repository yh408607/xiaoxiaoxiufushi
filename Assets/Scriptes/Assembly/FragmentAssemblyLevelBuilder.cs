using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碎片拼接关卡生成器。
/// 根据 FragmentAssemblyLevelData 在场景中动态创建背景和所有碎片。
/// </summary>
public class FragmentAssemblyLevelBuilder : MonoBehaviour
{
    [Header("关卡数据")]
    [SerializeField] private FragmentAssemblyLevelData levelData;

    [Header("生成根节点")]
    [Tooltip("所有运行时生成的对象都会放在该节点下。")]
    [SerializeField] private Transform levelRoot;

    [Header("拖拽相机")]
    [SerializeField] private Camera dragCamera;

    [Header("擦拭材质")]
    [Tooltip("请配置使用 Custom/FragmentAssemblyWipe Shader 的材质。")]
    [SerializeField] private Material wipeMaterial;

    [Header("UI 擦拭工具")]
    [SerializeField] private FragmentAssemblyWipeUITool brushTool;
    [SerializeField] private FragmentAssemblyWipeUITool towelTool;

    [Header("拼接手指引导")]
    [Tooltip("拖动碎片时跟随指针移动的 UI 手指图标。")]
    [SerializeField] private FingerDragUI fingerDragUI;

    [Header("生成设置")]
    [Tooltip("启动场景时是否自动生成关卡。")]
    [SerializeField] private bool buildOnStart = true;

    // 记录本次生成的对象，方便重新生成或清理。
    private readonly List<GameObject> generatedObjects =new List<GameObject>();

    // 当前关卡的拼接管理器。
    private FragmentAssemblyManager assemblyManager;
    private GameObject wireframeObject;
    private GameObject backgroundObject;
    private GameObject cleanBackgroundObject;

    /// <summary>
    /// 当前使用的关卡数据。
    /// </summary>
    public FragmentAssemblyLevelData LevelData => levelData;

    /// <summary>
    /// 当前生成的拼接管理器。
    /// </summary>
    public FragmentAssemblyManager AssemblyManager =>
        assemblyManager;

    private void Start()
    {
        //if (buildOnStart)
        //{
        //    BuildLevel();
        //}
    }

    /// <summary>
    /// 设置当前要生成的关卡数据。
    /// </summary>
    public void SetLevelData(FragmentAssemblyLevelData data)
    {
        levelData = data;
    }

    /// <summary>
    /// 清理旧关卡并重新生成当前关卡。
    /// </summary>
    public void BuildLevel()
    {
        ClearLevel();

        assemblyManager = null;

        if (levelData == null)
        {
            Debug.LogWarning(
                "FragmentAssemblyLevelBuilder：没有配置关卡数据。"
            );
            return;
        }

        EnsureLevelRoot();

        if (dragCamera == null)
        {
            dragCamera = Camera.main;
        }

        if (fingerDragUI == null)
        {
            fingerDragUI =
                FindObjectOfType<FingerDragUI>(true);
        }

        // 完整底图在拼接阶段保持隐藏，全部碎片固定后再显示，
        // 用来覆盖碎片之间可能存在的微小缝隙。
        BuildBackground();

        BuildWireframe();
        BuildFragments();
        BuildAssemblyManager();
        BuildWipeStages();
    }

    /// <summary>
    /// 确保关卡有一个生成根节点。
    /// </summary>
    private void EnsureLevelRoot()
    {
        if (levelRoot != null)
        {
            return;
        }

        GameObject rootObject = new GameObject(
            levelData.levelName + "_AssemblyRoot"
        );

        levelRoot = rootObject.transform;
        generatedObjects.Add(rootObject);
    }

    /// <summary>
    /// 创建文物完整底图。
    /// </summary>
    private void BuildBackground()
    {
        if (levelData.backgroundSprite == null ||
            levelData.cleanBackgroundSprite == null)
        {
            Debug.LogError(
                "请同时配置待清洁完整图和完全干净底图。",
                this
            );
            return;
        }

        // 下层：完全干净底图。
        cleanBackgroundObject = new GameObject("CleanBackground");
        cleanBackgroundObject.transform.SetParent(levelRoot);
        cleanBackgroundObject.transform.position =
            levelData.backgroundPosition;
        cleanBackgroundObject.transform.localScale =
            levelData.backgroundScale;

        SpriteRenderer cleanRenderer =
            cleanBackgroundObject.AddComponent<SpriteRenderer>();

        cleanRenderer.sprite = levelData.cleanBackgroundSprite;
        cleanRenderer.sortingOrder =
            levelData.backgroundSortingOrder - 1;

        cleanBackgroundObject.SetActive(false);
        generatedObjects.Add(cleanBackgroundObject);

        // 上层：接受两阶段擦拭的完整图。
        backgroundObject = new GameObject("AssemblyBackground");
        backgroundObject.transform.SetParent(levelRoot);
        backgroundObject.transform.position =
            levelData.backgroundPosition;
        backgroundObject.transform.localScale =
            levelData.backgroundScale;

        SpriteRenderer renderer =
            backgroundObject.AddComponent<SpriteRenderer>();

        renderer.sprite = levelData.backgroundSprite;
        renderer.sortingOrder = levelData.backgroundSortingOrder;

        backgroundObject.SetActive(false);
        generatedObjects.Add(backgroundObject);
    }

    /// <summary>
    /// 创建玩家拼接时使用的完整文物线框。
    /// 线框只作为目标提示，不会替代真实碎片。
    /// </summary>
    private void BuildWireframe()
    {
        if (levelData.wireframeSprite == null)
        {
            Debug.LogWarning(
                "FragmentAssemblyLevelBuilder：没有配置拼接线框图。"
            );
            return;
        }

        wireframeObject = new GameObject("AssemblyWireframe");
        wireframeObject.transform.SetParent(levelRoot);
        wireframeObject.transform.position =
            levelData.wireframePosition;
        wireframeObject.transform.localScale =
            levelData.wireframeScale;

        SpriteRenderer renderer =
            wireframeObject.AddComponent<SpriteRenderer>();
        renderer.sprite = levelData.wireframeSprite;
        renderer.sortingOrder =
            levelData.wireframeSortingOrder;

        generatedObjects.Add(wireframeObject);
    }

    /// <summary>
    /// 根据数据创建所有可拖拽碎片。
    /// </summary>
    private void BuildFragments()
    {
        if (levelData.pieces == null)
        {
            return;
        }

        foreach (FragmentAssemblyPieceData pieceData in levelData.pieces)
        {
            if (pieceData == null)
            {
                continue;
            }

            if (pieceData.sprite == null)
            {
                Debug.LogWarning(
                    "FragmentAssemblyLevelBuilder：碎片 "
                    + pieceData.id
                    + " 没有配置图片。"
                );

                continue;
            }

            GameObject fragmentObject =
                CreateFragmentObject(pieceData);

            generatedObjects.Add(fragmentObject);
        }
    }

    /// <summary>
    /// 创建单个碎片对象并完成初始化。
    /// </summary>
    private GameObject CreateFragmentObject(
        FragmentAssemblyPieceData pieceData)
    {
        GameObject fragmentObject = new GameObject(
            "Fragment_" + pieceData.id
        );

        fragmentObject.transform.SetParent(levelRoot);
        fragmentObject.transform.position =
            pieceData.startPosition;
        fragmentObject.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                pieceData.startRotation
            );

        SpriteRenderer renderer =
            fragmentObject.AddComponent<SpriteRenderer>();

        renderer.sprite = pieceData.sprite;
        renderer.sortingOrder = pieceData.sortingOrder;

        AddCollider(fragmentObject, pieceData);

        FragmentAssemblyPiece piece =
            fragmentObject.AddComponent<FragmentAssemblyPiece>();

        piece.Init(
            pieceData.id,
            pieceData.startPosition,
            pieceData.startRotation,
            pieceData.targetPosition,
            pieceData.targetRotation,
            pieceData.snapDistance,
            pieceData.rotationTolerance,
            dragCamera,
            fingerDragUI
        );

        return fragmentObject;
    }

    /// <summary>
    /// 根据关卡数据给碎片添加碰撞器。
    /// </summary>
    private void AddCollider(
        GameObject fragmentObject,
        FragmentAssemblyPieceData pieceData)
    {
        if (pieceData.usePolygonCollider)
        {
            // PolygonCollider2D 会根据 Sprite 轮廓生成不规则碰撞区域。
            fragmentObject.AddComponent<PolygonCollider2D>();
        }
        else
        {
            // BoxCollider2D 适合形状比较规则的碎片。
            fragmentObject.AddComponent<BoxCollider2D>();
        }
    }

    /// <summary>
    /// 创建拼接管理器并绑定全部碎片。
    /// </summary>
    private void BuildAssemblyManager()
    {
        GameObject managerObject = new GameObject(
            "FragmentAssemblyManager"
        );

        managerObject.transform.SetParent(levelRoot);

        assemblyManager =
            managerObject.AddComponent<FragmentAssemblyManager>();

        FragmentAssemblyPiece[] pieces =
            levelRoot.GetComponentsInChildren<
                FragmentAssemblyPiece
            >();

        assemblyManager.Init(pieces);
        generatedObjects.Add(managerObject);
    }

    /// <summary>
    /// 创建灰尘层、污渍层和两个阶段之间的切换控制器。
    /// </summary>
    /// 

    private void BuildWipeStages()
    {
        if (backgroundObject == null || cleanBackgroundObject == null)
        {
            return;
        }

        if (brushTool == null || towelTool == null)
        {
            FragmentAssemblyWipeUITool[] tools =
                FindObjectsOfType<FragmentAssemblyWipeUITool>(true);

            foreach (FragmentAssemblyWipeUITool tool in tools)
            {
                if (brushTool == null && tool.name.Contains("Brush"))
                {
                    brushTool = tool;
                }
                else if (towelTool == null && tool.name.Contains("Towel"))
                {
                    towelTool = tool;
                }
            }
        }

        if (brushTool == null || towelTool == null)
        {
            Debug.LogError("请配置刷子和毛巾 UI 工具。", this);
            return;
        }

        if (wipeMaterial == null ||
            wipeMaterial.shader == null ||
            wipeMaterial.shader.name != "Custom/FragmentAssemblyWipe")
        {
            Debug.LogError(
                "请给生成器配置使用 Custom/FragmentAssemblyWipe 的材质。",
                this
            );
            return;
        }

        FragmentWipeController wipeController =
            backgroundObject.AddComponent<FragmentWipeController>();

        if (!wipeController.Init(wipeMaterial))
        {
            return;
        }

        // 两个工具操作同一个完整图。
        brushTool.Init(wipeController, dragCamera);
        towelTool.Init(wipeController, dragCamera);

        GameObject stageObject =
            new GameObject("FragmentAssemblyWipeStageController");

        stageObject.transform.SetParent(levelRoot);
        generatedObjects.Add(stageObject);

        FragmentAssemblyWipeStageController stageController =
            stageObject.AddComponent<FragmentAssemblyWipeStageController>();

        stageController.Init(
            assemblyManager,
            wipeController,
            brushTool,
            towelTool,
            wireframeObject,
            backgroundObject,
            cleanBackgroundObject,
            levelData
        );

        FragmentAssemblyScoreController scoreController =
            stageObject.AddComponent<FragmentAssemblyScoreController>();

        scoreController.Init(
            stageController,
            levelData.levelName,
            levelData.threeStarTime,
            levelData.twoStarTime
        );
    }


    /// <summary>
    /// 创建一个世界空间擦拭层。
    /// </summary>
    //private FragmentWipeController CreateWipeLayer(
    //    string objectName,
    //    Sprite layerSprite,
    //    int sortingOrder,
    //    float size,
    //    float completePercent)
    //{
    //    if (layerSprite == null)
    //    {
    //        return null;
    //    }

    //    GameObject layerObject = new GameObject(objectName);
    //    layerObject.transform.SetParent(levelRoot);
    //    layerObject.transform.position = levelData.backgroundPosition;
    //    layerObject.transform.localScale = levelData.backgroundScale;

    //    SpriteRenderer renderer =
    //        layerObject.AddComponent<SpriteRenderer>();
    //    renderer.sprite = layerSprite;
    //    renderer.sortingOrder = sortingOrder;

    //    FragmentWipeController controller =
    //        layerObject.AddComponent<FragmentWipeController>();
    //    controller.Init(wipeMaterial, size, completePercent);
    //    controller.DisableWiping();

    //    generatedObjects.Add(layerObject);
    //    return controller;
    //}

    /// <summary>
    /// 清理当前生成的关卡对象。
    /// </summary>
    public void ClearLevel()
    {
        for (int i = generatedObjects.Count - 1; i >= 0; i--)
        {
            GameObject generatedObject = generatedObjects[i];

            if (generatedObject == null)
            {
                continue;
            }

#if UNITY_EDITOR
            // 编辑器非运行状态下必须使用 DestroyImmediate。
            if (!Application.isPlaying)
            {
                DestroyImmediate(generatedObject);
            }
            else
            {
                Destroy(generatedObject);
            }
#else
            Destroy(generatedObject);
#endif
        }

        //generatedObjects.Clear();
        //assemblyManager = null;
        //wireframeObject = null;
        //backgroundObject = null;

        generatedObjects.Clear();
        assemblyManager = null;
        wireframeObject = null;
        backgroundObject = null;
        cleanBackgroundObject = null;
    }

    private void OnDestroy()
    {
        ClearLevel();
    }
}
