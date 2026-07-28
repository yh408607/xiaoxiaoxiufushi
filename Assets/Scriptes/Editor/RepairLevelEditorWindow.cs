#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class RepairLevelEditorWindow : EditorWindow
{
    private RepairLevelData levelData;
    private Vector2 scroll;

    private string newLevelName = "NewRepairLevel";
    private string saveFolder = "Assets/RepairLevels";

    [MenuItem("Tools/Repair Game/Level Editor")]
    public static void OpenWindow()
    {
        RepairLevelEditorWindow window = GetWindow<RepairLevelEditorWindow>();
        window.titleContent = new GUIContent("Repair Level Editor");
        window.Show();
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawLevelAssetArea();

        if (levelData == null)
        {
            EditorGUILayout.HelpBox("请先创建或选择一个 RepairLevelData。", MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawBaseInfo();
        DrawRepairPoints();

        EditorGUILayout.EndScrollView();

        DrawBottomButtons();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelData);
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("修复缺口关卡编辑器", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "玩法逻辑：完整底图一直显示，灰色遮挡图盖住部分区域。拖拽图片修复成功后，拖拽图片和灰色遮挡图隐藏，露出完整底图。",
            MessageType.Info
        );
        EditorGUILayout.Space(4);
    }

    private void DrawLevelAssetArea()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("关卡资源", EditorStyles.boldLabel);

        levelData = (RepairLevelData)EditorGUILayout.ObjectField(
            "当前关卡数据",
            levelData,
            typeof(RepairLevelData),
            false
        );

        EditorGUILayout.Space(4);

        newLevelName = EditorGUILayout.TextField("新关卡名称", newLevelName);
        saveFolder = EditorGUILayout.TextField("保存目录", saveFolder);

        if (GUILayout.Button("创建新关卡数据", GUILayout.Height(26)))
        {
            CreateNewLevelAsset();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBaseInfo()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);

        levelData.levelName = EditorGUILayout.TextField(
            "关卡名称",
            levelData.levelName
        );

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("底图公共变换", EditorStyles.boldLabel);

        levelData.backgroundPosition = EditorGUILayout.Vector3Field(
            "公共位置",
            levelData.backgroundPosition
        );

        levelData.backgroundScale = EditorGUILayout.Vector3Field(
            "公共缩放",
            levelData.backgroundScale
        );

        EditorGUILayout.HelpBox(
            "修复阶段底图、干净底图、灰尘图层都会使用这个公共位置和缩放，确保三者完全重合。",
            MessageType.Info
        );

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("修复阶段底图", EditorStyles.boldLabel);

        levelData.repairBackgroundSprite = (Sprite)EditorGUILayout.ObjectField(
            "修复阶段底图",
            levelData.repairBackgroundSprite,
            typeof(Sprite),
            false
        );

        levelData.repairBackgroundSortingOrder = EditorGUILayout.IntField(
            "修复底图层级",
            levelData.repairBackgroundSortingOrder
        );

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("干净底图", EditorStyles.boldLabel);

        levelData.cleanBackgroundSprite = (Sprite)EditorGUILayout.ObjectField(
            "干净底图",
            levelData.cleanBackgroundSprite,
            typeof(Sprite),
            false
        );

        levelData.cleanBackgroundSortingOrder = EditorGUILayout.IntField(
            "干净底图层级",
            levelData.cleanBackgroundSortingOrder
        );

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("灰尘擦拭", EditorStyles.boldLabel);

        levelData.dustSprite = (Sprite)EditorGUILayout.ObjectField(
            "灰尘图层",
            levelData.dustSprite,
            typeof(Sprite),
            false
        );

        levelData.dustSortingOrder = EditorGUILayout.IntField(
            "灰尘层级",
            levelData.dustSortingOrder
        );

        levelData.wipeCompletePercent = EditorGUILayout.Slider(
            "完成擦除比例",
            levelData.wipeCompletePercent,
            0f,
            1f
        );

        levelData.wipeBrushSize = EditorGUILayout.FloatField(
            "擦拭笔刷大小",
            levelData.wipeBrushSize
        );

        if (levelData.wipeBrushSize < 0.001f)
        {
            levelData.wipeBrushSize = 0.001f;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("评分时间", EditorStyles.boldLabel);

        levelData.threeStarTime = EditorGUILayout.FloatField(
            "3星时间/秒",
            levelData.threeStarTime
        );

        levelData.twoStarTime = EditorGUILayout.FloatField(
            "2星时间/秒",
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

        EditorGUILayout.HelpBox(
            "用时 <= 3星时间：3星；用时 <= 2星时间：2星；超过2星时间：1星。",
            MessageType.Info
        );


        EditorGUILayout.EndVertical();
    }

    private void DrawRepairPoints()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("修复点列表", EditorStyles.boldLabel);

        if (GUILayout.Button("添加修复点", GUILayout.Width(120)))
        {
            AddRepairPoint();
        }

        EditorGUILayout.EndHorizontal();

        if (levelData.repairPoints == null)
        {
            EditorGUILayout.HelpBox("修复点列表为空。", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        if (levelData.repairPoints.Count == 0)
        {
            EditorGUILayout.HelpBox("当前没有修复点，请点击“添加修复点”。", MessageType.Info);
        }

        for (int i = 0; i < levelData.repairPoints.Count; i++)
        {
            DrawRepairPointItem(i);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRepairPointItem(int index)
    {
        RepairPointData point = levelData.repairPoints[index];

        if (point == null)
        {
            EditorGUILayout.HelpBox($"修复点 {index + 1} 数据为空。", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"修复点 {index + 1}: {point.id}", EditorStyles.boldLabel);

        if (GUILayout.Button("删除", GUILayout.Width(60)))
        {
            levelData.repairPoints.RemoveAt(index);
            EditorUtility.SetDirty(levelData);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        point.id = EditorGUILayout.TextField("匹配 ID", point.id);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("图片资源", EditorStyles.boldLabel);

        point.graySlotSprite = (Sprite)EditorGUILayout.ObjectField(
            "灰色遮挡图",
            point.graySlotSprite,
            typeof(Sprite),
            false
        );

        point.dragPieceSprite = (Sprite)EditorGUILayout.ObjectField(
            "拖拽图片",
            point.dragPieceSprite,
            typeof(Sprite),
            false
        );

        EditorGUILayout.HelpBox(
            "这里不需要修复后图片。修复成功后会隐藏灰色遮挡图和拖拽图片，露出完整底图。",
            MessageType.None
        );

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("位置", EditorStyles.boldLabel);

        point.slotPosition = EditorGUILayout.Vector3Field(
            "灰色遮挡位置",
            point.slotPosition
        );

        point.dragStartPosition = EditorGUILayout.Vector3Field(
            "拖拽初始位置",
            point.dragStartPosition
        );

        EditorGUILayout.HelpBox(
            "建议点击“生成预览到当前场景”后，在 Scene 视图中对齐灰色遮挡图，再点击“从场景回写位置”。",
            MessageType.None
        );

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("显示层级", EditorStyles.boldLabel);

        point.slotSortingOrder = EditorGUILayout.IntField(
            "灰色遮挡层级",
            point.slotSortingOrder
        );

        point.dragSortingOrder = EditorGUILayout.IntField(
            "拖拽图片层级",
            point.dragSortingOrder
        );

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("吸附", EditorStyles.boldLabel);

        point.snapDistance = EditorGUILayout.FloatField(
            "吸附距离",
            point.snapDistance
        );

        if (point.snapDistance < 0f)
        {
            point.snapDistance = 0f;
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("碰撞器", EditorStyles.boldLabel);

        point.colliderType = (RepairColliderType)EditorGUILayout.EnumPopup(
            "碰撞器类型",
            point.colliderType
        );

        EditorGUILayout.HelpBox(
            "拖拽图片需要 Collider2D 才能响应 OnMouseDown / OnMouseDrag。普通矩形用 Box，不规则图片用 Polygon。",
            MessageType.None
        );

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("拖拽高亮", EditorStyles.boldLabel);

        point.enableOutline = EditorGUILayout.Toggle(
            "启用描边",
            point.enableOutline
        );

        if (point.enableOutline)
        {
            point.outlineColor = EditorGUILayout.ColorField(
                "描边颜色",
                point.outlineColor
            );

            point.outlineSize = EditorGUILayout.FloatField(
                "描边粗细",
                point.outlineSize
            );

            if (point.outlineSize < 0f)
            {
                point.outlineSize = 0f;
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBottomButtons()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("场景预览与保存", EditorStyles.boldLabel);

        if (GUILayout.Button("生成预览到当前场景", GUILayout.Height(32)))
        {
            GenerateLevelInScene();
        }

        if (GUILayout.Button("从场景回写位置", GUILayout.Height(28)))
        {
            SavePositionsFromScene();
        }

        if (GUILayout.Button("保存关卡数据", GUILayout.Height(28)))
        {
            SaveLevelData();
        }

        EditorGUILayout.HelpBox(
            "推荐流程：先生成预览到当前场景，然后在 Scene 视图中拖动 Slot_xxx 和 DragPiece_xxx，最后点击从场景回写位置。",
            MessageType.Info
        );

        EditorGUILayout.EndVertical();
    }

    private void AddRepairPoint()
    {
        if (levelData.repairPoints == null)
        {
            levelData.repairPoints = new System.Collections.Generic.List<RepairPointData>();
        }

        int number = levelData.repairPoints.Count + 1;

        RepairPointData point = new RepairPointData();
        point.id = "piece_" + number.ToString("00");

        point.slotPosition = Vector3.zero;
        point.dragStartPosition = new Vector3(number * 1.2f, -3f, 0f);

        point.slotSortingOrder = 0;
        point.dragSortingOrder = 5;

        point.snapDistance = 0.5f;
        point.colliderType = RepairColliderType.Polygon;

        point.enableOutline = true;
        point.outlineColor = Color.yellow;
        point.outlineSize = 0.05f;

        levelData.repairPoints.Add(point);

        EditorUtility.SetDirty(levelData);
    }

    private void CreateNewLevelAsset()
    {
        if (string.IsNullOrEmpty(newLevelName))
        {
            Debug.LogWarning("新关卡名称不能为空");
            return;
        }

        if (string.IsNullOrEmpty(saveFolder))
        {
            saveFolder = "Assets/RepairLevels";
        }

        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            AssetDatabase.Refresh();
        }

        RepairLevelData asset = CreateInstance<RepairLevelData>();
        asset.levelName = newLevelName;

        string path = Path.Combine(saveFolder, newLevelName + ".asset");
        path = path.Replace("\\", "/");
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        levelData = asset;
        Selection.activeObject = asset;

        Debug.Log("创建关卡数据成功：" + path);
    }

    private void GenerateLevelInScene()
    {
        if (levelData == null)
        {
            Debug.LogWarning("没有选择关卡数据");
            return;
        }

        GameObject builderObj = GameObject.Find("RepairLevelBuilder");

        if (builderObj == null)
        {
            builderObj = new GameObject("RepairLevelBuilder");
        }

        RepairLevelBuilder builder = builderObj.GetComponent<RepairLevelBuilder>();

        if (builder == null)
        {
            builder = builderObj.AddComponent<RepairLevelBuilder>();
        }

        SetPrivateField(
            typeof(RepairLevelBuilder),
            builder,
            "levelData",
            levelData
        );

        SetPrivateField(
            typeof(RepairLevelBuilder),
            builder,
            "buildOnStart",
            false
        );

        builder.BuildLevel();

        EditorUtility.SetDirty(builderObj);

        Debug.Log("关卡预览已生成到当前场景：" + levelData.levelName);
    }

    private void SavePositionsFromScene()
    {
        if (levelData == null)
        {
            Debug.LogWarning("没有选择关卡数据");
            return;
        }

        if (levelData.repairPoints == null)
        {
            Debug.LogWarning("修复点列表为空");
            return;
        }

        int saveCount = 0;

        for (int i = 0; i < levelData.repairPoints.Count; i++)
        {
            RepairPointData point = levelData.repairPoints[i];

            if (point == null) continue;

            GameObject slotObj = GameObject.Find("Slot_" + point.id);

            if (slotObj != null)
            {
                point.slotPosition = slotObj.transform.position;
                saveCount++;
            }

            GameObject dragObj = GameObject.Find("DragPiece_" + point.id);

            if (dragObj != null)
            {
                point.dragStartPosition = dragObj.transform.position;
                saveCount++;
            }
        }

        GameObject dustObj = GameObject.Find("DustLayer");

        if (dustObj != null)
        {
            levelData.backgroundPosition = dustObj.transform.position;
        }

        GameObject repairBgObj = GameObject.Find("RepairBackground");

        if (repairBgObj != null)
        {
            levelData.backgroundPosition = repairBgObj.transform.position;
            levelData.backgroundScale = repairBgObj.transform.localScale;
        }
        else
        {
            GameObject cleanBgObj = GameObject.Find("CleanBackground");

            if (cleanBgObj != null)
            {
                levelData.backgroundPosition = cleanBgObj.transform.position;
                levelData.backgroundScale = cleanBgObj.transform.localScale;
            }
        }

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("已从场景回写位置，回写数量：" + saveCount);
    }

    private void SaveLevelData()
    {
        if (levelData == null)
        {
            Debug.LogWarning("没有选择关卡数据");
            return;
        }

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("关卡数据已保存：" + levelData.levelName);
    }

    private void SetPrivateField(System.Type type, object target, string fieldName, object value)
    {
        var field = type.GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field == null)
        {
            Debug.LogWarning($"字段不存在：{type.Name}.{fieldName}");
            return;
        }

        field.SetValue(target, value);
    }
}
#endif
