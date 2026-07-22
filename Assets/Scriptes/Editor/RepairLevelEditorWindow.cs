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
        EditorGUILayout.Space(8);
    }

    private void DrawLevelAssetArea()
    {
        EditorGUILayout.BeginVertical("box");

        levelData = (RepairLevelData)EditorGUILayout.ObjectField(
            "当前关卡数据",
            levelData,
            typeof(RepairLevelData),
            false
        );

        newLevelName = EditorGUILayout.TextField("新关卡名称", newLevelName);
        saveFolder = EditorGUILayout.TextField("保存目录", saveFolder);

        if (GUILayout.Button("创建新关卡数据"))
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

        levelData.levelName = EditorGUILayout.TextField("关卡名称", levelData.levelName);
        levelData.backgroundSprite = (Sprite)EditorGUILayout.ObjectField(
            "背景图",
            levelData.backgroundSprite,
            typeof(Sprite),
            false
        );

        levelData.backgroundPosition = EditorGUILayout.Vector3Field(
            "背景位置",
            levelData.backgroundPosition
        );

        levelData.backgroundSortingOrder = EditorGUILayout.IntField(
            "背景层级",
            levelData.backgroundSortingOrder
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

        for (int i = 0; i < levelData.repairPoints.Count; i++)
        {
            DrawRepairPointItem(i);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRepairPointItem(int index)
    {
        RepairPointData point = levelData.repairPoints[index];

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

        point.id = EditorGUILayout.TextField("匹配 ID", point.id);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("图片资源", EditorStyles.boldLabel);

        point.graySlotSprite = (Sprite)EditorGUILayout.ObjectField(
            "灰色缺口图",
            point.graySlotSprite,
            typeof(Sprite),
            false
        );

        point.repairedSprite = (Sprite)EditorGUILayout.ObjectField(
            "修复后图",
            point.repairedSprite,
            typeof(Sprite),
            false
        );

        point.dragPieceSprite = (Sprite)EditorGUILayout.ObjectField(
            "拖拽图",
            point.dragPieceSprite,
            typeof(Sprite),
            false
        );

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("位置", EditorStyles.boldLabel);

        point.slotPosition = EditorGUILayout.Vector3Field(
            "缺口位置",
            point.slotPosition
        );

        point.dragStartPosition = EditorGUILayout.Vector3Field(
            "拖拽初始位置",
            point.dragStartPosition
        );

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("层级", EditorStyles.boldLabel);

        point.slotSortingOrder = EditorGUILayout.IntField(
            "灰色缺口层级",
            point.slotSortingOrder
        );

        point.repairedSortingOrder = EditorGUILayout.IntField(
            "修复图层级",
            point.repairedSortingOrder
        );

        point.dragSortingOrder = EditorGUILayout.IntField(
            "拖拽图层级",
            point.dragSortingOrder
        );

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("吸附与碰撞", EditorStyles.boldLabel);

        point.snapDistance = EditorGUILayout.FloatField(
            "吸附距离",
            point.snapDistance
        );

        point.addPolygonCollider  = EditorGUILayout.Toggle(
            "自动添加 BoxCollider2D",
            point.addPolygonCollider 
        );

        EditorGUILayout.EndVertical();
    }

    private void DrawBottomButtons()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("一键生成到当前场景", GUILayout.Height(32)))
        {
            GenerateLevelInScene();
        }

        if (GUILayout.Button("保存关卡数据"))
        {
            SaveLevelData();
        }

        EditorGUILayout.EndVertical();
    }

    private void AddRepairPoint()
    {
        if (levelData.repairPoints == null)
        {
            levelData.repairPoints = new System.Collections.Generic.List<RepairPointData>();
        }

        int index = levelData.repairPoints.Count + 1;

        RepairPointData point = new RepairPointData();
        point.id = "piece_" + index.ToString("00");
        point.slotPosition = Vector3.zero;
        point.dragStartPosition = new Vector3(index * 1.2f, -3f, 0f);

        levelData.repairPoints.Add(point);

        EditorUtility.SetDirty(levelData);
    }

    private void CreateNewLevelAsset()
    {
        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            AssetDatabase.Refresh();
        }

        RepairLevelData asset = CreateInstance<RepairLevelData>();
        asset.levelName = newLevelName;

        string path = Path.Combine(saveFolder, newLevelName + ".asset");
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
        Debug.Log("关卡已生成到当前场景：" + levelData.levelName);
    }

    private void SaveLevelData()
    {
        if (levelData == null) return;

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

        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
#endif
