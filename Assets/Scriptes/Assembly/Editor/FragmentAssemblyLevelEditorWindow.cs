#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// 碎片拼接关卡编辑器窗口。
/// 用于创建关卡数据和编辑碎片基础配置。
/// </summary>
public class FragmentAssemblyLevelEditorWindow : EditorWindow
{
    private FragmentAssemblyLevelData levelData;
    private Vector2 scrollPosition;

    private string newLevelName = "NewAssemblyLevel";
    private string saveFolder = "Assets/AssemblyLevels";

    [MenuItem("Tools/Repair Game/Fragment Assembly Editor")]
    public static void OpenWindow()
    {
        FragmentAssemblyLevelEditorWindow window =
            GetWindow<FragmentAssemblyLevelEditorWindow>();

        window.titleContent =
            new GUIContent("Fragment Assembly Editor");

        window.Show();
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawLevelAssetArea();

        if (levelData == null)
        {
            EditorGUILayout.HelpBox(
                "请先创建或选择一个拼接关卡数据。",
                MessageType.Info
            );

            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition
        );

        DrawBaseSettings();
        DrawPieceList();

        EditorGUILayout.EndScrollView();

        DrawActionButtons();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelData);
        }
    }

    /// <summary>
    /// 绘制编辑器标题区域。
    /// </summary>
    private void DrawHeader()
    {
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "文物碎片拼接关卡编辑器",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "玩家需要将散落的文物碎片拖动到正确位置，" +
            "并自动吸附到指定旋转角度。",
            MessageType.Info
        );
    }

    /// <summary>
    /// 绘制关卡数据选择和创建区域。
    /// </summary>
    private void DrawLevelAssetArea()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(
            "关卡数据",
            EditorStyles.boldLabel
        );

        levelData = (FragmentAssemblyLevelData)
            EditorGUILayout.ObjectField(
                "当前关卡数据",
                levelData,
                typeof(FragmentAssemblyLevelData),
                false
            );

        newLevelName = EditorGUILayout.TextField(
            "新关卡名称",
            newLevelName
        );

        saveFolder = EditorGUILayout.TextField(
            "保存目录",
            saveFolder
        );

        if (GUILayout.Button("创建新的拼接关卡"))
        {
            CreateNewLevelData();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制关卡基础配置。
    /// </summary>
    private void DrawBaseSettings()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(
            "关卡基础配置",
            EditorStyles.boldLabel
        );

        levelData.levelName = EditorGUILayout.TextField(
            "关卡名称",
            levelData.levelName
        );

        levelData.narrationClip = (AudioClip)EditorGUILayout.ObjectField(
            "文物讲解音频",
            levelData.narrationClip,
            typeof(AudioClip),
            false
        );

        levelData.backgroundSprite = (Sprite)
            EditorGUILayout.ObjectField(
                "待清洁完整图",
                levelData.backgroundSprite,
                typeof(Sprite),
                false
            );

        levelData.cleanBackgroundSprite = (Sprite)
                EditorGUILayout.ObjectField(
                "完全干净底图",
                 levelData.cleanBackgroundSprite,
                typeof(Sprite),
                false);

        EditorGUILayout.HelpBox(
                "拼接完成后隐藏碎片，同时显示两张完整图。\n" +
                "刷子把上层图擦到 50% Alpha，毛巾再擦到 0%。\n" +
                "两张图共用位置和缩放，应保持相同尺寸、Pixels Per Unit 和 Pivot。",
                 MessageType.Info);

        levelData.backgroundPosition =
            EditorGUILayout.Vector3Field(
                "底图位置",
                levelData.backgroundPosition
            );
        if (levelData.backgroundSprite == null ||levelData.cleanBackgroundSprite == null)
        {
            EditorGUILayout.HelpBox(
                "请同时配置待清洁完整图和完全干净底图。",
                MessageType.Warning
            );
        }
        levelData.backgroundScale =
            EditorGUILayout.Vector3Field(
                "底图缩放",
                levelData.backgroundScale
            );

        levelData.backgroundSortingOrder =
            EditorGUILayout.IntField(
                "底图层级",
                levelData.backgroundSortingOrder
            );

        //levelData.showBackgroundAtRuntime =
        //    EditorGUILayout.Toggle(
        //        "运行时显示完整底图",
        //        levelData.showBackgroundAtRuntime
        //    );

        //EditorGUILayout.HelpBox(
        //    "完整底图只建议用于编辑器对齐。关闭后，运行时画面只显示实际碎片。",
        //    MessageType.Info
        //);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(
            "运行时拼接线框",
            EditorStyles.boldLabel
        );

        levelData.wireframeSprite = (Sprite)
            EditorGUILayout.ObjectField(
                "完整文物线框图",
                levelData.wireframeSprite,
                typeof(Sprite),
                false
            );

        levelData.wireframePosition =
            EditorGUILayout.Vector3Field(
                "线框位置",
                levelData.wireframePosition
            );

        levelData.wireframeScale =
            EditorGUILayout.Vector3Field(
                "线框缩放",
                levelData.wireframeScale
            );

        levelData.wireframeSortingOrder =
            EditorGUILayout.IntField(
                "线框层级",
                levelData.wireframeSortingOrder
            );

        EditorGUILayout.HelpBox(
            "线框会在拼接阶段显示，所有碎片固定后自动隐藏。",
            MessageType.Info
        );

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(
            "刷子除尘阶段",
            EditorStyles.boldLabel
        );

        //levelData.dustLayerSprite = (Sprite)
        //    EditorGUILayout.ObjectField(
        //        "灰尘层图片",
        //        levelData.dustLayerSprite,
        //        typeof(Sprite),
        //        false
        //    );

        //levelData.dustLayerSortingOrder =
        //    EditorGUILayout.IntField(
        //        "灰尘层级",
        //        levelData.dustLayerSortingOrder
        //    );

        levelData.dustCompletePercent =
            EditorGUILayout.Slider(
                "除尘完成比例",
                levelData.dustCompletePercent,
                0f,
                1f
            );

        levelData.brushSize =
            EditorGUILayout.FloatField(
                "刷子大小",
                levelData.brushSize
            );

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(
            "毛巾擦亮阶段",
            EditorStyles.boldLabel
        );

        //levelData.polishLayerSprite = (Sprite)
        //    EditorGUILayout.ObjectField(
        //        "污渍层图片",
        //        levelData.polishLayerSprite,
        //        typeof(Sprite),
        //        false
        //    );

        //levelData.polishLayerSortingOrder =
        //    EditorGUILayout.IntField(
        //        "污渍层级",
        //        levelData.polishLayerSortingOrder
        //    );

        levelData.polishCompletePercent =
            EditorGUILayout.Slider(
                "擦亮完成比例",
                levelData.polishCompletePercent,
                0f,
                1f
            );

        levelData.towelSize =
            EditorGUILayout.FloatField(
                "毛巾大小",
                levelData.towelSize
            );

        if (levelData.brushSize < 0.001f)
        {
            levelData.brushSize = 0.001f;
        }

        if (levelData.towelSize < 0.001f)
        {
            levelData.towelSize = 0.001f;
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(
            "评分时间",
            EditorStyles.boldLabel
        );

        levelData.threeStarTime =
            EditorGUILayout.FloatField(
                "三星时间/秒",
                levelData.threeStarTime
            );

        levelData.twoStarTime =
            EditorGUILayout.FloatField(
                "二星时间/秒",
                levelData.twoStarTime
            );

        if (levelData.threeStarTime < 1f)
        {
            levelData.threeStarTime = 1f;
        }

        if (levelData.twoStarTime < levelData.threeStarTime)
        {
            levelData.twoStarTime = levelData.threeStarTime;
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制所有碎片配置。
    /// </summary>
    private void DrawPieceList()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            "碎片列表",
            EditorStyles.boldLabel
        );

        if (GUILayout.Button("添加碎片", GUILayout.Width(100)))
        {
            AddPiece();
        }

        EditorGUILayout.EndHorizontal();

        if (levelData.pieces == null)
        {
            levelData.pieces =
                new System.Collections.Generic.List<
                    FragmentAssemblyPieceData
                >();
        }

        for (int i = 0; i < levelData.pieces.Count; i++)
        {
            DrawPieceItem(i);
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制单个碎片的编辑内容。
    /// </summary>
    private void DrawPieceItem(int index)
    {
        FragmentAssemblyPieceData piece =
            levelData.pieces[index];

        if (piece == null)
        {
            return;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            "碎片 " + (index + 1),
            EditorStyles.boldLabel
        );

        if (GUILayout.Button("删除", GUILayout.Width(60)))
        {
            Undo.RecordObject(
                levelData,
                "Delete Assembly Fragment"
            );

            levelData.pieces.RemoveAt(index);
            EditorUtility.SetDirty(levelData);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        piece.id = EditorGUILayout.TextField(
            "碎片 ID",
            piece.id
        );

        piece.sprite = (Sprite)
            EditorGUILayout.ObjectField(
                "碎片图片",
                piece.sprite,
                typeof(Sprite),
                false
            );

        EditorGUILayout.LabelField(
            "初始状态",
            EditorStyles.boldLabel
        );

        piece.startPosition =
            EditorGUILayout.Vector3Field(
                "初始位置",
                piece.startPosition
            );

        piece.startRotation =
            EditorGUILayout.FloatField(
                "初始旋转",
                piece.startRotation
            );

        EditorGUILayout.LabelField(
            "目标状态",
            EditorStyles.boldLabel
        );

        piece.targetPosition =
            EditorGUILayout.Vector3Field(
                "目标位置",
                piece.targetPosition
            );

        piece.targetRotation =
            EditorGUILayout.FloatField(
                "目标旋转",
                piece.targetRotation
            );

        piece.sortingOrder =
            EditorGUILayout.IntField(
                "显示层级",
                piece.sortingOrder
            );

        piece.snapDistance =
            EditorGUILayout.FloatField(
                "位置吸附距离",
                piece.snapDistance
            );

        piece.rotationTolerance =
            EditorGUILayout.FloatField(
                "旋转容错角度",
                piece.rotationTolerance
            );

        piece.usePolygonCollider =
            EditorGUILayout.Toggle(
                "使用多边形碰撞器",
                piece.usePolygonCollider
            );

        if (piece.snapDistance < 0f)
        {
            piece.snapDistance = 0f;
        }

        if (piece.rotationTolerance < 0f)
        {
            piece.rotationTolerance = 0f;
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制预览和保存按钮。
    /// </summary>
    private void DrawActionButtons()
    {
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("生成运行时拼接预览"))
        {
            GeneratePreview();
        }

        if (GUILayout.Button("生成编辑器布局预览"))
        {
            FragmentAssemblyLayoutPreview.CreateOrRefresh(levelData);
        }

        if (GUILayout.Button("保存编辑器布局"))
        {
            FragmentAssemblyLayoutPreview.SaveToLevelData(levelData);
        }

        if (GUILayout.Button("清理编辑器布局预览"))
        {
            FragmentAssemblyLayoutPreview.Clear();
        }

        if (GUILayout.Button("保存关卡数据"))
        {
            SaveLevelData();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 添加一个默认碎片。
    /// </summary>
    private void AddPiece()
    {
        Undo.RecordObject(
            levelData,
            "Add Assembly Fragment"
        );

        FragmentAssemblyPieceData piece =
            new FragmentAssemblyPieceData();

        int number = levelData.pieces.Count + 1;

        piece.id = "fragment_" + number.ToString("00");
        piece.startPosition =
            new Vector3(number * 1.2f, -3f, 0f);
        piece.targetPosition = Vector3.zero;
        piece.startRotation = 0f;
        piece.targetRotation = 0f;
        piece.snapDistance = 0.35f;
        piece.rotationTolerance = 12f;
        piece.usePolygonCollider = true;

        levelData.pieces.Add(piece);
        EditorUtility.SetDirty(levelData);
    }

    /// <summary>
    /// 创建新的 ScriptableObject 关卡数据。
    /// </summary>
    private void CreateNewLevelData()
    {
        if (string.IsNullOrWhiteSpace(newLevelName))
        {
            Debug.LogWarning("关卡名称不能为空。");
            return;
        }

        if (string.IsNullOrWhiteSpace(saveFolder))
        {
            saveFolder = "Assets/AssemblyLevels";
        }

        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            Debug.LogError(
                "保存目录不存在，请先在 Unity 中创建该目录："
                + saveFolder
            );

            return;
        }

        FragmentAssemblyLevelData asset =
            CreateInstance<FragmentAssemblyLevelData>();

        asset.levelName = newLevelName;

        string path = saveFolder + "/" +
                      newLevelName + ".asset";

        path = AssetDatabase.GenerateUniqueAssetPath(path);

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        levelData = asset;
        Selection.activeObject = asset;

        Debug.Log("拼接关卡创建成功：" + path);
    }

    /// <summary>
    /// 在当前场景生成拼接预览。
    /// </summary>
    private void GeneratePreview()
    {
        if (levelData == null)
        {
            Debug.LogWarning("没有选择拼接关卡数据。");
            return;
        }

        GameObject builderObject =
            GameObject.Find("FragmentAssemblyLevelBuilder");

        if (builderObject == null)
        {
            builderObject =
                new GameObject(
                    "FragmentAssemblyLevelBuilder"
                );
        }

        FragmentAssemblyLevelBuilder builder =
            builderObject.GetComponent<
                FragmentAssemblyLevelBuilder
            >();

        if (builder == null)
        {
            builderObject.AddComponent<
                FragmentAssemblyLevelBuilder
            >();

            builder =
                builderObject.GetComponent<
                    FragmentAssemblyLevelBuilder
                >();
        }

        builder.SetLevelData(levelData);
        builder.BuildLevel();

        EditorUtility.SetDirty(builderObject);

        Debug.Log("拼接关卡预览已生成。");
    }

    /// <summary>
    /// 保存当前关卡数据。
    /// </summary>
    private void SaveLevelData()
    {
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "拼接关卡数据已保存：" + levelData.levelName
        );
    }
}

#endif
